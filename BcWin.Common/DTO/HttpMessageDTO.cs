using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class HttpMessageDTO
    {
        [DataMember]
        public string StatusDescription { get; set; }
        [DataMember]
        public string Result { get; set; }
        [DataMember]
        public string SetCookie { get; set; }
        [DataMember]
        public string ReponseUri { get; set; }
        [DataMember]
        public string Location { get; set; }
    }
}
