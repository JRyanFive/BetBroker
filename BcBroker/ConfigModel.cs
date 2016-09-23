using BcWin.Common.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcBroker
{
    public class ConfigModel
    {
        public string buyAmount { get; set; }
        //public bool IsBuyTotal { get; set; }
        public float MinHDPOdd { get; set; }
        public int BuyAfterMin { get; set; }
        public float ProfitMin { get; set; }
        public float OddDef { get; set; }
        public int TotalScoreMaxBuy { get; set; }
        //public bool FakeRandomIpIbet { get; set; }
        //public bool FakeRandomIpSbo { get; set; }
        public bool ProxyLoginIbet { get; set; }
        public string ProxyLoginIbetValue { get; set; }

        public List<Account> BuyAccounts { get; set; }
        public List<Account> SellAccounts { get; set; }

        public bool SelectAll { get; set; }
        public List<LeaguesSetting> LeaguesSelected { get; set; }

        public int TotalAccBuy { get; set; }
    }

    public class Account 
    {
        public string username { get; set; }
        public string password { get; set; }
        public string rate { get; set; }
        public string domain { get; set; }
        public string serverType { get; set; }
        public bool IsChecked { get; set; }
        public string IP { get; set; }
    }
}
