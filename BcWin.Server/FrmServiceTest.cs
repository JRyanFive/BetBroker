using System;
using System.Collections;
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
using BcWin.Common.DTO;
using BcWin.Common.FaultDTO;
using BCWin.Metadata;
using BcWin.Processor;
using BcWin.Processor.Interface;
using BcWin.Server.Service;
using mshtml;
using BcWin.Core.Server;

namespace BcWin.Server
{
    public partial class FrmServiceTest : Form
    {
        //public static FrmServiceTest CurrentInstance;
        //public SynchronizationContext MySynchronizationContext;

        Dictionary<Guid, IProcessor> ProcessorContainer = new Dictionary<Guid, IProcessor>();
        ArrayList tabpages = new ArrayList();
        Dictionary<Guid, WebBrowser> webpageDic = new Dictionary<Guid, WebBrowser>();
        private ServiceHost oSvc;

        public FrmServiceTest()
        {
            InitializeComponent();
            //MySynchronizationContext = SynchronizationContext.Current;
            //CurrentInstance = this;
            this.CreateNewTab("http://www.88cado.com/", out niGuid);
            this.CreateNewTab("http://www.currybread.com/", out nsGuid);
        }

        public Guid InitProcessor(IbetEngine ibetEngine, SboEngine sboEngine, ProcessorConfigInfoDTO secondAccountDto)
        {
            try
            {
                Guid ni = Guid.NewGuid();
                IProcessor processor = new IbetSboProcessor();
                processor.IbetEngine = ibetEngine;
                processor.SboEngine = sboEngine;
               
                //processor.IbetEngine.InitEngine();
                //processor.SboEngine.InitEngine();
                //processor.Initialize();
                //processor.Start();
                ProcessorContainer.Add(ni, processor);
                return ni;
            }
            finally
            {
                //RemoveTab(firstAccountDto.GuidID);
                //RemoveTab(secondAccountDto.GuidID);
            }

            //Object.ReferenceEquals(n1.GetType(), n2.GetType()));
        }

        public void InitProcessor(Guid processGuid, AccountDTO firstAccountDto, AccountDTO secondAccountDto)
        {
            try
            {
                IProcessor processor = new IbetSboProcessor();

                LoginEngine(firstAccountDto, ref processor);
                LoginEngine(secondAccountDto, ref processor);
                //processor.IbetEngine.InitEngine();
                //processor.SboEngine.InitEngine();
                //processor.Initialize();
                //processor.Start();
                ProcessorContainer.Add(processGuid, processor);
            }
            finally
            {
                //RemoveTab(firstAccountDto.GuidID);
                //RemoveTab(secondAccountDto.GuidID);
            }

            //Object.ReferenceEquals(n1.GetType(), n2.GetType()));
        }

        public void StartProcessor(Guid processGuid)
        {
            var processor = ProcessorContainer[processGuid];
            //processor.Initialize();
            processor.Start();
            foreach (var account in processor.AccountDic)
            {
                RemoveTab(account.Key);
            }
        }

        public void StopProcessor(Guid processGuid)
        {
            var currentProcessor = ProcessorContainer[processGuid];
            currentProcessor.Dispose();
        }

        private void RemoveTab(Guid guid)
        {
            TabPage current_tab = tabMain.TabPages["tab" + guid];
            WebBrowser thiswebpage = webpageDic[guid];
            thiswebpage.Navigated -= OnWebBrowserNavigated;
            thiswebpage.Dispose();
            tabpages.Remove(current_tab);
            current_tab.Dispose();
            tabMain.TabPages.Remove(current_tab);
            webpageDic.Remove(guid);
            //current_tab_count--;
        }

