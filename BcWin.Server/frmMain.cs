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
using System.Windows.Forms;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Processor;
using BcWin.Server.Service;
using mshtml;

namespace BcWin.Server
{
    public partial class frmMain : Form
    {
        public static frmMain CurrentInstance;
        public SynchronizationContext MySynchronizationContext;
        public void Show()
        {
            //webIBet.Navigate("http://www.currybread.com/");
            addNewTab("http://www.88cado.com/");
        }
        public frmMain()
        {
            InitializeComponent();
            MySynchronizationContext = SynchronizationContext.Current;
            CurrentInstance = this;
            
            //webIBet.Navigate("http://www.88cado.com/");
           // webSbo.Navigate("http://www.currybread.com/");

            MainService.Initialize();
        }

        private void addNewTab(string url)
        {
            TabPage tpage = new TabPage();
            tpage.Text = url;
            tpage.BorderStyle = BorderStyle.Fixed3D;
            tabMain.TabPages.Insert(tabMain.TabCount - 1, tpage);
            WebBrowser browser = new WebBrowser();
            browser.Name = "bow1";
            browser.Navigate(url);
            tpage.Controls.Add(browser);
            browser.Dock = DockStyle.Fill;
            tabMain.SelectTab(tpage);
        }
        
        private void btnLoginIBet_Click(object sender, EventArgs e)
        {
            //AccountDTO a = DataContainer.IbetAccounts[0];
            //MainService.IbetEngineObj.Login(ref webIBet, a.Username, a.Password);
        }

        private void btnLoginSbo_Click(object sender, EventArgs e)
        {
            //AccountDTO a = DataContainer.SboAccounts[0];
            //MainService.SboEngineObj.Login(ref webSbo, a.Username, a.Password);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = MainService.IbetEngineObj.LiveMatchOddDatas;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                //webIBet.Navigate("http://www.88cado.com/");
                //MainService.IbetSboProcessorObj.Initialize();
                var oSvc = new ServiceHost(typeof(BcService));
                oSvc.Open();
                //MainService.IbetEngineObj.InitEngine(ref webIBet, "http://www.88cado.com/");
                //MainService.SboEngineObj.InitEngine(ref webSbo, "http://www.currybread.com/");
                //MainService.IbetSboProcessorObj.IbetEngine = MainService.IbetEngineObj;
                //MainService.IbetSboProcessorObj.SboEngine = MainService.SboEngineObj;
                //MainService.IbetSboProcessorObj.Initialize();
                //MainService.IbetSboProcessorObj.Start();
            }
            catch (Exception)
            {
                throw;
            }
            

        }
        private void InjectAlertBlocker(ref WebBrowser webBrowser)
        {
            HtmlElement head = webBrowser.Document.GetElementsByTagName("head")[0];
            HtmlElement scriptEl = webBrowser.Document.CreateElement("script");
            IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
            string alertBlocker = "window.alert = function () { }";
            element.text = alertBlocker;
            head.AppendChild(scriptEl);
        }
        private void ScanProcessorObj_UpdateNonLiveDataChangeTest(List<Common.DTO.MatchOddDTO> m1)
        {
            //List<MatchOddTest> newList = MainService.IbetEngineObj.LiveMatchDatas
            //    .SelectMany(m => m.Odds.Select(o => new MatchOddTest()
            //    {
            //        HomeTeamName = m.HomeTeamName,
            //        AwayTeamName = m.AwayTeamName,
            //        LeagueName = m.LeagueName,
            //        Odd = o.Odd,
            //        Home = o.Home,
            //        Away = o.Away,
            //        Type = o.Type
            //    })).OrderBy(o => o.HomeTeamName).ToList();
            SetDGVValue(m1);
            //dataGridView1.DataSource = newList;
        }

        private delegate void SetDGVValueDelegate(List<Common.DTO.MatchOddDTO> table);

        private void SetDGVValue(List<Common.DTO.MatchOddDTO> table)
        {
            if (dataGridView1.InvokeRequired)
            {
                //dataGridView1.Refresh();
                //dataGridView1.Invoke(new SetDGVValueDelegate(SetDGVValue), table);
                dataGridView1.Invoke((MethodInvoker) delegate()
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

        #region Sbobet test

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                MainService.SboEngineObj.StartScanEngine();
               
                MainService.IbetSboProcessorObj.SboEngine = MainService.SboEngineObj;
                MainService.IbetSboProcessorObj.InitializeSbobet();
                webSbo.Navigate("https://www.google.com/");
            }
            catch (Exception)
            {
                
                throw;
            }
        }

        private void ScanProcessorObj_SbobetUpdateNonLiveDataChangeTest(List<Common.DTO.MatchOddDTO> m1)
        {
            SetDGVValue(m1);
            //dataGridView1.DataSource = newList;
        }

        #endregion

        private void button5_Click(object sender, EventArgs e)
        {
            var result = MainService.SboEngineObj.ProcessUpdateLiveData();
            var result1 = MainService.SboEngineObj.ProcessUpdateNonLiveData();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            MainService.SboEngineObj.InitEngine();
            MainService.IbetEngineObj.InitEngine();
            
            MainService.IbetSboProcessorObj.IbetEngine = MainService.IbetEngineObj;
            MainService.IbetSboProcessorObj.SboEngine = MainService.SboEngineObj;
            MainService.IbetSboProcessorObj.Initialize();
            MainService.IbetSboProcessorObj.Start();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            //var oddId = dataGridView1.SelectedCells[0].Value.ToString();
            //MainService.IbetSboProcessorObj.PrepareBetTest(oddId);
        }
    }
}
