using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Common.FaultDTO;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Core.Helper;
using BcWin.Processor.Interface;
using BcWin.Processor.ScanDriver;
using BcWin.Processor.Service;
using log4net;

namespace BcWin.Processor
{
    public partial class IbetSboProcessor : BaseProcessor, IProcessor
    {
        public ConcurrentQueue<List<TransactionDTO>> TransactionQueue { get; set; }

        public event ProcessStateChangeEvent OnProcessStateChange;

        public event PingEvent OnPingEvent;

        public ServerCallback ServerCallback { get; set; }
        public int CountBet { get; set; }
        public float MinOddDefBet { get; set; }
        public int MinTimeToBet { get; set; }
        public int MaxCountBet { get; set; }
        public DateTime LastBetTime { get; set; }
        //public DateTime LastPrepareTime { get; set; }
        public DateTime LastBetIbetSuccess { get; set; }

        public bool BetAgainstIbet { get; set; }
        //public int WaitingTimeRebetIbet { get; set; }
        public int AcceptMinLossIbet { get; set; }

        public double CompareValueDifferent { get; set; }
        public eBetStakeType BetStakeType { get; set; }
        public string Stake { get; set; }
        public List<int> BetStake { get; set; }
        public int TimeOffStakeOdds { get; set; }
        public int IbetRateExchange { get; set; }
        public int SboRateExchange { get; set; }

