using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using mshtml;

namespace BcBetRun
{
    public partial class frmBetList : Form
    {
        public frmBetList(string source, string username)
        {
            InitializeComponent();
            this.Text = "Bet List - " + username;
            if (string.IsNullOrEmpty(source))
            {
                webSboStatement.DocumentText = "<!DOCTYPE html> <html> <body>  <h1>ERROR</h1>  <p>CANNOT GET BET LIST.</p>  </body> </html>";
            }
            webSboStatement.DocumentText = source;
        }
    }
}
