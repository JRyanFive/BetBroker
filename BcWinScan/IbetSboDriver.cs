using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Processor;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;
using BCWin.Metadata;
using log4net;

namespace BcWinScan
{
    public class IbetSboDriver
    {
        public event FakeRequestEvent OnFakeRequest;

        public eServiceStatus Status;
        public eScanType ScanType;
        public IBcManageService ManageService;

        private bool _useRealIp;

        private readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboDriver));

        public List<SboEngine> SboScanEngines = new List<SboEngine>();

        public List<IbetSubEngine> IbetScanEngines = new List<IbetSubEngine>();

        private IBcScanService service;

        private System.Threading.Timer objCheckScanDriver;

        public IbetSboDriver()
        {
            ConnectScanServer();
            objCheckScanDriver = new System.Threading.Timer(WaitCheckCallbackScan, null, 0, 25000);
        }

        public void Start(eScanType scanType)
        {
            Status = eServiceStatus.Started;
            ScanType = scanType;
            //DataContainer.SourceIpFakes = IpData.SourceIp;

            if (ScanData.IpAddress != null && ScanData.IpAddress.Any())
            {
                _useRealIp = true;
            }

            StartLoginSbo();

            StartIbet();

            StartSbo();

            Logger.Info("KHOI DONG SCAN THANH CONG!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        public void StartIbet()
        {
            int i = 0;

            int countIp = 0;
            if (_useRealIp)
            {
                countIp = ScanData.IpAddress.Count;
            }

            Parallel.ForEach(ScanData.ScanAccount.IbetAccounts, ibetAc =>
             {
                 try
                 {
                     IbetSubEngine ibet1 = new IbetSubEngine();
                     ibet1.ProxyLogin = ScanData.ProxyLoginIbet;
                     ibet1.ProxyEndpoint = ScanData.ProxyLoginIbetEndpoint;

                     Random r = new Random();
                     if (_useRealIp)
                     {
                         var ip = ScanData.IpAddress[r.Next(countIp)];
                         ibet1.RealIpAddress = ip;
                     }

                     string url = DataContainer.IbetServers[r.Next(DataContainer.IbetServers.Count())];

                     var sourceIp = DataContainer.SourceIpFakes[r.Next(DataContainer.SourceIpFakes.Count)];
                     ibet1.IpFake = string.Concat(sourceIp, r.Next(2, 242));

                     if (_useRealIp)
                     {
                         ibet1.TryLogin(DataContainer.IbetServers.ToList(), url, ibetAc.Username, ibetAc.Password, ScanData.IpAddress);
                     }
                     else
                     {
                         ibet1.TryLogin(DataContainer.IbetServers.ToList(), url, ibetAc.Username, ibetAc.Password, null);
                     }

                     if (ibet1.AccountStatus == eAccountStatus.Online)
                     {
                         i++;

                         ibet1.OnExceptionEvent += OnExceptionEvent;
                         ibet1.StartScanDriver(ScanType);
                         IbetScanEngines.Add(ibet1);

                         Logger.InfoFormat("[COUNT = {0}] SCAN LOGIN IBET SUCCESS : " + ibetAc.Username, i);
                     }
                 }
                 catch (Exception ex)
                 {
                     Logger.Error(ex);
                 }
             });

            //foreach (var ibetAc in ScanData.ScanAccount.IbetAccounts)
            //{

            //}


            switch (ScanType)
            {
                case eScanType.Live:
                    StartUpdateIbetLiveDriver();
                    break;
                case eScanType.NonLive:
                    StartUpdateIbetNonLiveDriver();
                    break;
                case eScanType.All:
                    StartUpdateIbetLiveDriver();
                    StartUpdateIbetNonLiveDriver();
                    break;

            }

            Logger.InfoFormat("[TOTAL SUCCESS = {0}] START IBET SCAN THANH CONG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!", IbetScanEngines.Count);
        }

        private void StartUpdateIbetLiveDriver()
        {
            var sleppTime = SystemConfig.TIME_GET_UPDATE_LIVE_IBET / IbetScanEngines.Count;
            foreach (var ibetScanEngine in IbetScanEngines)
            {
                ibetScanEngine.IpFake = null;
                ibetScanEngine.StartUpdateLiveDriver();
                Thread.Sleep(sleppTime);
            }
        }

        private void StartUpdateIbetNonLiveDriver()
        {
            var sleppTime = SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET / IbetScanEngines.Count;
            foreach (var ibetScanEngine in IbetScanEngines)
            {
                ibetScanEngine.IpFake = null;
                ibetScanEngine.StartUpdateNonLiveDriver();
                Thread.Sleep(sleppTime);
            }
        }

        public void StartLoginSbo()
        {
            int i = 0;
            int countServer = DataContainer.SboServers.Count();
            int countIp = 0;
            if (_useRealIp)
            {
                countIp = ScanData.IpAddress.Count;
            }

            Parallel.ForEach(ScanData.ScanAccount.SboAccounts, sboAcc =>
            {
                try
                {
                    Random r = new Random();
                    var url = DataContainer.SboServers[r.Next(countServer)];

                    SboEngine sbo1 = new SboEngine();

                    if (_useRealIp)
                    {
                        var ip = ScanData.IpAddress[r.Next(countIp)];
                        sbo1.RealIpAddress = ip;
                    }

                    if (ScanData.HasFakeIpSbo)
                    {
                        sbo1.IpFake = IpHelper.GetRandomIp();
                    }

                    sbo1.OnFakeRequest += OnFakeRequest;

                    sbo1.TryLogin(url, sboAcc.Username, sboAcc.Password);

                    if (sbo1.AccountStatus == eAccountStatus.Online)
                    {
                        i++;
                        SboScanEngines.Add(sbo1);
                        Logger.Info("SBO COUNT = " + i);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                }
            });

            //foreach (var sboAcc in ScanData.ScanAccount.SboAccounts)
            //{

            //}

            Logger.Info("LOGIN SBO SCAN THANH CONG @@@@@@@@@@@@@@@@@@@ TONG ACC: " + SboScanEngines.Count);
        }

        public void StartSbo()
        {
            foreach (var sbo1 in SboScanEngines)
            {
                sbo1.UpdateLiveDataChange += sbobetUpdateChange_Event;
                sbo1.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
                sbo1.OnExceptionEvent += OnExceptionEvent;
                sbo1.StartScanDriver(ScanType);
            }

            switch (ScanType)
            {
                case eScanType.Live:
                    StartUpdateSboLiveDriver();
                    break;
                case eScanType.NonLive:
                    StartUpdateSboNonLiveDriver();
                    break;
                case eScanType.All:
                    StartUpdateSboLiveDriver();
                    StartUpdateSboNonLiveDriver();
                    break;

            }

            Logger.Info("START SBO SCAN THANH CONG>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }

        private void StartUpdateSboLiveDriver()
        {
            var timeSboLiveSleep = SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET / SboScanEngines.Count;
            int oType = 0;
            foreach (var sboScanEngine in SboScanEngines)
            {
                sboScanEngine.TypeGetScan = oType;
                sboScanEngine.StartUpdateLiveDriver();
                //Thread.Sleep(1000);
                Thread.Sleep(timeSboLiveSleep);

                oType = oType == 0 ? 1 : 0;
            }
        }

        private void StartUpdateSboNonLiveDriver()
        {
            var timeSboLiveSleep = SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET / SboScanEngines.Count;
            int oType = 0;
            foreach (var sboScanEngine in SboScanEngines)
            {
                sboScanEngine.TypeGetScanToday = oType;
                sboScanEngine.StartUpdateNonLiveDriver();
                //Thread.Sleep(1000);
                Thread.Sleep(timeSboLiveSleep);

                oType = oType == 0 ? 1 : 0;
            }
        }

        void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
            if (serverType == eServerType.Ibet)
            {
                Thread thread = new Thread(() =>
                {
                    ReStartIbet(obj);
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }
            else
            {
                Thread thread = new Thread(() =>
                {
                    ReStartSbo(obj);
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
            }

            //if (serverType == eServerType.Ibet)
            //{
            //    Thread thread = new Thread(ReStartFromIbet);
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.Start();
            //}
            //else
            //{
            //    Thread thread = new Thread(ReStartFromSbo);
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.Start();
            //}
        }

        private void ReStartIbet(object obj)
        {
            try
            {
                IbetSubEngine i = obj as IbetSubEngine;
                Logger.Info("TAM DUNG IBET SCAN<<<<<<<<<<<<<<<<<<");
                i.OnExceptionEvent -= OnExceptionEvent;
                i.LogOff();
                i.ProxyLogin = ScanData.ProxyLoginIbet;
                i.ProxyEndpoint = ScanData.ProxyLoginIbetEndpoint;

                if (i.ReLogin())
                {
                    i.OnExceptionEvent += OnExceptionEvent;
                    i.StartScanDriver(ScanType);
                    switch (ScanType)
                    {
                        case eScanType.Live:
                            i.StartUpdateLiveDriver();
                            break;
                        case eScanType.NonLive:
                            i.StartUpdateNonLiveDriver();
                            break;
                        case eScanType.All:
                            i.StartUpdateLiveDriver();
                            i.StartUpdateNonLiveDriver();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("BIG BUG", ex);
            }
        }

        private void ReStartSbo(object obj)
        {
            try
            {
                SboEngine s = obj as SboEngine;
                Logger.Info("TAM DUNG SBO SCAN<<<<<<<<<<<<<<<<<<");
                s.OnExceptionEvent -= OnExceptionEvent;
                s.UpdateLiveDataChange -= sbobetUpdateChange_Event;
                s.UpdateNonLiveDataChange -= sbobetUpdateChange_Event;
                s.LogOff();

                if (s.ReLogin())
                {
                    s.UpdateLiveDataChange += sbobetUpdateChange_Event;
                    s.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
                    s.OnExceptionEvent += OnExceptionEvent;
                    s.StartScanDriver(ScanType);

                    switch (ScanType)
                    {
                        case eScanType.Live:
                            s.StartUpdateLiveDriver();
                            break;
                        case eScanType.NonLive:
                            s.StartUpdateNonLiveDriver();
                            break;
                        case eScanType.All:
                            s.StartUpdateLiveDriver();
                            s.StartUpdateNonLiveDriver();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Fatal("BIG BUG", ex);
            }
        }

        private bool IsReConnect;

        public void Stop()
        {
            StopSbo();
            StopIbet();
        }

        public void StopSbo()
        {
            foreach (var sboEngine in SboScanEngines)
            {
                sboEngine.OnFakeRequest -= OnFakeRequest;
                sboEngine.UpdateLiveDataChange -= sbobetUpdateChange_Event;
                sboEngine.UpdateNonLiveDataChange -= sbobetUpdateChange_Event;
                sboEngine.OnExceptionEvent -= OnExceptionEvent;
                sboEngine.LogOff();
            }
            SboScanEngines = new List<SboEngine>();
        }

        public void StopIbet()
        {
            foreach (var ibetEngine in IbetScanEngines)
            {
                ibetEngine.OnExceptionEvent -= OnExceptionEvent;
                ibetEngine.LogOff();
            }

            IbetScanEngines = new List<IbetSubEngine>();
            DataContainer.LiveMatchOddBag = new BlockingCollection<MatchDTO>();
            DataContainer.NonLiveMatchOddBag = new BlockingCollection<MatchDTO>();
        }

        private void sbobetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive, int type = 0)
        {
            //Logger.Debug("IbetSboProcessor => sbo change event");
            if (updatedData != null && updatedData.Count > 0)
            {
                updatedData.Shuffle();

                try
                {
                    if (isLive)
                    {
                        CompareOdd2(updatedData, DataContainer.LiveMatchOddBag, isLive);
                    }
                    else
                    {
                        CompareOdd2(updatedData, DataContainer.NonLiveMatchOddBag, isLive);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    //return;
                }
            }
        }

        //public float LastOddCheck { get; set; }
        public string LastOddIdCheck { get; set; }
        public void CompareOdd2(List<MatchOddDTO> dataUpdated, BlockingCollection<MatchDTO> targetSource, bool isLive)
        {
            //TransactionDTO transaction = new TransactionDTO>();

            foreach (var data in dataUpdated)
            {
                if (!DataContainer.LeaguesDenyKeywords.Any(data.LeagueName.ToUpper().Contains)
                    && (!DataContainer.MatchsDenyKeywords.Any(data.HomeTeamName.ToUpper().Contains)
                        || !DataContainer.MatchsDenyKeywords.Any(data.AwayTeamName.ToUpper().Contains)))
                {
                    MatchDTO ibetMatchTarget = targetSource.FirstOrDefault(m =>
                        (String.Equals(m.HomeTeamName, data.HomeTeamName,
                            StringComparison.CurrentCultureIgnoreCase)
                         ||
                         String.Equals(m.AwayTeamName, data.AwayTeamName,
                             StringComparison.CurrentCultureIgnoreCase))
                        && !DataContainer.LeaguesDenyKeywords.Any(m.LeagueName.ToUpper().Contains));

                    if (ibetMatchTarget != null)
                    {
                        var ibetOdd = ibetMatchTarget.Odds.FirstOrDefault(o => o.Odd.Equals(data.Odd)
                            && o.OddType.Equals(data.OddType));

                        if (ibetOdd != null)
                        {
                            string oddCheck = ibetOdd.OddID + ibetOdd.HomeOdd + ibetOdd.AwayOdd +
                                         data.Odd + ibetOdd.Odd + data.HomeOdd + data.AwayOdd;

                            if (oddCheck.Equals(LastOddIdCheck))
                            {
                                continue;
                            }

                            if (IsValidOddPair(data.AwayOdd, ibetOdd.HomeOdd))
                            {
                                LastOddIdCheck = oddCheck;

                                BroadCastTransaction(new TransactionDTO
                                {
                                    IsLive = isLive,
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Away,
                                    IbetBetType = eBetType.Home,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                }, oddCheck, 0);
                                break;
                            }

                            if (IsValidOddPair(data.HomeOdd, ibetOdd.AwayOdd))
                            {
                                LastOddIdCheck = oddCheck;
                                BroadCastTransaction(new TransactionDTO
                                {
                                    IsLive = isLive,
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Home,
                                    IbetBetType = eBetType.Away,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                }, oddCheck, 0);

                                break;
                            }
                        }
                    }
                }
            }
        }

        private ChannelFactory<IBcScanService> cf;
        private void ConnectScanServer()
        {
            try
            {
                if (cf != null && (cf.State == CommunicationState.Opened || cf.State == CommunicationState.Opening))
                {
                    cf.Close();
                }

                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;
                EndpointAddress vEndPoint = new EndpointAddress(ScanData.EndpointRoute);
                cf = new ChannelFactory<IBcScanService>
                     (b, vEndPoint);
                service = cf.CreateChannel();
                var p = service.Ping();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void BroadCastTransaction(TransactionDTO tran, string oddIdCheck, float oddCheck)
        {
            try
            {
                service.TransactionNotify(DateTime.Now, tran, oddIdCheck, oddCheck);
            }
            catch (CommunicationException)
            {
                ConnectScanServer();
            }
            catch (Exception ex)
            {
                ConnectScanServer();
                Logger.Error("Ex", ex);
            }

            //var now = DateTime.Now;
            //ScanData.ClientConDic.AsParallel().ForAll(_ =>
            //{
            //    var back = _;

            //    try
            //    {
            //        back.Value.TransactionNotify(now, tran);
            //    }
            //    catch (CommunicationException)
            //    {
            //        UnRegisterClient(back.Key);
            //    }
            //    catch (Exception ex)
            //    {
            //        Logger.Error("Ex", ex);
            //    }
            //});

            //lock (ScanData.Lock)
            //{
            //    foreach (var clientCallBack in ScanData.ClientConDic)
            //    {
            //        var back = clientCallBack;

            //        try
            //        {
            //            back.Value.TransactionNotify(now, tran);
            //        }
            //        catch (CommunicationException)
            //        {
            //            UnRegisterClient(back.Key);
            //        }
            //        catch (Exception ex)
            //        {
            //            Logger.Error("Ex", ex);
            //        }

            //        //new Thread(() =>
            //        //{

            //        //}).Start();
            //    }

            //    //ScanData.ClientCallbacks = ScanData.ClientCallbacks.Shuffle();
            //}
        }

        public bool IsValidOddPair(float firstOdd, float secondOdd)
        {
            bool result = false;
            if ((firstOdd < 0f && secondOdd < 0f) || Math.Abs(firstOdd + secondOdd) == 2)
            {
                return true;
            }


            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                result = firstOdd + secondOdd >= 0;
                if (!result)
                {
                    result = Math.Abs(firstOdd + secondOdd) <= Process.OddCompare;
                    //result = oddsDiff <= 0.03;
                }
                //else
                //{
                //    oddsDiff = -1;
                //}
            }

            return result;
        }

        private void WaitCheckCallbackScan(object obj)
        {
            try
            {
                service.Ping();
            }
            catch (Exception ex)
            {
                objCheckScanDriver.Dispose();

                if (ScanData.IsDispose)
                {
                    return;
                }

                // TryConnectServerScan();
                ConnectScanServer();
                objCheckScanDriver = new System.Threading.Timer(WaitCheckCallbackScan, null, 0, 25000);
            }
        }
    }


}
