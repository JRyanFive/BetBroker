using System.Collections.Generic;
using BcWin.Common.Objects;
using System;
using System.Threading;

namespace BcWin.Common.DTO
{
    public class MatchBrokerDTO : MatchDTO
    {
        public int OldHomeScore { get; set; }
        public int OldAwayScore { get; set; }

        public bool IsChangedScore { get; set; }
        
        public int CountTimeCheck { get; set; }
        public bool FinishCheck { get; set; }
    }
}
