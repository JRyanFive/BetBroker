using BcWin.Common.Objects;
using log4net;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BcWin.Core.Helper;

namespace BCWin.Engine.Isn
{
    public class IsnHelper : CoreEngine
    {
        private readonly ILog Logger = LogManager.GetLogger(typeof(IsnHelper));

        private string HOST = "http://apiisn.com/betting/api";

        public long ConvertToUnixTime(DateTime dateTime_0)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan dateTime0 = dateTime_0 - dateTime.ToLocalTime();
            return checked((long)Math.Round((double)dateTime0.TotalSeconds * 1000));
            //var date = new DateTime(1970, 1, 1, 0, 0, 0, dt.Kind);
            //var unixTimestamp = System.Convert.ToInt64((dt - date).TotalMilliseconds);

            //return unixTimestamp;
        }

        //public long ConvertToUnixTime2(DateTime dateTime_0)
        //{
        //    DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        //    TimeSpan dateTime0 = dateTime_0 - dateTime.ToLocalTime();
        //    return checked((long)Math.Round((double)dateTime0.TotalSeconds * 1000));
        //}
        public SendResponse SendIsn(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
           byte[] data, string host, string accept, string referer = "", string contentType = "", string origin = "", string x_fp = "")
        {
            HttpWebRequest request;
            HttpWebResponse response = null;
            SendResponse sendResponse = new SendResponse();
            try
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;
                request.Method = method;
                request.UserAgent = userAgent;
                request.Host = host;
                request.Proxy = null;
                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version11;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;

             //   request.AllowAutoRedirect = false;
                request.ServicePoint.Expect100Continue = false;
                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                     System.Net.DecompressionMethods.Deflate);
                request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = referer;
                }

                if (!string.IsNullOrEmpty(x_fp))
                {
                         request.Headers.Add("X-fp", x_fp);
                }
                //if (!String.IsNullOrEmpty(origin))
                //{
                //    request.Headers.Add("Origin", origin);

                //}

                if (data != null)
                {
                    // Set the content length in the request headers
                    request.ContentLength = data.Length;
                    // Write data  
                    using (Stream stream = request.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }

                // Get response  
                response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse == true && response != null)
                {
                    // Get the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sendResponse.StatusCode = response.StatusCode;
                        sendResponse.StatusDescription = response.StatusDescription;
                        sendResponse.SetCookie = response.Headers.Get("Set-Cookie");
                        sendResponse.Result = reader.ReadToEnd();
                    }
                }
            }
            catch (WebException wex)
            {
                if (wex.Response != null)
                {
                    using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
                    {
                        sendResponse.StatusCode = errorResponse.StatusCode;
                        sendResponse.StatusDescription = errorResponse.StatusDescription;
                    }
                }
            }
            catch (Exception ex)
            {
                sendResponse.StatusCode = HttpStatusCode.NotFound;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return sendResponse;
        }


        public string Login(string username, string password, string locale = "en_US")
        {
            try
            {
                string param = "userName=" + username + "&password=" + password;

                using (WebClient wc = new WebClient())
                {
                    //   wc.Headers["userName"] = username;
                    // wc.Headers["password"] = password;
                    wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                    wc.Headers["Accept-Encoding"] = "gzip";
                    string HtmlResult = wc.UploadString(HOST + "/member/login", param);
                }

                var request = (HttpWebRequest)WebRequest.Create(HOST + "/member/login?" + param);

                //request.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                request.Method = "POST";
                //request.Accept = "application/json";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Headers.Add("Accept-Encoding", "gzip");
                //byte[] byteArray = Encoding.UTF8.GetBytes(param);

                //Stream dataStream = request.GetRequestStream();
                //dataStream.Write(byteArray, 0, byteArray.Length);
                //dataStream.Close();

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                string responseBody;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }

                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetAPIKey(string memberToken, string userId)
        {
            try
            {

                var request = (HttpWebRequest)WebRequest.Create(HOST + "/member/apikey");
                request.Proxy = null;
                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Method = "GET";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";


                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                string responseBody = string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }
                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetUserInfo(string memberToken, string userId, string apiKey)
        {
            try
            {

                var request = (HttpWebRequest)WebRequest.Create(HOST + "/member/info");
                request.Proxy = null;
                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);
                request.Method = "GET";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";


                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                string responseBody = string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }
                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }
        /// <summary>
        /// eventScheduleId = 3 is LIVE
        /// </summary>
        /// <param name="memberToken"></param>
        /// <param name="userId"></param>
        /// <param name="apiKey"></param>
        /// <param name="sportId"></param>
        /// <param name="eventScheduleId"></param>
        /// <returns></returns>
        public string GetEvents(string memberToken, string userId, string apiKey, int sportId = 0, int eventScheduleId = 3)
        {

            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HOST + "/event/list/" + sportId + "/" + eventScheduleId);
                request.Proxy = null;
                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);
                request.Method = "GET";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";


                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;


                }


                var stream = response.GetResponseStream();
                string responseBody = string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }
                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetOdds(string memberToken, string userId, string apiKey, int sportId = 0, string oddsGroupId = "", string binaryOddsFormat = "", int eventScheduleId = 3, string lastRequestKey = "")
        {
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HOST + "/event/list/" + sportId + "/" + oddsGroupId + "/" + binaryOddsFormat + "/" + eventScheduleId + "/" + lastRequestKey);
                request.Proxy = null;
                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);
                request.Method = "GET";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";


                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;


                }


                var stream = response.GetResponseStream();
                string responseBody = string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }
                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string PrepareBet(string memberToken, string userId, string apiKey, string selectionId, double stake, string odds, string handicap, string nativeOdds, string score, string eventPitcherId = "")
        {
            try
            {
                StringBuilder param = new StringBuilder("selectionId=" + selectionId);
                param.Append("&stake=" + stake)
                    .Append("&odds=" + odds)
                    .Append("&handicap=" + handicap)
                    .Append("&nativeOdds=" + nativeOdds)
                    .Append("&score=" + score);

                if (!String.IsNullOrEmpty(eventPitcherId))
                {
                    param.Append("&eventPitcherId=" + eventPitcherId);
                }

                var request = (HttpWebRequest)WebRequest.Create(HOST + "/bet/preparebet");

                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);

                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";

                byte[] byteArray = Encoding.UTF8.GetBytes(param.ToString());

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                string responseBody;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }

                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string ConfirmBet(string memberToken, string userId, string apiKey, string preparedBetId = "")
        {
            try
            {
                string param = "preparedBetId=" + preparedBetId;

                var request = (HttpWebRequest)WebRequest.Create(HOST + "/bet/confirmbet");

                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);

                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";

                byte[] byteArray = Encoding.UTF8.GetBytes(param.ToString());

                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;
                }

                var stream = response.GetResponseStream();
                string responseBody;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }

                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public string GetBetList(string memberToken, string userId, string apiKey)
        {
            try
            {

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HOST + "/bet/betlist");
                request.Proxy = null;
                request.Headers.Add("memberToken", memberToken);
                request.Headers.Add("userId", userId);
                request.Headers.Add("apiKey", apiKey);
                request.Method = "GET";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";


                HttpWebResponse response;
                try
                {
                    response = (HttpWebResponse)request.GetResponse();
                }
                catch (WebException ex)
                {
                    response = (HttpWebResponse)ex.Response;


                }


                var stream = response.GetResponseStream();
                string responseBody = string.Empty;
                using (var reader = new StreamReader(stream))
                {
                    responseBody = reader.ReadToEnd();
                }
                return responseBody;
            }
            catch (Exception)
            {
                return "";
            }

        }

        public Bitmap GetImage(string requestUri, CookieContainer cookieContainer, string host, string referal = "", string contentType = "")
        {


            HttpWebRequest request;
            HttpWebResponse response = null;
            //SendResponse sendResponse = new SendResponse();
            try
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;

                request.Method = "GET";
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:38.0) Gecko/20100101 Firefox/38.0";

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                    Logger.Info("Cookies before get image capcha: " + cookieContainer.GetCookieHeader(new Uri("http://isn01.com/membersite/login.jsp")));

                }

                request.Proxy = null;
                request.KeepAlive = true;

                request.Host = host;
                request.Accept = "*/*";
                request.Timeout = 10000;
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");

                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);


                if (!string.IsNullOrEmpty(referal))
                {
                    request.Referer = referal;
                }

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                // Get response  
                response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse && response != null)
                {
                    Logger.Info("Cookies after get image capcha: " + cookieContainer.GetCookieHeader(new Uri("http://isn01.com/membersite/login.jsp")));

                    //sendResponse.Location = response.Headers.Get("Location");
                    //sendResponse.SetCookie = response.Headers.Get("Set-Cookie");
                    //sendResponse.ReponseUri = response.ResponseUri.ToString();
                    using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                    {
                        Byte[] lnByte = reader.ReadBytes(1024 * 10);

                        MemoryStream mStream = new MemoryStream();
                        mStream.Write(lnByte, 0, Convert.ToInt32(lnByte.Length));

                        Bitmap bm = new Bitmap(mStream, false);
                        return bm;
                    }
                }
            }

            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return null;
        }
    }
}
