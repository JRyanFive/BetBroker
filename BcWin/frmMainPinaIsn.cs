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
using DevExpress.XtraBars.Docking;
using DevExpress.XtraGrid.Views.Grid;
using log4net;

namespace BcWin
{
    public partial class frmMainPinaIsn : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmMainSboPina));

        List<AccountSummary> AccountSummaries = new List<AccountSummary>();

        // new version
        private int betPinaSuccess = 0;
        private int betPinaFail = 0;
        private int betPinaMiss = 0;
        private int betPinaAgainst = 0;

        private int betIsnSuccess = 0;
        private int betIsnFail = 0;
        private int betIsnMiss = 0;
        private int betIsnAgainst = 0;
        
        /*
        //old version
        private int betIbetSuccess = 0;
        private int betSboSuccess = 0;
        private int betSboFail = 0;
        private int betSboMiss = 0;
        private int betIbetAgainst = 0;
        */


        private bool isDispose;
        private List<LogScanMessage> scanMsgSource = new List<LogScanMessage>();
        private List<LogBetMessage> betMsgSource = new List<LogBetMessage>();
        private string folderName = "";
        private Dictionary<int, string> mapTabs = new Dictionary<int, string>();
        private System.Threading.Timer objCheckScanDriver;

        public string FileName = "";
        public ProcessorConfigs ProcessorConfigs { get; set; }
        private ServerCallback _serverCallback { get; set; }
        public List<PinaIsnManagement> IbetSboManagements = new List<PinaIsnManagement>();

        private IBcManageService _manageService;

        public frmMainPinaIsn()
        {
            if (DateTime.Now > new DateTime(2015, 11, 25))
            {
                MessageBox.Show("EX12301: Lỗi parse data!");
                return;
            }

            InitializeComponent();
            this.Text = "BcWin - " + SelfInfo.Username;

            Init();

            for (int i = 0; i < 4; i++)
            {
                AddNewOnePairAccountTab(i);
            }

            dgvAccountSummary.DataSource = AccountSummaries;
        }

        public frmMainPinaIsn(IBcManageService service, ScanInfoDTO scanConfig)
        {
            if (DateTime.Now > new DateTime(2015, 10, 25))
            {
                MessageBox.Show("EX12301: Lỗi parse data!");
                return;
            }

            InitializeComponent();

            this.Text = "BcWin - " + SelfInfo.Username;
            _manageService = service;

            Init();

            for (int i = 0; i < 1; i++)
            {
                AddNewOnePairAccountTab(i);
            }

            dgvAccountSummary.DataSource = AccountSummaries;

            StartLocalScan(scanConfig);
            //ConnectServerScan();
            //objCheckScanDriver = new System.Threading.Timer(WaitCheckDriverCallbackScan, null, 0, 25000);
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
            try
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

                //timerPing.Stop();
                //timerPing.Dispose();
                GC.SuppressFinalize(this);
                Application.Exit();
            }
            catch (Exception ex)
            {
                Logger.Error("Dispose " + ex.Message);
            }

        }

        private System.Timers.Timer timerPing;
        private void Init()
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            //init docking
            dockManager1.ForceInitialize();
            dockManager1.DockingOptions.HideImmediatelyOnAutoHide = true;

            ProcessorConfigs = new ProcessorConfigs();

            folderName = Path.Combine(Application.StartupPath, "AccountData");
            FileName = Path.Combine(folderName, "PinaIsnAccounts.xml");

            InitProcessConfig();

            Thread scanLogThread = new Thread(DoShowScanLog);
            scanLogThread.Start();

            Thread betLogThread = new Thread(DoShowBetLog);
            betLogThread.Start();

            //     _manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, 10);

            //timerPing = new System.Timers.Timer(60000);
            //timerPing.Elapsed += PingManage;
            //timerPing.Start();
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
            }
            try
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

        private void AddNewOnePairAccountTab(int code)
        {
            PinaIsnManagement ibetSbo = new PinaIsnManagement(code);
            ibetSbo.Dock = DockStyle.Fill;
            ibetSbo.MainForm = this;

            ibetSbo.BindControlData();
            ibetSbo.OnAccountSatus += OnAccountSatus;

            TabPage myTabPage = new TabPage();
            myTabPage.Controls.Add(ibetSbo);
            myTabPage.Name = ibetSbo.ID.ToString();
            myTabPage.Text = "Pi-Isn";
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
            try
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
            catch (Exception ex)
            {
                Logger.Error("DoShowScanLog " + ex.Message);
            }

        }


        /// <summary>
        /// old version
        /// </summary>
        //public void DoShowBetLog()
        //{
        //    try
        //    {
        //        while (true)
        //        {
        //            DataContainer.LogBetResetEvent.WaitOne();
        //            if (isDispose)
        //            {
        //                break;
        //            }

        //            while (DataContainer.LogBetQueue.Count > 0)
        //            {
        //                LogBetMessage logMsg;
        //                bool success = DataContainer.LogBetQueue.TryDequeue(out logMsg);
        //                if (success)
        //                {
        //                    if (logMsg.ServerType == eServerType.Isn)
        //                    {
        //                        switch (logMsg.Status)
        //                        {
        //                            case eBetStatusType.Success:
        //                                betSboSuccess++;
        //                                break;
        //                            case eBetStatusType.Fail:
        //                                betSboFail++;
        //                                break;
        //                            case eBetStatusType.MissOddSbo:
        //                                betSboSuccess++;
        //                                betSboMiss++;
        //                                break;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        switch (logMsg.Status)
        //                        {
        //                            case eBetStatusType.Success:
        //                                betIbetSuccess++;
        //                                break;
        //                            case eBetStatusType.BetAgainstIbet:
        //                                betIbetAgainst++;
        //                                break;
        //                        }
        //                    }

        //                    betMsgSource.Insert(0, logMsg);

        //                    var acSum = AccountSummaries.FirstOrDefault(a => a.TabCode == logMsg.TabCode);

        //                    if (acSum != null)
        //                    {
        //                        if (logMsg.ServerType == eServerType.Isn)
        //                        {
        //                            switch (logMsg.Status)
        //                            {
        //                                case eBetStatusType.Success:
        //                                    DataContainer.SuccessSound.PlaySync();
        //                                    acSum.SboSuccess++;
        //                                    break;
        //                                case eBetStatusType.Fail:
        //                                    DataContainer.FailSound.PlaySync();
        //                                    acSum.SboFail++;
        //                                    break;
        //                                case eBetStatusType.MissOddSbo:
        //                                    acSum.SboSuccess++;
        //                                    acSum.SboMiss++;
        //                                    break;
        //                            }
        //                        }
        //                        else
        //                        {
        //                            switch (logMsg.Status)
        //                            {
        //                                case eBetStatusType.Success:
        //                                    DataContainer.SuccessSound.PlaySync();
        //                                    acSum.IbetSuccess++;
        //                                    break;
        //                                case eBetStatusType.BetAgainstIbet:
        //                                    DataContainer.SuccessSound.PlaySync();
        //                                    acSum.IbetRebet++;
        //                                    break;
        //                            }
        //                        }

        //                        this.Invoke((MethodInvoker)(() =>
        //                        {
        //                            dgvAccountSummary.DataSource = AccountSummaries;
        //                            dgvAccountSummary.Refresh();
        //                        }));

        //                    }

        //                    this.Invoke((MethodInvoker)(() =>
        //                    {
        //                        devGridBetLog.DataSource = betMsgSource;
        //                        devGridBetLog.RefreshDataSource();
        //                        lblIbetBetSuccess.Text = betIbetSuccess.ToString();
        //                        lblBetAgainstIbet.Text = betIbetAgainst.ToString();
        //                        lblSboBetSuccess.Text = betSboSuccess.ToString();
        //                        lblBetSboFail.Text = betSboFail.ToString();
        //                        lblBetMissSbo.Text = betSboMiss.ToString();
        //                    }));
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error("DoShowBetLog: " + ex.Message);
        //    }

        //}



       
        /// <summary>
        /// new version 
        /// </summary>
        public void DoShowBetLog()
        {
            try
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
                        if (success)
                        {
                            if (logMsg.ServerType == eServerType.Isn)
                            {
                                switch (logMsg.Status)
                                {
                                    case eBetStatusType.Success:
                                        betIsnSuccess++;
                                        break;
                                    case eBetStatusType.Fail:
                                        betIsnFail++;
                                        break;
                                    case eBetStatusType.MissOddIsn:
                                        betIsnSuccess++;
                                        betIsnMiss++;
                                        break;
                                    case eBetStatusType.BetAgainIsn:
                                        betIsnAgainst++;
                                        break;
                                }
                            }
                            else if (logMsg.ServerType == eServerType.Pina)
                            {
                                switch (logMsg.Status)
                                {
                                    case eBetStatusType.Success:
                                        betPinaSuccess++;
                                        break;
                                    case eBetStatusType.Fail:
                                        betPinaFail++;
                                        break;
                                    case eBetStatusType.MissOddPina:
                                        betPinaSuccess++;
                                        betPinaMiss++;
                                        break;
                                    case eBetStatusType.BetAgainPina:
                                        betPinaAgainst++;
                                        break;

                                }
                            }
                           
                            betMsgSource.Insert(0, logMsg);

                            var acSum = AccountSummaries.FirstOrDefault(a => a.TabCode == logMsg.TabCode);

                            if (acSum != null)
                            {
                                if (logMsg.ServerType == eServerType.Isn)
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
                                        case eBetStatusType.MissOddIsn:
                                            acSum.SboSuccess++;
                                            acSum.SboMiss++;
                                            break;
                                        case eBetStatusType.BetAgainIsn:
                                            DataContainer.SuccessSound.PlaySync();
                                            acSum.SboRebet++;
                                            break;
                                    }
                                }
                                else if (logMsg.ServerType == eServerType.Pina)
                                {
                                    switch (logMsg.Status)
                                    {
                                        case eBetStatusType.Success:
                                            DataContainer.SuccessSound.PlaySync();
                                            acSum.IbetSuccess++;
                                            break;
                                        case eBetStatusType.Fail:
                                            DataContainer.FailSound.PlaySync();
                                            acSum.IbetFail++;
                                            break;
                                        case eBetStatusType.MissOddPina:
                                            acSum.IbetSuccess++;
                                            acSum.IbetMiss++;
                                            break;
                                        case eBetStatusType.BetAgainPina:
                                            DataContainer.SuccessSound.PlaySync();
                                            acSum.IbetRebet++;
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
                                lblIbetBetSuccess.Text = betPinaSuccess.ToString();
                                lblBetAgainstIbet.Text = betPinaAgainst.ToString();
                                lblSboBetSuccess.Text = betIsnSuccess.ToString();
                                lblBetSboFail.Text = betIsnFail.ToString();
                                lblBetMissSbo.Text = betIsnMiss.ToString();
                            }));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("DoShowBetLog: " + ex.Message);
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

        private void StartLocalScan(ScanInfoDTO scanConfig)
        {
            try
            {
                SaveLocalScanInfo(scanConfig);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi động scan!" + Environment.NewLine + ex);
            }
        }

        private void SaveLocalScanInfo(ScanInfoDTO scanInfo)
        {
            DataContainer.SboScanServers = scanInfo.ScanServers;

            this.Invoke((MethodInvoker)(() =>
            {
                RealConfig.PickSboHdpBot = ckPickSboHdpBot.Checked;
                RealConfig.PickSboHdpTop = ckPickSboHdpTop.Checked;
                RealConfig.PickSboOver = ckPickSboOver.Checked;
                RealConfig.PickSboUnder = ckPickSboUnder.Checked;
            }));

            DataContainer.SboScanAccounts = scanInfo.Accounts;
        }

        private void btnSaveConfigGenaral_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                RealConfig.PickSboHdpBot = ckPickSboHdpBot.Checked;
                RealConfig.PickSboHdpTop = ckPickSboHdpTop.Checked;
                RealConfig.PickSboOver = ckPickSboOver.Checked;
                RealConfig.PickSboUnder = ckPickSboUnder.Checked;
            }));
        }

        private void dgvAccountSummary_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }
            if (e.ColumnIndex == 0)
            {
                DockPanel p1 = dockManager1.AddPanel(DockingStyle.Left);
                p1.Text = "Pina " + e.RowIndex;
                p1.Visibility = DockVisibility.AutoHide;
                //dockPanel1.ControlContainer.Controls.Add(new UserControl1());

                DockPanel p2 = dockManager1.AddPanel(DockingStyle.Right);
                p2.Text = "ISN " + e.RowIndex;
                p2.Visibility = DockVisibility.AutoHide;
            }
        }

        private void pbPause_Click(object sender, EventArgs e)
        {

        }
    }
}
