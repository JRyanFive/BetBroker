using BcWin.UserControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BcWin
{
    public partial class BcWin2 : Form
    {
        public BcWin2()
        {
            InitializeComponent();
            for (int i = 0; i < 4; i++)
            {
                AddNewOnePairAccountTab();
            }
        }

        private void AddNewOnePairAccountTab()
        {
            //BetTabControl control = new BetTabControl(this);
            //control.Dock = DockStyle.Fill;

            IbetSboManagement ibetSbo = new IbetSboManagement();
            ibetSbo.Dock = DockStyle.Fill;

            TabPage myTabPage = new TabPage();
            myTabPage.Controls.Add(ibetSbo);
           // myTabPage.Name = ibetSbo.TabID.ToString();
            //myTabPage.Name = "Ibet_Sbo";
            myTabPage.Text = "Ibet-Sbo";
            tabMain.TabPages.Add(myTabPage);
        }
    }
}
