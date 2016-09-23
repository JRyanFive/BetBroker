using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin
{
    public class ClientData
    {
        public static string EndpointManage { get; set; }
        public static string EndpointRoute { get; set; }
        public static bool ProxyIbet { get; set; }
        public static string ProxyIbetEndpoint { get; set; }
        public static List<string> IpAddress { get; set; }
        public static int Tab { get; set; }
    }
}
