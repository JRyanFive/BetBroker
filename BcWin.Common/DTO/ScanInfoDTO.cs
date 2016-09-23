using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class ScanInfoDTO
    {
        public ScanInfoDTO()
        {
            Accounts = new List<ScanAccountInfoDTO>();
            ScanServers = new List<string>();
        }

        [DataMember]
        public IList<ScanAccountInfoDTO> Accounts { get; set; }

        [DataMember]
        public IList<string> ScanServers { get; set; }
    }
}
