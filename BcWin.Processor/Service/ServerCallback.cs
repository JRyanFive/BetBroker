using System;
using System.Collections.Generic;
using BcWin.Common.DTO;
using BcWin.Common.EventDelegate;
using BcWin.Contract;

namespace BcWin.Processor.Service
{
    public class ServerCallback : IBcWinServiceCallback
    {
        public static event TransactionEvent OnTransactionStatic;

        //public event TransactionEvent OnTransaction;

        public static event ScanUpdateEvent OnSboScanUpdate;

        public static event ScanUpdateEvent OnIbetScanUpdate;

        public void ScanNotify(DateTime now, List<MatchOddDTO> matchOdds, bool isLive, eServerType serverType)
        {
            if (serverType == eServerType.Sbo)
            {
                if (OnSboScanUpdate != null)
                {
                    var invo = OnSboScanUpdate.GetInvocationList();
                    foreach (ScanUpdateEvent action in invo)
                    {
                        action.BeginInvoke(matchOdds, isLive, now, null, null);
                    }
                }
            }
            else
            {
                if (OnIbetScanUpdate != null)
                {
                    var invke = OnIbetScanUpdate.GetInvocationList();
                    foreach (ScanUpdateEvent action in invke)
                    {
                        action.BeginInvoke(matchOdds, isLive, now, null, null);
                    }
                }
            }
        }

        public void TransactionNotify1(DateTime now, List<TransactionDTO> transactions)
        {
            //if (OnTransaction != null)
            //{
            //    var invke = OnTransaction.GetInvocationList();
            //    foreach (TransactionEvent action in invke)
            //    {
            //        action.BeginInvoke(now, transactions, null, null);
            //    }
            //}
        }

        public void TransactionNotify(DateTime now, TransactionDTO transaction, string oddIdCheck, float oddCheck)
        {
            if (OnTransactionStatic != null)
            {
                var invke = OnTransactionStatic.GetInvocationList();
                foreach (TransactionEvent action in invke)
                {
                    action.BeginInvoke(now, transaction, oddIdCheck, oddCheck, null, null);
                }
            }
        }

        public int PingClient()
        {
            return 1;
        }
    }
}
