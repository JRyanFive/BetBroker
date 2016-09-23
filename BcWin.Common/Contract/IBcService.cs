using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.FaultDTO;

namespace BcWin.Common.Contract
{
    [ServiceContract]
    public interface IBcService
    {
        [OperationContract]        
        int Ping();

        /// <summary>
        /// Login Client side
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns>Guid Id token</returns>
        [OperationContract]
        Guid LogIn(string userName, string password);

        [OperationContract]
        IList<string> GetSboServers();

        [OperationContract]
        IList<string> GetIbetServers();

        /// <summary>
        /// Check status server.
        /// </summary>
        /// <param name="url"></param>
        /// <returns>If has return, init ok.</returns>
        [OperationContract]
        [FaultContract(typeof(StartServerFault))]
        Guid? InitBetServer(string url);
   
        [OperationContract]
        //[FaultContract(typeof(StartServerFault))]
        Guid InitServer(Guid guidToken, AccountDTO firstAccountDto, AccountDTO secondAccountDto,
            ProcessorConfigInfoDTO processorConfigDto);

        /// <summary>
        /// Start Scan and Bet Service
        /// </summary>
        /// <returns>Guid token for earch matchs started</returns>
        [OperationContract]
        [FaultContract(typeof(StartServerFault))]
        void StartServer(Guid processorToken);

        /// <summary>
        /// Get Server Config info.
        /// </summary>
        /// <param name="guidToken">Guid token for earch matchs started</param>
        /// <returns></returns>
        [OperationContract]
        ProcessorConfigInfoDTO GetServerConfigInfo(Guid tokenServer);

        /// <summary>
        /// Start Scan and Bet Service 
        /// </summary>
        /// <param name="tokenServer">Token returned after Start Server</param>
        [OperationContract]
        void StopServer(Guid tokenServer);

        [OperationContract]
        string GetServerReport(Guid processGuid, AccountDTO accountDto);

        [OperationContract]
        string GetServiceReport(Guid tokenServer);
    }
}
