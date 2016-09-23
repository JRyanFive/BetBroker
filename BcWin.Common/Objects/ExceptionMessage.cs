using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;

namespace BcWin.Common.Objects
{
    public class ExceptionMessage
    {
        public ExceptionMessage(eExceptionType exceptionType)
        {
            ExceptionType = exceptionType;
        }

        public ExceptionMessage(eExceptionType exceptionType, string msg)
        {
            ExceptionType = exceptionType;
            Message = msg;
        }

        public eExceptionType ExceptionType { get; set; }

        public string Message { get; set; }
    }
}
