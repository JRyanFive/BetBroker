using System;
using System.Xml.Serialization;

namespace BcWin.Common.DTO
{
    //[DataContract]
    public class AccountDTO
    {
        //[DataMember]
         [XmlElement("GuidID")]
        public Guid GuidID { set; get; }

        [XmlElement("DomainName")]
        public string DomainName { get; set; }

        [XmlElement("ServerType")]
        public eServerType ServerType { set; get; }

        [XmlElement("Username")]
        public string Username { set; get; }

        [XmlElement("Password")]
        public string Password { set; get; }

        [XmlElement("RateExchange")]
        public int RateExchange { get; set; } //Tỷ giá
        //[DataMember]
        //public bool IsRoundNumber { get; set; } //Số làm tròn or not
        //[DataMember]
        //public int RoundValue { get; set; } //Value làm tròn

        [XmlElement("MaxStake")]
        public int MaxStake { get; set; } //Số tiền cược tối đa cho trận
    }
}
