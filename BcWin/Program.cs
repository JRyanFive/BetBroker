using System;
using System.Threading;
using System.Windows.Forms;
using BcWin.Processor;
using log4net;

namespace BcWin
{
    static class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));
        
        [STAThread]
        static void Main()
        {
            //Application.ThreadException += ApplicationThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            CoreProcessor.InitConfig();
            Application.Run(new frmLoginFull());
        }

        static void ApplicationThreadException(object sender, ThreadExceptionEventArgs t)
        {
            Logger.Error(t.Exception.Message, t.Exception);
        }
    }
}
