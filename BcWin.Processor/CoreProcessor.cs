using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.CssStyle;
using BcWin.Processor.Properties;
using BCWin.Metadata;

namespace BcWin.Processor
{
    public class CoreProcessor
    {
        public static void InitConfig()
        {
            log4net.Config.XmlConfigurator.Configure();

            DataContainer.LoginIbetScript = Resources.LoginIbet;
            DataContainer.LeaguesDenyKeywords = System.IO.File.ReadAllLines(@"Config/LeaguesFilterDenyKeywords.txt");
            DataContainer.MatchsDenyKeywords = System.IO.File.ReadAllLines(@"Config/MatchsFilterDenyKeywords.txt");
            DataContainer.SboServers = System.IO.File.ReadAllLines(@"Config/SboServers.txt");
            DataContainer.IbetServers = System.IO.File.ReadAllLines(@"Config/IbetServers.txt");
            DataContainer.PiServers = System.IO.File.ReadAllLines(@"Config/PiServers.txt");
            DataContainer.IsnServers = System.IO.File.ReadAllLines(@"Config/IsnServers.txt");
            var leaguesSettings = System.IO.File.ReadAllLines(@"Config/LeaguesComparisionSettings.txt");
            DataContainer.LeaguesSettings = new List<LeaguesSetting>();
            for (int i = 0; i < leaguesSettings.Count(); i++)
            {
                var leagues = leaguesSettings[i].Split(new[] { "==" }, StringSplitOptions.None);
                DataContainer.LeaguesSettings.Add(new LeaguesSetting()
                {
                    IbetLeagueName = leagues[0],
                    SboLeagueName = leagues[1],
                    LeagueValue = i
                });
            }
        }

        public static void Init()
        {
            log4net.Config.XmlConfigurator.Configure();

            DataContainer.LoginIbetScript = Resources.LoginIbet;

            DataContainer.LeaguesDenyKeywords = System.IO.File.ReadAllLines(@"Config/LeaguesFilterDenyKeywords.txt");
            DataContainer.MatchsDenyKeywords = System.IO.File.ReadAllLines(@"Config/MatchsFilterDenyKeywords.txt");
           
            //DataContainer.SboServers = System.IO.File.ReadAllLines(@"Config/SboServers.txt");
            //DataContainer.IbetServers = System.IO.File.ReadAllLines(@"Config/IbetServers.txt");
            //SystemConfig.TIME_GET_UPDATE_LIVE_IBET = Convert.ToInt32(ConfigurationManager.AppSettings["TIME_GET_UPDATE_LIVE_IBET"]);
            //SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET = Convert.ToInt32(ConfigurationManager.AppSettings["TIME_GET_UPDATE_NON_LIVE_IBET"]);
            //SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET = Convert.ToInt32(ConfigurationManager.AppSettings["TIME_GET_UPDATE_LIVE_SBOBET"]);
            //SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET = Convert.ToInt32(ConfigurationManager.AppSettings["TIME_GET_UPDATE_NON_LIVE_SBOBET"]);
            //SystemConfig.IBET_LINK_1 = Convert.ToString(ConfigurationManager.AppSettings["IBET_LINK1"]);
            //SystemConfig.SBO_LINK_1 = Convert.ToString(ConfigurationManager.AppSettings["SBO_LINK1"]);
        }
    }
}
