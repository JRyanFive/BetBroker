using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

namespace BcManagement
{
    public partial class frmUserInfo : Form
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(frmUserInfo));
        public frmUserInfo()
        {
            InitializeComponent();
            cbUserType.SelectedIndex = 0;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            int getSbo=0;
            int getIbet=0;

            byte userType = (byte) (cbUserType.SelectedIndex + 1);

            if (userType==1)
            {
                getSbo = (int)numSboScanAcc.Value;
            }
            else
            {
                getSbo = (int)numSboScanAcc.Value;
                getIbet = (int)numIbetScanAcc.Value;
            }

            if (string.IsNullOrEmpty(txtUsername.Text) || string.IsNullOrEmpty(txtPassword.Text))
            {
                MessageBox.Show("Invalid input user information!");
                return;
            }

            try
            {
                using (var context = new WinDbEntities())
                {
                    User u = new User();
                    u.Username = txtUsername.Text;
                    u.Password = txtPassword.Text;
                    u.Type = userType;

                    var sbo = context.ScanAccounts.OrderBy(x => Guid.NewGuid()).Where(s => !s.IsBlock && s.IsFree && s.ServerType == 2).Take(getSbo);

                    foreach (var sba in sbo)
                    {
                        sba.IsFree = false;
                        u.AccScanInUses.Add(new AccScanInUse { ScanAccount = sba });
                    }

                    var ibetA = context.ScanAccounts.OrderBy(x => Guid.NewGuid()).Where(s => !s.IsBlock && s.IsFree && s.ServerType == 1).Take(getIbet);
                    foreach (var ibe in ibetA)
                    {
                        ibe.IsFree = false;
                        u.AccScanInUses.Add(new AccScanInUse() { ScanAccount = ibe });
                    }

                    context.Users.Add(u);

                    context.SaveChanges();
                }

                MessageBox.Show("DONE!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                MessageBox.Show("Add user fail!");
            }
        }
    }
}
