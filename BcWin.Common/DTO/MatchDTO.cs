using System.Collections.Generic;
using BcWin.Common.Objects;
using System;

namespace BcWin.Common.DTO
{
    public class MatchDTO
    {
        public string ID { get; set; }
        public string MatchID { get; set; }

        public eServerType ServerType { get; set; }

        public eTimeMatchType TimeType { get; set; }
        public int Minutes { get; set; }

        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public string LeagueID { get; set; }
        public string LeagueName { get; set; }

        public int HomeScore { get; set; }
        public int AwayScore { get; set; }

        public List<OddDTO> Odds { get; set; }

        public bool IsDeleted { get; set; }
    }
}
