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
    public class IbetHelper : CoreEngine
    {
        protected const int ODD_INDEX = 2;

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

            return SendIbet(urlRequest, "GET", agent, cookieContainer,
                null, host, accept, urlRefer, contentType);
        }

        public unsafe SendResponse Post(string urlHost, string host, string urlRequest, string accept,
            string agent, CookieContainer cookieContainer, string urlRefer, string param, string contentType = "")
        {
            byte[] byteArrayData = Encoding.UTF8.GetBytes(param);
            urlRequest = urlHost + urlRequest;
            urlRefer = urlHost + urlRefer;
            return SendIbet(urlRequest, "POST", agent, cookieContainer,
                byteArrayData, host, accept, urlRefer, contentType);
        }

        public unsafe SendResponse SendIbet(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.Method = method;
                request.UserAgent = userAgent;
                request.Host = host;
                request.Proxy = null;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.Accept = accept;
                request.ProtocolVersion = HttpVersion.Version11;
                request.Timeout = 10000;
                request.CookieContainer = cookieContainer;

                request.AllowAutoRedirect = false;
                request.ServicePoint.Expect100Continue = false;

                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                if (!string.IsNullOrEmpty(contentType))
                {
                    request.ContentType = contentType;
                }

                if (!string.IsNullOrEmpty(referer))
                {
                    request.Referer = referer;
                }

                if (string.IsNullOrEmpty(IpFake))
                {
                    request.KeepAlive = true;
                    request.Headers.Add("Accept-Encoding", "gzip, deflate");
                    ServicePoint servicePoaint = request.ServicePoint;
                    PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                    property.SetValue(servicePoaint, (byte)0, null);
                }
                else
                {
                    request.Headers.Add("X-Forwarded-For", IpFake);
                    request.Headers.Add("Accept-Encoding", "gzip, deflate,sdch");
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

        public unsafe SendResponse SendIbet302(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.Method = method;
                request.UserAgent = userAgent;

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }
                //request.AllowAutoRedirect = false;
                request.Proxy = null;
                request.Referer = referal;
                request.Host = host;
                request.Accept = accept;
                request.Timeout = 10000;
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                if (string.IsNullOrEmpty(IpFake))
                {
                    request.KeepAlive = true;
                    request.Headers.Add("Accept-Encoding", "gzip, deflate");
                    ServicePoint servicePoaint = request.ServicePoint;
                    PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                    property.SetValue(servicePoaint, (byte)0, null);
                }
                else
                {
                    request.Headers.Add("X-Forwarded-For", IpFake);
                    request.Headers.Add("Accept-Encoding", "gzip, deflate,sdch");
                }


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

        public unsafe SendResponse Send302Https(string requestUri, string method, string userAgent, CookieContainer cookieContainer,
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

                request.Proxy = null;
                request.Method = method;
                request.UserAgent = userAgent;

                if (cookieContainer != null)
                {
                    request.CookieContainer = cookieContainer;
                }

                request.KeepAlive = true;
                request.Referer = referal;
                request.Host = host;
                request.Accept = accept;
                request.Timeout = 10000;
                request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Add("Accept-Encoding", "gzip, deflate");
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");
                ServicePoint servicePoaint = request.ServicePoint;
                PropertyInfo property = servicePoaint.GetType().GetProperty("HttpBehaviour", BindingFlags.Instance | BindingFlags.NonPublic);
                property.SetValue(servicePoaint, (byte)0, null);

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


        public double RandomDouble()
        {
            Random rand = new Random();
            return rand.NextDouble();
        }


        public string GetKey(string username, string url, string key, string bp_Match = "", string ct = "", string rt = "")
        {
            string result = "";
            double random = RandomDouble();
            var _loc2_ = (int)Math.Round(random * 9999);
            //Debug.WriteLine("loc2 " + _loc2_);
            var _loc3_ = encode2((_loc2_ + 10000).ToString().Substring(1));
            //     Debug.WriteLine("loc3 " + _loc3_);

            var _loc4_ = "," + url; //CheckDomain();
            //  Debug.WriteLine("loc4 " + _loc4_);
            //if (_loc4_ == "")
            //{
            //    return encode(_loc2_.ToString() + key3, _loc2_).Trim() + _loc3_;
            //    //   return result.Trim();
            //}
            //_loc4_ = "," + _loc4_;
            var _loc5_ = "";
            var _loc6_ = "";
            switch (key)
            {
                case "bet":
                    _loc6_ = bp_Match; //eval("100"); // ExternalInterface.call("eval","fFrame.leftFrame.document.getElementById(\'bp_Match\').value");
                    _loc5_ = "," + _loc6_;
                    break;
                case "lodds":
                    _loc6_ = "l"; //ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_L.Market.value");
                    _loc6_ = _loc6_ + "_" + "1";//ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_L.Sport.value");
                    _loc6_ = _loc6_ + "_" + rt;//ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_L.RT.value");
                    _loc6_ = _loc6_ + "_" + ct;//ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_L.CT.value");
                    _loc5_ = "," + _loc6_;
                    break;
                case "dodds":
                    _loc6_ = "t"; //_local6 = ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_D.Market.value");
                    _loc6_ = _loc6_ + "_" + "1";//_local6 = ((_local6 + "_") + ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_D.Sport.value"));
                    _loc6_ = _loc6_ + "_" + rt; //_local6 = ((_local6 + "_") + ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_D.RT.value"));
                    _loc6_ = _loc6_ + "_" + ct; //_local6 = ((_local6 + "_") + ExternalInterface.call("eval", "fFrame.mainFrame.document.DataForm_D.CT.value"));
                    _loc5_ = "," + _loc6_; //_local5 = ("," + _local6);
                    break;
                case "login":
                    _loc6_ = username; // ExternalInterface.call("eval","fFrame.UserName");
                    _loc5_ = "," + _loc6_;
                    break;
            }
            var _loc7_ = "," + username.ToLower(); // ExternalInterface.call("eval",username);

            var stBuilder = new StringBuilder(_loc2_.ToString());
            stBuilder.Append(GetDateTime());
            stBuilder.Append(_loc5_);
            stBuilder.Append(_loc4_);
            stBuilder.Append(_loc7_);
            result = encode(stBuilder.ToString(), _loc2_).Trim() + _loc3_;
            return result;

        }

        public void ConvertServerTime(string strV)
        {
            try
            {
                Match match = (new Regex("var\\syear\\s=\\s\\d+;")).Match(strV);
                string str = match.Value.Replace("var year = ", "");
                str = str.Replace(";", "");
                int num = int.Parse(str.Trim());
                match = (new Regex("var\\smonth\\s=\\s\\d+;")).Match(strV);
                string str1 = match.Value.Replace("var month = ", "");
                str1 = str1.Replace(";", "");
                int num1 = int.Parse(str1.Trim());
                match = (new Regex("var\\sday\\s=\\s\\d+;")).Match(strV);
                string str2 = match.Value.Replace("var day = ", "");
                str2 = str2.Replace(";", "");
                int num2 = int.Parse(str2.Trim());
                match = (new Regex("var\\shrs\\s=\\s\\d+")).Match(strV);
                string str3 = match.Value.Replace("var hrs = ", "");
                str3 = str3.Replace("\n", "");
                int num3 = int.Parse(str3.Trim());
                match = (new Regex("var\\smin\\s=\\s\\d+;")).Match(strV);
                string str4 = match.Value.Replace("var min = ", "");
                str4 = str4.Replace(";", "");
                int num4 = int.Parse(str4.Trim());
                match = (new Regex("var\\ssec\\s=\\s\\d+;")).Match(strV);
                string str5 = match.Value.Replace("var sec = ", "");
                str5 = str5.Replace(";", "");
                int num5 = int.Parse(str5.Trim());
                DateTime svTime = new DateTime(num, num1, num2, num3, num4, num5);


                //DateTime svTime = new DateTime(yy, MM, dd, hh, mm, ss);
                ServerTimeSpan = DateTime.Now - svTime;
            }
            catch (Exception)
            {
                ServerTimeSpan = new TimeSpan(0);
            }

        }

        public string GetDateTime()
        {

            int _local2;
            //DateTime aaasaaa = DateTime.Now - aaaaaa;
            //string ssss = aaasaaa.ToString("yyyy/M/dd hh:mm:ss");

            //string _local1 = "2015/8/24 20:23:59"; // eval("100"); // ExternalInterface.call("eval", "fFrame.topFrame.year+'/'+fFrame.topFrame.month+'/'+fFrame.topFrame.day+' '+fFrame.topFrame.hrs+':'+fFrame.topFrame.min+':'+fFrame.topFrame.sec");
            string _local1 = (DateTime.Now - ServerTimeSpan).ToString("yyyy/M/dd hh:mm:ss");
            do
            {
                _local2 = DateTime.Now.Millisecond;
            }

            while (msec == _local2);
            msec = _local2;

            return (((_local1 + ".") + _local2));
        }

        public string encode(string param1, int param2)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(xor(param1, param2));

            var a = System.Convert.ToBase64String(plainTextBytes);
            //Debug.WriteLine("flush: " + a);
            return a;
            //return "";
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public string encode2(string param1)
        {
            string _loc2_ = "";
            int _loc3_ = 0;
            while (_loc3_ < param1.Length)
            {
                _loc2_ = _loc2_ + key2.Substring(int.Parse(param1.Substring(_loc3_, 1), 0), 1);
                _loc3_++;
            }
            return _loc2_;
        }

        public string xor(string _arg1, int _arg2)
        {
            string _local3 = key1;
            string _local4 = "";
            int _local5 = 0;
            while (_local5 < _arg1.Length)
            {
                _local4 = _local4 + Convert.ToChar(_arg1[_local5] ^ _local3[(_local5 + _arg2) % _local3.Length]);
                _local5++;
            };
            return _local4;

        }
    }
}
