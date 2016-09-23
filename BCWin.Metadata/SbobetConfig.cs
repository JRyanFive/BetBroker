using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCWin.Metadata
{
    public class SbobetConfig
    {
   
        public const string URL_TODAY_All_NON_LIVE_DATA = "web-root/restricted/odds-display/today-data.aspx?od-param=1,1,1,1,1,2,1,2,0&fi=1&v=0&dl=0";
        public const string URL_TODAY_UPDATE_NON_LIVE_DATA = "web-root/restricted/odds-display/today-data.aspx?od-param=1,1,1,1,1,2,1,2,0&fi=1"; // + none live version

        public const string URL_LIVE_ALL_DATA = "web-root/restricted/odds-display/live-data.aspx?od-param=1,1,3,1,1,2,1,2,0&v=0";
        public const string URL_LIVE_UPDATE_DATA = "web-root/restricted/odds-display/live-data.aspx?od-param=1,1,3,1,1,2,1,2,0"; // + live version

        //public const string URL_TEST = "web-root/restricted/odds-display/today-data.aspx?od-param=1,1,1,1,1,2,2,2,1&fi=0&v=0";

        //public const string URL_LOGIN_NAME_REFER = "http://{0}/web-root/restricted/default.aspx?loginname={1}&redirect=true"; //0 : domain, 1: login name
      
        public const string URL_BET_LIST = "web-root/restricted/betlist/running-bet-list.aspx";
        public const string URL_BALANCE_CREDIT = "web-root/restricted/top-module/action-data.aspx?action=bet-credit";

        public const string URL_MINI_BET_LIST = "web-root/restricted/betlist/bet-list-mini-data.aspx";
        public const string URL_BET_LIST_ALL = "web-root/restricted/betlist/bet-list-all.aspx?d={0}&option=1&p=sb"; // {0} date = yyyy-MM-dd
    }
}
