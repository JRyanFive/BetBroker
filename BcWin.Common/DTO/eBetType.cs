using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public enum eBetType
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Home,
        [EnumMember]
        Away
    }
}
