using System.Runtime.Serialization;

namespace BcWin.Common.Objects
{
    [DataContract]
    public enum eTimeMatchType
    {
        [EnumMember]
        Undefined,
        [EnumMember]
        H1,
        [EnumMember]
        H2,
        [EnumMember]
        Break
    }
}
