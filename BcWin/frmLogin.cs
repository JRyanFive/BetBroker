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
using BcWin.Common.DTO;
using BcWin.Contract;
using BcWin.Core;
using BCWin.Metadata;
using BcWin.Processor;
using EnCryptDecrypt;
using log4net;

namespace BcWin
{
    public partial class frmLogin : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmLogin));

        public frmLogin()
        {
            InitializeComponent();
            txtUsername.Text = Settings.Default.Username;
            txtPassword.Text = Settings.Default.Password;
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
            ConnectManage();
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
           // if (txtUsername.Text=="admin"&&txtPassword.Text=="123gaga123@")
            if (service.Login(txtUsername.Text, txtPassword.Text, eUserType.Bet, SelfInfo.Ip, SelfInfo.MacAddress, SelfInfo.Hostname))
            {
                Settings.Default.Username = txtUsername.Text;
                Settings.Default.Password = txtPassword.Text;
                Settings.Default.Save();
                
                SelfInfo.Username = txtUsername.Text;
                var scanType = service.RouteType(SelfInfo.Username);
                SetupBetConfigDTO betConfig = service.BetSetupConfig(SelfInfo.Username, scanType);

                DataContainer.HasLocalScan = betConfig.HasLocalScan;
                SystemConfig.TIME_GET_UPDATE_LIVE_IBET = betConfig.TimeScanLiveIbet;
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET = betConfig.TimeScanNonLiveIbet;
                SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = betConfig.TimeScanLiveSbo;
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET = betConfig.TimeScanNonLiveSbo;
                DataContainer.SboScanAccounts = betConfig.SboScanAccounts;
                DataContainer.SboScanServers = betConfig.SboScanServers;
                ClientData.EndpointRoute = betConfig.EndpointRoute;

                frmMainIbetSbo form = new frmMainIbetSbo(service);
                form.Show();
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
        }
    }
}
