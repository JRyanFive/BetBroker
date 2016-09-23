using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
    public enum eUserType : byte
    {
        [EnumMember]
        Bet = 1,
        [EnumMember]
        Scan = 2
    }
}
