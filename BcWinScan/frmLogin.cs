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
using BcWin.Processor;
using BCWin.Metadata;
using EnCryptDecrypt;
using log4net;

namespace BcWinScan
{
    public partial class frmLogin : Form
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(frmLogin));
        private IBcManageService service;
        public frmLogin()
        {
            InitializeComponent();
            textBox1.Text = Settings.Default.Username;
            textBox2.Text = Settings.Default.Password;
            txtScanType.Text = Settings.Default.ScanEndpoint.ToString();
            txtOddCompare.Text = Settings.Default.OddCompare.ToString();
            numIbetLiveTime.Value = Settings.Default.IbetTimeScan;
            numSboLiveTime.Value = Settings.Default.SboTimeScan;
            numIbetTodayTime.Value= Settings.Default.IbetTodayScan;
            numSboTodayTime.Value= Settings.Default.SboTodayScan;
            txtIpAddress.Text = Settings.Default.IpAddress;
            ckFakeIpSbo.Checked = Settings.Default.HasFakeIpSbo;
            if (!string.IsNullOrEmpty(Settings.Default.IpFakeSource))
            {
                txtIpFakeSource.Text = Settings.Default.IpFakeSource;
            }

            this.AcceptButton = button1;

            SelfInfo.MacAddress =
             (
                 from nic in NetworkInterface.GetAllNetworkInterfaces()
                 where nic.OperationalStatus == OperationalStatus.Up
                 select nic.GetPhysicalAddress().ToString()
                 ).FirstOrDefault();
            SelfInfo.Hostname = Dns.GetHostName();

            SelfInfo.Ip = Dns.GetHostByName(SelfInfo.Hostname).AddressList[0].ToString();

            ScanData.EndpointManage = CryptorEngine.Decrypt(Convert.ToString(ConfigurationManager.AppSettings["IK"]));

            ConnectManage();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (service.Login(textBox1.Text, textBox2.Text, eUserType.Scan, SelfInfo.Ip, SelfInfo.MacAddress, SelfInfo.Hostname))
            {
                Settings.Default.Username = textBox1.Text;
                Settings.Default.Password = textBox2.Text;
                Settings.Default.ScanEndpoint = int.Parse(txtScanType.Text);
                Settings.Default.OddCompare = float.Parse(txtOddCompare.Text);
                Settings.Default.IbetTimeScan = (int)numIbetLiveTime.Value;
                Settings.Default.SboTimeScan = (int)numSboLiveTime.Value;
                Settings.Default.IbetTodayScan = (int)numIbetTodayTime.Value;
                Settings.Default.SboTodayScan = (int)numSboTodayTime.Value;
                Settings.Default.IbetProxyAddress = txtProxyIbetAddress.Text;
                Settings.Default.IpAddress = txtIpAddress.Text;
                Settings.Default.HasFakeIpSbo = ckFakeIpSbo.Checked;
                Settings.Default.IpFakeSource = txtIpFakeSource.Text;
                Settings.Default.Save();

                if (!string.IsNullOrEmpty(txtIpAddress.Text))
                {
                    var ipList = txtIpAddress.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
                    ScanData.IpAddress = ipList.ToList();
                }

                if (!string.IsNullOrEmpty(txtIpFakeSource.Text))
                {
                    var ipList = txtIpFakeSource.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
                    DataContainer.SourceIpFakes = ipList.ToList();
                }

                Process.OddCompare = Settings.Default.OddCompare;
                ScanData.HasFakeIpSbo = ckFakeIpSbo.Checked;
                ScanData.ScanEndpointType = Settings.Default.ScanEndpoint;
                SelfInfo.Username = textBox1.Text;
                if (string.IsNullOrEmpty(Settings.Default.IbetProxyAddress))
                {
                    ScanData.ProxyLoginIbet = false;
                }
                else
                {
                    ScanData.ProxyLoginIbet = true;
                    ScanData.ProxyLoginIbetEndpoint = string.Concat("net.tcp://", Settings.Default.IbetProxyAddress, ":9998/bcwinsupservice");
                }

                SetupScanConfigDTO scanConfig = service.ScanConfig(SelfInfo.Username, ScanData.ScanEndpointType);
                scanConfig.TimeScanLiveIbet = Settings.Default.IbetTimeScan;
                scanConfig.TimeScanLiveSbo = Settings.Default.SboTimeScan;
                ScanAccountDTO scanAccount = service.ScanAccount(SelfInfo.Username);

                //if (scanAccount.IbetAccounts.Count == 0 || scanAccount.SboAccounts.Count == 0)
                //{
                //    var msg = string.Format("Tong tai khoang sbo {0} & ibet {1}", scanAccount.SboAccounts.Count, scanAccount.IbetAccounts.Count);
                //    MessageBox.Show(msg);
                //}
                ScanData.ScanAccount = scanAccount;
                Init(scanConfig);

                frmMain f = new frmMain(service);
                f.Show();
                this.Hide();
            }
        }

        private void ConnectManage()
        {
            try
            {
                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;
                b.MaxReceivedMessageSize = 2147483647;
                b.MaxBufferPoolSize = 2147483647;
                //101.251.121.241
                EndpointAddress vEndPoint = new EndpointAddress(ScanData.EndpointManage);
                ChannelFactory<IBcManageService> cf = new ChannelFactory<IBcManageService>
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

        private void Init(SetupScanConfigDTO scanConfig)
        {
            try
            {
                SystemConfig.TIME_GET_UPDATE_LIVE_IBET = scanConfig.TimeScanLiveIbet;
                SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = scanConfig.TimeScanLiveSbo;
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET = Settings.Default.IbetTodayScan;
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET = Settings.Default.SboTodayScan;
                DataContainer.IbetServers = scanConfig.IbetScanServers.ToArray();
                DataContainer.SboServers = scanConfig.SboScanServers.ToArray();
                ScanData.EndpointRoute = scanConfig.EndpointRoute;
                //ScanData.HasFakeIpSbo = scanConfig.HasFakeIpSbo;
            }
            catch (Exception ex)
            {
                MessageBox.Show("LOI INIT");
                Logger.Error("LOI INIT : ", ex);
            }
        }
    }
}
