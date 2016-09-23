using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using BcWin.Common.Objects;

namespace BcWin.Common.DTO
{
    public class ScanDriverSetup
    {
        public ScanDriverSetup()
        {
            ParamContainer = new Dictionary<string, ParamRequest>();
        }
        public int ExceptionCount { set; get; }
        public Object LockException = new Object();

        public string Username { set; get; }

        public string Password { set; get; }

        public string UrlHost { set; get; }
        public string Host { set; get; }

        public eAccountStatus AccountStatus { set; get; }

        public CookieContainer CookieContainer { set; get; }

        public Dictionary<string, ParamRequest> ParamContainer { get; set; }

        public System.Threading.Timer CheckLoginTimer { get; set; }

        public System.Threading.Timer ScanLiveTimer { get; set; }

        public System.Threading.Timer ScanNonLiveTimer { get; set; }
    }
}
