using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class BetInfoQueue
    {
        public BetInfoQueue(MatchOddDTO matchOdd, eBetType betType, bool isLive)
        {
            MatchOdd = matchOdd;
            BetType = betType;
            IsLive = isLive;
        }

        public MatchOddDTO MatchOdd { get; set; }
        public eBetType BetType{ get; set; }
        public bool IsLive{ get; set; }
    }
}
