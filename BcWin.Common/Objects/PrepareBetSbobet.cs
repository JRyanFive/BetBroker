using BcWin.Common.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Common.Objects
{
    public class PrepareBetSbobet : PrepareBetDTO
    {
        public PrepareBetSbobet(MatchOddDTO matchOdd)
            : base(matchOdd)
        {
        }

        //public BetProcessSbobet BetProcessSbobet { get; set; }

        public string BetCount { get; set; }
        public string BetPage { get; set; }

        //Need to generate to bet
        public DateTime TimeProcessBet { get; set; }
    }
}
