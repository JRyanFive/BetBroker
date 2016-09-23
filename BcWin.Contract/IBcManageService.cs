using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Contract
{
    [ServiceContract]
    public interface IBcManageService
    {
        [OperationContract]
        int Ping();

        [OperationContract]
        int PingBet(string username, string mac, int key);

        [OperationContract]
        int PingScan(string ipAddress, int sboOnline, int ibetOnline);

        [OperationContract]
        bool Login(string username, string password, eUserType userType, string ip, string mac, string hostname);

        [OperationContract]
        int RouteType(string username);

        [OperationContract]
        int GetAccType(string username);

        [OperationContract]
        SetupScanConfigDTO ScanConfig(string username, int type);

        [OperationContract]
        ScanAccountDTO ScanAccount(string username);

        [OperationContract]
        SetupBetConfigDTO BetSetupConfig(string username, int type);

        [OperationContract]
        ScanInfoDTO ClientScanAccount(string username);

        [OperationContract]
        ScanInfoDTO ClientScanAccountBuyServerType(string username, eServerType serverType);

        [OperationContract]
        string GetKey();
    }
}
