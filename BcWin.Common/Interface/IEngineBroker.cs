using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Common.Objects;

namespace BcWin.Common.Interface
{
    public interface IEngineBroker
    {
        bool IsBuyBroker { get; set; }
        event ExceptionEvent OnExceptionEvent;
        string EngineId { get; set; }
        eServerType ServerType { get; set; }
        int ExchangeRate { get; set; }
        eAccountStatus AccountStatus { get; set; }
        string UserName { get; set; }

        void StartScanEngine(eScanType scanType);
        void PauseScan();
        void Dispose();
        void LogOff();
        bool ReLogin();

        PrepareBetDTO PrepareBetBroker(MatchOddDTO matchOdd, eBetType betType, float oddVerify, OddDTO oddDto = null);
        bool ConfirmBetBroker(int stake);

        string GetBetList();
        string GetStatement(DateTime date);
        float UpdateAvailabeCredit();
    }
}
