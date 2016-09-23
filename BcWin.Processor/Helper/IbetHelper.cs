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
    public class IbetHelper : CoreEngine
    {
        public SendResponse Get(string urlHost, string host, string urlRequest, string accept,
            string agent, CookieContainer cookieContainer, string urlRefer = "", string contentType = "")
        {
            urlRequest = urlHost + urlRequest;
            if (!string.IsNullOrEmpty(urlRefer))
            {
                urlRefer = urlHost + urlRefer;
            }

            return SendIbet(urlRequest, "GET", agent, cookieContainer,
                null, host, accept, urlRefer, contentType);
        }

        public SendResponse Post(string urlHost, string host, string urlRequest, string accept,
            string agent, CookieContainer cookieContainer, string urlRefer, string param, string contentType = "")
        {
            byte[] byteArrayData = Encoding.UTF8.GetBytes(param);
            urlRequest = urlHost + urlRequest;
            urlRefer = urlHost + urlRefer;
            return SendIbet(urlRequest, "POST", agent, cookieContainer,
                byteArrayData, host, accept, urlRefer, contentType);
        }

        public SendResponse SendIbet(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
            byte[] data, string host, string accept, string referer = "", string contentType = "")
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

                request.Method = method;
                request.UserAgent = userAgent;
                request.Host = host;
                request.Proxy = null;
                //request.Proxy = new WebProxy("203.113.17.59", 808);
                //request.Proxy.Credentials = new NetworkCredential("b88", "abcd1234");
                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version11;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;

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
        
        public SendResponse SendIbet302(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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
  
                request.Method = method;
                request.UserAgent = userAgent;

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }
                //request.AllowAutoRedirect = false;
                request.Proxy = null;
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
    }
}
