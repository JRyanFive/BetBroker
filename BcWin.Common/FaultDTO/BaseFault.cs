using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Common.FaultDTO
{
    [DataContract]
    public class BaseFault
    {
        [DataMember]
        public string Message { get; set; }
    }
}
