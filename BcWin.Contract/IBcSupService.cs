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
    public interface IBcSupService
    {
        [OperationContract]
        int Ping();

        [OperationContract]
        HttpMessageDTO Login(string url, string user, string pass);

    }
}
