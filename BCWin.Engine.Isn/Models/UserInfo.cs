using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCWin.Engine.Isn.Models
{
    public class Description
    {
        public string teamName { get; set; }
        public string name { get; set; }
        public int cards { get; set; }
    }

    public class Line
    {
        public bool isEmpty { get; set; }
        public int sequence { get; set; }
        public List<Selection> selections { get; set; }
        public int favoriteType { get; set; }
    }

    public class Selection
    {
        public int id { get; set; }
        public bool isEmpty { get; set; }
        public int status { get; set; }
        public string name { get; set; }
        public string selIndicator { get; set; }
        public float handicap { get; set; }
        public string odds_formatted { get; set; }
        public float odds { get; set; }
        public bool isNegativeHdp { get; set; }
    }
    public class Market
    {
        public int groupType { get; set; }
        public List<Line> lines { get; set; }
        public string htOrFt { get; set; }
    }

    public class Event
    {
        public bool isFTHdpEmpty { get; set; }
        public long startDate { get; set; }
        public string periods { get; set; }
        public int groundType { get; set; }
        public int showScoreInfo { get; set; }
        public bool isAllEmpty { get; set; }
        public string score { get; set; }
        public bool isOtherMarketsEmpty { get; set; }
        public List<Description> descriptions { get; set; }
        public string id { get; set; }
        public int eventPitcherId { get; set; }
        public bool isHTOneByTwoEmpty { get; set; }
        public bool isAnyEventInHTEXTPeriod { get; set; }
        public bool is80MinsGame { get; set; }
        public bool isFTOneByTwoEmpty { get; set; }
        public string matchName { get; set; }
        public int noOfLines { get; set; }
        public int noOfFTLines { get; set; }
        public bool isAnyEventInHTPeriod { get; set; }
        public int noOfOtherOpenedMarkets { get; set; }
        public bool isOneByTwoEmpty { get; set; }
        public bool isHdpEmpty { get; set; }
        public bool hasInplayMarkets { get; set; }
        public List<Market> markets { get; set; }
        public int noOfHTLines { get; set; }
        public bool isHTHdpEmpty { get; set; }
        public int currentBettingPeriodChronoOrder { get; set; }
        public int noOfOtherOpenedMarketsSimple { get; set; }
    }

    public class League
    {
        public bool isFTHdpEmpty { get; set; }
        public bool isAnyEventInHTPeriod { get; set; }
        public bool isOneByTwoEmpty { get; set; }
        public bool isHdpEmpty { get; set; }
        public List<Event> events { get; set; }
        public bool isAllEmpty { get; set; }
        public bool isOtherMarketsEmpty { get; set; }
        public int scoreType { get; set; }
        public string id { get; set; }
        public bool isHTHdpEmpty { get; set; }
        public string name { get; set; }
        public bool isHTOneByTwoEmpty { get; set; }
        public bool isAnyEventInHTEXTPeriod { get; set; }
        public bool isFTOneByTwoEmpty { get; set; }
    }

    public class DataOddsResponse
    {
        public int sportId { get; set; }
        public bool showDateFilter { get; set; }
        public List<League> leagues { get; set; }
        public string rbIcon { get; set; }
        public int currentOddsFormat { get; set; }
        public List<string> fiveDays { get; set; }
        public int numOfLiveEvent { get; set; }
        public int viewType { get; set; }
        public int eventSchedule { get; set; }
    }


    public class Announcement
    {
        public object date { get; set; }
        public int daysLimit { get; set; }
        public object fromDate { get; set; }
        public object toDate { get; set; }
        public List<object> messageList { get; set; }
        public bool @private { get; set; }
        public object subject { get; set; }
    }
    public class ApiResponse
    {
        public string apiKey { get; set; }
    }

    public class LoginResponse
    {
        public string memberToken { get; set; }
        public string userId { get; set; }
        public string lastRequestKey { get; set; }

    }

    public class UserInfo
    {
        public int memberId { get; set; }
        public string memberCode { get; set; }
        public string memberName { get; set; }
        public int currencyId { get; set; }
        public string currencyCode { get; set; }
        public double cashBalance { get; set; }
        public double outstanding { get; set; }
        public float betCredit { get; set; }
        public float givenCredit { get; set; }
        public long lastLogin { get; set; }
        public long lastTransaction { get; set; }
        public int oddsGroupId { get; set; }
        public int hasChangedPassword { get; set; }
        public int hasAcceptedAgreement { get; set; }
        public int hasChangedPref { get; set; }
        public int preferOddsFormat { get; set; }
        public int preferPageType { get; set; }
        public double preferDefaultStake { get; set; }
        public bool preferPlaceMaxBet { get; set; }
        public int preferOddsRefreshTime90Min { get; set; }
        public int preferOddsRefreshTimeRb { get; set; }
        public int preferOddsRefreshTimeOther { get; set; }
        public int preferOddsRefreshTimeFavourite { get; set; }
        public int preferWagerShowFormat { get; set; }
        public bool preferAcceptHigherOdds { get; set; }
        public string preferLanguageId { get; set; }
        public int preferTimeZone { get; set; }
        public bool restrictParlay { get; set; }
        public Announcement announcement { get; set; }
        public bool hasAcceptedNewTnc { get; set; }
        public string preferTimeZoneInString { get; set; }
        public string skypeId { get; set; }
    }

    //public class UserInfo
    //{

    //    public double cashBalance { get; set; }
    //    public double outstanding { get; set; }
    //    public double betCredit { get; set; }
    //    public string loginId { get; set; }
    //    public string memberToken { get; set; }
    //    public string currencyCode { get; set; }
    //    public string binaryOddsFormat { get; set; }
    //    public double givenCredit { get; set; }
    //    public string oddsGroupId { get; set; }
    //    public string memberCode { get; set; }


    //}

    public class ConfirmResponse
    {
        public int eventPitcherId { get; set; }
        public int respCode { get; set; }
        public double stake { get; set; }
        public int selectionId { get; set; }
        public string requestId { get; set; }
        public int status { get; set; }
        public string respMessage { get; set; }
        public double handicap { get; set; }
        //public DateTime betDate { get; set; }
        public double odds { get; set; }
        public long betId { get; set; }
    }


    //public class PrepareResponse // for API
    //{
    //    public int otherSportsLiveBetDelay { get; set; }
    //    public string preparedBetId { get; set; }

    //}
    public class PrepareResponse
    {
        //public string sport { get; set; }
        //public string marketType { get; set; }
        //public string marketTypeShort { get; set; }
        //public string marketTypeShortDisplayName { get; set; }
        //public string league { get; set; }
        //public string match { get; set; }
        //public int eventId { get; set; }
        //public long matchStartTime { get; set; }
        public string score { get; set; }
        //public bool hdpMarket { get; set; }
        //public int selectionId { get; set; }
        //public string selection { get; set; }
        //public int minBet { get; set; }
        //public int maxBet { get; set; }
        public float handicap { get; set; }
        public float odds { get; set; }
        //public int oddsFormat { get; set; }
        //public string displayOdds { get; set; }
        //public string accumulativeOdds { get; set; }
        public float decimalOdds { get; set; }
        //public string respMsg { get; set; }
        //public bool hasSession { get; set; }
        //public bool live { get; set; }
        //public bool showHdpSign { get; set; }
        //public bool halfTime { get; set; }
        //public int eventPitcherId { get; set; }
        //public object parlayBets { get; set; }
        //[JsonProperty("80MinsGame")]
        //public bool _80MinsGame { get; set; }
    }

    public class PrepareLimitResponse
    {
        public int minBet { get; set; }
        public int maxBet { get; set; }
        //public bool hasSession { get; set; }
    }


    public class PriceList
    {
        public int marketId { get; set; }
        public string currentBettingPeriod { get; set; }
        public int marketTypeId { get; set; }
        public int selectionId { get; set; }
        public int categoryId { get; set; }
        public string decimalOdds { get; set; }
        public string handicap { get; set; }
        public string currentRedCards { get; set; }
        public int lineOrder { get; set; }
        public string eventSchedule { get; set; }
        public string elapsedTime { get; set; }
        public int marketSelectionId { get; set; }
        public int eventId { get; set; }
        public string nativeOdds { get; set; }
        public string currentScore { get; set; }
    }

    public class OddResponse
    {
        public List<PriceList> priceList { get; set; }
        public string lastRequestKey { get; set; }
    }


    public class OneBet
    {
        public int awayPitcherId { get; set; }
        public string betStatus { get; set; }
        public string odds { get; set; }
        public string cancelReason { get; set; }
        public int marketSelectionId { get; set; }
        public int eventPitcherId { get; set; }
        public double stake { get; set; }
        public int sportId { get; set; }
        public string marketType { get; set; }
        public bool is80MinsGame { get; set; }
        public string homeTeam { get; set; }
        public int marketId { get; set; }
        public DateTime betPlacedDate { get; set; }
        public string awayTeam { get; set; }
        public int marketTypeId { get; set; }
        public string sportName { get; set; }
        public string handicap { get; set; }
        public string league { get; set; }
        public string homePitcherName { get; set; }
        public string selection { get; set; }
        public long betId { get; set; }
        public int homePitcherId { get; set; }
        public DateTime matchDate { get; set; }
        public string homeAwayScore { get; set; }
        public double estimatePayout { get; set; }
        public int matchId { get; set; }
        public string cancelRemarks { get; set; }
        public string match { get; set; }
        public string awayPitcherName { get; set; }

    }

    public class LineStatement
    {
        public object details { get; set; }
        public string status { get; set; }
        public object htScore { get; set; }
        public object ftScore { get; set; }
        public string htmlDisplay { get; set; }
    }

    public class TicketStatement
    {
        public bool parlay { get; set; }
        public object lines { get; set; }
    
        public LineStatement line { get; set; }
   //     public string detailString { get { return line.htmlDisplay; } }
        public int eventId { get; set; }
        public string eventName { get; set; }

        public int seqNo { get; set; }
        public object transTime { get; set; }
        public string formattedTransDate { get; set; }
        public object refNo { get; set; }
        public int marketTypeId { get; set; }
        public string marketTypeName { get; set; }
        public int marketGroup { get; set; }
        public double stake { get; set; }
        public double winLoss { get; set; }
        public double commission { get; set; }
        public bool unsettled { get; set; }
        public bool cancelled { get; set; }
        public bool lastSettle { get; set; }
        public int dummy { get; set; }
        public int startTime { get; set; }
        public int totalBetCount { get; set; }
        public object leagueName { get; set; }
        public int leagueId { get; set; }
        public int numOfLines { get; set; }
    }

    public class StatementResponse
    {
        public List<TicketStatement> tickets { get; set; }
        public double summaryStake { get; set; }
        public object summaryTotalBetCount { get; set; }
        public double summaryWinLoss { get; set; }
        public double summaryCommission { get; set; }
    }

    ///////////////////////////////////////
    public class GroupList
    {
        public object leagueName { get; set; }
        public string matchName { get; set; }
        public string eventId { get; set; }
        public string leagueId { get; set; }
        public int betCount { get; set; }
        public double stake { get; set; }
        public double estimatePayout { get; set; }
        public int seqNo { get; set; }
        public string stakeStr { get; set; }
        public object mrktTypeName { get; set; }
        public int startTime { get; set; }
        public int marketTypeId { get; set; }
        public string estimatePayoutStr { get; set; }
    }

    public class BetListItem
    {
        public string league { get; set; }
        public string matchName { get; set; }
        public double stake { get; set; }
        public string htmlWrappedDetails { get; set; }

    }

    public class NormalList
    {
        public string league { get; set; }
        public string match { get; set; }
        public string homeTeam { get; set; }
        public string awayTeam { get; set; }
        public string selection { get; set; }
        public string score { get; set; }
        public object betPeriod { get; set; }
        public string handicap { get; set; }
        public double odds { get; set; }
        public double stake { get; set; }
        public object displayBetDate { get; set; }
        public object displayBetTime { get; set; }
        public long betPlacedDateMillis { get; set; }
        public string betStatus { get; set; }
        public object preparedBetStatus { get; set; }
        public long betId { get; set; }
        public int seqNo { get; set; }
        public double estimatePayout { get; set; }
        public object winLoss { get; set; }
        public object commission { get; set; }
        public int sportId { get; set; }
        public string estimatePayoutStr { get; set; }
        public string stakeStr { get; set; }
        public object winLossStr { get; set; }
        public object commissionStr { get; set; }
        public bool is80MinsGame { get; set; }
        public string marketType { get; set; }
        public int marketTypeId { get; set; }
        public string oddsStr { get; set; }
        public int marketGroup { get; set; }
        public string marketGroupName { get; set; }
        public long betPlacedDate { get; set; }
        public object displayBetPlacedDate { get; set; }
        public object parlayLines { get; set; }
        public string selectionOutcome { get; set; }
        public bool ht { get; set; }
        public object matchDate { get; set; }
        public long matchDateMillis { get; set; }
        public object displayMatchDate { get; set; }
        public string htmlWrappedDetails { get; set; }
        public object wrappingParam { get; set; }
        public object parlayWrappingParams { get; set; }
        public bool parlay { get; set; }
        public bool inPlay { get; set; }
        public bool fullTime { get; set; }
        public bool halfTime { get; set; }
    }

    public class SportTypeList
    {
        public int sportTypeId { get; set; }
        public string sportTypeName { get; set; }

  //       "sportTypeList": [
  //  {
  //    "sportTypeId": -1,
  //    "sportTypeName": "All"
  //  },
  //  {
  //    "sportTypeId": 1,
  //    "sportTypeName": "Soccer"
  //  },
  //  {
  //    "sportTypeId": 2,
  //    "sportTypeName": "Basketball"
  //  },
  //  {
  //    "sportTypeId": 3,
  //    "sportTypeName": "Baseball"
  //  },
  //  {
  //    "sportTypeId": 4,
  //    "sportTypeName": "Football"
  //  }
  //],
    }

    public class BetListResponse
    {
        public string totalStake { get; set; }
        public string totalEstimatePayout { get; set; }
        public int totalBetCount { get; set; }
        public List<NormalList> normalList { get; set; }
        public List<GroupList> groupList { get; set; }
     //   public List<SportTypeList> sportTypeList { get; set; }
        public object allSprtsBetList { get; set; }
    }
}
