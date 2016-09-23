using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Core.Server
{
    public class ServerHelper
    {
        public static bool CheckDomain(string url)
        {
            try
            {
                var ping = new System.Net.NetworkInformation.Ping();
                if (url.IndexOf("http://") >= 0 || url.IndexOf("/") >= 0)
                {
                    url = url.Replace("http://", "").Replace("/", "");
                }
                var result = ping.Send(url);
                if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception)
            {               
            }
            return false;
        }

        public static bool CheckDomain(System.Net.IPAddress ipAddress)
        {
            try
            {
                var ping = new System.Net.NetworkInformation.Ping();
                var result = ping.Send(ipAddress);
                if (result.Status == System.Net.NetworkInformation.IPStatus.Success)
                {
                    return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }
    }
}
