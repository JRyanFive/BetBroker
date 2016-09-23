using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BcWin.Common.Objects;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class SetupBetConfigDTO
    {
        public SetupBetConfigDTO()
        {
            SboScanAccounts = new List<ScanAccountInfoDTO>();
        }

        [DataMember]
        public string EndpointRoute { get; set; }
        [DataMember]
        public int TimeScanLiveIbet { get; set; }
        [DataMember]
        public int TimeScanLiveSbo { get; set; }
        [DataMember]
        public int TimeScanNonLiveIbet { get; set; }
        [DataMember]
        public int TimeScanNonLiveSbo { get; set; }
        [DataMember]
        public bool HasLocalScan { get; set; }
        [DataMember]
        public IList<string> SboScanServers { get; set; }

        [DataMember]
        public IList<ScanAccountInfoDTO> SboScanAccounts { get; set; }
    }
}
