using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class ScanAccountDTO
    {
        public ScanAccountDTO()
        {
            SboAccounts = new List<ScanAccountInfoDTO>();
            IbetAccounts = new List<ScanAccountInfoDTO>();
        }

        [DataMember]
        public IList<ScanAccountInfoDTO> SboAccounts { get; set; }
        [DataMember]
        public IList<ScanAccountInfoDTO> IbetAccounts { get; set; }
    }
}
