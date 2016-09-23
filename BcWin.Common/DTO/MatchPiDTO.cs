using System.Collections.Generic;
using BcWin.Common.Objects;
using System;

namespace BcWin.Common.DTO
{
    public class MatchPiDTO : MatchDTO
    {
        public MatchPiDTO()
        {
            ServerType = eServerType.Pina;
        }

        public string starts { get; set; }
        public DateTime startDate { get; set; }
    }
}
