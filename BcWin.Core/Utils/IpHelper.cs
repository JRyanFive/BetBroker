using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Core.Utils
{
    public class IpHelper
    {
        public static string GetRandomIp()
        {
            Random r = new Random();
            return string.Format("{0}.{1}.{2}.{3}", r.Next(1, 199), r.Next(1, 253), r.Next(1, 253), r.Next(1, 253));
        }
    }
}
