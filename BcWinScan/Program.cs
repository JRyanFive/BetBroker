using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Contract;
using BcWin.Processor;
using log4net.Repository.Hierarchy;

namespace BcWinScan
{
    static class Program
    {

        //private static void Main(string[] args)
        //{
        //    IBcScanService _manageService = null;
        //    try
        //    {
        //        NetTcpBinding b = new NetTcpBinding();
        //        b.Security.Mode = SecurityMode.None;
        //        EndpointAddress vEndPoint = new EndpointAddress("net.tcp://192.168.10.101:8899/bcscanservice");
        //        ChannelFactory<IBcScanService> cf = new ChannelFactory<IBcScanService>
        //            (b, vEndPoint);
        //        _manageService = cf.CreateChannel();
        //        var p = _manageService.Ping();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    Console.ReadLine();
        //    Console.WriteLine("START");
        //    Stopwatch stopwatch1 = new Stopwatch();

        //    while (true)
        //    {
        //        stopwatch1.Start();
        //        var aa = _manageService.Ping();
        //        stopwatch1.Stop();

        //        Console.WriteLine(stopwatch1.ElapsedMilliseconds);
        //        stopwatch1.Reset();
        //        Thread.Sleep(999);
        //    }

        //    Console.WriteLine("DONE!!");
        //    Console.ReadLine();
        //}

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            CoreProcessor.Init();
            Application.Run(new frmLogin());
        }
    }
}
