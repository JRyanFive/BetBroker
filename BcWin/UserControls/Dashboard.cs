using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;

namespace BcWin.UserControls
{
    public partial class Dashboard : UserControl
    {
        private AccountConfig _accountConfig;

        Color GreyBackground = Color.Gray;
        Color OldLaceBackgroud = Color.OldLace;

        public Dashboard()
        {
            InitializeComponent();
        }
        public Dashboard(AccountConfig accountConfig)
        {
            InitializeComponent();
            _accountConfig = accountConfig;

            pbPause.BackColor = GreyBackground;
            pbStop.BackColor = GreyBackground;
            //Init
            SetAccountInfo();

            cboMarket.SelectedIndex = 0;
        }

        public void SetDefaultStyle()
        {
            pbPlay.Enabled = true;
            pbPlay.BackColor = OldLaceBackgroud;

            pbPause.Enabled = false;
            pbPause.BackColor = GreyBackground;
            pbStop.Enabled = false;
            pbStop.BackColor = GreyBackground;

            pbLoading.Visible = false;
            lblStatus.Visible = false;
        }

        public void SetPlayClickStyle()
        {
            pbPlay.Enabled = false;
            pbPlay.BackColor = GreyBackground;

            pbPause.Enabled = true;
            pbPause.BackColor = OldLaceBackgroud;
            pbStop.Enabled = true;
            pbStop.BackColor = OldLaceBackgroud;

            pbLoading.Visible = true;
            lblStatus.Text = "Running";
            lblStatus.Visible = true;

            cboMarket.Enabled = false;
        }

        public void SetPauseClickStyle()
        {
            pbPause.Enabled = false;
            pbPause.BackColor = GreyBackground;

            pbPlay.Enabled = true;
            pbPlay.BackColor = OldLaceBackgroud;

            pbStop.Enabled = true;
            pbStop.BackColor = OldLaceBackgroud;

            pbLoading.Visible = false;
            lblStatus.Text = "Pause";

            cboMarket.Enabled = true;
        }

        public void SetFourceStopClickStyle()
        {
            pbPause.Enabled = false;
            pbPause.BackColor = GreyBackground;

            pbPlay.Enabled = false;
            pbPlay.BackColor = GreyBackground;

            pbStop.Enabled = true;
            pbStop.BackColor = OldLaceBackgroud;

            pbLoading.Visible = false;
            lblStatus.Text = "Stopped";
        }


        public void SetAccountInfo()
        {
            TextBox account = _accountConfig.Controls.Find("txtFirstAccountName", true).FirstOrDefault() as TextBox;
            TextBox exchange = _accountConfig.Controls.Find("txtFirstExchangeRate", true).FirstOrDefault() as TextBox;
            // numericFirstExchangeRate
            lblFirstDomain.Text = _accountConfig.IbetEngine.UrlHost;
            lblFirstAccountName.Text = account.Text;
            lblFirstAvaiableCredit.Text = _accountConfig.IbetEngine.GetAccountProfile().AvailabeCredit.ToString();
            lblFirstRateExchange.Text = exchange.Text.ToString();

            exchange = _accountConfig.Controls.Find("txtSecondExchangeRate", true).FirstOrDefault() as TextBox;

            lblSecondDomain.Text = _accountConfig.SboEngine.UrlHost;
            lblSecondAccountName.Text = _accountConfig.SboEngine.UserName;
            lblSecondAvaiableCredit.Text = _accountConfig.SboEngine.GetAvailabeCredit().ToString();
            lblSecondRateExchange.Text = exchange.Text.ToString();
        }

        #region Hanlde Play, Pause, Stop

        private void pbPlay_Click(object sender, EventArgs e)
        {
            eScanType scanType = (eScanType)cboMarket.SelectedIndex;
            SetPlayClickStyle();
            Guid guidProcessor = _accountConfig.InitProcessor(this);
            _accountConfig.StartProcessor(this, guidProcessor, scanType);
        }

        private void pbPause_Click(object sender, EventArgs e)
        {

            SetPauseClickStyle();
            _accountConfig.PauseProcessor();

        }

