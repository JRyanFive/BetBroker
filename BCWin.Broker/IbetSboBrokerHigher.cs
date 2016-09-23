using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Interface;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Utils;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;
using BCWin.Metadata;
using log4net;

namespace BCWin.Broker
{
    public class IbetSboBrokerHigher
    {
        private string LastSboEngineIdSell { get; set; }
        private string LastIbetEngineIdSell { get; set; }

        /// DUONG la nha chap khach, AM la khach chap nha
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboBrokerHigher));

        public delegate void WriteTextLogEvent(string logMsg, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow);
        public event WriteTextLogEvent OnWriteTextLog;

        public SboEngine SboEn = null;
        public IbetSubBrokerEngine IbetEn = null;
        public SboEngine SboEnScan = null;
        public IbetSubBrokerEngine IbetEnScan = null;

        //public object LockScoredMatch = new object();
        public BlockingCollection<MatchBrokerDTO> ScoredMatchs = new BlockingCollection<MatchBrokerDTO>();

        //public object LockSellMatch = new object();
        public BlockingCollection<BrokerTransaction> SellMatchs = new BlockingCollection<BrokerTransaction>();
        public object LockSellTrans = new object();
        public List<BrokerTransaction> SellTrans = new List<BrokerTransaction>();

        public object LockBetBuyEngine = new object();
        public List<IEngineBroker> BetBuyEngines = new List<IEngineBroker>();

        public eServiceStatus Status { get; set; }

        //Param must set from user
        public int TimeCheckScan { get; set; }

        public int TotalAccSell { get; set; }

        public int SumScoreMaxBuy { get; set; }
        public float GoalDefCheck { get; set; }
        public float OddPairCheck { get; set; }
        public float OddDevCheck { get; set; }

        public bool PickUnder { get; set; }

        public bool BuySbo { get; set; }

        public bool HasCheckAllLeagues { get; set; }
        public List<string> FilterLeagues { get; set; }

        public int StakeBuy { get; set; }
        public int MaxPointCheck { get; set; }

        public int MainRate { get; set; }

        private Random R = new Random();
        private List<int> WaitSellTimes = new List<int>() { 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000, 15000 };

        public IbetSboBrokerHigher()
        {
            SboEn = new SboEngine();
            SboEnScan = new SboEngine();
            IbetEn = new IbetSubBrokerEngine();
            IbetEnScan = new IbetSubBrokerEngine();
        }

        public void StartScanBroker()
        {
            SellMatchs = new BlockingCollection<BrokerTransaction>();

            ScoredMatchs = new BlockingCollection<MatchBrokerDTO>();

            lock (LockSellTrans)
            {
                SellTrans.Clear();
            }

            IbetEn.ScoreEventChanged += IbetEnScoreEventChanged;
            SboEn.UpdateLiveDataChange += SboEnOnUpdateLiveDataChange;
            SboEn.OnExceptionEvent += SboScan_OnExceptionEvent;
            IbetEn.OnExceptionEvent += IbetScan_OnExceptionEvent;

            SboEnScan.OnExceptionEvent += SboScan_OnExceptionEvent;
            IbetEnScan.OnExceptionEvent += IbetScan_OnExceptionEvent;

            int inputStakeBuy = StakeBuy;
            StakeBuy = (inputStakeBuy * MainRate * 10) / 1000;
            MaxPointCheck = (50 * StakeBuy) / inputStakeBuy;


            SboEnScan.StartScanEngine(eScanType.Live);
            IbetEnScan.StartScanEngine(eScanType.Live);

            IbetEn.StartScanEngine(eScanType.Live);
            SboEn.StartScanEngine(eScanType.Live);
            Status = eServiceStatus.Started;
            UpdateWriteTextLog(string.Format("KHỞI ĐỘNG THÀNH CÔNG!"), eLogTextType.Highlight);
        }

        public void Pause()
        {
            SboEn.UpdateLiveDataChange -= SboEnOnUpdateLiveDataChange;
            IbetEn.ScoreEventChanged -= IbetEnScoreEventChanged;
            SboEn.OnExceptionEvent -= SboScan_OnExceptionEvent;
            IbetEn.OnExceptionEvent -= IbetScan_OnExceptionEvent;

            SboEnScan.OnExceptionEvent -= SboScan_OnExceptionEvent;
            IbetEnScan.OnExceptionEvent -= IbetScan_OnExceptionEvent;

            if (IbetEn.Status == eServiceStatus.Started)
            {
                IbetEn.PauseScan();
            }

            if (SboEn.Status == eServiceStatus.Started)
            {
                SboEn.PauseScan();
            }

            if (SboEnScan.Status == eServiceStatus.Started)
            {
                SboEnScan.PauseScan();
            }

            if (IbetEnScan.Status == eServiceStatus.Started)
            {
                IbetEnScan.PauseScan();
            }

            Status = eServiceStatus.Paused;
        }

        public void Stop()
        {
            Pause();
            SboEn.LogOff();
            IbetEn.LogOff();
            SboEnScan.LogOff();
            IbetEnScan.LogOff();

            Status = eServiceStatus.Stopped;
            UpdateWriteTextLog(string.Format("DỪNG QUÉT THÀNH CÔNG!"), eLogTextType.Highlight);
        }

        public void AddNewBetEngine(IEngineBroker engine, bool isBuy)
        {
            engine.OnExceptionEvent += engine_OnExceptionEvent;
            engine.IsBuyBroker = isBuy;
            if (isBuy)
            {
                lock (LockBetBuyEngine)
                {
                    BetBuyEngines.Add(engine);
                }
            }
        }

        public void RemoveBetEngine(string engineId, bool isBuy)
        {
            if (isBuy)
            {
                lock (LockBetBuyEngine)
                {
                    var engine = BetBuyEngines.First(b => b.EngineId == engineId);
                    engine.OnExceptionEvent -= engine_OnExceptionEvent;
                    engine.LogOff();
                    BetBuyEngines.Remove(engine);
                }
            }
        }

        private void SboScan_OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            SboEngine engine = obj as SboEngine;
            engine.OnExceptionEvent -= SboScan_OnExceptionEvent;
            engine.LogOff();

            while (engine.AccountStatus == eAccountStatus.Offline)
            {
                if (engine.ReLogin())
                {
                    engine.StartScanEngine(eScanType.Live);
                    engine.OnExceptionEvent += SboScan_OnExceptionEvent;
                    break;
                }

                Thread.Sleep(15000);
            }
        }

        private void IbetScan_OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            IbetSubBrokerEngine engine = obj as IbetSubBrokerEngine;
            engine.OnExceptionEvent -= IbetScan_OnExceptionEvent;
            engine.LogOff();

            while (engine.AccountStatus == eAccountStatus.Offline)
            {
                if (engine.ReLogin())
                {
                    engine.StartScanEngine(eScanType.Live);
                    engine.OnExceptionEvent += IbetScan_OnExceptionEvent;
                    break;
                }

                Thread.Sleep(15000);
            }
        }

        void engine_OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            var engine = obj as IEngineBroker;
            engine.OnExceptionEvent -= engine_OnExceptionEvent;
            engine.LogOff();
            for (int i = 0; i < 5; i++)
            {
                if (engine.ReLogin())
                {
                    engine.StartScanEngine(eScanType.Live);
                    engine.OnExceptionEvent += engine_OnExceptionEvent;
                    break;
                }

                Thread.Sleep(15000);
            }

            if (engine.AccountStatus == eAccountStatus.Offline)
            {
                RemoveBetEngine(engine.EngineId, engine.IsBuyBroker);
            }
        }

        //private bool HasGoodMatchOdd(List<MatchOddDTO> matchOddSource, out MatchOddDTO matchOdd, out eBetType buyType, out eServerType serverBuy, out OddDTO ibetOdd)
        //{
        //    buyType = eBetType.Unknown;
        //    serverBuy = eServerType.Unknown;
        //    matchOdd = null;
        //    ibetOdd = null;

        //    if (!BuySbo & matchOddSource.Any())
        //    {
        //        var sboMatchOdd = matchOddSource.First();
        //        var ibetMatch = IbetEn.LiveMatchOddBag.FirstOrDefault(m =>
        //            !m.IsDeleted &&
        //            (String.Equals(m.HomeTeamName, sboMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
        //             ||
        //             String.Equals(m.AwayTeamName, sboMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase)));

        //        if (ibetMatch != null)
        //        {
        //            foreach (var oddDto in ibetMatch.Odds)
        //            {

        //            }
        //        }
        //    }


        //    var count = matchOddSource.Count;
        //    for (int i = 1; i <= count; i++)
        //    {
        //        var matchOddCheck = matchOddSource[count - i];

        //        if (matchOddCheck.AbsOdd >= GoalDefCheck)
        //        {
        //            float checkOddDef = 1;
        //            if (matchOddCheck.HomeOdd > 0 && matchOddCheck.AwayOdd > 0)
        //            {
        //                checkOddDef = (1 - matchOddCheck.HomeOdd) + (1 - matchOddCheck.AwayOdd);
        //            }
        //            else
        //            {
        //                checkOddDef = Math.Abs(matchOddCheck.HomeOdd + matchOddCheck.AwayOdd);
        //            }

        //            //so sanh nho hon hoac = 10 xu
        //            if (checkOddDef <= OddDevCheck)
        //            {
        //                buyType = matchOddCheck.Odd > 0 ? eBetType.Away : eBetType.Home;
        //                if (!PickUnder)
        //                {
        //                    buyType = buyType == eBetType.Home ? eBetType.Away : eBetType.Home;
        //                }
        //                //so sanh keo tren thap nhat la 0.70
        //                bool validHighOdd;


        //                if (buyType == eBetType.Home)
        //                {
        //                    validHighOdd = Math.Abs(matchOddCheck.AwayOdd) > 0.75;
        //                }
        //                else
        //                {
        //                    validHighOdd = Math.Abs(matchOddCheck.AwayOdd) > 0.75;
        //                }

        //                if (validHighOdd)
        //                {
        //                    serverBuy = eServerType.Sbo;
        //                    return true;
        //                }
        //            }
        //        }
        //    }

        //    return false;
        //}


        private bool HasGoodMatchOdd(List<MatchOddDTO> matchOddSource, out MatchOddDTO matchOdd, out eBetType buyType, out eServerType serverBuy, out OddDTO ibetOdd)
        {
            matchOdd = null;
            buyType = eBetType.Unknown;
            serverBuy = eServerType.Unknown;
            ibetOdd = null;
            var count = matchOddSource.Count;
            for (int i = 1; i <= count; i++)
            {
                var matchOddCheck = matchOddSource[count - i];

                if (GetValidIbetMatchOdd(matchOddCheck, out ibetOdd) && matchOddCheck.AbsOdd >= GoalDefCheck)
                {
                    float checkOddDef = 1;
                    if (matchOddCheck.HomeOdd > 0 && matchOddCheck.AwayOdd > 0)
                    {
                        checkOddDef = (1 - matchOddCheck.HomeOdd) + (1 - matchOddCheck.AwayOdd);
                    }
                    else
                    {
                        checkOddDef = Math.Abs(matchOddCheck.HomeOdd + matchOddCheck.AwayOdd);
                    }

                    //so sanh nho hon hoac = 10 xu
                    if (checkOddDef <= OddDevCheck)
                    {
                        buyType = matchOddCheck.Odd > 0 ? eBetType.Away : eBetType.Home;
                        if (!PickUnder)
                        {
                            buyType = buyType == eBetType.Home ? eBetType.Away : eBetType.Home;
                        }

                        //so sanh keo tren thap nhat la 0.70
                        bool validHighOdd;
                        float ibetOddCheck = 0f;
                        float sboOddCheck = 0f;

                        if (buyType == eBetType.Home)
                        {
                            validHighOdd = Math.Abs(matchOddCheck.AwayOdd) > 0.75;
                            ibetOddCheck = ibetOdd.HomeOdd;
                            sboOddCheck = matchOddCheck.HomeOdd;
                        }
                        else
                        {
                            validHighOdd = Math.Abs(matchOddCheck.AwayOdd) > 0.75;
                            ibetOddCheck = ibetOdd.AwayOdd;
                            sboOddCheck = matchOddCheck.AwayOdd;
                        }

                        if (validHighOdd)
                        {
                            if (ibetOddCheck > sboOddCheck)
                            {
                                serverBuy = eServerType.Ibet;
                            }
                            else
                            {
                                serverBuy = eServerType.Sbo;
                            }

                            matchOdd = matchOddCheck;
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private void SboEnOnUpdateLiveDataChange(List<MatchOddDTO> matchOddDtos, bool isLive, int type)
        {
            if (matchOddDtos == null)
            {
                return;
            }

            try
            {
                var scoredMatch = ScoredMatchs.Where(sm => !sm.FinishCheck);
                if (scoredMatch.Any())
                {
                    foreach (var matchBroker in scoredMatch)
                    {
                        var sboMatch = matchOddDtos.Where(m =>
                            (String.Equals(m.HomeTeamName, matchBroker.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, matchBroker.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                            && (m.OddType == eOddType.HCP))
                            .OrderBy(sm => sm.AbsOdd).ToList();

                        if (!sboMatch.Any(s => s.AbsOdd >= GoalDefCheck))
                        {
                            matchBroker.CountTimeCheck++;
                            continue;
                        }

                        MatchOddDTO correctSboMatchOdd;
                        eBetType buyBetType;
                        eServerType serverBuy;
                        OddDTO ibetOdd;
                        if (HasGoodMatchOdd(sboMatch, out correctSboMatchOdd, out buyBetType, out serverBuy, out ibetOdd))
                        {
                            lock (LockBetBuyEngine)
                            {
                                if (BetBuyEngines.Count < 2)
                                {
                                    //khong du bet engine thoat khoi ham
                                    matchBroker.CountTimeCheck = 30;
                                    break;
                                }

                                var buyEngines = BetBuyEngines.Where(be => be.AccountStatus == eAccountStatus.Online
                                                             && be.ServerType == serverBuy);
                                BetBuyEngines.Shuffle();

                                float buyOdd = 0f;
                                int moneyBuy = 0;

                                foreach (var buyEngine in buyEngines)
                                {
                                    var buyPrepare = buyEngine.PrepareBetBroker(correctSboMatchOdd, buyBetType, correctSboMatchOdd.Odd, ibetOdd);
                                    if (buyPrepare != null)
                                    {
                                        int pointBuy = ConvertMoneyToPoint(StakeBuy, buyEngine.ExchangeRate);

                                        if (pointBuy > buyPrepare.MaxBet)
                                        {
                                            pointBuy = buyPrepare.MaxBet;
                                        }

                                        pointBuy = GetGoodPoint(pointBuy, buyPrepare.MaxBet, false);
                                        float bOdd = buyPrepare.NewOdd;

                                        if (buyEngine.ConfirmBetBroker(pointBuy))
                                        {
                                            buyOdd = bOdd;
                                            moneyBuy = ConvertPointToMoney(pointBuy, buyEngine.ExchangeRate); ;

                                            UpdateWriteTextLog(string.Format("MUA [{6}]: {0} vs {1} {2}:{3} PICK {4}: {5} **stake: {7}",
                                                 correctSboMatchOdd.HomeTeamName, correctSboMatchOdd.AwayTeamName, correctSboMatchOdd.OddType, correctSboMatchOdd.Odd, buyBetType, buyPrepare.NewOdd,
                                                 buyEngine.UserName, pointBuy), eLogTextType.Warning, eBrokerStatus.Buy);

                                            break;
                                        }
                                    }
                                }

                                if (moneyBuy != 0)
                                {
                                    //BrokerTransaction trans = new BrokerTransaction();
                                    //trans.SumScore = matchBroker.HomeScore + matchBroker.AwayScore;
                                    //trans.BuyMatchOdd = correctSboMatchOdd;
                                    //trans.BuyTime = DateTime.Now;
                                    //trans.BuyBetType = buyBetType;
                                    //trans.TimeCheckScan = CalTimeCheckScan(matchBroker.TimeType, matchBroker.Minutes);

                                    //trans.SumMoneyBuy = moneyBuy;
                                    //trans.CountBuyEngine = TotalAccSell;
                                    //trans.SellMoneyAverage = moneyBuy / TotalAccSell;
                                    //trans.BuyOdd = buyOdd;

                                    //trans.SellBetType = GetAgainstBetType(buyBetType);
                                    //SellMatchs.Add(trans);

                                    matchBroker.CountTimeCheck = 30;
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            matchBroker.CountTimeCheck++;
                            continue;
                        }
                    }

                    ScoredMatchs.Where(sm => sm.CountTimeCheck > 20 && !sm.FinishCheck).Select(c =>
                    {
                        c.FinishCheck = true;
                        return c;
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("SboEnOnUpdateLiveDataChange func", ex);
            }
        }

        void IbetEnScoreEventChanged(MatchBrokerDTO sender, EventArgs e)
        {
            try
            {
                bool isValidLang = false;

                if (HasCheckAllLeagues)
                {
                    isValidLang = true;
                }
                else
                {
                    isValidLang = FilterLeagues.Any(sender.LeagueName.ToUpper().Contains);
                }

                if (isValidLang && !DataContainer.LeaguesDenyKeywords.Any(sender.LeagueName.ToUpper().Contains)
                       && (!DataContainer.MatchsDenyKeywords.Any(sender.HomeTeamName.ToUpper().Contains)
                           || !DataContainer.MatchsDenyKeywords.Any(sender.AwayTeamName.ToUpper().Contains)))
                {
                    UpdateWriteTextLog(string.Format("GOAL!!!! {0} vs {1} score {2}-{3} ", sender.HomeTeamName,
                      sender.AwayTeamName, sender.HomeScore, sender.AwayScore), eLogTextType.Highlight);

                    var checkMatchScored = ScoredMatchs.FirstOrDefault(m => String.Equals(m.HomeTeamName, sender.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, sender.AwayTeamName, StringComparison.CurrentCultureIgnoreCase));
                    if (checkMatchScored == null)
                    {
                        ScoredMatchs.Add(sender);
                    }
                    else
                    {
                        checkMatchScored.HomeScore = sender.HomeScore;
                        checkMatchScored.AwayScore = sender.AwayScore;
                        if ((SumScoreMaxBuy != 0 && (sender.HomeScore + sender.AwayScore) <= SumScoreMaxBuy) || SumScoreMaxBuy == 0)
                        {
                            checkMatchScored.CountTimeCheck = 0;
                            checkMatchScored.FinishCheck = false;
                        }
                        else
                        {
                            checkMatchScored.FinishCheck = true;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private eBetType GetAgainstBetType(eBetType betType)
        {
            if (betType == eBetType.Home)
            {
                return eBetType.Away;
            }

            return eBetType.Home;
        }

        private bool GetValidIbetMatchOdd(MatchOddDTO sboMatchOdd, out OddDTO oddObj)
        {
            bool isValid = false;
            oddObj = null;
            var ibetMatch = IbetEn.LiveMatchOddBag.FirstOrDefault(m =>
                !m.IsDeleted &&
                (String.Equals(m.HomeTeamName, sboMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                || String.Equals(m.AwayTeamName, sboMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase)));

            if (ibetMatch != null)
            {
                var odd = ibetMatch.Odds.FirstOrDefault(o => !o.IsDeleted
                                                             && o.OddType == sboMatchOdd.OddType &&
                                                             o.Odd.Equals(sboMatchOdd.Odd));
                if (odd != null)
                {
                    oddObj = odd;
                    isValid = true;
                }
            }
            return isValid;
        }

        private MatchOddDTO GetValidSboMatchOdd(MatchOddDTO sboMatchOdd, bool fromScan)
        {
            MatchOddDTO sboMatch;
            if (fromScan)
            {
                lock (SboEnScan.LockLive)
                {
                    sboMatch = SboEnScan.LiveMatchOddDatas.FirstOrDefault(m =>
                         (String.Equals(m.HomeTeamName, sboMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                                || String.Equals(m.AwayTeamName, sboMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                        && m.Odd.Equals(sboMatchOdd.Odd)
                        && m.OddType == sboMatchOdd.OddType);
                }
            }
            else
            {
                lock (SboEn.LockLive)
                {
                    sboMatch = SboEn.LiveMatchOddDatas.FirstOrDefault(m =>
                         (String.Equals(m.HomeTeamName, sboMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                                || String.Equals(m.AwayTeamName, sboMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                        && m.Odd.Equals(sboMatchOdd.Odd)
                        && m.OddType == sboMatchOdd.OddType);
                }
            }

            return sboMatch;
        }

        private bool IsValidOddPair(float firstOdd, float secondOdd)
        {
            if (firstOdd < 0f && secondOdd < 0f)
                return (firstOdd + 1) + (secondOdd + 1) >= OddPairCheck;

            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                return firstOdd + secondOdd >= OddPairCheck;
            }
            return false;
        }

        private int CalTimeCheckScan(eTimeMatchType timeType, int minute)
        {
            if (timeType == eTimeMatchType.H1 && minute + TimeCheckScan > 47)
            {
                //int remainMinute = 45 - minute;
                return TimeCheckScan + 17;
            }
            return TimeCheckScan;
        }

        public int ConvertMoneyToPoint(int money, int rate)
        {
            return (int)Math.Round((float)(money * 1000) / rate, 0);
        }

        public int ConvertPointToMoney(int point, int rate)
        {
            return (int)Math.Round((float)(point * rate) / 1000, 0);
        }

        public int GetGoodPoint(int point, int maxBet, bool fromSell, bool isLast = false)
        {
            int newPoint = 0;

            if (isLast)
            {
                if (fromSell)
                {
                    newPoint = point.Round(50);

                    if (newPoint == 0)
                    {
                        return point;
                    }
                    return newPoint;
                }

                return point;
            }

            if (point > 10000)
            {
                newPoint = point - (point % 100);
            }
            else if (point > 1000)
            {
                newPoint = point - (point % 10);
            }
            //else if (point > 100)
            //{
            //    return point - (point % 10);
            //}
            if (fromSell)
            {
                newPoint = point.Round(100);

                if (newPoint == 0)
                {
                    return point;
                }
            }

            if (newPoint == 0)
            {
                newPoint = point;
            }

            if (newPoint > maxBet)
            {
                return maxBet;
            }
            else
            {
                return newPoint;
            }
        }

        private void UpdateWriteTextLog(string log, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow)
        {
            if (OnWriteTextLog != null)
            {
                Task.Factory.StartNew(() =>
                {
                    OnWriteTextLog(log, type, status);
                });
            }
        }
    }
}
