using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BCWin.Metadata
{
    public class IbetConfig
    {
        public readonly static int[] ODDTYPE_VALUES_UPDATED = new [] { 1, 3, 7, 8 };
        
        public const string URL_MAIN = "/main.aspx";

        public const string URL_NEW_MARKET = "/UnderOver.aspx?Market=t&DispVer=new";
        public const string URL_LEFT_ALL_IN_ONE = "/LeftAllInOne.aspx";
        //public const string URL_NEW_MARKET_REFER = "main.aspx";
        
        public const string URL_CHECK_LOGIN = "/login_checkin.aspx";
        public const string URL_CHECK_LOGIN_REFER = "/topmenu.aspx";


        //public const string URL_UPDATE_ODD_REFER = "UnderOver.aspx?Market=t&DispVer=new";
        public const string URL_INIT_LODDS_MARKET = "/UnderOver_data.aspx?Market=l&Sport=1&DT=&RT=W&CT=&Game=0&OrderBy=0&OddsType=4&MainLeague=0&key=lodds&";
        public const string URL_INIT_DODDS_MARKET = "/UnderOver_data.aspx?Market=t&Sport=1&DT=&RT=W&CT=&Game=0&OrderBy=0&OddsType=4&MainLeague=0&DispRang=0&key=dodds&";

        public const string URL_UPDATE_LODDS_MARKET = "/UnderOver_data.aspx?Market=l&Sport=1&DT=&RT=U&Game=0&OrderBy=0&OddsType=4&MainLeague=0&";
        public const string URL_UPDATE_DODDS_MARKET = "/UnderOver_data.aspx?Market=t&Sport=1&DT=&RT=U&Game=0&OrderBy=0&OddsType=4&MainLeague=0&DispRang=0&";

        public const string URL_BET_PREPARE = "/BetProcess_Data.aspx?";
        public const string URL_BET_CONFIRM = "/underover/confirm_bet_data.aspx";

        public const string URL_GET_ACCOUNT_INFO = "/leftAllInOneAccount_data.aspx";
        public const string URL_GET_BETLIST = "/Betlist.aspx?from=full";
        public const string URL_GET_STATEMENT = "/DBetlist.aspx?";
        public const string URL_GET_STATEMENT_REFER = "/BettingStatement.aspx?";
    }
}
