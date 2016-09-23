using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using BcWin.Common.Objects;

namespace BcWin.Core.Helper
{
    public class IsnHelper : CoreEngine
    {
        private string key1 = ")*&^%RGHJKIUREDF<MNHGREWlopadotemachose093ug3i;3jg9-[j4elachogaleokranioleipsanTEWVNEOWodrimhypotrimmatosilphioparaome4o34-jr'prweogue0hlitokatakech12dfefyj9=1!$^(&*BFRYJymenokichlepikBGTH%U*YDSossyphophattop./[-0io;'[o0231eristeralektryonoptekephalliokigklopeleiolagoiosiraio94-m3f4-otu349j23wsiraiobaphetraganopterygon";
        private string key2 = "KEBCWQUIVP";
        private string key3 = "!%d*a$gjsdm#gQB,K5fgYSg1erPI4TgD6SBMg8eMJFgeSETgyG9re$U";
        private int msec = 0;
        public TimeSpan ServerTimeSpan { get; set; }

        public SendResponse Get(string urlHost, string host, string urlRequest, string accept,
            string agent, CookieContainer cookieContainer, string urlRefer = "", string contentType = "")
        {
            urlRequest = urlHost + urlRequest;
            if (!string.IsNullOrEmpty(urlRefer))
            {
                urlRefer = urlHost + urlRefer;
            }

            return SendIsn(urlRequest, "GET", agent, cookieContainer,
                null, host, accept, urlRefer, contentType);
        }

        public unsafe SendResponse Post(string urlHost, string host, string urlRequest, string accept,
            string agent, CookieContainer cookieContainer, string urlRefer, string param, string contentType = "")
        {
            byte[] byteArrayData = Encoding.UTF8.GetBytes(param);
            urlRequest = urlHost + urlRequest;
            urlRefer = urlHost + urlRefer;
            return SendIsn(urlRequest, "POST", agent, cookieContainer,
                byteArrayData, host, accept, urlRefer, contentType);
        }

        public static SendResponse SendIsn(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
            byte[] data, string host, string accept, string referer = "", string contentType = "", string origin = "")
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
               // request.Proxy = null;
         //       request.UnsafeAuthenticatedConnectionSharing = true;
                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version11;
                request.KeepAlive = true;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;

               // request.AllowAutoRedirect = false;
       //         request.ServicePoint.Expect100Continue = false;

                request.Headers.Add("Accept-Language", "en-US,en;q=0.8");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                //force keep alive
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

                if (!String.IsNullOrEmpty(origin))
                {
                request.Headers.Add("Origin", origin);
                    
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


    }
}
