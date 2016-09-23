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
using BcWin.Engine.PinnacleSports;
using BcWin.Processor.Interface;
using BcWin.Processor.ScanDriver;
using BcWin.Processor.Service;
using BCWin.Engine.Sbo;
using log4net;

namespace BcWin.Processor
{
    public partial class PiSboProcessor
    {
        public eServiceStatus Status { get; set; }

        public PiEngine PiEngine { get; set; }

        public SboEngine SboEngine { get; set; }

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
        public Dictionary<string, int> SboBetPool { get; set; }
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PiSboProcessor));

        public int StartFailCount { get; set; }
        public string ProcessorName { get; set; }

        public PiSboProcessor(ServerCallback serverCallback)
        {
            StartFailCount = 0;
            PiEngine = new PiEngine();

            SboEngine = new SboEngine();
        }

        public void Initialize()
        {
            if (SboEngine.CheckLogin())
            {
                if (SboEngine.Status == eServiceStatus.Unknown)
                {
                    SboBetPool = new Dictionary<string, int>();
                    SboEngine.OnExceptionEvent += OnExceptionEvent;
                    SboEngine.InitEngine();
                }
                PiBetPool = new Dictionary<string, int>();
                PiEngine.InitEngine();
                Status = eServiceStatus.Initialized;
            }
        }

        private bool IsReConnect;

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
            return false;
            //return ReConnectSbo();
        }

        private void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            if (serverType == eServerType.Sbo)
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
                SboEngine.LogOff();
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

        List<double> AddToCompare = new List<double>() { 0, 0.001, 0.002, 0.003, 0.004, 0.005 };
        public eServiceStatus Start(eScanType scanType)
        {
            Random r = new Random();
            CompareValueDifferent = ProcessorConfigInfo.CompareValue + AddToCompare[r.Next(AddToCompare.Count)];

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

            if (ProcessorConfigInfo.AccountFirst.ServerType == eServerType.Sbo)
            {
                IbetMaxStakeMatch = ProcessorConfigInfo.AccountSecond.MaxStake;
                IbetRateExchange = ProcessorConfigInfo.AccountSecond.RateExchange;
                SboMaxStakeMatch = ProcessorConfigInfo.AccountFirst.MaxStake;
                SboRateExchange = ProcessorConfigInfo.AccountFirst.RateExchange;
            }
            else
            {
                IbetMaxStakeMatch = ProcessorConfigInfo.AccountFirst.MaxStake;
                IbetRateExchange = ProcessorConfigInfo.AccountFirst.RateExchange;
                SboMaxStakeMatch = ProcessorConfigInfo.AccountSecond.MaxStake;
                SboRateExchange = ProcessorConfigInfo.AccountSecond.RateExchange;
            }

            try
            {
                Initialize();
                ProcessorName = PiEngine.UserName + " - " + SboEngine.UserName;

                SboEngine.EngineName = ProcessorName;
                SboEngine.Rebet = ProcessorConfigInfo.RebetSbo;
                SboEngine.UpdateAvailabeCredit();
                SboEngine.StartScanEngine(scanType);

                PiEngine.EngineName = ProcessorName;
                PiEngine.UpdateAvailabeCredit();
                PiEngine.StartScanEngine(scanType);

                objUpdateCreditTimer = new Timer(WaitUpdateCreditCallback, null, 60000 * 5, 60000 * 5);
                StartFailCount = 0;

                SboEngine.UpdateLiveDataChange += OnSboUpdateScan;
                PiSboDriver.OnSboScanUpdate += PiSboDriver_OnSboScanUpdate;
                //_driver.OnSboScanUpdate += OnSboUpdateScan;

                FireProcessStateChange(eServiceStatus.Started, ProcessorName);
                Status = eServiceStatus.Started;
            }
            catch (Exception ex)
            {
                Logger.Error("Start Processor Fail !", ex);

                StartFailCount++;

                PiEngine.Dispose();
                SboEngine.Dispose();
                SboEngine.Dispose();
                Status = eServiceStatus.Unknown;
            }

            return Status;
        }

        void PiSboDriver_OnSboScanUpdate(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time)
        {
            Task.Run(() => OnPingEvent(time, eServerType.Sbo));

            List<MatchPiDTO> piMatch;
            lock (PiEngine.LockLive)
            {
                piMatch = PiEngine._matchs;
            }

            CompareOdd(updatedDataSbo, piMatch, true);
        }

        //private bool isProcess;
        // private void OnSboUpdateScan(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time)
        private void OnSboUpdateScan(List<MatchOddDTO> updatedDataSbo, bool isLive, int a)
        {
            //if (LastBetTime.AddSeconds(TimeOffStakeOdds) > time || isProcess)
            //{
            //    return;
            //}

            Task.Run(() => OnPingEvent(DateTime.Now, eServerType.Ibet));
            if (isLive)
            {
                List<MatchPiDTO> piMatch;
                lock (PiEngine.LockLive)
                {
                    piMatch = PiEngine._matchs;
                }

                CompareOdd(updatedDataSbo, piMatch, true);
            }
        }

        public void Pause()
        {
            //_driver.OnSboScanUpdate -= OnSboUpdateScan;
            SboEngine.UpdateLiveDataChange -= OnSboUpdateScan;
            PiSboDriver.OnSboScanUpdate -= PiSboDriver_OnSboScanUpdate;

            PiEngine.PauseScan();

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
            Pause();

            SboEngine.OnExceptionEvent -= OnExceptionEvent;

            if (objUpdateCreditTimer != null)
            {
                objUpdateCreditTimer.Dispose();
            }

            SboEngine.LogOff();
            Status = eServiceStatus.Stopped;
            StartFailCount = 0;
        }

        //// compare from sbo (dataUpdated) to Pi (targetSource)
        public void CompareOdd(List<MatchOddDTO> dataUpdated, List<MatchPiDTO> targetSource, bool isLive)
        {
            try
            {
                foreach (var sbo in dataUpdated)
                {
                    if (!DataContainer.LeaguesDenyKeywords.Any(sbo.LeagueName.ToUpper().Contains)
                        && (!DataContainer.MatchsDenyKeywords.Any(sbo.HomeTeamName.ToUpper().Contains)
                            || !DataContainer.MatchsDenyKeywords.Any(sbo.AwayTeamName.ToUpper().Contains)))
                    {
                        MatchPiDTO piMatchTarget = targetSource.FirstOrDefault(m =>
                            (String.Equals(m.HomeTeamName, sbo.HomeTeamName,
                                StringComparison.CurrentCultureIgnoreCase)
                             ||
                             String.Equals(m.AwayTeamName, sbo.AwayTeamName,
                                 StringComparison.CurrentCultureIgnoreCase)));

                        if (piMatchTarget != null)
                        {
                            OddDTO piOdd = piMatchTarget.Odds.FirstOrDefault(o => o.PiOdd.Equals(sbo.Odd)
                                                                                 && o.OddType.Equals(sbo.OddType));

                            if (piOdd != null)
                            {
                                bool isValid1 = IsValidOddPair(sbo.HomeOdd, piOdd.AwayOdd, CompareValueDifferent)
                                    && IsValidPick(sbo, eBetType.Home);

                                if (isValid1)
                                {
                                    MatchOddDTO sboMatchSource = GetSboMatch(sbo, isLive);

                                    if (sboMatchSource != null)
                                    {
                                        //     if (data.HomeOdd.Equals(sboMatchSource.OldOdd) && data.HomeOdd != sboMatchSource.NewUpdateOdd)
                                        //     {
                                        //         continue;
                                        //     }

                                        sboMatchSource.HomeOdd = sbo.HomeOdd;
                                        sboMatchSource.AwayOdd = sbo.AwayOdd;

                                        ProcessPrepareBet(piMatchTarget, piOdd, eBetType.Away, sboMatchSource, eBetType.Home, isLive);
                                        return;
                                    }
                                }

                                bool isValid2 = IsValidOddPair(sbo.AwayOdd, piOdd.HomeOdd, CompareValueDifferent)
                                    && IsValidPick(sbo, eBetType.Away);

                                if (isValid2)
                                {
                                    MatchOddDTO sboMatchSource = GetSboMatch(sbo, isLive);

                                    if (sboMatchSource != null)
                                    {
                                        //       if (data.AwayOdd == sboMatchSource.OldOdd && data.AwayOdd != sboMatchSource.NewUpdateOdd)
                                        //       {
                                        //           continue;
                                        //       }

                                        sboMatchSource.HomeOdd = sbo.HomeOdd;
                                        sboMatchSource.AwayOdd = sbo.AwayOdd;

                                        ProcessPrepareBet(piMatchTarget, piOdd, eBetType.Home, sboMatchSource, eBetType.Away, isLive);
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
                result = firstOdd + secondOdd > 0.02;

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

        public bool IsValidPick(MatchOddDTO sboMatchOdd, eBetType betType)
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
        public bool ProcessPrepareBet(MatchPiDTO piMatchOdd, OddDTO piOdd, eBetType piBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive,
            eServerScan serverScan = eServerScan.Local)
        {
            if (isProcess)
            {
                return false;
            }
            string oddCheck = sboMatchOdd.OddID + sboMatchOdd.HomeOdd + sboMatchOdd.AwayOdd + piOdd.HomeOdd +
                              piOdd.AwayOdd;

            if (oddCheck.Equals(OddCheck))
            {
                return false;
            }

            OddCheck = oddCheck;

            try
            {
                isProcess = true;

                sboMatchOdd.OldOdd = sboBetType == eBetType.Home ? sboMatchOdd.HomeOdd : sboMatchOdd.AwayOdd;

                var prepareBetSbo = CallPrepareBet(sboMatchOdd, sboBetType, isLive, piOdd.PiOdd);

                if (prepareBetSbo == null)
                {
                    return false;
                }

                bool betAny;
                float piOddValue = piBetType == eBetType.Home ? piOdd.HomeOdd : piOdd.AwayOdd;
                bool isGood = IsGoodOddPair(piOddValue, prepareBetSbo.NewOdd, CompareValueDifferent, out betAny);

                if (isGood)
                {
                    int piStake;
                    int sboStake;

                    if (CaculateStake(BetStakeType,
                        piMatchOdd.MatchID,
                        150,
                        (int)piOdd.max.Value,
                        prepareBetSbo.MatchOdd.MatchID,
                        prepareBetSbo.MinBet,
                        prepareBetSbo.MaxBet,
                        out piStake, out sboStake))
                    {
                        var piConfirmBet = PiEngine.ConfirmBet(piStake, piMatchOdd, piOdd, piBetType);

                        if (piConfirmBet)
                        {
                            if (SboEngine.ConfirmBet(sboStake, piOdd.PiOdd, isLive, serverScan, piMatchOdd.HomeTeamName, piMatchOdd.AwayTeamName))
                            {
                                Task.Run(
                                    () =>
                                    {
                                        UpdateBetPool(
                                            prepareBetSbo.MatchOdd.ServerType,
                                            prepareBetSbo.MatchOdd.MatchID,
                                            SboBetPool, sboStake);
                                        UpdateBetPool(
                                            eServerType.Pina,
                                            piMatchOdd.MatchID,
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
                                if (BetAgainstIbet)
                                {
                                    Logger.Info(ProcessorName + " DAT NGUOC PINA<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
                                    var betTypeAgaint = GetAgainstBetType(piBetType);

                                    if (!PiEngine.TryConfirmBet(piStake, piMatchOdd, piOdd, betTypeAgaint))
                                    {
                                        Thread.Sleep(15000);
                                        Logger.Info("CALL LAN 2 BET MGUOC PINA<<<<<<<<<<<<<<<<");
                                        if (PiEngine.TryConfirmBet(piStake, piMatchOdd, piOdd, betTypeAgaint))
                                        {
                                            isProcess = false;
                                            return true;
                                        }
                                        else
                                        {
                                            Logger.Info("CALL LAN 3 BET MGUOC LAI PINA<<<<<<<<<<<<<<<<");
                                            prepareBetSbo = CallPrepareBet(sboMatchOdd, sboBetType, isLive, piOdd.PiOdd);
                                            if (prepareBetSbo == null)
                                            {
                                                Logger.Fatal("CALL LAN 3 Prepare Sbo fail!!!!!");
                                                Thread.Sleep(25000);
                                                if (PiEngine.TryConfirmBet(piStake, piMatchOdd, piOdd, betTypeAgaint))
                                                {
                                                    isProcess = false;
                                                    return true;
                                                }
                                            }
                                            else
                                            {
                                                if (!SboEngine.ConfirmBet(sboStake, piOdd.PiOdd, isLive, serverScan, piMatchOdd.HomeTeamName, piMatchOdd.AwayTeamName))
                                                {
                                                    Thread.Sleep(25000);
                                                    if (PiEngine.TryConfirmBet(piStake, piMatchOdd, piOdd, betTypeAgaint))
                                                    {
                                                        isProcess = false;
                                                        return true;
                                                    }
                                                    else
                                                    {
                                                        prepareBetSbo = CallPrepareBet(sboMatchOdd, sboBetType, isLive, piOdd.PiOdd);
                                                        if (prepareBetSbo == null)
                                                        {
                                                            Logger.Fatal("CALL LAN 4 Prepare Sbo fail!!!!!");
                                                            Thread.Sleep(25000);
                                                            PiEngine.TryConfirmBet(piStake, piMatchOdd, piOdd,
                                                                betTypeAgaint);
                                                        }
                                                        else
                                                        {
                                                            SboEngine.ConfirmBet(sboStake, piOdd.PiOdd, isLive, serverScan, piMatchOdd.HomeTeamName, piMatchOdd.AwayTeamName);
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    isProcess = false;
                                                    return true;
                                                }
                                            }
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
                                    UpdateBetPool(eServerType.Pina, piMatchOdd.MatchID, PiBetPool, piStake));

                            return true;
                        }

                        FireLogScan(piMatchOdd, piOdd, sboMatchOdd, serverScan);
                    }
                }
                else
                {
                    FireLogScan(piMatchOdd, piOdd, sboMatchOdd, serverScan);
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

        private PrepareBetDTO CallPrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float oddDef)
        {
            switch (matchOdd.ServerType)
            {
                //case eServerType.Ibet:
                //    return PiEngine.PrepareBet(matchOdd, betType, isLive, oddDef);
                case eServerType.Sbo:
                    return SboEngine.PrepareBet(matchOdd, betType, isLive, oddDef);
                default:
                    throw new Exception("CallPrepareBet => FAIL : Unknow matchOdd->eServerType param");
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
                var isIbetMax = IsMaxStake(ibetMatchId, PiBetPool, IbetMaxStakeMatch, ibetStake);
                var isSboMax = IsMaxStake(sboMatchId, SboBetPool, SboMaxStakeMatch, sboStake);
                if (isSboMax || isIbetMax || ibetStake > PiEngine.AvailabeCredit || sboStake > SboEngine.AvailabeCredit)
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
                SboEngine.UpdateAvailabeCredit();
            }
        }

        private void FireLogScan(MatchPiDTO piMatchOdd, OddDTO piOdd, MatchOddDTO sboMatchOddDto, eServerScan serverScan)
        {
            var msg = new LogScanMessage()
            {
                ProcessorName = this.ProcessorName,
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                HomeTeamName = piMatchOdd.HomeTeamName,
                AwayTeamName = piMatchOdd.AwayTeamName,
                OddType = piOdd.OddType,
                Odd = piOdd.PiOdd,
                FirstOddValues = piOdd.HomeOdd + " | " + piOdd.AwayOdd,
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
                PiEngine.UpdateAvailabeCredit();
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
