using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.Objects;
using BcWin.Core.WCF;
using BcWin.Common.Contract;
using BcWin.Common.DTO;
using BCWin.Metadata;
using BcWin.Utils;
using BcWin.Processor;
using mshtml;
using BcWin.Processor.Interface;
using BcWin.UICustom;

namespace BcWin.UserControls
{
    public partial class AccountConfig : UserControl
    {
        public Guid ID { get; set; }

        // public Dictionary<Guid, IProcessor> ProcessorContainer = new Dictionary<Guid, IProcessor>();
        private IProcessor Processor = new IbetSboProcessor();
        public IbetEngine IbetEngine = new IbetEngine();
        public SboEngine SboEngine = new SboEngine();

        //Need store in BcWin
        //public List<string> FirstAccounts = new List<string>();
        //public List<string> SecondAccounts = new List<string>();

        BetTabControl _betTabControl;

        public bool IsAccountFirstLogged = false;
        public bool IsAccountSecondLogged = false;

        internal BcWin _bcWin;

        private void SetDefaultInitialize()
        {

            IsAccountFirstLogged = false;

            cboIBetDomain.Enabled = true;
            btnCheckIbetDomain.Enabled = true;
            panelBet.Enabled = false;
            linkChangeIbetDomain.Visible = false;
            btnFirstLogOff.Visible = false;
            lblFirstDomain.Visible = false;
            //WebBrowserNavigate(webBrowserIbet, cboIBetDomain.Text);


            IsAccountSecondLogged = false;
            cboSboDomain.Enabled = true;
            btnCheckSboDomain.Enabled = true;
            panelSbobet.Enabled = false;
            linkSboChangeDomain.Visible = false;
            btnSecondLogOff.Visible = false;
            lblSecondDomain.Visible = false;
            WebBrowserNavigate(webBrowserSbo, cboSboDomain.Text);


            if (_bcWin.FirstAccounts.Contains(txtFirstAccountName.Text.Trim()))
            {
                _bcWin.FirstAccounts.Remove(txtFirstAccountName.Text.Trim());
            }

            if (_bcWin.SecondAccounts.Contains(txtSecondAccountName.Text.Trim()))
            {
                _bcWin.SecondAccounts.Remove(txtSecondAccountName.Text.Trim());
            }

            Processor = new IbetSboProcessor();

            btnGoDashboard.Enabled = true;

            TabControl tabIbetSbobet = _betTabControl.Controls.Find("tabIbetSbobet", true).FirstOrDefault() as TabControl;
            TabPage tab = tabIbetSbobet.TabPages["TabDashboard"];
            if (tab != null)
            {
                tabIbetSbobet.TabPages.Remove(tab);
            }
            tab = tabIbetSbobet.TabPages["TabOdds"];
            if (tab != null)
            {
                tabIbetSbobet.TabPages.Remove(tab);
            }

            TabControl tabMain = _bcWin.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
            tabMain.SelectedTab.Text = "Ibet-Sbo";
        }

        public AccountConfig(BetTabControl betTabControl, BcWin bcWin)
        {
            InitializeComponent();
            ID = Guid.NewGuid();
           
            _betTabControl = betTabControl;
            _bcWin = bcWin;

            CoreProcessor.InitConfig();

            cboIBetDomain.DataSource = EngineHelper.GetIbetServers();
            cboSboDomain.DataSource = EngineHelper.GetSboServers();

            cboCompareValue.SelectedIndex = 0;

        }


        private void btnCheckIbetDomain_Click(object sender, EventArgs e)
        {
            bool result = true; //Need Check url is avaialbe here
            //
            if (result == false)
            {
                MessageBox.Show("Tên miền không tồn tại hoặc không kết nối được mạng. Vui lòng chọn tên miền khác hoặc kiểm tra mạng.", "Lỗi xảy ra.");
            }
            else
            {
                //WebBrowserNavigate(webBrowserIbet, cboIBetDomain.Text);
                btnCheckIbetDomain.Enabled = false;
                panelBet.Enabled = true;
            }
        }

        private void btnCheckSboDomain_Click(object sender, EventArgs e)
        {
            //WebBrowserNavigate(webBrowserSbo, cboSboDomain.Text);

            bool result = true; //Need Check url is avaialbe here
            if (result == false)
            {
                MessageBox.Show("Tên miền không tồn tại hoặc không kết nối được mạng. Vui lòng chọn tên miền khác hoặc kiểm tra mạng.", "Lỗi xảy ra.");
            }
            else
            {
                WebBrowserNavigate(webBrowserSbo, cboSboDomain.Text);
                btnCheckSboDomain.Enabled = false;
                panelSbobet.Enabled = true;
            }
        }

