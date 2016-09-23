namespace BcWin.Common.DTO
{
    public class PrepareBetDTO
    {
        public PrepareBetDTO()
        {}

        public PrepareBetDTO(bool status)
        {
            IsSuccess = status;
        }


        public PrepareBetDTO(MatchOddDTO matchOdd)
        {
            MatchOdd = matchOdd;
        }

        //public string ID
        //{
        //    get
        //    {
        //        if (MatchOdd!=null)
        //        {
        //            return MatchOdd.MatchID + MatchOdd.OddID;
        //        }

        //        return null;
        //    }
        //}

        public bool IsRunning { get; set; }
        public bool IsSuccess { get; set; }

        public MatchOddDTO MatchOdd { get; set; }
        public eBetType BetType { get; set; }
        public float OddDef { get; set; }

        public int MinBet { get; set; }
        public int MaxBet { get; set; }

        public bool IsLive { get; set; }

        public bool HasScore { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }

        public bool HasChangeOdd { get; set; }
        public float NewOdd { get; set; }

        #region [ using for ISN]
        public string score { get; set; }
        public OddDTO Odd { get; set; }
        #endregion

        #region [using for Pina]
        public object PendingTicket { get; set; }

        #endregion
    }
}
