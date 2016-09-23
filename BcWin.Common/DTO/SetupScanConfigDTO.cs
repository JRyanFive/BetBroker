using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BcWin.Common.Objects;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class SetupScanConfigDTO
    {
        [DataMember]
        public string EndpointRoute { get; set; }
        [DataMember]
        public int TimeScanLiveIbet { get; set; }
        [DataMember]
        public int TimeScanLiveSbo { get; set; }
        [DataMember]
        public IList<string> IbetScanServers { get; set; }
        [DataMember]
        public IList<string> SboScanServers { get; set; }
        [DataMember]
        public bool HasFakeIpSbo { get; set; }
    }
}
