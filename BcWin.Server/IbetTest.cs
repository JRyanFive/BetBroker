using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcWin.Server
{
    public partial class IbetTest : Form
    {
        Dictionary<Guid, WebBrowser> webpageDic = new Dictionary<Guid, WebBrowser>();

        AutoResetEvent autoEvent = new AutoResetEvent(false);

        public IbetTest()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            var aa = webpageDic;
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void IbetTest_Load(object sender, EventArgs e)
        {
            //this.webBrowser1.Navigated += webBrowser1_Navigated(this,new WebBrowserNavigatedEventArgs(new Uri("ab")));
            webBrowser1.Navigate("http://www.88cado.com/");
            Task.Run(() => Sleep(3000));
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var aa = (WebBrowser)sender;
            var bbb = aa.Url;
        }

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            var aa = (WebBrowser) sender;
            var bbb = aa.Url;
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            var aa = (WebBrowser)sender;
            var bbb = aa.Url.ToString();
            if (!bbb.Contains("www"))
            {
                autoEvent.Set();
            }
        }

        private int Sleep(int ms)
        {
            //WriteLog("TEST");
            //Console.WriteLine("Sleeping for " + ms);
            //Thread.Sleep(ms);
            //Console.WriteLine("Sleeping for " + ms + " FINISHED");
            ms = ms + 123;
            autoEvent.WaitOne();
            ms = ms + 4444;
            return ms;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Guid g = Guid.NewGuid();
            WebBrowser w= new WebBrowser();
            w.Navigate("http://www.88cado.com/");
            webpageDic[g] = w;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var webBrowserIbet = webpageDic.First().Value;
            var webBrowserIbet = webBrowser1;
            webBrowserIbet.Document.GetElementById("txtID").InnerText = "ZW209902010";
            webBrowserIbet.Document.GetElementById("txtPW").InnerText = "ABab1212";

            var links = webBrowserIbet.Document.GetElementsByTagName("div");
            foreach (HtmlElement link in links)
            {

                if (link.GetAttribute("className") == "loginBtnPos")
                {
                    var lglinks = link.Document.GetElementsByTagName("a");
                    lglinks[0].InvokeMember("click");
                    //DataContainer.IsLoginIbet = true;
                    ////TODO:Check already login
                    //Task.Run(() =>
                    //{
                    //    while (true)
                    //    {
                    //        //var aa = webBrowserIbet;
                    //        Task.Delay(10000);
                    //    }

                    //});
                    //SetIbetCookieContainer(webBrowserIbet);

                    break;
                }
            }
        }
    }
}
