using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Engine.FrOneOne
{
    public class FrData
    {
        public static BlockingCollection<MatchDTO> LiveMatchOddBag = new BlockingCollection<MatchDTO>();
        public static BlockingCollection<MatchDTO> NonLiveMatchOddBag = new BlockingCollection<MatchDTO>();
    }
}
