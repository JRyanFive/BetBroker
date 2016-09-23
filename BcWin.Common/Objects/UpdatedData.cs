using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class UpdatedData
    {
        public UpdatedData()
        {
            DeleteMatchIDs = new List<string>();
            UpdatedOdds=new List<UpdatedOdd>();
            NewMatchs = new List<MatchDTO>();
        }

        public List<string> DeleteMatchIDs { get; set; }

        //Get live old, both old mactch and new macth
        public List<UpdatedOdd> UpdatedOdds { get; set; }
        //public List<UpdatedMatch> UpdatedMatchs { get; set; }
        //Get live match, both old mactch and new macth
        public List<MatchDTO> NewMatchs { get; set; }
    }
 
}
