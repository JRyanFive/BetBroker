using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BCWin.Metadata;
using BcWin.Processor;
using BcWin.Processor.Interface;
using mshtml;
using BcWin.Core.XML;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Timers;
using BcWin.Common.EventDelegate;
using BcWin.Core;
using BcWin.Core.CssStyle;
using BcWin.Processor.ScanDriver;
using BcWin.Processor.Service;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;

namespace BcWin.UserControls
{
    public sealed partial class IbetSboManagement : UserControl
    {
        public AccountStatusEvent OnAccountSatus { get; set; }

        public string IbetLoginUrl { get; set; }
        public string SboLoginUrl { get; set; }

        Color GreyBackground = Color.Gray;
        Color OldLaceBackgroud = Color.OldLace;
        private bool isDispose;
        private AutoResetEvent loginCheckEvent;
        //private System.Threading.Timer objUpdateCreditTimer;

        public int Code { get; set; }
        public Guid ID { get; set; }

        public frmMainIbetSbo MainIbetSboForm { get; set; }

        // public Dictionary<Guid, IProcessor> ProcessorContainer = new Dictionary<Guid, IProcessor>();
        public IProcessor Processor { get; set; }
        public IbetEngine IbetEngine { get; set; }
        public SboEngine SboEngine { get; set; }
        private IbetSboDriver _driver;
        private bool isLoginFail = false;
        private System.Timers.Timer timerCheck;
        private string IpReal { get; set; }

        public IbetSboManagement(int code, IbetSboDriver driver, ServerCallback serverCallback)
        {
            if (ClientData.IpAddress != null && ClientData.IpAddress.Any())
            {
                Random r = new Random();
                IpReal = ClientData.IpAddress[r.Next(ClientData.IpAddress.Count)];
            }

            Code = code;
            //this.BackColor = Color.White;
            InitializeComponent();
            Init(driver, serverCallback);
            _driver = driver;
        }

        public void BindControlData()
        {
            var processor = this.MainIbetSboForm.ProcessorConfigs.ListProcessorConfig.Where(x => x.Code == this.Code).FirstOrDefault();
            if (processor != null)
            {
                cboIBetDomain.SelectedItem = processor.AccountFirst.DomainName;
                txtIbetAccountName.Text = processor.AccountFirst.Username;
                txtIbetPassword.Text = processor.AccountFirst.Password;
                txtIbetExchangeRate.Text = processor.AccountFirst.RateExchange.ToString();
                txtIbetMaxBet.Text = processor.AccountFirst.MaxStake.ToString();

                cboSboDomain.SelectedItem = processor.AccountSecond.DomainName;
                txtSboAccountName.Text = processor.AccountSecond.Username;
                txtSboPassword.Text = processor.AccountSecond.Password;
                txtSboExchangeRate.Text = processor.AccountSecond.RateExchange.ToString();
                txtSboMaxBet.Text = processor.AccountSecond.MaxStake.ToString();

                cboMarket.SelectedItem = processor.Market;

                switch (processor.BetStakeType)
                {
                    case eBetStakeType.Ibet:
                        rdoIbet.Checked = true;
                        break;
                    case eBetStakeType.Sbo:
                        rdoSbobet.Checked = true;
                        break;
                    case eBetStakeType.Max:
                        rdoMax.Checked = true;
                        break;
                    default:
                        rdoSbobet.Checked = true;
                        break;
                }
                txtBetStake.Text = processor.BetStake.ToString();
                numericTimeOffStakeOdds.Value = processor.TimeOffStakeOdds;
                cboCompareValue.Text = processor.CompareValue.ToString();
                numericMaxCountBet.Value = processor.MaxCountBet;
                numericRebetSbo.Value = processor.RebetSbo;
                numMinTimeToBet.Value = processor.MinTimeToBet;
                cboMinOddDef.Text = processor.MinOddDefBet.ToString();
                ckReBetIbet.Checked = processor.RebetIbet;
                numTimeWaitToRebetIbet.Value = processor.WaitingTimeRebetIbet;
            }
        }

