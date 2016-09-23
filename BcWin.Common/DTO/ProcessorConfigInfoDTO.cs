using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BcWin.Common.DTO
{
    /// <summary>
    /// Contain VPS Host Name, IP, and one pair of bet and sbo
    /// </summary>
    [Serializable()]   
    public class ProcessorConfigInfoDTO
    {
        [XmlElement("Code")]
        public int Code { get; set; } // Code of Tab
        //[DataMember]

        [XmlElement("Market")]
        public string Market { get; set; }

        [XmlElement("AccountFirst")]
        public AccountDTO AccountFirst { get; set; }

        [XmlElement("AccountSecond")]
        public AccountDTO AccountSecond { get; set; }

        [XmlElement("BetStakeType")]
        public eBetStakeType BetStakeType { get; set; }

        [XmlElement("BetStake")]
        public string BetStake { get; set; }

        [XmlElement("TimeOffStakeOdds")]
        public int TimeOffStakeOdds { get; set; } //Time giữa 1 lần bắt kèo, tinh theo giay

        [XmlElement("CompareValue")]
        public double CompareValue { get; set; } //lech gia bao nhieu, vi du: 0,01 || 0,02

        [XmlElement("MaxCountBet")]
        public int MaxCountBet { get; set; } //so lan bat keo toi da

        [XmlElement("RebetSbo")]
        public int RebetSbo { get; set; }

        [XmlElement("RebetIbet")]
        public bool RebetIbet { get; set; }

        [XmlElement("WaitingTimeRebetIbet")]
        public int WaitingTimeRebetIbet { get; set; }

        [XmlElement("AcceptMinLossIbet")]
        public int AcceptMinLossIbet { get; set; }

        [XmlElement("MinOddDefBet")]
        public float MinOddDefBet { get; set; }

        [XmlElement("MinTimeToBet")]
        public int MinTimeToBet { get; set; }

        //[DataMember]
        //public int MinOddPrice { get; set; } //Giá kèo nhỏ nhất

        //[DataMember]
        //public int TimeFrom { get; set; }// Thời gian đánh từ
        //[DataMember]
        //public int TimeTo { get; set; } // Thời gian đánh đến
    }
    
    [XmlRoot("Root", Namespace = "bcwin", IsNullable = true)]
    [Serializable()] 
    public class ProcessorConfigs {
       
        [XmlArray("Lists"), XmlArrayItem(ElementName = "ProcessorConfig", Type = typeof(ProcessorConfigInfoDTO))]
        public List<ProcessorConfigInfoDTO> ListProcessorConfig { get; set; }

        public ProcessorConfigs()
        {
            this.ListProcessorConfig = new List<ProcessorConfigInfoDTO>();
        }
    }

   
}
