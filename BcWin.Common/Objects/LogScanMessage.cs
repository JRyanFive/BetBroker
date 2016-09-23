using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class LogScanMessage
    {
        public string ProcessorName { get; set; }

        public string Time { get; set; }

        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public eOddType OddType { get; set; }
        public float Odd { get; set; }

        //public eServerType FirstServerType { get; set; }
        public string FirstOddValues { get; set; }
        //public float FirstAwayOdd { get; set; }

        //public eServerType SecondServerType { get; set; }
        public string SecondOddValues { get; set; }
        //public float SecondAwayOdd { get; set; }

        public eServerScan ServerScan { get; set; }
    }
}
