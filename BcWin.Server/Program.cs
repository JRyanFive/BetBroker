using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using log4net;

namespace BcWin.Server
{
    static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.ThreadException += ApplicationThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form2());
        }

        static void ApplicationThreadException(object sender, System.Threading.ThreadExceptionEventArgs t)
        {
            Logger.Error(t.Exception.Message, t.Exception);

            //DialogResult result = DialogResult.Cancel;
            //try
            //{
            //    result = ShowThreadExceptionDialog("Windows Forms Error", t.Exception);
            //}
            //catch
            //{
            //    try
            //    {
            //        MessageBox.Show("Fatal Windows Forms Error",
            //            "Fatal Windows Forms Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Stop);
            //    }
            //    finally
            //    {
                    Application.Exit();
            //    }
            //}

            //// Exits the program when the user clicks Abort. 
            //if (result == DialogResult.Abort)
            //    Application.Exit();
        }

        private static DialogResult ShowThreadExceptionDialog(string title, Exception e)
        {
            string errorMsg = "An application error occurred. Please contact the adminstrator " +
                "with the following information:\n\n";
            errorMsg = errorMsg + e.Message;// + "\n\nStack Trace:\n" + e.StackTrace;
            return MessageBox.Show(errorMsg, title, MessageBoxButtons.AbortRetryIgnore,
                MessageBoxIcon.Stop);
        }
    }
}
