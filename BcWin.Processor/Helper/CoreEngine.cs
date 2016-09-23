using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Core.EventDelegate;

namespace BcWin.Processor.Helper
{
    public abstract class CoreEngine
    {
        public int TabCode { get; set; }

        private TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("China Standard Time");
        public event ExceptionEvent OnExceptionEvent;
        public event LogOffEvent OnLogOffEvent;
        public string UrlHost { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string LoginUrl { get; set; }
        public string Host { get; set; }
        public float AvailabeCredit { get; set; }
        public float CashBalance { get; set; }
        public string EngineName { get; set; }
        //public DateTime DataTimePrepare = DateTime.Now;
        public eScanType ScanType { get; set; }
        public CookieContainer CookieContainer { get; set; }
        public eAccountStatus AccountStatus { get; set; }
        public eServiceStatus Status { get; set; }
        public eServerType ServerType { get; set; }
        public int ExceptionCount { get; set; }
        //public EngineLogger EngineLogger { get; set; }
        public List<MatchOddDTO> LiveMatchOddDatas = new List<MatchOddDTO>();
        public List<MatchOddDTO> NoneLiveMatchOddDatas = new List<MatchOddDTO>();
        //public List<MatchBag> BetQueue = new List<MatchBag>();
        public Object LockLive = new Object();
        public Object LockNonLive = new Object();
        public Object LockException = new Object();

        protected CoreEngine()
        {
            //WebBrowserLogin = new WebBrowser();
            //LoginEvent = new ManualResetEvent(false);
            //ReLoginEvent = new ManualResetEvent(false);
            Status = eServiceStatus.Unknown;
            //ExceptionCount = 0;
            //IsReConnect = false;
            //SetCookieEvent = new AutoResetEvent(false);
        }

        public CookieContainer BindCookieContainer(Uri uri, string setCookieData, CookieContainer cookieContainer = null)
        {
            if (cookieContainer == null)
            {
                cookieContainer = new CookieContainer();
            }

            if (!string.IsNullOrEmpty(setCookieData))
            {
                setCookieData =
                    setCookieData.Replace("HttpOnly,", "")
                        .Replace("HttpOnly", "")
                        .Replace(" ", "")
                        .Replace("Path=/,", "").Replace("path=/,", ""); //path=/,

                CookieCollection cookieCollection = new CookieCollection();
                var setCookies = setCookieData.Split(';');
                foreach (var sc in setCookies)
                {
                    var scValues = sc.Split('=');

                    if (scValues.Count() == 2)
                    {
                        cookieCollection.Add(new Cookie(scValues[0], HttpUtility.UrlEncode(scValues[1])));
                    }
                    else if (scValues.Count() == 1)
                    {
                        if (!string.IsNullOrEmpty(scValues[0]))
                        {
                            cookieCollection.Add(new Cookie(scValues[0], ""));
                        }
                    }
                }
                cookieContainer.Add(uri, cookieCollection);
            }

            return cookieContainer;
        }
        
        public DateTime GetDateTimeZone()
        {
            //"China Standard Time"            
            //DateTime dateTime = DateTime.UtcNow;
            //Get date and time in US Mountain Standard Time 
            return TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            //return dateTime;
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
            }
        }

        public void UpdateException(object obj, eExceptionType exceptionType = 0)
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
                        FireExceptionEvent(new ExceptionMessage(exceptionType), obj);
                        break;
                    default:
                        if (ExceptionCount >= 8 && ServerType == eServerType.Ibet)
                        {
                            FireExceptionEvent(new ExceptionMessage(exceptionType), obj);
                        }
                        else if (ExceptionCount >= 30 && ServerType == eServerType.Sbo)
                        {
                            FireExceptionEvent(new ExceptionMessage(exceptionType), obj);
                        }
                        else
                        {
                            ExceptionCount++;
                        }
                        break;
                }
            }
        }

        public void FireLogBet(MatchOddDTO match, eBetType betType, int stake, eBetStatusType betStatus, eServerScan serverScan)
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
                EngineName = this.EngineName,
                TabCode = TabCode,
                ServerScan = serverScan
            };

            Task.Run(() =>
            {
                DataContainer.LogBetQueue.Enqueue(msg);
                DataContainer.LogBetResetEvent.Set();
            });
        }
        private void FireExceptionEvent(ExceptionMessage msg, object obj)
        {
            if (OnExceptionEvent != null)
            {
                OnExceptionEvent(msg, ServerType, obj);
            }
        }

        public void FireLogOffEvent()
        {
            if (OnLogOffEvent != null)
            {
                OnLogOffEvent(UserName, ServerType);
            }
        }

        protected string CleanTeamName(string teamName)
        {
            if (teamName.Contains("(w)") || teamName.Contains("(W)"))
            {
                var name =
                    teamName.Replace("(w)", "").Replace("(n)", "").Replace("(W)", "").Replace("(N)", "")
                    .Replace("FC", "").Trim();
                return name + " W";
            }
            return teamName.Replace("(n)", "").Replace("(N)", "").Replace("FC", "").Trim();
        }
    }
}
