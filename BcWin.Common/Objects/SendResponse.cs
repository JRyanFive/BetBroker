using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Common.Objects
{
    public class SendResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string Result { get; set; }
        public string SetCookie { get; set; }
        public string ReponseUri { get; set; }
        public string Location { get; set; }
    }
}
