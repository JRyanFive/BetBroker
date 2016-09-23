using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.Contract;
using BcWin.Processor;

namespace BcWin.Service
{
    public class BcService : IBcService
    {
        public IList<string> GetSboServers()
        {
            //ar aa= BcWin.Server
            return DataContainer.SbobetServers;
        }

        public IList<string> GetIbetServers()
        {
            return DataContainer.IbetServers;
        }
    }
}
