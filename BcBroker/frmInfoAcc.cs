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
using BcWin.Core;

namespace BcBroker
{
    public partial class frmInfoAcc : Form
    {
        public string ServerType { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Domain { get; set; }
        public int Rate { get; set; }
        public string IpFake { get; set; }

        public frmInfoAcc()
        {
            InitializeComponent();

            cboAccountType.SelectedIndex = 0;
            cboDomain.DataSource = DataContainer.IbetServers;
        }

        public frmInfoAcc(string serverType, string userName, string password, string domain, string rate, string ipFake)
        {
            InitializeComponent();
            
            cboAccountType.Text = serverType;
            txtAccountName.Text = userName;
            txtPassword.Text = password;
            cboDomain.Text = domain;
            txtExchangeRate.Text = rate;
            txtIpFake.Text = ipFake;
            if (!string.IsNullOrEmpty(ipFake))
            {
                ckFakeIp.Checked = true;
            }
        }

        private void cboAccountType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboAccountType.SelectedIndex == 0)
            {
                cboDomain.DataSource = DataContainer.IbetServers;
            }
            else if (cboAccountType.SelectedIndex == 1)
            {
                cboDomain.DataSource = DataContainer.SboServers;
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            ServerType = cboAccountType.Text;
            Username = txtAccountName.Text.Trim();
            Password = txtPassword.Text.Trim();
            Rate = int.Parse(txtExchangeRate.Text);
            Domain = cboDomain.Text.Trim();
            if (ckFakeIp.Checked)
            {
                IpFake = txtIpFake.Text.Trim();
            }
            else
            {
                IpFake = "";
            }

            this.Close();
        }

        private void ckFakeIp_CheckedChanged(object sender, EventArgs e)
        {
            txtIpFake.Enabled = ckFakeIp.Checked;
        }
    }
}
