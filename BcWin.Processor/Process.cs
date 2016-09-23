using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Processor.Service;

namespace BcWin.Processor
{
    public class Process
    {
        public static BlockingCollection<string> OddCheckScan = new BlockingCollection<string>();
    }
}
