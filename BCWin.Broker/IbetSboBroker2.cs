using System;
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
    public class IbetSboBroker2
    {
        /// DUONG la nha chap khach, AM la khach chap nha
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboBroker2));

        public delegate void WriteTextLogEvent(string logMsg, eLogTextType type = eLogTextType.Info, eBrokerStatus status = eBrokerStatus.Unknow);
        public event WriteTextLogEvent OnWriteTextLog;

        public SboEngine SboEn = null;
        public IbetSubBrokerEngine IbetEn = null;

        public Object LockScoredMatch = new Object();
        public List<MatchBrokerDTO> ScoredMatchs = new List<MatchBrokerDTO>();

        public Object LockSellMatch = new Object();
        public List<BrokerTransaction> SellMatchs = new List<BrokerTransaction>();
        public Object LockSellTrans = new Object();
        public List<BrokerTransaction> SellTrans = new List<BrokerTransaction>();

        public Object LockBetEngine = new Object();
        public List<IEngineBroker> BetEngine = new List<IEngineBroker>();

        public eServiceStatus Status { get; set; }

        //Param must set from user
        public int TimeCheckScan { get; set; }
        public float GoalDefCheck { get; set; }
        public float OddPairCheck { get; set; }
        public float OddDevCheck { get; set; }

        public int StakeBuy { get; set; }

        public int MainRate { get; set; }

        private Random R = new Random();
        private List<int> WaitSellTimes = new List<int>() { 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000, 15000 };

        public IbetSboBroker2()
        {
            SboEn = new SboEngine();
            IbetEn = new IbetSubBrokerEngine();
        }

        public void StartScanBroker()
        {
            lock (LockSellMatch)
            {
                SellMatchs.Clear();
            }
            lock (LockScoredMatch)
            {
                ScoredMatchs = new List<MatchBrokerDTO>();
            }

            lock (LockSellTrans)
            {
                SellTrans.Clear();
            }

            IbetEn.ScoreEventChanged += IbetEnScoreEventChanged;
            SboEn.UpdateLiveDataChange += SboEnOnUpdateLiveDataChange;
            IbetEn.UpdateLiveDataChange += IbetEnOnUpdateLiveDataChange;

            StakeBuy = (StakeBuy * MainRate * 10) / 1000;

            IbetEn.StartScanEngine(eScanType.Live);
            SboEn.StartScanEngine(eScanType.Live);
            Status = eServiceStatus.Started;
            UpdateWriteTextLog(string.Format("KHỞI ĐỘNG THÀNH CÔNG!"), eLogTextType.Highlight);
        }

        public void Pause()
        {
            SboEn.UpdateLiveDataChange -= SboEnOnUpdateLiveDataChange;
            IbetEn.ScoreEventChanged -= IbetEnScoreEventChanged;
            IbetEn.UpdateLiveDataChange -= IbetEnOnUpdateLiveDataChange;
            if (IbetEn.Status == eServiceStatus.Started)
            {
                IbetEn.PauseScan();
            }

            if (SboEn.Status == eServiceStatus.Started)
            {
                SboEn.PauseScan();
            }

            Status = eServiceStatus.Paused;
        }

        public void Stop()
        {
            Pause();
            SboEn.LogOff();
            IbetEn.LogOff();

            //foreach (var engine in BetEngine)
            //{
            //    engine.Dispose();
            //}
            //BetEngine.Clear();

            Status = eServiceStatus.Stopped;
            UpdateWriteTextLog(string.Format("DỪNG QUÉT THÀNH CÔNG!"), eLogTextType.Highlight);
        }

        public void AddNewBetEngine(IEngineBroker engine)
        {
            engine.OnExceptionEvent += engine_OnExceptionEvent;
            lock (LockBetEngine)
            {
                BetEngine.Add(engine);
            }
        }

        public void RemoveBetEngine(string engineId)
        {
            lock (LockBetEngine)
            {
                var engine = BetEngine.First(b => b.EngineId == engineId);
                engine.OnExceptionEvent -= engine_OnExceptionEvent;
                engine.LogOff();
                BetEngine.Remove(engine);
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
                RemoveBetEngine(engine.EngineId);
            }
        }

        private void IbetEnOnUpdateLiveDataChange(List<MatchOddDTO> matchOddDtos, bool isLive)
        {
            try
            {
                //dung LiveMatchOddBag
                lock (LockSellMatch)
                {
                    if (SellMatchs.Any())
                    {
                        int countCheck = 0;

                        foreach (var sellTran in SellMatchs)
                        {
                            ////Kiem tra xem tran do co vao ty so chua
                            lock (LockScoredMatch)
                            {
                                var checkMatchScored = ScoredMatchs.FirstOrDefault(m =>
                                    (String.Equals(m.HomeTeamName, sellTran.BuyMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                                || String.Equals(m.AwayTeamName, sellTran.BuyMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                                && sellTran.SumScore != (m.HomeScore + m.AwayScore));

                                if (checkMatchScored != null)
                                {
                                    sellTran.FinishCheck = true;
                                    UpdateWriteTextLog(string.Format("Trận đấu {0}-{1} có tỷ số mới, hủy quét giá bán!", sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName), eLogTextType.Error, eBrokerStatus.Bet);
                                    continue;
                                }
                            }

                            if (countCheck > 0)
                            {
                                Thread.Sleep(500);
                            }

                            countCheck++;

                            if (sellTran.BuyTime.AddMinutes(sellTran.TimeCheckScan) < DateTime.Now)
                            {

                                foreach (var sellEngine in sellTran.BetSellEngine)
                                {
                                    var sellPrepare = sellEngine.PrepareBetBroker(sellTran.BuyMatchOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                    if (sellPrepare != null)
                                    {
                                        int sellPoint = ConvertMoneyToPoint(sellTran.SellMoneyAverage, sellEngine.ExchangeRate);
                                        if (sellPoint > sellPrepare.MaxBet)
                                        {
                                            sellPoint = sellPrepare.MaxBet;
                                        }

                                        sellPoint = GetGoodPoint(sellPoint);
                                        if (sellEngine.ConfirmBetBroker(sellPoint))
                                        {
                                            sellTran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);

                                            UpdateWriteTextLog(string.Format("BÁN NHANH [{3}] {0}-{1} sau {2} phút  *stake: {4}",
                                       sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                       sellTran.TimeCheckScan, sellEngine.UserName, sellPoint), eLogTextType.Warning, eBrokerStatus.Bet);

                                            lock (LockSellTrans)
                                            {
                                                sellTran.SellTimer = new System.Threading.Timer(SellCallBack, sellTran, WaitSellTimes[R.Next(WaitSellTimes.Count)], 60000);
                                                sellTran.FinishCheck = true;
                                                sellTran.FastSell = true;
                                                sellTran.SellSuccessCount++;
                                                SellTrans.Add(sellTran);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var sboMatchSell = GetValidSboMatchOdd(sellTran.BuyMatchOdd);
                                if (sboMatchSell != null)
                                {
                                    var sellPrepareSbo = SboEn.PrepareBetBroker(sboMatchSell, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                    if (sellPrepareSbo != null)
                                    {
                                        UpdateWriteTextLog(string.Format("-----> Quet {0}-{1} {2}:{3} result {4}: {5}",
                                               sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                               sellTran.BuyMatchOdd.OddType, sellTran.BuyMatchOdd.Odd, sellTran.SellBetType, sellPrepareSbo.NewOdd), eLogTextType.LowLevel);

                                        if (IsValidOddPair(sellTran.BuyOdd, sellPrepareSbo.NewOdd))
                                        {
                                            foreach (var sellEngine in sellTran.BetSellEngine)
                                            {
                                                var sellPrepare = sellEngine.PrepareBetBroker(sellTran.BuyMatchOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                                if (sellPrepare != null && IsValidOddPair(sellTran.BuyOdd, sellPrepare.NewOdd))
                                                {
                                                    int sellPoint = ConvertMoneyToPoint(sellTran.SellMoneyAverage, sellEngine.ExchangeRate);
                                                    if (sellPoint > sellPrepare.MaxBet)
                                                    {
                                                        sellPoint = sellPrepare.MaxBet;
                                                    }

                                                    sellPoint = GetGoodPoint(sellPoint);

                                                    if (sellEngine.ConfirmBetBroker(sellPoint))
                                                    {
                                                        sellTran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);
                                                        UpdateWriteTextLog(string.Format("BÁN [{6}]: {0}-{1} {2}:{3} PICK {4}: {5}  **stake: {7}",
                                            sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                            sellTran.BuyMatchOdd.OddType, sellTran.BuyMatchOdd.Odd, sellTran.SellBetType, sellPrepareSbo.NewOdd, sellEngine.UserName, sellPoint), eLogTextType.Warning, eBrokerStatus.Bet);

                                                        lock (LockSellTrans)
                                                        {
                                                            sellTran.SellTimer = new System.Threading.Timer(SellCallBack, sellTran, WaitSellTimes[R.Next(WaitSellTimes.Count)], 60000);
                                                            sellTran.FinishCheck = true;
                                                            sellTran.SellSuccessCount++;
                                                            SellTrans.Add(sellTran);
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                            continue;
                                        }
                                    }
                                }
                            }
                        }

                        SellMatchs.RemoveAll(sm => sm.FinishCheck);
                    }
                }

                lock (LockSellTrans)
                {
                    SellTrans.RemoveAll(sm => sm.FinishTransaction);
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("IbetEnOnUpdateLiveDataChange func", ex);
            }
        }

        private void SellCallBack(object obj)
        {
            BrokerTransaction tran = obj as BrokerTransaction;

            lock (LockScoredMatch)
            {
                var checkMatchScored = ScoredMatchs.FirstOrDefault(m => String.Equals(m.HomeTeamName, tran.BuyMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                          || String.Equals(m.AwayTeamName, tran.BuyMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase));

                if (checkMatchScored != null)
                {
                    tran.FinishTransaction = true;
                    tran.SellTimer.Dispose();
                    return;
                }
            }

            if (tran.SumMoneyBuy <= tran.CountSellEngine)
            {
                tran.FinishTransaction = true;
                tran.SellTimer.Dispose();
                return;
            }

            tran.BetSellEngine.Shuffle();

            foreach (var sellEngine in tran.BetSellEngine)
            {
                var sellPrepare = sellEngine.PrepareBetBroker(tran.BuyMatchOdd, tran.SellBetType, tran.BuyMatchOdd.Odd);
                if (sellPrepare != null)
                {
                    bool last = (tran.SellSuccessCount + 1) >= tran.CountSellEngine;
                    int sellMoney = last ? tran.SumMoneyBuy : tran.SellMoneyAverage;

                    int sellPoint = ConvertMoneyToPoint(sellMoney, sellEngine.ExchangeRate);
                    if (sellPoint > sellPrepare.MaxBet)
                    {
                        sellPoint = sellPrepare.MaxBet;
                    }
                    sellPoint = GetGoodPoint(sellPoint, last);

                    if (sellEngine.ConfirmBetBroker(sellPoint))
                    {
                        tran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);

                        if (tran.FastSell)
                        {
                            UpdateWriteTextLog(string.Format("BÁN NHANH [{6}] sau {8} phút  {0}-{1} {2}:{3} PICK {4}: {5}  *stake: {7}",
            tran.BuyMatchOdd.HomeTeamName, tran.BuyMatchOdd.AwayTeamName,
            tran.BuyMatchOdd.OddType, tran.BuyMatchOdd.Odd, tran.SellBetType, sellPrepare.NewOdd, sellEngine.UserName, sellPoint, tran.TimeCheckScan), eLogTextType.Warning, eBrokerStatus.Bet);
                        }
                        else
                        {
                            UpdateWriteTextLog(string.Format("BÁN [{6}]: {0}-{1} {2}:{3} PICK {4}: {5}  **stake: {7}",
            tran.BuyMatchOdd.HomeTeamName, tran.BuyMatchOdd.AwayTeamName,
            tran.BuyMatchOdd.OddType, tran.BuyMatchOdd.Odd, tran.SellBetType, sellPrepare.NewOdd, sellEngine.UserName, sellPoint), eLogTextType.Warning, eBrokerStatus.Bet);
                        }

                        tran.SellSuccessCount++;
                        if (tran.SumMoneyBuy <= tran.CountSellEngine)
                        {
                            tran.FinishTransaction = true;
                            tran.SellTimer.Dispose();
                            break;
                        }
                        else
                        {
                            tran.SellTimer.Change(WaitSellTimes[R.Next(WaitSellTimes.Count)], 60000);
                            break;
                        }
                    }
                }
            }
        }

        private bool HasGoodMatchOdd(List<MatchOddDTO> matchOddSource, out MatchOddDTO matchOdd, out eBetType buyType)
        {
            matchOdd = null;
            buyType = eBetType.Unknown;

            var count = matchOddSource.Count;
            for (int i = 1; i <= count; i++)
            {
                var matchOddCheck = matchOddSource[count - i];

                OddDTO ibetOdd;
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

                        //so sanh keo tren thap nhat la 0.70
                        bool validHighOdd;
                        if (buyType == eBetType.Home)
                        {
                            validHighOdd = Math.Abs(matchOddCheck.AwayOdd) >= 0.7;
                        }
                        else
                        {
                            validHighOdd = Math.Abs(matchOddCheck.HomeOdd) >= 0.7;
                        }

                        if (validHighOdd)
                        {
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
                lock (LockScoredMatch)
                {
                    if (ScoredMatchs.Any())
                    {
                        foreach (var matchBroker in ScoredMatchs)
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
                            if (HasGoodMatchOdd(sboMatch, out correctSboMatchOdd, out buyBetType))
                            {
                                if (BetEngine.Count < 2)
                                {
                                    //khong du bet engine thoat khoi ham
                                    matchBroker.CountTimeCheck = 20;
                                    break;
                                }

                                int buyEngineCount = BetEngine.Count(be => be.AccountStatus == eAccountStatus.Online) / 2;
                                BetEngine.Shuffle();
                                List<string> engineUsedId = new List<string>();
                                List<IEngineBroker> sellEngines = new List<IEngineBroker>();
                                float sumBuyOdd = 0f;
                                int moneyBuy = 0;
                                int mnBuy = StakeBuy / buyEngineCount;
                                for (int i = 0; i < buyEngineCount; i++)
                                {
                                    var buyEngine = BetEngine.First(be => be.AccountStatus == eAccountStatus.Online && !engineUsedId.Contains(be.EngineId));
                                    var buyPrepare = buyEngine.PrepareBetBroker(correctSboMatchOdd, buyBetType, correctSboMatchOdd.Odd);
                                    if (buyPrepare != null)
                                    {
                                        int pointBuy = ConvertMoneyToPoint(mnBuy, buyEngine.ExchangeRate);

                                        if (pointBuy > buyPrepare.MaxBet)
                                        {
                                            pointBuy = buyPrepare.MaxBet;
                                        }

                                        pointBuy = GetGoodPoint(pointBuy);
                                        float buyOdd = buyPrepare.NewOdd;

                                        if (buyEngine.ConfirmBetBroker(pointBuy))
                                        {
                                            sumBuyOdd += buyOdd;
                                            moneyBuy += ConvertPointToMoney(pointBuy, buyEngine.ExchangeRate); ;
                                            engineUsedId.Add(buyEngine.EngineId);
                                            var sellEngine = BetEngine.First(be => be.AccountStatus == eAccountStatus.Online && !engineUsedId.Contains(be.EngineId));
                                            sellEngines.Add(sellEngine);
                                            engineUsedId.Add(sellEngine.EngineId);

                                            UpdateWriteTextLog(string.Format("MUA [{6}]: {0}-{1} {2}:{3} PICK {4}: {5} **stake: {7}",
                                                 correctSboMatchOdd.HomeTeamName, correctSboMatchOdd.AwayTeamName, correctSboMatchOdd.OddType, correctSboMatchOdd.Odd, buyBetType, buyPrepare.NewOdd,
                                                 buyEngine.UserName, pointBuy), eLogTextType.Warning, eBrokerStatus.Bet);
                                        }
                                    }
                                }

                                if (sellEngines.Any())
                                {
                                    BrokerTransaction trans = new BrokerTransaction();
                                    trans.SumScore = matchBroker.HomeScore + matchBroker.AwayScore;
                                    trans.BuyMatchOdd = correctSboMatchOdd;
                                    trans.BuyTime = DateTime.Now;
                                    trans.BuyBetType = buyBetType;
                                    trans.TimeCheckScan = CalTimeCheckScan(matchBroker.TimeType, matchBroker.Minutes);
                                    trans.BetEngineIdUsed = engineUsedId;
                                    trans.BetSellEngine = sellEngines;
                                    trans.SumMoneyBuy = moneyBuy;
                                    trans.CountSellEngine = sellEngines.Count;
                                    trans.SellMoneyAverage = moneyBuy / trans.CountSellEngine;
                                    trans.BuyOdd = sumBuyOdd / trans.CountSellEngine;
                                    //trans.BuyOdd = buyBetType == eBetType.Home ?
                                    //    correctSboMatchOdd.HomeOdd : correctSboMatchOdd.AwayOdd;
                                    trans.SellBetType = GetAgainstBetType(buyBetType);
                                    lock (LockSellMatch)
                                    {
                                        SellMatchs.Add(trans);
                                    }

                                    matchBroker.CountTimeCheck = 20;
                                    continue;
                                }
                            }
                            else
                            {
                                matchBroker.CountTimeCheck++;
                                continue;
                            }
                        }

                        ScoredMatchs.RemoveAll(sm => sm.CountTimeCheck > 7);
                    }
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
                lock (SboEn.LockLive)
                {
                    if (!DataContainer.LeaguesDenyKeywords.Any(sender.LeagueName.ToUpper().Contains)
                        && (!DataContainer.MatchsDenyKeywords.Any(sender.HomeTeamName.ToUpper().Contains)
                            || !DataContainer.MatchsDenyKeywords.Any(sender.AwayTeamName.ToUpper().Contains)))
                    {

                        lock (LockScoredMatch)
                        {
                            var checkMatchScored = ScoredMatchs.FirstOrDefault(m => m.MatchID == sender.MatchID);
                            if (checkMatchScored == null)
                            {
                                ScoredMatchs.Add(sender);
                            }
                            else
                            {
                                ScoredMatchs.Remove(checkMatchScored);
                                ScoredMatchs.Add(sender);
                            }
                        }

                        UpdateWriteTextLog(string.Format("GOAL!!!! {0}-{1} score {2}-{3} ", sender.HomeTeamName,
                           sender.AwayTeamName, sender.HomeScore, sender.AwayScore), eLogTextType.Highlight);
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

        private MatchOddDTO GetValidSboMatchOdd(MatchOddDTO sboMatchOdd)
        {
            MatchOddDTO sboMatch;
            lock (SboEn.LockLive)
            {
                sboMatch = SboEn.LiveMatchOddDatas.FirstOrDefault(m =>
                     (String.Equals(m.HomeTeamName, sboMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, sboMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                    && m.Odd.Equals(sboMatchOdd.Odd)
                    && m.OddType == sboMatchOdd.OddType);
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
            if (timeType == eTimeMatchType.H1)
            {
                if (minute > 39)
                {
                    return TimeCheckScan + 14;
                }
            }
            return TimeCheckScan;
        }

        private int ConvertMoneyToPoint(int money, int rate)
        {
            return (int)Math.Round((float)(money * 1000) / rate, 0);
        }

        private int ConvertPointToMoney(int point, int rate)
        {
            return (int)Math.Round((float)(point * rate) / 1000, 0);
        }

        private int GetGoodPoint(int point, bool isLast = false)
        {
            if (isLast)
            {
                return point;
            }

            if (point > 10000)
            {
                return point - (point % 100);
            }
            else if (point > 1000)
            {
                return point - (point % 10);
            }
            //else if (point > 100)
            //{
            //    return point - (point % 10);
            //}
            else
            {
                return point;
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
