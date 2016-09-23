using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Core.EventDelegate;
using BcWin.Processor.Helper;
using log4net;

namespace BcWin.Processor
{
    public abstract class CoreEngine : RestClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(CoreEngine));

        //public event LogBetEvent OnLogBetEvent;
        public event ExceptionEvent OnExceptionEvent;
        public event LogOffEvent OnLogOffEvent;

        public eServiceStatus Status { get; set; }

        public AccountDTO Account { get; set; }
        public string UrlHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LoginUrl { get; set; }
        public ManualResetEvent LoginEvent { get; set; }
        public AutoResetEvent ReLoginEvent { get; set; }
        public WebBrowser WebBrowserLogin { get; set; }
        public bool IsReConnect { get; set; }
        public eScanType ScanType { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public eAccountStatus AccountStatus { get; set; }

        public eServerType ServerType { get; set; }

        public string SboLoginName { get; set; }
        public string Host { get; set; }
        public float AvailabeCredit { get; set; }
        public float CashBalance { get; set; }
        public string EngineName { get; set; }
        public EngineLogger EngineLogger { get; set; }

        public int ExceptionCount { get; set; }

        private TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");

        public List<MatchOddDTO> LiveMatchOddDatas = new List<MatchOddDTO>();
        public List<MatchOddDTO> NoneLiveMatchOddDatas = new List<MatchOddDTO>();

        public Object LockLive = new Object();
        public Object LockNonLive = new Object();
        public Object LockException = new Object();

        //internal System.Threading.Timer objScanTimer;

        protected CoreEngine()
        {
            WebBrowserLogin = new WebBrowser();
            LoginEvent = new ManualResetEvent(false);
            //ReLoginEvent = new ManualResetEvent(false);
            Status = eServiceStatus.Unknown;
            ExceptionCount = 0;
            IsReConnect = false;
            //SetCookieEvent = new AutoResetEvent(false);
        }

        public async void SetIbetCookieContainer(WebBrowser webBrowser, int timeToWait)
        {
            //WaitSomeTime(10000);
            await Task.Delay(timeToWait);

            try
            {
                //SetCookieEvent.WaitOne();
                if (!webBrowser.Url.Host.Contains("www"))
                {
                    AccountStatus = eAccountStatus.Online;
                    UrlHost = webBrowser.Url.Scheme + Uri.SchemeDelimiter +
                              webBrowser.Url.Host;
                    Uri hostUri = new Uri(UrlHost);
                    Host = webBrowser.Url.Host;
                    CookieContainer = GetUriCookieContainer(hostUri);
                    //System.Net.CookieContainer cookie = GetUriCookieContainer(hostUri);
                    //CookieCollection cookieCollection = cookie.GetCookies(hostUri);
                    //DataContainer.CookieContainer.Add(cookieCollection);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                LoginEvent.Set();
                if (ReLoginEvent != null)
                {
                    ReLoginEvent.Set();
                }
                //setCookieEvent.Set();
            }
        }
        public async void SetSboCookieContainer(WebBrowser webBrowser, int timeToWait, string userName)
        {
            //WaitSomeTime(10000);
            await Task.Delay(timeToWait);

            try
            {
                if (!webBrowser.Url.Host.Contains("www"))
                {
                    if (webBrowser.Url.Query.IndexOf("loginname=") >= 0)
                    {
                        AccountStatus = eAccountStatus.Online;
                        UrlHost = webBrowser.Url.Scheme + Uri.SchemeDelimiter +
                                  webBrowser.Url.Host;
                        SboLoginName = GetLoginName(webBrowser.Url.Query.Split('&'));

                        Host = webBrowser.Url.Host;
                        UserName = userName;
                        Uri hostUri = new Uri(UrlHost);
                        CookieContainer = GetUriCookieContainer(hostUri);
                        //System.Net.CookieContainer cookie = GetUriCookieContainer(hostUri);
                        //CookieCollection cookieCollection = cookie.GetCookies(hostUri);
                        //DataContainer.CookieContainer.Add(cookieCollection);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            finally
            {
                LoginEvent.Set();
            }
        }

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetGetCookieEx(string url, string cookieName,
            StringBuilder cookieData, ref int size,
            Int32 dwFlags, IntPtr lpReserved);
        private const Int32 InternetCookieHttponly = 0x2000;
        private static CookieContainer GetUriCookieContainer(Uri uri)
        {
            CookieContainer cookies = null;
            // Determine the size of the cookie
            int datasize = 8192 * 16;
            StringBuilder cookieData = new StringBuilder(datasize);
            if (!InternetGetCookieEx(uri.ToString(), null, cookieData, ref datasize, InternetCookieHttponly, IntPtr.Zero))
            {
                if (datasize < 0)
                    return null;
                // Allocate stringbuilder large enough to hold the cookie
                cookieData = new StringBuilder(datasize);
                if (!InternetGetCookieEx(
                    uri.ToString(),
                   null, cookieData,
                    ref datasize,
                    InternetCookieHttponly,
                    IntPtr.Zero))
                    return null;
            }
            if (cookieData.Length > 0)
            {
                cookies = new CookieContainer();
                cookies.SetCookies(uri, cookieData.ToString().Replace(';', ','));
            }
            return cookies;
        }

        public CookieContainer GetCookieContainer(WebBrowser webBrowser)
        {
            var urlHost = webBrowser.Url.Scheme + Uri.SchemeDelimiter +
                     webBrowser.Url.Host;
            Uri hostUri = new Uri(urlHost);
            System.Net.CookieContainer cookie = GetUriCookieContainer(hostUri);
            //CookieCollection cookieCollection = cookie.GetCookies(hostUri);
            //DataContainer.CookieContainer.Add(cookieCollection);
            return cookie;
        }

        private string GetLoginName(string[] url)
        {

            return url[url.Length - 2].Replace('?', ' ').Split('=')[1];
        }

        public DateTime GetDateTimeZone()
        {
            //"China Standard Time"            
            DateTime dateTime = DateTime.UtcNow;
            //Get date and time in US Mountain Standard Time 
            dateTime = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            return dateTime;
        }

        public void FireLogBet(MatchOddDTO match, eBetType betType, int stake, bool betStatus)
        {
            var msg = new LogBetMessage()
            {
                Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                HomeTeamName = match.HomeTeamName,
                AwayTeamName = match.AwayTeamName,
                OddType = match.OddType,
                ServerType = match.ServerType,
                OddValues = match.HomeOdd + "|" + match.AwayOdd,
                Odd = match.Odd,
                BetStake = stake,
                BetType = betType,
                Status = betStatus,
                EngineName = this.EngineName
            };

            Task.Run(() =>
            {
                DataContainer.LogBetQueue.Enqueue(msg);
                DataContainer.LogBetResetEvent.Set();
            });
        }

        public void RemoveCookie()
        {
            if (!string.IsNullOrEmpty(UrlHost))
            {
                if (CookieContainer != null)
                {
                    var cookies = CookieContainer.GetCookies(new Uri(UrlHost));
                    foreach (Cookie co in cookies)
                    {
                        co.Expires = DateTime.Now.AddDays(-1);
                    }

                    CookieContainer = null;
                }
                //var cookies = DataContainer.CookieContainer.GetCookies(new Uri(UrlHost));
                //foreach (Cookie co in cookies)
                //{
                //    co.Expires = DateTime.Now.AddDays(-1);
                //}
            }
        }

        public void UpdateException(eExceptionType exceptionType = 0, string msg = "")
        {
            lock (LockException)
            {
                if (exceptionType == 0)
                {
                    ExceptionCount = 0;
                    return;
                }

                switch (exceptionType)
                {
                    case eExceptionType.LoginFail:
                        FireExceptionEvent(new ExceptionMessage(exceptionType));
                        break;
                    default:
                        if (ExceptionCount >= 4)
                        {
                            FireExceptionEvent(new ExceptionMessage(exceptionType));
                        }
                        else
                        {
                            ExceptionCount++;
                        }
                        break;
                }
            }
        }

        private void FireExceptionEvent(ExceptionMessage msg)
        {
            if (OnExceptionEvent != null)
            {
                OnExceptionEvent(msg, ServerType);
            }
        }

        public void FireLogOffEvent()
        {
            if (OnLogOffEvent != null)
            {
                OnLogOffEvent(UserName, ServerType);
            }
        }
    }
}
