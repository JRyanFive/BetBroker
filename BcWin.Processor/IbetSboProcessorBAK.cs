using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.FaultDTO;
using BcWin.Common.Objects;
using BcWin.Core.EventDelegate;
using BcWin.Processor.Helper;
using BcWin.Processor.Interface;
using log4net;

namespace BcWin.Processor
{
    public class IbetSboProcessorBAK : BaseProcessor, IProcessor
    {
        public event LogScanEvent OnLogScanEvent;
        public event LogBetEvent OnLogBetEvent;
        public event ExceptionEvent OnProcessExceptionEvent;
        public event EventHandler OnUpdateCredit;

        public int CountBet { get; set; }
        public int MaxCountBet { get; set; }
        public DateTime LastBetTime { get; set; }
        public double CompareValueDifferent { get; set; }
        public eBetStakeType BetStakeType { get; set; }
        public int BetStake { get; set; }
        public int TimeOffStakeOdds { get; set; }
        public int IbetRateExchange { get; set; }
        public int SboRateExchange { get; set; }

        public int IbetMaxStakeMatch { get; set; }
        public int SboMaxStakeMatch { get; set; }
        public Dictionary<string, int> IbetBetPool { get; set; }
        public Dictionary<string, int> SboBetPool { get; set; }
        //public Dictionary<Guid, IEngine> EngineContainer { get; set; }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboProcessor));

        //private static Semaphore semaphore = new Semaphore(3, 5);

        public int StartFailCount { get; set; }

        public Dictionary<Guid, AccountDTO> AccountDic { get; set; }

        public IbetSboProcessorBAK()
        {
            AccountDic = new Dictionary<Guid, AccountDTO>();
            StartFailCount = 0;
            //EngineContainer = new Dictionary<Guid, IEngine>();
        }

        public void Initialize()
        {
            if (SboEngine.CheckLogin() && IbetEngine.CheckLogin())
            {
                if (SboEngine.Status == eServiceStatus.Unknown)
                {
                    SboBetPool = new Dictionary<string, int>();
                    SboEngine.UpdateLiveDataChange += sbobetUpdateChange_Event;
                    SboEngine.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
                    SboEngine.OnExceptionEvent += OnExceptionEvent;
                    SboEngine.InitEngine();
                }

                if (IbetEngine.Status == eServiceStatus.Unknown)
                {
                    IbetEngine.OnExceptionEvent += OnExceptionEvent;
                    IbetBetPool = new Dictionary<string, int>();
                    IbetEngine.InitEngine();
                }

                Status = eServiceStatus.Initialized;
            }
        }

        private void OnExceptionEvent(ExceptionMessage logMsg)
        {
            this.Dispose();

            if (OnProcessExceptionEvent != null)
            {
                OnProcessExceptionEvent(logMsg);
            }
        }

        public eServiceStatus Start(eScanType scanType)
        {
            CompareValueDifferent = ProcessorConfigInfo.CompareValue;

            CountBet = 1;
            MaxCountBet = ProcessorConfigInfo.MaxCountBet;
            LastBetTime = DateTime.Now;
            BetStakeType = ProcessorConfigInfo.BetStakeType;
            BetStake = ProcessorConfigInfo.BetStake;
            TimeOffStakeOdds = ProcessorConfigInfo.TimeOffStakeOdds;
            if (ProcessorConfigInfo.AccountFirst.ServerType == eServerType.Ibet)
            {
                IbetMaxStakeMatch = ProcessorConfigInfo.AccountFirst.MaxStake;
                IbetRateExchange = ProcessorConfigInfo.AccountFirst.RateExchange;
                SboMaxStakeMatch = ProcessorConfigInfo.AccountSecond.MaxStake;
                SboRateExchange = ProcessorConfigInfo.AccountSecond.RateExchange;
            }
            else
            {
                IbetMaxStakeMatch = ProcessorConfigInfo.AccountSecond.MaxStake;
                IbetRateExchange = ProcessorConfigInfo.AccountSecond.RateExchange;
                SboMaxStakeMatch = ProcessorConfigInfo.AccountFirst.MaxStake;
                SboRateExchange = ProcessorConfigInfo.AccountFirst.RateExchange;
            }

            try
            {
                Initialize();
                EngineLogger engineLogger = new EngineLogger(IbetEngine.UserName + " - " + SboEngine.UserName);
                //if (SboEngine.CheckLogin() && IbetEngine.CheckLogin())
                //{
                SboEngine.EngineLogger = engineLogger;
                SboEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                //SboEngine.InitEngine();
                SboEngine.UpdateAvailabeCredit();
                SboEngine.StartScanEngine(scanType);

                //IbetEngine.InitEngine();
                IbetEngine.EngineLogger = engineLogger;
                IbetEngine.UpdateAvailabeCredit();
                IbetEngine.StartScanEngine(scanType);

                objUpdateCreditTimer = new Timer(WaitUpdateCreditCallback, null, 60000 * 5, 60000 * 5);
                StartFailCount = 0;
                Status = eServiceStatus.Started;
                //}

                // return eServiceStatus.Unknown;
            }
            catch (Exception ex)
            {
                Logger.Error("Start Processor Fail !", ex);

                StartFailCount++;

                SboEngine.Dispose();
                SboEngine.Dispose();
                Status = eServiceStatus.Unknown;
            }

            return Status;

            //else
            //{
            //    StartServerFault startServerFault = new StartServerFault();
            //    startServerFault.ServerID = IbetEngine.Account.GuidID;
            //    startServerFault.Message = "Login Fail !";
            //    throw new FaultException<StartServerFault>(startServerFault);
            //}

        }

