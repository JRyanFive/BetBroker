using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BcWinScan
{
    public class Process
    {
        public static IbetSboDriver Driver { get; set; }
        public static float OddCompare { get; set; }
        //public static AutoResetEvent TransactionEvent = new AutoResetEvent(false);
    }
}
