using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcBetRun;
using BcBroker;
using BcWin.Common.DTO;
using BcWin.Contract;
using BcWin.Core;
using BCWin.Metadata;
using BcWin.Processor;
using EnCryptDecrypt;
using log4net;

namespace BcWin
{
    public partial class frmLoginFull : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmLoginFull));

        public frmLoginFull()
        {
            InitializeComponent();
            txtUsername.Text = Settings.Default.Username;
            txtPassword.Text = Settings.Default.Password;
            numTab.Value = Settings.Default.Tab;
            txtIpAddress.Text = Settings.Default.IpAddress;
            lblInfo.Text = "";
            this.AcceptButton = button1;

            SelfInfo.MacAddress =
            (
                from nic in NetworkInterface.GetAllNetworkInterfaces()
                where nic.OperationalStatus == OperationalStatus.Up
                select nic.GetPhysicalAddress().ToString()
                ).FirstOrDefault();
            SelfInfo.Hostname = Dns.GetHostName();

            SelfInfo.Ip = Dns.GetHostByName(SelfInfo.Hostname).AddressList[0].ToString();

            ClientData.EndpointManage = CryptorEngine.Decrypt(Convert.ToString(ConfigurationManager.AppSettings["IK"]));
            //ConnectManage();
        }

        private IBcManageService service;
        private ChannelFactory<IBcManageService> cf;
        private void ConnectManage()
        {
            try
            {
                //var aaa = cf.State;
                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;
                //101.251.121.241
                EndpointAddress vEndPoint = new EndpointAddress(ClientData.EndpointManage);
                cf = new ChannelFactory<IBcManageService>
                   (b, vEndPoint);
                service = cf.CreateChannel();

                var p = service.Ping();
            }
            catch (Exception ex)
            {
                MessageBox.Show("LOI CONNECT SERVER");
                Logger.Error("LOI CONNECT SERVER : ", ex);
            }
        }

        private int logFail = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            //var date = DateTime.Now.ToString("ddMMyy");
            if (txtUsername.Text == "admin" && txtPassword.Text == "1")
            //if (true)
            {
                //ScanInfoDTO betConfigSbo = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Sbo);
                //ScanInfoDTO betConfigIbet = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Ibet);
                Settings.Default.Username = txtUsername.Text;
                Settings.Default.Password = txtPassword.Text;
                Settings.Default.Tab = (int)numTab.Value;
                Settings.Default.IpAddress = txtIpAddress.Text;
                Settings.Default.Save();
                frmIbetSboBrokerHigher f = new frmIbetSboBrokerHigher(SelfInfo.Username);
                f.Show();
                this.Hide();
            }
            else
            {
                if (logFail >= 2)
                {
                    MessageBox.Show(
                        "Bạn đăng nhập sai quá nhiều lần, hệ thống đã tự gởi email đến người quản trị.\n Vui lòng liên lạc người quản trị để lấy mật khẩu mới!",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Exit();
                }
                else
                {
                    lblInfo.Text = "Thông tin sai! Vui lòng thử lại.";
                    logFail++;
                }
            }

            //if (service.Login(txtUsername.Text, txtPassword.Text, eUserType.Bet, SelfInfo.Ip, SelfInfo.MacAddress, SelfInfo.Hostname))
            //{
            //    Settings.Default.Username = txtUsername.Text;
            //    Settings.Default.Password = txtPassword.Text;
            //    Settings.Default.Tab = (int)numTab.Value;
            //    Settings.Default.IpAddress = txtIpAddress.Text;
            //    Settings.Default.Save();

            //    SelfInfo.Username = txtUsername.Text;
            //    if (!string.IsNullOrEmpty(txtIpAddress.Text))
            //    {
            //        var ipList = txtIpAddress.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
            //        ClientData.IpAddress = ipList.ToList();
            //    }



            //    var accType = service.GetAccType(SelfInfo.Username);
            //    if (accType == 1)
            //    {

            //        var scanType = service.RouteType(SelfInfo.Username);
            //        SetupBetConfigDTO betConfig = service.BetSetupConfig(SelfInfo.Username, scanType);

            //        DataContainer.HasLocalScan = betConfig.HasLocalScan;
            //        SystemConfig.TIME_GET_UPDATE_LIVE_IBET = betConfig.TimeScanLiveIbet;
            //        SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET = betConfig.TimeScanNonLiveIbet;
            //        SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = betConfig.TimeScanLiveSbo;
            //        SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET = betConfig.TimeScanNonLiveSbo;
            //        DataContainer.SboScanAccounts = betConfig.SboScanAccounts;
            //        DataContainer.SboScanServers = betConfig.SboScanServers;
            //        ClientData.EndpointRoute = betConfig.EndpointRoute;
            //        ClientData.Tab = Settings.Default.Tab;

            //        frmMainIbetSbo form = new frmMainIbetSbo(service);
            //        form.Show();
            //    }
            //    else if (accType == 2)
            //    {
            //        SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = 5500;
            //        SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET = 15000;
            //        ScanInfoDTO betConfig = service.ClientScanAccount(SelfInfo.Username);

            //        frmMainSboPina form = new frmMainSboPina(service, betConfig);
            //        form.Show();
            //    }
            //    else if (accType == 3)
            //    {
            //        ScanInfoDTO betConfig = service.ClientScanAccount(SelfInfo.Username);
            //        frmSboBet f = new frmSboBet(SelfInfo.Username, service, betConfig);
            //        f.Show();
            //    }
            //    else if (accType == 4)
            //    {
            //        ScanInfoDTO betConfigSbo = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Sbo);
            //        ScanInfoDTO betConfigIbet = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Ibet);
            //        frmIbetSboBroker f = new frmIbetSboBroker(SelfInfo.Username, service, betConfigSbo, betConfigIbet);
            //        f.Show();
            //    }
            //    else if (accType == 5)
            //    {
            //        frmMainPinaIsn form = new frmMainPinaIsn();
            //        form.Show();
            //    }
            //    else if (accType == 6)
            //    {
            //        ScanInfoDTO betConfigSbo = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Sbo);
            //        ScanInfoDTO betConfigIbet = service.ClientScanAccountBuyServerType(SelfInfo.Username, eServerType.Ibet);
            //        frmIbetSboBrokerHigher f = new frmIbetSboBrokerHigher(SelfInfo.Username, service, betConfigSbo, betConfigIbet);
            //        f.Show();
            //    }
            //    else
            //    {
            //        MessageBox.Show(
            //          "Lỗi khởi tạo hệ thống .\n Vui lòng liên lạc người quản trị!",
            //          "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //    this.Hide();
            //}
            //else
            //{
            //    if (logFail >= 2)
            //    {
            //        MessageBox.Show(
            //            "Bạn đăng nhập sai quá nhiều lần, hệ thống đã tự gởi email đến người quản trị.\n Vui lòng liên lạc người quản trị để lấy mật khẩu mới!",
            //            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        Application.Exit();
            //    }
            //    else
            //    {
            //        lblInfo.Text = "Thông tin sai! Vui lòng thử lại.";
            //        logFail++;
            //    }
            //}
        }
    }
}
