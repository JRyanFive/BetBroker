using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using BCWin.Metadata;
using BcWin.Common;
using BcWin.Core.Helper;
using BcWin.Processor.Interface;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using BcWin.Common.DTO;
using TypescriptB;
using BcWin.Common.Objects;
using BcWin.Core;

namespace BcWin.Processor
{
    public partial class SboEngine
    {
        public bool Login(string url, string username, string password)
        {
            return LoginNomal(url, username, password);
        }

        private bool LoginNomal(string url, string username, string password)
        {
            Uri uriLogin = new Uri(url);
            string host = uriLogin.Host;
            LoginUrl = url;
            UserName = username;
            Password = password;

            try
            {
                var getMain = SendSbo302(url, "GET", userAgent, null, null, url, host, accept, null);
                //Logger.Info("MAIN RESULT : " + getMain.Result);
                var tokenIndex = getMain.Result.IndexOf("'ms','ps'");
                var tokenIndexEnd = getMain.Result.IndexOf("]));", tokenIndex);
                string token = getMain.Result.Substring(tokenIndex + 12, tokenIndexEnd - (tokenIndex + 12));
                //string token = "1,0,1,0,0,0,0,'20150825',0,0,0,4";
                // cookieCollection.Add(new Cookie("secure", ""));
                Uri uriProcessSignIn = new Uri(url + "web/public/process-sign-in.aspx");
                //var cookieProcessSignIn = BindCookieContainer(uriProcessSignIn, getMain.SetCookie);
                string loginParam = string.Format(@"id={0}&password={1}&lang=en&tk={2}&5=1&type=form&tzDiff=1", username,
                   HttpUtility.UrlEncode(password), token);
                byte[] byteLoginParam = Encoding.UTF8.GetBytes(loginParam);
                var processLoginMsg = SendSbo302(url + "web/public/process-sign-in.aspx", "POST", userAgent, new CookieContainer(), 
                    byteLoginParam, url, host, accept, "application/x-www-form-urlencoded");

                if (processLoginMsg.Location.Contains("error"))
                {
                    throw new Exception(string.Format("Incorrect username: [{0}] and password: [{1}]",
                      username, password));
                }

                Uri uriWellcome = new Uri(processLoginMsg.Location);
                var hostWellcome = uriWellcome.Host;
                Uri uriMain = new Uri("http://" + hostWellcome);
                //var cookieWellcome = BindCookieContainer(uriWellcome, processLoginMsg.SetCookie);
                var wellcomeMsg = SendSbo302(processLoginMsg.Location, "GET", userAgent, null,
                    null, url + "web/public/process-sign-in.aspx", hostWellcome, accept, "text/html");

                //var cookieWellcometest = cookieWellcome.GetCookies(uriWellcome);
                CookieContainer = BindCookieContainer(uriMain, wellcomeMsg.SetCookie);
                //var cookieWellcometest1 = CookieContainer.GetCookies(uriMain);
                var processWellcomeMsg = SendSbo302("http://" + hostWellcome + wellcomeMsg.Location, "GET", userAgent,
                    CookieContainer, null, processLoginMsg.Location, hostWellcome, accept, "text/html");


                CookieContainer = BindCookieContainer(uriMain, processWellcomeMsg.SetCookie, CookieContainer);
                //var cookieWellcometest2 = cookieWellcome.GetCookies(uriWellcome);
                var finalLoginMsg = SendSbo302("http://" + hostWellcome + processWellcomeMsg.Location, "GET", userAgent,
                    CookieContainer, null, url + "web/public/process-sign-in.aspx", hostWellcome, accept, "text/html");


                if (finalLoginMsg.Location != null && finalLoginMsg.Location.Contains("ogout"))
                {
                    throw new Exception();
                }

                var sboFinalReponseUrls = finalLoginMsg.ReponseUri.Split('&');

                SboLoginName = sboFinalReponseUrls[sboFinalReponseUrls.Length - 2].Replace('?', ' ').Split('=')[1];

                UrlHost = finalLoginMsg.ReponseUri;
                Host = hostWellcome;
                //CookieContainer = cookieFinalLogin;
                AccountStatus = eAccountStatus.Online;
                UpdateException(this);

                objCheckLoginTimer = new System.Threading.Timer(WaitCheckLoginCallBack, null,
                SystemConfig.TIME_CHECK_LOGIN_SBOBET, SystemConfig.TIME_CHECK_LOGIN_SBOBET);

                Logger.InfoFormat("LOGIN [{0}] successful!!!", UserName);
                return true;
                //var todayRespone = Get(SbobetConfig.URL_TODAY_All_NON_LIVE_DATA);
                //var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);
            }
            catch (Exception ex)
            {
                Logger.Error("login ex url: " + url, ex);

                UrlHost = string.Empty;
                Host = string.Empty;
                AvailabeCredit = 0;
                CashBalance = 0;
                Status = eServiceStatus.Unknown;
                AccountStatus = eAccountStatus.Offline;

                if (objCheckLoginTimer != null)
                {
                    objCheckLoginTimer.Dispose();
                }

                return false;
            }
        }

