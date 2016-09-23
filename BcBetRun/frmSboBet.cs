using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Core;
using BcWin.Processor;
using BCWin.Engine.Sbo;
using log4net;
using System.IO;
using BcWin.Core.Utils;
using Newtonsoft.Json;

namespace BcBetRun
{
    public partial class frmSboBet : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(frmSboBet));

        SboEngine sboScan = new SboEngine();
        List<SboEngine> buyAccEngines = new List<SboEngine>();
        List<SboEngine> sellAccEngines = new List<SboEngine>();
        List<string> accSboBet = new List<string>();
        private int buyExchange;
        private int sellExchange;
        private int maxStakeCompare;
        eScanType scanType { get; set; }
        private IBcManageService _manageService;
        private System.Timers.Timer timerPing;
        public frmSboBet()
        {
            this.Text = "BetRunDown - © 2015";
        }

        public frmSboBet(string user, IBcManageService service, ScanInfoDTO betConfig, bool debug = false)
        {
            InitializeComponent();
            this.btnLoginBuyGroup.Image = Properties.Resources.LoginGroup;
            this.btnLoginSellGroup.Image = Properties.Resources.LoginGroup;
            this.btnSaveExchange.Image = BcBetRun.Properties.Resources.SaveBmp;
            this.btnSaveExchange2.Image = BcBetRun.Properties.Resources.SaveBmp;

            cboMarket.SelectedIndex = 0;

            InitConfigSaved();

            this.Text = string.Concat("BetRunDown - ", user, " © Copyright 2015");

            CoreProcessor.InitConfig();

            sboScan.UpdateLiveDataChange += sboScan_UpdateLiveDataChange;
            sboScan.UpdateNonLiveDataChange += sboScan_UpdateLiveDataChange;
            sboScan.OnExceptionEvent += sboScan_OnExceptionEvent;

            if (!debug)
            {
                _manageService = service;

                DataContainer.SboScanServers = betConfig.ScanServers;

                _manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, 10);

                timerPing = new System.Timers.Timer(60000);
                timerPing.Elapsed += PingManage;
                timerPing.Start();
            }
            else
            {
                DataContainer.SboScanServers = new List<string>()
                {
                    "http://www.currybread.com/",
                    "http://www.beer000.com/",
                    "http://www.beer777.com/",
                    "http://www.harybox.com/",
                    "http://www.pic5678.com/"
                };
            }
        }

        private void InitConfigSaved()
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "AccountData");
                FileInfo savedFile = new FileInfo(path + @"\betrundown.json");
                if (savedFile.Exists)
                {
                    StreamReader a = savedFile.OpenText();
                    var json = a.ReadToEnd();
                    var con = JsonConvert.DeserializeObject<ConfigModel>(json);

                    if (con != null)
                    {

                        txtUserscan.Text = con.userScan;
                        txtPassScan.Text = con.userPassScan;
                        cboMarket.SelectedIndex = con.cboMarketSelectedIndex;
                        numMaxStake.Value = con.numMaxStake;
                        txtBuyEx.Text = con.txtBuyEx;
                        txtSellEx.Text = con.txtSellEx;
                        foreach (var item in con.ListBuy)
                        {
                            dgvUserBuy.Rows.Add(item.username, item.password);
                        }

                        foreach (var item in con.ListSell)
                        {
                            dgvUserSell.Rows.Add(item.username, item.password);
                        }


                    }

                }
            }
            catch (Exception)
            {


            }

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            GC.SuppressFinalize(this);
            Application.Exit();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            //5c8c3e5a7fb0ef40406cc6f5962be464
            //if (string.IsNullOrEmpty(txtSellEx.Text) || string.IsNullOrEmpty(txtBuyEx.Text))
            //{
            //    MessageBox.Show("Nhập thiếu thông tin!");
            //    return;
            //}

            UpdateLogText("Đang kết nối server....................", eLogTextType.Warning);
            var key = _manageService.GetKey();
            if (key != "5c8c3e5a7fb0ef40406cc6f5962be464")
            {
                UpdateLogText("Lỗi parse data. Kết nối server thất bai. Không thể khởi động tài khoản scan.", eLogTextType.Error);
                return;
            }

            Task.Run(() =>
            {
                this.Invoke((MethodInvoker)(() =>
                {
                    scanType = (eScanType)cboMarket.SelectedIndex;
                    Random r = new Random();
                    var url = DataContainer.SboScanServers[r.Next(DataContainer.SboScanServers.Count())];
                    sboScan.IpFake = txtIpFakeScan.Text;
                    sboScan.Login(url, txtUserscan.Text.Trim(), txtPassScan.Text.Trim());
                    if (sboScan.AccountStatus == eAccountStatus.Online)
                    {
                        sboScan.StartBetRunEngine(scanType, false);
                        buyExchange = int.Parse(txtBuyEx.Text);
                        sellExchange = int.Parse(txtSellEx.Text);
                        txtUserscan.Enabled = false;
                        txtPassScan.Enabled = false;
                        btnStart.Enabled = false;
                        //txtBuyEx.Enabled = false;
                        maxStakeCompare = (int)numMaxStake.Value;
                        numMaxStake.Enabled = false;
                        cboMarket.Enabled = false;
                        txtIpFakeScan.Enabled = false;
                        //txtSellEx.Enabled = false;
                        //dgvUserBuy.Enabled = true;
                        //dgvUserSell.Enabled = true;

                        UpdateLogText("Kết nối server thành công!");
                    }
                    else
                    {
                        UpdateLogText("Kết nối server thất bai.", eLogTextType.Error);
                    }
                }));
            });
        }

        private void btnTrade_Click(object sender, EventArgs e)
        {
            //List<int> listA= new List<int>(){1,2,3,4,5};
            //List<int> listB = new List<int>() { 10, 20, 30, 40, 50 };
            //if (ckSellAfter.Checked)
            //{
            //    UpdateLogText(string.Format("{0} aa", DateTime.Now));
            //    return;
            //}
            //int timeWaitSell = (int) numSellAfterSecond.Value;
            //Task.Factory.StartNew(() =>
            //{
            //    var buySt2atus = Parallel.ForEach(listA, (engine) =>
            //    {
            //        UpdateLogText(string.Format("{0} value {1}", DateTime.Now, engine));
            //    }).IsCompleted;

            //});

            //Task.Factory.StartNew(() =>
            //{
            //    //Task.Delay(timeWaitSell);
            //    Thread.Sleep(timeWaitSell);
            //    var buySt2atus = Parallel.ForEach(listB, (engine) =>
            //    {
            //        UpdateLogText(string.Format("{0} value {1}", DateTime.Now, engine));
            //    }).IsCompleted;

            //});
            // return;

            if (dgvLogScan.SelectedCells.Count > 0)
            {

                bool hasSellAfter = ckSellAfter.Checked;
                int timeWaitSell = (int)numSellAfterSecond.Value;

                if (hasSellAfter && string.IsNullOrEmpty(txtStake.Text))
                {
                    UpdateLogText("Lỗi chưa nhập tổng tiền mua.", eLogTextType.Error);
                    return;
                }

                int selectedrowindex = dgvLogScan.SelectedCells[0].RowIndex;

                DataGridViewRow selectedRow = dgvLogScan.Rows[selectedrowindex];
                string value1 = selectedRow.Cells[0].Value.ToString();
                bool isLive = value1 == "LIVE";
                string homeTeamName = selectedRow.Cells[1].Value.ToString();
                string awayTeamName = selectedRow.Cells[2].Value.ToString();
                eOddType oddType = (eOddType)selectedRow.Cells[3].Value;
                float odd = (float)selectedRow.Cells[4].Value;

                eBetType buyBetType = rdBetHome.Checked ? eBetType.Home : eBetType.Away;
                eBetType sellBetType = buyBetType == eBetType.Home ? eBetType.Away : eBetType.Home;
                int stake;
                int.TryParse(txtStake.Text, out stake);
                if (stake == 0)
                {
                    stake = -1;
                }

                if (hasSellAfter)
                {
                    UpdateLogText("Bắt đầu giao dịch!", eLogTextType.Warning);

                    var buyOnlineEngine = buyAccEngines.Where(b => b.AccountStatus == eAccountStatus.Online).ToList();

                    int sumBuyStake = 0;
                    int stakeBuy = (int)Math.Round((double)(stake / buyOnlineEngine.Count), 0);
                    Task.Factory.StartNew(() =>
                    {
                        var buyStatus = Parallel.ForEach(buyOnlineEngine, (engine) =>
                        {
                            int realStake;
                            if (engine.ProcessBetAnyway(stakeBuy, homeTeamName, awayTeamName, odd, oddType, buyBetType, isLive, out realStake))
                            {
                                sumBuyStake += realStake;
                                UpdateLogText(string.Format("MUA {0} điểm vào tài khoản [{1}] trận {2} - {3} thành công!",
                             realStake, engine.UserName, homeTeamName, awayTeamName), eLogTextType.Highlight);
                            }
                            else
                            {
                                UpdateLogText(string.Format("MUA vào tài khoản [{0}] trận {1} - {2} không thành công!",
                                    engine.UserName, homeTeamName, awayTeamName), eLogTextType.Error);
                            }
                        }).IsCompleted;

                        UpdateLogText(string.Format("TỔNG MUA: {0}", sumBuyStake), eLogTextType.Highlight);
                    });

                    var sellOnlineEngine = sellAccEngines.Where(b => b.AccountStatus == eAccountStatus.Online).ToList();

                    var sumSell = ConvertExchange(stake, buyExchange, sellExchange);
                    var sellStake = (int)Math.Round((double)(sumSell / sellOnlineEngine.Count), 0);
                    UpdateLogText(string.Format("CHUẨN BỊ BÁN: {0}", sumSell), eLogTextType.Highlight);

                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(timeWaitSell);
                        int sumSellStake = 0;
                        var sellStatus = Parallel.ForEach(sellOnlineEngine, (engine) =>
                        {
                            int realStake;
                            if (engine.ProcessBetAnyway(sellStake, homeTeamName, awayTeamName, odd, oddType, sellBetType,
                                isLive, out realStake))
                            {
                                sumSellStake += realStake;
                                UpdateLogText(
                                    string.Format("BÁN {0} điểm vào tài khoản [{1}] trận {2} - {3} thành công!",
                                        realStake, engine.UserName, homeTeamName, awayTeamName), eLogTextType.Highlight);
                            }
                            else
                            {
                                UpdateLogText(string.Format("BÁN vào tài khoản [{0}] trận {1} - {2} không thành công!",
                                    engine.UserName, homeTeamName, awayTeamName), eLogTextType.Error);
                            }
                        }).IsCompleted;
                        UpdateLogText(string.Format("TỔNG BÁN: {0}", sumSellStake), eLogTextType.Highlight);
                    });
                }
                else
                {
                    Task.Run(() =>
                    {
                        UpdateLogText("Bắt đầu giao dịch!", eLogTextType.Warning);

                        var buyOnlineEngine = buyAccEngines.Where(b => b.AccountStatus == eAccountStatus.Online).ToList();

                        int sumBuyStake = 0;
                        var buyStatus = Parallel.ForEach(buyOnlineEngine, (engine) =>
                        {
                            int realStake;
                            if (engine.ProcessBetAnyway(stake, homeTeamName, awayTeamName, odd, oddType, buyBetType, isLive, out realStake))
                            {
                                sumBuyStake += realStake;
                                UpdateLogText(string.Format("MUA {0} điểm vào tài khoản [{1}] trận {2} - {3} thành công!",
                             realStake, engine.UserName, homeTeamName, awayTeamName), eLogTextType.Highlight);
                            }
                            else
                            {
                                UpdateLogText(string.Format("MUA vào tài khoản [{0}] trận {1} - {2} không thành công!",
                                    engine.UserName, homeTeamName, awayTeamName), eLogTextType.Error);
                            }
                        }).IsCompleted;

                        UpdateLogText(string.Format("TỔNG MUA: {0}", sumBuyStake), eLogTextType.Highlight);

                        if (sumBuyStake == 0)
                        {
                            UpdateLogText("Hoàn thành giao dịch!", eLogTextType.Warning);
                            return;
                        }

                        var sellOnlineEngine = sellAccEngines.Where(b => b.AccountStatus == eAccountStatus.Online).ToList();
                        var sellStake = (int)Math.Round((double)(ConvertExchange(sumBuyStake, buyExchange, sellExchange) / sellOnlineEngine.Count), 0);
                        int sumSellStake = 0;
                        var sellStatus = Parallel.ForEach(sellOnlineEngine, (engine) =>
                        {
                            int realStake;
                            if (engine.ProcessBetAnyway(sellStake, homeTeamName, awayTeamName, odd, oddType, sellBetType, isLive, out realStake))
                            {
                                sumSellStake += realStake;
                                UpdateLogText(string.Format("BÁN {0} điểm vào tài khoản [{1}] trận {2} - {3} thành công!",
                             realStake, engine.UserName, homeTeamName, awayTeamName), eLogTextType.Highlight);
                            }
                            else
                            {
                                UpdateLogText(string.Format("BÁN vào tài khoản [{0}] trận {1} - {2} không thành công!",
                                    engine.UserName, homeTeamName, awayTeamName), eLogTextType.Error);
                            }
                        }).IsCompleted;
                        UpdateLogText(string.Format("TỔNG BÁN: {0}", sumSellStake), eLogTextType.Highlight);
                        UpdateLogText("Hoàn thành giao dịch!", eLogTextType.Warning);
                    });
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                //save info
                var config = new ConfigModel
                {
                    userScan = txtUserscan.Text,
                    userPassScan = txtPassScan.Text,
                    cboMarketSelectedIndex = cboMarket.SelectedIndex,
                    numMaxStake = (int)numMaxStake.Value,
                    txtBuyEx = txtBuyEx.Text,
                    txtSellEx = txtSellEx.Text,
                    ListBuy = new List<Account>(),
                    ListSell = new List<Account>(),

                };

                foreach (DataGridViewRow item in dgvUserBuy.Rows)
                {
                    if (item.Cells[0].Value != null && item.Cells[1].Value != null
                        && !String.IsNullOrEmpty(item.Cells[0].Value.ToString())
                        && !String.IsNullOrEmpty(item.Cells[1].Value.ToString())
                        )
                    {
                        config.ListBuy.Add(new Account
                        {
                            username = item.Cells[0].Value.ToString(),
                            password = item.Cells[1].Value.ToString()
                        });

                    }
                }

                foreach (DataGridViewRow item in dgvUserSell.Rows)
                {
                    if (item.Cells[0].Value != null && item.Cells[1].Value != null
                        && !String.IsNullOrEmpty(item.Cells[0].Value.ToString())
                        && !String.IsNullOrEmpty(item.Cells[1].Value.ToString())
                        )
                    {
                        config.ListSell.Add(new Account
                        {
                            username = item.Cells[0].Value.ToString(),
                            password = item.Cells[1].Value.ToString()
                        });

                    }
                }
                string path = Path.Combine(Application.StartupPath, "AccountData");
                FileInfo saveFile = new FileInfo(path + @"\betrundown.json");
                saveFile.Directory.Create();

                StreamWriter writer = saveFile.CreateText();
                writer.Write(JsonConvert.SerializeObject(config));
                writer.Close();

                UpdateLogText("Lưu thông tin thành công !", eLogTextType.Warning);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                UpdateLogText("Lỗi lưu thong tin.", eLogTextType.Error);
            }


        }

        private void dgvAccEngine_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                return;
            }

            DataGridView dgv = (DataGridView)sender;
            DataGridViewRow selectedRow = dgv.Rows[e.RowIndex];
            var user = selectedRow.Cells[0].Value;
            var pass = selectedRow.Cells[1].Value;
            var status = selectedRow.Cells[2].Value;

            if (status == null)
            {
                selectedRow.Cells[2].Style.BackColor = Color.Red;
                selectedRow.Cells[2].Value = "Login";
                return;
            }

            if (e.ColumnIndex == dgv.Columns[0].Index && status.ToString() == "Logout")
            {
                SboEngine s;
                if (dgv.Name == "dgvUserBuy")
                {
                    s = buyAccEngines.First(se => se.UserName == user.ToString());
                }
                else
                {
                    s = sellAccEngines.First(se => se.UserName == user.ToString());
                }

                var betlist = s.GetBetList();
                frmBetList f = new frmBetList(betlist, user.ToString());
                f.Show();
            }

            if (e.ColumnIndex == dgv.Columns[3].Index)
            {
                if (status.ToString() != "Logout")
                {
                    UpdateLogText("Lỗi tài khoản chưa được đăng nhập.", eLogTextType.Error);
                    return;
                }

                SboEngine s;
                if (dgv.Name == "dgvUserBuy")
                {
                    s = buyAccEngines.First(se => se.UserName == user.ToString());
                }
                else
                {
                    s = sellAccEngines.First(se => se.UserName == user.ToString());
                }

                var creadit = s.UpdateAvailabeCredit();
                UpdateLogText(string.Format("Hạn mức khả dụng tài khoản [{0}] là {1}", user, creadit), eLogTextType.Warning);
            }

            if (e.ColumnIndex == dgv.Columns[2].Index && status.ToString() == "Login")
            {
                if (user == null || pass == null)
                {
                    UpdateLogText("Thông tin tài khoản không hợp lệ!", eLogTextType.Error);
                    return;
                }

                if (accSboBet.Exists(s => s == user.ToString()))
                {
                    UpdateLogText("Tài khoản này đã tồn tại trong hệ thống, vui lòng kiểm tra lại!", eLogTextType.Error);
                    return;
                }

                bool hasFakeIp;
                if (dgv.Name == "dgvUserBuy")
                {
                    hasFakeIp = ckBuyAccRandomIp.Checked;
                }
                else
                {
                    hasFakeIp = ckSellAccRandomIp.Checked;
                }

                UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản [{0}]......", user), eLogTextType.Warning);
                SboEngine sbo = new SboEngine();
                if (hasFakeIp)
                {
                    sbo.IpFake = IpHelper.GetRandomIp();
                }

                Random r = new Random();
                var url = DataContainer.SboScanServers[r.Next(DataContainer.SboScanServers.Count())];
                sbo.Login(url, user.ToString(), pass.ToString());
                if (sbo.AccountStatus == eAccountStatus.Online)
                {
                    sbo.OnExceptionEvent += SboBetOnOnExceptionEvent;
                    sbo.StartBetRunEngine(scanType, true);
                    UpdateLogText(string.Format("Đăng nhập [{0}] thành công! Số dư khả dụng là {1}", user, sbo.UpdateAvailabeCredit()));
                    if (dgv.Name == "dgvUserBuy")
                    {
                        buyAccEngines.Add(sbo);
                    }
                    else
                    {
                        sellAccEngines.Add(sbo);
                    }

                    accSboBet.Add(user.ToString());

                    selectedRow.Cells[0] = new DataGridViewLinkCell();
                    selectedRow.Cells[0].Value = user.ToString();
                    selectedRow.Cells[2].Style.BackColor = Color.SteelBlue;
                    selectedRow.Cells[2].Value = "Logout";
                }
                else
                {
                    UpdateLogText(string.Format("Đăng nhập [{0}] thất bai.", user), eLogTextType.Error);
                }
            }

            if (e.ColumnIndex == dgv.Columns[2].Index && status.ToString() == "Logout")
            {
                SboEngine s;

                if (dgv.Name == "dgvUserBuy")
                {
                    s = buyAccEngines.First(se => se.UserName == user.ToString());
                }
                else
                {
                    s = sellAccEngines.First(se => se.UserName == user.ToString());
                }

                s.LogOff();

                if (s.AccountStatus == eAccountStatus.Offline)
                {
                    accSboBet.RemoveAll(x => x == user.ToString());

                    UpdateLogText(string.Format("Đã đăng xuất tài khoản [{0}]", user), eLogTextType.Warning);

                    if (dgv.Name == "dgvUserBuy")
                    {
                        buyAccEngines.Remove(s);
                    }
                    else
                    {
                        sellAccEngines.Remove(s);
                    }
                    selectedRow.Cells[0] = new DataGridViewTextBoxCell();
                    selectedRow.Cells[0].Value = user.ToString();
                    selectedRow.Cells[2].Style.BackColor = Color.Red;
                    selectedRow.Cells[2].Value = "Login";
                }

            }
        }

        private void btnSaveExchange_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)(() =>
            {
                buyExchange = int.Parse(txtBuyEx.Text);
                sellExchange = int.Parse(txtSellEx.Text);
            }));

            UpdateLogText("Cập nhật tỷ giá thành công!");
        }

        private void btnLoginGroup_Click(object sender, EventArgs e)
        {
            var button = sender as PictureBox;
            DataGridView dgvView = button.Name == "btnLoginBuyGroup" ? dgvUserBuy : dgvUserSell;

            bool hasFakeIp;
            if (button.Name == "btnLoginBuyGroup")
            {
                hasFakeIp = ckBuyAccRandomIp.Checked;
            }
            else
            {
                hasFakeIp = ckSellAccRandomIp.Checked;
            }

            //var aaa = dgvUserBuy.DataSource;
            for (int rows = 0; rows < dgvView.Rows.Count; rows++)
            {
                var user = dgvView.Rows[rows].Cells[0].Value;
                var pass = dgvView.Rows[rows].Cells[1].Value;
                var status = dgvView.Rows[rows].Cells[2].Value;

                if (user == null && pass == null)
                {
                    continue;
                }

                if (user == null || pass == null)
                {
                    UpdateLogText(string.Format("Thông tin tài khoản {0}/{1} không đúng!", user, pass), eLogTextType.Error);
                    continue;
                }

                if (status == null || status.ToString() == "Login")
                {
                    UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản [{0}]......", user),
                            eLogTextType.Warning);
                    SboEngine sbo = new SboEngine();
                    if (hasFakeIp)
                    {
                        sbo.IpFake = IpHelper.GetRandomIp();
                    }

                    Random r = new Random();
                    var url = DataContainer.SboScanServers[r.Next(DataContainer.SboScanServers.Count())];
                    sbo.Login(url, user.ToString(), pass.ToString());
                    if (sbo.AccountStatus == eAccountStatus.Online)
                    {
                        sbo.OnExceptionEvent += SboBetOnOnExceptionEvent;
                        sbo.StartBetRunEngine(scanType, true);
                        UpdateLogText(string.Format("Đăng nhập [{0}] thành công! Số dư khả dụng là {1}", user,
                            sbo.UpdateAvailabeCredit()));
                        if (dgvView.Name == "dgvUserBuy")
                        {
                            buyAccEngines.Add(sbo);
                        }
                        else
                        {
                            sellAccEngines.Add(sbo);
                        }

                        accSboBet.Add(user.ToString());

                        dgvView.Rows[rows].Cells[0] = new DataGridViewLinkCell();
                        dgvView.Rows[rows].Cells[0].Value = user.ToString();
                        dgvView.Rows[rows].Cells[2].Style.BackColor = Color.SteelBlue;
                        dgvView.Rows[rows].Cells[2].Value = "Logout";

                        //dgvView.Invoke(new MethodInvoker(
                        //    delegate
                        //    {

                        //    }));

                        //this.Invoke((MethodInvoker) (() =>
                        //{
                        //    dgvView.Rows[rows].Cells[0] = new DataGridViewLinkCell();
                        //    dgvView.Rows[rows].Cells[0].Value = user.ToString();
                        //    dgvView.Rows[rows].Cells[2].Style.BackColor = Color.SteelBlue;
                        //    dgvView.Rows[rows].Cells[2].Value = "Logout";
                        //}));
                    }
                    else
                    {
                        UpdateLogText(string.Format("Đăng nhập [{0}] thất bai.", user), eLogTextType.Error);
                    }
                }
            }

        }

        private void SboBetOnOnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object o)
        {
            Thread thread = new Thread(() =>
            {
                ReStartSboBet(o);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void ReStartSboBet(object obj)
        {
            SboEngine s = obj as SboEngine;
            UpdateLogText(string.Format("Mất kết nối tài khoản [{0}] đang thực hiện kết nối lại....", s.UserName), eLogTextType.Error);
            s.OnExceptionEvent -= SboBetOnOnExceptionEvent;
            s.LogOff();

            if (s.ReLogin())
            {
                s.OnExceptionEvent += SboBetOnOnExceptionEvent;
                s.StartBetRunEngine(scanType, true);
                UpdateLogText(string.Format("Kết nối tài khoản [{0}] thành công!", s.UserName), eLogTextType.Warning);
            }
        }

        private void ReStartSbo(object obj)
        {
            SboEngine s = obj as SboEngine;
            s.OnExceptionEvent -= sboScan_OnExceptionEvent;
            s.UpdateLiveDataChange -= sboScan_UpdateLiveDataChange;
            s.UpdateNonLiveDataChange -= sboScan_UpdateLiveDataChange;
            s.LogOff();

            if (s.ReLogin())
            {
                s.UpdateLiveDataChange += sboScan_UpdateLiveDataChange;
                s.UpdateNonLiveDataChange += sboScan_UpdateLiveDataChange;
                s.OnExceptionEvent += sboScan_OnExceptionEvent;
                s.StartBetRunEngine(scanType, false);
            }
        }

        private int ConvertExchange(int stake, int buyEx, int sellEx)
        {
            return (int)Math.Round((double)((stake * buyEx) / sellEx), 0);
        }

        void sboScan_OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            Thread thread = new Thread(() =>
            {
                ReStartSbo(obj);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        void sboScan_UpdateLiveDataChange(List<MatchOddDTO> m, bool isLive, int type = 0)
        {
            LastColor = RandomColor();
            string scanType = isLive ? "LIVE" : "Today";
            foreach (var matchOdd in m)
            {
                var prepareBet = sboScan.PrepareBetAnyway(matchOdd, eBetType.Home, isLive);
                if (prepareBet != null && prepareBet.MaxBet <= maxStakeCompare)
                {
                    dgvLogScan.Invoke(
                        new MethodInvoker(
                            delegate
                            {
                                DataGridViewRow row = new DataGridViewRow();
                                row.CreateCells(dgvLogScan, scanType, matchOdd.HomeTeamName, matchOdd.AwayTeamName, matchOdd.OddType, matchOdd.Odd,
                                    matchOdd.HomeOdd, matchOdd.AwayOdd, prepareBet.MaxBet);
                                row.DefaultCellStyle.BackColor = LastColor;
                                this.dgvLogScan.Rows.Insert(0, row);
                            }));
                }
            }
        }

        private Color LastColor = Color.Teal;
        private Color RandomColor()
        {
            //Random r= new Random();
            //return ColorData.Wher[r.Next(ColorData.Count)];
            if (LastColor == Color.Teal)
            {
                return Color.BurlyWood;
            }
            else if (LastColor == Color.BurlyWood)
            {
                return Color.Gray;
            }
            else if (LastColor == Color.Gray)
            {
                return Color.CadetBlue;
            }
            else if (LastColor == Color.CadetBlue)
            {
                return Color.CornflowerBlue;
            }
            else if (LastColor == Color.CornflowerBlue)
            {
                return Color.White;
            }
            else
            {
                return Color.Teal;
            }
        }

        private void UpdateLogText(string log, eLogTextType type = eLogTextType.Info)
        {
            Task.Factory.StartNew(() =>
            //Task.Run(() =>
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
                            c = Color.Red;
                            break;
                        case eLogTextType.Warning:
                            c = Color.DarkBlue;
                            break;
                        default:
                            c = Color.Black;
                            break;
                    }
                    txtLog.Select(txtLog.TextLength, 0);
                    txtLog.SelectionColor = c;
                    txtLog.AppendText(log);
                    txtLog.ScrollToCaret();
                }));
            });
        }

        public static int keyPing = 11;
        void PingManage(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (_manageService.PingBet(SelfInfo.Username, SelfInfo.MacAddress, keyPing) != 1)
                {
                    timerPing.Stop();
                    sboScan.LogOff();
                    foreach (var buyEngine in buyAccEngines)
                    {
                        if (buyEngine.AccountStatus == eAccountStatus.Online)
                        {
                            buyEngine.LogOff();
                        }
                    }

                    foreach (var sellEn in sellAccEngines)
                    {
                        if (sellEn.AccountStatus == eAccountStatus.Online)
                        {
                            sellEn.LogOff();
                        }
                    }

                    buyAccEngines.Clear();
                    sellAccEngines.Clear();

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

        private void rdBetHome_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rdBetAway_CheckedChanged(object sender, EventArgs e)
        {

        }

    }
}
