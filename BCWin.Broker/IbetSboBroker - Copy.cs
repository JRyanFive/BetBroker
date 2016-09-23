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
    public class IbetSboBroker
    {
        private string LastSboEngineIdSell { get; set; }
        private string LastIbetEngineIdSell { get; set; }

        /// DUONG la nha chap khach, AM la khach chap nha
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboBroker));

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

        public object LockBetSellEngine = new object();
        public List<IEngineBroker> BetSellEngines = new List<IEngineBroker>();

        public eServiceStatus Status { get; set; }

        //Param must set from user
        public int TimeCheckScan { get; set; }

        public int TotalAccBuy { get; set; }

        public int SumScoreMaxBuy { get; set; }
        public float GoalDefCheck { get; set; }
        public float OddPairCheck { get; set; }
        public float OddDevCheck { get; set; }

        public bool HasCheckAllLeagues { get; set; }
        public List<string> FilterLeagues { get; set; }

        public int StakeBuy { get; set; }
        public int MaxPointCheck { get; set; }

        public int MainRate { get; set; }

        private Random R = new Random();
        private List<int> WaitSellTimes = new List<int>() { 5000, 6000, 7000, 8000, 9000, 10000, 11000, 12000, 13000, 14000, 15000 };

        public IbetSboBroker()
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
            IbetEn.UpdateLiveDataChange += IbetEnOnUpdateLiveDataChange;
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
            IbetEn.UpdateLiveDataChange -= IbetEnOnUpdateLiveDataChange;
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
            else
            {
                lock (LockBetSellEngine)
                {
                    BetSellEngines.Add(engine);
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
            else
            {
                lock (LockBetSellEngine)
                {
                    var engine = BetSellEngines.First(b => b.EngineId == engineId);
                    engine.OnExceptionEvent -= engine_OnExceptionEvent;
                    engine.LogOff();
                    BetSellEngines.Remove(engine);
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

        private void IbetEnOnUpdateLiveDataChange(List<MatchOddDTO> matchOddDtos, bool isLive)
        {
            try
            {
                var sellMatchs = SellMatchs.Where(sm => !sm.FinishCheck);
                //dung LiveMatchOddBag
                if (sellMatchs.Any())
                {
                    int countCheck = 0;

                    foreach (var sellTran in sellMatchs)
                    {
                        ////Kiem tra xem tran do co vao ty so chua
                        var checkMatchScored = ScoredMatchs.Any(m =>
                                (String.Equals(m.HomeTeamName, sellTran.BuyMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, sellTran.BuyMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                            && sellTran.SumScore != (m.HomeScore + m.AwayScore));

                        if (checkMatchScored)
                        {
                            sellTran.FinishCheck = true;
                            UpdateWriteTextLog(string.Format("Trận đấu {0} vs {1} có tỷ số mới, hủy quét giá bán!", sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName), eLogTextType.Error, eBrokerStatus.GoalFailBet);
                            continue;
                        }


                        if (countCheck > 0)
                        {
                            Thread.Sleep(500);
                        }

                        countCheck++;

                        var sboMatchSell = GetValidSboMatchOdd(sellTran.BuyMatchOdd, true);
                        eServerType serverSell = eServerType.Unknown;
                        float sellOddScan = 0f;
                        if (sboMatchSell != null)
                        {
                            var sellPrepareSbo = SboEnScan.PrepareBetBroker(sboMatchSell, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);

                            if (sellPrepareSbo == null)
                            {
                                sboMatchSell = GetValidSboMatchOdd(sellTran.BuyMatchOdd, false);
                                if (sboMatchSell != null)
                                {
                                    sellPrepareSbo = SboEn.PrepareBetBroker(sboMatchSell, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                }
                            }

                            if (sellPrepareSbo != null)
                            {
                                serverSell = eServerType.Sbo;
                                sellOddScan = sellPrepareSbo.NewOdd;
                                OddDTO ibetOdd;
                                if (GetValidIbetMatchOdd(sellTran.BuyMatchOdd, out ibetOdd))
                                {
                                    var newIbetOdd = IbetEnScan.PrepareBet(ibetOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                    if (newIbetOdd != null)
                                    {
                                        if ((newIbetOdd.Value * sellPrepareSbo.NewOdd > 0 && newIbetOdd.Value > sellPrepareSbo.NewOdd)
                                            || (newIbetOdd.Value * sellPrepareSbo.NewOdd < 0 && newIbetOdd.Value < 0))
                                        {
                                            serverSell = eServerType.Ibet;
                                            sellOddScan = newIbetOdd.Value;
                                        }
                                    }
                                }
                            }

                            UpdateWriteTextLog(string.Format("-----> quét {0} vs {1} {2}:{3} result {4}: {5}",
                                             sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                             sellTran.BuyMatchOdd.OddType, sellTran.BuyMatchOdd.Odd, sellTran.SellBetType, sellOddScan), eLogTextType.LowLevel);
                        }
                        else
                        {
                            OddDTO ibetOdd;
                            if (GetValidIbetMatchOdd(sellTran.BuyMatchOdd, out ibetOdd))
                            {
                                var newIbetOdd = IbetEn.PrepareBet(ibetOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                if (newIbetOdd != null)
                                {
                                    serverSell = eServerType.Ibet;
                                    sellOddScan = newIbetOdd.Value;
                                    UpdateWriteTextLog(string.Format("-----> quét {0} vs {1} {2}:{3} result {4}: {5}",
                                               sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                               sellTran.BuyMatchOdd.OddType, sellTran.BuyMatchOdd.Odd, sellTran.SellBetType, sellOddScan), eLogTextType.LowLevel);
                                }
                            }
                        }

                        lock (LockBetSellEngine)
                        {
                            BetSellEngines.Shuffle();

                            if (sellTran.BuyTime.AddMinutes(sellTran.TimeCheckScan) < DateTime.Now)
                            {
                                foreach (var sellEngine in BetSellEngines)
                                {
                                    if (serverSell != eServerType.Unknown && sellEngine.ServerType != serverSell)
                                    {
                                        continue;
                                    }

                                    if ((serverSell == eServerType.Ibet && sellEngine.EngineId == LastIbetEngineIdSell)
                                        || (serverSell == eServerType.Sbo && sellEngine.EngineId == LastSboEngineIdSell))
                                    {
                                        continue;
                                    }

                                    var sellPrepare = sellEngine.PrepareBetBroker(sellTran.BuyMatchOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                    if (sellPrepare != null)
                                    {
                                        var odd = sellPrepare.NewOdd;
                                        int sellPoint = ConvertMoneyToPoint(sellTran.SellMoneyAverage, sellEngine.ExchangeRate);
                                        if (sellPoint > sellPrepare.MaxBet)
                                        {
                                            sellPoint = sellPrepare.MaxBet;
                                        }

                                        sellPoint = GetGoodPoint(sellPoint, sellPrepare.MaxBet, true);
                                        if (sellEngine.ConfirmBetBroker(sellPoint))
                                        {
                                            if (serverSell == eServerType.Ibet)
                                            {
                                                LastIbetEngineIdSell = sellEngine.EngineId;
                                            }
                                            else
                                            {
                                                LastSboEngineIdSell = sellEngine.EngineId;
                                            }

                                            sellTran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);

                                            UpdateWriteTextLog(string.Format("BÁN NHANH [{3}] {0} vs {1} sau {2} phút ODD: {5}  *stake: {4}",
                                       sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                       sellTran.TimeCheckScan, sellEngine.UserName, sellPoint, odd), eLogTextType.Warning, eBrokerStatus.Sell);

                                            lock (LockSellTrans)
                                            {
                                                sellTran.SellTimer = new System.Threading.Timer(SellCallBack, sellTran, WaitSellTimes[R.Next(WaitSellTimes.Count)], 60000);
                                                sellTran.FinishCheck = true;
                                                sellTran.FastSell = true;
                                                sellTran.ServerSell = serverSell;
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
                                if (serverSell != eServerType.Unknown && IsValidOddPair(sellTran.BuyOdd, sellOddScan))
                                {
                                    foreach (var sellEngine in BetSellEngines)
                                    {
                                        if (sellEngine.ServerType != serverSell)
                                        {
                                            continue;
                                        }

                                        if ((serverSell == eServerType.Ibet && sellEngine.EngineId == LastIbetEngineIdSell)
                                            || (serverSell == eServerType.Sbo && sellEngine.EngineId == LastSboEngineIdSell))
                                        {
                                            continue;
                                        }

                                        var sellPrepare = sellEngine.PrepareBetBroker(sellTran.BuyMatchOdd, sellTran.SellBetType, sellTran.BuyMatchOdd.Odd);
                                        if (sellPrepare != null)
                                        {
                                            int sellPoint = ConvertMoneyToPoint(sellTran.SellMoneyAverage, sellEngine.ExchangeRate);
                                            if (sellPoint > sellPrepare.MaxBet)
                                            {
                                                sellPoint = sellPrepare.MaxBet;
                                            }

                                            sellPoint = GetGoodPoint(sellPoint, sellPrepare.MaxBet, true);

                                            if (sellEngine.ConfirmBetBroker(sellPoint))
                                            {
                                                if (serverSell == eServerType.Ibet)
                                                {
                                                    LastIbetEngineIdSell = sellEngine.EngineId;
                                                }
                                                else
                                                {
                                                    LastSboEngineIdSell = sellEngine.EngineId;
                                                }

                                                sellTran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);
                                                UpdateWriteTextLog(string.Format("BÁN [{6}]: {0} vs {1} {2}:{3} PICK {4}: {5}  **stake: {7}",
                                    sellTran.BuyMatchOdd.HomeTeamName, sellTran.BuyMatchOdd.AwayTeamName,
                                    sellTran.BuyMatchOdd.OddType, sellTran.BuyMatchOdd.Odd, sellTran.SellBetType, sellOddScan, sellEngine.UserName, sellPoint), eLogTextType.Warning, eBrokerStatus.Sell);

                                                lock (LockSellTrans)
                                                {
                                                    sellTran.SellTimer = new System.Threading.Timer(SellCallBack, sellTran, WaitSellTimes[R.Next(WaitSellTimes.Count)], 60000);
                                                    sellTran.FinishCheck = true;
                                                    sellTran.SellSuccessCount++;
                                                    sellTran.ServerSell = serverSell;
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

            var checkMatchScored = ScoredMatchs.FirstOrDefault(m => (String.Equals(m.HomeTeamName, tran.BuyMatchOdd.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                      || String.Equals(m.AwayTeamName, tran.BuyMatchOdd.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                      && tran.SumScore != (m.HomeScore + m.AwayScore));

            if (checkMatchScored != null)
            {
                tran.FinishTransaction = true;
                tran.SellTimer.Dispose();
                return;
            }

            if (tran.SumMoneyBuy <= MaxPointCheck)
            {
                tran.FinishTransaction = true;
                tran.SellTimer.Dispose();
                return;
            }

            lock (LockBetSellEngine)
            {
                BetSellEngines.Shuffle();

                foreach (var sellEngine in BetSellEngines)
                {
                    if ((!tran.FastSell && sellEngine.ServerType != tran.ServerSell)
                        || (tran.FastSell && tran.ServerSell != eServerType.Unknown && sellEngine.ServerType != tran.ServerSell))
                    {
                        continue;
                    }

                    if ((tran.ServerSell == eServerType.Ibet && sellEngine.EngineId == LastIbetEngineIdSell)
                                            || (tran.ServerSell == eServerType.Sbo && sellEngine.EngineId == LastSboEngineIdSell))
                    {
                        continue;
                    }

                    var sellPrepare = sellEngine.PrepareBetBroker(tran.BuyMatchOdd, tran.SellBetType, tran.BuyMatchOdd.Odd);
                    if (sellPrepare != null)
                    {
                        bool last = (tran.SellSuccessCount + 1) >= tran.CountBuyEngine;
                        int sellMoney = last ? tran.SumMoneyBuy : tran.SellMoneyAverage;

                        int sellPoint = ConvertMoneyToPoint(sellMoney, sellEngine.ExchangeRate);
                        if (sellPoint > sellPrepare.MaxBet)
                        {
                            sellPoint = sellPrepare.MaxBet;
                        }
                        sellPoint = GetGoodPoint(sellPoint, sellPrepare.MaxBet, true, last);

                        if (sellEngine.ConfirmBetBroker(sellPoint))
                        {
                            if (tran.ServerSell == eServerType.Ibet)
                            {
                                LastIbetEngineIdSell = sellEngine.EngineId;
                            }
                            else
                            {
                                LastSboEngineIdSell = sellEngine.EngineId;
                            }

                            tran.SumMoneyBuy -= ConvertPointToMoney(sellPoint, sellEngine.ExchangeRate);

                            if (tran.FastSell)
                            {
                                UpdateWriteTextLog(string.Format("BÁN NHANH [{6}] sau {8} phút  {0} vs {1} {2}:{3} PICK {4}: {5}  *stake: {7}",
                tran.BuyMatchOdd.HomeTeamName, tran.BuyMatchOdd.AwayTeamName,
                tran.BuyMatchOdd.OddType, tran.BuyMatchOdd.Odd, tran.SellBetType, sellPrepare.NewOdd, sellEngine.UserName, sellPoint, tran.TimeCheckScan), eLogTextType.Warning, eBrokerStatus.Sell);
                            }
                            else
                            {
                                UpdateWriteTextLog(string.Format("BÁN [{6}]: {0} vs {1} {2}:{3} PICK {4}: {5}  **stake: {7}",
                tran.BuyMatchOdd.HomeTeamName, tran.BuyMatchOdd.AwayTeamName,
                tran.BuyMatchOdd.OddType, tran.BuyMatchOdd.Odd, tran.SellBetType, sellPrepare.NewOdd, sellEngine.UserName, sellPoint), eLogTextType.Warning, eBrokerStatus.Sell);
                            }

                            tran.SellSuccessCount++;
                            if (tran.SumMoneyBuy <= MaxPointCheck)
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
        }

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

                                float sumBuyOdd = 0f;
                                int moneyBuy = 0;
                                int buyTime = 0;
                                //so lan du tinh mua
                                int buyEngineCount = (int)(buyEngines.Count() / 1.3);
                                //if (TotalAccBuy!=0)
                                //{
                                //    buyEngineCount = TotalAccBuy;
                                //}
                                int mnBuy = StakeBuy / buyEngineCount;

                                foreach (var buyEngine in buyEngines)
                                {
                                    var buyPrepare = buyEngine.PrepareBetBroker(correctSboMatchOdd, buyBetType, correctSboMatchOdd.Odd, ibetOdd);
                                    if (buyPrepare != null)
                                    {
                                        if (buyTime >= buyEngineCount)
                                        {
                                            mnBuy = StakeBuy - moneyBuy;
                                        }

                                        int pointBuy = ConvertMoneyToPoint(mnBuy, buyEngine.ExchangeRate);

                                        if (pointBuy > buyPrepare.MaxBet)
                                        {
                                            pointBuy = buyPrepare.MaxBet;
                                        }

                                        pointBuy = GetGoodPoint(pointBuy, buyPrepare.MaxBet, false);
                                        float buyOdd = buyPrepare.NewOdd;

                                        if (buyEngine.ConfirmBetBroker(pointBuy))
                                        {
                                            buyTime++;
                                            sumBuyOdd += buyOdd;
                                            moneyBuy += ConvertPointToMoney(pointBuy, buyEngine.ExchangeRate); ;

                                            UpdateWriteTextLog(string.Format("MUA [{6}]: {0} vs {1} {2}:{3} PICK {4}: {5} **stake: {7}",
                                                 correctSboMatchOdd.HomeTeamName, correctSboMatchOdd.AwayTeamName, correctSboMatchOdd.OddType, correctSboMatchOdd.Odd, buyBetType, buyPrepare.NewOdd,
                                                 buyEngine.UserName, pointBuy), eLogTextType.Warning, eBrokerStatus.Buy);

                                            if ((moneyBuy + 10) >= StakeBuy)
                                            {
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (moneyBuy != 0)
                                {
                                    BrokerTransaction trans = new BrokerTransaction();
                                    trans.SumScore = matchBroker.HomeScore + matchBroker.AwayScore;
                                    trans.BuyMatchOdd = correctSboMatchOdd;
                                    trans.BuyTime = DateTime.Now;
                                    trans.BuyBetType = buyBetType;
                                    trans.TimeCheckScan = CalTimeCheckScan(matchBroker.TimeType, matchBroker.Minutes);

                                    trans.SumMoneyBuy = moneyBuy;
                                    trans.CountBuyEngine = buyEngineCount;
                                    trans.SellMoneyAverage = moneyBuy / buyEngineCount;
                                    trans.BuyOdd = sumBuyOdd / buyTime;

                                    trans.SellBetType = GetAgainstBetType(buyBetType);
                                    SellMatchs.Add(trans);

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

                    ScoredMatchs.Where(sm => sm.CountTimeCheck > 15 && !sm.FinishCheck).Select(c =>
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

            if (newPoint==0)
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
