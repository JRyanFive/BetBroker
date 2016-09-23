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
    public abstract class RestClient
    {
        //public static SendResponse Send(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
        //    Dictionary<string, string> headerParams, bool isDecompression, byte[] data)
        //{
        //    ServicePointManager.ServerCertificateValidationCallback =
        //        delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        //        {
        //            return true;
        //        };

        //    HttpWebRequest request;
        //    HttpWebResponse response = null;
        //    SendResponse sendResponse = new SendResponse();
        //    try
        //    {
        //        request = WebRequest.Create(requestUri) as HttpWebRequest;
        //        //request.ProtocolVersion
        //        request.Method = method;
        //        request.UserAgent = userAgent;

        //        request.CookieContainer = cookieContainer;

        //        foreach (var headerParam in headerParams)
        //        {
        //            request.Headers.Add(headerParam.Key, headerParam.Value);
        //        }

        //        if (isDecompression)
        //        {
        //            request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
        //                                              System.Net.DecompressionMethods.Deflate);

        //        }

        //        if (data != null)
        //        {
        //            // Set the content length in the request headers
        //            request.ContentLength = data.Length;
        //            // Write data  
        //            using (Stream stream = request.GetRequestStream())
        //            {
        //                stream.Write(data, 0, data.Length);
        //            }
        //        }

        //        // Get response  
        //        response = request.GetResponse() as HttpWebResponse;

        //        if (request.HaveResponse == true && response != null)
        //        {
        //            // Get the response stream
        //            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
        //            {
        //                sendResponse.StatusCode = response.StatusCode;
        //                sendResponse.StatusDescription = response.StatusDescription;
        //                sendResponse.Result = reader.ReadToEnd();
        //            }
        //        }
        //    }
        //    catch (WebException wex)
        //    {
        //        // This exception will be raised if the server didn't return 200 - OK  
        //        // Try to retrieve more information about the network error  
        //        if (wex.Response != null)
        //        {
        //            using (HttpWebResponse errorResponse = (HttpWebResponse)wex.Response)
        //            {
        //                sendResponse.StatusCode = errorResponse.StatusCode;
        //                sendResponse.StatusDescription = errorResponse.StatusDescription;
        //            }
        //        }
        //    }
        //    finally
        //    {
        //        if (response != null)
        //        {
        //            response.Close();
        //        }
        //    }

        //    return sendResponse;
        //}

        public SendResponse SendIbet(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
            bool isDecompression, byte[] data,string host, string referer = "", string contentType = "")
        {
            ServicePointManager.ServerCertificateValidationCallback =
                delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

                request.CookieContainer = cookieContainer;
                request.Host = host;
                request.Proxy = null;

                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                //request.ContentType = "application/x-www-form-urlencoded";
                request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                //request.ServicePoint.Expect100Continue = false;
                request.KeepAlive = true;
                request.ProtocolVersion = HttpVersion.Version10;
                //request.ServicePoint.ConnectionLimit = 24;                

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }
                
                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = referer;
                }

                //foreach (var headerParam in headerParams)
                //{
                //    request.Headers.Add(headerParam.Key, headerParam.Value);
                //}

                if (isDecompression)
                {
                    request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);

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
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return sendResponse;
        }


        /// <summary>
        /// User for Sbobet
        /// </summary>

        public SendResponse SendSbo(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
           Dictionary<string, string> headerParams, bool isDecompression, byte[] data, string referer, string host, string accept = "", string contentType = "")
        {
            //ServicePointManager.ServerCertificateValidationCallback =
            //    delegate(Object obj, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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

                request.CookieContainer = cookieContainer;
                
                request.Host = host;
                request.Proxy = null;
                request.KeepAlive = true;

                if (!string.IsNullOrEmpty(accept))
                {
                    request.Accept = accept;
                }
                //request.Accept = "text/plain, */*; q=0.01";
                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = referer;
                }

                foreach (var headerParam in headerParams)
                {
                    request.Headers.Add(headerParam.Key, headerParam.Value);
                }

                if (isDecompression)
                {
                    request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);

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
            finally
            {
                if (response != null)
                {
                    response.Close();
                }
            }

            return sendResponse;
        }
    }}
