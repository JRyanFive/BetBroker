using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Common.Objects;
using BCWin.Engine.Ibet;
using BCWin.Engine.Sbo;

namespace BcWin.Processor.Interface
{
    public interface IProcessor
    {
        event ProcessStateChangeEvent OnProcessStateChange;
        event PingEvent OnPingEvent;
        eServiceStatus Status { get; set; }
        int StartFailCount { get; set; }

        string ProcessorName { get; set; }

        Dictionary<Guid, AccountDTO> AccountDic { get; set; }
        IbetEngine IbetEngine { get; set; }
        SboEngine SboEngine { get; set; }
        ProcessorConfigInfoDTO ProcessorConfigInfo { get; set; }
        eServiceStatus Start(eScanType scanType);
        eServiceStatus ReStart();
        void Pause();
   
        void Dispose();

    }
}
