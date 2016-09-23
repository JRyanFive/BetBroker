using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class TransactionDTO
    {
        [DataMember]
        public bool IsLive { get; set; }

        [DataMember]
        public MatchOddDTO IbetMatchOdd { get; set; }

        [DataMember]
        public eBetType IbetBetType { get; set; }

        [DataMember]
        public MatchOddDTO SboMatchOdd { get; set; }

        [DataMember]
        public eBetType SboBetType { get; set; }

        //[DataMember]
        //public float OddsDiff { get; set; }
    }
}
