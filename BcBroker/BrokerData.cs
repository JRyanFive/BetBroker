using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcBroker
{
    public class BrokerData
    {
        public static string ScanIbetUsername { get; set; }
        public static string ScanIbetPassword { get; set; }
        public static string ScanIbetDomain { get; set; }
        public static string ScanSboUsername { get; set; }
        public static string ScanSboPassword { get; set; }
        public static string ScanSboDomain { get; set; }

        public static IList<ScanAccountInfoDTO> SboAccounts { get; set; }
        public static IList<ScanAccountInfoDTO> IbetAccounts { get; set; }
        public static IList<string> SboScanServers { get; set; }
        public static IList<string> IbetScanServers { get; set; }
    }
}
