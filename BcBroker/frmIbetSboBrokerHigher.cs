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
    public partial class frmIbetSboBrokerHigher : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(frmIbetSboBrokerHigher));
       // private IBcManageService _manageService;
        private System.Timers.Timer timerPing;

        public frmIbetSboBrokerHigher()
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

            BrokerHigherControl ibetSbo = new BrokerHigherControl();
            ibetSbo.Dock = DockStyle.Fill;
            this.Controls.Add(ibetSbo);
        }
        
        public frmIbetSboBrokerHigher(string user)
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

            //BrokerData.IbetScanServers = betConfigIbet.ScanServers;
            //BrokerData.IbetAccounts = betConfigIbet.Accounts;
            //BrokerData.SboScanServers = betConfigSbo.ScanServers;
            //BrokerData.SboAccounts = betConfigSbo.Accounts;

            //Random r = new Random();
            //var ibetAcc = betConfigIbet.Accounts[r.Next(betConfigIbet.Accounts.Count)];
            //var ibetServer = betConfigIbet.ScanServers[r.Next(betConfigIbet.ScanServers.Count)];
            //var sboAcc = betConfigSbo.Accounts[r.Next(betConfigSbo.Accounts.Count)];
            //var sboServer = betConfigSbo.ScanServers[r.Next(betConfigSbo.ScanServers.Count)];

            //BrokerData.ScanIbetUsername = ibetAcc.Username;
            //BrokerData.ScanIbetPassword = ibetAcc.Password;
            //BrokerData.ScanIbetDomain = ibetServer;
            //BrokerData.ScanSboUsername = sboAcc.Username;
            //BrokerData.ScanSboPassword = sboAcc.Password;
            //BrokerData.ScanSboDomain = sboServer;

            this.Text = string.Concat("BcBroker - ", user, " © Copyright 2015");

            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            BrokerHigherControl ibetSbo = new BrokerHigherControl();
            ibetSbo.Dock = DockStyle.Fill;
            this.Controls.Add(ibetSbo);}

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);

            Application.Exit();
        }
    }
}
