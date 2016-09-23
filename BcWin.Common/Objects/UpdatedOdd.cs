using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class UpdatedOdd : OddDTO
    {
        public string OddID { get; set; }

        //String = "" if in sbobet
        public string MatchID { get; set; }

        public bool IsOld { get; set; }
    }
}