        public void Pause()
        {
            //SboEngine.UpdateLiveDataChange -= sbobetUpdateChange_Event;
            //SboEngine.UpdateNonLiveDataChange -= sbobetUpdateChange_Event;

            if (IbetEngine.Status == eServiceStatus.Started)
            {
                IbetEngine.PauseScan();
            }

            if (SboEngine.Status == eServiceStatus.Started)
            {
                SboEngine.PauseScan();
            }

            if (objUpdateCreditTimer != null)
            {
                objUpdateCreditTimer.Dispose();
            }

            Status = eServiceStatus.Paused;
        }

        public void Dispose()
        {
            //IbetEngine.UpdateLiveDataChange -= ibetUpdateChange_Event;
            //IbetEngine.UpdateNonLiveDataChange -= ibetUpdateChange_Event;
            SboEngine.UpdateLiveDataChange -= sbobetUpdateChange_Event;
            SboEngine.UpdateNonLiveDataChange -= sbobetUpdateChange_Event;
            SboEngine.OnExceptionEvent -= OnExceptionEvent;
            IbetEngine.OnExceptionEvent -= OnExceptionEvent;

            if (objUpdateCreditTimer != null)
            {
                objUpdateCreditTimer.Dispose();
            }

            IbetEngine.LogOff();
            SboEngine.LogOff();
            Status = eServiceStatus.Stopped;
        }

        public void PrepareBetTest(string oddId)
        {
            var match = IbetEngine.LiveMatchOddDatas.Where(l => l.OddID == oddId);
            var fmatch = match.FirstOrDefault();
            IbetEngine.PrepareBet(fmatch, eBetType.Home, true);
            IbetEngine.ConfirmBet(3);
        }


        private void sbobetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive)
        {
            //Logger.Debug("IbetSboProcessor => sbo change event");
            if (updatedData != null && updatedData.Count > 0)
            {
                if (isLive)
                {
                    lock (IbetEngine.LockLive)
                    {
                        ////Compare Live
                        CompareOdd(updatedData, IbetEngine.LiveMatchOddDatas, true);
                    }
                }
                else
                {
                    lock (IbetEngine.LockNonLive)
                    {
                        ////Compare Non Live
                        CompareOdd(updatedData, IbetEngine.NoneLiveMatchOddDatas, false);
                    }
                }
                ////
            }
        }

