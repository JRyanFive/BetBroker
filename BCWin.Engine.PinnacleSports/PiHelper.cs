using System;
using System.Security.Cryptography;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BcWin.Common.Objects;

namespace BCWin.Engine.PinnacleSports
{
    public  class PiHelper
    {
        public static SendResponse Send302(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
             byte[] data, string referal, string host, string accept, string contentType = "")
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate
                {
                    return true;
                };

            HttpWebRequest request;
            HttpWebResponse response = null;
            SendResponse sendResponse = new SendResponse();
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(MyCertValidationCb); 
                request = WebRequest.Create(requestUri) as HttpWebRequest;
                request.Proxy = null;
                request.Method = method;
                request.UserAgent = userAgent;

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }
                request.AllowAutoRedirect = false;
                request.KeepAlive = true;
                request.Referer = referal;
                request.Host = host;
                request.Accept = accept;
                request.Timeout = 10000;
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

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

                if (request.HaveResponse && response != null)
                {
                    sendResponse.Location = response.Headers.Get("Location");
                    sendResponse.SetCookie = response.Headers.Get("Set-Cookie");
                    sendResponse.ReponseUri = response.ResponseUri.ToString();

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sendResponse.StatusCode = response.StatusCode;
                        sendResponse.StatusDescription = response.StatusDescription;
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

        public static SendResponse SendPinna(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
     byte[] data, string host, string accept, string referer = "", string contentType = "", string xml = "")
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate
                {
                    return true;
                };

            HttpWebRequest request;
            HttpWebResponse response = null;
            SendResponse sendResponse = new SendResponse();
            try
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;

                request.Method = method;
                request.UserAgent = userAgent;
                request.Host = host;
                //request.Proxy = null;
                //request.Proxy = new WebProxy("203.113.17.59", 808);
                //request.Proxy.Credentials = new NetworkCredential("b88", "abcd1234");
                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version10;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;
                if (!String.IsNullOrEmpty(xml))
                {
                    request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                    request.Headers.Add("Cache-Control", "no-cache");


                }
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");

                //request.ServicePoint.Expect100Continue = false;
                //request.ServicePoint.ConnectionLimit = 24;                

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = referer;
                }

                request.AutomaticDecompression = (DecompressionMethods.GZip | DecompressionMethods.Deflate);

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
                    sendResponse.SetCookie = response.Headers.Get("Set-Cookie");
                    sendResponse.ReponseUri = response.ResponseUri.ToString();

                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        sendResponse.StatusCode = response.StatusCode;
                        sendResponse.StatusDescription = response.StatusDescription;
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


      
        //private static Random rng = new Random();  
        //public static void Shuffle<T>(this IList<T> list)
        //{
        //    int n = list.Count;
        //    while (n > 1)
        //    {
        //        n--;
        //        int k = rng.Next(n + 1);
        //        T value = list[k];
        //        list[k] = list[n];
        //        list[n] = value;
        //    }
        //}

        public static string ScanOdd(string authorization, int sportId, int isLive = 0, long lastTime = 0, string leagueId = "")
        {
            try
            {
                string url = "https://api.pinnaclesports.com/v1/odds?oddsFormat=MALAY&sportid=" + sportId;
                if (lastTime != 0)
                {
                    url += "&since=" + lastTime;
                }

                if (isLive == 1)
                {
                    url += "&islive=" + isLive;
                }

                if (!String.IsNullOrEmpty(leagueId))
                {
                    url += "&leagueids=" + leagueId;
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                request.Headers.Add("Authorization", authorization);
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

        public static string ScanFixture(string authorization, int sportId, int isLive = 0, long lastTime = 0)
        {
            try
            {
                string url = "https://api.pinnaclesports.com/v1/fixtures?sportid=" + sportId;

                if (lastTime != 0)
                {
                    url += "&since=" + lastTime;
                }

                if (isLive == 1)
                {
                    url += "&islive=" + isLive;
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Proxy = null;
                request.Headers.Add("Authorization", authorization);
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

        //prepare
        public static string GetLine(string authorization, int sportId, string leagueId, string eventId, int periodNumber, string team, string betType)
        {
            //BET_TYPE
            //Value	Description
            //SPREAD	
            //MONEYLINE	
            //TOTAL_POINTS	
            //TEAM_TOTAL_POINTS	
            string url = "https://api.pinnaclesports.com/v1/line?oddsFormat=MALAY&sportid=" + sportId + "&leagueId=" + leagueId + "&eventId=" + eventId + "&periodNumber=" + periodNumber + "&betType=" + betType;

            if (betType == "SPREAD")
            {
                url += "&team=" + team;

            }
            else
            {
                url += "&side=" + team;
            }


            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Proxy = null;
            request.Headers.Add("Authorization", authorization);
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

        //confirm
        public static string PlaceBet(string authorization, string postJson)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.pinnaclesports.com/v1/bets/place");
                request.ServicePoint.Expect100Continue = false;

                request.Headers.Add("Authorization", authorization);
                request.Proxy = null;
                request.Method = "POST";
                request.Accept = "application/json";
                request.ContentType = "application/json; charset=utf-8";
                byte[] byteArray = Encoding.UTF8.GetBytes(postJson);
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

        public static string GetBalance(string authorization)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create("https://api.pinnaclesports.com/v1/client/balance");

                request.Proxy = null;
                request.Headers.Add("Authorization", authorization);
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

        public static string GetBetList(string authorization, DateTime from, DateTime to, string betlist = "settled")
        {
            try
            {
                string url = "https://api.pinnaclesports.com/v1/bets?betlist=" + betlist + "&fromDate=" + from.ToString("yyyy-MM-dd") + "&toDate=" + to.ToString("yyyy-MM-dd");
                var request = (HttpWebRequest)WebRequest.Create(url);

                request.Proxy = null;
                request.Headers.Add("Authorization", authorization);
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
    }


}
