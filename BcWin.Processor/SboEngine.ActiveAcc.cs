using System;
using System.Collections.Generic;
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

namespace BcWin.Processor
{
    public partial class SboEngine
    {
        public bool AcceptCon(string url, string username, string password)
        {
            Uri uriLogin = new Uri(url);
            string host = uriLogin.Host;
            LoginUrl = url;
            UserName = username;
            Password = password;

            try
            {
                var getMain = SendSbo302(url, "GET", userAgent, new CookieContainer(), null, url, host, accept, null);
                var tokenIndex = getMain.Result.IndexOf("'oev','sev','pid','sid','rid','tid','eid','cd','dt','mt','ms','ps'");
                var tokenIndexEnd = getMain.Result.IndexOf("]));", tokenIndex);
                string token = getMain.Result.Substring(tokenIndex + 69, tokenIndexEnd - (tokenIndex + 69));


                // cookieCollection.Add(new Cookie("secure", ""));
                Uri uriProcessSignIn = new Uri(url + "web/public/process-sign-in.aspx");
                var cookieProcessSignIn = BindCookieContainer(uriProcessSignIn, getMain.SetCookie);
                string loginParam = string.Format(@"id={0}&password={1}&lang=en&tk={2}&5=1&type=form&tzDiff=1", username,
                   HttpUtility.UrlEncode(password), token);
                byte[] byteLoginParam = Encoding.UTF8.GetBytes(loginParam);
                var processLoginMsg = SendSbo302(url + "web/public/process-sign-in.aspx", "POST", userAgent, cookieProcessSignIn,
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
                var processWellcomeMsg = SendSbo("http://" + hostWellcome + wellcomeMsg.Location, "GET", userAgent,
                    CookieContainer, null, processLoginMsg.Location, hostWellcome, accept, "text/html");

                //string loginParam = string.Format(@"id={0}&password={1}&lang=en&tk={2}&5=1&type=form&tzDiff=1", username,
                // HttpUtility.UrlEncode(password), token);
                byte[] aggree = Encoding.UTF8.GetBytes("action=I Agree");

                var accepCon = SendSbo("http://" + hostWellcome + "/web-root/restricted/misc/TermAndConditions.aspx",
                    "POST", userAgent,
                    CookieContainer, aggree, "http://" + hostWellcome + "/web-root/restricted/misc/TermAndConditions.aspx"
                    , hostWellcome, accept, "application/x-www-form-urlencoded");
                if (accepCon.Result.Contains("You are able to create a login name used to sign in to your account"))
                {
                    return true;
                }
                Logger.Error(username);
                return false;

            }
            catch (Exception ex)
            {
                Logger.Error(username);

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

        public bool LoginToActive(string url, string username, string password, string newpass)
        {
            Uri uriLogin = new Uri(url);
            string host = uriLogin.Host;
            LoginUrl = url;
            UserName = username;
            Password = password;

            try
            {
                var getMain = SendSbo302(url, "GET", userAgent, new CookieContainer(), null, url, host, accept, null);
                var tokenIndex = getMain.Result.IndexOf("'oev','sev','pid','sid','rid','tid','eid','cd','dt','mt','ms','ps'");
                var tokenIndexEnd = getMain.Result.IndexOf("]));", tokenIndex);
                string token = getMain.Result.Substring(tokenIndex + 69, tokenIndexEnd - (tokenIndex + 69));


                // cookieCollection.Add(new Cookie("secure", ""));
                Uri uriProcessSignIn = new Uri(url + "web/public/process-sign-in.aspx");
                var cookieProcessSignIn = BindCookieContainer(uriProcessSignIn, getMain.SetCookie);

                string loginParam = string.Format(@"id={0}&password={1}&lang=en&tk={2}&5=1&type=form&tzDiff=1", username,
                   HttpUtility.UrlEncode(password), token);
                byte[] byteLoginParam = Encoding.UTF8.GetBytes(loginParam);
                var processLoginMsg = SendSbo302(url + "web/public/process-sign-in.aspx", "POST", userAgent, cookieProcessSignIn,
                    byteLoginParam, url, host, accept, "application/x-www-form-urlencoded");

                if (processLoginMsg.Location.Contains("error"))
                {
                    throw new Exception(string.Format("Incorrect username: [{0}] and password: [{1}]",
                      username, password));
                }

                //var cookieforce = BindCookieContainer(uriLogin, processLoginMsg.SetCookie, cookieProcessSignIn);
                Uri uriProcessSignIn2 = new Uri("http://" + host + processLoginMsg.Location);
                var cookieProcessSignIn2 = BindCookieContainer(uriProcessSignIn2, getMain.SetCookie);
                var forceChangePass = SendSbo302("http://" + host + processLoginMsg.Location, "GET", userAgent, cookieProcessSignIn2,
                  null, url + "en/euro", host, accept, "text/html; charset=utf-8");
                //var cookieforceChangePass = BindCookieContainer(uriLogin, forceChangePass.SetCookie, cookieProcessSignIn2);

                var paramChangePass =
                    string.Format(
                        @"txtOldPwd={0}&txtNewPwd1={1}&txtNewPwd2={2}&hidAction=submit&redirect=",
                        HttpUtility.UrlEncode(password), HttpUtility.UrlEncode(newpass), HttpUtility.UrlEncode(newpass));

                paramChangePass = paramChangePass + "_{redirect}_";

                byte[] aaa = Encoding.UTF8.GetBytes(paramChangePass);
                var wellcomeMsg = SendSbo302("http://" + host + processLoginMsg.Location, "POST", userAgent, cookieProcessSignIn2,
                    aaa, url + "en/page/force-change-password", host, accept, "application/x-www-form-urlencoded");

                if (wellcomeMsg.Result.Contains("welcome.aspx"))
                {
                    return true;
                }
                Logger.Error(username);
                

                return true;

            }
            catch (Exception ex)
            {
                Logger.Error(username);

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
    }


}
