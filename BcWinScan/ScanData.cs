using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Contract;
using BcWinScan.Objects;

namespace BcWinScan
{
    public class ScanData
    {
        public static bool IsDispose = false;

        public static int ScanEndpointType { get; set; }

        public static string EndpointManage { get; set; }

        public static string EndpointRoute { get; set; }

        public static bool ProxyLoginIbet { get; set; }

        public static string ProxyLoginIbetEndpoint { get; set; }

        public static bool HasFakeIpSbo { get; set; }

        public static ScanAccountDTO ScanAccount { get; set; }

        public static List<string> IpAddress { get; set; }
    }
}
