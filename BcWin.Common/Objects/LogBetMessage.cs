using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class LogBetMessage
    {
        public string EngineName { get; set; }
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public eOddType OddType { get; set; }

        public eServerType ServerType { get; set; }
        public string OddValues { get; set; }
        public float Odd { get; set; }
        public int BetStake { get; set; }
        public eBetType BetType { get; set; }
        public eBetStatusType Status { get; set; }
        public string Time { get; set; }

        public int TabCode { get; set; }

        public eServerScan ServerScan { get; set; }
    }
}
