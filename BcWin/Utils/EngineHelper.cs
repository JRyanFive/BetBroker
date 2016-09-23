using BcWin.Processor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Core;

namespace BcWin.Utils
{
   public class EngineHelper
    {
       public static IList<string> GetSboServers()
       {
           return DataContainer.SboServers;
       }

       public static IList<string> GetIbetServers()
       {
           return DataContainer.IbetServers;
       }
    }
}
