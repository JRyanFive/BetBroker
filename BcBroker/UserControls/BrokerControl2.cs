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
    public partial class BrokerControl2 : UserControl
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BrokerControl2));

        Color ClickedStyleButton = Color.LightCyan;
        Color NomalStyleButton = Color.WhiteSmoke;

        IbetSboBroker2 broker = new IbetSboBroker2();

        private int mainRate = 0;

        public BrokerControl2()
        {
            InitializeComponent();

            dgvLeaguesSetting.DataSource = DataContainer.LeaguesSettings;

            cbOddDev.SelectedIndex = 0;
            if (!InitConfigSaved())
            {
                cbOddPairCheck.SelectedIndex = 2;
                cbGoalDefCheck.SelectedIndex = 2;
                cbOddDev.SelectedIndex = 0;
                numTimeCheckScan.Value = 10;
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

                            txtStake.Text = con.buyAmount;
                            cbGoalDefCheck.Text = con.MinHDPOdd.ToString();
                            numTimeCheckScan.Value = con.BuyAfterMin;
                            cbOddPairCheck.Text = con.ProfitMin.ToString();
                            ckRandomIpIbet.Checked = con.FakeRandomIpIbet;
                            ckRandomIpSbo.Checked = con.FakeRandomIpSbo;
                            cbOddDev.Text = con.OddDef.ToString();
                            ckProxyIbet.Checked = con.ProxyLoginIbet;
                            txtIpProxyIbet.Text = con.ProxyLoginIbetValue;

                            dgvAccount.Rows.Clear();
                            foreach (var item in con.Accounts)
                            {
                                dgvAccount.Rows.Add(item.serverType, item.username, item.password, item.rate, "Login", "", "", "", item.domain, "", item.IsChecked);

                                if (item.IsChecked)
                                {
                                    mainRate = Convert.ToInt32(item.rate);
                                }
                            }

                            if (dgvAccount.Rows.Count > 0 && mainRate == 0)
                            {
                                dgvAccount.Rows[0].Cells[10].Value = true;
                                mainRate = Convert.ToInt32(dgvAccount.Rows[0].Cells[3].Value);
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
                    BuyAfterMin = (int)numTimeCheckScan.Value,
                    ProfitMin = float.Parse(cbOddPairCheck.Text),
                    OddDef = float.Parse(cbOddDev.Text),
                    FakeRandomIpIbet = ckRandomIpIbet.Checked,
                    FakeRandomIpSbo = ckRandomIpSbo.Checked,

                    ProxyLoginIbet = ckProxyIbet.Checked,
                    ProxyLoginIbetValue = txtIpProxyIbet.Text,
                    Accounts = new List<Account>(),
                };

                foreach (DataGridViewRow item in dgvAccount.Rows)
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

                        config.Accounts.Add(new Account
                        {
                            serverType = item.Cells[0].Value.ToString(),
                            username = item.Cells[1].Value.ToString(),
                            password = item.Cells[2].Value.ToString(),
                            rate = item.Cells[3].Value.ToString(),
                            domain = item.Cells[8].Value.ToString(),
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
            frmInfoAcc f = new frmInfoAcc();
            f.ShowDialog();
            if (!string.IsNullOrEmpty(f.ServerType))
            {
                dgvAccount.Rows.Add(f.ServerType, f.Username, f.Password, f.Rate, "Login", null, null, null, f.Domain);
            }

            if (dgvAccount.Rows.Count == 1)
            {
                dgvAccount.Rows[0].Cells[10].Value = true;
                mainRate = Convert.ToInt32(dgvAccount.Rows[0].Cells[3].Value);
            }
        }

        private void ckProxyIbet_CheckedChanged(object sender, EventArgs e)
        {
            txtIpProxyIbet.Enabled = ckProxyIbet.Checked;
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
            DataGridViewRow selectedRow = dgv.Rows[e.RowIndex];
            var serverType = selectedRow.Cells[0].Value;
            var user = selectedRow.Cells[1].Value;
            var pass = selectedRow.Cells[2].Value;
            var rate = selectedRow.Cells[3].Value;
            var status = selectedRow.Cells[4].Value;
            var domain = selectedRow.Cells[8].Value;

            //dang nhap
            if (e.ColumnIndex == dgv.Columns[4].Index)
            {
                if (status.ToString() == "Login")
                {
                    IEngineBroker engine = null;
                    if (serverType.ToString() == "IBET")
                    {
                        engine = InitLoginIbet(domain.ToString(), user.ToString(), pass.ToString(),
                            ckRandomIpIbet.Checked, Convert.ToInt32(rate));
                    }
                    else if (serverType.ToString() == "SBO")
                    {
                        engine = InitLoginSbo(domain.ToString(), user.ToString(), pass.ToString(),
                            ckRandomIpSbo.Checked, Convert.ToInt32(rate));
                    }

                    if (engine != null)
                    {
                        broker.AddNewBetEngine(engine);
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
                    broker.RemoveBetEngine(engineId);

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
                    frmInfoAcc f = new frmInfoAcc(serverType.ToString(), user.ToString(), pass.ToString(), domain.ToString(), rate.ToString());
                    f.ShowDialog();
                    if (!string.IsNullOrEmpty(f.ServerType))
                    {
                        selectedRow.Cells[0].Value = f.ServerType;
                        selectedRow.Cells[1].Value = f.Username;
                        selectedRow.Cells[2].Value = f.Password;
                        selectedRow.Cells[3].Value = f.Rate;
                        selectedRow.Cells[8].Value = f.Domain;
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
                    IEngineBroker engine = broker.BetEngine.First(b => b.EngineId == engineId);
                    frmStatement f = new frmStatement(engine, user.ToString());
                    f.Show();
                }
            }
            else if (e.ColumnIndex == dgv.Columns[5].Index)
            {
                if (status.ToString() == "Logout")
                {
                    var engineId = selectedRow.Cells[9].Value.ToString();
                    IEngineBroker engine = broker.BetEngine.First(b => b.EngineId == engineId);
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
                foreach (DataGridViewRow dr in dgv.Rows)
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
            SetProcessingStyle();

            if (broker.IbetEn.AccountStatus == eAccountStatus.Offline)
            {
                if (ckProxyIbet.Checked)
                {
                    broker.IbetEn.ProxyLogin = true;
                    broker.IbetEn.ProxyEndpoint = string.Concat("net.tcp://", txtIpProxyIbet.Text, ":9998/bcwinsupservice");
                }

                if (broker.IbetEn.Login(BrokerData.ScanIbetDomain, BrokerData.ScanIbetUsername, BrokerData.ScanIbetPassword))
                {
                    UpdateLogText("Kết nối server ibet thành công.");
                }
                else
                {
                    UpdateLogText("Kết nối server ibet không thành công.", eLogTextType.Error);
                }
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Offline)
            {
                if (broker.SboEn.Login(BrokerData.ScanSboDomain, BrokerData.ScanSboUsername, BrokerData.ScanSboPassword))
                {
                    UpdateLogText("Kết nối server sbo thành công.");
                }
                else
                {
                    UpdateLogText("Kết nối server sbo không thành công.", eLogTextType.Error);
                }
            }

            if (broker.SboEn.AccountStatus == eAccountStatus.Online
                        && broker.IbetEn.AccountStatus == eAccountStatus.Online)
            {
                broker.OddPairCheck = float.Parse(cbOddPairCheck.Text);
                broker.GoalDefCheck = float.Parse(cbGoalDefCheck.Text);
                broker.TimeCheckScan = (int)numTimeCheckScan.Value;
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

        private IbetEngine InitLoginIbet(string domain, string username, string pass, bool hasFakeIp, int rate)
        {
            UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản IBET [{0}]......", username), eLogTextType.Warning);

            IbetEngine ibetEngine = new IbetEngine();
            if (ckProxyIbet.Checked)
            {
                broker.IbetEn.ProxyLogin = true;
                broker.IbetEn.ProxyEndpoint = string.Concat("net.tcp://", txtIpProxyIbet.Text, ":9998/bcwinsupservice");
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

        private SboEngine InitLoginSbo(string domain, string username, string pass, bool hasFakeIp, int rate)
        {
            UpdateLogText(string.Format("Bắt đầu đăng nhập tài khoản SBO [{0}]......", username), eLogTextType.Warning);

            SboEngine sboEngine = new SboEngine();
            if (hasFakeIp)
            {
                sboEngine.IpFake = IpHelper.GetRandomIp();
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

            if (status == eBrokerStatus.Bet)
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
    }
}
