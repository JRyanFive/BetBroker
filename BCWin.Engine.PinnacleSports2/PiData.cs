using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Engine.PinnacleSports2
{
    public class PiData
    {
        //public static BlockingCollection<MatchPiDTO> LiveMatchOddBag = new BlockingCollection<MatchPiDTO>();
        //public static BlockingCollection<MatchPiDTO> NonLiveMatchOddBag = new BlockingCollection<MatchPiDTO>();

        public static BlockingCollection<MatchPiDTO> LiveMatchOddBag { get; set; }
        public BlockingCollection<MatchPiDTO> NonLiveMatchOddBag { get; set; }
    }
}
