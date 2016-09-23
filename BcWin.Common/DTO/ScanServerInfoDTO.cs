using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class ScanServerInfoDTO
    {
        [DataMember]
        public int SboScanOnline { set; get; }

        [DataMember]
        public int SboLive { set; get; }

        [DataMember]
        public int SboToday { set; get; }

        [DataMember]
        public int IbetScanOnline { set; get; }

        [DataMember]
        public int IbetLive { set; get; }

        [DataMember]
        public int IbetToday { set; get; }
    }
}
