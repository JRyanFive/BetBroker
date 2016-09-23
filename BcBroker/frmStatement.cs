using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Interface;
using BcWin.Core.CssStyle;
using mshtml;

namespace BcBroker
{
    public partial class frmStatement : Form
    {
        public IEngineBroker Engine { get; set; }

        public frmStatement(IEngineBroker source, string username)
        {
            InitializeComponent();
            Engine = source;
            this.Text = username + " - Statement";

            if (Engine.ServerType == eServerType.Ibet)
            {
                webStatement.DocumentCompleted += OnIbetStatementDocumentCompleted;
            }

            var betlist = Engine.GetBetList();
            webStatement.DocumentText = betlist;
        }

        void OnIbetStatementDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (webStatement.Document != null)
            {
                IHTMLDocument2 currentDocument = (IHTMLDocument2)webStatement.Document.DomDocument;
                int length = currentDocument.styleSheets.length;
                IHTMLStyleSheet styleSheet = currentDocument.createStyleSheet(@"", length + 1);
                styleSheet.cssText = Css.IbetStatements;

            }
        }

        private void btnBetList_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                var betList = Engine.GetBetList();
                webStatement.DocumentText = betList;
            });
        }

        private void btnHistory_Click(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate()
            {
                var statement = Engine.GetStatement(dateHistoryPick.Value);
                webStatement.DocumentText = statement;
            });
        }
    }
}
