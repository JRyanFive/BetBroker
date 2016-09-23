using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Processor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcWin.Server
{

   
    public partial class SboTest : Form
    {
        public string GetKey()
        { 
            var rd = new Random();
            decimal n = rd.Next();
            var _local2 = Math.Round(n * 9999);
            var _local3 = encode2((_local2 + 10000).ToString().Substring(1));
            return _local3;
        }
        private static string endcode(string _arg1, int _arg2)
        {
            //var _local3 = new Base64Encoder();
            return null;
        }
        private static string encode2(string _arg1){
            var KEY2 = new byte[16];
            var _local2 = "";
            var _local3 = 0;
            var aa = KEY2.ToString();
            while (_local3 < _arg1.Length) {
                _local2 = (_local2 + KEY2.ToString().Substring(int.Parse(_arg1.Substring(_local3, 1), 0), 1));
                _local3++;
            };
            return (_local2);
        }

        private System.Threading.Timer objLiveScanTimer; //Test
        public SboTest()
        {
            InitializeComponent();
            webBrowser1.Navigate("http://www.sbobet.com/");
            MainService.Initialize();
        
        
        }
        //Login
        private void button1_Click(object sender, EventArgs e)
        {
            //AccountDTO a = DataContainer.SboAccounts[0];
            //MainService.SboEngineObj.Login(ref webBrowser1, a.Username, a.Password);
        }


        //Start engine
        private void button2_Click(object sender, EventArgs e)
        {
           
                
        }
        private void SetDGVValue(List<MatchOddDTO> table)
        {
            if (dataGridView1.InvokeRequired)
            {
                //dataGridView1.Refresh();
                //dataGridView1.Invoke(new SetDGVValueDelegate(SetDGVValue), table);
                dataGridView1.Invoke((MethodInvoker)delegate()
                {
                    dataGridView1.Invalidate();
                    dataGridView1.DataSource = table;
                });
            }
            else
            {
                dataGridView1.DataSource = table;
            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
           // MainService.SboEngineObj.InitEngine();
            webBrowser1.Navigate("www.google.com");
        }

        private void WaitScanCallback(object obj)
        {
            GetLiveData();
        }

        private void GetLiveData()
        {
            var list = MainService.SboEngineObj.ProcessUpdateLiveData();

            dataGridView1.DataSource = MainService.SboEngineObj.LiveMatchOddDatas;

            //Test Check da update
            if (list.Count() > 0)
            {
                foreach (var item in list)
                {
                    MatchOddDTO oldOdd = MainService.SboEngineObj.LiveMatchOddDatas.FirstOrDefault(x => x.OddID == item.OddID);
                    if (oldOdd != null)
                    {
                        string re = DateTime.Now.ToString() + "-- Tran: " + oldOdd.HomeTeamName + " : " + oldOdd.Odd + " -Update to : Home: " + oldOdd.HomeOdd.ToString() + " - Away: " + oldOdd.AwayOdd.ToString() + " \r";
                        re += "From : Home: " + item.HomeOdd.ToString() + " - Away: " + item.AwayOdd.ToString() + " \r";
                        re += "-------------\r";
                        richTextBox1.BeginInvoke(
                             (Action)(() =>
                             {
                                 richTextBox1.AppendText(Environment.NewLine + re);
                             }));
                    }
                }
            }
        }

        
       
      
        //Get live and update data
        private void button3_Click(object sender, EventArgs e)
        {
            //MainService.SboEngineObj.PaserTest(richTextBox1.Text.Trim());
            MainService.SboEngineObj.GetStatement(DateTime.Now);
            //var list = MainService.SboEngineObj.ProcessUpdateNonLiveData();

            ////Test Check da update
            //if (list.Count() > 0)
            //{
            //    foreach (var item in list)
            //    {
            //        string betcount = "";
            //        int maxStake = 0;
            //        bool allow = false;
            //        string betKind = "";
            //        string homeName = "";
            //        string awayName = "";

            //        MainService.SboEngineObj.PrepareBet(item.OddID, item.HomeOdd.ToString(),"h", out betcount, out maxStake, out allow, out betKind, out homeName, out awayName);
            //    }
            //}

        }
        //Betting

        private void button5_Click(object sender, EventArgs e)
        {
            var oddId = txtOid_1.Text.Trim();
            var match = MainService.SboEngineObj.LiveMatchOddDatas.Where(l => l.OddID == oddId);
            var fmatch = match.FirstOrDefault();
            if (fmatch != null)
            {
                MainService.SboEngineObj.PrepareBet(fmatch, BcWin.Common.DTO.eBetType.Home, true);
                MainService.SboEngineObj.ConfirmBet(10);
            }


            //string[] oids = new string[] {txtOid_1.Text.Trim(), txtOid_2.Text.Trim() };
            //foreach (string id in oids)
            //{
                
            //}
           
            //IbetEngine.ConfirmBet(3);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            GetLiveData();
            //MainService.SboEngineObj.PaserTest(richTextBox1.Text.Trim());
            //string result = MainService.SboEngineObj.GetBetList();
            //if (!string.IsNullOrEmpty(result))
            //{
            //    webBrowser2.DocumentText = result;
            //}
        }
    }



}
