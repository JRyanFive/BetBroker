namespace BCWin.Metadata
{
    public class SystemConfig
    {
        public const int TIME_GET_COOKIE_AFTER_LOGIN = 10000;

        ////IBET CONFIG
        public static int TIME_GET_UPDATE_LIVE_IBET;//= 8000;//14000;
        public static int TIME_GET_UPDATE_NON_LIVE_IBET; //= 14000;//20000;
        public const int TIME_CHECK_LOGIN_IBET = 60000;

        //SBOBET CONFIG
        public static int TIME_GET_UPDATE_LIVE_SBOBET = 7000;// = 4000;
        public static int TIME_GET_UPDATE_NON_LIVE_SBOBET;// = 9000;
        public const int TIME_CHECK_LOGIN_SBOBET = 60000 * 5;

        //public static string SBO_LINK_1 { get; set; }
        //public static string IBET_LINK_1 { get; set; }
    }
}
