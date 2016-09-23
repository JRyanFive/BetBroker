using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Core.EventDelegate;
using BcWin.Core.Utils;
using BcWin.Processor;
using BCWin.Metadata;
using log4net;

namespace BcWinScan
{
    public class IbetSboDriver
    {
        public event FakeRequestEvent OnFakeRequest;

        public string IbetUrl { get; set; }
        public string SboUrl { get; set; }
        public eServiceStatus Status;

        private readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboDriver));

        public List<SboEngine> SboScanEngines = new List<SboEngine>();

        public List<IbetSubEngine> IbetScanEngines = new List<IbetSubEngine>();

        //private Thread broadcastClientThread;
        public IbetSboDriver()
        {
            //broadcastClientThread = new Thread(DoBroadcastTransaction);
            //broadcastClientThread.Start();
        }

        public void Start(string ibetUrl, string sboUrl)
        {
            Status = eServiceStatus.Started;
            IbetUrl = ibetUrl;
            SboUrl = sboUrl;
            StartIbet(ibetUrl);
            StartSbo(sboUrl);
        }
        public void StartIbet(string url)
        {
            Dictionary<string, string> ibetAccdics = new Dictionary<string, string>();
            var ibetScanAccs = System.IO.File.ReadAllLines(@"Config/IbetScanAccs.txt");
            foreach (var sboScanAcc in ibetScanAccs)
            {
                var a = sboScanAcc.Split(new[] { "<>" }, StringSplitOptions.None);
                ibetAccdics[a[0]] = a[1];
            }

            //ibetAccdics.AsParallel().ForAll(item =>
            //{

            //});

            foreach (var ibetAc in ibetAccdics)
            {
                IbetSubEngine ibet1 = new IbetSubEngine();
                while (ibet1.AccountStatus == eAccountStatus.Offline)
                {
                    ibet1.TryLogin(url, ibetAc.Key, ibetAc.Value);
                }

                ibet1.OnExceptionEvent += OnExceptionEvent;
                ibet1.StartScanLiveDriver();
                IbetScanEngines.Add(ibet1);

                Logger.Info("SCAN LOGIN IBET SUCCESS : " + ibetAc.Key);
            }

            int timeSleep = 6000;
            var sleppTime = timeSleep / ibetAccdics.Count;
            foreach (var ibetScanEngine in IbetScanEngines)
            {
                ibetScanEngine.StartUpdateLiveDriver(timeSleep);
                Thread.Sleep(sleppTime);
            }

            Logger.Info("START IBET SCAN THANH CONG!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        public void StartSbo(string url)
        {
            var sboScanAccs = System.IO.File.ReadAllLines(@"Config/SboScanAccs.txt");
            Dictionary<string, string> sboAccdics = new Dictionary<string, string>();
            foreach (var sboScanAcc in sboScanAccs)
            {
                var leagues = sboScanAcc.Split(new[] { "<>" }, StringSplitOptions.None);
                sboAccdics[leagues[0]] = leagues[1];
            }

            //sboAccdics.AsParallel().ForAll(item =>
            //{

            //});

            foreach (var sboAcc in sboAccdics)
            {
                SboEngine sbo1 = new SboEngine();
                sbo1.OnFakeRequest += OnFakeRequest;

                while (sbo1.AccountStatus == eAccountStatus.Offline)
                {
                    sbo1.TryLogin(url, sboAcc.Key, sboAcc.Value);
                }

                sbo1.UpdateLiveDataChange += sbobetUpdateChange_Event;
                sbo1.OnExceptionEvent += OnExceptionEvent;
                sbo1.StartScanLiveDriver();
                SboScanEngines.Add(sbo1);
            }

            var timeSboLiveSleep = SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET / sboAccdics.Count;
            int oType = 0;
            foreach (var sboScanEngine in SboScanEngines)
            {
                sboScanEngine.TypeGetScan = oType;
                sboScanEngine.StartUpdateLiveDriver();
                //Thread.Sleep(1000);
                Thread.Sleep(timeSboLiveSleep);

                oType = oType == 0 ? 1 : 0;
            }

            Logger.Info("START SBO SCAN THANH CONG>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }

        void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType)
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

        private void ReStartFromSbo()
        {
            if (!IsReConnect)
            {
                IsReConnect = true;
                StopSbo();

                Logger.Info("TAM DUNG HE THONG SCAN<<<<<<<<<<<<<<<<<<");

                StartSbo(SboUrl);
                IsReConnect = false;
            }

        }

        private bool IsReConnect;

        private void ReStartFromIbet()
        {
            if (!IsReConnect)
            {
                IsReConnect = true;

                StopIbet();

                Logger.Info("TAM DUNG HE THONG IBET SCAN<<<<<<<<<<<<<<<<<<");

                StartIbet(IbetUrl);

                IsReConnect = false;
            }
        }

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
            DataContainer.LiveMatchOddBag = new ConcurrentBag<MatchDTO>();
            DataContainer.NonLiveMatchOddBag = new ConcurrentBag<MatchDTO>();
        }

        void ibet1_UpdateLiveDataChange(List<MatchOddDTO> m, bool isLive)
        {
            if (m != null && m.Count > 0)
            {
                m.Shuffle();

                //Task.Factory.StartNew(() =>
                //{
                //    Thread.Sleep(10000);
                //    return "Return from Server : " + msg;
                //});

                foreach (var clientCallBack in ScanData.ClientConDic)
                {
                    var back = clientCallBack;
                    new Thread(() =>
                    {
                        try
                        {
                            back.Value.ScanNotify(DateTime.Now, m, isLive, eServerType.Ibet);
                        }
                        catch (CommunicationException)
                        {
                            UnRegisterClient(back.Key);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Ex", ex);
                        }
                    }).Start();
                }
            }
        }

        private void sbobetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive, int type = 0)
        {
            //Logger.Debug("IbetSboProcessor => sbo change event");
            if (updatedData != null && updatedData.Count > 0)
            {
                //updatedData.Shuffle();

                CompareOdd2(updatedData, DataContainer.LiveMatchOddBag, isLive);
                //lock (ScanData.Lock)
                //{
                //    foreach (var clientCallBack in ScanData.ClientCallbacks)
                //    {
                //        var back = clientCallBack;

                //        new Thread(() =>
                //        {
                //            try
                //            {
                //                back.Value.ScanNotify(DateTime.Now, updatedData, isLive, eServerType.Sbo);
                //            }
                //            catch (CommunicationException)
                //            {
                //                UnRegisterClient(back.Key);
                //            }
                //            catch (Exception ex)
                //            {
                //                Logger.Error("Ex", ex);
                //            }
                //        }).Start();
                //    }

                //    ScanData.ClientCallbacks = ScanData.ClientCallbacks.Shuffle();
                //}
            }
        }

        public void UnRegisterClient(string key)
        {

            if (ScanData.ClientConDic.ContainsKey(key))
            {
                IBcWinServiceCallback callback;

                ScanData.ClientConDic.TryRemove(key, out callback);
            }

            //lock (ScanData.Lock)
            //{
            //    if (ScanData.ClientCallbacks.ContainsKey(key))
            //    {
            //        ScanData.ClientCallbacks.Remove(key);
            //        ScanData.ClientConnecteds.RemoveAll(c => c.Mac == key);
            //    }
            //}
        }


        public void CompareOdd(List<MatchOddDTO> dataUpdated, ConcurrentBag<MatchDTO> targetSource, bool isLive)
        {
            List<TransactionDTO> transaction = new List<TransactionDTO>();

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
                        var ibetOdd = ibetMatchTarget.Odds.FirstOrDefault(o => o.Odd == data.Odd && o.OddType == data.OddType);

                        if (ibetOdd != null)
                        {
                            bool isValid1 = IsValidOddPair(data.HomeOdd, ibetOdd.AwayOdd);

                            if (isValid1)
                            {
                                transaction.Add(new TransactionDTO
                                {
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Home,
                                    IbetBetType = eBetType.Away,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                });
                            }

                            bool isValid2 = IsValidOddPair(data.AwayOdd, ibetOdd.HomeOdd);

                            if (isValid2)
                            {
                                transaction.Add(new TransactionDTO
                                {
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Away,
                                    IbetBetType = eBetType.Home,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                });
                            }
                        }
                    }
                }
            }

            if (transaction.Any())
            {
                //ScanData.TransactionQueue.Enqueue(transaction);
                //Process.TransactionEvent.Set();
                var now = DateTime.Now;
                lock (ScanData.Lock)
                {
                    foreach (var clientCallBack in ScanData.ClientConDic)
                    {
                        var back = clientCallBack;

                        new Thread(() =>
                        {
                            try
                            {
                                back.Value.TransactionNotify1(now, transaction);
                            }
                            catch (CommunicationException)
                            {
                                UnRegisterClient(back.Key);
                            }
                            catch (Exception ex)
                            {
                                Logger.Error("Ex", ex);
                            }
                        }).Start();
                    }

                    //ScanData.ClientCallbacks = ScanData.ClientCallbacks.Shuffle();
                }

            }
        }

        public float LastOddCheck { get; set; }
        public string LastOddIdCheck { get; set; }
        public void CompareOdd2(List<MatchOddDTO> dataUpdated, ConcurrentBag<MatchDTO> targetSource, bool isLive)
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
                        var ibetOdd = ibetMatchTarget.Odds.FirstOrDefault(o => o.Odd == data.Odd && o.OddType == data.OddType);

                        if (ibetOdd != null)
                        {
                            float oddCheck = ibetOdd.HomeOdd + ibetOdd.AwayOdd +
                                         data.HomeOdd + data.HomeOdd + data.Odd + ibetOdd.Odd;

                            if (oddCheck == LastOddCheck && LastOddIdCheck == ibetOdd.OddID)
                            {
                                continue;
                            }
                            
                            if (IsValidOddPair(data.AwayOdd, ibetOdd.HomeOdd))
                            {
                                LastOddCheck = oddCheck;
                                LastOddIdCheck = ibetOdd.OddID;
                                BroadCastTransaction(new TransactionDTO
                                {
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Away,
                                    IbetBetType = eBetType.Home,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                });
                                break;
                            }

                            if (IsValidOddPair(data.HomeOdd, ibetOdd.AwayOdd))
                            {
                                LastOddCheck = oddCheck;
                                LastOddIdCheck = ibetOdd.OddID;
                                BroadCastTransaction(new TransactionDTO
                                {
                                    SboMatchOdd = data,
                                    SboBetType = eBetType.Home,
                                    IbetBetType = eBetType.Away,
                                    IbetMatchOdd = new MatchOddDTO(ibetMatchTarget.MatchID, ibetMatchTarget.LeagueName,
                                        ibetMatchTarget.HomeTeamName, ibetMatchTarget.AwayTeamName, ibetMatchTarget.TimeType, ibetMatchTarget.Minutes,
                                        ibetOdd.OddID, ibetOdd.Odd, ibetOdd.HomeOdd, ibetOdd.AwayOdd, ibetOdd.OddType)
                                });
                                break;
                            }
                        }
                    }
                }
            }
        }

        private void BroadCastTransaction(TransactionDTO tran)
        {
            var now = DateTime.Now;
            ScanData.ClientConDic.AsParallel().ForAll(_ =>
            {
                var back = _;

                try
                {
                    back.Value.TransactionNotify(now, tran);
                }
                catch (CommunicationException)
                {
                    UnRegisterClient(back.Key);
                }
                catch (Exception ex)
                {
                    Logger.Error("Ex", ex);
                }
            });

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
            //if (Math.Abs(firstOdd) < MinOddDefBet || Math.Abs(secondOdd) < MinOddDefBet
            //    || firstOdd == 0 || secondOdd == 0)
            //{
            //    return false;
            //}
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
                    result = Math.Abs(firstOdd + secondOdd) <= 0.01;
                    //result = oddsDiff <= 0.03;
                }
                //else
                //{
                //    oddsDiff = -1;
                //}
            }
            return result;
        }


        public void DoBroadcastTransaction()
        {
            while (true)
            {
                //Process.TransactionEvent.WaitOne();

                if (ScanData.IsDispose)
                {
                    break;
                }

                //while (ScanData.TransactionQueue.Count > 0)
                //{
                //    List<TransactionDTO> transactionDtos;
                //    bool success = ScanData.TransactionQueue.TryDequeue(out transactionDtos);
                //    if (success)
                //    {
                //        var now = DateTime.Now;
                //        lock (ScanData.Lock)
                //        {
                //            foreach (var clientCallBack in ScanData.ClientCallbacks)
                //            {
                //                var back = clientCallBack;

                //                new Thread(() =>
                //                {
                //                    try
                //                    {
                //                        back.Value.TransactionNotify(now, transactionDtos);
                //                    }
                //                    catch (CommunicationException)
                //                    {
                //                        UnRegisterClient(back.Key);
                //                    }
                //                    catch (Exception ex)
                //                    {
                //                        Logger.Error("Ex", ex);
                //                    }
                //                }).Start();
                //            }

                //            ScanData.ClientCallbacks = ScanData.ClientCallbacks.Shuffle();
                //        }
                //    }
                //}
            }
        }

    }


}
