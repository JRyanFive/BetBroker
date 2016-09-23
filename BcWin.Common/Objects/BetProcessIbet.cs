using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Common.Objects
{
    public class BetProcessIbet
    {
        //public string lblSportKind { get; set; }
        //public string lblBetKind { get; set; }
        //public string lblHome { get; set; }
        //public string lblAway { get; set; }
        //public string lblLeaguename { get; set; }
        public float lblBetKindValue { get; set; }

        public float lblOddsValue { get; set; } //ok -- odd moi

        //public string lblPlaceOddsValue { get; set; }
        //public string lblOddsColor { get; set; }
        //public string lblScore { get; set; }
        public string lblScoreValue { get; set; } //ok
        //public string hideTicketBox { get; set; }
        //public string liveBgColor { get; set; }
        public string lblChoiceClass { get; set; }
        public string lblChoiceValue { get; set; }
        public string lblMinBetValue { get; set; }
        public string hiddenStakeRequest { get; set; }

        public float hiddenOddsRequest { get; set; } //ok --- odd cu
        public string hiddenMinBet { get; set; }//OK
        public string hiddenMaxBet { get; set; }//OK

        public string hiddenBetType { get; set; }//OK
        //public string lblMaxBetValue { get; set; }
        public string lbloddsStatus { get; set; }
        //public string lbloddsStatusClass { get; set; }
        //public string lblhdp1Value { get; set; }
        //public string lblhdp2Value { get; set; }
        //public string lblLiveScoreH { get; set; }
        //public string lblLiveScoreA { get; set; }
        //public string lblReadyOnly { get; set; }
        //public string lblhedgeBox { get; set; }
        //public string hiddenliveIndicator { get; set; }
        public string hiddenSportType { get; set; } //ok
        //public string imgHorseJockey { get; set; }
        //public string chk_BettingChange { get; set; }
        public string hiddenOddsType { get; set; } //ok
        public string hiddenBetKey { get; set; }//OK
        //public string lblAutoAccept { get; set; }
        //public string lblAcceptBetterOdds { get; set; }
        //public string confirmMode { get; set; }
        //public string matchid { get; set; }
        //public string OddsStakeChg { get; set; }
        //public string HasScoreMap { get; set; }
        //public string isAutoLoad { get; set; }
        //public string singleBetData { get; set; }
    }
}
