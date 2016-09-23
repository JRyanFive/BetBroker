using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public enum eServerType
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Ibet,
        [EnumMember]
        Sbo,
        [EnumMember]
        Pina,
          [EnumMember]
        Isn
    }
}
