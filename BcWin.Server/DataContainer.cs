using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common;

namespace BcWin.Server
{
    public class DataContainer
    {
        public static bool IsLoginSbo { get; set; }
        public static bool IsLoginIBet { get; set; }
        public static IList<Account> IbetAccounts { get; set; }
        public static IList<Account> SboAccounts { get; set; }
    }
}
