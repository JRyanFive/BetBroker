using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Core.Utils;
using BcWin.Processor;
using BcWin.Processor.Service;
using BCWin.Metadata;
using log4net;
using log4net.Repository.Hierarchy;

namespace BcWinScan
{
    public partial class frmMain : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmMain));
        private IBcManageService _manageService;
        private System.Threading.Timer objCheckScanDriver;
        public frmMain(IBcManageService manageService)
        {
            InitializeComponent();
            cboMarket.SelectedIndex = 0;
            Process.Driver = new IbetSboDriver();
            
            _manageService = manageService;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            ScanData.IsDispose = true;
            base.Dispose(disposing);
            Application.Exit();
        }

        private void btnIbetSboStart_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                lblStartTime.Text = DateTime.Now.ToString();
                lblUsername.Text = SelfInfo.Username;
                btnIbetSboStart.Enabled = false;
            });

            eScanType scanType = (eScanType)cboMarket.SelectedIndex;

            Process.Driver.ManageService = _manageService;
            System.Timers.Timer timerPing = new System.Timers.Timer(60000 * 2);
            timerPing.Elapsed += PingManage;
            timerPing.Start();

            StartDriver(scanType);
        }

        void PingManage(object sender, ElapsedEventArgs e)
        {
            try
            {
                int sboOn = Process.Driver.SboScanEngines.Count(s => s.AccountStatus == eAccountStatus.Online);
                int ibetOn = Process.Driver.IbetScanEngines.Count(s => s.AccountStatus == eAccountStatus.Online);
                _manageService.PingScan(SelfInfo.Ip, sboOn, ibetOn);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                ConnectManage();
                //throw;
            }
        }

        private void ConnectManage()
        {
            try
            {
                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;
                EndpointAddress vEndPoint = new EndpointAddress(ScanData.EndpointManage);
                ChannelFactory<IBcManageService> cf = new ChannelFactory<IBcManageService>
                    (b, vEndPoint);
                _manageService = cf.CreateChannel();
                var p = _manageService.Ping();
                Process.Driver.ManageService = _manageService;
            }
            catch (Exception ex)
            {
                Logger.Error("LOI CONNECT SERVER : ", ex);
            }
        }
       
        private void StartDriver(eScanType scanType)
        {
            Process.Driver.Start(scanType);
        }
    }
}
