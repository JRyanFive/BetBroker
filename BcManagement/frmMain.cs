using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.Objects.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using EnCryptDecrypt;
using log4net;

namespace BcManagement
{
    public partial class frmMain : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(frmMain));

        public frmMain()
        {
            InitializeComponent();
        
            var host = new ServiceHost(typeof(BcManagementService));
            try
            {
                host.Open();
            }
            catch (Exception e)
            {
                host.Abort();
                MessageBox.Show("Bug");
            }

            txtEncryp.Text = "net.tcp://XXXXXXXXXXXX:7979/bcwinmanage";
            //var aaa = CryptorEngine.Encrypt("net.tcp://103.61.138.138:7979/bcwinmanage");

            //var bbb = CryptorEngine.Decrypt(aaa);
            SaveScanConfig();
            SaveBetClientConfig();
        }

        private void btnIbetFile_Click(object sender, EventArgs e)
        {
            ImportScanAcc(1);
        }

        private void btnSboFile_Click(object sender, EventArgs e)
        {
            ImportScanAcc(2);
        }

        private void btnAddNewAcc_Click(object sender, EventArgs e)
        {
            frmUserInfo f = new frmUserInfo();
            f.Show();
        }

        private void ImportScanAcc(byte serverType)
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                //openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "TXT files|*.txt";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string filename = openFileDialog1.FileName;

                    string[] filelines = File.ReadAllLines(filename);

                    Dictionary<string, string> ibetAccdics = new Dictionary<string, string>();
                    foreach (var sboScanAcc in filelines)
                    {
                        var a = sboScanAcc.Split(new[] { "<>" }, StringSplitOptions.None);
                        ibetAccdics[a[0]] = a[1];
                    }

                    using (var context = new WinDbEntities())
                    {
                        foreach (var o in ibetAccdics)
                        {
                            context.ScanAccounts.Add(new ScanAccount()
                            {
                                ServerType = serverType,
                                Username = o.Key,
                                Password = o.Value,
                                IsBlock = false,
                                IsFree = true,
                                PassChangeVersion = 0
                            });
                        }

                        context.SaveChanges();
                    }

                    MessageBox.Show("DONE!");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("Lổi import data !");
            }
        }

        private void SaveScanConfig()
        {
            try
            {
                int timeScanLiveIbet;

                int timeScanLiveSbo;

                IList<string> ibetScanServers;

                IList<string> sboScanServers;
                bool hasFakeIpSbo;

                using (var context = new WinDbEntities())
                {
                    timeScanLiveIbet = Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "IBET_TIME_SCAN_LIVE").ValueConfig);
                    timeScanLiveSbo = Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "SBO_TIME_SCAN_LIVE").ValueConfig);

                    sboScanServers = context.SystemConfigs.Where(a => a.KeyConfig == "SBO_SCAN_LINK")
                        .Select(a => a.ValueConfig).ToList();
                    ibetScanServers = context.SystemConfigs.Where(a => a.KeyConfig == "IBET_SCAN_LINK")
                        .Select(a => a.ValueConfig).ToList();

                    var fakeIpSbo =
                      Convert.ToInt32(
                          context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "FAKE_IP_SBO").ValueConfig);
                    hasFakeIpSbo = fakeIpSbo == 1;
                }

                var sumHub = Convert.ToInt32(ConfigurationManager.AppSettings["SUMHUB"]);
                using (var context = new WinDbEntities())
                {
                    for (int i = 0; i <= sumHub; i++)
                    {
                        SetupScanConfigDTO scanCon = new SetupScanConfigDTO()
                        {
                            TimeScanLiveIbet = timeScanLiveIbet,
                            TimeScanLiveSbo = timeScanLiveSbo,
                            IbetScanServers = ibetScanServers,
                            SboScanServers = sboScanServers,
                  
                            HasFakeIpSbo = hasFakeIpSbo
                        };

                        string en = "ENDPOINT_ROUTE_SCAN_" + i;
                        scanCon.EndpointRoute =
                       context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == en).ValueConfig;

                        ProcessData.ScanConfigs.Add(scanCon);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        private void SaveBetClientConfig()
        {
            try
            {
                int timeScanLiveIbet;
                int timeScanLiveSbo;
                int timeScanNonLiveIbet;
                int timeScanNonLiveSbo;
                bool hasLocalScan;
                IList<string> sboScanServers;

                using (var context = new WinDbEntities())
                {
                    timeScanLiveIbet =
                        Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "IBET_TIME_BET_LIVE").ValueConfig);
                    timeScanLiveSbo =
                        Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "SBO_TIME_BET_LIVE").ValueConfig);

                    timeScanNonLiveIbet =
                                            Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "IBET_TIME_BET_NONLIVE").ValueConfig);
                    timeScanNonLiveSbo =
                                            Convert.ToInt32(context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "SBO_TIME_BET_NONLIVE").ValueConfig);

                    var localScan =
                        Convert.ToInt32(
                            context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == "HAS_LOCAL_SCAN").ValueConfig);

                    hasLocalScan = localScan == 1;

                    sboScanServers = context.SystemConfigs.Where(a => a.KeyConfig == "SBO_SCAN_LINK")
                        .Select(a => a.ValueConfig).ToList();
                }

                var sumHub = Convert.ToInt32(ConfigurationManager.AppSettings["SUMHUB"]);

                using (var context = new WinDbEntities())
                {
                    for (int i = 0; i <= sumHub; i++)
                    {
                        SetupBetConfigDTO s = new SetupBetConfigDTO()
                        {
                            TimeScanLiveIbet = timeScanLiveIbet,
                            TimeScanLiveSbo = timeScanLiveSbo,
                            TimeScanNonLiveIbet = timeScanNonLiveIbet,
                            TimeScanNonLiveSbo = timeScanNonLiveSbo,
                            HasLocalScan = hasLocalScan,
                            SboScanServers = sboScanServers
                        };

                        string en = "ENDPOINT_ROUTE_BET_" + i;
                        s.EndpointRoute =
                       context.SystemConfigs.FirstOrDefault(w => w.KeyConfig == en).ValueConfig;
                        ProcessData.BetClientConfigs.Add(s);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }
        }

        private void btnLoadScanConfig_Click(object sender, EventArgs e)
        {
            try
            {
                SaveScanConfig();

                MessageBox.Show("OK !");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("LOI!");
            }
        }

        private void btnLoadBetClientConfig_Click(object sender, EventArgs e)
        {
            try
            {
                SaveBetClientConfig();
                MessageBox.Show("OK !");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("LOI!");
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            txtEncryp.Text = CryptorEngine.Encrypt(txtEncryp.Text);
        }


    }
}