        private void pbStop_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có thật sự muốn ngừng quét không?", "Confirmation", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                _accountConfig.StopProcessor();
            }


        }

        #endregion


        private Button FindButtonControl(string buttonName)
        {
            return _accountConfig.Controls.Find(buttonName, true).FirstOrDefault() as Button;
        }



        ////public void OnLogBetEvent(Common.Objects.LogBetMessage logMsg)
        ////{
        ////    var odd = logMsg.HomeOdd + " | " + logMsg.AwayOdd;
        ////    string[] row1 =
        ////    {
        ////        logMsg.Time, logMsg.ServerType.ToString(), logMsg.HomeTeamName, logMsg.AwayTeamName, odd,
        ////        logMsg.BetType.ToString(), logMsg.BetStake.ToString(), logMsg.Status.ToString()
        ////    };

        ////    this.Invoke((MethodInvoker)delegate()
        ////    {
        ////        dgvBetLog.Rows.Insert(0, row1);
        ////        //switch (logMsg.ServerType)
        ////        //{
        ////        //    case eServerType.Ibet:
        ////        //        this.lblFirstAvaiableCredit.Text = _accountConfig.IbetEngine.AvailabeCredit.ToString();
        ////        //        break;
        ////        //    case eServerType.Sbo:
        ////        //        this.lblSecondAvaiableCredit.Text = _accountConfig.SboEngine.AvailabeCredit.ToString();
        ////        //        break;
        ////        //}
        ////    });
        ////}

        public void OnProcessorExceptionEvent(Common.Objects.ExceptionMessage exMsg, eServerType serverType)
        {
            string msg = string.Empty;
            switch (exMsg.ExceptionType)
            {
                case eExceptionType.LoginFail:
                    msg =
                    "Tài khoản của bạn hiện đang đăng nhâp tai một địa chỉ khác. Hệ thống đã tự động dừng quét. Vui lòng chọn STOP và cấu hình lại.";
                    break;
                default:
                    msg = "Lỗi hệ thống quét. Vui lòng chọn STOP và cấu hình lại.";
                    break;
            }

            this.Invoke((MethodInvoker)delegate()
            {
                TabControl tabMain = _accountConfig._bcWin.Controls.Find("tabMain", true).FirstOrDefault() as TabControl;
                TabPage currentTab = tabMain.TabPages[_accountConfig.ID.ToString()];
                string tabText = currentTab.Text;
                //currentTab.Name = processGuid.ToString();
                currentTab.Text = "LỖI";

                SetFourceStopClickStyle();
                lbExMsg.Text = msg;
                MessageBox.Show(this,
                    string.Format("Lỗi cặp {0}" + Environment.NewLine + "Vui lòng kiểm tra lại !", tabText));
            });
            //this.Invoke(new Action(() => { MessageBox.Show(this, "text"); }));
        }

        public void OnLogScanEvent(Common.Objects.LogScanMessage logMsg)
        {
            ///insert here
            ///
            // dgvScanLog.Da
            //string sboOdd;
            //string ibetOdd;
            //if (logMsg.FirstServerType == eServerType.Ibet)
            //{
            //    ibetOdd = logMsg.FirstHomeOdd + " | " + logMsg.FirstAwayOdd;
            //    sboOdd = logMsg.SecondHomeOdd + " | " + logMsg.SecondAwayOdd;
            //}
            //else
            //{
            //    ibetOdd = logMsg.SecondHomeOdd + " | " + logMsg.SecondAwayOdd;
            //    sboOdd = logMsg.FirstHomeOdd + " | " + logMsg.FirstAwayOdd;
            //}

            //string[] row1 = { logMsg.Time, logMsg.HomeTeamName, logMsg.AwayTeamName, sboOdd, ibetOdd, logMsg.OddType.ToString() };

            //this.Invoke((MethodInvoker)delegate()
            //{
            //    dgvScanLog.Rows.Insert(0, row1);
            //});
        }

        public void OnUpdateCredit(object sender, EventArgs e)
        {
            ///insert here

            this.Invoke((MethodInvoker)delegate()
            {
                var ibetCredit = _accountConfig.IbetEngine.AvailabeCredit;
                var sboCredit = _accountConfig.SboEngine.AvailabeCredit;

                lbStatusAccFirst.Text = ibetCredit <= 0 ? "Offline" : "Online";
                lbStatusAccSecond.Text = sboCredit <= 0 ? "Offline" : "Online";

                this.lblFirstAvaiableCredit.Text = ibetCredit.ToString();
                this.lblSecondAvaiableCredit.Text = sboCredit.ToString();
                this.lblFirstDomain.Text = _accountConfig.IbetEngine.UrlHost;
                this.lblSecondDomain.Text = _accountConfig.SboEngine.UrlHost;
            });
        }

        private void pbFirstGetCredit_Click(object sender, EventArgs e)
        {
            if (_accountConfig.IsAccountFirstLogged)
            {
                string credit = _accountConfig.IbetEngine.GetAvailabeCredit().ToString();
                lblFirstAvaiableCredit.Invoke((MethodInvoker)(() => lblFirstAvaiableCredit.Text = credit));
            }
        }

        private void pbSecondGetCredit_Click(object sender, EventArgs e)
        {
            if (_accountConfig.IsAccountSecondLogged)
            {
                string credit = _accountConfig.SboEngine.GetAvailabeCredit().ToString();
                lblSecondAvaiableCredit.Invoke((MethodInvoker)(() => lblSecondAvaiableCredit.Text = credit));
            }
        }







    }
}
