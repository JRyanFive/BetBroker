using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public class ScanAccountInfoDTO
    {
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string Password { get; set; }
    }
}
