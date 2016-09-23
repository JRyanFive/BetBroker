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
    public interface IBcWinServiceCallback
    {
        [OperationContract]
        void ScanNotify(DateTime now, List<MatchOddDTO> matchOdds, bool isLive, eServerType serverType);

        [OperationContract]
        void TransactionNotify1(DateTime now, List<TransactionDTO> transactions);

        [OperationContract(IsOneWay = true)]
        void TransactionNotify(DateTime now, TransactionDTO transaction, string oddIdCheck, float oddCheck);

        [OperationContract]
        int PingClient();
    }
}
