using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    /// <summary>
    /// Contain bet config between two Ibet and sbobet (ex: exchange rate, 
    /// </summary>
    [DataContract]
    public class BetConfigInfoDTO
    {
        [DataMember]
        public bool IsMaxStake { get; set; } //Cược tối đa or not, if true, IBetStake = 0 vs SboStake = 0
        [DataMember]
        public int? IBetStake { get; set; }
        [DataMember]
        public int? SboStake { get; set; }
        [DataMember]
        public int MixOddPrice { get; set; } //Giá kèo nhỏ nhất
        [DataMember]
        public int TimeOffStakeOdds { get; set; } //Time giữa 1 lần bắt kèo
        [DataMember]
        public int TimeFrom { get; set; }// Thời gian đánh từ
        [DataMember]
        public int TimeTo { get; set; } // Thời gian đánh đến
    }
}
