using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Processor.Helper;
using mshtml;
using HtmlAgilityPack;

namespace BcWin.Server
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            webBrowser1.ScriptErrorsSuppressed = true;
            //var aa = DateTime.Now.ToString("hh:mm:ss t z");
            //var a1a = DateTime.Now.ToString("HH:mm:ss.fff");
            
        }
        EngineLogger logger = new EngineLogger("ABC");

        private void Form1_Load(object sender, EventArgs e)
        {
            
            logger.Log("DFASD");
            logger.Error("EXCEPTION", new Exception("TEST"));
            var aa = System.IO.File.ReadAllText(@"log.html");
            webBrowser1.DocumentText = aa;
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            AddStyles();
        }

        private void AddStyles()
        {
            try
            {
                if (webBrowser1.Document != null)
                {
                    IHTMLDocument2 currentDocument = (IHTMLDocument2)webBrowser1.Document.DomDocument;

                    int length = currentDocument.styleSheets.length;
                    IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
                    //length = currentDocument.styleSheets.length;
                    //styleSheet.addRule("body", "background-color:blue");
                    TextReader reader = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "table_w.css"));
                    TextReader reader1 = new StreamReader(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "oddsFamily.css"));
                    string style = reader.ReadToEnd();
                    style += reader1.ReadToEnd();
                    styleSheet.cssText = style;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int i = 1;
        private void button1_Click(object sender, EventArgs e)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://www.bong88.com");
            request.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();



            // Print the properties of each cookie.
            foreach (Cookie cook in response.Cookies)
            {
                // Show the string representation of the cookie.
                Console.WriteLine("String: {0}", cook.ToString());
            }
        }
       
    }
    public class MyWebClient
    {
        //The cookies will be here.
        private CookieContainer _cookies = new CookieContainer();

        //In case you need to clear the cookies
        public void ClearCookies()
        {
            _cookies = new CookieContainer();
        }

        public HtmlAgilityPack.HtmlDocument GetPage(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";

            //Set more parameters here...
            //...

            //This is the important part.
            request.CookieContainer = _cookies;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            //When you get the response from the website, the cookies will be stored
            //automatically in "_cookies".

            using (var reader = new StreamReader(stream))
            {
                string html = reader.ReadToEnd();
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(html);
                return doc;
            }
        }
    }
}
