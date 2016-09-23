using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Contract;
using BcWin.Core;
using BcWin.Core.EventDelegate;
using BCWin.Metadata;
using BcWin.Core.Helper;
using BcWin.Processor.Interface;
using BcWin.Processor.Properties;
using HtmlAgilityPack;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using TypescriptB;
using mshtml;

namespace BcWin.Processor
{
    public partial class IbetSubEngine
    {
        public bool ProxyLogin { get; set; }
        public string ProxyEndpoint { get; set; }

        public event FakeRequestEvent OnFakeRequest;

        public bool Login(string url, string userName, string password)
        {
            if (ProxyLogin)
            {
                return LoginProxy(url, userName, password);
            }

            return LoginNomal(url, userName, password);
        }

        public bool LoginProxy(string url, string userName, string password)
        {
            UrlHost = string.Empty;
            Host = string.Empty;
            LoginUrl = url;
            UserName = userName.ToUpper();
            Password = password;
            ParamContainer = new Dictionary<string, ParamRequest>();

            try
            {
                Uri uri = new Uri(url);

                NetTcpBinding b = new NetTcpBinding();
                b.Security.Mode = SecurityMode.None;

                EndpointAddress vEndPoint = new EndpointAddress(ProxyEndpoint);
                ChannelFactory<IBcSupService> cf = new ChannelFactory<IBcSupService>
                    (b, vEndPoint);

                IBcSupService serviceProxy = cf.CreateChannel();
                serviceProxy.Ping();

                var processLoginMsg = serviceProxy.Login(url, userName, password);

                if (processLoginMsg == null || processLoginMsg.Result.Contains("Incorrect username"))
                {
                    throw new Exception(string.Format("Incorrect username: [{0}] and password: [{1}]",
                        userName, password));
                }

                var validateIndex = processLoginMsg.Result.IndexOf("location='");
                var validateIndexEnd = processLoginMsg.Result.IndexOf("';", validateIndex);
                string validateUrl = processLoginMsg.Result.Substring(validateIndex + 10, validateIndexEnd - (validateIndex + 10));

                Logger.Info(UserName + " >> Validate Url::::" + validateUrl);
                Uri uriValidate = new Uri(validateUrl);
                if (validateUrl.Contains("www"))
                {
                    return false;
                }

                var cookieValidate = BindCookieContainer(uriValidate, processLoginMsg.SetCookie);

                var validateMsg = SendIbet302(validateUrl, "GET", userAgent, cookieValidate, null, url + "ProcessLogin.aspx",
                    uriValidate.Host, accept, null);

                UrlHost = uri.Scheme + Uri.SchemeDelimiter + uriValidate.Host;
                Uri urimain = new Uri(UrlHost);
                Host = urimain.Host;

                var cookieMain = BindCookieContainer(urimain, validateMsg.SetCookie, cookieValidate);

                //2 lan get main roi get top roi get left
                //var mainMsg1 = SendIbet302(UrlHost + validateMsg.Location,
                //    "GET", userAgent, cookieMain, null, validateUrl, Host, accept, null);
                var mainMsg1 = SendIbet302(validateMsg.ReponseUri,
                    "GET", userAgent, cookieMain, null, validateUrl, Host, accept, "text/html");

                CookieContainer = BindCookieContainer(urimain, mainMsg1.SetCookie, cookieMain);

                var mainMsg2 = SendIbet302(mainMsg1.ReponseUri,
                    "GET", userAgent, CookieContainer, null, mainMsg1.ReponseUri,
                    Host, accept, "text/html");
                CookieContainer = BindCookieContainer(urimain, mainMsg2.SetCookie, CookieContainer);

                if (mainMsg2.Result.Contains("frmChangePW") || mainMsg2.Result.Contains("Change_Password"))
                {
                    var changePass = SendIbet302(UrlHost + "/Change_Password.aspx?expiry=yes",
                        "GET", userAgent, CookieContainer, null, mainMsg1.ReponseUri,
                        Host, accept, "text/html");

                    string changePassSubmitParam =
                        "txtOldPW=&txtPW=&txtConPW=&hidSubmit=&hidRemindSubmit=next&hidLowerCaseOldPW=&hidExDate=1";

                    byte[] byteChangePassSubmitParam = Encoding.UTF8.GetBytes(changePassSubmitParam);

                    var changePassSubmit = SendIbet302(UrlHost + "/Change_Password.aspx",
                        "POST", userAgent, CookieContainer, byteChangePassSubmitParam,
                        UrlHost + "/Change_Password.aspx?expiry=yes",
                        Host, accept, "application/x-www-form-urlencoded");
                }

                var topMenuMsg = SendIbet302(UrlHost + "/topmenu.aspx",
                   "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                   Host, accept, "text/html");
                ConvertServerTime(topMenuMsg.Result);

                CookieContainer = BindCookieContainer(urimain, topMenuMsg.SetCookie, CookieContainer);

                SendResponse leftMsg = SendIbet302(UrlHost + "/LeftAllInOne.aspx",
                  "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                  Host, accept, "text/html");

                if (string.IsNullOrEmpty(leftMsg.Result))
                {
                    Logger.Info("GET LEFT MESSAGE LAN 2!");
                    leftMsg = SendIbet302(UrlHost + "/LeftAllInOne.aspx",
                        "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                        Host, accept, "text/html");
                }

                CookieContainer = BindCookieContainer(urimain, leftMsg.SetCookie, CookieContainer);

                int indexNameK = leftMsg.Result.IndexOf("name=\"k");
                int endIndexNameK = leftMsg.Result.IndexOf(" ", indexNameK);
                int indexValueK = leftMsg.Result.IndexOf("value=\"v", indexNameK);
                int endIndexValueK = leftMsg.Result.IndexOf(" ", indexValueK);
                string kName = leftMsg.Result.Substring(indexNameK + 6, endIndexNameK - (indexNameK + 7)).Trim();
                string kValue =
                    leftMsg.Result.Substring(indexValueK + 7, endIndexValueK - (indexValueK + 8)).Trim();
                ParamContainer["K"] = new ParamRequest(kName, kValue);
           
                var newMarketMsg = SendIbet302(UrlHost + "/UnderOver.aspx?Market=t&DispVer=new",
                   "GET", userAgent, CookieContainer, null, leftMsg.ReponseUri,
                   Host, accept, "text/html");
                CookieContainer = BindCookieContainer(urimain, newMarketMsg.SetCookie, CookieContainer);

                //CookieContainer = BindCookieContainer(urimain, setCookie, CookieContainer);

                UpdateException(this);

                ////Begin check login
                objCheckLoginTimer = new System.Threading.Timer(CheckLoginCallback, null, 0,
                    SystemConfig.TIME_CHECK_LOGIN_IBET);

                AccountStatus = eAccountStatus.Online;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Loi dang nhap", ex);

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

        public bool LoginNomal(string url, string userName, string password)
        {
            UrlHost = string.Empty;
            Host = string.Empty;
            LoginUrl = url;
            UserName = userName.ToUpper();
            Password = password;
            ParamContainer = new Dictionary<string, ParamRequest>();

            try
            {
                Uri uri = new Uri(url);
                string host = uri.Host;

                var getMain = SendIbet302(url + "login888.aspx", "GET", userAgent, null, null, url, host, accept, null);
                var docrootLeftInOne = new HtmlAgilityPack.HtmlDocument();
                docrootLeftInOne.LoadHtml(getMain.Result);
                var codeLogin = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='text' and @name='txtCode']")
                    .Attributes["value"].Value;
                var hashpass = GetHashPass(userName, password, codeLogin);
                string loginParam = string.Format(@"selLang=en&txtID={0}&txtPW={1}&txtCode={2}&hidubmit=&IEVerison
=0&detecResTime=138&IsSSL=0&PF=Default", userName, hashpass, codeLogin);
                var cookieContainer = BindCookieContainer(uri, getMain.SetCookie);
                byte[] byteLoginParam = Encoding.UTF8.GetBytes(loginParam);
                SendResponse processLoginMsg = SendIbet302(url + "ProcessLogin.aspx", "POST", userAgent, cookieContainer, byteLoginParam, url, host, accept,
                    "application/x-www-form-urlencoded");

                if (processLoginMsg.Result.Contains("www"))
                {
                    processLoginMsg = SendIbet302(url + "ProcessLogin.aspx", "POST", userAgent, cookieContainer, byteLoginParam, url, host, accept,
                    "application/x-www-form-urlencoded");
                }
                //Logger.Info("Process Msg: " + processLoginMsg.Result);
                if (processLoginMsg.Result.Contains("Incorrect username"))
                {
                    throw new Exception(string.Format("Incorrect username: [{0}] and password: [{1}]",
                        userName, password));
                }

                var validateIndex = processLoginMsg.Result.IndexOf("location='");
                var validateIndexEnd = processLoginMsg.Result.IndexOf("';", validateIndex);
                string validateUrl = processLoginMsg.Result.Substring(validateIndex + 10, validateIndexEnd - (validateIndex + 10));

                Logger.Info(UserName + " >> Validate Url::::" + validateUrl);
                Uri uriValidate = new Uri(validateUrl);
                if (validateUrl.Contains("www"))
                {
                    return false;
                }

                var cookieValidate = BindCookieContainer(uriValidate, processLoginMsg.SetCookie);

                var validateMsg = SendIbet302(validateUrl, "GET", userAgent, cookieValidate, null, url + "ProcessLogin.aspx",
                    uriValidate.Host, accept, null);

                UrlHost = uri.Scheme + Uri.SchemeDelimiter + uriValidate.Host;
                Uri urimain = new Uri(UrlHost);
                Host = urimain.Host;

                var cookieMain = BindCookieContainer(urimain, validateMsg.SetCookie, cookieValidate);

                var mainMsg1 = SendIbet302(validateMsg.ReponseUri,
                    "GET", userAgent, cookieMain, null, validateUrl, Host, accept, "text/html");

                CookieContainer = BindCookieContainer(urimain, mainMsg1.SetCookie, cookieMain);

                var mainMsg2 = SendIbet302(mainMsg1.ReponseUri,
                    "GET", userAgent, CookieContainer, null, mainMsg1.ReponseUri,
                    Host, accept, "text/html");
                CookieContainer = BindCookieContainer(urimain, mainMsg2.SetCookie, CookieContainer);

                if (mainMsg2.Result.Contains("frmChangePW") || mainMsg2.Result.Contains("Change_Password"))
                {
                    var changePass = SendIbet302(UrlHost + "/Change_Password.aspx?expiry=yes",
                        "GET", userAgent, CookieContainer, null, mainMsg1.ReponseUri,
                        Host, accept, "text/html");

                    string changePassSubmitParam =
                        "txtOldPW=&txtPW=&txtConPW=&hidSubmit=&hidRemindSubmit=next&hidLowerCaseOldPW=&hidExDate=1";

                    byte[] byteChangePassSubmitParam = Encoding.UTF8.GetBytes(changePassSubmitParam);

                    var changePassSubmit = SendIbet302(UrlHost + "/Change_Password.aspx",
                        "POST", userAgent, CookieContainer, byteChangePassSubmitParam,
                        UrlHost + "/Change_Password.aspx?expiry=yes",
                        Host, accept, "application/x-www-form-urlencoded");
                }

                var topMenuMsg = SendIbet302(UrlHost + "/topmenu.aspx",
                   "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                   Host, accept, "text/html");
                ConvertServerTime(topMenuMsg.Result);

                CookieContainer = BindCookieContainer(urimain, topMenuMsg.SetCookie, CookieContainer);

                SendResponse leftMsg = SendIbet302(UrlHost + "/LeftAllInOne.aspx",
                  "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                  Host, accept, "text/html");

                if (string.IsNullOrEmpty(leftMsg.Result))
                {
                    Logger.Info("GET LEFT MESSAGE LAN 2!");
                    leftMsg = SendIbet302(UrlHost + "/LeftAllInOne.aspx",
                        "GET", userAgent, CookieContainer, null, mainMsg2.ReponseUri,
                        Host, accept, "text/html");
                }

                CookieContainer = BindCookieContainer(urimain, leftMsg.SetCookie, CookieContainer);

                int indexNameK = leftMsg.Result.IndexOf("name=\"k");
                int endIndexNameK = leftMsg.Result.IndexOf(" ", indexNameK);
                int indexValueK = leftMsg.Result.IndexOf("value=\"v", indexNameK);
                int endIndexValueK = leftMsg.Result.IndexOf(" ", indexValueK);
                string kName = leftMsg.Result.Substring(indexNameK + 6, endIndexNameK - (indexNameK + 7)).Trim();
                string kValue =
                    leftMsg.Result.Substring(indexValueK + 7, endIndexValueK - (indexValueK + 8)).Trim();
                ParamContainer["K"] = new ParamRequest(kName, kValue);
              
                var newMarketMsg = SendIbet302(UrlHost + "/UnderOver.aspx?Market=t&DispVer=new",
                   "GET", userAgent, CookieContainer, null, leftMsg.ReponseUri,
                   Host, accept, "text/html");
                CookieContainer = BindCookieContainer(urimain, newMarketMsg.SetCookie, CookieContainer);

                //CookieContainer = BindCookieContainer(urimain, setCookie, CookieContainer);

                UpdateException(this);

                ////Begin check login
                objCheckLoginTimer = new System.Threading.Timer(CheckLoginCallback, null, 0,
                    SystemConfig.TIME_CHECK_LOGIN_IBET);

                AccountStatus = eAccountStatus.Online;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error("Loi dang nhap", ex);

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
        
        public string GetHashPass(string userName, string pass, string code)
        {
            pass = IbetUtils.hashPass(userName, pass, code);
            if (pass != null)
            {
                return pass;
            }
            else
            {
                WebBrowser w = new WebBrowser();
                w.DocumentText = DataContainer.LoginIbetScript;

                object[] o = new object[3];
                o[0] = userName;
                o[1] = pass;
                o[2] = code;
                while (w.ReadyState != WebBrowserReadyState.Complete)
                {
                    Application.DoEvents();
                    Thread.Sleep(300);
                }

                object hashPass = w.Document.InvokeScript("GetHashPass", o);

                return hashPass.ToString();
            }
        }

        public bool TryLogin(string url, string userName, string password)
        {
            for (int i = 0; i < 3; i++)
            {
                Logger.Info("Loop " + i + " Begin TryLogin : " + userName);
                if (Login(url, userName, password))
                {
                    return true;
                }
                Logger.Info("Loop " + i + " End TryLogin : " + userName);
                Thread.Sleep(15000);
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

                Logger.Info("Loop " + i + " Begin Relogin : " + UserName + " URL: " + LoginUrl);
                if (Login(LoginUrl, UserName, Password))
                {
                    Logger.Info("LOGIN THANH CONG : " + UserName);
                    return true;
                }
                Random r = new Random();
                LoginUrl = DataContainer.IbetServers[r.Next(DataContainer.IbetServers.Count())];
                Logger.Info("Loop " + i + " End Relogin : " + UserName);
                Thread.Sleep(15000);

                if (i >= 2)
                {
                    Thread.Sleep(60000 * (i + 2));
                }
            }

            return false;
        }
    }
}
