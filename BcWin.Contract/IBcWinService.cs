using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Contract
{
    [ServiceContract(CallbackContract = typeof(IBcWinServiceCallback))]
    public interface IBcWinService
    {
        [OperationContract]
        int PingCheck();

        [OperationContract]
        int Ping(string mac);
        
        [OperationContract]
        void RegisterForClient(string ip, string mac, string hostName);

        [OperationContract]
        ScanServerInfoDTO GetIbetSboScanInfo(string ip, string mac, string hostName);
    }
}
