using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Common.Objects;

namespace BcWin.Core
{
    public class DataContainer
    {
        public static bool IsDispose = false;

        public static BlockingCollection<MatchDTO> LiveMatchOddBag = new BlockingCollection<MatchDTO>();
        public static BlockingCollection<MatchDTO> NonLiveMatchOddBag = new BlockingCollection<MatchDTO>();

        public static string LoginIbetScript { get; set; }

        public static List<string> FirstAccounts = new List<string>();
        public static List<string> SecondAccounts = new List<string>();

        public static string[] LeaguesDenyKeywords { get; set; }
        public static string[] MatchsDenyKeywords { get; set; }
        public static string[] SboServers { get; set; }
        public static string[] IbetServers { get; set; }
        public static string[] PiServers { get; set; }
        public static string[] IsnServers { get; set; }

        public static string[] IbetSelectedLeagues { get; set; }
        public static string[] SboSelectedLeagues { get; set; }
        public static List<LeaguesSetting> LeaguesSettings { get; set; }


        public static ConcurrentQueue<LogBetMessage> LogBetQueue = new ConcurrentQueue<LogBetMessage>();
        public static ConcurrentQueue<LogScanMessage> LogScanQueue = new ConcurrentQueue<LogScanMessage>();

        public static AutoResetEvent LogBetResetEvent = new AutoResetEvent(false);
        public static AutoResetEvent LogScanResetEvent = new AutoResetEvent(false);

        public static ManualResetEvent TransactionEvent = new ManualResetEvent(false);

        public static List<BetAgainstTransaction> BetAgainstTransactions =
            new List<BetAgainstTransaction>();

        public static bool HasLocalScan { get; set; }

        public static IList<string> SboScanServers { get; set; }
        public static IList<ScanAccountInfoDTO> SboScanAccounts { get; set; }

        public static System.Media.SoundPlayer SuccessSound = new System.Media.SoundPlayer(@"sound\success.wav");
        public static System.Media.SoundPlayer FailSound = new System.Media.SoundPlayer(@"sound\fail.wav");
        public static System.Media.SoundPlayer AlarmSound = new System.Media.SoundPlayer(@"sound\alarm.wav");

        public static List<string> SourceIpFakes = new List<string>();
    }
}
