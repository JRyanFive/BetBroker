using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.UICustom;
using BcWin.Properties;

namespace BcWin.UserControls
{
    public partial class BetTabControl : UserControl
    {
        public Guid TabID { get; set; }

        BcWin _bcWin;
        BcWin2 _bcWin2;
        public BetTabControl(BcWin bcWin)
        {
            InitializeComponent();                       
            _bcWin = bcWin;           
            AccountConfig accountConfig = new AccountConfig(this, _bcWin);
            accountConfig.Dock = DockStyle.Fill;
            TabID = accountConfig.ID;

            BcTabPage configTab = new BcTabPage();
            configTab.Controls.Add(accountConfig);
            configTab.Name = "TabConfig";
            configTab.Text = "Cấu hình";
            tabIbetSbobet.TabPages.Add(configTab);

        }

        public BetTabControl(BcWin2 bcWin)
        {
            InitializeComponent();
            //_bcWin2 = bcWin;
            //Account account = new Account();
            //account.Dock = DockStyle.Fill;

            IbetSboManagement ibetSbo = new IbetSboManagement();
            ibetSbo.Dock = DockStyle.Fill;
            //tabIbetSbobet.Controls.Add(ibetSbo);

            BcTabPage configTab = new BcTabPage();
            configTab.Controls.Add(ibetSbo);
            configTab.Name = "TabConfig";
            configTab.Text = "Cấu hình";
            tabIbetSbobet.TabPages.Add(configTab);

        }
    }
}
