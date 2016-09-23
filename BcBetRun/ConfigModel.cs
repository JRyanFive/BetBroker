using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcBetRun
{
    public class ConfigModel
    {
        public string userScan { get; set; }
        public string userPassScan { get; set; }
        public int cboMarketSelectedIndex { get; set; }
        public int numMaxStake { get; set; }
        public string txtBuyEx { get; set; }
        public string txtSellEx { get; set; }

        public List<Account> ListBuy { get; set; }
        public List<Account> ListSell { get; set; }
    }

    public class Account 
    {
        public string username { get; set; }
        public string password { get; set; }
  //      public bool isChecked{ get; set; }
    }
}
