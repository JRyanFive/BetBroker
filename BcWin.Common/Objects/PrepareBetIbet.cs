using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class PrepareBetIbet : PrepareBetDTO
    {
        public PrepareBetIbet()
        {}

        public PrepareBetIbet(MatchOddDTO matchOdd)
            : base(matchOdd)
        {
        }

        public BetProcessIbet BetProcessIbet { get; set; }
    }
}