        public void CreateNewTab(string url, out Guid? uId)
        {
            //if (ServerHelper.CheckDomain(url) == false)
            //{
            //    uId = Guid.Empty;
            //    return;
            //}
            uId = Guid.NewGuid();
            //if (current_tab_count == 10) return;
            //text tht will appear on first time opening of aplication
            TabPage newpage = new TabPage(uId.ToString());
            newpage.Name = "tab" + uId.ToString();
            tabpages.Add(newpage);
            tabMain.TabPages.Add(newpage);
            //current_tab_count++;
            WebBrowser webpage = new WebBrowser();
            webpage.ScriptErrorsSuppressed = true;
            webpage.Navigated += OnWebBrowserNavigated;
            webpage.Name = uId.ToString();
            webpage.Navigate(url);
            webpageDic[(Guid)uId] = webpage;
            webpage.Parent = newpage;
            webpage.Dock = DockStyle.Fill;



            //event to keep check of webpage loaded completly or not
            //webpage.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);
            //timer1.Enabled = true;
            //tabControl1.SelectedTab = newpage;
        }

        
        private void LoginEngine(AccountDTO accountDto, ref IProcessor ibetSboProcessor)
        {
            switch (accountDto.ServerType)
            {
                case eServerType.Ibet:
                    ibetSboProcessor.IbetEngine = new IbetEngine();
                    ibetSboProcessor.IbetEngine.Account = accountDto;
                    //ibetSboProcessor.EngineContainer.Add(accountDto.GuidID, ibetSboProcessor.IbetEngine);
                    WebBrowser ibetWeb = webpageDic[accountDto.GuidID];
                    ibetSboProcessor.IbetEngine.Login(ref ibetWeb, accountDto.Username, accountDto.Password);
                    //var idelay = Task.Run(() => DelayTask(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000));

                    //if (idelay.Result && !ibetSboProcessor.IbetEngine.CheckLogin())
                    //{
                    //    StartServerFault startServerFault = new StartServerFault();
                    //    startServerFault.ServerID = accountDto.GuidID;
                    //    startServerFault.Message = "Login Fail !";
                    //    throw new FaultException<StartServerFault>(startServerFault);
                    //}
                    break;
                case eServerType.Sbo:
                    ibetSboProcessor.SboEngine = new SboEngine();
                    ibetSboProcessor.SboEngine.Account = accountDto;
                    WebBrowser sboWeb = webpageDic[accountDto.GuidID];
                    ibetSboProcessor.SboEngine.Login(ref sboWeb, accountDto.Username, accountDto.Password);

                    //Task.Delay(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000);
                    //var sdelay = Task.Run(() => DelayTask(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000));
                    //////var a= DelayTask(SystemConfig.TIME_GET_COOKIE_AFTER_LOGIN + 3000, ibetSboProcessor.SboEngine);
                    //var b =a.Result;
                    //if (sdelay.Result && !ibetSboProcessor.SboEngine.CheckLogin())
                    //{
                    //    StartServerFault startServerFault = new StartServerFault();
                    //    startServerFault.ServerID = accountDto.GuidID;
                    //    startServerFault.Message = "Login Fail !";
                    //    throw new FaultException<StartServerFault>(startServerFault);
                    //}
                    break;
            }

            ibetSboProcessor.AccountDic[accountDto.GuidID] = accountDto;
        }


        private static int ii = 0;
        private void OnWebBrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            //HideScriptErrors((WebBrowser)sender, true);
            var w = (WebBrowser)sender;
            if (w.ReadyState == WebBrowserReadyState.Complete)
            {
                HtmlElement head = w.Document.GetElementsByTagName("head")[0];
                HtmlElement scriptEl = w.Document.CreateElement("script");
                IHTMLScriptElement element = (IHTMLScriptElement)scriptEl.DomElement;
                string alertBlocker = "window.alert = function () { }";
                element.text = alertBlocker;
                head.AppendChild(scriptEl);
            }
        }

        private void btnStartService_Click(object sender, EventArgs e)
        {
            CoreProcessor.InitConfig();
            //if (oSvc == null || oSvc.State != CommunicationState.Opened)
            //{
            //    oSvc = new ServiceHost(typeof(BcService));
            //    oSvc.Open();
            //}
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            //if (oSvc != null && oSvc.State == CommunicationState.Opened)
            //{
            //    oSvc.Close();
            //}
        }

        Guid? niGuid = Guid.Empty;
        Guid? nsGuid = Guid.Empty;
        Guid procesGuid = Guid.Empty;

        private void btnInit_Click(object sender, EventArgs e)
        {
            this.CreateNewTab("http://www.88cado.com/", out niGuid);
            this.CreateNewTab("http://www.currybread.com/", out nsGuid);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            procesGuid = Guid.NewGuid();

            AccountDTO iaccountDto = new AccountDTO();
            iaccountDto.Username = "ZW209902020"; //test
            //iaccountDto.Username = "ZW209902002";
            //iaccountDto.Username = "ZW209902030";
            iaccountDto.Password = "ABab1212";
            iaccountDto.ServerType = eServerType.Ibet;
            iaccountDto.GuidID = (Guid)niGuid;
            AccountDTO saccountDto = new AccountDTO();
            saccountDto.Username = "msn99aa020"; //test
            //saccountDto.Username = "msn99aa002";
            //saccountDto.Username = "msn99aa030";
            saccountDto.Password = "123@@Googlee";
            saccountDto.ServerType = eServerType.Sbo;
            saccountDto.GuidID = (Guid)nsGuid;

            this.InitProcessor(procesGuid, iaccountDto, saccountDto);
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            this.StartProcessor(procesGuid);

        }

        private void btnGetList_Click(object sender, EventArgs e)
        {
            var proc = ProcessorContainer.First().Value;
            var ibetBet = proc.IbetEngine.GetBetList(DateTime.Now);
            var sboBet = proc.SboEngine.GetBetList();
            FrmBetList frmBetList = new FrmBetList();
            frmBetList.Web1Source = ibetBet;
            frmBetList.Web2Source = sboBet;
            frmBetList.Show();
        }
    }
}
