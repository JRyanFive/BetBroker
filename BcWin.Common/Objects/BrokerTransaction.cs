using System;
using System.Collections.Generic;
using BcWin.Common.DTO;
using BcWin.Common.Interface;

namespace BcWin.Common.Objects
{
    public class BrokerTransaction
    {
        public MatchOddDTO BuyMatchOdd { get; set; }
        public eBetType BuyBetType { get; set; }
        public eBetType SellBetType { get; set; }
        public DateTime BuyTime { get; set; }
        public float BuyOdd { get; set; }
        public int SumScore { get; set; }

        public int TimeCheckScan { get; set; }

        public bool FinishCheck { get; set; }
        public bool FinishTransaction { get; set; }

        public eServerType ServerSell { get; set; }

        public int SumMoneyBuy { get; set; }
        public int SellMoneyAverage { get; set; }
        
        public int CountBuyEngine { get; set; }

        public int SellSuccessCount { get; set; }

        public bool FastSell { get; set; }

        public System.Threading.Timer SellTimer { get; set; }
    }
}
