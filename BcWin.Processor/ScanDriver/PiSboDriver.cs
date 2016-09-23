﻿using System;
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
    public class PiSboDriver : IDisposable
    {
        public int TimeSboLiveScan { get; set; }
        public string IbetUrl { get; set; }
        public string SboUrl { get; set; }
        public eServiceStatus Status;

        public static event ScanUpdateEvent OnSboScanUpdate;

        private readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboDriver));

        public List<SboEngine> SboScanEngines = new List<SboEngine>();

        //public static List<IbetSubEngine> IbetScanEngines = new List<IbetSubEngine>();

        public void Start(int timeSboLiveScan)
        {
            if (DataContainer.SboScanAccounts == null || !DataContainer.SboScanAccounts.Any())
            {
                return;
            }

            TimeSboLiveScan = timeSboLiveScan;
            IsReConnect = false;
            Status = eServiceStatus.Started;
            StartSbo();
            //StartIbet(ibetUrl);
        }

        public void Stop()
        {
            foreach (var sboEngine in SboScanEngines)
            {
                sboEngine.UpdateLiveDataChange -= sbobetUpdateChange_Event;
                sboEngine.OnExceptionEvent -= OnExceptionEvent;
                sboEngine.LogOff();
            }
            SboScanEngines = new List<SboEngine>();
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

                sbo1.TryLoginScan(url, sboAcc.Username, sboAcc.Password);
                if (sbo1.AccountStatus == eAccountStatus.Online)
                {
                    sbo1.UpdateLiveDataChange += sbobetUpdateChange_Event;
                    sbo1.OnExceptionEvent += OnExceptionEvent;
                    sbo1.StartScanDriver(eScanType.Live);
                    SboScanEngines.Add(sbo1);
                }
            }
            var timeSboLiveSleep = TimeSboLiveScan / sboCount;
            int oType = 0;
            foreach (var sboScanEngine in SboScanEngines)
            {
                sboScanEngine.TypeGetScan = oType;
                sboScanEngine.StartUpdateLiveDriver();
                Thread.Sleep(timeSboLiveSleep);

                oType = oType == 0 ? 1 : 0;
            }

            Logger.Info("START SBO SCAN THANH CONG>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>");
        }

        void OnExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj)
        {
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
            s.LogOff();

            if (s.ReLogin())
            {
                s.UpdateLiveDataChange += sbobetUpdateChange_Event;
                s.OnExceptionEvent += OnExceptionEvent;
                s.StartScanDriver(eScanType.Live);
                s.StartUpdateLiveDriver();
            }
        }

        private bool IsReConnect;

        private void sbobetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive, int type = 0)
        {
            //Logger.Debug("IbetSboProcessor => sbo change event");
            if (updatedData != null && updatedData.Count > 0 && OnSboScanUpdate != null)
            {
                updatedData.Shuffle();
                var invoke = OnSboScanUpdate.GetInvocationList();
                var now = DateTime.Now;
                foreach (ScanUpdateEvent action in invoke)
                {
                    action.BeginInvoke(updatedData, isLive, now, null, null);
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Stop();
        }
    }


}
