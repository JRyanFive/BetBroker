using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using BcWin.Common.Objects;

namespace BcWin.Core.Helper
{
    public abstract class SboHelper : CoreEngine
    {
        public string SboLoginName { get; set; }
        
        public unsafe SendResponse SendSbo(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.ServicePoint.BindIPEndPointDelegate = delegate(ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
                {

                    if (remoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        if (string.IsNullOrEmpty(RealIpAddress))
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else
                        {
                            return new IPEndPoint(IPAddress.Parse(RealIpAddress), 0);
                        }
                    }
                    else
                    {
                        return new IPEndPoint(IPAddress.IPv6Any, 0);
                    }

                };

                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                if (!string.IsNullOrEmpty(IpFake))
                {
                    request.Headers.Add("X-Forwarded-For", IpFake);
                }

                request.Method = method;
                request.UserAgent = userAgent;
                request.Accept = accept;
                request.Host = host;
                request.Referer = referer;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;
                request.ServicePoint.Expect100Continue = false;
                request.AllowAutoRedirect = false;
                request.ProtocolVersion = HttpVersion.Version11;

                //request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");

                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);

                ServicePoint servicePoaint = request.ServicePoint;
                PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(servicePoaint, (byte)0, null);

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

        public unsafe SendResponse SendSbo302(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.ServicePoint.BindIPEndPointDelegate = delegate (ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
                {

                    if (remoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        if (string.IsNullOrEmpty(RealIpAddress))
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else
                        {
                            return new IPEndPoint(IPAddress.Parse(RealIpAddress), 0);
                        }
                    }
                    else
                    {
                        return new IPEndPoint(IPAddress.IPv6Any, 0);
                    }

                };
                //request.Headers.Add("X-Requested-With", "XMLHttpRequest");

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
                //else
                //{
                //    request.CookieContainer = new CookieContainer();
                //}

                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                //request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 20000;
                request.Referer = referal;
                request.Host = host;


                ServicePoint servicePoaint = request.ServicePoint;
                PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(servicePoaint, (byte)0, null);

                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                //request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.AllowAutoRedirect = false;

                if (!string.IsNullOrEmpty(accept))
                {
                    request.Accept = accept;
                }

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
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
                response = (HttpWebResponse)request.GetResponse();

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
                throw;
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
                throw;
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


        public unsafe SendResponse SendSbo302(ref CookieCollection cookieCollection, string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.ServicePoint.BindIPEndPointDelegate = delegate (ServicePoint servicePoint, IPEndPoint remoteEndPoint, int retryCount)
                {

                    if (remoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        if (string.IsNullOrEmpty(RealIpAddress))
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else
                        {
                            return new IPEndPoint(IPAddress.Parse(RealIpAddress), 0);
                        }
                    }
                    else
                    {
                        return new IPEndPoint(IPAddress.IPv6Any, 0);
                    }

                };
                //request.Headers.Add("X-Requested-With", "XMLHttpRequest");

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
                //else
                //{
                //    request.CookieContainer = new CookieContainer();
                //}

                request.ServicePoint.Expect100Continue = false;
                request.ProtocolVersion = HttpVersion.Version11;
                //request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 20000;
                request.Referer = referal;
                request.Host = host;


                ServicePoint servicePoaint = request.ServicePoint;
                PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(servicePoaint, (byte)0, null);

                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                //request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.AllowAutoRedirect = false;

                if (!string.IsNullOrEmpty(accept))
                {
                    request.Accept = accept;
                }

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
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
                response = (HttpWebResponse)request.GetResponse();

                if (request.HaveResponse && response != null)
                {
                    cookieCollection = response.Cookies;
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
                throw;
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
                throw;
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

        public StringBuilder CleanData(StringBuilder data)
        {
            return data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "")
                     .Replace("\\xF3ng", "")
                     .Replace("\\u0110", "")
                     .Replace("\\x", "");
        }

        public string CleanData(string data)
        {
            return data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "")
                     .Replace("\\xF3ng", "")
                     .Replace("\\u0110", "")
                     .Replace("\\x", "");
        }
    }
}
