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
using BcWin.Common.Objects;
using BcWin.Processor;
using BcWin.UserControls;
namespace BcWin
{
    public partial class BcWin : Form, IDisposable
    {
        public List<string> FirstAccounts = new List<string>();
        public List<string> SecondAccounts = new List<string>();
        private bool isDispose;

        public BcWin()
        {            
            InitializeComponent();
            for (int i = 0; i < 4; i++)
            {
                AddNewOnePairAccountTab();
            }

            Thread betLogThread = new Thread(DoShowBetLog);
            betLogThread.Start();                                                                                        
        }

        private void AddNewOnePairAccountTab()
        {
            BetTabControl control = new BetTabControl(this);
            control.Dock = DockStyle.Fill;

            TabPage myTabPage = new TabPage();
            myTabPage.Controls.Add(control);
            myTabPage.Name = control.TabID.ToString();
            //myTabPage.Name = "Ibet_Sbo";
            myTabPage.Text = "Ibet-Sbo";
            tabMain.TabPages.Add(myTabPage);    
        }


        public void DoShowBetLog()
        {
            while (true)
            {
                DataContainer.LogBetResetEvent.WaitOne();
                if (isDispose)
                {
                    break;
                }

                while (DataContainer.LogBetQueue.Count > 0)
                {
                    LogBetMessage logMsg;
                    bool success = DataContainer.LogBetQueue.TryDequeue(out logMsg);

                    if (success)
                    {
                        var odd = logMsg.HomeOdd + " | " + logMsg.AwayOdd;

                        string[] row1 =
                            {
                                logMsg.Time,logMsg.EngineName, logMsg.ServerType.ToString(), logMsg.HomeTeamName, logMsg.AwayTeamName, odd,
                                logMsg.BetType.ToString(), logMsg.BetStake.ToString(), logMsg.Status.ToString()
                            };

                            this.Invoke((MethodInvoker)(() => dgvBetLog.Rows.Insert(0, row1)));
                        
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
            isDispose = true;
            DataContainer.LogBetResetEvent.Set();
            GC.SuppressFinalize(this);
        }

        private void BcWin_FormClosing(object sender, FormClosingEventArgs e)
        {
         //   if (MessageBox.Show("Bạn thật sự muốn thoát không?", "Exit",
         //MessageBoxButtons.YesNo) == DialogResult.Yes)
         //   {
         //       // Cancel the Closing event from closing the form.
         //       e.Cancel = true;
         //       Application.Exit();
         //       // Call method to save file...
         //   }
         //   else
         //   {
         //       e.Cancel = false;
         //   }
        }
    }
}
