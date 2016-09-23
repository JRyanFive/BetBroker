using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BcWin.Common.Objects;

namespace BcWin.Common.DTO
{
    [Serializable]
    [DataContract]
    public class MatchOddDTO
    {
        public MatchOddDTO() { }

        public MatchOddDTO(string matchID, string leagueName, string homeTeamName,
            string awayTeamName, eTimeMatchType timeType, int minutes, string oddID, float odd, float homeOdd,
            float awayOdd, eOddType oddType)
        {
            this.MatchID = matchID;
            this.LeagueName = leagueName;
            this.HomeTeamName = homeTeamName;
            this.AwayTeamName = awayTeamName;
            this.TimeType = timeType;
            this.Minutes = minutes;
            this.OddID = oddID;
            this.Odd = odd;
            this.HomeOdd = homeOdd;
            this.AwayOdd = awayOdd;
            this.OddType = oddType;
            this.ServerType = eServerType.Ibet;
        }

        //public MatchOddDTO(string id, string matchID, eServerType serverType, string leagueID, string leagueName, string homeTeamName,
        // string awayTeamName, eTimeMatchType timeType, int minutes)
        //{
        //    this.ID = id;
        //    this.MatchID = matchID;
        //    this.ServerType = serverType;
        //    this.LeagueID = leagueID;
        //    this.LeagueName = leagueName;
        //    this.HomeTeamName = homeTeamName;
        //    this.AwayTeamName = awayTeamName;
        //    this.TimeType = timeType;
        //    this.Minutes = minutes;
        //}

        public MatchOddDTO(string matchID, eServerType serverType, string leagueID, string leagueName, string homeTeamName,
            string awayTeamName, eTimeMatchType timeType, int minutes)
        {
            this.MatchID = matchID;
            this.ServerType = serverType;
            this.LeagueID = leagueID;
            this.LeagueName = leagueName;
            this.HomeTeamName = homeTeamName;
            this.AwayTeamName = awayTeamName;
            this.TimeType = timeType;
            this.Minutes = minutes;
            //OddChecks = new List<float>();
        }
        //public List<float> OddChecks { get; set; }

        [DataMember]
        public string ID { get; set; }
        [DataMember]
        public string MatchID { get; set; }
        [DataMember]
        public eServerType ServerType { get; set; }
        [DataMember]
        public string LeagueID { get; set; }
        [DataMember]
        public string LeagueName { get; set; }
        [DataMember]
        public eTimeMatchType TimeType { get; set; }
        [DataMember]
        public int Minutes { get; set; }
        [DataMember]
        public string HomeTeamName { get; set; }
        [DataMember]
        public string AwayTeamName { get; set; }
        [DataMember]
        public string OddID { get; set; }
        [DataMember]
        public float Odd { get; set; }
        [DataMember]
        public eOddType OddType { get; set; }
        [DataMember]
        public float HomeOdd { get; set; }
        [DataMember]
        public float AwayOdd { get; set; }

        public int HomeScore { get; set; }
        public int AwayScore { get; set; }

        public float OldOdd { get; set; }
        public float NewUpdateOdd { get; set; }



        //using for đầu tư
        public bool IsChoosedHome { get; set; }
        public bool IsDeleted { get; set; }

        public float AbsOdd
        {
            get
            {
                if (Odd >= 0)
                {
                    return Odd;
                }

                return Math.Abs(Odd);
            }
        }

        //public bool Close { get; set; }

        //public float OddDefSort
        //{
        //    get { return Math.Abs(this.Odd); }
        //}

        //[DataMember]
        //public bool IsDeleted { get; set; }
    }
}
