using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Objects;

namespace BcWin.Common.EventDelegate
{
    public delegate void LogScanEvent(LogScanMessage logMsg);
    public delegate void LogBetEvent(LogBetMessage logMsg);
    public delegate void LogOffEvent(string userName, eServerType serverType);

    public delegate void ExceptionEvent(ExceptionMessage logMsg, eServerType serverType, object obj);
    
    public delegate void ScanUpdateEvent(List<MatchOddDTO> updatedDataSbo, bool isLive, DateTime time);
    
    public delegate void ProcessStateChangeEvent(eServiceStatus status, string processState);

    public delegate void AccountStatusEvent(int tabCode, bool status);

    public delegate void FakeRequestEvent(string url);

    public delegate void PingEvent(DateTime time, eServerType serverType);

    public delegate void TransactionEvent(DateTime time, TransactionDTO transaction, string oddIdCheck, float oddCheck);
    
}
