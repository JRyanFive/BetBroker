using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.CssStyle;
using BCWin.Broker;
using mshtml;

namespace BcBroker.UserControls
{
    public sealed partial class IbetSboBrokerControl : UserControl
    {
        public int SumBuy { get; set; }
        public int SumSell { get; set; }
        public int SumGoodTras { get; set; }
        public int SumBadTrans { get; set; }
        public int SumMissTrans { get; set; }

        IbetSboBroker broker = new IbetSboBroker();

        Color ClickedStyleButton = Color.LightCyan;
        Color NomalStyleButton = Color.WhiteSmoke;

        public IbetSboBrokerControl()
        {
            InitializeComponent();
            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            cboIBetDomain.DataSource = DataContainer.IbetServers;
            cboSboDomain.DataSource = DataContainer.SboServers;
            cbOddPairCheck.SelectedIndex = 2;
            cbGoalDefCheck.SelectedIndex = 2;

            SetDefaultStyle();

            broker.OnWriteTextLog += broker_OnWriteTextLog;

            webIbetStatement.DocumentCompleted += OnIbetStatementDocumentCompleted;
            lblSboStatus.TextChanged += OnStatusTextChanged;
            lblIbetStatus.TextChanged += OnStatusTextChanged;
        }

        void broker_OnWriteTextLog(string logMsg, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow)
        {
            switch (status)
            {
                case eBrokerStatus.Buy:
                    SumBuy++;
                    break;
                case eBrokerStatus.Sell:
                    SumSell++;
                    break;
                case eBrokerStatus.GoodTrans:
                    SumSell++;
                    SumGoodTras++;
                    break;
                case eBrokerStatus.BadTrans:
                    SumSell++;
                    SumBadTrans++;
                    break;
                case eBrokerStatus.MissTrans:
                    SumMissTrans++;
                    break;
            }

            if (status != eBrokerStatus.Unknow)
            {
                BindSummary();
            }

            UpdateLogText(logMsg, type);
        }

        private void btnStartIbetSbo_Click(object sender, EventArgs e)
        {
            StartProcessBroker();
        }

        private void btnPauseIbetSbo_Click(object sender, EventArgs e)
        {
            PauseProcessBroker();
        }

        private void btnStopIbetSbo_Click(object sender, EventArgs e)
        {
            StopProcessBroker();
        }