        //// compare from sbo (dataUpdated) to IBET (targetSource)
        public void CompareOdd(List<MatchOddDTO> dataUpdated, List<MatchOddDTO> targetSource, bool isLive)
        {
            foreach (var data in dataUpdated)
            {
                if (!DataContainer.LeaguesDenyKeywords.Any(data.LeagueName.ToUpper().Contains)
                    && (!DataContainer.MatchsDenyKeywords.Any(data.HomeTeamName.ToUpper().Contains)
                    || !DataContainer.MatchsDenyKeywords.Any(data.AwayTeamName.ToUpper().Contains)))
                {
                    MatchOddDTO matchTarget = targetSource.FirstOrDefault(m =>
                        (Utils.LevenshteinDistance(m.AwayTeamName, data.AwayTeamName) <= 4
                        || Utils.LevenshteinDistance(m.HomeTeamName, data.HomeTeamName) <= 4)
                        && !DataContainer.LeaguesDenyKeywords.Any(m.LeagueName.ToUpper().Contains)
                        && m.Odd == data.Odd && m.OddType == data.OddType);

                    if (matchTarget != null)
                    {
                        if (!IsMaxStake(data.MatchID, SboBetPool, SboMaxStakeMatch)
                            && !IsMaxStake(matchTarget.MatchID, IbetBetPool, IbetMaxStakeMatch))
                        {
                            bool isValid1 = IsValidOddPair(data.HomeOdd, matchTarget.AwayOdd, CompareValueDifferent);
                            if (isValid1)
                            {
                                //FireLogScan(data, matchTarget);
                                ProcessPrepareBet(matchTarget, eBetType.Away, data, eBetType.Home, isLive);
                                FireLogScan(data, matchTarget);
                                break;
                            }

                            bool isValid2 = IsValidOddPair(data.AwayOdd, matchTarget.HomeOdd, CompareValueDifferent);
                            if (isValid2)
                            {
                                ProcessPrepareBet(matchTarget, eBetType.Home, data, eBetType.Away, isLive);
                                FireLogScan(data, matchTarget);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public bool IsValidOddPair(float firstOdd, float secondOdd, double oddValueDifferent)
        {
            bool result = false;
            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = firstOdd + secondOdd >= 0;

                if (!result)
                {
                    result = (Math.Abs((Math.Round((firstOdd + secondOdd), 2))) <= oddValueDifferent);
                }
            }
            return result;
        }

        public void ProcessPrepareBet(MatchOddDTO ibetMatchOdd, eBetType ibetBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive)
        {
            try
            {
                if (MaxCountBet == 0 || CountBet <= MaxCountBet)
                {
                    var taskPrepareBetIbet = Task.Run(() => CallPrepareBet(ibetMatchOdd, ibetBetType, isLive));
                    var taskPrepareBetSbo = Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive));

                    if (taskPrepareBetIbet.Result != null && taskPrepareBetSbo.Result != null &&
                        !taskPrepareBetIbet.Result.HasChangeOdd && !taskPrepareBetSbo.Result.HasChangeOdd)
                    {
                        if (LastBetTime.AddSeconds(TimeOffStakeOdds) <= DateTime.Now)
                        {
                            int ibetStake;
                            int sboStake;

                            if (CaculateStake(BetStakeType,
                               taskPrepareBetIbet.Result.MatchOdd.MatchID, taskPrepareBetIbet.Result.MinBet, taskPrepareBetIbet.Result.MaxBet,
                              taskPrepareBetSbo.Result.MatchOdd.MatchID, taskPrepareBetSbo.Result.MinBet, taskPrepareBetSbo.Result.MaxBet,
                                out ibetStake, out sboStake))
                            {
                                if (CallConfirmBet(taskPrepareBetIbet.Result, ibetStake))
                                {
                                    if (CallConfirmBet(taskPrepareBetSbo.Result, sboStake))
                                    {
                                        Task.Run(
                                            () =>
                                                UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                                    taskPrepareBetSbo.Result.MatchOdd.MatchID, SboBetPool, sboStake));
                                        Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                        LastBetTime = DateTime.Now;
                                        CountBet++;
                                    }

                                    Task.Run(
                                        () =>
                                            UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                                taskPrepareBetIbet.Result.MatchOdd.MatchID, IbetBetPool, ibetStake));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                throw;
            }

        }

        private PrepareBetDTO CallPrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive)
        {
            switch (matchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.PrepareBet(matchOdd, betType, isLive);
                case eServerType.Sbo:
                    return SboEngine.PrepareBet(matchOdd, betType, isLive);
                default:
                    throw new Exception("CallPrepareBet => FAIL : Unknow matchOdd->eServerType param");
            }
        }

        private bool CallConfirmBet(PrepareBetDTO prepareBet, int stake)
        {
            switch (prepareBet.MatchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.ConfirmBet(stake);
                case eServerType.Sbo:
                    return SboEngine.ConfirmBet(stake);
                default:
                    throw new Exception("CallConfirmBet => FAIL : Unknow prepareBet->MatchOdd->eServerType param");
            }
        }

        private bool CaculateStake(eBetStakeType betStakeType, string ibetMatchId, int ibetMin, int ibetMax,
            string sboMatchId, int sboMin, int sboMax,
            out int ibetStake, out int sboStake)
        {
            bool isSuccess = false;
            switch (betStakeType)
            {
                case eBetStakeType.Ibet:
                    ibetStake = BetStake;
                    sboStake = (int)Math.Round((double)((ibetStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax
                        && sboStake >= sboMin && sboStake <= sboMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                    }
                    break;
                case eBetStakeType.Sbo:
                    sboStake = BetStake;
                    ibetStake = (int)Math.Round((double)((sboStake * IbetRateExchange) / SboRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax && sboStake >= sboMin && sboStake <= sboMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                    }
                    break;
                case eBetStakeType.Max:
                    ibetStake = ibetMax;
                    sboStake = (int)Math.Round((double)((ibetStake * SboRateExchange) / IbetRateExchange), 0);
                    if (sboStake <= sboMax && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }

                    sboStake = sboMax;
                    ibetStake = (int)Math.Round((double)((sboStake * IbetRateExchange) / SboRateExchange), 0);
                    if (ibetStake <= ibetMax && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        //break;
                    }
                    break;
                default:
                    throw new Exception("Not support eBetStakeType");
            }

            if (isSuccess)
            {
                var isIbetMax = IsMaxStake(ibetMatchId, IbetBetPool, IbetMaxStakeMatch, true, ibetStake);
                var isSboMax = IsMaxStake(sboMatchId, SboBetPool, SboMaxStakeMatch, true, sboStake);
                if ((isSboMax || isIbetMax)
                    && ibetStake > IbetEngine.AvailabeCredit && sboStake > SboEngine.AvailabeCredit)
                {
                    return false;
                }
            }

            return isSuccess;
        }

        private bool IsMaxStake(string matchOddId, Dictionary<string, int> betPool, int maxStakeMatch, bool isBetting = false, int stake = 0)
        {
            if (betPool.ContainsKey(matchOddId))
            {
                if (isBetting)
                {
                    return betPool[matchOddId] + stake > maxStakeMatch;
                }
                else
                {
                    return betPool[matchOddId] >= maxStakeMatch;
                }
            }
            return false;
        }

        private void UpdateBetPool(eServerType serverType, string matchId, Dictionary<string, int> betPool, int stake)
        {
            if (!betPool.ContainsKey(matchId))
            {
                betPool.Add(matchId, 0);
            }
            betPool[matchId] += stake;
            if (serverType == eServerType.Ibet)
            {
                IbetEngine.UpdateAvailabeCredit();
            }
            else
            {
                SboEngine.UpdateAvailabeCredit();
            }

            //if (OnLogBetEvent != null)
            //{
            //    var msg = new LogBetMessage()
            //    {
            //        HomeTeamName = match.HomeTeamName,
            //        AwayTeamName = match.AwayTeamName,
            //        OddType = match.OddType,
            //        ServerType = match.ServerType,
            //        HomeOdd = match.HomeOdd,
            //        AwayOdd = match.AwayOdd,
            //        BetStake = stake,
            //        BetType = betType
            //    };
            //    OnLogBetEvent(msg);
            //}
        }

        private void FireLogScan(MatchOddDTO matchOdd1, MatchOddDTO matchOdd2)
        {
            Task.Run(() =>
            {
                if (OnLogScanEvent != null)
                {
                    var msg = new LogScanMessage()
                    {
                        Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                        HomeTeamName = matchOdd1.HomeTeamName,
                        AwayTeamName = matchOdd1.AwayTeamName,
                        OddType = matchOdd1.OddType,
                        FirstServerType = matchOdd1.ServerType,
                        FirstHomeOdd = matchOdd1.HomeOdd,
                        FirstAwayOdd = matchOdd1.AwayOdd,

                        SecondServerType = matchOdd2.ServerType,
                        SecondHomeOdd = matchOdd2.HomeOdd,
                        SecondAwayOdd = matchOdd2.AwayOdd
                    };

                    OnLogScanEvent(msg);
                }
            });
        }

        private void WaitUpdateCreditCallback(object obj)
        {
            try
            {
                IbetEngine.UpdateAvailabeCredit();
                SboEngine.UpdateAvailabeCredit();

                if (OnUpdateCredit != null)
                {
                    OnUpdateCredit(null, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
