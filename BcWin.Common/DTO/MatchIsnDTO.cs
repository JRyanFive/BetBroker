using System.Collections.Generic;
using BcWin.Common.Objects;
using System;

namespace BcWin.Common.DTO
{
    public class MatchIsnDTO:MatchDTO
    {
        public MatchIsnDTO()
        {
            ServerType = eServerType.Isn;
        }

        public string score { set; get; }
        public int eventPitcherId { get; set; }

    }
}
