using System;
using System.Security.Cryptography;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BcWin.Common.Objects;
using BcWin.Core.Helper;

namespace BCWin.Engine.PinnacleSports2
{
    public  class PiHelper: CoreEngine
    {
        public static SendResponse Send302Https(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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
                request.Proxy = null;

                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version11;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                
                ServicePoint servicePoaint = request.ServicePoint;
                PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(servicePoaint, (byte)0, null);

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
