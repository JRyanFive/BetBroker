using System;
using System.Collections.Concurrent;
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
using BcWin.Processor.Service;
using log4net;

namespace BcWin.Processor
{
    public partial class IbetSboProcessor : BaseProcessor, IProcessor
    {
        //public event LogScanEvent OnLogScanEvent;
        //public event LogBetEvent OnLogBetEvent;
        //public event ExceptionEvent OnProcessExceptionEvent;
        public event ProcessStateChangeEvent OnProcessStateChange;

        public event PingEvent OnPingEvent;

        public int CountBet { get; set; }
        public float MinOddDefBet { get; set; }
        public int MinTimeToBet { get; set; }
        public int MaxCountBet { get; set; }
        public DateTime LastBetTime { get; set; }
        //public DateTime LastPrepareTime { get; set; }
        public DateTime LastBetIbetSuccess { get; set; }

        public bool BetAgainstIbet { get; set; }
        public int WaitingTimeRebetIbet { get; set; }
        public int AcceptMinLossIbet { get; set; }

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
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboProcessor));

        //private static Semaphore semaphore = new Semaphore(3, 5);

        public int StartFailCount { get; set; }
        public string ProcessorName { get; set; }

        public Dictionary<Guid, AccountDTO> AccountDic { get; set; }

        public IbetSboProcessor()
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
                    //SboEngine.UpdateLiveDataChange += sbobetUpdateChange_Event;
                    //SboEngine.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
                    SboEngine.OnExceptionEvent += OnExceptionEvent;
                    SboEngine.InitEngine();
                }

                if (IbetEngine.Status == eServiceStatus.Unknown)
                {
                    //IbetEngine.UpdateLiveDataChange += ibetUpdateChange_Event;
                    //IbetEngine.UpdateNonLiveDataChange += ibetUpdateChange_Event;

                    IbetEngine.OnExceptionEvent += OnExceptionEvent;
                    //IbetEngine.OnReConnectFail += IbetEngine_OnReConnectFail;
                    //IbetEngine.OnReConnectSuccess += IbetEngine_OnReConnectSuccess;
                    IbetBetPool = new Dictionary<string, int>();
                    IbetEngine.InitEngine();
                }

                Status = eServiceStatus.Initialized;
            }
        }

        private bool IsReConnect;

        private void ReStartFromIbet()
        {
            if (!IsReConnect)
            {
                IsReConnect = true;

                this.Pause();
                FireProcessStateChange(eServiceStatus.Paused, "reconnecting...");
                Logger.Info("TAM DUNG HE THONG, KET NOI LAI IBET!!!!!!!!!");
                IbetEngine.LogOff();

                ReconnectIbet();
                IsReConnect = false;
            }
        }

        private bool ReconnectIbet()
        {
            if (IbetEngine.ReLogin())
            {
                Logger.Info("Ket noi ibet thanh cong>>>>>>>>>>>>>>>>>>>>>");

                for (int i = 0; i < 4; i++)
                {
                    Logger.Info(ProcessorName + " START Service LOOP: " + i);
                    if (this.Status == eServiceStatus.Started)
                    {
                        IsReConnect = false;
                        return true;
                    }

                    if (Start(ScanType) == eServiceStatus.Started)
                    {
                        IsReConnect = false;
                        Logger.Info(ProcessorName + " >>>>>>>>>>>>>>>>>>>>>>>>>>START THANH CONG!!! ");
                        return true;
                    }

                    Thread.Sleep(15000);
                }
                if (IbetEngine.AccountStatus == eAccountStatus.Online)
                {
                    IbetEngine.LogOff();
                }

                Thread.Sleep(30000);
                //Start(ScanType);
            }

            return ReconnectIbet();
        }

        private void ReStartFromSbo()
        {
            if (!IsReConnect)
            {
                IsReConnect = true;
                this.Pause();
                FireProcessStateChange(eServiceStatus.Paused, "reconnecting...");
                Logger.Info("TAM DUNG HE THONG, KET NOI LAI SBO!!!!!!!!!");
                SboEngine.LogOff();

                ReConnectSbo();
                IsReConnect = false;
            }
        }

        public bool ReConnectSbo()
        {
            if (SboEngine.ReLogin())
            {
                Logger.Info("Ket noi sbo thanh cong>>>>>>>>>>>>>>>>>>>>>");

                for (int i = 0; i < 4; i++)
                {
                    Logger.Info(ProcessorName + " START Service LOOP: " + i);

                    if (this.Status == eServiceStatus.Started)
                    {
                        IsReConnect = false;
                        return true;
                    }

                    if (Start(ScanType) == eServiceStatus.Started)
                    {
                        IsReConnect = false;
                        Logger.Info(ProcessorName + " >>>>>>>>>>>>>>>>>>>>>>>>>>START THANH CONG!!! ");
                        return true;
                    }
                    Thread.Sleep(15000);
                }

                if (SboEngine.AccountStatus == eAccountStatus.Online)
                {
                    SboEngine.LogOff();
                }

                Thread.Sleep(30000);
            }
            return ReConnectSbo();
        }

        private void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType)
        {
            if (serverType == eServerType.Ibet)
            {
                Thread thread = new Thread(ReStartFromIbet);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
            {
                Thread thread = new Thread(ReStartFromSbo);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
        }

        public eServiceStatus Start(eScanType scanType)
        {
            CompareValueDifferent = ProcessorConfigInfo.CompareValue;
            ScanType = scanType;
            CountBet = 1;
            MaxCountBet = ProcessorConfigInfo.MaxCountBet;
            LastBetTime = DateTime.Now.AddMinutes(-10);
            LastBetIbetSuccess = DateTime.Now.AddMinutes(-10);

            BetStakeType = ProcessorConfigInfo.BetStakeType;
            BetStake = ProcessorConfigInfo.BetStake;
            TimeOffStakeOdds = ProcessorConfigInfo.TimeOffStakeOdds;
            MinOddDefBet = ProcessorConfigInfo.MinOddDefBet;
            MinTimeToBet = ProcessorConfigInfo.MinTimeToBet;

            BetAgainstIbet = ProcessorConfigInfo.RebetIbet;
            WaitingTimeRebetIbet = ProcessorConfigInfo.WaitingTimeRebetIbet;
            AcceptMinLossIbet = ProcessorConfigInfo.AcceptMinLossIbet;

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
                ProcessorName = IbetEngine.UserName + " - " + SboEngine.UserName;
                //EngineLogger engineLogger = new EngineLogger(engineName);

                //if (SboEngine.CheckLogin() && IbetEngine.CheckLogin())
                //{
                SboEngine.EngineName = ProcessorName;
                //SboEngine.EngineLogger = engineLogger;
                SboEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                //SboEngine.InitEngine();
                SboEngine.UpdateAvailabeCredit();
                SboEngine.StartScanEngine(scanType);

                //IbetEngine.InitEngine();
                IbetEngine.EngineName = ProcessorName;
                //IbetEngine.EngineLogger = engineLogger;
                IbetEngine.UpdateAvailabeCredit();
                IbetEngine.StartScanEngine(scanType);

                objUpdateCreditTimer = new Timer(WaitUpdateCreditCallback, null, 60000 * 5, 60000 * 5);
                StartFailCount = 0;

                //IbetSboDriver.OnSboScanUpdate += IbetSboDriverOnUpdateScan;
                ServerCallback.OnSboScanUpdate += OnSboUpdateScan;
                ServerCallback.OnIbetScanUpdate += OnIbetUpdateScan;
                FireProcessStateChange(eServiceStatus.Started, ProcessorName);
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
        }

        private bool isProcess;
        private void OnSboUpdateScan(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time)
        {
            if (isProcess || LastBetTime.AddSeconds(TimeOffStakeOdds) > DateTime.Now)
            {
                return;
            }

            isProcess = true;

            Task.Run(() => OnPingEvent(time, eServerType.Sbo));

            if (isLive)
            {
                lock (IbetEngine.LockLive)
                {
                    ////Compare Live
                    CompareOdd(updatedDataSbo, IbetEngine.LiveMatchOddDatas, true);
                }
            }
            else
            {
                lock (IbetEngine.LockNonLive)
                {
                    ////Compare Non Live
                    CompareOdd(updatedDataSbo, IbetEngine.NoneLiveMatchOddDatas, false);
                }
            }

            isProcess = false;
        }

        private void OnIbetUpdateScan(List<MatchOddDTO> updatedDataIbet, bool isLive, DateTime time)
        {
            if (isProcess || LastBetTime.AddSeconds(TimeOffStakeOdds) > DateTime.Now)
            {
                return;
            }

            isProcess = true;
            Task.Run(() => OnPingEvent(time, eServerType.Ibet));

            if (isLive)
            {
                lock (SboEngine.LockLive)
                {
                    ////Compare Live
                    CompareOdd(SboEngine.LiveMatchOddDatas, updatedDataIbet, true, true);
                }
            }
            else
            {
                lock (SboEngine.LockNonLive)
                {
                    ////Compare Non Live
                    CompareOdd(SboEngine.NoneLiveMatchOddDatas, updatedDataIbet, false, true);
                }
            }

            isProcess = false;
        }

        public void Pause()
        {
            ServerCallback.OnSboScanUpdate -= OnSboUpdateScan;
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
            ServerCallback.OnSboScanUpdate -= OnSboUpdateScan;
            SboEngine.OnExceptionEvent -= OnExceptionEvent;
            IbetEngine.OnExceptionEvent -= OnExceptionEvent;

            if (objUpdateCreditTimer != null)
            {
                objUpdateCreditTimer.Dispose();
            }

            IbetEngine.LogOff();
            SboEngine.LogOff();
            Status = eServiceStatus.Stopped;
            StartFailCount = 0;
        }

        //// compare from sbo (dataUpdated) to IBET (targetSource)
        public void CompareOdd(List<MatchOddDTO> dataUpdated, List<MatchOddDTO> targetSource, bool isLive, bool isIbetRaise = false)
        {
            foreach (var data in dataUpdated)
            {
                if (!DataContainer.LeaguesDenyKeywords.Any(data.LeagueName.ToUpper().Contains)
                    && (!DataContainer.MatchsDenyKeywords.Any(data.HomeTeamName.ToUpper().Contains)
                        || !DataContainer.MatchsDenyKeywords.Any(data.AwayTeamName.ToUpper().Contains)))
                {
                    MatchOddDTO ibetMatchTarget = targetSource.FirstOrDefault(m =>
                        (String.Equals(m.HomeTeamName, data.HomeTeamName,
                            StringComparison.CurrentCultureIgnoreCase)
                         ||
                         String.Equals(m.AwayTeamName, data.AwayTeamName,
                             StringComparison.CurrentCultureIgnoreCase))
                        && !DataContainer.LeaguesDenyKeywords.Any(m.LeagueName.ToUpper().Contains)
                        && m.Odd.Equals(data.Odd) && m.OddType == data.OddType);

                    if (ibetMatchTarget != null)
                    {
                        if (!IsMaxStake(ibetMatchTarget.MatchID, IbetBetPool, IbetMaxStakeMatch)
                            && IsValidTime(ibetMatchTarget.TimeType, ibetMatchTarget.Minutes))
                        {
                            bool isValid1 = IsValidOddPair(data.HomeOdd, ibetMatchTarget.AwayOdd, CompareValueDifferent);

                            if (isValid1)
                            {
                                //Logger.InfoFormat("{3} Sbo {0}, Home Odd {1}, Away Odd {2}",
                                //    data.HomeTeamName + " | " + data.AwayTeamName+" ODD: "+data.Odd,
                                //    data.HomeOdd, data.AwayOdd, ProcessorName);

                                MatchOddDTO sboMatchSource = !isIbetRaise ? GetSboMatch(data, isLive) : data;


                                if (sboMatchSource != null &&
                                    !IsMaxStake(sboMatchSource.MatchID, SboBetPool, SboMaxStakeMatch))
                                {
                                    sboMatchSource.HomeOdd = data.HomeOdd;
                                    sboMatchSource.AwayOdd = data.AwayOdd;

                                    if (ProcessPrepareBet(ibetMatchTarget, eBetType.Away, sboMatchSource, eBetType.Home,
                                        isLive))
                                    {
                                        return;
                                    }
                                }
                            }

                            bool isValid2 = IsValidOddPair(data.AwayOdd, ibetMatchTarget.HomeOdd, CompareValueDifferent);
                            if (isValid2)
                            {
                                //Logger.InfoFormat("{3} Sbo {0}, Home Odd {1}, Away Odd {2}",
                                //    data.HomeTeamName + " | " + data.AwayTeamName + " ODD: " + data.Odd,
                                //    data.HomeOdd, data.AwayOdd, ProcessorName);

                                MatchOddDTO sboMatchSource = !isIbetRaise ? GetSboMatch(data, isLive) : data;

                                if (sboMatchSource != null &&
                                    !IsMaxStake(sboMatchSource.MatchID, SboBetPool, SboMaxStakeMatch))
                                {
                                    sboMatchSource.HomeOdd = data.HomeOdd;
                                    sboMatchSource.AwayOdd = data.AwayOdd;

                                    //Logger.Debug(ProcessorName + ">>> CALL PROCESSS BET");
                                    if (ProcessPrepareBet(ibetMatchTarget, eBetType.Home, sboMatchSource, eBetType.Away,
                                        isLive))
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public MatchOddDTO GetSboMatch(MatchOddDTO sboMatchCompare, bool isLive)
        {
            MatchOddDTO sboMatchSource;
            if (isLive)
            {
                lock (SboEngine.LockLive)
                {
                    sboMatchSource = SboEngine.LiveMatchOddDatas.FirstOrDefault(m =>
                        m.MatchID == sboMatchCompare.MatchID && m.Odd.Equals(sboMatchCompare.Odd) &&
                        m.OddType == sboMatchCompare.OddType);
                }
            }
            else
            {
                lock (SboEngine.LockNonLive)
                {
                    sboMatchSource = SboEngine.NoneLiveMatchOddDatas.FirstOrDefault(m =>
                        m.MatchID == sboMatchCompare.MatchID && m.Odd.Equals(sboMatchCompare.Odd) &&
                        m.OddType == sboMatchCompare.OddType);
                }
            }

            return sboMatchSource;
        }

        public bool IsValidTime(eTimeMatchType matchTimeType, int matchMinute)
        {
            if (matchTimeType == eTimeMatchType.H1 || matchTimeType == eTimeMatchType.H2)
            {
                return matchMinute + MinTimeToBet < 45;
            }

            return true;
        }

        public bool IsValidOddPair(float firstOdd, float secondOdd, double oddValueDifferent)
        {
            if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet
                || firstOdd == 0 || secondOdd == 0)
            {
                return false;
            }

            bool result = false;
            if ((firstOdd < 0f && secondOdd < 0f) || Math.Abs(firstOdd + secondOdd) == 2)
                return true;

            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = firstOdd + secondOdd >= 0;

                if (!result)
                {
                    result = Math.Abs(firstOdd + secondOdd) <= oddValueDifferent;
                }
            }
            return result;
        }

        public bool ProcessPrepareBet(MatchOddDTO ibetMatchOdd, eBetType ibetBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive)
        {
            var taskPrepareBetIbet =
                Task.Run(() => CallPrepareBet(ibetMatchOdd, ibetBetType, isLive, sboMatchOdd.Odd));
            var taskPrepareBetSbo =
                Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive, ibetMatchOdd.Odd));

            if (taskPrepareBetIbet.Result.IsSuccess && taskPrepareBetSbo.Result != null)
            {
                if (taskPrepareBetIbet.Result.IsRunning)
                {
                    bool isGood = IsValidOddPair(taskPrepareBetIbet.Result.NewOdd,
                   taskPrepareBetSbo.Result.NewOdd, CompareValueDifferent);


                    if (isGood)
                    {
                        int ibetStake;
                        int sboStake;

                        if (CaculateStake(BetStakeType,
                            taskPrepareBetIbet.Result.MatchOdd.MatchID,
                            taskPrepareBetIbet.Result.MinBet,
                            taskPrepareBetIbet.Result.MaxBet,
                            taskPrepareBetSbo.Result.MatchOdd.MatchID,
                            taskPrepareBetSbo.Result.MinBet,
                            taskPrepareBetSbo.Result.MaxBet,
                            out ibetStake, out sboStake))
                        {
                            if (CallConfirmBet(taskPrepareBetIbet.Result, ibetStake, sboMatchOdd.Odd))
                            {
                                LastBetIbetSuccess = DateTime.Now;

                                if (CallConfirmBet(taskPrepareBetSbo.Result, sboStake, ibetMatchOdd.Odd, isLive))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(
                                                taskPrepareBetSbo.Result.MatchOdd.ServerType,
                                                taskPrepareBetSbo.Result.MatchOdd.MatchID,
                                                SboBetPool, sboStake);
                                            UpdateBetPool(
                                                taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                                taskPrepareBetIbet.Result.MatchOdd.MatchID,
                                                IbetBetPool,
                                                ibetStake);
                                        });
                                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    CountBet++;
                                    LastBetTime = DateTime.Now;
                                    return true;
                                }
                                else
                                {
                                    Logger.Info(ProcessorName + " DAT NGUOC IBET<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                    if (BetAgainstIbet)
                                    {
                                        int waitTime = WaitingTimeRebetIbet -
                                                       (DateTime.Now - LastBetIbetSuccess).Seconds;

                                        if (waitTime > 0)
                                        {
                                            Thread.Sleep(WaitingTimeRebetIbet);
                                        }

                                        if (!BetAgainstIbetRequest(taskPrepareBetIbet.Result, ibetMatchOdd, ibetBetType,
                                            isLive, sboMatchOdd.Odd, ibetStake))
                                        {
                                            Logger.Info("CALL LAN 2 BET MGUOC IBET<<<<<<<<<<<<<<<<");
                                            if (BetAgainstIbetRequest(taskPrepareBetIbet.Result, ibetMatchOdd, ibetBetType,
                                                isLive, sboMatchOdd.Odd, ibetStake))
                                            {
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            return true;
                                        }

                                        //var againstIbetPrepare = CallPrepareBet(ibetMatchOdd, GetAgainstBetType(ibetBetType), isLive, sboMatchOdd.Odd);
                                        //if (againstIbetPrepare.IsRunning)
                                        //{
                                        //    //if (againstIbetPrepare.HasScore)
                                        //    //{
                                        //    if (againstIbetPrepare.HomeScore == taskPrepareBetIbet.Result.HomeScore
                                        //    && againstIbetPrepare.AwayScore == taskPrepareBetIbet.Result.AwayScore)
                                        //    {
                                        //        if (CallConfirmBet(againstIbetPrepare, ibetStake, isLive, true))
                                        //        {
                                        //            LastBetTime = DateTime.Now;

                                        //            Task.Run(
                                        //                () =>
                                        //                    UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                        //                        taskPrepareBetIbet.Result.MatchOdd.MatchID, IbetBetPool,
                                        //                        ibetStake * 2));
                                        //            return true;
                                        //        }
                                        //    }
                                        //    //}
                                        //    Logger.Info("BET MGUOC IBET FAIL....TY SO THAY DOI....");
                                        //}

                                        Logger.Info("BET MGUOC IBET FAIL....KEO HUY....");
                                    }
                                }

                                LastBetTime = DateTime.Now;

                                Task.Run(
                                    () =>
                                        UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                            taskPrepareBetIbet.Result.MatchOdd.MatchID, IbetBetPool,
                                            ibetStake));

                                return true;
                            }
                            else
                            {
                                FireLogScan(ibetMatchOdd, sboMatchOdd);
                            }
                        }
                    }
                    else
                    {
                        FireLogScan(ibetMatchOdd, sboMatchOdd);
                    }
                }
                else
                {
                    FireLogScan(ibetMatchOdd, sboMatchOdd);
                }
            }

            return false;
        }

        private bool BetAgainstIbetRequest(PrepareBetDTO oldIbetPrepare, MatchOddDTO ibetMatchOdd, eBetType ibetBetType, bool isLive, float sboMatchOdd, int ibetStake)
        {
            var againstIbetPrepare = CallPrepareBet(ibetMatchOdd, GetAgainstBetType(ibetBetType), isLive, sboMatchOdd);
            if (againstIbetPrepare.IsRunning)
            {
                //if (againstIbetPrepare.HasScore)
                //{
                if (againstIbetPrepare.HomeScore == oldIbetPrepare.HomeScore
                && againstIbetPrepare.AwayScore == oldIbetPrepare.AwayScore)
                {
                    if (CallConfirmBet(againstIbetPrepare, ibetStake, oldIbetPrepare.OddDef, isLive, true))
                    {
                        LastBetTime = DateTime.Now;

                        Task.Run(
                            () =>
                                UpdateBetPool(oldIbetPrepare.MatchOdd.ServerType,
                                    oldIbetPrepare.MatchOdd.MatchID, IbetBetPool,
                                    ibetStake * 2));
                        return true;
                    }
                }
                //}
                Logger.Info("BET MGUOC IBET FAIL....TY SO THAY DOI....");
            }

            return false;
        }

        private eBetType GetAgainstBetType(eBetType betType)
        {
            if (betType == eBetType.Home)
            {
                return eBetType.Away;
            }

            return eBetType.Home;
        }

        private PrepareBetDTO CallPrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float oddDef)
        {
            switch (matchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.PrepareBet(matchOdd, betType, isLive, oddDef);
                case eServerType.Sbo:
                    return SboEngine.PrepareBet(matchOdd, betType, isLive, oddDef);
                default:
                    throw new Exception("CallPrepareBet => FAIL : Unknow matchOdd->eServerType param");
            }
        }

        private bool CallConfirmBet(PrepareBetDTO prepareBet, int stake, float oddDef, bool isLive = false, bool betAgainst = false)
        {
            switch (prepareBet.MatchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.ConfirmBet(stake, betAgainst);
                case eServerType.Sbo:
                    return SboEngine.ConfirmBet(stake, oddDef, isLive);
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

        private void FireLogScan(MatchOddDTO ibetMatchOddDto, MatchOddDTO sboMatchOddDto)
        {
            var msg = new LogScanMessage()
            {
                ProcessorName = this.ProcessorName,
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                HomeTeamName = ibetMatchOddDto.HomeTeamName,
                AwayTeamName = ibetMatchOddDto.AwayTeamName,
                OddType = ibetMatchOddDto.OddType,
                Odd = ibetMatchOddDto.Odd,
                FirstOddValues = ibetMatchOddDto.HomeOdd + " | " + ibetMatchOddDto.AwayOdd,
                SecondOddValues = sboMatchOddDto.HomeOdd + " | " + sboMatchOddDto.AwayOdd
            };

            Task.Run(() =>
            {
                DataContainer.LogScanQueue.Enqueue(msg);
                DataContainer.LogScanResetEvent.Set();
            });
        }

        private void WaitUpdateCreditCallback(object obj)
        {
            try
            {
                IbetEngine.UpdateAvailabeCredit();
                SboEngine.UpdateAvailabeCredit();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void FireProcessStateChange(eServiceStatus status, string desc)
        {
            Task.Run(() =>
            {
                if (OnProcessStateChange != null)
                {
                    OnProcessStateChange(status, desc);
                }
            });
        }
    }
}