        public bool TryLogin(string url, string userName, string password)
        {
            for (int i = 0; i < 4; i++)
            {
                Logger.Info("Loop " + i + " Begin TryLogin : " + userName);
                if (AccountStatus == eAccountStatus.Online)
                {
                    return true;
                }

                if (Login(url, userName, password))
                {
                    return true;
                }
                Random r = new Random();
                url = DataContainer.SboServers[r.Next(DataContainer.SboServers.Count())];
                Logger.Info("Loop " + i + " End TryLogin : " + userName);
                Thread.Sleep(15000);

                if (i >= 0)
                {
                    if (OnFakeRequest != null)
                    {
                        //  Task.Run(() => );
                        OnFakeRequest(url);
                    }

                    Thread.Sleep(10000 * i);

                    //WebBrowser w = new WebBrowser();
                    //w.ScriptErrorsSuppressed = true;
                    //w.Navigate(url);
                    //while (w.ReadyState != WebBrowserReadyState.Complete)
                    //{
                    //    Application.DoEvents();
                    //}
                    //w.Navigate("about:blank");
                }
            }

            return false;
        }

        public bool TryLoginScan(string url, string userName, string password)
        {
            for (int i = 0; i < 4; i++)
            {
                Logger.Info("Loop " + i + " Begin TryLogin : " + userName);
                if (AccountStatus == eAccountStatus.Online)
                {
                    return true;
                }

                if (Login(url, userName, password))
                {
                    return true;
                }
                Random r = new Random();
                url = DataContainer.SboScanServers[r.Next(DataContainer.SboScanServers.Count())];
                Logger.Info("Loop " + i + " End TryLogin : " + userName);
                Thread.Sleep(15000);

                if (i >= 0)
                {
                    if (OnFakeRequest != null)
                    {
                        //  Task.Run(() => );
                        OnFakeRequest(url);
                    }

                    Thread.Sleep(10000 * i);

                    //WebBrowser w = new WebBrowser();
                    //w.ScriptErrorsSuppressed = true;
                    //w.Navigate(url);
                    //while (w.ReadyState != WebBrowserReadyState.Complete)
                    //{
                    //    Application.DoEvents();
                    //}
                    //w.Navigate("about:blank");
                }
            }

            return false;
        }

        public bool ReLogin()
        {
            for (int i = 0; i < 4; i++)
            {
                if (AccountStatus == eAccountStatus.Online)
                {
                    return true;
                }

                if (Login(LoginUrl, UserName, Password))
                {
                    return true;
                }

                Thread.Sleep(15000);
                if (i >= 1)
                {
                    if (OnFakeRequest != null)
                    {
                        Task.Run(() => OnFakeRequest(LoginUrl));
                    }

                    Thread.Sleep(10000);
                    //WebBrowser w = new WebBrowser();
                    //w.ScriptErrorsSuppressed = true;
                    //w.Navigate(LoginUrl);
                    //while (w.ReadyState != WebBrowserReadyState.Complete)
                    //{
                    //    Application.DoEvents();
                    //}
                    //w.Navigate("about:blank");
                }
            }

            return false;
        }
    }


}
