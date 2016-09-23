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
    public interface IBcScanService
    {
        [OperationContract]
        int Ping();

        [OperationContract(IsOneWay = true)]
        void TransactionNotify(DateTime now, TransactionDTO transaction, string oddIdCheck, float oddCheck);
    }
}
