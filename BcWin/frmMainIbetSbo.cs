using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Processor;
using BcWin.UserControls;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Timers;
using System.Xml.Linq;
using BcWin.Contract;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Core.XML;
using BcWin.Processor.ScanDriver;
using BcWin.Processor.Service;
using BCWin.Metadata;
using DevExpress.XtraGrid.Views.Grid;
using log4net;

namespace BcWin
{
    public partial class frmMainIbetSbo : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmMainIbetSbo));

        List<AccountSummary> AccountSummaries = new List<AccountSummary>();

        private int betIbetSuccess = 0;
        private int betSboSuccess = 0;
        private int betSboFail = 0;
        private int betSboMiss = 0;
        private int betIbetAgainst = 0;

        private bool isDispose;
        private List<LogScanMessage> scanMsgSource = new List<LogScanMessage>();
        private List<LogBetMessage> betMsgSource = new List<LogBetMessage>();
        private string folderName = "";
        private Dictionary<int, string> mapTabs = new Dictionary<int, string>();
        private System.Threading.Timer objCheckScanDriver;

        //private string IpS;
        //private string IpScan;

        public string FileName = "";
        public ProcessorConfigs ProcessorConfigs { get; set; }
        private ServerCallback _serverCallback { get; set; }
        public List<IbetSboManagement> IbetSboManagements = new List<IbetSboManagement>();
        private IbetSboDriver _driver;
        private IBcManageService _manageService;

        public frmMainIbetSbo(IBcManageService service)
        {
            InitializeComponent();

            this.Text = "BcWin - " + SelfInfo.Username;
            _manageService = service;

            Init();

            _serverCallback = new ServerCallback();
            _driver = new IbetSboDriver();

            ConnectServerScan();
            objCheckScanDriver = new System.Threading.Timer(WaitCheckDriverCallbackScan, null, 0, 25000);

            for (int i = 0; i < ClientData.Tab; i++)
            {
                AddNewOnePairAccountTab(i, _driver);
            }

            dgvAccountSummary.DataSource = AccountSummaries;

            //System.Timers.Timer timer = new System.Timers.Timer(60000 * 45);
            //timer.Elapsed += ResetProcessor;
            //timer.Start();

            //System.Timers.Timer dtimer = new System.Timers.Timer(60000 * 60);
            //dtimer.Elapsed += ResetDriver;
            //dtimer.Start();
        }

        //void ResetDriver(object sender, ElapsedEventArgs e)
        //{
        //    _driver.Stop();

        //    Thread thread = new Thread(() =>
        //    {
        //        _driver.Start(SystemConfig.IBET_LINK_1, SystemConfig.SBO_LINK_1);
        //    });
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //}

        void ResetProcessor(object sender, ElapsedEventArgs e)
        {
            foreach (var ibetSboManagement in IbetSboManagements)
            {
                if (ibetSboManagement.Processor.Status == eServiceStatus.Started)
                {
                    ibetSboManagement.StopProcessor();
                    ibetSboManagement.SetStartClickStyle();
                }
            }

            foreach (var ibetSboManagement in IbetSboManagements)
            {
                if (ibetSboManagement.Processor.Status != eServiceStatus.Started)
                {
                    Thread thread = new Thread(() =>
                    {
                        ibetSboManagement.Processor.ReStart();
                    });

                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
            }
        }

        private IBcWinService service;


        private bool ConnectServerScan()
        {
            try
            {
                InstanceContext serverCallback = new InstanceContext(_serverCallback);

                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;


                //Public
                EndpointAddress vEndPoint = new EndpointAddress(ClientData.EndpointRoute);
                DuplexChannelFactory<IBcWinService> cf = new DuplexChannelFactory<IBcWinService>
                    (serverCallback, b, vEndPoint);

                service = cf.CreateChannel();
                service.RegisterForClient(SelfInfo.Ip, SelfInfo.MacAddress, SelfInfo.Hostname);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool TryConnectServerScan()
        {
            if (isDispose)
            {
                return false;
            }

            if (!ConnectServerScan())
            {
                return TryConnectServerScan();
            }

            return false;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            isDispose = true;
            DataContainer.IsDispose = true;
            DataContainer.LogBetResetEvent.Set();
            DataContainer.LogScanResetEvent.Set();
            DataContainer.TransactionEvent.Set();

            if (objCheckScanDriver != null)
            {
                objCheckScanDriver.Dispose();
            }

            timerPing.Stop();
            timerPing.Dispose();
            GC.SuppressFinalize(this);
            Application.Exit();
        }

        private System.Timers.Timer timerPing;
        private void Init()
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            string proxyIbetEp = Convert.ToString(ConfigurationManager.AppSettings["PROXYIBET"]);
            if (string.IsNullOrEmpty(proxyIbetEp))
            {
                ClientData.ProxyIbet = false;
            }
            else
            {
                ClientData.ProxyIbet = true;
                ClientData.ProxyIbetEndpoint = string.Concat("net.tcp://", proxyIbetEp, ":9998/bcwinsupservice");
            }

            ProcessorConfigs = new ProcessorConfigs();

            folderName = Path.Combine(Application.StartupPath, "AccountData");
            FileName = Path.Combine(folderName, "IbetSboAccounts.xml");

            InitProcessConfig();

            Thread scanLogThread = new Thread(DoShowScanLog);
            scanLogThread.Start();

            Thread betLogThread = new Thread(DoShowBetLog);
            betLogThread.Start();

            _manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, 10);

            timerPing = new System.Timers.Timer(60000 * 2);
            timerPing.Elapsed += PingManage;
            timerPing.Start();
        }

        private void InitProcessConfig()
        {
            if (!File.Exists(this.FileName))
            {
                if (!Directory.Exists(folderName))
                {
                    Directory.CreateDirectory(folderName);
                }
                File.Create(this.FileName);
            } try
            {
                XElement xmlObject = XElement.Load(this.FileName);
                string xmlString = xmlObject.ToString();
                this.ProcessorConfigs = XMLHelper.ConvertXmlStringtoObject<ProcessorConfigs>(xmlString);
                this.ProcessorConfigs.ListProcessorConfig = this.ProcessorConfigs.ListProcessorConfig.OrderBy(x => x.Code).ToList();

            }
            catch (Exception)
            {

            }
        }

        private void AddNewOnePairAccountTab(int code, IbetSboDriver d)
        {
            IbetSboManagement ibetSbo = new IbetSboManagement(code, d, _serverCallback);

            ibetSbo.Dock = DockStyle.Fill;
            ibetSbo.MainIbetSboForm = this;

            //ibetSbo.BackColor = Color.White;
            //Add Processor Config Data
            ibetSbo.BindControlData();
            ibetSbo.OnAccountSatus += OnAccountSatus;

            ibetSbo.SboEngine.OnFakeRequest += EngineOnFakeRequest;
            ibetSbo.SboEngine.OnFakeRequest += EngineOnFakeRequest;

            TabPage myTabPage = new TabPage();
            myTabPage.Controls.Add(ibetSbo);
            myTabPage.Name = ibetSbo.ID.ToString();
            //myTabPage.Name = "Ibet_Sbo";
            myTabPage.Text = "Ibet-Sbo";
            tabMain.TabPages.Add(myTabPage);
            mapTabs[code] = ibetSbo.ID.ToString();

            IbetSboManagements.Add(ibetSbo);

            AccountSummaries.Add(new AccountSummary()
            {
                TabCode = code,
                TabName = "Tab " + (code + 1),
                Status = Properties.Resources.StopStatus
            });
        }

        void EngineOnFakeRequest(string url)
        {
            webFakeRequest.Navigate(url);
        }

        private void OnAccountSatus(int tabCode, bool status)
        {
            var acSum = AccountSummaries.FirstOrDefault(a => a.TabCode == tabCode);

            if (acSum != null)
            {
                if (status)
                {
                    var process = IbetSboManagements.FirstOrDefault(p => p.Code == tabCode);

                    acSum.Status = process.Processor.Status == eServiceStatus.Started
                        ? Properties.Resources.StartStatus : Properties.Resources.StopStatus;
                }
                else
                {
                    acSum.Status = Properties.Resources.StopStatus;
                }

                this.Invoke((MethodInvoker)(() =>
                {
                    dgvAccountSummary.DataSource = AccountSummaries;
                    dgvAccountSummary.Refresh();
                }));
            }
        }

        public void DoShowScanLog()
        {
            while (true)
            {
                DataContainer.LogScanResetEvent.WaitOne();
                if (isDispose)
                {
                    break;
                }

                while (DataContainer.LogScanQueue.Count > 0)
                {
                    LogScanMessage logMsg;
                    bool success = DataContainer.LogScanQueue.TryDequeue(out logMsg);
                    //(myGridcontrol.MainView as DevExpress.XtraGrid.Views.Grid.GridView).AddNewRow();
                    if (success)
                    {
                        scanMsgSource.Insert(0, logMsg);
                        this.Invoke((MethodInvoker)(() =>
                        {
                            devGridScanLog.DataSource = scanMsgSource;
                            devGridScanLog.RefreshDataSource();
                        }));
                    }
                }
            }
        }

        public void DoShowBetLog()
        {
            while (true)
            {
                DataContainer.LogBetResetEvent.WaitOne();
                if (isDispose)
                {
                    break;
                }

                while (DataContainer.LogBetQueue.Count > 0)
                {
                    LogBetMessage logMsg;
                    bool success = DataContainer.LogBetQueue.TryDequeue(out logMsg);
                    //(myGridcontrol.MainView as DevExpress.XtraGrid.Views.Grid.GridView).AddNewRow();
                    if (success)
                    {
                        if (logMsg.ServerType == eServerType.Ibet)
                        {
                            switch (logMsg.Status)
                            {
                                case eBetStatusType.Success:
                                    betIbetSuccess++;
                                    break;
                                case eBetStatusType.BetAgainstIbet:
                                    betIbetAgainst++;
                                    break;
                            }
                        }
                        else
                        {
                            switch (logMsg.Status)
                            {
                                case eBetStatusType.Success:
                                    betSboSuccess++;
                                    break;
                                case eBetStatusType.Fail:
                                    betSboFail++;
                                    break;
                                case eBetStatusType.MissOddSbo:
                                    betSboSuccess++;
                                    betSboMiss++;
                                    break;
                            }
                        }

                        betMsgSource.Insert(0, logMsg);

                        var acSum = AccountSummaries.FirstOrDefault(a => a.TabCode == logMsg.TabCode);

                        if (acSum != null)
                        {
                            if (logMsg.ServerType == eServerType.Ibet)
                            {
                                switch (logMsg.Status)
                                {
                                    case eBetStatusType.Success:
                                        DataContainer.SuccessSound.PlaySync();
                                        acSum.IbetSuccess++;
                                        break;
                                    case eBetStatusType.BetAgainstIbet:
                                        DataContainer.SuccessSound.PlaySync();
                                        acSum.IbetRebet++;
                                        break;
                                }
                            }
                            else
                            {
                                switch (logMsg.Status)
                                {
                                    case eBetStatusType.Success:
                                        DataContainer.SuccessSound.PlaySync();
                                        acSum.SboSuccess++;
                                        break;
                                    case eBetStatusType.Fail:
                                        DataContainer.FailSound.PlaySync();
                                        acSum.SboFail++;
                                        break;
                                    case eBetStatusType.MissOddSbo:
                                        acSum.SboSuccess++;
                                        acSum.SboMiss++;
                                        break;
                                }
                            }

                            this.Invoke((MethodInvoker)(() =>
                            {
                                dgvAccountSummary.DataSource = AccountSummaries;
                                dgvAccountSummary.Refresh();
                            }));

                        }

                        this.Invoke((MethodInvoker)(() =>
                        {
                            devGridBetLog.DataSource = betMsgSource;
                            devGridBetLog.RefreshDataSource();
                            lblIbetBetSuccess.Text = betIbetSuccess.ToString();
                            lblBetAgainstIbet.Text = betIbetAgainst.ToString();
                            lblSboBetSuccess.Text = betSboSuccess.ToString();
                            lblBetSboFail.Text = betSboFail.ToString();
                            lblBetMissSbo.Text = betSboMiss.ToString();
                        }));
                    }
                }
            }
        }

        private void WaitCheckDriverCallbackScan(object obj)
        {
            try
            {
                service.Ping(SelfInfo.MacAddress);

                this.Invoke((MethodInvoker)(() =>
                {
                    lblPing.Text = DateTime.Now.ToShortTimeString();

                }));
            }
            catch (Exception ex)
            {
                objCheckScanDriver.Dispose();

                if (isDispose)
                {
                    return;
                }

                this.Invoke((MethodInvoker)(() =>
                {
                    lblPing.Text = "Fail!";

                }));

                TryConnectServerScan();

                objCheckScanDriver = new System.Threading.Timer(WaitCheckDriverCallbackScan, null, 0, 25000);
            }
        }

        private void pbPlay_Click(object sender, EventArgs e)
        {
            foreach (var ibetSboManagement in IbetSboManagements)
            {
                if (ibetSboManagement.Processor.Status != eServiceStatus.Started)
                {
                    ibetSboManagement.StartProcessor();
                }
            }
        }

        private void btnDeleteScanLog_Click(object sender, EventArgs e)
        {
            scanMsgSource = new List<LogScanMessage>();
            this.Invoke((MethodInvoker)(() =>
            {
                devGridScanLog.DataSource = scanMsgSource;
                devGridScanLog.RefreshDataSource();
            }));
        }

        private void dgvAccountSummary_SelectionChanged(object sender, EventArgs e)
        {
            dgvAccountSummary.ClearSelection();
        }

        private void WebFakeDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var w = (WebBrowser)sender;
            if (w.Url.AbsoluteUri != "about:blank"
                && webFakeRequest.ReadyState == WebBrowserReadyState.Complete)
            {
                webFakeRequest.Navigate("about:blank");
            }
        }

        private void pbStop_Click(object sender, EventArgs e)
        {
            foreach (var ibetSboManagement in IbetSboManagements)
            {
                if (ibetSboManagement.Processor.Status == eServiceStatus.Started)
                {
                    ibetSboManagement.StopProcessor();
                }
            }
        }

        public static int keyPing = 11;
        void PingManage(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, keyPing) != 1)
                {
                    timerPing.Stop();
                    foreach (var ibetSboManagement in IbetSboManagements)
                    {
                        if (ibetSboManagement.Processor.Status == eServiceStatus.Started)
                        {
                            ibetSboManagement.StopProcessor();
                        }

                        this.Invoke((MethodInvoker)(() =>
                        {
                            ibetSboManagement.Enabled = false;
                        }));
                    }

                    this.Invoke((MethodInvoker)(() =>
                    {
                        pbPlay.Enabled = false;
                        pbStop.Enabled = false;
                    }));

                    MessageBox.Show("Tài khoản này hiện đang login tại một nơi khác, vui lòng kiểm tra lại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    this.Dispose();
                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                //ConnectManage();
                //throw;
            }
        }
    }
}
