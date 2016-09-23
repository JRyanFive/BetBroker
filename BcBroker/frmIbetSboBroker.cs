using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using BcBroker.UserControls;
using BcWin.Common.DTO;
using BcWin.Contract;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Processor;
using BCWin.Metadata;
using log4net;

namespace BcBroker
{
    public partial class frmIbetSboBroker : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(frmIbetSboBroker));
        private IBcManageService _manageService;
        private System.Timers.Timer timerPing;

        public frmIbetSboBroker()
        {
            InitializeComponent();

            //var newPoint = 126.Round(50);

            CoreProcessor.InitConfig();
            SystemConfig.TIME_GET_UPDATE_LIVE_IBET = 11000;
            SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = 10000;

            BrokerData.ScanIbetUsername = "TKH19900030";
            BrokerData.ScanIbetPassword = "ABab1212";
            BrokerData.ScanIbetDomain = "http://www.88cado.com/";
            BrokerData.ScanSboUsername = "msn99aa030";
            BrokerData.ScanSboPassword = "123@@Google";
            BrokerData.ScanSboDomain = "http://www.currybread.com/";

            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            BrokerControl ibetSbo = new BrokerControl();
            ibetSbo.Dock = DockStyle.Fill;
            this.Controls.Add(ibetSbo);
        }
        
        public frmIbetSboBroker(string user, IBcManageService service, ScanInfoDTO betConfigSbo, ScanInfoDTO betConfigIbet)
        {
            //if (DateTime.Now > new DateTime(2015, 11, 20))
            //{
            //    MessageBox.Show("EX12301: Lỗi parse data!");
            //    return;
            //}

            InitializeComponent();
            CoreProcessor.InitConfig();
            SystemConfig.TIME_GET_UPDATE_LIVE_IBET = 11000;
            SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = 10000;

            BrokerData.IbetScanServers = betConfigIbet.ScanServers;
            BrokerData.IbetAccounts = betConfigIbet.Accounts;
            BrokerData.SboScanServers = betConfigSbo.ScanServers;
            BrokerData.SboAccounts = betConfigSbo.Accounts;

            Random r = new Random();
            var ibetAcc = betConfigIbet.Accounts[r.Next(betConfigIbet.Accounts.Count)];
            var ibetServer = betConfigIbet.ScanServers[r.Next(betConfigIbet.ScanServers.Count)];
            var sboAcc = betConfigSbo.Accounts[r.Next(betConfigSbo.Accounts.Count)];
            var sboServer = betConfigSbo.ScanServers[r.Next(betConfigSbo.ScanServers.Count)];

            BrokerData.ScanIbetUsername = ibetAcc.Username;
            BrokerData.ScanIbetPassword = ibetAcc.Password;
            BrokerData.ScanIbetDomain = ibetServer;
            BrokerData.ScanSboUsername = sboAcc.Username;
            BrokerData.ScanSboPassword = sboAcc.Password;
            BrokerData.ScanSboDomain = sboServer;

            this.Text = string.Concat("BcBroker - ", user, " © Copyright 2015");

            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            BrokerControl ibetSbo = new BrokerControl();
            ibetSbo.Dock = DockStyle.Fill;
            this.Controls.Add(ibetSbo);

            _manageService = service;
            _manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, 10);

            timerPing = new System.Timers.Timer(60000);
            timerPing.Elapsed += PingManage;
            timerPing.Start();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            Application.Exit();
        }

        public static int keyPing = 11;
        void PingManage(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, keyPing) != 1)
                {
                    timerPing.Stop();

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
