using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Processor;

namespace BcWin.Server
{
    public class MainService
    {
       
        public static IbetEngine IbetEngineObj { get; set; }
        public static SboEngine SboEngineObj { get; set; }
        public static IbetSboProcessor IbetSboProcessorObj { get; set; }

        public static void Initialize()
        {
            IbetEngineObj = new IbetEngine();
            SboEngineObj = new SboEngine();
            IbetSboProcessorObj = new IbetSboProcessor();

            //DataContainer.IbetAccounts = new List<AccountDTO>()
            //{
            //    new AccountDTO() {Username = "ZW209902002", Password = "ABab1212"}
            //};

            //DataContainer.SboAccounts = new List<AccountDTO>()
            //{
            //    new AccountDTO() {Username = "msn99aa010", Password = "123@@Googlee"}
            //};

            //DataContainer.CookieContainer = new CookieContainer();
        }
    }
}
