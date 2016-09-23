using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.FaultDTO;
using BcWin.Common.Objects;
using BcWin.Processor.Helper;
using BcWin.Processor.Interface;
using log4net;

namespace BcWin.Processor
{
    public class IbetSboProcessorTest //: IProcessor
    {
        public int CountTest { get; set; }
        public DateTime LastBetTime { get; set; }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSboProcessorTest));

        //private static Semaphore semaphore = new Semaphore(3, 5);

        public double CompareValueDifferent { get; set; }

        public Dictionary<Guid, AccountDTO> AccountDic { get; set; }

        public IbetEngine IbetEngine { get; set; }
        public SboEngine SboEngine { get; set; }

        //public SboEngine SboEngine { get; set; }

        public IbetSboProcessorTest()
        {
            AccountDic = new Dictionary<Guid, AccountDTO>();
        }

        public void Initialize()
        {
            CountTest = 1;
            CompareValueDifferent = 2;
            LastBetTime = DateTime.Now;
            //IbetEngine.UpdateLiveDataChange += ibetUpdateChange_Event;
            //IbetEngine.UpdateNonLiveDataChange += ibetUpdateChange_Event;
            //SboEngine.UpdateLiveDataChange += sbobetUpdateChange_Event;
            //SboEngine.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
        }

        public void Start()
        {
            if (IbetEngine.CheckLogin())
            {
                IbetEngine.InitEngine();
                IbetEngine.StartScanEngine();
            }
            else
            {
                StartServerFault startServerFault = new StartServerFault();
                startServerFault.ServerID = IbetEngine.Account.GuidID;
                startServerFault.Message = "Login Fail !";
                throw new FaultException<StartServerFault>(startServerFault);
            }

        }

        public void Dispose()
        {
            //IbetEngine.UpdateLiveDataChange -= ibetUpdateChange_Event;
            //IbetEngine.UpdateNonLiveDataChange -= ibetUpdateChange_Event;
           
            IbetEngine.Dispose();
        }
        
        public void PrepareBetTest(string oddId)
        {
            var match = IbetEngine.LiveMatchOddDatas.Where(l => l.OddID == oddId);
            var fmatch = match.FirstOrDefault();
            IbetEngine.PrepareBet(fmatch, eBetType.Home, true);
            IbetEngine.ConfirmBet(3);
        }
        //HUng: Test
        public void InitializeSbobet()
        {
            //objIbetEngine = new IbetEngine();
            //SboEngine.UpdateLiveDataChange += sbobetUpdateChange_Event;
            //SboEngine.UpdateNonLiveDataChange += sbobetUpdateChange_Event;
        }
        static Random rnd = new Random();
        private void ibetUpdateChange_Event(List<MatchOddDTO> updatedData, bool isLive)
        {
            //Logger.Debug("IbetSboProcessor => ibet change event : Live = " + isLive);
            if (updatedData != null && updatedData.Count > 0)
            {
                int r = rnd.Next(updatedData.Count);
                var data = updatedData[r];

                Task.Run(() =>
                {
                    Logger.InfoFormat(
                        "===============================GOOD COMPARE (Live: {0})===============================", isLive);
                    Logger.InfoFormat(
                           "Pick IBET: {0} - Home Name: {1} , Away Name: {2} , Leangue Name: {3} , Home Odd: {4}, Away Odd: {5}, Odd Type: {6}, Odd Value: {7}"
                           , eBetType.Home, data.HomeTeamName, data.AwayTeamName,
                           data.LeagueName, data.HomeOdd, data.AwayOdd, data.OddType, data.Odd);
                });
                var taskPrepareBetIbet = Task.Run(() => CallPrepareBet(data, eBetType.Home, isLive));
                if (taskPrepareBetIbet.Result != null &&
                        !taskPrepareBetIbet.Result.HasChangeOdd)
                {
                    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                }

                //if (isLive)
                //{
                //    //CompareOdd(updatedData, SboEngine.LiveMatchOddDatas, true);
                //    //var validData = CompareOdd(updatedData,IbetEngine.LiveMatchDatas,SboEngine.LiveMatchDatas);
                //}
                //else
                //{
                //    //CompareOdd(updatedData, SboEngine.NoneLiveMatchOddDatas, false);
                //    ////Compare Non Live
                //    //IbetEngine.NonLiveMatchDatas = UpdateIbetData(IbetEngine.NonLiveMatchDatas, updatedData);
                //}

                ////
            }
        }

        public void CompareOdd(List<MatchOddDTO> dataUpdated, List<MatchOddDTO> targetSource, bool isLive)
        {
            foreach (var data in dataUpdated)
            {
                if (!DataContainer.LeaguesDenyKeywords.Any(data.LeagueName.ToUpper().Contains))
                {
                    var matchTarget1 = targetSource.Where(m => 
                        (m.AwayTeamName == data.AwayTeamName && m.HomeTeamName == data.HomeTeamName) ||
                        (Utils.LevenshteinDistance(m.AwayTeamName, data.AwayTeamName) <= 3 
                        && Utils.LevenshteinDistance(m.HomeTeamName, data.HomeTeamName) <= 3)).ToList();

                    var matchTarget = targetSource.FirstOrDefault(m => (
                        (m.AwayTeamName == data.AwayTeamName && m.HomeTeamName == data.HomeTeamName) ||
                        (Utils.LevenshteinDistance(m.AwayTeamName, data.AwayTeamName) <= 3 && Utils.LevenshteinDistance(m.HomeTeamName, data.HomeTeamName) <= 3))
                        && !DataContainer.LeaguesDenyKeywords.Any(m.LeagueName.ToUpper().Contains)
                        && m.Odd == data.Odd && m.OddType == data.OddType);

                    if (matchTarget != null)
                    {
                        bool isValid1 = IsValidOddPair(data.HomeOdd, matchTarget.AwayOdd, CompareValueDifferent);
                        if (isValid1)
                        {
                            ProcessPrepareBet(data, eBetType.Home, matchTarget, eBetType.Away, isLive);
                            break;
                        }

                        bool isValid2 = IsValidOddPair(data.AwayOdd, matchTarget.HomeOdd, CompareValueDifferent);
                        if (isValid2)
                        {
                            ProcessPrepareBet(data, eBetType.Away, matchTarget, eBetType.Home, isLive);
                            break;
                        }
                    }
                }
            }

            //return validData;
        }

        public bool IsValidOddPair(float firstOdd, float secondOdd, double oddValueDifferent)
        {
            bool result = false;
            if ((firstOdd < 0f && secondOdd > 0f) || (firstOdd > 0f && secondOdd < 0f))
            {
                result = (Math.Abs((Math.Round((firstOdd + secondOdd), 2))) <= oddValueDifferent);
            }
            return result;
        }

        public void ProcessPrepareBet(MatchOddDTO ibetMatchOdd, eBetType ibetBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive)
        {
            Task.Run(() =>
            {
                Logger.InfoFormat(
                    "===============================GOOD COMPARE (Live: {0})===============================", isLive);
                Logger.InfoFormat(
                       "Pick IBET: {0} - Home Name: {1} , Away Name: {2} , Leangue Name: {3} , Home Odd: {4}, Away Odd: {5}, Odd Type: {6}, Odd Value: {7}"
                       , ibetBetType, ibetMatchOdd.HomeTeamName, ibetMatchOdd.AwayTeamName,
                       ibetMatchOdd.LeagueName, ibetMatchOdd.HomeOdd, ibetMatchOdd.AwayOdd, ibetMatchOdd.OddType, ibetMatchOdd.Odd);
                Logger.InfoFormat(
                    "Pick SBO: {0} - Home Name: {1} , Away Name: {2} , Leangue Name: {3} , Home Odd: {4}, Away Odd: {5}, Odd Type: {6}, Odd Value: {7}"
                    , sboBetType, sboMatchOdd.HomeTeamName, sboMatchOdd.AwayTeamName,
                    sboMatchOdd.LeagueName, sboMatchOdd.HomeOdd, sboMatchOdd.AwayOdd, sboMatchOdd.OddType,
                    sboMatchOdd.Odd);
            });

            if (CountTest <= 90000)
            {
                try
                {
                    var taskPrepareBetIbet = Task.Run(() => CallPrepareBet(ibetMatchOdd, ibetBetType, isLive));
                    var taskPrepareBetSbo = Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive));
                    //Logger.InfoFormat("Home type {0}, Max bet {1}, Min bet {2}",
                    //    taskPrepareBetHome.Result.MatchOdd.ServerType, taskPrepareBetHome.Result.MaxBet, taskPrepareBetHome.Result.MinBet);
                    //Logger.InfoFormat("Away type {0}, Away bet {1}, Away bet {2}",
                    //    taskPrepareBetAway.Result.MatchOdd.ServerType, taskPrepareBetAway.Result.MaxBet, taskPrepareBetAway.Result.MinBet);
                    if (taskPrepareBetIbet.Result != null && taskPrepareBetSbo.Result != null &&
                        !taskPrepareBetIbet.Result.HasChangeOdd && !taskPrepareBetSbo.Result.HasChangeOdd)
                    {
                        Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        //if (LastBetTime.AddSeconds(20) <= DateTime.Now)
                        //{
                        //    Task.Run(() => CallConfirmBet(taskPrepareBetIbet.Result));
                        //    Task.Run(() => CallConfirmBet(taskPrepareBetSbo.Result));
                        //    Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        //    LastBetTime = DateTime.Now;
                        //    CountTest++;
                        //}
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    throw;
                }
            }
        }

        private PrepareBetDTO CallPrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive)
        {
            switch (matchOdd.ServerType)
            {
                case eServerType.Ibet:
                    return IbetEngine.PrepareBet(matchOdd, betType, isLive);
                    break;
                case eServerType.Sbo:
                    return SboEngine.PrepareBet(matchOdd, betType, isLive);
                    break;
                default:
                    throw new Exception("CallPrepareBet => FAIL : Unknow matchOdd->eServerType param");
                    break;
            }
        }

        private void CallConfirmBet(PrepareBetDTO prepareBet)
        {
            switch (prepareBet.MatchOdd.ServerType)
            {
                case eServerType.Ibet:
                    IbetEngine.ConfirmBet(3);
                    break;
               
                default:
                    throw new Exception("CallConfirmBet => FAIL : Unknow prepareBet->MatchOdd->eServerType param");
                    break;
            }
        }
    }
}