        private void Init(IbetSboDriver driver, ServerCallback serverCallback)
        {
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            ID = Guid.NewGuid();
            SetDefaultStyle();
            Processor = new IbetSboProcessor(driver, serverCallback);
            //DataContainer.TransactionProcess.Add(Processor.TransactionQueue);
            IbetEngine = new IbetEngine();
            IbetEngine.RealIpAddress = IpReal;
            SboEngine = new SboEngine();
            SboEngine.RealIpAddress = IpReal;
            IbetEngine.TabCode = Code;
            SboEngine.TabCode = Code;

            cboIBetDomain.DataSource = DataContainer.IbetServers;
            cboSboDomain.DataSource = DataContainer.SboServers;
            dgvLeaguesSetting.DataSource = DataContainer.LeaguesSettings;
            cboMarket.SelectedIndex = 0;
            cboCompareValue.SelectedIndex = 0;
            cboMinOddDef.SelectedIndex = 1;

            BindConvertStakeInfo();

            lblSboStatus.TextChanged += OnStatusTextChanged;
            lblIbetStatus.TextChanged += OnStatusTextChanged;

            IbetEngine.OnLogOffEvent += OnLogOffEvent;
            SboEngine.OnLogOffEvent += OnLogOffEvent;

            Processor.OnProcessStateChange += ProcessorOnOnProcessStateChange;
            Processor.OnPingEvent += ProcessorOnOnPingEvent;
            webIbetStatement.DocumentCompleted += OnIbetStatementDocumentCompleted;
            webSboStatement.DocumentCompleted += OnSboStatementDocumentCompleted;

            timerCheck = new System.Timers.Timer(60000 * 2);
            timerCheck.Elapsed += CheckInfoStatus;
        }

