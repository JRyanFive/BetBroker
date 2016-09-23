using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class BetAgainstTransaction
    {
        //public int IdCode { get; set; }
        public MatchOddDTO MatchOdd { get; set; }
        public eBetType BetType { get; set; }
        public bool IsLive { get; set; }
        public int Stake { get; set; }
        public float OddCompare { get; set; }

        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
    }
}
