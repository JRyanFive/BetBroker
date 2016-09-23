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
    public partial class PiIsnProcessor2
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

        public PiIsnProcessor2()
        {
            StartFailCount = 0;
            PiEngine = new PiEngine();
            IsnEngine = new IsnEngine();
        }

        public void Initialize()
        {
            if (IsnEngine.CheckLogin())
            {
                if (IsnEngine.Status == eServiceStatus.Unknown)
                {
                    IsnBetPool = new Dictionary<string, int>();
                    IsnEngine.OnExceptionEvent += OnExceptionEvent;
                    IsnEngine.InitEngine();
                }
                PiBetPool = new Dictionary<string, int>();
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

        private void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            if (serverType == eServerType.Isn)
            {
                Thread thread = new Thread(ReStartFromIsn);
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
                IsnEngine.StartScanEngine(scanType);

                PiEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                PiEngine.EngineName = ProcessorName;
                PiEngine.UpdateAvailabeCredit();
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

                if (objUpdateCreditTimer != null)
                {
                    objUpdateCreditTimer.Dispose();
                }

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
                                    //bool isValid1 = IsValidOddPair(isnOdd.HomeOdd, piOdd.AwayOdd, CompareValueDifferent);
                                    bool isValid1 = IsValidOddPair(isnOdd.HomeOdd, piOdd.AwayOdd, CompareValueDifferent);
                                    //Debug.WriteLine(isnOdd.HomeOdd + "|" + piOdd.AwayOdd);

                                    //   && IsValidPick(isnOdd, eBetType.Home);

                                    if (isValid1)
                                    {
                                        ProcessPrepareBet(piMatchTarget, piOdd, eBetType.Away, isnMatch, isnOdd, eBetType.Home, isLive);
                                        return;

                                    }

                                    bool isValid2 = IsValidOddPair(isnOdd.AwayOdd, piOdd.HomeOdd, CompareValueDifferent);
                                    //Debug.WriteLine(isnOdd.AwayOdd + "|" + piOdd.HomeOdd);

                                    //      && IsValidPick(isnOdd, eBetType.Away);

                                    if (isValid2)
                                    {

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
            if (firstOdd == 0 && secondOdd == 0)
            {
                return false;
            }

            if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet)//|| firstOdd == 0 || secondOdd == 0
            {
                return false;
            }

            if ((firstOdd < 0f && secondOdd < 0f) || Math.Abs(firstOdd + secondOdd) == 2)
                return true;
            bool result = false;
            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //  result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = firstOdd + secondOdd >= 0;

                if (!result)
                {
                    result = Math.Abs(firstOdd + secondOdd) <= oddValueDifferent;
                }
            }
            return result;
        }

        public bool IsGoodOddPair(float firstOdd, float secondOdd, double oddValueDifferent, out bool betAny)
        {
            betAny = false;

            if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet)
            {
                return false;
            }

            bool result = false;

            if (firstOdd < 0f && secondOdd < 0f)
            {
                betAny = true;
                return true;
            }
            if (Math.Abs(firstOdd + secondOdd) == 2)
            {
                return true;
            }

            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                //result = Math.Round((firstOdd + secondOdd), 2) >= 0;
                result = firstOdd + secondOdd >= 0;

                if (!result)
                {
                    result = Math.Abs(firstOdd + secondOdd) <= oddValueDifferent;
                }
                else
                {
                    betAny = true;
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
                Stopwatch a = new Stopwatch();
                a.Start();
                Task<int> taskMaxBet = Task.Run(() => IsnEngine.GetMax((isnBetType == eBetType.Home ? isnOdd.selectionIdHome : isnOdd.selectionIdAway)));

                var prepareBetIsn = Task.Run(() => IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd));

                var preparePina = PiEngine.PrepareBet(piMatch, piOdd, piBetType, piOdd.Odd);

                if (preparePina == null || prepareBetIsn.Result == null)
                {
                    FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);
                    return false;
                }

                a.Stop();
                Logger.Info("Total time prepare: " + a.ElapsedMilliseconds + " ms");
                // Debug.WriteLine("Thoi gian prepare: " + a.ElapsedMilliseconds);

                bool betAny;
                //   float piOddValue = piBetType == eBetType.Home ? piOdd.HomeOdd : piOdd.AwayOdd;
                //bool isGood = IsGoodOddPair(piOddValue, prepareBetIsn.NewOdd, CompareValueDifferent, out betAny);
                bool isGood = IsGoodOddPair(preparePina.NewOdd, prepareBetIsn.Result.NewOdd, CompareValueDifferent, out betAny);
                // Logger.Debug("After isGood=" + isGood.ToString() + " " + piOddValue + "_" + prepareBetIsn.NewOdd + " " + piBetType.ToString());

                if (isGood)
                {
                    int piStake;
                    int isnStake;

                    if (CaculateStake(BetStakeType,
                       piMatch.MatchID, 150, (int)((PendingTicket)preparePina.PendingTicket).TicketItem.MaxRiskAmount,
                      isnMatch.MatchID, 4, taskMaxBet,
                       out piStake, out isnStake))
                    // if (true)
                    {
                        a.Restart();
                        Logger.Info("Total time prepare: " + a.ElapsedMilliseconds + " ms");

                        var taskpiConfirmBet = Task.Run(() => PiEngine.ConfirmBet((PendingTicket)preparePina.PendingTicket, piStake, piMatch, piOdd, piBetType, eBetStatusType.Success));
                        var betisnOk = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType, eBetStatusType.Success);
                        if (taskpiConfirmBet.Result == true && betisnOk == true)
                        {
                            a.Stop();
                            Logger.Info("Total time prepare: " + a.ElapsedMilliseconds + " ms");

                            Task.Run(
                                () =>
                                {
                                    UpdateBetPool(
                                         eServerType.Isn,
                                        isnMatch.MatchID,
                                        IsnBetPool,
                                        isnStake);

                                    UpdateBetPool(
                                        eServerType.Pina,
                                        piMatch.MatchID,
                                        PiBetPool,
                                        piStake);
                                });
                            Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                            CountBet++;
                            LastBetTime = DateTime.Now;
                            return true;
                        }
                        else if (taskpiConfirmBet.Result == false && betisnOk == false)
                        {
                            return false;
                        }
                        else if (taskpiConfirmBet.Result == true)
                        {
                            Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));

                            bool ok = false;
                            for (int i = 0; i < IsnEngine.Rebet; i++)
                            {
                                var prepare = IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, true, isnOdd.Odd);
                                if (prepare != null)
                                {
                                    ok = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType, eBetStatusType.MissOddIsn,true);
                                    if (ok)
                                    {
                                        LastBetTime = DateTime.Now;

                                        Task.Run(() => UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake));
                                        return true;
                                    }
                                }
                                Thread.Sleep(500);
                            }

                            if (!ok) // ReBet pina
                            {
                                var betTypeAgaint = GetAgainstBetType(piBetType);
                                var prepare = PiEngine.PrepareBet(piMatch, piOdd, betTypeAgaint, piOdd.Odd);
                                if (prepare != null)
                                {
                                    ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, piMatch, piOdd, betTypeAgaint, eBetStatusType.BetAgainPina,true);
                                    if (ok)
                                    {
                                        LastBetTime = DateTime.Now;
                                        Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));
                                        return true;
                                    }
                                }
                            }

                            IsnEngine.FireLogBet(isnMatch, isnOdd, isnBetType, isnStake, eBetStatusType.Fail, eServerScan.Local);
                        }
                        else if (betisnOk == true)
                        {
                            Task.Run(() => UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake));

                            bool ok = false;
                            for (int i = 0; i < PiEngine.Rebet; i++)
                            {
                                var prepare = PiEngine.PrepareBet(piMatch, piOdd, piBetType, piOdd.Odd);
                                if (prepare != null)
                                {
                                    ok = PiEngine.ConfirmBet((PendingTicket)prepare.PendingTicket, piStake, piMatch, piOdd, piBetType, eBetStatusType.MissOddPina,true);
                                    if (ok)
                                    {
                                        LastBetTime = DateTime.Now;

                                        Task.Run(() => UpdateBetPool(eServerType.Pina, piMatch.MatchID, PiBetPool, piStake));

                                        return true;
                                    }
                                }
                                Thread.Sleep(500);
                            }

                            if (!ok)// Rebet Isn
                            {
                                var betTypeAgaint = GetAgainstBetType(isnBetType);
                                var prepare = IsnEngine.PrepareBet(isnMatch, isnOdd, betTypeAgaint, true, isnOdd.Odd);
                                if (prepare != null)
                                {
                                    ok = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, betTypeAgaint, eBetStatusType.BetAgainIsn,true);
                                    if (ok)
                                    {
                                        LastBetTime = DateTime.Now;
                                        Task.Run(() => UpdateBetPool(eServerType.Isn, isnMatch.MatchID, IsnBetPool, isnStake));
                                        return true;
                                    }
                                }
                            }

                            PiEngine.FireLogBet(piMatch, piOdd, piBetType, piStake, eBetStatusType.Fail, eServerScan.Local);

                        }

                        return false;

                    }

                    // tạm đóng
                    FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);
                }
                //else
                //{
                //    // tạm đóng
                //    FireLogScan(piMatch, piOdd, isnMatch, isnOdd, serverScan);
                //}

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

        /*
        public bool ProcessPrepareBet2(MatchPiDTO piMatch, OddDTO piOdd, eBetType piBetType, MatchIsnDTO isnMatch, OddDTO isnOdd, eBetType isnBetType, bool isLive,
           eServerScan serverScan = eServerScan.Local)
        {
            if (isProcess)
            {
                return false;
            }



            try
            {
                isProcess = true;


                Task<int> taskMaxBet = Task.Run(() => IsnEngine.GetMax((isnBetType == eBetType.Home ? isnOdd.selectionIdHome : isnOdd.selectionIdAway)));

                var taskpreparePina = Task.Run(() => PiEngine.PrepareBet(piMatch, piOdd, piBetType));
                if (taskpreparePina.Result == null)
                {
                    return false;
                }
                var prepareBetIsn = IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd);
                if (prepareBetIsn == null)
                {
                    return false;
                }

                bool betAny;
                float piOddValue = piBetType == eBetType.Home ? piOdd.HomeOdd : piOdd.AwayOdd;
                bool isGood = IsGoodOddPair(piOddValue, prepareBetIsn.NewOdd, CompareValueDifferent, out betAny);

                if (isGood)
                {
                    int piStake;
                    int isnStake;

                    if (CaculateStake(BetStakeType,
                       piMatch.MatchID, 150, (int)piOdd.max.Value,
                      isnMatch.MatchID, 4, taskMaxBet,
                       out piStake, out isnStake))
                    // if (true)
                    {
                        var isnConfirmBet = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType);

                        if (isnConfirmBet)
                        {
                            if (PiEngine.ConfirmBet(piStake, piMatch, piOdd, piBetType))
                            {
                                Task.Run(
                                    () =>
                                    {
                                        UpdateBetPool(
                                             eServerType.Isn,
                                            isnMatch.MatchID,
                                            IsnBetPool,
                                            isnStake);

                                        UpdateBetPool(
                                            eServerType.Pina,
                                            piMatch.MatchID,
                                            PiBetPool,
                                            piStake);
                                    });
                                Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                CountBet++;
                                LastBetTime = DateTime.Now;
                                return true;
                            }
                            else
                            {
                                for (int i = 0; i < IsnEngine.Rebet; i++)
                                {
                                    var a = PiEngine.TryConfirmBet(piStake, piMatch, piOdd, piBetType);
                                    if (a == true)
                                    {
                                        return true;
                                    }
                                }

                                PiEngine.FireLogBet(piMatch, piOdd, piBetType, piStake, eBetStatusType.Fail, eServerScan.Local);
                            }

                            LastBetTime = DateTime.Now;


                            return true;
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
        */

        /*
        /// <summary>
        /// Mr Kiệt version
        /// </summary>
        /// <param name="piMatch"></param>
        /// <param name="piOdd"></param>
        /// <param name="piBetType"></param>
        /// <param name="isnMatch"></param>
        /// <param name="isnOdd"></param>
        /// <param name="isnBetType"></param>
        /// <param name="isLive"></param>
        /// <param name="serverScan"></param>
        /// <returns></returns>
        public bool ProcessPrepareBet3(MatchPiDTO piMatch, OddDTO piOdd, eBetType piBetType, MatchIsnDTO isnMatch, OddDTO isnOdd, eBetType isnBetType, bool isLive,
     eServerScan serverScan = eServerScan.Local)
        {
            if (isProcess)
            {
                return false;
            }

            try
            {
                isProcess = true;


                Task<int> taskMaxBet = Task.Run(() => IsnEngine.GetMax((isnBetType == eBetType.Home ? isnOdd.selectionIdHome : isnOdd.selectionIdAway)));

                var taskpreparePina = Task.Run(() => PiEngine.PrepareBet(piMatch, piOdd, piBetType));
                if (taskpreparePina.Result == null)
                {
                    return false;
                }
                var prepareBetIsn = IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd);
                if (prepareBetIsn == null)
                {
                    return false;
                }

                bool betAny;
                float piOddValue = piBetType == eBetType.Home ? piOdd.HomeOdd : piOdd.AwayOdd;
                bool isGood = IsGoodOddPair(piOddValue, prepareBetIsn.NewOdd, CompareValueDifferent, out betAny);

                if (isGood)
                {
                    int piStake;
                    int isnStake;

                    if (CaculateStake(BetStakeType,
                       piMatch.MatchID, 150, (int)piOdd.max.Value,
                      isnMatch.MatchID, 4, taskMaxBet,
                       out piStake, out isnStake))
                    {
                        // bet kèo dưới trước
                        #region [Bet pina trước nếu là kèo dưới]
                        if (((piOdd.OddType == eOddType.HCP || piOdd.OddType == eOddType.HalfHCP) && piOdd.Odd >= 0)// odd dương là được chấp
                            || ((piOdd.OddType == eOddType.OU || piOdd.OddType == eOddType.HalfOU) && piBetType == eBetType.Away))// dựa vào pina bet type để biết là Under
                        {
                            var piConfirmBet = PiEngine.ConfirmBet(piStake, piMatch, piOdd, piBetType);

                            if (piConfirmBet)
                            {
                                if (IsnEngine.ConfirmBet(isnStake, isnMatch, isnOdd, isnBetType))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(
                                                 eServerType.Isn,
                                                isnMatch.MatchID,
                                                IsnBetPool,
                                                isnStake);

                                            UpdateBetPool(
                                                eServerType.Pina,
                                                piMatch.MatchID,
                                                PiBetPool,
                                                piStake);
                                        });
                                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    CountBet++;
                                    LastBetTime = DateTime.Now;
                                    return true;
                                }
                                else
                                {
                                    // rebet Pina
                                    //for (int i = 0; i < IsnEngine.Rebet; i++)
                                    //{
                                    //    var a = PiEngine.TryConfirmBet(piStake, piMatch, piOdd, piBetType);
                                    //    if (a == true)
                                    //    {
                                    //        return true;
                                    //    }
                                    //}

                                    PiEngine.FireLogBet(piMatch, piOdd, piBetType, piStake, eBetStatusType.Fail, eServerScan.Local);

                                }
                            }

                        }
                        #endregion
                        #region [Bet ISN trước nếu isn là kèo dưới]
                        else // OU
                        {
                            var isnConfirmBet = IsnEngine.ConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType);

                            if (isnConfirmBet)
                            {
                                if (PiEngine.ConfirmBet2(piStake, piMatch, piOdd, piBetType))
                                {
                                    Task.Run(
                                        () =>
                                        {
                                            UpdateBetPool(
                                                 eServerType.Isn,
                                                isnMatch.MatchID,
                                                IsnBetPool,
                                                isnStake);

                                            UpdateBetPool(
                                                eServerType.Pina,
                                                piMatch.MatchID,
                                                PiBetPool,
                                                piStake);
                                        });
                                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                    CountBet++;
                                    LastBetTime = DateTime.Now;
                                    return true;
                                }
                                else
                                {
                                    for (int i = 0; i < IsnEngine.Rebet; i++)
                                    {
                                        //call prepare
                                        prepareBetIsn = IsnEngine.PrepareBet(isnMatch, isnOdd, isnBetType, isLive, isnOdd.Odd);
                                        if (prepareBetIsn == null)
                                        {
                                            continue;
                                        }

                                        var a = IsnEngine.TryConfirmBet2(isnStake, isnMatch, isnOdd, isnBetType);
                                        if (a == true)
                                        {
                                            return true;
                                        }
                                    }


                                }

                                LastBetTime = DateTime.Now;


                                return true;
                            }

                        }
                        #endregion



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
         */

        private eBetType GetAgainstBetType(eBetType betType)
        {
            return betType == eBetType.Home ? eBetType.Away : eBetType.Home;

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
            string sboMatchId, int sboMin, Task<int> taskMaxBet,
            out int ibetStake, out int sboStake)
        {
            bool isSuccess = false;
            int isnMax = taskMaxBet.Result;
            if (isnMax == 0)
            {
                ibetStake = 0;
                sboStake = 0;
                return false;
            }
            Random r = new Random();
            switch (betStakeType)
            {
                case eBetStakeType.Ibet:
                    ibetStake = BetStake[r.Next(BetStake.Count)];
                    sboStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax
                        && sboStake >= sboMin && sboStake <= isnMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Sbo:
                    sboStake = BetStake[r.Next(BetStake.Count)];
                    ibetStake = (int)Math.Round((double)((sboStake * SboRateExchange) / IbetRateExchange), 0);
                    if (ibetStake >= ibetMin && ibetStake <= ibetMax && sboStake >= sboMin && sboStake <= isnMax
                        && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }
                    goto case eBetStakeType.Max;
                case eBetStakeType.Max:
                    ibetStake = ibetMax;
                    sboStake = (int)Math.Round((double)((ibetStake * IbetRateExchange) / SboRateExchange), 0);
                    if (sboStake <= isnMax && ibetStake != 0 && sboStake != 0)
                    {
                        isSuccess = true;
                        break;
                    }

                    sboStake = isnMax;
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
                var isIbetMax = IsMaxStake(ibetMatchId, PiBetPool, IbetMaxStakeMatch, ibetStake);
                var isSboMax = IsMaxStake(sboMatchId, IsnBetPool, SboMaxStakeMatch, sboStake);
                if (isSboMax || isIbetMax
                    || ibetStake > PiEngine.AvailabeCredit || sboStake > IsnEngine.AvailabeCredit)
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
