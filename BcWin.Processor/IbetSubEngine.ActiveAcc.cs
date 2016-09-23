using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
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
        public bool LoginToActive(string url, string userName, string password)
        {
            UrlHost = string.Empty;
            Host = string.Empty;
            LoginUrl = url;
            UserName = userName;
            Password = password;
            
            try
            {
                Uri uri = new Uri(url);
                string host = uri.Host;

                var getMain = SendIbet302(url, "GET", userAgent, null, null, url, host, accept, null);
                var docrootLeftInOne = new HtmlAgilityPack.HtmlDocument();
                docrootLeftInOne.LoadHtml(getMain.Result);
                var codeLogin = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='text' and @name='txtCode']")
                    .Attributes["value"].Value;
                var hashpass = GetHashPass(userName, password, codeLogin);
                string loginParam = string.Format(@"selLang=en&txtID={0}&txtPW={1}&txtCode={2}&hidubmit=&IEVerison
=0&detecResTime=138&IsSSL=0&PF=Default", userName, hashpass, codeLogin);
                var cookieContainer = BindCookieContainer(uri, getMain.SetCookie);
                byte[] byteLoginParam = Encoding.UTF8.GetBytes(loginParam);
                var processLoginMsg = SendIbet302(url + "ProcessLogin.aspx", "POST", userAgent, cookieContainer, byteLoginParam, url, host, accept,
                    "application/x-www-form-urlencoded");

                if (processLoginMsg.Result.Contains("Incorrect username"))
                {
                    throw new Exception(string.Format("Incorrect username: [{0}] and password: [{1}]",
                        userName, password));
                }

                var validateIndex = processLoginMsg.Result.IndexOf("location='");
                var validateIndexEnd = processLoginMsg.Result.IndexOf("';", validateIndex);
                string validateUrl = processLoginMsg.Result.Substring(validateIndex + 10, validateIndexEnd - (validateIndex + 10));

                //Logger.Info(UserName + " >> Validate Url::::" + validateUrl);
                Uri uriValidate = new Uri(url + validateUrl);
                if (validateUrl.Contains("www"))
                {
                    return false;
                }

                var cookieValidate = BindCookieContainer(uriValidate, processLoginMsg.SetCookie);
                var validateMsg = SendIbet302(url + validateUrl, "GET", userAgent, cookieValidate, null, url + "ProcessLogin.aspx",
                    uriValidate.Host, accept, null);

                //var validate11Msg = SendIbet302(url + "/rulesContent.aspx", "GET", userAgent, cookieValidate, null, url + "ProcessLogin.aspx",
                //    uriValidate.Host, accept, null);
                //var cookieMaian = BindCookieContainer(uriValidate, validate11Msg.SetCookie, cookieValidate);
                //cookieMaian = BindCookieContainer(uriValidate, validateMsg.SetCookie, cookieValidate);
                byte[] byteLogin1 = Encoding.UTF8.GetBytes("Accept=YES");
                var validate1Msg = SendIbet302(url + validateUrl, "POST", userAgent, cookieContainer,
                    byteLogin1, url + validateUrl,
    uriValidate.Host, accept, "application/x-www-form-urlencoded");

                if (validate1Msg.Result.Contains("ValidateTicket"))
                {
                    return true;
                }
                Logger.Error(userName);
                //throw new Exception("Wrong user" + userName); 
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(userName);
                
                return false;
            }

        }
    }
}
