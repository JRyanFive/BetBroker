using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BcWin.Common.Objects;

namespace BcWin.Processor.Helper
{
    public abstract class SboHelper : CoreEngine
    {
        public string SboLoginName { get; set; }

        public string IpFake { get; set; }

        public SendResponse SendSbo(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
         byte[] data, string referer, string host, string accept, string contentType = "")
        {
            //ServicePointManager.ServerCertificateValidationCallback =
            //    delegate
            //    {
            //        return true;
            //    };

            HttpWebRequest request;
            HttpWebResponse response = null;
            SendResponse sendResponse = new SendResponse();
            try
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;

                //request.Headers.Add("HTTP_X_FORWARDED_FOR", "122.141.229.153");
                //request.Headers.Add("HTTP_CLIENT_IP", "122.141.229.153");
                //request.Headers.Add("HTTP_VIA", "122.141.229.153");
                //request.Headers.Set("HTTP_X_FORWARDED_FOR", "122.141.229.153");
                //request.Headers.Set("HTTP_CLIENT_IP", "122.141.229.153");
                //request.Headers.Set("HTTP_VIA", "122.141.229.153");
                //request.Headers.Add("REMOTE_ADDR", "101.251.121.3");
                //request.Headers.Add("REMOTE_HOST", "101.251.121.3");
                //request.Headers.Add("REMOTE_USER", "101.251.121.3");
                //request.Headers.Set("REMOTE_ADDR", "101.251.121.3");
                //request.Headers.Set("REMOTE_USER", "101.251.121.3");
                //request.Headers.Add("X-Forwarded-For", "187.139.211.62");
                //request.Headers.Add("Remote_Addr", "101.251.111.111");

                if (!string.IsNullOrEmpty(IpFake))
                {
                    request.Headers.Add("X-Forwarded-For", IpFake);
                }

                request.Method = method;
                request.UserAgent = userAgent;
                request.Accept = accept;
                request.Host = host;
                request.Referer = referer;
                request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;

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
                    // Get the response stream
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
                // This exception will be raised if the server didn't return 200 - OK  
                // Try to retrieve more information about the network error  
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

        public SendResponse SendSbo302(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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
                request = WebRequest.Create(requestUri) as HttpWebRequest;

                if (!string.IsNullOrEmpty(IpFake))
                {
                    request.Headers.Add("X-Forwarded-For", IpFake);
                }

                request.Method = method;
                request.UserAgent = userAgent;

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }

                request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.Referer = referal;
                request.Host = host;

                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.AllowAutoRedirect = false;

                if (!string.IsNullOrEmpty(accept))
                {
                    request.Accept = accept;
                }
                //request.Accept = "text/plain, */*; q=0.01";
                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                  System.Net.DecompressionMethods.Deflate);

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
                    using (HttpWebResponse errorResponse = (HttpWebResponse) wex.Response)
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
    }
}
