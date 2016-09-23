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
using DevExpress.XtraEditors;
using System.IO;
using BcWin.Common.Interface;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Utils;
using BCWin.Broker;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;
using log4net;
using Newtonsoft.Json;

namespace BcBroker.UserControls
{
    public partial class BrokerHigherControl : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BrokerHigherControl));

        Color ClickedStyleButton = Color.LightCyan;
        Color NomalStyleButton = Color.WhiteSmoke;

        IbetSboBrokerHigher broker = new IbetSboBrokerHigher();

        private int mainRate = 0;

        public BrokerHigherControl()
        {
            InitializeComponent();

            dgvLeaguesSetting.DataSource = DataContainer.LeaguesSettings;
            txtIbetScan.Text = Settings1.Default.IbetScans;
            txtSboScan.Text = Settings1.Default.SboScans;
            txtIbetServer.Text = Settings1.Default.IbetScansServer;
            txtSboServer.Text = Settings1.Default.SboScansServer;

            cbOddDev.SelectedIndex = 0;
            if (!InitConfigSaved())
            {
                cbGoalDefCheck.SelectedIndex = 2;
                cbOddDev.SelectedIndex = 0;
                ckAllLeague.Checked = true;

                dgvLeaguesSetting.Refresh();
            }

            if (!this.IsHandleCreated)
            {
                this.CreateHandle();
            }

            SetDefaultStyle();

            broker.OnWriteTextLog += broker_OnWriteTextLog;
        }

        private bool InitConfigSaved()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "AccountData");
                FileInfo savedFile = new FileInfo(path + @"\brokerdata.json");
                if (savedFile.Exists)
                {
                    using (StreamReader a = savedFile.OpenText())
                    {
                        var json = a.ReadToEnd();
                        var con = JsonConvert.DeserializeObject<ConfigModel>(json);

                        if (con != null)
                        {
                            numMaxSumScoreBuy.Value = con.TotalScoreMaxBuy;
                            txtStake.Text = con.buyAmount;
                            cbGoalDefCheck.Text = con.MinHDPOdd.ToString();
                            cbOddDev.Text = con.OddDef.ToString();
                            ckProxyIbet.Checked = con.ProxyLoginIbet;
                            txtIpProxyIbet.Text = con.ProxyLoginIbetValue;
                            ckAllLeague.Checked = con.SelectAll;
                            dgvAccountBuy.Rows.Clear();

                            foreach (var item in con.BuyAccounts)
                            {
                                dgvAccountBuy.Rows.Add(item.serverType, item.username, item.password, item.rate, "Login", "", "", "", item.domain, "", item.IsChecked, item.IP);

                                if (item.IsChecked)
                                {
                                    mainRate = Convert.ToInt32(item.rate);
                                }
                            }

                            if (dgvAccountBuy.Rows.Count > 0 && mainRate == 0)
                            {
                                dgvAccountBuy.Rows[0].Cells[10].Value = true;
                                mainRate = Convert.ToInt32(dgvAccountBuy.Rows[0].Cells[3].Value);
                            }

                            if (con.LeaguesSelected != null && con.LeaguesSelected.Count != 0)
                            {
                                var ibetLeaguesNamesChecked = con.LeaguesSelected.Select(s => s.IbetLeagueName).ToList();

                                DataContainer.LeaguesSettings.Where(l => ibetLeaguesNamesChecked.Contains(l.IbetLeagueName))
                                    .Select(s =>
                                    {
                                        s.Selected = true;
                                        return s;
                                    }).ToList();
                            }



                        }
                    }
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartProcessBroker();
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            PauseProcessBroker();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            StopProcessBroker();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                //save info
                var config = new ConfigModel
                {
                    buyAmount = txtStake.Text,// ok

                    MinHDPOdd = float.Parse(cbGoalDefCheck.Text),
                    OddDef = float.Parse(cbOddDev.Text),
                    ProxyLoginIbet = ckProxyIbet.Checked,
                    ProxyLoginIbetValue = txtIpProxyIbet.Text,
                    BuyAccounts = new List<Account>(),
                    SellAccounts = new List<Account>(),
                    LeaguesSelected = new List<LeaguesSetting>(),
                    TotalScoreMaxBuy = (int)numMaxSumScoreBuy.Value,
                    SelectAll = ckAllLeague.Checked,
                };

                if (ckAllLeague.Checked == false)
                {
                    foreach (DataGridViewRow item in dgvLeaguesSetting.Rows)
                    {
                        if ((bool)item.Cells[0].Value == true)
                        {
                            config.LeaguesSelected.Add(new LeaguesSetting { IbetLeagueName = item.Cells[1].Value.ToString() });

                        }

                    }
                }

                foreach (DataGridViewRow item in dgvAccountBuy.Rows)
                {
                    if (item.Cells[1].Value != null && item.Cells[2].Value != null
                        && !String.IsNullOrEmpty(item.Cells[1].Value.ToString())
                        && !String.IsNullOrEmpty(item.Cells[2].Value.ToString())
                        )
                    {
                        bool check;
                        if (item.Cells[10].Value == null)
                        {
                            check = false;
                        }
                        else
                        {
                            check = (bool)item.Cells[10].Value;
                        }

                        config.BuyAccounts.Add(new Account
                        {
                            serverType = item.Cells[0].Value.ToString(),
                            username = item.Cells[1].Value.ToString(),
                            password = item.Cells[2].Value.ToString(),
                            rate = item.Cells[3].Value.ToString(),
                            domain = item.Cells[8].Value.ToString(),
                            IP = item.Cells[11].Value != null ? item.Cells[11].Value.ToString() : "",
                            IsChecked = check,
                        });

                    }
                }

                string path = Path.Combine(Application.StartupPath, "AccountData");
                FileInfo saveFile = new FileInfo(path + @"\brokerdata.json");
                saveFile.Directory.Create();

                using (StreamWriter writer = saveFile.CreateText())
                {
                    writer.Write(JsonConvert.SerializeObject(config));
                }
                UpdateLogText("Lưu thông tin thành công !", eLogTextType.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                UpdateLogText("Lỗi lưu thong tin.", eLogTextType.Error);
            }

        }

        private void EventGroupAccClick(object sender, MouseEventArgs e)
        {
            //var control = (GroupControl)sender;

            //if (e.Location.X < 45 && e.Location.Y < 40)
            //{
            //    //
            //    control.Text = e.Location.X + " -- " + e.Location.Y;
            //}
            //else
            //{
            //    control.Text = Guid.NewGuid().ToString();
            //}
        }

        private void btnAddAcc_Click(object sender, EventArgs e)
        {
            var btn = sender as SimpleButton;

            frmInfoAcc f = new frmInfoAcc();
            f.ShowDialog();
            if (!string.IsNullOrEmpty(f.ServerType))
            {
                DataGridView dgv = dgvAccountBuy;

                dgv.Rows.Add(f.ServerType, f.Username, f.Password, f.Rate, "Login", null, null, null, f.Domain, null, null, f.IpFake);

                if (dgvAccountBuy.Rows.Count == 1)
                {
                    dgv.Rows[0].Cells[10].Value = true;
                    mainRate = Convert.ToInt32(dgvAccountBuy.Rows[0].Cells[3].Value);
                }
            }
        }

        private void ckAllLeague_CheckedChanged(object sender, EventArgs e)
        {
            if (ckAllLeague.Checked)
            {
                DataContainer.LeaguesSettings.Select(s =>
                {
                    s.Selected = true;
                    return s;
                }).ToList();
                dgvLeaguesSetting.Refresh();
                dgvLeaguesSetting.ReadOnly = true;
            }
            else
            {
                DataContainer.LeaguesSettings.Select(s =>
                {
                    s.Selected = false;
                    return s;
                }).ToList();
                dgvLeaguesSetting.Refresh();
                dgvLeaguesSetting.ReadOnly = false;
            }
        }

        private void dgvAccount_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView dgv = (DataGridView)sender;

            bool isBuy = dgv.Name == "dgvAccountBuy";

            DataGridViewRow selectedRow = dgv.Rows[e.RowIndex];
            var serverType = selectedRow.Cells[0].Value;
            var user = selectedRow.Cells[1].Value;
            var pass = selectedRow.Cells[2].Value;
            var rate = selectedRow.Cells[3].Value;
            var status = selectedRow.Cells[4].Value;
            var domain = selectedRow.Cells[8].Value;
            var ip = selectedRow.Cells[11].Value;

            //dang nhap
            if (e.ColumnIndex == dgv.Columns[4].Index)
            {
                if (status.ToString() == "Login")
                {
                    IEngineBroker engine = null;
                    if (serverType.ToString() == "IBET")
                    {
                        engine = InitLoginIbet(domain.ToString(), user.ToString(), pass.ToString(),
                            ip, Convert.ToInt32(rate));
                    }
                    else if (serverType.ToString() == "SBO")
                    {
                        engine = InitLoginSbo(domain.ToString(), user.ToString(), pass.ToString(),
                            ip, Convert.ToInt32(rate));
                    }

                    if (engine != null)
                    {
                        broker.AddNewBetEngine(engine, isBuy);
                        selectedRow.Cells[9].Value = engine.EngineId;

                        selectedRow.Cells[1] = new DataGridViewLinkCell();
                        selectedRow.Cells[1].Value = user.ToString();
                        selectedRow.Cells[4].Style.BackColor = Color.SteelBlue;
                        selectedRow.Cells[4].Value = "Logout";
                    }
                }
                else
                {
                    ////Logout
                    var engineId = selectedRow.Cells[9].Value.ToString();
                    broker.RemoveBetEngine(engineId, isBuy);

                    UpdateLogText("Thoát tài khoản thành công!");
                    selectedRow.Cells[1] = new DataGridViewTextBoxCell();
                    selectedRow.Cells[1].Value = user.ToString();
                    selectedRow.Cells[4].Style.BackColor = Color.White;
                    selectedRow.Cells[4].Value = "Login";
                }

            }
            //Delete row
            else if (e.ColumnIndex == dgv.Columns[7].Index)
            {
                if (status.ToString() == "Login")
                {
                    dgv.Rows.RemoveAt(e.RowIndex);
                    UpdateLogText("Xóa tài khoản thành công!");
                }
                else
                {
                    UpdateLogText("Không thể thao tác trên tài khoản đang online!", eLogTextType.Error);
                }
            }
            //Edit row
            else if (e.ColumnIndex == dgv.Columns[6].Index)
            {
                if (status.ToString() == "Login")
                {
                    frmInfoAcc f = new frmInfoAcc(serverType.ToString(), user.ToString(), pass.ToString(), domain.ToString(), rate.ToString(), ip.ToString());
                    f.ShowDialog();
                    if (!string.IsNullOrEmpty(f.ServerType))
                    {
                        selectedRow.Cells[0].Value = f.ServerType;
                        selectedRow.Cells[1].Value = f.Username;
                        selectedRow.Cells[2].Value = f.Password;
                        selectedRow.Cells[3].Value = f.Rate;
                        selectedRow.Cells[8].Value = f.Domain;
                        selectedRow.Cells[11].Value = f.IpFake;
                    }
                }
                else
                {
                    UpdateLogText("Không thể thao tác trên tài khoản đang online!", eLogTextType.Error);
                }
            }
            else if (e.ColumnIndex == dgv.Columns[1].Index)
            {
                if (status.ToString() == "Logout")
                {
                    var engineId = selectedRow.Cells[9].Value.ToString();
                    IEngineBroker engine = broker.BetBuyEngines.First(b => b.EngineId == engineId);


                    frmStatement f = new frmStatement(engine, user.ToString());
                    f.Show();
                }
            }
            else if (e.ColumnIndex == dgv.Columns[5].Index)
            {
                if (status.ToString() == "Logout")
                {
                    var engineId = selectedRow.Cells[9].Value.ToString();
                    IEngineBroker engine = broker.BetBuyEngines.First(b => b.EngineId == engineId);

                    var creadit = engine.UpdateAvailabeCredit();
                    UpdateLogText(string.Format("Hạn mức khả dụng tài khoản [{0}] là {1}", user, creadit), eLogTextType.Warning);
                }
                else
                {
                    UpdateLogText("Lỗi tài khoản chưa được đăng nhập.", eLogTextType.Error);
                }
            }
            //Chon ty gia chinh
            else if (e.ColumnIndex == dgv.Columns[10].Index)
            {
                foreach (DataGridViewRow dr in dgvAccountBuy.Rows)
                {
                    dr.Cells[10].Value = false;
                }

                selectedRow.Cells[10].Value = true;
                mainRate = Convert.ToInt32(selectedRow.Cells[3].Value);
            }
        }


        private void btnClearLog_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                txtLog.Text = "";
                txtLog.ScrollToCaret();
            }));
        }

        public void StartProcessBroker()
        {
            Random r = new Random();

            if (!string.IsNullOrEmpty(txtIbetScan.Text)&& !string.IsNullOrEmpty(txtIbetServer.Text) 
                && broker.IbetEn.AccountStatus == eAccountStatus.Offline)
            {
                BrokerData.IbetAccounts = new List<ScanAccountInfoDTO>();
                var accList = txtIbetScan.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
                foreach (var s in accList)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        var a = s.Split(new string[] { "<>" }, StringSplitOptions.None);
                        if (a.Length == 2)
                        {
                            BrokerData.IbetAccounts.Add(new ScanAccountInfoDTO() { Username = a[0], Password = a[1] });
                        }
                    }
                }

                var ibetServerList = txtIbetServer.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();
                ibetServerList.RemoveAll(string.IsNullOrEmpty);
                BrokerData.IbetScanServers = ibetServerList;
                Settings1.Default.IbetScans = txtIbetScan.Text;
                Settings1.Default.IbetScansServer = txtIbetServer.Text;
                Settings1.Default.Save();
                var ibetAcc = BrokerData.IbetAccounts[r.Next(BrokerData.IbetAccounts.Count)];
                var ibetServer = BrokerData.IbetScanServers[r.Next(BrokerData.IbetScanServers.Count)];
                BrokerData.ScanIbetUsername = ibetAcc.Username;
                BrokerData.ScanIbetPassword = ibetAcc.Password;
                BrokerData.ScanIbetDomain = ibetServer;
            }
            else
            {
                if (broker.IbetEn.AccountStatus == eAccountStatus.Offline)
                {
                    MessageBox.Show("Không xác nhận được tài khoản quét IBET");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(txtSboScan.Text) && !string.IsNullOrEmpty(txtSboServer.Text) && broker.SboEn.AccountStatus == eAccountStatus.Offline)
            {
                BrokerData.SboAccounts = new List<ScanAccountInfoDTO>();
                var accList = txtSboScan.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None);
                foreach (var s in accList)
                {
                    if (!string.IsNullOrEmpty(s))
                    {
                        var a = s.Split(new string[] { "<>" }, StringSplitOptions.None);
                        if (a.Length == 2)
                        {
                            BrokerData.SboAccounts.Add(new ScanAccountInfoDTO() { Username = a[0], Password = a[1] });
                        }
                    }
                }

                var ibetServerList = txtSboServer.Text.Split(new string[] { Environment.NewLine, "\n" }, StringSplitOptions.None).ToList();
                ibetServerList.RemoveAll(string.IsNullOrEmpty);
                BrokerData.SboScanServers = ibetServerList;
                Settings1.Default.SboScans = txtSboScan.Text;
                Settings1.Default.SboScansServer = txtSboServer.Text;
                Settings1.Default.Save();
                var ibetAcc = BrokerData.SboAccounts[r.Next(BrokerData.SboAccounts.Count)];
                var ibetServer = BrokerData.SboScanServers[r.Next(BrokerData.SboScanServers.Count)];
                BrokerData.ScanSboUsername = ibetAcc.Username;
                BrokerData.ScanSboPassword = ibetAcc.Password;
                BrokerData.ScanSboDomain = ibetServer;
            }
            else
            {
                if (broker.SboEn.AccountStatus == eAccountStatus.Offline)
                {
                    MessageBox.Show("Không xác nhận được tài khoản quét IBET");
                    return;
                }
            }

            SetProcessingStyle();

            if (broker.IbetEn.AccountStatus == eAccountStatus.Offline)
            {
                if (ckProxyIbet.Checked)
                {
                    string endpointProxy = string.Concat("net.tcp://", txtIpProxyIbet.Text, ":9998/bcwinsupservice");
                    broker.IbetEn.ProxyLogin = true;
                    broker.IbetEn.ProxyEndpoint = endpointProxy;
                    broker.IbetEnScan.ProxyLogin = true;
                    broker.IbetEnScan.ProxyEndpoint = endpointProxy;
                }

                if (broker.IbetEn.Login(BrokerData.ScanIbetDomain, BrokerData.ScanIbetUsername, BrokerData.ScanIbetPassword))
                {
                    foreach (var acc in BrokerData.IbetAccounts)
                    {
                        if (acc.Username == BrokerData.ScanIbetUsername)
                        {
                            continue;
                        }
                        broker.IbetEnScan.Login(BrokerData.ScanIbetDomain, acc.Username, acc.Password);

                        if (broker.IbetEnScan.AccountStatus == eAccountStatus.Online)
                        {
                            break;
                        }
                    }

                    UpdateLogText("Kết nối server ibet thành công.");
                }
                else
                {
                    var ibetAcc = BrokerData.IbetAccounts[r.Next(BrokerData.IbetAccounts.Count)];
                    var ibetServer = BrokerData.IbetScanServers[r.Next(BrokerData.IbetScanServers.Count)];
                    BrokerData.ScanIbetUsername = ibetAcc.Username;
                    BrokerData.ScanIbetPassword = ibetAcc.Password;
                    BrokerData.ScanIbetDomain = ibetServer;

                    UpdateLogText("Kết nối server ibet không thành công.", eLogTextType.Error);
                }
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Offline)
            {
                if (broker.SboEn.Login(BrokerData.ScanSboDomain, BrokerData.ScanSboUsername, BrokerData.ScanSboPassword))
                {
                    //if (broker.SboEnScan.Login(BrokerData.ScanSboDomain, "msn99aa001", "123@@Googlee"))
                    //{
                    //    broker.SboEnScan.Login(BrokerData.ScanSboDomain, "msn99aa001", "123@@Googlee");
                    //    if (broker.SboEnScan.AccountStatus == eAccountStatus.Offline)
                    //    {
                    //        throw new Exception();
                    //    }
                    //}

                    foreach (var scanAccountInfoDto in BrokerData.SboAccounts)
                    {
                        if (scanAccountInfoDto.Username == BrokerData.ScanSboUsername)
                        {
                            continue;
                        }
                        broker.SboEnScan.Login(BrokerData.ScanSboDomain, scanAccountInfoDto.Username, scanAccountInfoDto.Password);

                        if (broker.SboEnScan.AccountStatus == eAccountStatus.Online)
                        {
                            break;
                        }
                    }

                    UpdateLogText("Kết nối server sbo thành công.");
                }
                else
                {
                    var sboAcc = BrokerData.SboAccounts[r.Next(BrokerData.SboAccounts.Count)];
                    var sboServer = BrokerData.SboScanServers[r.Next(BrokerData.SboScanServers.Count)];
                    BrokerData.ScanSboUsername = sboAcc.Username;
                    BrokerData.ScanSboPassword = sboAcc.Password;
                    BrokerData.ScanSboDomain = sboServer;

                    UpdateLogText("Kết nối server sbo không thành công.", eLogTextType.Error);
                }
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Online
                        && broker.IbetEn.AccountStatus == eAccountStatus.Online)
            {
                broker.HasCheckAllLeagues = ckAllLeague.Checked;

                if (!ckAllLeague.Checked)
                {
                    broker.FilterLeagues = DataContainer.LeaguesSettings.Where(s => s.Selected).Select(ss => ss.IbetLeagueName).ToList();
                }
                broker.PickUnder = ckPickUnder.Checked;
                broker.BuySbo = rdoBuySbo.Checked;
                broker.SumScoreMaxBuy = (int)numMaxSumScoreBuy.Value;
                broker.GoalDefCheck = float.Parse(cbGoalDefCheck.Text);
                broker.OddDevCheck = (float)int.Parse(cbOddDev.Text) / 100;
                broker.MainRate = mainRate;
                if (!string.IsNullOrEmpty(txtStake.Text))
                {
                    broker.StakeBuy = Convert.ToInt32(txtStake.Text);
                }

                broker.StartScanBroker();
                if (broker.Status == eServiceStatus.Started)
                {
                    SetStartClickStyle();
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

        private IbetEngine InitLoginIbet(string domain, string username, string pass, object ipFake, int rate)
        {
            UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản IBET [{0}]......", username), eLogTextType.Warning);

            IbetEngine ibetEngine = new IbetEngine();
            if (ckProxyIbet.Checked)
            {
                ibetEngine.ProxyLogin = true;
                ibetEngine.ProxyEndpoint = string.Concat("net.tcp://", txtIpProxyIbet.Text, ":9998/bcwinsupservice");
            }

            if (ipFake != null)
            {
                if (!string.IsNullOrEmpty(ipFake.ToString()))
                {
                    ibetEngine.IpFake = ipFake.ToString();
                }
            }

            if (ibetEngine.Login(domain, username, pass))
            {
                ibetEngine.EngineId = Guid.NewGuid().ToString();
                ibetEngine.ExchangeRate = rate * 10;
                ibetEngine.StartScanEngine(eScanType.Live);
                UpdateLogText(string.Format("Đăng nhập [{0}] thành công! Số dư khả dụng là {1}", username, ibetEngine.UpdateAvailabeCredit()));
                return ibetEngine;
            }
            else
            {
                UpdateLogText(string.Format("Đăng nhập ibet [{0}] không thành công!", username), eLogTextType.Error);
                return null;
            }
        }

        private SboEngine InitLoginSbo(string domain, string username, string pass, object ipFake, int rate)
        {
            UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản SBO [{0}]......", username), eLogTextType.Warning);

            SboEngine sboEngine = new SboEngine();
            if (ipFake != null)
            {
                if (!string.IsNullOrEmpty(ipFake.ToString()))
                {
                    sboEngine.IpFake = ipFake.ToString();
                }
            }

            if (sboEngine.Login(domain, username, pass))
            {
                sboEngine.EngineId = Guid.NewGuid().ToString();
                sboEngine.ExchangeRate = rate * 10;
                sboEngine.StartScanEngine(eScanType.Live);
                UpdateLogText(string.Format("Đăng nhập [{0}] thành công! Số dư khả dụng là {1}", username, sboEngine.UpdateAvailabeCredit()));
                return sboEngine;
            }
            else
            {
                UpdateLogText(string.Format("Đăng nhập sbo [{0}] không thành công!", username), eLogTextType.Error);
                return null;
            }
        }

        private void broker_OnWriteTextLog(string logMsg, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow)
        {
            //switch (status)
            //{
            //    case eBrokerStatus.Buy:
            //        SumBuy++;
            //        break;
            //    case eBrokerStatus.Sell:
            //        SumSell++;
            //        break;
            //    case eBrokerStatus.GoodTrans:
            //        SumSell++;
            //        SumGoodTras++;
            //        break;
            //    case eBrokerStatus.BadTrans:
            //        SumSell++;
            //        SumBadTrans++;
            //        break;
            //    case eBrokerStatus.MissTrans:
            //        SumMissTrans++;
            //        break;
            //}

            //if (status != eBrokerStatus.Unknow)
            //{
            //    BindSummary();
            //}

            UpdateLogText(logMsg, type, status);
        }

        private void SetProcessingStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStart.Enabled = false;
                btnStart.BackColor = ClickedStyleButton;

                btnPause.Enabled = false;
                btnPause.BackColor = ClickedStyleButton;
                btnStop.Enabled = false;
                btnStop.BackColor = ClickedStyleButton;

                pbLoading.Visible = true;
                lblStatus.Text = "processing";
                lblStatus.Visible = true;
            });
        }

        private void SetDefaultStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStart.Enabled = true;
                btnStart.BackColor = ClickedStyleButton;

                btnPause.Enabled = false;
                btnPause.BackColor = NomalStyleButton;
                btnStop.Enabled = false;
                btnStop.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Visible = false;
            });
        }

        private void SetStartClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnStart.Enabled = false;
                btnStart.BackColor = ClickedStyleButton;

                btnPause.Enabled = true;
                btnPause.BackColor = NomalStyleButton;
                btnStop.Enabled = true;
                btnStop.BackColor = NomalStyleButton;

                pbLoading.Visible = true;
                lblStatus.Text = "Running";
                lblStatus.Visible = true;
            });
        }

        private void SetPauseClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnPause.Enabled = false;
                btnPause.BackColor = ClickedStyleButton;

                btnStart.Enabled = true;
                btnStart.BackColor = NomalStyleButton;

                btnStop.Enabled = true;
                btnStop.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Text = "Pause";
            });
        }

        private void SetFourceStopClickStyle()
        {
            this.Invoke((MethodInvoker)delegate ()
            {
                btnPause.Enabled = false;
                btnPause.BackColor = ClickedStyleButton;

                btnStart.Enabled = false;
                btnStart.BackColor = ClickedStyleButton;

                btnStop.Enabled = true;
                btnStop.BackColor = NomalStyleButton;

                pbLoading.Visible = false;
                lblStatus.Text = "Stopped";
            });
        }

        private void UpdateLogText(string log, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow)
        {
            Color c;
            switch (type)
            {
                case eLogTextType.Highlight:
                    c = Color.SteelBlue;
                    break;
                case eLogTextType.Error:
                    c = Color.Crimson;
                    break;
                case eLogTextType.Warning:
                    if (status == eBrokerStatus.Buy)
                    {
                        c = Color.MediumBlue;
                        break;
                    }
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
            if (status == eBrokerStatus.GoalFailBet)
            {
                DataContainer.AlarmSound.PlaySync();
            }
            if (status != eBrokerStatus.Unknow)
            {
                Task.Run(() =>
                {
                    this.Invoke((MethodInvoker)(() =>
                    {
                        txtLogBet.AppendText(Environment.NewLine);
                        var logBet = string.Concat(DateTime.Now, ">> ", log);
                        txtLogBet.Select(txtLogBet.TextLength, 0);
                        txtLogBet.SelectionColor = c;
                        txtLogBet.AppendText(logBet);
                        txtLogBet.ScrollToCaret();
                    }));
                });
            }

            Task.Run(() =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    txtLog.AppendText(Environment.NewLine);
                    var logScan = string.Concat(DateTime.Now, ">> ", log);
                    txtLog.Select(txtLog.TextLength, 0);
                    txtLog.SelectionColor = c;
                    txtLog.AppendText(logScan);
                    txtLog.ScrollToCaret();
                }));
            });
        }

        private void ckProxyIbet_CheckedChanged(object sender, EventArgs e)
        {
            txtIpProxyIbet.Enabled = ckProxyIbet.Checked;
        }
    }
}
