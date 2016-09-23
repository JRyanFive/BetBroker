using System.Runtime.Serialization;

namespace BcWin.Common.DTO
{
    [DataContract]
	public enum eOddType
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        HCP,
        [EnumMember]
        OU,
        [EnumMember]
        HalfHCP,
        [EnumMember]
		HalfOU
	}
}
