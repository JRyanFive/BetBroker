using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Utils;
using BcWin.Core.Helper;
using BCWin.Engine.Sbo;
using BCWin.Metadata;
using log4net;

namespace BcWin.Processor.ScanDriver
{
    public class IbetSboDriver
    {
        public event FakeRequestEvent OnFakeRequest;

        public string IbetUrl { get; set; }
        public string SboUrl { get; set; }
        public eServiceStatus Status;
        //public static event ValidScanEvent OnValidScan;
        public event ScanUpdateEvent OnSboScanUpdate;


        private readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboDriver));

        public List<SboEngine> SboScanEngines = new List<SboEngine>();

        //public static List<IbetSubEngine> IbetScanEngines = new List<IbetSubEngine>();

        public eScanType ScanType;
        public void Start(eScanType scanType)
        {
            IsReConnect = false;
            Status = eServiceStatus.Started;
            ScanType = scanType;
            StartSbo();
            //StartIbet(ibetUrl);
        }

        public void StartSbo()
        {
            int sboCount = DataContainer.SboScanAccounts.Count;
            foreach (var sboAcc in DataContainer.SboScanAccounts)
            {
                Random r = new Random();
                string url = DataContainer.SboScanServers[r.Next(DataContainer.SboScanServers.Count())];

                SboEngine sbo1 = new SboEngine();
                //sbo1.IpFake = IpHelper.GetRandomIp();
                sbo1.OnFakeRequest += OnFakeRequest;

                sbo1.TryLoginScan(url, sboAcc.Username, sboAcc.Password);
                if (sbo1.AccountStatus == eAccountStatus.Online)
                {
                    sbo1.UpdateLiveDataChange += sbobetUpdateChange_Event;
                    sbo1.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
                    sbo1.OnExceptionEvent += OnExceptionEvent;
                    sbo1.StartScanDriver(ScanType);
                    SboScanEngines.Add(sbo1);
                }
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
            //var timeSboLiveSleep = SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET / sboCount;
            //int oType = 0;
            //foreach (var sboScanEngine in SboScanEngines)
            //{
            //    sboScanEngine.TypeGetScan = oType;
            //    sboScanEngine.StartUpdateLiveDriver();
            //    //Thread.Sleep(1000);
            //    Thread.Sleep(timeSboLiveSleep);
            //    oType = oType == 0 ? 1 : 0;
            //}

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
            //if (serverType == eServerType.Ibet)
            //{
            //    Thread thread = new Thread(ReStartFromIbet);
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.Start();
            //}
            //else
            //{
            //Thread thread = new Thread(ReStartFromSbo);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            //}

            Thread thread = new Thread(() =>
            {
                ReStartSbo(obj);
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private void ReStartSbo(object obj)
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

        //private void ReStartFromSbo()
        //{
        //    if (!IsReConnect)
        //    {
        //        IsReConnect = true;
        //        Stop();

        //        Logger.Info("TAM DUNG HE THONG SCAN<<<<<<<<<<<<<<<<<<");

        //        StartSbo(SboUrl);
        //        IsReConnect = false;
        //    }

        //}

        private bool IsReConnect;

        public void Stop()
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

        Random R = new Random();
        private void sbobetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive, int type = 0)
        {
            //Logger.Debug("IbetSboProcessor => sbo change event");
            if (updatedData != null && updatedData.Count > 0 && OnSboScanUpdate != null)
            {
                var invoke = OnSboScanUpdate.GetInvocationList();
                var now = DateTime.Now;

                //ScanUpdateEvent tabInvoke = (ScanUpdateEvent)invoke[R.Next(invoke.Count())];
                //tabInvoke.BeginInvoke(updatedData, isLive, now, null, null);

                foreach (ScanUpdateEvent action in invoke)
                {
                    action.BeginInvoke(updatedData, isLive, now, null, null);
                }
            }
        }

    }


}
