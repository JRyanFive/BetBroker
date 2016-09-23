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
    public partial class FrmBetList : Form
    {
        public string Web1Source { get; set; }
        public string Web2Source { get; set; }
        public FrmBetList()
        {
            InitializeComponent();
            web1.ScriptErrorsSuppressed = true;
            web2.ScriptErrorsSuppressed = true;
        }

        private void FrmBetList_Load(object sender, EventArgs e)
        {
            web1.DocumentText = Web1Source;
            web2.DocumentText = Web2Source;
        }
    }
}
