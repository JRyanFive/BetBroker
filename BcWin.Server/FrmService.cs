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
    public partial class FrmService : Form
    {
        public static FrmService CurrentInstance;
        public SynchronizationContext MySynchronizationContext;

        Dictionary<Guid, IProcessor> ProcessorContainer = new Dictionary<Guid, IProcessor>();
        ArrayList tabpages = new ArrayList();
        Dictionary<Guid, WebBrowser> webpageDic = new Dictionary<Guid, WebBrowser>();
        private ServiceHost oSvc;

        public FrmService()
        {
            InitializeComponent();
            MySynchronizationContext = SynchronizationContext.Current;
            CurrentInstance = this;
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
                ProcessorContainer.Add(processGuid,processor);
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
            TabPage current_tab = tabMain.TabPages[guid.ToString()];
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
                Thread.Sleep(10000);
                //text tht will appear on first time opening of aplication
                TabPage newpage = new TabPage(uId.ToString());
                newpage.Name = uId.ToString();
                tabpages.Add(newpage);
                tabMain.TabPages.Add(newpage);
                //current_tab_count++;
                WebBrowser webpage = new WebBrowser();
                webpage.Navigated += OnWebBrowserNavigated;
                webpage.Navigate(url);
                webpageDic[(Guid)uId] = webpage;
                webpage.Parent = newpage;
                webpage.Dock = DockStyle.Fill;
           
          

            //event to keep check of webpage loaded completly or not
            //webpage.DocumentCompleted += new WebBrowserDocumentCompletedEventHandler(webpage_DocumentCompleted);
            //timer1.Enabled = true;
            //tabControl1.SelectedTab = newpage;
        }

       

        public void Login(AccountDTO accountDto)
        {
            
        }

        private void LoginEngine(AccountDTO accountDto, ref IProcessor ibetSboProcessor)
        {
            switch (accountDto.ServerType)
            {
                case eServerType.Ibet:
                    ibetSboProcessor.IbetEngine = new IbetEngine();
                    ibetSboProcessor.IbetEngine.Account = accountDto;
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
            if (oSvc == null || oSvc.State != CommunicationState.Opened)
            {
                oSvc = new ServiceHost(typeof(BcService));
                oSvc.Open();
            }
        }

        private void btnStopService_Click(object sender, EventArgs e)
        {
            if (oSvc != null && oSvc.State == CommunicationState.Opened)
            {
                oSvc.Close();
            }
        }
    }
}