        private void ProcessorOnOnPingEvent(DateTime time, eServerType serverType)
        {
            if (serverType == eServerType.Ibet)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    lblPingSbo.Text = "SKIPPED";
                    var ms = Math.Abs(Math.Round((DateTime.Now - time).TotalMilliseconds / 1000, 1));
                    lblPingIbet.Text = ms.ToString();
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    lblPingIbet.Text = "SKIPPED";
                    lblPingSbo.Text = (DateTime.Now - time).TotalMilliseconds.ToString();
                });
            }
        }

        private void ProcessorOnOnProcessStateChange(eServiceStatus status, string processState)
        {
            TabControl tabMain = MainIbetSboForm.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
            TabPage currentTab = tabMain.TabPages[ID.ToString()];
            this.Invoke((MethodInvoker)delegate ()
            {
                currentTab.Text = processState;
                if (processState != "reconnecting...")
                {
                    var sboProfile = SboEngine.GetAccountProfile();
                    lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                    lblSboRealDomain.Text = sboProfile.UrlHost;
                    lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                    lblSboCashBalance.Text = sboProfile.CashBalance.ToString();

                    var ibetProfile = IbetEngine.GetAccountProfile();
                    lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                    lblIbetRealDomain.Text = ibetProfile.UrlHost;
                    lblIbetCreadit.Text = ibetProfile.AvailabeCredit.ToString();
                    lblIbetCashBalance.Text = ibetProfile.CashBalance.ToString();
                }
            });
        }

        void OnLogOffEvent(string userName, eServerType serverType)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                switch (serverType)
                {
                    case eServerType.Ibet:
                        lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                        lblIbetRealDomain.Text = IbetEngine.UrlHost;
                        lblIbetCreadit.Text = string.Empty;
                        lblIbetCashBalance.Text = string.Empty;
                        //if (DataContainer.FirstAccounts.Contains(userName))
                        //{
                        //    DataContainer.FirstAccounts.Remove(userName);
                        //}
                        break;
                    case eServerType.Sbo:
                        lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                        lblSboRealDomain.Text = SboEngine.UrlHost;
                        lblSboCreadit.Text = string.Empty;
                        lblSboCashBalance.Text = string.Empty;
                        //if (DataContainer.SecondAccounts.Contains(userName))
                        //{
                        //    DataContainer.SecondAccounts.Remove(userName);
                        //}
                        break;
                }

                //SetDefaultStyle();
            });
        }

        void OnStatusTextChanged(object sender, EventArgs e)
        {
            var l = (Label)sender;
            if (l.Text == "Online")
            {
                l.ForeColor = Color.Blue;
                if (OnAccountSatus != null)
                {
                    OnAccountSatus(Code, true);
                }
            }
            else
            {
                l.ForeColor = Color.Red;
                if (OnAccountSatus != null)
                {
                    OnAccountSatus(Code, false);
                }
            }
            //throw new NotImplementedException();
        }

        protected override void Dispose(bool disposing)
        {
            isDispose = true;
            if (loginCheckEvent != null)
            {
                loginCheckEvent.Set();
            }
            //if (objUpdateCreditTimer != null)
            //{
            //    objUpdateCreditTimer.Dispose();
            //}

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void btnStartIbetSbo_Click(object sender, EventArgs e)
        {
            //SetStartClickStyle();
            StartProcessor();
        }

        private eScanType scanType;
        public void StartProcessor()
        {
            if (!rdoMax.Checked && string.IsNullOrEmpty(txtBetStake.Text))
            {
                return;
            }

            SetProcessingStyle();

            if (IbetEngine.AccountStatus == eAccountStatus.Offline)
            {
                LoginIbet();
            }

            if (SboEngine.AccountStatus == eAccountStatus.Offline)
            {
                LoginSbo();
            }

            TabControl tabMain = MainIbetSboForm.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
            TabPage currentTab = tabMain.TabPages[ID.ToString()];

            if (SboEngine.AccountStatus == eAccountStatus.Online
                        && IbetEngine.AccountStatus == eAccountStatus.Online)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    scanType = (eScanType)cboMarket.SelectedIndex;
                    Processor.IbetEngine = IbetEngine;
                    Processor.SboEngine = SboEngine;
                    Processor.ProcessorConfigInfo = ProcessBindData();

                    if (Processor.Start(scanType) == eServiceStatus.Started)
                    {
                        //currentTab.Text = Processor.ProcessorName;

                        var sboProfile = SboEngine.GetAccountProfile();
                        lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                        lblSboRealDomain.Text = sboProfile.UrlHost;
                        lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                        lblSboCashBalance.Text = sboProfile.CashBalance.ToString();

                        var ibetProfile = IbetEngine.GetAccountProfile();
                        lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                        lblIbetRealDomain.Text = ibetProfile.UrlHost;
                        lblIbetCreadit.Text = ibetProfile.AvailabeCredit.ToString();
                        lblIbetCashBalance.Text = ibetProfile.CashBalance.ToString();
                        StartCheckStatus();
                        SetStartClickStyle();

                        ////TODO:QUOCLE: REM TO TEST
                        if (_driver.Status != eServiceStatus.Started && DataContainer.HasLocalScan)
                        {
                            Thread thread = new Thread(StartDriver);
                            thread.SetApartmentState(ApartmentState.STA);
                            thread.Start();
                        }

                        if (OnAccountSatus != null)
                        {
                            OnAccountSatus(Code, true);
                        }
                    }
                    else
                    {
                        //Quang msgbox start ko thanh cong
                        if (Processor.StartFailCount >= 3)
                        {
                            currentTab.Text = "FAIL (Force STOP)";
                            StopProcessor();
                        }
                        else
                        {
                            currentTab.Text = "FAIL";
                            SetDefaultStyle();
                        }
                    }
                });

                //Processor.OnLogScanEvent += dashboard.OnLogScanEvent;
                //Processor.OnProcessExceptionEvent += dashboard.OnProcessorExceptionEvent;
                //Processor.OnUpdateCredit += dashboard.OnUpdateCredit;
            }
            else
            {
                SetDefaultStyle();
            }
        }

        public void StartCheckStatus()
        {
            timerCheck.Start();
            //objUpdateCreditTimer = new System.Threading.Timer(UpdateInfoCallback, null, 100000, 100000);
        }

        private void StartDriver()
        {
            _driver.Start(scanType);
            //IbetSboDriver.Start(IbetLoginUrl, SboLoginUrl);
        }

        public void PauseProcessor()
        {
            SetPauseClickStyle();
            //objUpdateCreditTimer.
            timerCheck.Stop();
            if (Processor != null && Processor.Status == eServiceStatus.Started)
            {
                Processor.Pause();
            }
        }

        public void StopProcessor()
        {
            SetDefaultStyle();
            timerCheck.Close();
            //if (Processor.Status == eServiceStatus.Started || Processor.Status == eServiceStatus.Initialized
            //    || Processor.Status == eServiceStatus.Paused)
            if (Processor.Status != eServiceStatus.Stopped)
            {
                Processor.Dispose();
            }
        }


        private int sboLoginFailCount = 0;
        private void LoginSbo()
        {
            // if (SboEngine.Login(cboSboDomain.Text, txtSboAccountName.Text.Trim(), txtSboPassword.Text.Trim()))
            if (TryLoginSbo(cboSboDomain.Text, txtSboAccountName.Text.Trim(), txtSboPassword.Text.Trim()))
            {
                DataContainer.SecondAccounts.Add(txtSboAccountName.Text.Trim());
                //Show tien cuoc, thong tin user
                this.Invoke((MethodInvoker)delegate ()
                {
                    sboLoginFailCount = 0;
                    lblSboRealDomain.Text = SboEngine.Host;
                    //var sboProfile = SboEngine.GetAccountProfile();
                    lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                    //lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                    //lblSboCashBalance.Text = sboProfile.CashBalance.ToString();
                    SboLoginUrl = cboSboDomain.Text;
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    sboLoginFailCount++;
                    lblSboStatus.Text = "FAIL " + sboLoginFailCount;
                });
            }
        }

        private bool TryLoginSbo(string url, string un, string pass)
        {
            SboEngine.IpFake = txtIpFake.Text;
            if (SboEngine.Login(url, un, pass))
            {
                return true;
            }

            Task.Run(() => SboEngine.TryFakeRequest(url));

            return SboEngine.Login(url, un, pass);
        }

        private int ibetLoginFailCount = 0;
        private void LoginIbet()
        {
            IbetEngine.ProxyLogin = ClientData.ProxyIbet;
            IbetEngine.ProxyEndpoint = ClientData.ProxyIbetEndpoint;

            if (IbetEngine.Login(cboIBetDomain.Text, txtIbetAccountName.Text.Trim(), txtIbetPassword.Text.Trim()))
            {
                DataContainer.FirstAccounts.Add(txtSboAccountName.Text.Trim());
                //Show tien cuoc, thong tin user
                this.Invoke((MethodInvoker)delegate ()
                {
                    ibetLoginFailCount = 0;
                    lblIbetRealDomain.Text = IbetEngine.Host;
                    //var sboProfile = SboEngine.GetAccountProfile();
                    lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                    //lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                    //lblSboCashBalance.Text = sboProfile.CashBalance.ToString();
                    IbetLoginUrl = cboIBetDomain.Text;
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    ibetLoginFailCount++;
                    lblIbetStatus.Text = "FAIL " + ibetLoginFailCount;
                });
            }
        }

        public void SetProcessingStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = GreyBackground;

                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = GreyBackground;
                btnStopIbetSbo.Enabled = false;
                btnStopIbetSbo.BackColor = GreyBackground;

                pbLoading.Visible = true;
                lblStatus.Text = "processing";
                lblStatus.Visible = true;
            });
        }

        public void SetDefaultStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStartIbetSbo.Enabled = true;
                btnStartIbetSbo.BackColor = OldLaceBackgroud;

                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = GreyBackground;
                btnStopIbetSbo.Enabled = false;
                btnStopIbetSbo.BackColor = GreyBackground;

                pbLoading.Visible = false;
                lblStatus.Visible = false;
            });
        }

        public void SetStartClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = GreyBackground;

                btnPauseIbetSbo.Enabled = true;
                btnPauseIbetSbo.BackColor = OldLaceBackgroud;
                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = OldLaceBackgroud;

                pbLoading.Visible = true;
                lblStatus.Text = "Running";
                lblStatus.Visible = true;

                cboMarket.Enabled = false;
            });
        }

        public void SetPauseClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = GreyBackground;

                btnStartIbetSbo.Enabled = true;
                btnStartIbetSbo.BackColor = OldLaceBackgroud;

                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = OldLaceBackgroud;

                pbLoading.Visible = false;
                lblStatus.Text = "Pause";

                cboMarket.Enabled = true;
            });
        }

        public void SetFourceStopClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = GreyBackground;

                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = GreyBackground;

                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = OldLaceBackgroud;

                pbLoading.Visible = false;
                lblStatus.Text = "Stopped";
            });
        }

        private void btnPauseIbetSbo_Click(object sender, EventArgs e)
        {
            //if (objUpdateCreditTimer != null)
            //{
            //    objUpdateCreditTimer.Dispose();
            //}

            PauseProcessor();
        }

        private void btnStopIbetSbo_Click(object sender, EventArgs e)
        {
            //if (objUpdateCreditTimer != null)//{
            //    objUpdateCreditTimer.Dispose();
            //}

            StopProcessor();
        }

        private void btnSaveManagement_Click(object sender, EventArgs e)
        {
            //Todo: Validate                   
            UpdateProcessConfig(this.Code);
        }

        private void UpdateProcessConfig(int code)
        {
            List<ProcessorConfigInfoDTO> list = this.MainIbetSboForm.ProcessorConfigs.ListProcessorConfig;
            var obj = list.Where(x => x.Code == code).FirstOrDefault();
            if (obj != null)
            {
                list.Remove(obj);
            }
            var newObj = ProcessBindData();
            list.Add(newObj);
            File.Delete(this.MainIbetSboForm.FileName);
            XMLHelper.SaveToXML(this.MainIbetSboForm.ProcessorConfigs, this.MainIbetSboForm.FileName);

        }


        #region DATA



        private AccountDTO IbetBindData()
        {
            return new AccountDTO()
            {
                ServerType = eServerType.Ibet,
                DomainName = cboIBetDomain.SelectedItem.ToString(),

                Username = txtIbetAccountName.Text.Trim(),
                Password = txtIbetPassword.Text.Trim(),
                RateExchange = Convert.ToInt16(txtIbetExchangeRate.Text),

                MaxStake = int.Parse(txtIbetMaxBet.Text.Trim()),
            };
        }
        private AccountDTO SbobetBindData()
        {
            return new AccountDTO()
            {
                ServerType = eServerType.Sbo,
                DomainName = cboSboDomain.SelectedItem.ToString(),
                Username = txtSboAccountName.Text.Trim(),
                Password = txtSboPassword.Text.Trim(),
                RateExchange = Convert.ToInt16(txtSboExchangeRate.Text),
                MaxStake = int.Parse(txtSboMaxBet.Text.Trim()),
            };
        }

        private ProcessorConfigInfoDTO ProcessBindData()
        {
            eBetStakeType betStakeType = eBetStakeType.Unknow;
            if (rdoMax.Checked)
                betStakeType = eBetStakeType.Max;
            if (rdoSbobet.Checked)
                betStakeType = eBetStakeType.Sbo;
            if (rdoIbet.Checked)
                betStakeType = eBetStakeType.Ibet;

            return new ProcessorConfigInfoDTO()
            {
                //TokenGUI = Guid.NewGuid(),
                Code = this.Code,
                Market = cboMarket.SelectedItem.ToString(),
                AccountFirst = IbetBindData(),
                AccountSecond = SbobetBindData(),
                BetStakeType = betStakeType,
                //BetStake = Convert.ToInt16(txtBetStake.Text.Trim()),
                BetStake = txtBetStake.Text.Trim(),
                TimeOffStakeOdds = (int)numericTimeOffStakeOdds.Value,
                CompareValue = Convert.ToDouble(cboCompareValue.Text),
                MaxCountBet = (int)numericMaxCountBet.Value,
                RebetSbo = (int)numericRebetSbo.Value,
                MinOddDefBet = Convert.ToSingle(cboMinOddDef.Text),
                MinTimeToBet = (int)numMinTimeToBet.Value,
                RebetIbet = ckReBetIbet.Checked,
                WaitingTimeRebetIbet = (int)numTimeWaitToRebetIbet.Value
            };
        }



        #endregion

        private void CheckInfoStatus(object sender, ElapsedEventArgs e)
        {
            try
            {
                var ibetProfile = IbetEngine.GetAccountProfile();

                if (ibetProfile.AvailabeCredit == 0)
                {
                    this.StopProcessor();
                    this.SetStartClickStyle();

                    Thread thread = new Thread(() => this.Processor.ReStart());
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();

                    return;
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                        lblIbetRealDomain.Text = IbetEngine.Host;
                        lblIbetCreadit.Text = ibetProfile.AvailabeCredit.ToString();
                        lblIbetCashBalance.Text = ibetProfile.CashBalance.ToString();
                    });
                }
            }
            catch (Exception ex)
            {
                this.StopProcessor();
            }

            try
            {
                var sboProfile = SboEngine.GetAccountProfile();

                if (sboProfile.AvailabeCredit == 0)
                {
                    this.StopProcessor();
                    this.SetStartClickStyle();
                    Thread thread = new Thread(() => this.Processor.ReStart());
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                }
                else
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                        lblSboRealDomain.Text = SboEngine.Host;
                        lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                        lblSboCashBalance.Text = sboProfile.CashBalance.ToString();
                    });
                }
            }
            catch (Exception ex)
            {
                this.StopProcessor();
            }
        }

        private void btnBetList_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                var ibetBetList = IbetEngine.GetBetList();
                var sboBetList = SboEngine.GetBetList();
                webIbetStatement.DocumentText = ibetBetList;
                webSboStatement.DocumentText = RemoveHTMLTagsText(sboBetList, "<head>");
            });
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                var ibetBetList = IbetEngine.GetStatement(dateHistoryPick.Value);
                var sboBetList = SboEngine.GetStatement(dateHistoryPick.Value);
                webIbetStatement.DocumentText = ibetBetList;
                webSboStatement.DocumentText = RemoveHTMLTagsText(sboBetList, "<head>");
            });
        }

        void OnIbetStatementDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webIbetStatement.Document != null)
            {
                IHTMLDocument2 currentDocument = (IHTMLDocument2)webIbetStatement.Document.DomDocument;

                int length = currentDocument.styleSheets.length;
                IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
                styleSheet.cssText = Css.IbetStatements;

            }
        }

        void OnSboStatementDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webSboStatement.Document != null)
            {
                IHTMLDocument2 currentDocument = (IHTMLDocument2)webSboStatement.Document.DomDocument;

                int length = currentDocument.styleSheets.length;
                IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
                styleSheet.cssText = Css.SboStatements;
            }
        }

        private string RemoveHTMLTagsText(string html, string tag)
        {
            try
            {
                int startIndex = html.IndexOf(tag.Remove(tag.Length - 1));
                startIndex = html.IndexOf(">", startIndex) + 1;
                int endIndex = html.IndexOf(tag.Insert(1, "/"), startIndex) - startIndex;
                html = html.Remove(startIndex, endIndex);
                return html;
            }
            catch (Exception)
            {
                return html;
            }
        }

        private void OnBtnRefeshClick(object sender, EventArgs e)
        {
            var btn = (PictureBox)sender;
            this.Invoke((MethodInvoker)delegate ()
           {
               if (btn.Name == "btnRefeshIbet")
               {
                   var ibetProfile = IbetEngine.GetAccountProfile();
                   lblIbetStatus.Text = IbetEngine.AccountStatus.ToString();
                   lblIbetRealDomain.Text = ibetProfile.UrlHost;
                   lblIbetCreadit.Text = ibetProfile.AvailabeCredit.ToString();
                   lblIbetCashBalance.Text = ibetProfile.CashBalance.ToString();
                   lblPingSbo.Text = "SKIPPED";
                   lblPingIbet.Text = "SKIPPED";
               }
               else
               {
                   var sboProfile = SboEngine.GetAccountProfile();
                   lblSboStatus.Text = SboEngine.AccountStatus.ToString();
                   lblSboRealDomain.Text = sboProfile.UrlHost;
                   lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                   lblSboCashBalance.Text = sboProfile.CashBalance.ToString();
                   lblPingSbo.Text = "SKIPPED";
                   lblPingIbet.Text = "SKIPPED";
               }
           });
        }

        private void OnStakeChanged(object sender, EventArgs e)
        {
            BindConvertStakeInfo();
        }

        private void BindConvertStakeInfo()
        {
            var stakes = txtBetStake.Text.Split(new[] { '#' });

            string stakeConvert = "";
            if (rdoIbet.Checked)
            {
                stakeConvert = "$Sbo= ";
                foreach (var stake in stakes)
                {
                    if (!string.IsNullOrEmpty(stake))
                    {
                        stakeConvert += "#" + ConvertStake(eServerType.Ibet, stake);
                    }
                }
                //lblConverStakeInfo.Text = "$Sbo= " + ConvertStake(eServerType.Ibet);
            }
            else if (rdoSbobet.Checked)
            {
                //lblConverStakeInfo.Text = "$Ibet= " + ConvertStake(eServerType.Sbo);
                stakeConvert = "$Ibet= ";
                foreach (var stake in stakes)
                {
                    if (!string.IsNullOrEmpty(stake))
                    {
                        stakeConvert += "#" + ConvertStake(eServerType.Sbo, stake);
                    }
                }
            }
            else
            {
                //lblConverStakeInfo.Text = "$Max";
                stakeConvert = "$Max";
                txtBetStake.Text = "";
            }
            lblConverStakeInfo.Text = stakeConvert;
        }

        private int ConvertStake(eServerType serverType, string stakeS)
        {
            //return 0;

            try
            {
                int stake = Convert.ToInt32(stakeS);

                int i = Convert.ToInt32(txtIbetExchangeRate.Text);
                int s = Convert.ToInt32(txtSboExchangeRate.Text);
                if (serverType == eServerType.Ibet)
                {
                    return (int)Math.Round((double)((stake * i) / s), 0);
                }
                else
                {
                    return (int)Math.Round((double)((stake * s) / i), 0);
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }

        private void cbIpFake_CheckedChanged(object sender, EventArgs e)
        {
            if (cbIpFake.Checked)
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    txtIpFake.Text = "";
                    txtIpFake.Enabled = true;
                });
            }
            else
            {
                this.Invoke((MethodInvoker)delegate ()
                {
                    txtIpFake.Text = "";
                    txtIpFake.Enabled = false;
                });
            }
        }

    }
}
