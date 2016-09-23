using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class ValidDataSboIbet
    {
        public string IbetMatchId { get; set; }
        public string SboMatchId { get; set; }

        public string IbetOddId { get; set; }
        public string SboOddId { get; set; }

        public float HomeSbo { get; set; }
        public float AwaySbo { get; set; }

        public float HomeIbet { get; set; }
        public float AwayIbet { get; set; }
    }
}
