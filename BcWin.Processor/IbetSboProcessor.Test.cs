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
using BcWin.Core.EventDelegate;
using BcWin.Processor.Helper;
using BcWin.Processor.Interface;
using log4net;

namespace BcWin.Processor
{
    public partial class IbetSboProcessor : BaseProcessor, IProcessor
    {
        public void ProcessPrepareBet2(MatchOddDTO ibetMatchOdd, eBetType ibetBetType, MatchOddDTO sboMatchOdd, eBetType sboBetType, bool isLive)
        {
            try
            {
                if (MaxCountBet == 0 || CountBet <= MaxCountBet)
                {
                    var taskPrepareBetIbet = Task.Run(() => CallPrepareBet(ibetMatchOdd, ibetBetType, isLive));
                    var taskPrepareBetSbo = Task.Run(() => CallPrepareBet(sboMatchOdd, sboBetType, isLive));

                    if (taskPrepareBetIbet.Result != null && taskPrepareBetSbo.Result != null &&
                        !taskPrepareBetIbet.Result.HasChangeOdd && !taskPrepareBetSbo.Result.HasChangeOdd)
                    {
                        if (LastBetTime.AddSeconds(TimeOffStakeOdds) <= DateTime.Now)
                        {
                            int ibetStake;
                            int sboStake;

                            if (CaculateStake(BetStakeType,
                               taskPrepareBetIbet.Result.MatchOdd.MatchID, taskPrepareBetIbet.Result.MinBet, taskPrepareBetIbet.Result.MaxBet,
                              taskPrepareBetSbo.Result.MatchOdd.MatchID, taskPrepareBetSbo.Result.MinBet, taskPrepareBetSbo.Result.MaxBet,
                                out ibetStake, out sboStake))
                            {
                                if (CallConfirmBet(taskPrepareBetIbet.Result, ibetStake))
                                {
                                    if (CallConfirmBet(taskPrepareBetSbo.Result, sboStake))
                                    {
                                        Task.Run(
                                            () =>
                                                UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                                    taskPrepareBetSbo.Result.MatchOdd.MatchID, SboBetPool, sboStake));
                                        Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                                        LastBetTime = DateTime.Now;
                                        CountBet++;
                                    }

                                    Task.Run(
                                        () =>
                                            UpdateBetPool(taskPrepareBetIbet.Result.MatchOdd.ServerType,
                                                taskPrepareBetIbet.Result.MatchOdd.MatchID, IbetBetPool, ibetStake));
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
