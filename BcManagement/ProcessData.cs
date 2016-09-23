using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcManagement
{
    public class ProcessData
    {
        //public static SetupScanConfigDTO ScanConfig { get; set; }

        //public static SetupBetConfigDTO BetClientConfig { get; set; }

        public static List<SetupScanConfigDTO> ScanConfigs =new List<SetupScanConfigDTO>();

        public static List<SetupBetConfigDTO> BetClientConfigs = new List<SetupBetConfigDTO>();

        public static ConcurrentDictionary<string, string> ClientPingChecks =
            new ConcurrentDictionary<string, string>();
    }
}
