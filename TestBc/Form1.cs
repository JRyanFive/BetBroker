using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestBc
{
    public partial class Form1 : Form
    {
        private const string userAgent =
      "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var aaa = Send("http://checkip.dyndns.org/", "GET");
            richTextBox1.Text = aaa;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var aaa = Send("http://checkip.dyndns.org/", "GET",textBox1.Text);
            richTextBox1.Text = aaa;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var aaa = Send("http://checkip.dyndns.org/", "GET", textBox1.Text);
            richTextBox1.Text = aaa;
        }

        public string Send(string requestUri, string method, string ip = "")
        {
            //ServicePointManager.ServerCertificateValidationCallback =
            //    delegate
            //    {
            //        return true;
            //    };

            HttpWebRequest request;
            HttpWebResponse response = null;

            try
            {
                request = WebRequest.Create(requestUri) as HttpWebRequest;

                request.ServicePoint.BindIPEndPointDelegate = delegate(ServicePoint servicePoint,IPEndPoint remoteEndPoint,int retryCount)
                {

                    if (remoteEndPoint.AddressFamily != System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        if (string.IsNullOrEmpty(ip))
                        {
                            return new IPEndPoint(IPAddress.Any, 0);
                        }
                        else
                        {
                            return new IPEndPoint(IPAddress.Parse(ip), 0);
                        }
                    }
                    else
                    {
                        return new IPEndPoint(IPAddress.IPv6Any, 0);
                    }

                };

                request.Method = method;
                request.UserAgent = userAgent;
                request.Accept = accept;
                request.UnsafeAuthenticatedConnectionSharing = true;
                request.Proxy = null;
                request.KeepAlive = true;
                request.Timeout = 60000;
                //request.CookieContainer = cookieContainer;
                request.ServicePoint.Expect100Continue = false;
                request.AllowAutoRedirect = false;
                request.ProtocolVersion = HttpVersion.Version11;

                request.Headers.Add("Accept-Encoding", "gzip, deflate");

                request.AutomaticDecompression = (System.Net.DecompressionMethods.GZip |
                                                      System.Net.DecompressionMethods.Deflate);

                // Get response  
                response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse && response != null)
                {
                    // Get the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
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
