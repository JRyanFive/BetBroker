using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Processor;
using BcWinScan.Objects;

namespace BcWinScan
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, 
        ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class BcWinService : IBcWinService
    {
        public int Ping()
        {
            return 1;
        }

        public void RegisterForClient(string ip, string mac, string hostName)
        {
            OperationContext ctxt = OperationContext.Current;
            IBcWinServiceCallback callBack = ctxt.GetCallbackChannel<IBcWinServiceCallback>();

            ScanData.ClientConDic[mac] = callBack;
            
            
            //lock (ScanData.Lock)
            //{
            //    ScanData.ClientCallbacks[mac] = callBack;
            //    ScanData.ClientConnecteds.Add(new ClientConnected { Ip = ip, Mac = mac, Hostname = hostName });
            //}
        }

        public ScanServerInfoDTO GetIbetSboScanInfo(string ip, string mac, string hostName)
        {
            if (!ScanData.ClientConDic.ContainsKey(mac))
            {
                RegisterForClient(ip, mac, hostName);
            }

            int sboLive = 0, sboToday = 0, ibetLive = 0, ibetToday = 0;
            int sboOn = Process.Driver.SboScanEngines.Count(s => s.AccountStatus == eAccountStatus.Online);
            var sboScan = Process.Driver.SboScanEngines.FirstOrDefault(x => x.AccountStatus == eAccountStatus.Online);
            if (sboScan != null)
            {
                lock (sboScan.LockLive)
                {
                    sboLive = sboScan.LiveMatchOddDatas.Count();
                }

                lock (sboScan.LockNonLive)
                {
                    sboToday = sboScan.NoneLiveMatchOddDatas.Count();
                }
            }

            int ibetOn = Process.Driver.IbetScanEngines.Count(s => s.AccountStatus == eAccountStatus.Online);
            ibetLive = DataContainer.LiveMatchOddBag.Count(l => !l.IsDeleted);


            //ibetToday = DataContainer.NonLiveMatchOddBag.Count(l => !l.IsDeleted);

            //var ibetScan = Process.Driver.IbetScanEngines.FirstOrDefault(x => x.AccountStatus == eAccountStatus.Online);
            //if (ibetScan != null)
            //{
            //    lock (ibetScan.LockLive)
            //    {
            //        ibetLive = ibetScan.LiveMatchOddDatas.Count();
            //    }

            //    lock (ibetScan.LockNonLive)
            //    {
            //        ibetToday = ibetScan.NoneLiveMatchOddDatas.Count();
            //    }
            //}

            return new ScanServerInfoDTO()
            {
                SboScanOnline = sboOn,
                SboLive = sboLive,
                SboToday = sboToday,
                IbetScanOnline = ibetOn,
                IbetLive = ibetLive,
                IbetToday = ibetToday
            };
        }
    }
}