        public int IbetMaxStakeMatch { get; set; }
        public int SboMaxStakeMatch { get; set; }
        public Dictionary<string, int> IbetBetPool { get; set; }
        public Dictionary<string, int> SboBetPool { get; set; }
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboProcessor));
        private IbetSboDriver _driver;

        public int StartFailCount { get; set; }
        public string ProcessorName { get; set; }

        public Dictionary<Guid, AccountDTO> AccountDic { get; set; }

        public IbetSboProcessor(IbetSboDriver driver, ServerCallback serverCallback)
        {
            AccountDic = new Dictionary<Guid, AccountDTO>();
            StartFailCount = 0;
            //EngineContainer = new Dictionary<Guid, IEngine>();
            _driver = driver;
            ServerCallback = serverCallback;
            //TransactionQueue = new ConcurrentQueue<List<TransactionDTO>>();

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
                    //PiEngine.UpdateLiveDataChange += ibetUpdateChange_Event;
                    //PiEngine.UpdateNonLiveDataChange += ibetUpdateChange_Event;

                    IbetEngine.OnExceptionEvent += OnExceptionEvent;
                    //PiEngine.OnReConnectFail += IbetEngine_OnReConnectFail;
                    //PiEngine.OnReConnectSuccess += IbetEngine_OnReConnectSuccess;
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

                for (int i = 0; i < 15; i++)
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
                    if (i > 3)
                    {
                        Thread.Sleep(60000 * 3);
                    }
                }
                if (IbetEngine.AccountStatus == eAccountStatus.Online)
                {
                    IbetEngine.LogOff();
                }

                Thread.Sleep(30000);
                //Start(ScanType);
            }
            return false;
            //return ReconnectIbet();
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

                for (int i = 0; i < 15; i++)
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
                    if (i > 3)
                    {
                        Thread.Sleep(60000 * 3);
                    }
                }

                if (SboEngine.AccountStatus == eAccountStatus.Online)
                {
                    SboEngine.LogOff();
                }

                Thread.Sleep(30000);
            }
            return false;
            //return ReConnectSbo();
        }

        private void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
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

        public eServiceStatus ReStart()
        {
            try
            {
                Logger.Info("KHOI DONG LAI HE THONG!!!!!!!!!!!!!!!!!!!!!!");
                this.Pause();
                IbetEngine.LogOff();
                SboEngine.LogOff();
                IbetEngine.ReLogin();
                SboEngine.ReLogin();
                var status = Start(ScanType);
                Logger.Info("KHOI DONG LAI HE THONG THANH CONG!!!!!!!!!!!!!!!!!!!!!!");
                return status;
            }
            catch (Exception ex)
            {
                Logger.Error(ex); ;
            }

            return eServiceStatus.Unknown;
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
            Stake = ProcessorConfigInfo.BetStake;

            BetStake = new List<int>();
            var stak = Stake.Split(new[] { '#' });
            foreach (var s in stak)
            {
                if (!string.IsNullOrEmpty(s))
                {
                    int stake;
                    int.TryParse(s, out stake);
                    if (stake != 0)
                    {
                        //Logger.Info("STAKE " + stake);
                        BetStake.Add(stake);
                    }
                }
            }

            BetStake.Shuffle();


            TimeOffStakeOdds = ProcessorConfigInfo.TimeOffStakeOdds;
            MinOddDefBet = ProcessorConfigInfo.MinOddDefBet;
            MinTimeToBet = ProcessorConfigInfo.MinTimeToBet;

            BetAgainstIbet = ProcessorConfigInfo.RebetIbet;
            //WaitingTimeRebetIbet = ProcessorConfigInfo.WaitingTimeRebetIbet;
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

                //if (SboEngine.CheckLogin() && PiEngine.CheckLogin())
                //{
                SboEngine.EngineName = ProcessorName;
                //SboEngine.EngineLogger = engineLogger;
                SboEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                //SboEngine.InitEngine();
                SboEngine.UpdateAvailabeCredit();
                SboEngine.StartScanEngine(scanType);

                //PiEngine.InitEngine();
                IbetEngine.EngineName = ProcessorName;
                //PiEngine.EngineLogger = engineLogger;
                IbetEngine.UpdateAvailabeCredit();
                IbetEngine.StartScanEngine(scanType);

                objUpdateCreditTimer = new Timer(WaitUpdateCreditCallback, null, 60000 * 5, 60000 * 5);
                StartFailCount = 0;

                _driver.OnSboScanUpdate += OnSboUpdateScan;

                //SboEngine.UpdateLiveDataChange += SboEngineOnUpdateLiveDataChange;
                //IbetSboDriver.OnSboScanUpdate += OnSboUpdateScan;
                //ServerCallback.OnSboScanUpdate += OnSboUpdateScan;
                //ServerCallback.OnIbetScanUpdate += OnIbetUpdateScan;
                ServerCallback.OnTransactionStatic += ServerCallback_OnTransaction;
                FireProcessStateChange(eServiceStatus.Started, ProcessorName);
                Status = eServiceStatus.Started;

                //List<TransactionDTO> transactionDtos;
                //while (TransactionQueue.TryDequeue(out transactionDtos))
                //{
                //    // do nothing
                //}
                //processBetThread = new Thread(DoBetTransaction);
                //processBetThread.Start();
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

        void ServerCallback_OnTransaction(DateTime time, TransactionDTO transaction, string oddIdCheck, float oddCheck)
        {
            try
            {
                if (Status == eServiceStatus.Started)
                {
                    if ((transaction.IsLive && ScanType == eScanType.NonLive)
                        || (!transaction.IsLive && ScanType == eScanType.Live))
                    {
                        return;
                    }

                    if (!IsValidTime(transaction.IbetMatchOdd.TimeType, transaction.IbetMatchOdd.Minutes, transaction.IbetMatchOdd.OddType))
                    {
                        return;
                    }

                    if ((transaction.IbetBetType == eBetType.Home &&
                        (Math.Abs(transaction.IbetMatchOdd.HomeOdd) < MinOddDefBet
                        || Math.Abs(transaction.SboMatchOdd.AwayOdd) < MinOddDefBet)) ||
                       (transaction.IbetBetType == eBetType.Away &&
                        (Math.Abs(transaction.IbetMatchOdd.AwayOdd) < MinOddDefBet
                        || Math.Abs(transaction.SboMatchOdd.HomeOdd) < MinOddDefBet)))
                    {
                        return;
                    }

                    MatchOddDTO sboMatchSource = GetSboMatch(transaction.SboMatchOdd, transaction.IsLive);

                    if (sboMatchSource != null)
                    {
                        Task.Run(() => OnPingEvent(time, eServerType.Ibet));

                        sboMatchSource.HomeOdd = transaction.SboMatchOdd.HomeOdd;
                        sboMatchSource.AwayOdd = transaction.SboMatchOdd.AwayOdd;

                        ProcessPrepareBet(transaction.IbetMatchOdd, transaction.IbetBetType, sboMatchSource,
                            transaction.SboBetType, transaction.IsLive, eServerScan.Server);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ProcessorName, ex);
            }
        }

        //private bool isProcess;
        private void OnSboUpdateScan(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time)
        {
            if ((isLive && ScanType == eScanType.NonLive)
                       || (!isLive && ScanType == eScanType.Live))
            {
                return;
            }

            if (LastBetTime.AddSeconds(TimeOffStakeOdds) > time || isProcess)
            {
                return;
            }

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
        }

        public void Pause()
        {
            //if (processBetThread != null)
            //{
            //    processBetThread.Abort();
            //}

            //SboEngine.UpdateLiveDataChange -= SboEngineOnUpdateLiveDataChange;
            //ServerCallback.OnTransaction -= ServerCallback_OnTransaction;
            ServerCallback.OnTransactionStatic -= ServerCallback_OnTransaction;
            _driver.OnSboScanUpdate -= OnSboUpdateScan;
            //IbetSboDriver.OnSboScanUpdate -= OnSboUpdateScan;
            //ServerCallback.OnIbetScanUpdate -= OnIbetUpdateScan;
            //ServerCallback.OnSboScanUpdate -= OnSboUpdateScan;
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
            //if (processBetThread != null)
            //{
            //    processBetThread.Abort();
            //}

            //SboEngine.UpdateLiveDataChange -= SboEngineOnUpdateLiveDataChange;
            //ServerCallback.OnTransaction -= ServerCallback_OnTransaction;
            ServerCallback.OnTransactionStatic -= ServerCallback_OnTransaction;
            _driver.OnSboScanUpdate -= OnSboUpdateScan;
            //IbetSboDriver.OnSboScanUpdate -= OnSboUpdateScan;
            //ServerCallback.OnIbetScanUpdate -= OnIbetUpdateScan;
            //ServerCallback.OnSboScanUpdate -= OnSboUpdateScan;
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
        public void CompareOdd(List<MatchOddDTO> dataUpdated, List<MatchOddDTO> targetSource, bool isLive)
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
                        if (ibetMatchTarget.HomeOdd.Equals(0) || ibetMatchTarget.AwayOdd.Equals(0))
                        {
                            continue;
                        }

                        if (IsValidTime(ibetMatchTarget.TimeType, ibetMatchTarget.Minutes, ibetMatchTarget.OddType))
                        {
                            bool isValid1 = IsValidOddPair(data.HomeOdd, ibetMatchTarget.AwayOdd, CompareValueDifferent);

                            if (isValid1)
                            {
                                //Logger.InfoFormat("{3} Sbo {0}, Home Odd {1}, Away Odd {2}",
                                //    data.HomeTeamName + " | " + data.AwayTeamName + " ODD: " + data.Odd,
                                //    data.HomeOdd, data.AwayOdd, ProcessorName);

                                MatchOddDTO sboMatchSource = GetSboMatch(data, isLive);

                                if (sboMatchSource != null)
                                {
                                    if (data.HomeOdd.Equals(sboMatchSource.OldOdd)
                               && data.HomeOdd != sboMatchSource.NewUpdateOdd)
                                    {
                                        continue;
                                    }

                                    sboMatchSource.HomeOdd = data.HomeOdd;
                                    sboMatchSource.AwayOdd = data.AwayOdd;

                                    ProcessPrepareBet(ibetMatchTarget, eBetType.Away, sboMatchSource, eBetType.Home, isLive);
                                    return;
                                }
                            }

                            bool isValid2 = IsValidOddPair(data.AwayOdd, ibetMatchTarget.HomeOdd, CompareValueDifferent);
                            if (isValid2)
                            {
                                //Logger.InfoFormat("{3} Sbo {0}, Home Odd {1}, Away Odd {2}",
                                //    data.HomeTeamName + " | " + data.AwayTeamName + " ODD: " + data.Odd,
                                //    data.HomeOdd, data.AwayOdd, ProcessorName);

                                MatchOddDTO sboMatchSource = GetSboMatch(data, isLive);

                                if (sboMatchSource != null)
                                {
                                    if (data.AwayOdd == sboMatchSource.OldOdd
                             && data.AwayOdd != sboMatchSource.NewUpdateOdd)
                                    {
                                        continue;
                                    }

                                    sboMatchSource.HomeOdd = data.HomeOdd;
                                    sboMatchSource.AwayOdd = data.AwayOdd;

                                    //Logger.Debug(ProcessorName + ">>> CALL PROCESSS BET");
                                    ProcessPrepareBet(ibetMatchTarget, eBetType.Home, sboMatchSource, eBetType.Away, isLive);
                                    return;
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
                        (String.Equals(m.HomeTeamName, sboMatchCompare.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, sboMatchCompare.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                        && m.Odd.Equals(sboMatchCompare.Odd)
                        && m.OddType == sboMatchCompare.OddType);
                }
            }
            else
            {
                lock (SboEngine.LockNonLive)
                {
                    sboMatchSource = SboEngine.NoneLiveMatchOddDatas.FirstOrDefault(m =>
                         (String.Equals(m.HomeTeamName, sboMatchCompare.HomeTeamName, StringComparison.CurrentCultureIgnoreCase)
                            || String.Equals(m.AwayTeamName, sboMatchCompare.AwayTeamName, StringComparison.CurrentCultureIgnoreCase))
                        && m.Odd.Equals(sboMatchCompare.Odd)
                        && m.OddType == sboMatchCompare.OddType);
                }
            }

            return sboMatchSource;
        }

        public bool IsValidTime(eTimeMatchType matchTimeType, int matchMinute, eOddType oddType)
        {
            if (MinTimeToBet == 0)
            {
                return true;
            }

            if (matchTimeType == eTimeMatchType.H1
                && (oddType == eOddType.HalfHCP || oddType == eOddType.HalfOU))
            {
                return matchMinute + MinTimeToBet < 45;
            }

            if (matchTimeType == eTimeMatchType.H2)
            {
                return matchMinute + MinTimeToBet < 45;
            }

            return true;
        }

        public bool IsValidOddPair(float firstOdd, float secondOdd, double oddValueDifferent)
        {
            if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet)//|| firstOdd == 0 || secondOdd == 0
            {
                return false;
            }

            bool result = false;
            if (firstOdd < 0f && secondOdd < 0f)
            {
                return (firstOdd + 1) + (secondOdd + 1) < 0.27;
            }

            var sum = firstOdd + secondOdd;

            if (Math.Abs(sum).Equals(2))
                return true;

            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = sum >= 0 && sum < 0.27;

                if (!result)
                {
                    result = Math.Abs(sum) <= oddValueDifferent;
                }
            }
            return result;
        }

        public bool IsGoodOddPair(float firstOdd, float secondOdd, double oddValueDifferent)
        {
            if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet)//|| firstOdd == 0 || secondOdd == 0
            {
                return false;
            }

            bool result = false;
            if (firstOdd < 0f && secondOdd < 0f)
            {
                return (firstOdd + 1) + (secondOdd + 1) < 0.27;
            }

            var sum = firstOdd + secondOdd;

            if (Math.Abs(sum).Equals(2))
                return true;

            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = sum >= 0 && sum < 0.27;

                if (!result)
                {
                    result = Math.Abs(sum) <= oddValueDifferent;
                }
            }
            return result;
        }

        public DateTime LastProcessPrepare;
        private bool isProcess;
        public bool ProcessPrepareBet(MatchOddDTO ibetMatchOdd, eBetType ibetBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive,
            eServerScan serverScan = eServerScan.Local, bool callSecond = false, string bpMatch = "", PrepareBetDTO sboPrepareBet = null)
        {
            if (isProcess && !callSecond) // || LastProcessPrepare.AddMilliseconds(200) > DateTime.Now
            {
                return false;
            }

            try
            {
                isProcess = true;
                PrepareBetDTO ibetPrepareBet;
                if (!callSecond)
                {
                    sboMatchOdd.OldOdd = sboBetType == eBetType.Home ? sboMatchOdd.HomeOdd : sboMatchOdd.AwayOdd;

                    var taskPrepareBetSbo =
                        Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive, ibetMatchOdd.Odd));
                    ibetPrepareBet = CallPrepareBet(ibetMatchOdd, ibetBetType, isLive, sboMatchOdd.Odd);

                    if (!ibetPrepareBet.IsSuccess || taskPrepareBetSbo.Result == null)
                    {
                        return false;
                    }

                    sboPrepareBet = taskPrepareBetSbo.Result;
                }
                else
                {
                    Logger.Warn("BAT DAU PREPARE LAN 2");
                    ibetPrepareBet = CallPrepareIbetAgain(ibetMatchOdd, ibetBetType, isLive, sboMatchOdd.Odd, bpMatch);
                    if (!ibetPrepareBet.IsSuccess)
                    {
                        return false;
                    }
                }


                //sboMatchOdd.OldOdd = sboBetType == eBetType.Home ? sboMatchOdd.HomeOdd : sboMatchOdd.AwayOdd;

                //var taskPrepareBetSbo =
                //    Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive, ibetMatchOdd.Odd));
                //PrepareBetDTO ibetPrepareBet = CallPrepareBet(ibetMatchOdd, ibetBetType, isLive, sboMatchOdd.Odd);

                //if (!ibetPrepareBet.IsSuccess || taskPrepareBetSbo.Result == null)
                //{
                //    return false;
                //}
                //PrepareBetDTO sboPrepareBet = taskPrepareBetSbo.Result;

                if (ibetPrepareBet.IsRunning)
                {
                    bool isGood = IsGoodOddPair(ibetPrepareBet.NewOdd, sboPrepareBet.NewOdd, CompareValueDifferent);

                    if (isGood)
                    {
                        int ibetStake;
                        int sboStake;

                        if (CaculateStake(BetStakeType,
                            ibetPrepareBet.MatchOdd.MatchID,
                            ibetPrepareBet.MinBet,
                            ibetPrepareBet.MaxBet,
                            sboPrepareBet.MatchOdd.MatchID,
                            sboPrepareBet.MinBet,
                            sboPrepareBet.MaxBet,
                            out ibetStake, out sboStake))
                        {
                            string ibetConfirmMsg;

                            if (CallConfirmBet(ibetPrepareBet, ibetStake, sboMatchOdd.Odd, out ibetConfirmMsg, isLive, false, serverScan, callSecond))
                            {
                                LastBetIbetSuccess = DateTime.Now;

                                if (CallConfirmBet(sboPrepareBet, sboStake, ibetMatchOdd.Odd, out ibetConfirmMsg, isLive, false, serverScan))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(
                                                sboPrepareBet.MatchOdd.ServerType,
                                                sboPrepareBet.MatchOdd.MatchID,
                                                SboBetPool, sboStake);
                                            UpdateBetPool(
                                                ibetPrepareBet.MatchOdd.ServerType,
                                                ibetPrepareBet.MatchOdd.MatchID,
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
                                    if (BetAgainstIbet)
                                    {
                                        Logger.Info(ProcessorName + " DAT NGUOC IBET<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");

                                        if (!BetAgainstIbetRequest(ibetPrepareBet, ibetMatchOdd, ibetBetType,
                                            isLive, sboMatchOdd.Odd, ibetStake, false))
                                        {
                                            Thread.Sleep(3000);
                                            Logger.Info("CALL LAN 2 BET MGUOC IBET<<<<<<<<<<<<<<<<");
                                            if (BetAgainstIbetRequest(ibetPrepareBet, ibetMatchOdd, ibetBetType,
                                            isLive, sboMatchOdd.Odd, ibetStake, true))
                                            {
                                                isProcess = false;
                                                return true;
                                            }
                                            else
                                            {
                                                Logger.Info("DAT NGUOC THAT BAI, KHOI DONG LAI IBET!!!");
                                                IbetEngine.BetAgainstTransactions.Add(new BetAgainstTransaction()
                                                {
                                                    BetType = GetAgainstBetType(ibetBetType),
                                                    IsLive = isLive,
                                                    MatchOdd = ibetMatchOdd,
                                                    OddCompare = sboMatchOdd.Odd,
                                                    Stake = ibetStake,
                                                    HomeScore = ibetPrepareBet.HomeScore,
                                                    AwayScore = ibetPrepareBet.AwayScore
                                                });

                                                Task.Run(() =>
                                                {
                                                    Logger.Info("CALL KHOI DONG LAI IBET!!!");
                                                    IbetEngine.UpdateException(IbetEngine, eExceptionType.LoginFail);
                                                });
                                            }
                                        }
                                        else
                                        {
                                            isProcess = false;
                                            return true;
                                        }

                                        Logger.Info("BET MGUOC IBET FAIL...DAT NGUOC THAT BAI...");
                                    }
                                }

                                LastBetTime = DateTime.Now;

                                Task.Run(
                                    () =>
                                        UpdateBetPool(ibetPrepareBet.MatchOdd.ServerType,
                                            ibetPrepareBet.MatchOdd.MatchID, IbetBetPool,
                                            ibetStake));
                                return true;
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(ibetConfirmMsg))
                                {
                                    string bpValue = GetBpMatchIbet(ibetConfirmMsg);
                                    Logger.Info("GOI COMPARE LAI LAN 2, bp_match: " + bpValue);
                                    return ProcessPrepareBet(ibetMatchOdd, ibetBetType, sboMatchOdd, sboBetType,
                                        isLive,
                                        serverScan, true, bpValue, sboPrepareBet);
                                }
                            }

                            FireLogScan(ibetMatchOdd, sboMatchOdd, serverScan);
                        }
                    }
                    else
                    {
                        FireLogScan(ibetMatchOdd, sboMatchOdd, serverScan);
                    }
                }
                else
                {
                    FireLogScan(ibetMatchOdd, sboMatchOdd, serverScan);
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ProcessorName, ex);
                return false;
            }
            finally
            {
                //LastProcessPrepare = DateTime.Now;
                isProcess = false;
            }
        }

        private PrepareBetDTO CallPrepareIbetAgain(MatchOddDTO matchOdd, eBetType betType, bool isLive, float oddDef, string bpMatch)
        {
            return IbetEngine.PrepareBet(matchOdd, betType, isLive, oddDef, bpMatch);
        }

        private string GetBpMatchIbet(string confirmMsg)
        {
            var validateIndex = confirmMsg.IndexOf("var Type='");
            var validateIndexEnd = confirmMsg.IndexOf("';", validateIndex);
            return confirmMsg.Substring(validateIndex + 10, validateIndexEnd - (validateIndex + 10));
        }

        private bool BetAgainstIbetRequest(PrepareBetDTO oldIbetPrepare, MatchOddDTO ibetMatchOddSource, eBetType ibetBetType,
            bool isLive, float sboMatchOdd, int ibetStake, bool hasGetAgain)
        {
            MatchOddDTO ibetMatchOdd;

            if (hasGetAgain)
            {
                Thread.Sleep(60000);
                lock (IbetEngine.LockLive)
                {
                    ibetMatchOdd =
                        IbetEngine.LiveMatchOddDatas.FirstOrDefault(l => l.MatchID == ibetMatchOddSource.MatchID &&
                                                                         l.Odd.Equals(ibetMatchOddSource.Odd) &&
                                                                         l.OddType == ibetMatchOddSource.OddType);
                }

                if (ibetMatchOdd == null)
                {
                    return false;
                }
            }
            else
            {
                ibetMatchOdd = ibetMatchOddSource;
            }

            var againstIbetPrepare = CallPrepareIbet(ibetMatchOdd, GetAgainstBetType(ibetBetType), isLive, sboMatchOdd);
            if (againstIbetPrepare.IsRunning)
            {
                //if (againstIbetPrepare.HasScore)
                //{
                if (againstIbetPrepare.HomeScore == oldIbetPrepare.HomeScore
                && againstIbetPrepare.AwayScore == oldIbetPrepare.AwayScore)
                {
                    string ibetConfirmMsg;
                    if (CallConfirmBet(againstIbetPrepare, ibetStake, oldIbetPrepare.OddDef, out ibetConfirmMsg, isLive, true))
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

        private PrepareBetDTO CallPrepareIbet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float oddDef)
        {
            return IbetEngine.PrepareBet(matchOdd, betType, isLive, oddDef, true);
        }

        private bool CallConfirmBet(PrepareBetDTO prepareBet, int stake, float oddDef, out string ibetMsg,
            bool isLive = false, bool betAgainst = false,
            eServerScan serverScan = eServerScan.Local, bool callSecondIbet = false)
        {
            ibetMsg = null;
            switch (prepareBet.MatchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.ConfirmBet(stake, out ibetMsg, betAgainst, serverScan);
                case eServerType.Sbo:
                    return SboEngine.ConfirmBet(stake, oddDef, isLive, serverScan);
                default:
                    throw new Exception("CallConfirmBet => FAIL : Unknow prepareBet->MatchOdd->eServerType param");
            }
        }

        private bool CaculateStake(eBetStakeType betStakeType, string ibetMatchId, int ibetMin, int ibetMax,
            string sboMatchId, int sboMin, int sboMax,
            out int ibetStake, out int sboStake)
        {
            bool isSuccess = false;
            Random r = new Random();
            switch (betStakeType)
            {
                case eBetStakeType.Ibet:
                    ibetStake = BetStake[r.Next(BetStake.Count)];
                    sboStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax
                        && sboStake >= sboMin && sboStake <= sboMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Sbo:
                    sboStake = BetStake[r.Next(BetStake.Count)];
                    ibetStake = (int)Math.Round((double)((sboStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax && sboStake >= sboMin && sboStake <= sboMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Max:
                    ibetStake = ibetMax;
                    sboStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (sboStake <= sboMax && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }

                    sboStake = sboMax;
                    ibetStake = (int)Math.Round((double)((sboStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake <= ibetMax && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                    }
                    break;
                default:
                    throw new Exception("Not support eBetStakeType");
            }

            if (isSuccess)
            {
                var isIbetMax = IsMaxStake(ibetMatchId, IbetBetPool, IbetMaxStakeMatch, ibetStake);
                var isSboMax = IsMaxStake(sboMatchId, SboBetPool, SboMaxStakeMatch, sboStake);
                if (isSboMax || isIbetMax || ibetStake > IbetEngine.AvailabeCredit || sboStake > SboEngine.AvailabeCredit)
                {
                    return false;
                }
            }

            return isSuccess;
        }

        private bool IsMaxStake(string matchOddId, Dictionary<string, int> betPool, int maxStakeMatch, int stake)
        {
            if (betPool.ContainsKey(matchOddId))
            {
                return betPool[matchOddId] + stake > maxStakeMatch;
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

        private void FireLogScan(MatchOddDTO ibetMatchOddDto, MatchOddDTO sboMatchOddDto, eServerScan serverScan)
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
                SecondOddValues = sboMatchOddDto.HomeOdd + " | " + sboMatchOddDto.AwayOdd,
                ServerScan = serverScan
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
                //if (PiEngine.UpdateAvailabeCredit()==0)
                //{
                //    OnExceptionEvent(new ExceptionMessage(eExceptionType.LoginFail), eServerType.Ibet);
                //}

                //if (SboEngine.UpdateAvailabeCredit()==0)
                //{
                //    OnExceptionEvent(new ExceptionMessage(eExceptionType.LoginFail), eServerType.Sbo);
                //}
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
