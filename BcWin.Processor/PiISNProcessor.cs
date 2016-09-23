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
using BcWin.Common.FaultDTO;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Core.Helper;
using BcWin.Engine.PinnacleSports2;
using BcWin.Processor.Interface;
using BcWin.Processor.ScanDriver;
using BcWin.Processor.Service;
using BCWin.Engine.Sbo;
using log4net;
using BCWin.Engine.Isn;
using System.Diagnostics;
using BcWin.Common.EventDelegate;

namespace BcWin.Processor
{
    public partial class PiIsnProcessor
    {

        public eServiceStatus Status { get; set; }

        public PiEngine PiEngine { get; set; }

        public IsnEngine IsnEngine { get; set; }

        public ProcessorConfigInfoDTO ProcessorConfigInfo { get; set; }

        public eScanType ScanType { get; set; }

        protected System.Threading.Timer objUpdateCreditTimer { get; set; }

        public event ProcessStateChangeEvent OnProcessStateChange;

        public event PingEvent OnPingEvent;

        public ServerCallback ServerCallback { get; set; }
        public int CountBet { get; set; }
        public float MinOddDefBet { get; set; }
        public int MinTimeToBet { get; set; }
        public int MaxCountBet { get; set; }
        public DateTime LastBetTime { get; set; }

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
        public Dictionary<string, int> PiBetPool { get; set; }
        public Dictionary<string, int> IsnBetPool { get; set; }
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PiIsnProcessor));

        public int StartFailCount { get; set; }
        public string ProcessorName { get; set; }

        public PiIsnProcessor()
        {
            StartFailCount = 0;
            PiEngine = new PiEngine();
            IsnEngine = new IsnEngine();
        }

        public void Initialize()
        {
            if (IsnEngine.CheckLogin())
            {
                 //  if (IsnEngine.Status == eServiceStatus.Unknown)
                 //{
                IsnBetPool = new Dictionary<string, int>();
                IsnEngine.OnExceptionEvent += OnExceptionEvent;
                IsnEngine.InitEngine();
                 //}

                PiBetPool = new Dictionary<string, int>();
                PiEngine.OnExceptionEvent += OnExceptionEvent;
                PiEngine.InitEngine();

                Status = eServiceStatus.Initialized;
            }
        }

        private bool IsReConnect;

        private void ReStartFromIsn()
        {
            try
            {
                if (!IsReConnect)
                {
                    IsReConnect = true;
                    this.Pause();
                    FireProcessStateChange(eServiceStatus.Paused, "reconnecting...");
                    Logger.Info("TAM DUNG HE THONG, KET NOI LAI ISN!!!!!!!!!");
                    IsnEngine.LogOff();

                    ReConnectIsn();
                    IsReConnect = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ReStartFromIsn: " + ex.Message);
            }

        }

        public bool ReConnectIsn()
        {
            try
            {
                if (IsnEngine.TryLogin())
                {
                    Logger.Info("Ket noi ISN thanh cong>>>>>>>>>>>>>>>>>>>>>");

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

                    if (IsnEngine.AccountStatus == eAccountStatus.Online)
                    {
                        IsnEngine.LogOff();
                    }

                    Thread.Sleep(30000);
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ReConnectIsn: " + ex.Message);
                return false;
            }

        }

        private void ReStartFromPina()
        {
            try
            {
                if (!IsReConnect)
                {
                    IsReConnect = true;
                    this.Pause();
                    FireProcessStateChange(eServiceStatus.Paused, "reconnecting...");
                    Logger.Info("TAM DUNG HE THONG, KET NOI LAI Pina!!!!!!!!!");
                    PiEngine.LogOff();

                    ReConnectPina();
                    IsReConnect = false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ReStartFromPina: " + ex.Message);
            }

        }

        public bool ReConnectPina()
        {
            try
            {
                if (PiEngine.TryLogin())
                {
                    Logger.Info("Ket noi Pina thanh cong>>>>>>>>>>>>>>>>>>>>>");

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

                    if (PiEngine.AccountStatus == eAccountStatus.Online)
                    {
                        PiEngine.LogOff();
                    }

                    Thread.Sleep(30000);
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ReConnectPina: " + ex.Message);
                return false;
            }

        }

        private void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            if (serverType == eServerType.Isn)
            {
                Thread thread = new Thread(ReStartFromIsn);
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
                if (serverType == eServerType.Pina)
                {
                    Thread thread = new Thread(ReStartFromPina);
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
                IsnEngine.LogOff();
                PiEngine.LogOff();
                //   IsnEngine.ReLogin();
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
            //CompareValueDifferent = ProcessorConfigInfo.CompareValue + 0.005;
            ScanType = scanType;
            CountBet = 1;
            MaxCountBet = ProcessorConfigInfo.MaxCountBet;
            LastBetTime = DateTime.Now.AddMinutes(-10);

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
            AcceptMinLossIbet = ProcessorConfigInfo.AcceptMinLossIbet;

            if (ProcessorConfigInfo.AccountFirst.ServerType == eServerType.Pina)
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
                ProcessorName = PiEngine.UserName + " - " + IsnEngine.UserName;

                IsnEngine.EngineName = ProcessorName;
                IsnEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                IsnEngine.UpdateAvailabeCredit();


                PiEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                PiEngine.EngineName = ProcessorName;
                PiEngine.UpdateAvailabeCredit();

                IsnEngine.StartScanEngine(scanType);
                PiEngine.StartScanEngine(scanType);

                objUpdateCreditTimer = new Timer(WaitUpdateCreditCallback, null, 60000 * 5, 60000 * 5);
                StartFailCount = 0;

                IsnEngine.UpdateLiveDataChange += OnIsnUpdateScan;

                //tạm đóng
                //PiSboDriver.OnSboScanUpdate += PiSboDriver_OnSboScanUpdate;
                //_driver.OnSboScanUpdate += OnSboUpdateScan;

                FireProcessStateChange(eServiceStatus.Started, ProcessorName);
                Status = eServiceStatus.Started;
            }
            catch (Exception ex)
            {
                Logger.Error("Start Processor Fail !", ex);

                StartFailCount++;

                PiEngine.Dispose();
                IsnEngine.Dispose();
                Status = eServiceStatus.Unknown;
            }

            return Status;
        }


        //private bool isProcess;
        // private void OnSboUpdateScan(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time)
        private void OnIsnUpdateScan(List<MatchIsnDTO> updatedDataIsn, bool isLive, int a)
        {
            //if (LastBetTime.AddSeconds(TimeOffStakeOdds) > time || isProcess)
            //{
            //    return;
            //}
            //
            //    Task.Run(() => OnPingEvent(DateTime.Now, eServerType.Ibet));
            try
            {
                if (isLive)
                {
                    List<MatchPiDTO> piMatch;
                    //if (PiEngine.IsScanCompleted == false)
                    //{
                    //    Debug.WriteLine("Fail");
                    //    return;
                    //}

                    lock (PiEngine.LockLive)
                    {
                        piMatch = PiEngine._matchs;
                    }

                    List<MatchIsnDTO> isnMatch;
                    lock (IsnEngine.LockLive)
                    {
                        isnMatch = IsnEngine._matchs;

                    }
                    CompareOdd(isnMatch, piMatch, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("OnIsnUpdateScan " + ex.Message);
            }

        }

        public void Pause()
        {
            try
            {
                //_driver.OnSboScanUpdate -= OnSboUpdateScan;
                IsnEngine.UpdateLiveDataChange -= OnIsnUpdateScan;
                // tạm đóng
                //  PiSboDriver.OnSboScanUpdate -= PiSboDriver_OnSboScanUpdate;

                PiEngine.PauseScan();

                if (IsnEngine.Status == eServiceStatus.Started)
                {
                    IsnEngine.PauseScan();
                }

                if (objUpdateCreditTimer != null)
                {
                    objUpdateCreditTimer.Dispose();
                }


                Status = eServiceStatus.Paused;
            }
            catch (Exception ex)
            {
                Logger.Error("Dispose: " + ex.Message);

            }

        }

        public void Dispose()
        {
            try
            {
                Pause();

                IsnEngine.OnExceptionEvent -= OnExceptionEvent;
                PiEngine.OnExceptionEvent -= OnExceptionEvent;
                if (objUpdateCreditTimer != null)
                {
                    objUpdateCreditTimer.Dispose();
                }
                PiEngine.LogOff();
                IsnEngine.LogOff();
                Status = eServiceStatus.Stopped;
                StartFailCount = 0;
            }
            catch (Exception ex)
            {
                Logger.Error("Dispose: " + ex.Message);
            }

        }

        //// compare from sbo (dataUpdated) to Pi (targetSource)
        public void CompareOdd(List<MatchIsnDTO> dataUpdated, List<MatchPiDTO> targetSource, bool isLive)
        {
            try
            {
                foreach (var isnMatch in dataUpdated)
                {
                    if (/*!DataContainer.LeaguesDenyKeywords.Any(data.LeagueName.ToUpper().Contains)
                        &&*/ (!DataContainer.MatchsDenyKeywords.Any(isnMatch.HomeTeamName.ToUpper().Contains)
                            || !DataContainer.MatchsDenyKeywords.Any(isnMatch.AwayTeamName.ToUpper().Contains)))
                    {
                        MatchPiDTO piMatchTarget = targetSource.FirstOrDefault(m =>
                            (String.Equals(m.HomeTeamName, isnMatch.HomeTeamName,
                                StringComparison.CurrentCultureIgnoreCase)
                             ||
                             String.Equals(m.AwayTeamName, isnMatch.AwayTeamName,
                                 StringComparison.CurrentCultureIgnoreCase)
                           )
                                 );

                        if (piMatchTarget != null)
                        {

                            foreach (var isnOdd in isnMatch.Odds)
                            {
                                OddDTO piOdd = piMatchTarget.Odds.FirstOrDefault(o => o.Odd.Equals(isnOdd.Odd) /*&& !(o.HomeOdd == 0 && o.AwayOdd == 0)*/
                                                                                    && o.OddType == isnOdd.OddType);


                                if (piOdd != null)
                                {
                                    //string oddCheck = String.Concat(isnMatch.HomeTeamName, isnMatch.AwayTeamName, isnOdd.HomeOdd, isnOdd.AwayOdd, isnOdd.Odd, piOdd.Odd, piOdd.HomeOdd, piOdd.AwayOdd);

                                    //if (Process.OddCheckScan.Any(s => s == oddCheck))
                                    //{
                                    //    continue;
                                    //}

                                    //bool isValid1 = IsValidOddPair(isnOdd.HomeOdd, piOdd.AwayOdd, CompareValueDifferent);
                                    bool isValid1 = IsValidOddPair(isnOdd.HomeOdd, piOdd.AwayOdd, CompareValueDifferent);
                                    //Debug.WriteLine(isnOdd.HomeOdd + "|" + piOdd.AwayOdd);

                                    //   && IsValidPick(isnOdd, eBetType.Home);

                                    if (isValid1)
                                    {
                                        //Process.OddCheckScan.Add(oddCheck);
                                        ProcessPrepareBet(piMatchTarget, piOdd, eBetType.Away, isnMatch, isnOdd, eBetType.Home, isLive);
                                        return;

                                    }

                                    bool isValid2 = IsValidOddPair(isnOdd.AwayOdd, piOdd.HomeOdd, CompareValueDifferent);
                                    //Debug.WriteLine(isnOdd.AwayOdd + "|" + piOdd.HomeOdd);

                                    //      && IsValidPick(isnOdd, eBetType.Away);

                                    if (isValid2)
                                    {
                                        //Process.OddCheckScan.Add(oddCheck);
                                        ProcessPrepareBet(piMatchTarget, piOdd, eBetType.Home, isnMatch, isnOdd, eBetType.Away, isLive);
                                        return;

                                    }
                                }



                            }



                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal(ProcessorName, ex);
            }
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
                result = sum >= 0 && sum < 0.27;

                if (!result)
                {
                    result = Math.Abs(sum) <= oddValueDifferent;
                }
            }
            return result;
        }

        public bool IsValidPick(OddDTO sboMatchOdd, eBetType betType)
        {
            if (sboMatchOdd.OddType == eOddType.HCP || sboMatchOdd.OddType == eOddType.HalfHCP)
            {
                if (sboMatchOdd.Odd.Equals(0))
                {
                    return true;
                }

                if (sboMatchOdd.Odd > 0)
                {
                    if (betType == eBetType.Home)
                    {
                        return RealConfig.PickSboHdpBot;
                    }
                    return RealConfig.PickSboHdpTop;
                }
                else //if (sboMatchOdd.Odd < 0)
                {
                    if (betType == eBetType.Home)
                    {
                        return RealConfig.PickSboHdpTop;
                    }
                    return RealConfig.PickSboHdpBot;
                }
            }
            else
            {
                if (betType == eBetType.Home)
                {
                    return RealConfig.PickSboOver;
                }
                return RealConfig.PickSboUnder;
            }
        }

        public string OddCheck { get; set; }
        public DateTime LastProcessPrepare;
        private bool isProcess;

        public bool ProcessPrepareBet(MatchPiDTO piMatch, OddDTO piOdd, eBetType piBetType, MatchIsnDTO isnMatch, OddDTO isnOdd, eBetType isnBetType, bool isLive,
            eServerScan serverScan = eServerScan.Local)
        {
            if (isProcess)
            {
                return false;
            }

            //what?
            //string oddCheck = sboMatchOdd.OddID + sboMatchOdd.HomeOdd + sboMatchOdd.AwayOdd + piOdd.HomeOdd +
            //                  piOdd.AwayOdd;

            //if (oddCheck.Equals(OddCheck))
            //{
            //    return false;
            //}

            //OddCheck = oddCheck;

            try
            {
                isProcess = true;

                // wwhat?
                //sboMatchOdd.OldOdd = piBetType == eBetType.Home ? sboMatchOdd.HomeOdd : sboMatchOdd.AwayOdd;
                //   Logger.Debug("");
                //  Logger.Debug("Before call PREPARE ISN " + piMatch.HomeTeamName + "-" + piMatch.AwayTeamName + " " + piOdd.HomeOdd + "_" + piOdd.AwayOdd + " " + piBetType.ToString());
                //Stopwatch a = new Stopwatch();
                //a.Start();

                Task<int> taskMaxBet = Task.Run(() => IsnEngine.GetMax((isnBetType == eBetType.Home ? isnOdd.selectionIdHome : isnOdd.selectionIdAway)));

                Task<PrepareBetDTO> preparePina = null;
                Task<PrepareBetDTO> prepareIsn = null;

                if ((isnOdd.OddType == eOddType.OU || isnOdd.OddType == eOddType.HalfOU) && isnBetType == eBetType.Away)
                {
                    preparePina = Task.Run(() => PiEngine.PrepareBet(piMatch, piOdd, piBetType, piOdd.Odd));

                    prepareIsn = Task.Run(() => IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd));

                }
                else
                {
                    prepareIsn = Task.Run(() => IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd));

                    preparePina = Task.Run(() => PiEngine.PrepareBet(piMatch, piOdd, piBetType, piOdd.Odd));
                }

                if (preparePina.Result == null || prepareIsn.Result == null)
                {
                    FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);

                    return false;
                }

                //a.Stop();
                //Debug.WriteLine("Thoi gian prepare: " + a.ElapsedMilliseconds);

                //   float piOddValue = piBetType == eBetType.Home ? piOdd.HomeOdd : piOdd.AwayOdd;
                //bool isGood = IsGoodOddPair(piOddValue, prepareBetIsn.NewOdd, CompareValueDifferent, out betAny);
                bool isGood = IsGoodOddPair(preparePina.Result.NewOdd, prepareIsn.Result.NewOdd, CompareValueDifferent);
                // Logger.Debug("After isGood=" + isGood.ToString() + " " + piOddValue + "_" + prepareBetIsn.NewOdd + " " + piBetType.ToString());

                if (isGood)
                {
                    float firstOdd = piOdd.Odd;

                    int piStake;
                    int isnStake;

                    if (CaculateStake(BetStakeType,
                       piMatch.MatchID, 300, (int)((PendingTicket)preparePina.Result.PendingTicket).TicketItem.MaxRiskAmount,
                      isnMatch.MatchID, 4, taskMaxBet,
                       out piStake, out isnStake))
                    // if (true)
                    {
                        //if (piStake<(int)((PendingTicket)preparePina.Result.PendingTicket).TicketItem.MinRiskAmount)
                        //{
                        //    Logger.Info("Pi lower than riskamout");
                        //    return false;
                        //}
                        // kèo xỉu isn đnáh trước
                        if ((isnOdd.OddType == eOddType.OU || isnOdd.OddType == eOddType.HalfOU) && isnBetType == eBetType.Away)
                        {
                            var isnConfirmBet = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType, eBetStatusType.Success);
                            if (isnConfirmBet)
                            {
                                if (PiEngine.ConfirmBet2((PendingTicket)preparePina.Result.PendingTicket, piStake, piMatch, piOdd, piBetType, eBetStatusType.Success))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake);
                                            UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake);
                                        });
                                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    CountBet++;
                                    LastBetTime = DateTime.Now;
                                    return true;
                                }
                                else
                                {

                                    Task.Run(() => UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake));
                                    var betTypeAgaint = GetAgainstBetType(isnBetType);

                                    for (int i = 0; i < PiEngine.Rebet; i++)
                                    {
                                        lock (IsnEngine.LockLive)
                                        {
                                            var newIsnMatch = IsnEngine._matchs.FirstOrDefault(p => p.MatchID == isnMatch.MatchID);
                                            if (newIsnMatch != null)
                                            {
                                                var newIsnOdd = newIsnMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && p.OddType == isnOdd.OddType);
                                                if (newIsnOdd != null)
                                                {
                                                    var prepare = IsnEngine.PrepareBet(newIsnMatch, newIsnOdd, betTypeAgaint, true, firstOdd);
                                                    if (prepare != null)
                                                    {
                                                        var ok = IsnEngine.ConfirmBet2(isnStake, newIsnMatch, newIsnOdd, betTypeAgaint, eBetStatusType.BetAgainIsn, true);
                                                        if (ok)
                                                        {
                                                            LastBetTime = DateTime.Now;
                                                            Task.Run(() => UpdateBetPool(eServerType.Isn, newIsnMatch.MatchID, IsnBetPool, isnStake));
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                        Thread.Sleep(2000);

                                    }



                                    IsnEngine.FireLogBet(isnMatch, isnOdd, isnBetType, isnStake, eBetStatusType.Fail, eServerScan.Local);

                                }

                            }

                        }
                        else
                        {
                            var piConfirmBet = PiEngine.ConfirmBet((PendingTicket)preparePina.Result.PendingTicket, piStake, piMatch, piOdd, piBetType, eBetStatusType.Success);

                            if (piConfirmBet)
                            {
                                if (IsnEngine.ConfirmBet(isnStake, isnMatch, isnOdd, isnBetType))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake);
                                            UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake);
                                        });
                                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    CountBet++;
                                    LastBetTime = DateTime.Now;
                                    return true;
                                }
                                else if (BetAgainstIbet)
                                {
                                    var betTypeAgaint = GetAgainstBetType(piBetType);

                                    for (int i = 0; i < IsnEngine.Rebet; i++)
                                    {
                                        var prepare = PiEngine.PrepareBet(piMatch, piOdd, betTypeAgaint, firstOdd);
                                        if (prepare != null)
                                        {
                                            var ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, piMatch, piOdd, betTypeAgaint, eBetStatusType.BetAgainPina, true);
                                            if (ok)
                                            {
                                                LastBetTime = DateTime.Now;
                                                Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));
                                                return true;
                                            }
                                        }

                                        Thread.Sleep(1000);

                                        lock (PiEngine.LockLive)
                                        {
                                            var newPiMatch = PiEngine._matchs.FirstOrDefault(p => p.MatchID == piMatch.MatchID);
                                            if (newPiMatch != null)
                                            {
                                                var newPiOdd = newPiMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && p.OddType == piOdd.OddType);
                                                if (newPiOdd != null)
                                                {
                                                    prepare = PiEngine.PrepareBet(newPiMatch, newPiOdd, betTypeAgaint, firstOdd);
                                                    if (prepare != null)
                                                    {
                                                        var ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, newPiMatch, newPiOdd, betTypeAgaint, eBetStatusType.BetAgainPina, true);
                                                        if (ok)
                                                        {
                                                            LastBetTime = DateTime.Now;
                                                            Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        Thread.Sleep(1000);

                                        lock (IsnEngine.LockLive)
                                        {
                                            var newIsnMatch = IsnEngine._matchs.FirstOrDefault(p => p.MatchID == isnMatch.MatchID);
                                            if (newIsnMatch != null)
                                            {
                                                var newIsnOdd = newIsnMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && p.OddType == isnOdd.OddType);
                                                if (newIsnOdd != null)
                                                {
                                                    prepare = IsnEngine.PrepareBet(newIsnMatch, newIsnOdd, isnBetType, true, firstOdd);
                                                    if (prepare != null)
                                                    {
                                                        var ok = IsnEngine.ConfirmBet2(isnStake, newIsnMatch, newIsnOdd, isnBetType, eBetStatusType.MissOddIsn, true);
                                                        if (ok)
                                                        {
                                                            LastBetTime = DateTime.Now;
                                                            Task.Run(() => UpdateBetPool(eServerType.Isn, newIsnMatch.MatchID, IsnBetPool, isnStake));
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        Thread.Sleep(1000);
                                    }

                                    Thread.Sleep(60000);

                                    for (int i = 0; i < IsnEngine.Rebet * 2; i++)
                                    {
                                        var prepare = PiEngine.PrepareBet(piMatch, piOdd, betTypeAgaint, firstOdd);
                                        if (prepare != null)
                                        {
                                            var ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, piMatch, piOdd, betTypeAgaint, eBetStatusType.BetAgainPina, true);
                                            if (ok)
                                            {
                                                LastBetTime = DateTime.Now;
                                                Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));
                                                return true;
                                            }
                                        }

                                        Thread.Sleep(10000);

                                        lock (PiEngine.LockLive)
                                        {
                                            var newPiMatch = PiEngine._matchs.FirstOrDefault(p => p.MatchID == piMatch.MatchID);
                                            if (newPiMatch != null)
                                            {
                                                var newPiOdd = newPiMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && p.OddType == piOdd.OddType);
                                                if (newPiOdd != null)
                                                {
                                                    prepare = PiEngine.PrepareBet(newPiMatch, newPiOdd, betTypeAgaint, firstOdd);
                                                    if (prepare != null)
                                                    {
                                                        var ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, newPiMatch, newPiOdd, betTypeAgaint, eBetStatusType.BetAgainPina, true);
                                                        if (ok)
                                                        {
                                                            LastBetTime = DateTime.Now;
                                                            Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                        Thread.Sleep(10000);

                                        lock (IsnEngine.LockLive)
                                        {
                                            var newIsnMatch = IsnEngine._matchs.FirstOrDefault(p => p.MatchID == isnMatch.MatchID);
                                            if (newIsnMatch != null)
                                            {
                                                var newIsnOdd = newIsnMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && p.OddType == isnOdd.OddType);
                                                if (newIsnOdd != null)
                                                {
                                                    prepare = IsnEngine.PrepareBet(newIsnMatch, newIsnOdd, betTypeAgaint, true, firstOdd);
                                                    if (prepare != null)
                                                    {
                                                        var ok = IsnEngine.ConfirmBet2(isnStake, newIsnMatch, newIsnOdd, betTypeAgaint, eBetStatusType.MissOddIsn, true);
                                                        if (ok)
                                                        {
                                                            LastBetTime = DateTime.Now;
                                                            Task.Run(() => UpdateBetPool(eServerType.Isn, newIsnMatch.MatchID, IsnBetPool, isnStake));
                                                            return true;
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        Thread.Sleep(10000);
                                    }
                                }

                                LastBetTime = DateTime.Now;

                                Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));

                                return true;
                            }
                        }
                        // tạm đóng
                        FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);
                    }
                }
                else
                {
                    // tạm đóng
                    FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);
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
                isProcess = false;
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

        private PrepareBetDTO CallPrepareBet(MatchIsnDTO match, OddDTO odd, eBetType betType, bool isLive, float oddDef)
        {
            switch (match.ServerType)
            {
                //case eServerType.Ibet:
                //    return PiEngine.PrepareBet(matchOdd, betType, isLive, oddDef);
                case eServerType.Isn:
                    return IsnEngine.PrepareBet(match, odd, betType, isLive, oddDef);
                default:
                    return IsnEngine.PrepareBet(match, odd, betType, isLive, oddDef);

                //throw new Exception("CallPrepareBet => FAIL : Unknow matchOdd->eServerType param");
            }
        }

        private bool CaculateStake(eBetStakeType betStakeType, string ibetMatchId, int ibetMin, int ibetMax,
            string sboMatchId, int isnMin, Task<int> taskMaxBet,
            out int ibetStake, out int isnStake)
        {
            bool isSuccess = false;
            int isnMax = taskMaxBet.Result;
            if (isnMax == 0)
            {
                ibetStake = 0;
                isnStake = 0;
                return false;
            }
            Random r = new Random();
            switch (betStakeType)
            {
                case eBetStakeType.Ibet:
                    ibetStake = BetStake[r.Next(BetStake.Count)];
                    isnStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax
                        && isnStake >= isnMin && isnStake <= isnMax
                        && ibetStake != 0 && isnStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Sbo:
                    isnStake = BetStake[r.Next(BetStake.Count)];
                    ibetStake = (int)Math.Round((double)((isnStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax && isnStake >= isnMin && isnStake <= isnMax
                        && ibetStake != 0 && isnStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Max:
                    ibetStake = ibetMax;
                    isnStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (isnStake <= isnMax && ibetStake != 0 && isnStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }

                    isnStake = isnMax;
                    ibetStake = (int)Math.Round((double)((isnStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake <= ibetMax && ibetStake != 0 && isnStake != 0)
                    {
                        isSuccess = true;
                    }
                    break;
                default:
                    throw new Exception("Not support eBetStakeType");
            }

            if (isSuccess)
            {
                var isIbetMax = IsMaxStake(ibetMatchId, PiBetPool, IbetMaxStakeMatch, ibetStake);
                var isSboMax = IsMaxStake(sboMatchId, IsnBetPool, SboMaxStakeMatch, isnStake);
                if (isSboMax || isIbetMax
                    || ibetStake > PiEngine.AvailabeCredit || isnStake > IsnEngine.AvailabeCredit)
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
            if (serverType == eServerType.Pina)
            {
                PiEngine.UpdateAvailabeCredit();
            }
            else
            {
                IsnEngine.UpdateAvailabeCredit();
            }
        }

        private void FireLogScan(MatchPiDTO piMatchOdd, OddDTO piOdd, MatchIsnDTO isnMatch, OddDTO isnOdd, eServerScan serverScan)
        {
            var msg = new LogScanMessage()
            {
                ProcessorName = this.ProcessorName,
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                HomeTeamName = piMatchOdd.HomeTeamName,
                AwayTeamName = piMatchOdd.AwayTeamName,
                OddType = piOdd.OddType,
                Odd = piOdd.Odd,
                FirstOddValues = piOdd.HomeOdd + " | " + piOdd.AwayOdd,
                SecondOddValues = isnOdd.HomeOdd + " | " + isnOdd.AwayOdd,
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
                PiEngine.UpdateAvailabeCredit();
                IsnEngine.UpdateAvailabeCredit();
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