        private void btnIbetLogin_Click(object sender, EventArgs e)
        {
            bool isProcess = true;
            try
            {
                if (ValidateAccount(true))
                {
                    panelBet.Enabled = false;
                    pbLoading.Visible = true;

                    //AutoResetEvent setCookieEvent = new AutoResetEvent(false);
                    Task.Run(() =>
                    {
                        IbetEngine.LoginEvent.WaitOne(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000);
                        if (isProcess)
                        {
                            if (this.IbetEngine.CheckLogin())
                            {
                                //Show tien cuoc, thong tin user
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    _bcWin.FirstAccounts.Add(txtFirstAccountName.Text.Trim());
                                    lblFirstDomain.Text = this.IbetEngine.UrlHost;
                                    lblFirstDomain.Visible = true;
                                    pbLoading.Visible = false;
                                    panelBet.Enabled = false;
                                    btnFirstLogOff.Visible = true;
                                    btnFirstLogOff.Enabled = true;
                                    IsAccountFirstLogged = true;
                                    cboIBetDomain.Enabled = false;
                                });
                            }
                            else
                            {
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    MessageBox.Show("Lỗi trong quá trình đăng nhập. Vui lòng thử lại.", "Lỗi đăng nhập");
                                    pbLoading.Visible = false;
                                    FirstAccountLogOff();
                                });
                            }
                        }
                        IbetEngine.LoginEvent.Reset();
                    });
                    this.IbetEngine.Login(cboIBetDomain.Text, txtFirstAccountName.Text.Trim(), txtFirstPassword.Text.Trim());
                }
            }
            catch (Exception)
            {
                isProcess = false;
                MessageBox.Show("Lỗi trong quá trình đăng nhập. Vui lòng thử lại.", "Lỗi đăng nhập");
                pbLoading.Visible = false;
                FirstAccountLogOff();
            }

        }

        private void btnSboLogin_Click(object sender, EventArgs e)
        {
            bool isProcess = true;

            try
            {
                if (ValidateAccount(false))
                {
                    panelSbobet.Enabled = false;
                    pbLoading.Visible = true;
                    //AutoResetEvent setCookieEvent = new AutoResetEvent(false);                    
                    Task.Run(() =>
                    {
                        this.SboEngine.LoginEvent.WaitOne(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000);
                        if (isProcess)
                        {
                            if (this.SboEngine.CheckLogin())
                            {
                                //Show tien cuoc, thong tin user
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    _bcWin.SecondAccounts.Add(txtSecondAccountName.Text.Trim());
                                    lblSecondDomain.Text = this.SboEngine.UrlHost;
                                    lblSecondDomain.Visible = true;
                                    pbLoading.Visible = false;
                                    panelSbobet.Enabled = false;
                                    btnSecondLogOff.Visible = true;
                                    btnSecondLogOff.Enabled = true;
                                    IsAccountSecondLogged = true;
                                    cboSboDomain.Enabled = false;
                                });
                            }
                            else
                            {
                                this.Invoke((MethodInvoker)delegate()
                                {
                                    MessageBox.Show("Lỗi trong quá trình đăng nhập. Vui lòng thử lại.", "Lỗi đăng nhập");
                                    pbLoading.Visible = false;
                                    SecondAccountLogOff();
                                    });
                            }
                        }
                        this.SboEngine.LoginEvent.Reset();
                    });
                   // webBrowserSbo.DocumentCompleted -= OnDocumentCompleted;
                    this.SboEngine.Login(ref webBrowserSbo, txtSecondAccountName.Text.Trim(), txtSecondPassword.Text.Trim());
                   
                    //this.SboEngine.Login(ref webBrowserSbo, txtSecondAccountName.Text.Trim(), txtSecondPassword.Text.Trim());
                }
            }
            catch (Exception)
            {
                isProcess = false;
                MessageBox.Show("Lỗi trong quá trình đăng nhập. Vui lòng thử lại.", "Lỗi đăng nhập");
                pbLoading.Visible = false;
                SecondAccountLogOff();
            }

        }

        private void btnFirstLogOff_Click(object sender, EventArgs e)
        {
            if (_bcWin.FirstAccounts.Contains(txtFirstAccountName.Text.Trim()))
            {
                _bcWin.FirstAccounts.Remove(txtFirstAccountName.Text.Trim());
            }

            FirstAccountLogOff();
        }
        private void btnSecondLogOff_Click(object sender, EventArgs e)
        {
            if (_bcWin.SecondAccounts.Contains(txtSecondAccountName.Text.Trim()))
            {
                _bcWin.SecondAccounts.Remove(txtSecondAccountName.Text.Trim());
            }
            SecondAccountLogOff();

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //Need call Sync
            //Guid processGuid = service.InitServer(Guid.NewGuid(), IbetBindData(), SbobetBindData(), ProcessBindData());
            //System.Threading.Thread.Sleep(10000);
            //if (processGuid != Guid.Empty)
            //{
            //    service.StartServer(processGuid);
            //    btnStart.Enabled = false;
            //}
        }

        #region Bind Data

        private AccountDTO IbetBindData()
        {
            return new AccountDTO()
            {

                ServerType = eServerType.Ibet,
                Username = txtFirstAccountName.Text.Trim(),
                Password = txtFirstPassword.Text.Trim(),
                RateExchange = Convert.ToInt16(txtFirstExchangeRate.Text),

                MaxStake = int.Parse(txtFirstMaxBet.Text.Trim()),
            };
        }
        private AccountDTO SbobetBindData()
        {
            return new AccountDTO()
            {
                ServerType = eServerType.Sbo,
                Username = txtSecondAccountName.Text.Trim(),
                Password = txtSecondPassword.Text.Trim(),
                RateExchange = Convert.ToInt16(txtSecondExchangeRate.Text),
                MaxStake = int.Parse(txtSecondMaxBet.Text.Trim()),
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
                BetStakeType = betStakeType,
                BetStake = Convert.ToInt16(txtBetStake.Text.Trim()),
                TimeOffStakeOdds = (int)numericTimeOffStakeOdds.Value,
                CompareValue = Convert.ToDouble(cboCompareValue.Text),
                MaxCountBet = (int)numericMaxCountBet.Value,
                RebetSbo = (int)numericRebetSbo.Value,
                AccountFirst = IbetBindData(),
                AccountSecond = SbobetBindData()
                //IPAddress = txtIpAddress.Text.Trim(),
                //HostName = txtDomain.Text.Trim(),
                //IbetAccount = IbetBindData(),
                //SbobetAccount = SbobetBindData(),
                //IsMaxStake = rdoMax.Checked,
                //Todo...                          
            };
        }

        #endregion

        #region public event
        //private Dashboard _dashboard;
        public Guid InitProcessor(Dashboard dashboard)
        {
            //_dashboard = dashboard;
            try
            {
                btnFirstLogOff.Enabled = false;
                btnSecondLogOff.Enabled = false;

                //If pause va dang xuat, check procressId 
                //TabControl tabMain = _bcWin.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
                //Guid processGuid = new Guid();
                //Guid.TryParse(tabMain.SelectedTab.Name, out processGuid);


                //if (ProcessorContainer.ContainsKey(processGuid) == false)
                //{
                Guid ni = Guid.NewGuid();
                Processor.IbetEngine = this.IbetEngine;
                Processor.SboEngine = this.SboEngine;
                Processor.ProcessorConfigInfo = ProcessBindData();

               // Processor.OnLogScanEvent += dashboard.OnLogScanEvent;
                Processor.OnProcessExceptionEvent += dashboard.OnProcessorExceptionEvent;
                Processor.OnUpdateCredit += dashboard.OnUpdateCredit;

                /////Processor.IbetEngine.OnLogBetEvent += dashboard.OnLogBetEvent;
                /////Processor.SboEngine.OnLogBetEvent += dashboard.OnLogBetEvent;
                // ProcessorContainer.Add(ni, processor);
                return ni;
                //}
                //return processGuid;
            }
            finally
            {
                //RemoveTab(firstAccountDto.GuidID);
                //RemoveTab(secondAccountDto.GuidID);
            }

            //Object.ReferenceEquals(n1.GetType(), n2.GetType()));
        }

        public void StartProcessor(Dashboard dashboard, Guid processGuid, eScanType scanType)
        {
            //int countLogin = 0;
            //update Main Tab Name
            TabControl tabMain = _bcWin.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
            TabPage currentTab = tabMain.SelectedTab;
            currentTab.Text = txtFirstAccountName.Text + " - " + txtSecondAccountName.Text;
            //if (tabMain.SelectedTab.Name.StartsWith("Ibet_Sbo"))
            //{
            //    TabPage currentTab = tabMain.SelectedTab;
            //    //currentTab.Name = processGuid.ToString();
            //    currentTab.Text = txtFirstAccountName.Text + " - " + txtSecondAccountName.Text;
            //}


            //processor.Initialize();

            if (IsAccountFirstLogged && IsAccountSecondLogged)//Check da login 2 ben chua
            {
                Processor.IbetEngine = this.IbetEngine;
                Processor.SboEngine = this.SboEngine;
                Processor.ProcessorConfigInfo = ProcessBindData();

                if (Processor.Start(scanType) == Common.Objects.eServiceStatus.Started)
                {
                    //webBrowserIbet.Navigate("about:blank");
                    webBrowserSbo.Navigate("about:blank");
                }
                else
                {
                    //Quang msgbox start ko thanh cong
                    if (Processor.StartFailCount >= 3)
                    {
                        DialogResult result = MessageBox.Show("Khởi động máy quét vượt quá số lần cho phép. Vui lòng bấm OK để đăng nhập lại.", "Lỗi khởi động máy quét", MessageBoxButtons.OK);
                        if (result == DialogResult.OK)
                        {
                            StopProcessor();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lỗi xảy ra khi khởi động máy quét.", "Lỗi.");
                        dashboard.SetDefaultStyle();
                    }
                    //countLogin++;

                }
            }
            else
            {
                MessageBox.Show("Tài khoản chưa đăng nhập. Vui lòng kiểm tra lại.", "Lỗi đăng nhập.");
                dashboard.SetDefaultStyle();
            }

        }

        public void PauseProcessor()
        {
            btnFirstLogOff.Enabled = true;
            btnSecondLogOff.Enabled = true;


            if (Processor != null && Processor.Status == eServiceStatus.Started)
            {
                Processor.Pause();
            }
        }

        public void StopProcessor()
        {

            if (Processor.Status == eServiceStatus.Started || Processor.Status == eServiceStatus.Initialized || Processor.Status == eServiceStatus.Paused)
            {
                Processor.Dispose();

            }

            SetDefaultInitialize();
        }

        //public IProcessor GetCurrentProcessor()
        //{
        //    TabControl tabMain = _bcWin.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
        //    Guid processGuid = new Guid();
        //    Guid.TryParse(tabMain.SelectedTab.Name, out processGuid);
        //    if (ProcessorContainer.ContainsKey(processGuid))
        //    {
        //        return ProcessorContainer[processGuid];
        //    }
        //    return null;
        //}

        #endregion

        private void txtIbetMaxBet_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtSbobetMaxBet_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtBetStake_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtFirstExchangeRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtSecondExchangeRate_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Check if the pressed character was a backspace or numeric.
            if (e.KeyChar != (char)8 && !char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
            }
        }


        private void btnGoDashboard_Click(object sender, EventArgs e)
        {
            //
            if (ValidateForm())
            {
                TabControl tabIbetSbobet = _betTabControl.Controls.Find("tabIbetSbobet", true).FirstOrDefault() as TabControl;

                Dashboard dashboardControl = new Dashboard(this);
                dashboardControl.Dock = DockStyle.Fill;
                BcTabPage dashboardTab = new BcTabPage();
                dashboardTab.Controls.Add(dashboardControl);
                dashboardTab.Name = "TabDashboard";
                dashboardTab.Text = "Bảng điều khiển";
                tabIbetSbobet.TabPages.Add(dashboardTab);
                tabIbetSbobet.SelectedTab = dashboardTab;

                OddList oddControl = new OddList(this);
                oddControl.Dock = DockStyle.Fill;
                BcTabPage oddTab = new BcTabPage();
                oddTab.Controls.Add(oddControl);
                oddTab.Name = "TabOdds";
                oddTab.Text = "Danh sách kèo";
                tabIbetSbobet.TabPages.Add(oddTab);

                btnGoDashboard.Enabled = false;
            }
        }

        #region Validate

        private bool ValidateForm()
        {
            if (IsAccountFirstLogged == false || IsAccountSecondLogged == false)
            {
                MessageBox.Show("Vui lòng đăng nhập cặp tài khoản.");
                return false;
            }
            if (txtBetStake.Text.Trim().Length == 0)
            {
                MessageBox.Show("Vui lòng nhập tiền cược.");
                return false;
            }

            if (cboCompareValue.SelectedIndex < 0)
            {
                MessageBox.Show("Vui lòng chọn chêch lệch giá giữa 2 kèo.");
                return false;
            }

            return true;
        }

        private bool ValidateAccount(bool isFirstAccount)
        {
            string message = "Vui lòng nhập đầy đủ thông tin tài khoản.";
            if (isFirstAccount)
            {
                string FirstAccountName = txtFirstAccountName.Text.Trim();
                if (FirstAccountName.Length == 0
                    || txtFirstPassword.Text.Trim().Length == 0
                    || txtFirstExchangeRate.Text.Trim().Length == 0
                    || txtFirstMaxBet.Text.Trim().Length == 0)
                {
                    MessageBox.Show(message, "Lỗi đăng nhập.");
                    return false;
                }
                else
                {
                    if (_bcWin.FirstAccounts.Contains(FirstAccountName))
                    {
                        MessageBox.Show("Tài khoản này đã đăng nhập. Vui lòng nhập tài khoản khác.", "Lỗi đăng nhập.");
                        return false;
                    }
                }

            }
            else
            {
                string SecondAccountName = txtSecondAccountName.Text.Trim();
                if (SecondAccountName.Length == 0
                    || txtSecondPassword.Text.Trim().Length == 0
                    || txtSecondExchangeRate.Text.Trim().Length == 0
                    || txtSecondMaxBet.Text.Trim().Length == 0)
                {
                    MessageBox.Show(message, "Lỗi đăng nhập.");
                    return false;
                }
                else
                {
                    if (_bcWin.SecondAccounts.Contains(SecondAccountName))
                    {
                        MessageBox.Show("Tài khoản này đã đăng nhập. Vui lòng nhập tài khoản khác.", "Lỗi đăng nhập.");
                        return false;
                    }
                }
            }
            return true;
        }
        #endregion


        private void cboIBetDomain_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnCheckIbetDomain.Enabled = true;
            panelBet.Enabled = false;
        }

        private void cboSboDomain_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnCheckSboDomain.Enabled = true;
            panelSbobet.Enabled = false;
        }

        private void linkChangeIbetDomain_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cboIBetDomain.Enabled = true;
            btnCheckIbetDomain.Enabled = true;
            panelBet.Enabled = false;
            linkChangeIbetDomain.Visible = false;
        }

        private void linkSboChangeDomain_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            cboSboDomain.Enabled = true;
            btnCheckSboDomain.Enabled = true;
            panelSbobet.Enabled = false;
            linkSboChangeDomain.Visible = false;
        }


        #region Private function


        private void WebBrowserNavigate(WebBrowser webBrowser, string url)
        {
           // Guid uId = Guid.NewGuid();
            //webBrowser.ScriptErrorsSuppressed = true;
           
            //webBrowser.Name = uId.ToString();
            webBrowser.AllowNavigation = true;
            webBrowser.Navigate(url);
           // webBrowser.DocumentCompleted += OnDocumentCompleted;

        }
        void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var w = (WebBrowser)sender;
           
            //var aa = e.Url.AbsolutePath;
            if (w.ReadyState == WebBrowserReadyState.Complete)
            {
                w.AllowNavigation = true;
                if (w.Document.GetElementById("username") == null)
                {
                    MessageBox.Show("Tên miền không tồn tại hoặc không kết nối được mạng. Vui lòng chọn tên miền khác hoặc kiểm tra mạng.", "Lỗi xảy ra.");
                }
                else
                {                    
                    btnCheckSboDomain.Enabled = false;
                    panelSbobet.Enabled = true;
                }
               
            }
        }

        private void OnWebBrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var w = (WebBrowser)sender;
            if (w.ReadyState == WebBrowserReadyState.Complete)
            {
                HtmlElement head = w.Document.GetElementsByTagName("head")[0];
                HtmlElement scriptEl = w.Document.CreateElement("script");
                IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
                string alertBlocker = "window.alert = function () { }";
                element.text = alertBlocker;
                head.AppendChild(scriptEl);
            }
        }

        private void FirstAccountLogOff()
        {
            lblFirstDomain.Visible = false;
            btnFirstLogOff.Visible = false;
            linkChangeIbetDomain.Visible = true;
            this.IbetEngine.LogOff();
            this.IsAccountFirstLogged = false;
            this.panelBet.Enabled = true;
            //WebBrowserNavigate(webBrowserIbet, cboIBetDomain.Text);
        }

        private void SecondAccountLogOff()
        {
            lblSecondDomain.Visible = false;
            btnSecondLogOff.Visible = false;
            linkSboChangeDomain.Visible = true;
            this.SboEngine.LogOff();
            this.IsAccountSecondLogged = false;
            this.panelSbobet.Enabled = true;
            WebBrowserNavigate(webBrowserSbo, cboSboDomain.Text);
        }

        #endregion

    }
}
