using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class UpdatedMatch
    {
        public string MatchID { get; set; }
        public int HomeScore
        {
            get;
            set;
        }
        public int AwayScore
        {
            get;
            set;
        }
    }
}
