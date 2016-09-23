using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.CssStyle;
using mshtml;

namespace BcWin.UserControls
{
    public partial class OddList : UserControl
    {
        private AccountConfig _accountConfig;
        public OddList()
        {
            InitializeComponent();
        }
        
        public OddList(AccountConfig accountConfig)
        {
            InitializeComponent();
            _accountConfig = accountConfig;
            webBrowser1.DocumentCompleted += OnIbetDocumentCompleted;
        }

        private void btnBetList_Click(object sender, EventArgs e)
        {
            var ibetList = _accountConfig.IbetEngine.GetBetList(dateTimePicker1.Value);
            var sbobetList = _accountConfig.SboEngine.GetBetList();

            HtmlDocument doc1 = this.webBrowser1.Document;
            if (doc1!=null)
                doc1.Write(String.Empty);

            webBrowser1.DocumentText = ibetList;
           

            HtmlDocument doc2 = this.webBrowser2.Document;
            if (doc2 != null)
                doc2.Write(String.Empty);

            webBrowser2.DocumentText = sbobetList;
           
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            var ibetList = _accountConfig.IbetEngine.GetStatement(dateTimePicker1.Value);
            var sbobetList = _accountConfig.SboEngine.GetStatement(dateTimePicker1.Value);

            HtmlDocument doc1 = this.webBrowser1.Document;
            if (doc1 != null)
                doc1.Write(String.Empty);

            webBrowser1.DocumentText = ibetList;

            HtmlDocument doc2 = this.webBrowser2.Document;
            if (doc2 != null)
                doc2.Write(String.Empty);

            webBrowser2.DocumentText = sbobetList;
        }

        void OnIbetDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webBrowser1.Document != null)
            {
                IHTMLDocument2 currentDocument = (IHTMLDocument2)webBrowser1.Document.DomDocument;

                int length = currentDocument.styleSheets.length;
                IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
                styleSheet.cssText = Css.IbetStatements;

            }
        }
    }
}