        private void btnBetList_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                var ibetBetList = broker.IbetEn.GetBetList();
                var sboBetList = broker.SboEn.GetBetList();
                webIbetStatement.DocumentText = ibetBetList;
                webSboStatement.DocumentText = sboBetList;
            });
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                var ibetBetList = broker.IbetEn.GetStatement(dateHistoryPick.Value);
                var sboBetList = broker.SboEn.GetStatement(dateHistoryPick.Value);
                webIbetStatement.DocumentText = ibetBetList;
                webSboStatement.DocumentText = sboBetList;
            });
        }

        private void btnRefesh_Click(object sender, EventArgs e)
        {
            var btn = (PictureBox)sender;
            this.Invoke((MethodInvoker)delegate()
            {
                if (btn.Name == "btnRefeshIbet")
                {
                    var ibetProfile = broker.IbetEn.GetAccountProfile();
                    lblIbetStatus.Text = broker.IbetEn.AccountStatus.ToString();
                    lblIbetRealDomain.Text = ibetProfile.UrlHost;
                    lblIbetCreadit.Text = ibetProfile.AvailabeCredit.ToString();
                    lblIbetCashBalance.Text = ibetProfile.CashBalance.ToString();
                    lblPingSbo.Text = "SKIPPED";
                    lblPingIbet.Text = "OK!";
                }
                else
                {
                    var sboProfile = broker.SboEn.GetAccountProfile();
                    lblSboStatus.Text = broker.SboEn.AccountStatus.ToString();
                    lblSboRealDomain.Text = sboProfile.UrlHost;
                    lblSboCreadit.Text = sboProfile.AvailabeCredit.ToString();
                    lblSboCashBalance.Text = sboProfile.CashBalance.ToString();
                    lblPingSbo.Text = "OK!";
                    lblPingIbet.Text = "SKIPPED";
                }
            });
        }

        private void cbIpFake_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox ckBox = (CheckBox)sender;
            if (ckBox.Checked)
            {
                if (ckBox.Name == "ckIpFake")
                {
                    txtIpFakeSbo.Enabled = true;
                }
                else
                {
                    txtIpFakeIbet.Enabled = true;
                }
            }
            else
            {
                if (ckBox.Name == "ckIpFake")
                {
                    txtIpFakeSbo.Enabled = false;
                    //txtIpFakeSbo.Text = "";
                }
                else
                {
                    txtIpFakeIbet.Enabled = false;
                    //txtIpFakeIbet.Text = "";
                }
            }
        }

        public void StartProcessBroker()
        {
            SetProcessingStyle();

            if (broker.IbetEn.AccountStatus == eAccountStatus.Offline)
            {
                if (ckProxyLoginIbet.Checked)
                {
                    broker.IbetEn.ProxyLogin = true;
                    broker.IbetEn.ProxyEndpoint = string.Concat("net.tcp://", txtIpFakeIbet.Text, ":9998/bcwinsupservice");
                }
                LoginIbet();
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Offline)
            {
                LoginSbo();
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Online
                        && broker.IbetEn.AccountStatus == eAccountStatus.Online)
            {
                broker.OddPairCheck = float.Parse(cbOddPairCheck.Text);
                broker.GoalDefCheck = float.Parse(cbGoalDefCheck.Text);
                broker.TimeCheckScan = (int)numTimeCheckScan.Value;

                broker.StartScanBroker();
                if (broker.Status==eServiceStatus.Started)
                {
                    SetStartClickStyle();
                    this.Invoke((MethodInvoker) delegate()
                    {
                        lblIbetCreadit.Text = broker.IbetEn.AvailabeCredit.ToString();
                        lblSboCreadit.Text = broker.SboEn.AvailabeCredit.ToString();
                    });
                }
                else
                {
                    SetDefaultStyle();
                }
            }
            else
            {
                SetDefaultStyle();
            }
        }

        public void PauseProcessBroker()
        {
            if (broker.Status == eServiceStatus.Started)
            {
                broker.Pause();
                SetPauseClickStyle();
            }
        }

        public void StopProcessBroker()
        {
            if (broker.Status == eServiceStatus.Started || broker.Status == eServiceStatus.Paused)
            {
                broker.Stop();
                SetDefaultStyle();
            }
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

        void OnStatusTextChanged(object sender, EventArgs e)
        {
            var l = (Label)sender;
            if (l.Text == "Online")
            {
                l.ForeColor = Color.Blue;
            }
            else
            {
                l.ForeColor = Color.Red;
            }
        }

        private void LoginIbet()
        {
            if (broker.IbetEn.Login(cboIBetDomain.Text, txtIbetAccountName.Text.Trim(), txtIbetPassword.Text.Trim()))
            {
                UpdateLogText("Đăng nhập tài khoản ibet thành công.");
                this.Invoke((MethodInvoker)delegate()
                {
                    lblIbetRealDomain.Text = broker.IbetEn.Host;
                    lblIbetStatus.Text = broker.IbetEn.AccountStatus.ToString();
                });
            }
            else
            {
                UpdateLogText("Đăng nhập tài khoản ibet không thành công.", eLogTextType.Error);
            }
        }

        private void LoginSbo()
        {
            if (broker.SboEn.Login(cboSboDomain.Text, txtSboAccountName.Text.Trim(), txtSboPassword.Text.Trim()))
            {
                UpdateLogText("Đăng nhập tài khoản sbo thành công.");
                this.Invoke((MethodInvoker)delegate()
                {
                    lblSboRealDomain.Text = broker.SboEn.Host;
                    lblSboStatus.Text = broker.SboEn.AccountStatus.ToString();
                });
            }
            else
            {
                UpdateLogText("Đăng nhập tài khoản sbo không thành công.", eLogTextType.Error);
            }
        }

        private void UpdateLogText(string log, eLogTextType type = eLogTextType.Info)
        {
            Task.Run(() =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    txtLog.AppendText(Environment.NewLine);
                    Color c;
                    switch (type)
                    {
                        case eLogTextType.Highlight:
                            c = Color.DarkMagenta;
                            break;
                        case eLogTextType.Error:
                            c = Color.Crimson;
                            break;
                        case eLogTextType.Warning:
                            c = Color.DarkBlue;
                            break;
                        case eLogTextType.LowLevel:
                            c = Color.SlateGray;
                            break;
                        case eLogTextType.Important:
                            c = Color.Red;
                            break;
                        default:
                            c = Color.Black;
                            break;
                    }
                    log = string.Concat(DateTime.Now, ">> ", log);
                    txtLog.Select(txtLog.TextLength, 0);
                    txtLog.SelectionColor = c;
                    txtLog.AppendText(log);
                    txtLog.ScrollToCaret();
                }));
            });
        }

        public void BindSummary()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                lbBuy.Text = SumBuy.ToString();
                lbSell.Text = SumSell.ToString();
                lbGoodTrans.Text = SumGoodTras.ToString();
                lbBadTrans.Text = SumBadTrans.ToString();
                lbMissTrans.Text = SumMissTrans.ToString();
            });
        }

        public void SetProcessingStyle()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = ClickedStyleButton;

                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = ClickedStyleButton;
                btnStopIbetSbo.Enabled = false;
                btnStopIbetSbo.BackColor = ClickedStyleButton;

                pbLoading.Visible = true;
                lblStatus.Text = "processing";
                lblStatus.Visible = true;
            });
        }

        public void SetDefaultStyle()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                btnStartIbetSbo.Enabled = true;
                btnStartIbetSbo.BackColor = ClickedStyleButton;

                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = NomalStyleButton;
                btnStopIbetSbo.Enabled = false;
                btnStopIbetSbo.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Visible = false;
            });
        }

        public void SetStartClickStyle()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = ClickedStyleButton;

                btnPauseIbetSbo.Enabled = true;
                btnPauseIbetSbo.BackColor = NomalStyleButton;
                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = NomalStyleButton;

                pbLoading.Visible = true;
                lblStatus.Text = "Running";
                lblStatus.Visible = true;
            });
        }

        public void SetPauseClickStyle()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = ClickedStyleButton;

                btnStartIbetSbo.Enabled = true;
                btnStartIbetSbo.BackColor = NomalStyleButton;

                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Text = "Pause";
            });
        }

        public void SetFourceStopClickStyle()
        {
            this.Invoke((MethodInvoker)delegate()
            {
                btnPauseIbetSbo.Enabled = false;
                btnPauseIbetSbo.BackColor = ClickedStyleButton;

                btnStartIbetSbo.Enabled = false;
                btnStartIbetSbo.BackColor = ClickedStyleButton;

                btnStopIbetSbo.Enabled = true;
                btnStopIbetSbo.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Text = "Stopped";
            });
        }
    }
}
