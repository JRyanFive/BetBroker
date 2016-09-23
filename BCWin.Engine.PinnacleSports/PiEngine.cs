using BcWin.Common.DTO;
using BcWin.Core.Helper;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using BcWin.Common.Objects;
using BcWin.Core;
using BCWin.Engine.PinnacleSports;
using log4net;
using log4net.Repository.Hierarchy;
using System.Security.Cryptography;
using System.ComponentModel;

namespace BcWin.Engine.PinnacleSports
{
    public class PiEngine : PiHelper, IDisposable
    {
        public int TabCode { get; set; }
        public string UserName { get; set; }
        public eAccountStatus AccountStatus { get; set; }
        public eServiceStatus Status { get; set; }
        public eServerType ServerType { get; set; }
        public string EngineName { get; set; }

        private static readonly ILog Logger = LogManager.GetLogger(typeof(PiEngine));
        private const string User_Agent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private const string Accept_Encoding = "gzip, deflate";
        private const string Accept_Language = "en-US,en;q=0.5";
        private CookieContainer _cookieContainer = new CookieContainer();
        private string _authorization;
        private int Rebet = 0;
        public PiData PiData;
        private System.Threading.Timer objLiveScanTimer;
        public double AvailabeCredit { get; set; }

        public Object LockLive = new Object();

        public void InitEngine()
        {
            GetFixtures();
        }

        public void StartScanEngine(eScanType scanType)
        {
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true, 0, 5000);
        }

        public void PauseScan()
        {
            if (objLiveScanTimer != null)
            {
                objLiveScanTimer.Dispose();
            }
        }

        public void Dispose()
        {
            PauseScan();
        }

        private void WaitScanCallback(object obj)
        {
            ScanLiveAPI();
        }

        #region for API

        private List<League> _fixtures = new List<League>();
        public List<MatchPiDTO> _matchs = new List<MatchPiDTO>();
        private long _sinceFixture = 0;
        private long _sinceOdd = 0;

        public bool CreateCredential(string userName, string password)
        {
            try
            {
                string credentials = String.Format("{0}:{1}", userName, password);
                byte[] bytes = Encoding.UTF8.GetBytes(credentials);
                string base64 = Convert.ToBase64String(bytes);
                _authorization = String.Concat("Basic ", base64);
                if (String.IsNullOrEmpty(_authorization))
                {
                    Logger.Info("CreateCredential: can not created _authorization");
                }

                if (GetFixtures())
                {
                    UserName = userName;
                    AccountStatus = eAccountStatus.Online;
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("CreateCredential: " + ex.Message);
            }

            AccountStatus = eAccountStatus.Offline;
            return false;
        }

        public bool GetFixtures()
        {
            try
            {
                var fixtureString = ScanFixture(_authorization, 29, 1, _sinceFixture);
                if (!string.IsNullOrEmpty(fixtureString) && !fixtureString.Contains("INVALID_REQUEST_DATA"))
                {
                    var fixtureResponse = JsonConvert.DeserializeObject<FixtureResponse>(fixtureString);
                    _sinceFixture = fixtureResponse.last;
                    //if (fixtureResponse != null)
                    //{

                    //    //fixtureResponse.league.ForEach(p => p.events.RemoveAll(e => (!e.starts.StartsWith(DateTime.Now.ToString("yyyy-MM-dd")) && !e.starts.StartsWith(DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd")))));
                    //    //fixtureResponse.league.RemoveAll(p => p.events == null || p.events.Count == 0);
                    //    if (_fixtures.Count == 0)
                    _fixtures = fixtureResponse.league;
                    return true;
                    //}
                }
                Logger.Info("GetFixtures error: " + fixtureString);
            }
            catch (Exception ex)
            {
                Logger.Error("GetFixtures: " + ex.Message);
            }
            return false;
        }

        public void ScanLiveAPI(long version = 0)
        {
            try
            {
                lock (LockLive)
                {
                    //sportid = 29 -> soccer
                    var data = ScanOdd(_authorization, 29, 1, version);

                    if (!String.IsNullOrEmpty(data))
                    {
                        OddResponse oddResponse = null;
                        try
                        {
                            oddResponse = JsonConvert.DeserializeObject<OddResponse>(data);

                        }
                        catch (Exception)
                        {
                            Logger.Error("ScanLiveAPI(ERROR PARSE): " + data);
                            return;
                        }

                        //if (_sinceOdd==0)
                        //{

                        //}

                        _sinceOdd = oddResponse.last;

                        //if (version==0)
                        //{
                        //    _matchs.Clear();
                        //}

                        foreach (var league in oddResponse.leagues)
                        {
                            foreach (var event_ in league.events)
                            {
                                var match = _matchs.FirstOrDefault(p => p.MatchID == event_.id.ToString());
                                #region insert

                                if (match == null)
                                {

                                    var league_fixture = _fixtures.FirstOrDefault(p => p.id == league.id);
                                    if (league_fixture == null)
                                    {
                                        _sinceFixture = 0;
                                        GetFixtures();
                                        league_fixture = _fixtures.FirstOrDefault(p => p.id == league.id);

                                        if (league_fixture == null)
                                            continue;
                                    }

                                    var event_fixture = league_fixture.events.FirstOrDefault(e => e.id == event_.id);

                                    if (event_fixture == null)
                                    {
                                        continue;
                                    }

                                    if (event_fixture.status.Equals("H"))
                                    {
                                        continue;
                                    }

                                    DateTime date = DateTime.Parse(event_fixture.starts, null, System.Globalization.DateTimeStyles.RoundtripKind);
                                    if (date.Date.AddDays(2) < DateTime.Today)
                                    {
                                        continue;
                                    }

                                    // only for today
                                    //if (date.Date.AddDays(-1) > DateTime.Today)
                                    //{
                                    //    continue;
                                    //}

                                    match = new MatchPiDTO
                                    {
                                        startDate = date,
                                        starts = event_fixture.starts,
                                        LeagueID = league.id.ToString(),
                                        //LeagueName = league.
                                        MatchID = event_.id.ToString(),
                                        HomeTeamName = CoreEngine.CleanTeamNameMore(event_fixture.home),
                                        AwayTeamName = CoreEngine.CleanTeamNameMore(event_fixture.away),
                                        Odds = new List<OddDTO>(),

                                    };

                                    foreach (var period in event_.periods)
                                    {
                                        //if (event_.id == 493426827)
                                        //{

                                        //}
                                        //add HDP
                                        if (period.spreads != null)
                                        {
                                            for (int i = 0; i < period.spreads.Count; i++)
                                            {

                                                match.Odds.Add(new OddDTO
                                                {
                                                    lineId = (int)period.lineId,
                                                    altLineId = period.spreads[i].altLineId,
                                                    HomeOdd = (float)period.spreads[i].home,
                                                    AwayOdd = (float)period.spreads[i].away,
                                                    Odd = (float)period.spreads[i].hdp,
                                                    number = (int)period.number,
                                                    max = period.maxSpread,
                                                    OddType = (int)period.number == 1 ? eOddType.HalfHCP : eOddType.HCP,
                                                });

                                            }
                                        }
                                        //add OU
                                        if (period.totals != null)
                                        {
                                            for (int i = 0; i < period.totals.Count; i++)
                                            {

                                                match.Odds.Add(new OddDTO
                                                {
                                                    lineId = (int)period.lineId,
                                                    altLineId = period.totals[i].altLineId,
                                                    HomeOdd = (float)period.totals[i].over,
                                                    AwayOdd = (float)period.totals[i].under,
                                                    Odd = (float)period.totals[i].points,
                                                    number = (int)period.number,
                                                    max = period.maxTotal,
                                                    OddType = (int)period.number == 1 ? eOddType.HalfOU : eOddType.OU,

                                                });

                                            }
                                        }

                                    }

                                    _matchs.Add(match);
                                }
                                #endregion
                                #region update
                                else
                                {
                                    // update
                                    match.Odds.Clear();

                                    var league_fixture = _fixtures.FirstOrDefault(p => p.id == league.id);
                                    if (league_fixture == null)
                                    {
                                        _sinceFixture = 0;
                                        GetFixtures();
                                        league_fixture = _fixtures.FirstOrDefault(p => p.id == league.id);

                                        if (league_fixture == null)
                                            continue;
                                    }

                                    var event_fixture = league_fixture.events.FirstOrDefault(e => e.id == event_.id);
                                    if (event_fixture.status.Equals("H") || match.IsDeleted == true)
                                    {
                                        continue;
                                    }

                                    foreach (var period in event_.periods)
                                    {
                                        //add HDP
                                        //if (event_.id == 493426827)
                                        //{

                                        //}
                                        if (period.spreads != null)
                                        {
                                            for (int i = 0; i < period.spreads.Count; i++)
                                            {

                                                match.Odds.Add(new OddDTO
                                                {
                                                    lineId = (int)period.lineId,
                                                    altLineId = period.spreads[i].altLineId,
                                                    HomeOdd = (float)period.spreads[i].home,
                                                    AwayOdd = (float)period.spreads[i].away,
                                                    Odd = (float)period.spreads[i].hdp,
                                                    number = (int)period.number,
                                                    max = period.maxSpread,
                                                    OddType = (int)period.number == 1 ? eOddType.HalfHCP : eOddType.HCP,
                                                });

                                            }
                                        }
                                        //add OU
                                        if (period.totals != null)
                                        {
                                            for (int i = 0; i < period.totals.Count; i++)
                                            {

                                                match.Odds.Add(new OddDTO
                                                {
                                                    lineId = (int)period.lineId,
                                                    altLineId = period.totals[i].altLineId,
                                                    HomeOdd = (float)period.totals[i].over,
                                                    AwayOdd = (float)period.totals[i].under,
                                                    Odd = (float)period.totals[i].points,
                                                    number = (int)period.number,
                                                    max = period.maxTotal,
                                                    OddType = (int)period.number == 1 ? eOddType.HalfOU : eOddType.OU,

                                                });

                                            }
                                        }

                                    }
                                }
                                #endregion
                            }
                        }

                    }


                    _matchs.RemoveAll(p => p.Odds.Count == 0);

                    //new
                    //Stopwatch a = new Stopwatch();
                    //a.Start();
                    //Shuffle<MatchPiDTO>(_matchs);
                    //a.Stop();
                    //Debug.WriteLine(a.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ScanLiveAPI: " + ex.InnerException.Message);
            }

            //var m = _matchs.FirstOrDefault(p => p.MatchID == 492892065.ToString());
            //if (m!=null)
            //{
            //var a = PrepareBet(_matchs[0], _matchs[0].Odds[0], "TEAM1", "SPREAD");
            //if (a != null)
            //{
            //    var b = ConfirmBet(a, _matchs[0], _matchs[0].Odds[0], "TEAM1", "SPREAD", true, (decimal)a.minRiskStake, "WIN");

            //}
            //}

        }



        /// <summary>
        /// Chỉ trả những odd cùng loại 
        /// </summary>
        /// <param name="match"></param>
        /// <param name="odd"></param>
        /// <returns></returns>
        public OddDTO GetNewestOddByMatch(MatchDTO match, OddDTO odd)
        {
            OddDTO odd2 = null;
            try
            {

                var data = ScanOdd(_authorization, 29, 1, _sinceOdd, match.LeagueID);

                if (!String.IsNullOrEmpty(data))
                {
                    OddResponse oddResponse = JsonConvert.DeserializeObject<OddResponse>(data);
                    if (oddResponse != null)
                    {
                        _sinceOdd = oddResponse.last;
                        var event_ = oddResponse.leagues[0].events.FirstOrDefault(p => p.id.ToString() == match.MatchID);
                        if (event_ != null && event_.periods != null)
                        {
                            // get hiệp
                            var period = event_.periods.FirstOrDefault(p => p.number == odd.number);

                            //get odd base on oddTYpe
                            if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
                            {
                                var oddTemp = period.spreads.FirstOrDefault(p => p.hdp == odd.Odd);

                                if (oddTemp.altLineId == null)
                                {
                                    odd2 = new OddDTO
                                    {
                                        lineId = (int)period.lineId,
                                        HomeOdd = (float)oddTemp.home,
                                        AwayOdd = (float)oddTemp.away,
                                        Odd = (float)oddTemp.hdp,
                                        number = (int)period.number,
                                        max = period.maxSpread,
                                        OddType = (int)period.number == 1 ? eOddType.HalfHCP : eOddType.HCP,
                                    };
                                }
                                else
                                {
                                    odd2 = new OddDTO
                                    {
                                        altLineId = (int)oddTemp.altLineId,
                                        HomeOdd = (float)oddTemp.home,
                                        AwayOdd = (float)oddTemp.away,
                                        Odd = (float)oddTemp.hdp,
                                        number = (int)period.number,
                                        max = period.maxSpread,
                                        OddType = (int)period.number == 1 ? eOddType.HalfHCP : eOddType.HCP,
                                    };
                                }
                            } // add OU
                            else if (period.totals != null)
                            {
                                var oddTemp = period.totals.FirstOrDefault(p => p.points == odd.Odd);

                                if (oddTemp.altLineId == null)
                                {
                                    odd2 = new OddDTO
                                    {
                                        lineId = (int)period.lineId,
                                        HomeOdd = (float)oddTemp.over,
                                        AwayOdd = (float)oddTemp.under,
                                        Odd = (float)oddTemp.points,
                                        number = (int)period.number,
                                        max = period.maxTotal,
                                        OddType = (int)period.number == 1 ? eOddType.HalfOU : eOddType.OU,
                                    };
                                }
                                else
                                {
                                    odd2 = new OddDTO
                                    {
                                        altLineId = (int)oddTemp.altLineId,
                                        HomeOdd = (float)oddTemp.over,
                                        AwayOdd = (float)oddTemp.under,
                                        Odd = (float)oddTemp.points,
                                        number = (int)period.number,
                                        max = period.maxTotal,
                                        OddType = (int)period.number == 1 ? eOddType.HalfOU : eOddType.OU,
                                    };
                                }

                            }

                        }
                    }
                }



                return odd2;
            }
            catch (Exception ex)
            {
                Logger.Error("GetNewestOddByMatch" + ex.Message);
                return null;
            }

        }

        public LineResponse PrepareBet(MatchPiDTO match, OddDTO odd, eBetType betType)
        {

            string winRiskStake = "";
            if (betType == eBetType.Home)
            {
                winRiskStake = odd.HomeOdd > 0 ? "RISK" : "WIN";
            }
            else
            {
                winRiskStake = odd.AwayOdd > 0 ? "RISK" : "WIN";

            }

            string team = "";
            string type = "";
            if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
            {
                team = betType == eBetType.Home ? "TEAM1" : "TEAM2";
                type = "SPREAD";
            }
            else
            {
                team = betType == eBetType.Home ? "OVER" : "UNDER";
                type = "TOTAL_POINTS";
            }

            var lineString = GetLine(_authorization, 29, match.LeagueID, match.MatchID, odd.number, team, type);
            var ob = JsonConvert.DeserializeObject<LineResponse>(lineString);
            if (ob != null && ob.status == "SUCCESS")
            {
                return ob;
            }
            return null;
        }

        public bool ConfirmBet(int stakeBet, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                //  Stopwatch a = new Stopwatch();
                //a.Start();
                //WIN	Stake is win amount
                //RISK	Stake is risk amount
                var json = "";
                string winRiskStake = "";
                if (betType == eBetType.Home)
                {
                    winRiskStake = odd.HomeOdd > 0 ? "RISK" : "WIN";
                }
                else
                {
                    winRiskStake = odd.AwayOdd > 0 ? "RISK" : "WIN";

                }

                if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest_alt
                        {
                            acceptBetterLine = "false",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            altLineId = odd.altLineId,
                            lineId = odd.lineId,
                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest
                        {
                            acceptBetterLine = "false",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                }
                else
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2_alt
                        {
                            acceptBetterLine = "false",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,
                            altLineId = odd.altLineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2
                        {
                            acceptBetterLine = "false",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }
                var responsePlaceBet = PlaceBet(_authorization, json);
                //  a.Stop();

                //Debug.WriteLine(a.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(responsePlaceBet))
                {
                    if (responsePlaceBet.Contains("OFFLINE_EVENT"))
                    {
                        // odd.AwayOdd = 0;
                        //odd.HomeOdd = 0;
                        match.IsDeleted = true;
                        //Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);

                        return false;
                    }

                    var ob = JsonConvert.DeserializeObject<PlaceBetResponse>(responsePlaceBet);
                    if (ob != null && (ob.status == "ACCEPTED" || ob.status == "PENDING_ACCEPTANCE"))
                    {
                        //  Logger.Info("PI CONFIRM SUCCESS: " + responsePlaceBet);
                        FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Success, eServerScan.Local);
                        return true;
                    }


                }
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                //  Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ConfirmBet: " + ex.Message);
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                return false;
            }

        }

        public bool ConfirmBet2(int stakeBet, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                //  Stopwatch a = new Stopwatch();
                //a.Start();
                //WIN	Stake is win amount
                //RISK	Stake is risk amount
                var json = "";
                string winRiskStake = "";
                if (betType == eBetType.Home)
                {
                    winRiskStake = odd.HomeOdd > 0 ? "RISK" : "WIN";
                }
                else
                {
                    winRiskStake = odd.AwayOdd > 0 ? "RISK" : "WIN";

                }

                #region initdata
                if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest_alt
                        {
                            acceptBetterLine = "false",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            altLineId = odd.altLineId,
                            lineId = odd.lineId,
                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest
                        {
                            acceptBetterLine = "false",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                }
                else
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2_alt
                        {
                            acceptBetterLine = "false",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,
                            altLineId = odd.altLineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2
                        {
                            acceptBetterLine = "false",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }
                #endregion
                var responsePlaceBet = PlaceBet(_authorization, json);
                //  a.Stop();

                //Debug.WriteLine(a.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(responsePlaceBet))
                {
                    if (responsePlaceBet.Contains("OFFLINE_EVENT"))
                    {
                        // odd.AwayOdd = 0;
                        //odd.HomeOdd = 0;
                        match.IsDeleted = true;
                        //Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);

                        return false;
                    }

                    var ob = JsonConvert.DeserializeObject<PlaceBetResponse>(responsePlaceBet);
                    if (ob != null && (ob.status == "ACCEPTED" || ob.status == "PENDING_ACCEPTANCE"))
                    {
                        //  Logger.Info("PI CONFIRM SUCCESS: " + responsePlaceBet);
                        FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Success, eServerScan.Local);
                        return true;
                    }
                    else
                    {
                        for (int i = 0; i < Rebet; i++)
                        {
                            var ok = TryConfirmBet2(stakeBet, match, odd, betType);
                            if (ok)
                            {
                                return true;
                            }
                        }
                        return false;
                    }

                }

                FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                //  Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ConfirmBet: " + ex.Message);
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                return false;
            }

        }

        public bool TryConfirmBet(int stakeBet, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                //Hàm này trả ra nhanh những odd có liên quan đến trận đấu. và nếu odd truyền vào type là Handicap thì list này toàn handicap. tương tự nếu oddtype là OU.
                // nếu anh muốn thì lọc thêm theo hiệp luôn.
                var odd2 = GetNewestOddByMatch(match, odd);
                if (odd2 == null)
                {
                    return false;
                }
                // chỗ này a get odd nào a muốn để bet ngược nha.
                // rồi assign cái biến odd ở param odd truyền ở trên = odd get được từ listOdd là xong.

                var json = "";
                string winRiskStake = "";
                if (betType == eBetType.Home)
                {
                    winRiskStake = odd.HomeOdd > 0 ? "RISK" : "WIN";
                }
                else
                {
                    winRiskStake = odd.AwayOdd > 0 ? "RISK" : "WIN";

                }

                if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest_alt
                        {
                            acceptBetterLine = "true",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            altLineId = odd.altLineId,
                            lineId = odd.lineId,
                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });


                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest
                        {
                            acceptBetterLine = "true",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }
                else
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2_alt
                        {
                            acceptBetterLine = "true",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,
                            altLineId = odd.altLineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });


                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2
                        {
                            acceptBetterLine = "true",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }

                var responsePlaceBet = PlaceBet(_authorization, json);
                if (!String.IsNullOrEmpty(responsePlaceBet))
                {
                    var ob = JsonConvert.DeserializeObject<PlaceBetResponse>(responsePlaceBet);
                    if (ob != null && (ob.status == "ACCEPTED" || ob.status == "PENDING_ACCEPTANCE"))
                    {
                        //Logger.Info("Bet Pina Success");
                        FireLogBet(match, odd2, betType, stakeBet, eBetStatusType.BetAgainstIbet, eServerScan.Local);
                        return true;
                    }
                }
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                //     Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ConfirmBet: " + ex.Message);
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                return false;
            }

        }

        public bool TryConfirmBet2(int stakeBet, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                //Hàm này trả ra nhanh những odd có liên quan đến trận đấu. và nếu odd truyền vào type là Handicap thì list này toàn handicap. tương tự nếu oddtype là OU.
                // nếu anh muốn thì lọc thêm theo hiệp luôn.
                var odd2 = GetNewestOddByMatch(match, odd);
                if (odd2 == null)
                {
                    return false;
                }
                // chỗ này a get odd nào a muốn để bet ngược nha.
                // rồi assign cái biến odd ở param odd truyền ở trên = odd get được từ listOdd là xong.

                var json = "";
                string winRiskStake = "";
                if (betType == eBetType.Home)
                {
                    winRiskStake = odd.HomeOdd > 0 ? "RISK" : "WIN";
                }
                else
                {
                    winRiskStake = odd.AwayOdd > 0 ? "RISK" : "WIN";

                }

                #region initdata
                if (odd.OddType == eOddType.HalfHCP || odd.OddType == eOddType.HCP)
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest_alt
                        {
                            acceptBetterLine = "true",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            altLineId = odd.altLineId,
                            lineId = odd.lineId,
                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });


                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest
                        {
                            acceptBetterLine = "true",
                            betType = "SPREAD",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            team = betType == eBetType.Home ? "TEAM1" : "TEAM2",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }
                else
                {
                    if (odd.altLineId != null && odd.altLineId != 0)
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2_alt
                        {
                            acceptBetterLine = "true",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,
                            altLineId = odd.altLineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });


                    }
                    else
                    {
                        json = JsonConvert.SerializeObject(new PlaceBetRequest2
                        {
                            acceptBetterLine = "true",
                            betType = "TOTAL_POINTS",
                            eventId = int.Parse(match.MatchID),
                            lineId = odd.lineId,

                            periodNumber = odd.number,
                            sportId = 29,
                            stake = stakeBet,
                            side = betType == eBetType.Home ? "OVER" : "UNDER",
                            uniqueRequestId = Guid.NewGuid().ToString(),
                            winRiskStake = winRiskStake,
                            oddsFormat = "MALAY",

                        });
                    }

                }
                #endregion
                var responsePlaceBet = PlaceBet(_authorization, json);
                if (!String.IsNullOrEmpty(responsePlaceBet))
                {
                    var ob = JsonConvert.DeserializeObject<PlaceBetResponse>(responsePlaceBet);
                    if (ob != null && (ob.status == "ACCEPTED" || ob.status == "PENDING_ACCEPTANCE"))
                    {
                        //Logger.Info("Bet Pina Success");
                        FireLogBet(match, odd2, betType, stakeBet, eBetStatusType.MissOddSbo, eServerScan.Local);
                        return true;
                    }
                }
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                //     Logger.Info("ConfirmBet not success with value returned: " + responsePlaceBet);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ConfirmBet: " + ex.Message);
                //FireLogBet(match, odd, betType, stakeBet, eBetStatusType.Fail, eServerScan.Local);
                return false;
            }

        }

        public double UpdateAvailabeCredit()
        {
            try
            {
                var balanceString = GetBalance(_authorization);
                if (!String.IsNullOrEmpty(balanceString))
                {
                    var ob = JsonConvert.DeserializeObject<BalanceResponse>(balanceString);
                    AvailabeCredit = (double)ob.availableBalance;
                }

                return AvailabeCredit;
            }
            catch (Exception ex)
            {
                Logger.Info("UpdateAvailabeCredit: " + ex.Message);
                return 0;
            }

        }

        public List<Bet> GetBetList()
        {
            try
            {
                var stringBetList = PiHelper.GetBetList(_authorization, DateTime.Now.AddDays(-28), DateTime.Now.AddDays(1), "RUNNING");
                if (!String.IsNullOrEmpty(stringBetList))
                {
                    var obj = JsonConvert.DeserializeObject<BetListResponse>(stringBetList);
                    return obj.bets;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Info("GetBetList: " + ex.Message);
                return null;
            }

        }
        public List<Bet> GetStatement(DateTime dt)
        {
            try
            {
                var stringBetList = PiHelper.GetBetList(_authorization, dt.AddDays(-1), dt);
                if (!String.IsNullOrEmpty(stringBetList))
                {
                    var obj = JsonConvert.DeserializeObject<BetListResponse>(stringBetList);
                    return obj.bets;
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Info("GetStatement: " + ex.Message);
                return null;
            }

        }
        #endregion

        public void FireLogBet(MatchPiDTO match, OddDTO piOdd, eBetType betType, int stake, eBetStatusType betStatus, eServerScan serverScan)
        {
            try
            {
                var msg = new LogBetMessage()
                {
                    Time = DateTime.Now.ToString("HH:mm:ss.fff"),
                    HomeTeamName = match.HomeTeamName,
                    AwayTeamName = match.AwayTeamName,
                    OddType = piOdd.OddType,
                    ServerType = match.ServerType,
                    OddValues = piOdd.HomeOdd + "|" + piOdd.AwayOdd,
                    Odd = piOdd.Odd,
                    BetStake = stake,
                    BetType = betType,
                    Status = betStatus,
                    EngineName = this.EngineName,
                    TabCode = TabCode,
                    ServerScan = serverScan
                };

                Task.Run(() =>
                {
                    DataContainer.LogBetQueue.Enqueue(msg);
                    DataContainer.LogBetResetEvent.Set();
                });
            }
            catch (Exception ex)
            {
                Logger.Error("FireLogBet: " + ex.Message);
            }

        }
    }

    public class ObjectData
    {
        public Sport Sport { get; set; }
    }


    public class Sport
    {
        public int SportId { get; set; }
        public string SportName { get; set; }
        public int? SortOrder { get; set; }
        public List<Market> Markets { get; set; }
        public int LineViewType { get; set; }
        public int CssViewType { get; set; }
        public bool IsParlay { get; set; }
        public object Sports { get; set; }
        public object Times { get; set; }

        public List<Period> Periods { get; set; }
        // everything else gets stored here

    }
    public class TmH
    {
        public string Fc { get; set; }
        public string Txt { get; set; }
        public string Pd { get; set; }
        public string Ex { get; set; }
        public int Sc { get; set; }
    }

    public class TmA
    {
        public string Fc { get; set; }
        public string Txt { get; set; }
        public string Pd { get; set; }
        public string Ex { get; set; }
        public int Sc { get; set; }
    }

    public class GameLine
    {
        public string Rot { get; set; }
        public string EvId { get; set; }
        public string Lvl { get; set; }
        public TmH TmH { get; set; }
        public TmA TmA { get; set; }
        public string Alt { get; set; }
        public string DispDate { get; set; }
        public string Date { get; set; }
        public string[] WagerMap { get; set; }
    }

    public class BuySellLevel
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected { get; set; }
        public object BaseText { get; set; }
    }

    public class Game
    {
        public string DisplayName { get; set; }
        public string PeriodDesc { get; set; }
        public List<GameLine> GameLines { get; set; }
        public List<string> UnchangedGames { get; set; }
        public List<BuySellLevel> BuySellLevels { get; set; }
        public bool IsVisible { get; set; }
        public int SportType { get; set; }
        public int LeagueId { get; set; }
        public string Name { get; set; }
        public int? SortOrder { get; set; }
        public object MaxWagerLimit { get; set; }
    }
    public class Market
    {
        [JsonExtensionData]
        public IDictionary<string, JToken> games;
    }
    public class Period
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public bool IsSelected { get; set; }
        public object BaseText { get; set; }
    }
    public class LinesRequest
    {
        public List<int> MarketTypes { get; set; }
        public List<int> WagerTypes { get; set; }
        public List<int> PeriodNumbers { get; set; }
        public bool IsParlay { get; set; }
        public int TimeZoneId { get; set; }
    }

    ////////////////////////////// response confirm////////////////////////////////
    public class Error
    {
        public string Message { get; set; }
        public int Type { get; set; }
    }

    public class Notification
    {
        public string Message { get; set; }
        public int Type { get; set; }
    }

    public class EventLine
    {
        public string FavoriteCss { get; set; }
        public string FormattedLine { get; set; }
        public string FormattedPick { get; set; }
        public string FormattedPrice { get; set; }
        public double? Line { get; set; }
        public double? Price { get; set; }
    }

    public class TicketItem
    {
        public int? BuySellId { get; set; }
        public int? BuySellLevel { get; set; }
        public bool DisplayPick { get; set; }
        public int? EventId { get; set; }
        public EventLine EventLine { get; set; }
        public int? EventStatus { get; set; }
        public bool InDangerZone { get; set; }
        public string League { get; set; }
        public int LeagueId { get; set; }
        public string LineDescription { get; set; }
        public int LineId { get; set; }
        public string LineTypeLabel { get; set; }
        public string LineTypeShortLabel { get; set; }
        public string MarketId { get; set; }
        public string MaxRisk { get; set; }
        public int MaxRiskAmount { get; set; }
        public string MinRisk { get; set; }
        public int MinRiskAmount { get; set; }
        public string MaxWin { get; set; }
        public double MaxWinAmount { get; set; }
        public string MinWin { get; set; }
        public double MinWinAmount { get; set; }
        public object MaxPayout { get; set; }
        public string OverUnder { get; set; }
        public int Period { get; set; }
        public string PeriodDescription { get; set; }
        public string PeriodShortDescription { get; set; }
        public int Pick { get; set; }
        public int RiskOrWin { get; set; }
        public string Sport { get; set; }
        public int SportId { get; set; }
        public int SportType { get; set; }
        public int StakeAmount { get; set; }
        public string StartDate { get; set; }
        public string Team1FavoriteCss { get; set; }
        public string Team1Id { get; set; }
        public string Team1Name { get; set; }
        public string Team1Pitcher { get; set; }
        public bool Team1PitcherChecked { get; set; }
        public int? Team1RedCards { get; set; }
        public int? Team1Score { get; set; }
        public string Team2FavoriteCss { get; set; }
        public string Team2Id { get; set; }
        public string Team2Name { get; set; }
        public string Team2Pitcher { get; set; }
        public bool Team2PitcherChecked { get; set; }
        public int? Team2RedCards { get; set; }
        public int? Team2Score { get; set; }
        public int BetType { get; set; }
        public object EventFilterType { get; set; }
        public object EventFilterValue { get; set; }
        public object BallPercent { get; set; }
        public bool ListedPitchersVisibility { get; set; }
        public int ListedPitchers { get; set; }
        public bool ShowListedPitcherChanged { get; set; }
        public DateTime FullDate { get; set; }
        public string ProgressionTime { get; set; }
    }

    public class ResponseConfirm
    {
        public bool BetMaximum { get; set; }
        public bool AcceptBetterLines { get; set; }
        public int BetTicketId { get; set; }
        public Error Error { get; set; }
        public string IpAddress { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsEmpty { get; set; }
        public Notification Notification { get; set; }
        public long PcTag { get; set; }
        public string RefreshLink { get; set; }
        public TicketItem TicketItem { get; set; }
        public int TicketSource { get; set; }
        public string UniqueId { get; set; }
    }


    public class PendingTicket
    {
        public bool BetMaximum { get; set; }
        public bool AcceptBetterLines { get; set; }
        public int BetTicketId { get; set; }
        public Error Error { get; set; }
        public string IpAddress { get; set; }
        public bool IsAccepted { get; set; }
        public bool IsEmpty { get; set; }
        public Notification Notification { get; set; }
        public int PcTag { get; set; }
        public string RefreshLink { get; set; }
        public TicketItem TicketItem { get; set; }
        public int TicketSource { get; set; }
        public string UniqueId { get; set; }
    }

    public class ResponsePrepare
    {
        public PendingTicket PendingTicket { get; set; }
        public string CurrencyCode { get; set; }
        public int ErrorType { get; set; }
        public string Error { get; set; }
        public int SportType { get; set; }
    }

    public class BalanceResponse
    {
        public double? availableBalance { get; set; }
        public double? outstandingTransactions { get; set; }
        public double? givenCredit { get; set; }
        public string currency { get; set; }
    }

    public class Bet
    {
        public int betId { get; set; }
        //public int wagerNumber { get; set; }
        public string placedAt { get; set; }
        public string betStatus { get; set; }
        public string betType { get; set; }
        public double win { get; set; }
        //public double risk { get; set; }
        //public double? winLoss { get; set; }
        //public string oddsFormat { get; set; }
        //public double customerCommission { get; set; }
        //public int sportId { get; set; }
        //public int leagueId { get; set; }
        //public int eventId { get; set; }
        public double handicap { get; set; }
        public double price { get; set; }
        [Browsable(false)]
        public string teamName { get; set; }
        [Browsable(false)]
        public string side { get; set; }

        public string TEAMNAME
        {
            get
            {
                if (!String.IsNullOrEmpty(this.teamName))
                {
                    return this.teamName;

                }
                return this.side;
            }
        }

        //public object pitcher1 { get; set; }
        //public object pitcher2 { get; set; }
        //public object pitcher1MustStart { get; set; }
        //public object pitcher2MustStart { get; set; }
        public string team1 { get; set; }
        public string team2 { get; set; }
        [Browsable(false)]
        public int periodNumber { get; set; }
        public string TIME
        {
            get
            {
                if (this.periodNumber == 0)
                {
                    return "Full Match";

                }
                return "1st Half";
            }
        }
        //public double? team1Score { get; set; }
        //public double? team2Score { get; set; }
        //public double ftTeam1Score { get; set; }
        //public double ftTeam2Score { get; set; }
        //public object pTeam1Score { get; set; }
        //public object pTeam2Score { get; set; }
        //public string isLive { get; set; }
    }

    public class BetListResponse
    {
        public List<Bet> bets { get; set; }
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////
    /// </summary>
    #region object odd api

    public class Spread
    {
        public double? hdp { get; set; }
        public double? home { get; set; }
        public double? away { get; set; }
        public int? altLineId { get; set; }
    }

    public class Moneyline
    {
        public double? home { get; set; }
        public double? away { get; set; }
        public double? draw { get; set; }
    }

    public class Total
    {
        public double? points { get; set; }
        public double? over { get; set; }
        public double? under { get; set; }
        public int? altLineId { get; set; }
    }

    public class Home
    {
        public double? points { get; set; }
        public double? over { get; set; }
        public double? under { get; set; }
    }

    public class Away
    {
        public double? points { get; set; }
        public double? over { get; set; }
        public double? under { get; set; }
    }

    public class TeamTotal
    {
        public Home home { get; set; }
        public Away away { get; set; }
    }

    public class PeriodAPI
    {
        public int? lineId { get; set; }
        public int? number { get; set; }
        public string cutoff { get; set; }
        public double? maxSpread { get; set; }
        public double? maxMoneyline { get; set; }
        public double? maxTotal { get; set; }
        public double? maxTeamTotal { get; set; }
        public List<Spread> spreads { get; set; }
        public Moneyline moneyline { get; set; }
        public List<Total> totals { get; set; }
        public TeamTotal teamTotal { get; set; }
    }

    public class Event
    {
        /// <summary>
        /// using for odd api
        /// </summary>
        public int? id { get; set; }
        public double? awayScore { get; set; }
        public double? homeScore { get; set; }
        public double? awayRedCards { get; set; }
        public double? homeRedCards { get; set; }
        public List<PeriodAPI> periods { get; set; }

        /// <summary>
        /// using for fixture api
        /// </summary>
        //    public int id { get; set; }
        public string starts { get; set; }
        public string home { get; set; }
        public string away { get; set; }
        public string rotNum { get; set; }
        public int? liveStatus { get; set; }
        public string status { get; set; }
        public int? parlayRestriction { get; set; }
    }

    public class League
    {
        public int id { get; set; }
        public List<Event> events { get; set; }
    }

    public class OddResponse
    {
        public int sportId { get; set; }
        public int last { get; set; }
        public List<League> leagues { get; set; }
    }

    public class FixtureResponse
    {
        public string code { get; set; }
        public int sportId { get; set; }
        public int last { get; set; }
        public List<League> league { get; set; }
    }

    public class LineResponse
    {
        public string status { get; set; }
        public object price { get; set; }
        public object lineId { get; set; }
        public object altLineId { get; set; }
        public object team1Score { get; set; }
        public object team2Score { get; set; }
        public object team1RedCards { get; set; }
        public object team2RedCards { get; set; }
        public decimal? maxRiskStake { get; set; }
        public decimal? minRiskStake { get; set; }
        public decimal? maxWinStake { get; set; }
        public decimal? minWinStake { get; set; }
        // public  effectiveAsOf { get; set; }
    }

    public class PlaceBetResponse
    {
        public string status { get; set; }
        public string errorCode { get; set; }
        public int? betId { get; set; }
        public string uniqueRequestId { get; set; }
        public bool betterLineWasAccepted { get; set; }
    }

    public class PlaceBetRequest
    {
        public string uniqueRequestId { get; set; }
        public string acceptBetterLine { get; set; }
        public decimal stake { get; set; }
        public string winRiskStake { get; set; }
        public int lineId { get; set; }
        public int sportId { get; set; }
        public int eventId { get; set; }
        public int periodNumber { get; set; }
        public string betType { get; set; }
        public string team { get; set; }
        public string oddsFormat { get; set; }
    }

    public class PlaceBetRequest_alt
    {
        public string uniqueRequestId { get; set; }
        public string acceptBetterLine { get; set; }
        public decimal stake { get; set; }
        public string winRiskStake { get; set; }
        public int lineId { get; set; }
        public int? altLineId { get; set; }
        public int sportId { get; set; }
        public int eventId { get; set; }
        public int periodNumber { get; set; }
        public string betType { get; set; }
        public string team { get; set; }
        public string oddsFormat { get; set; }
    }

    public class PlaceBetRequest2
    {
        public string uniqueRequestId { get; set; }
        public string acceptBetterLine { get; set; }
        public decimal stake { get; set; }
        public string winRiskStake { get; set; }
        public int lineId { get; set; }
        public int sportId { get; set; }
        public int eventId { get; set; }
        public int periodNumber { get; set; }
        public string betType { get; set; }
        public string side { get; set; }
        public string oddsFormat { get; set; }
    }

    public class PlaceBetRequest2_alt
    {
        public string uniqueRequestId { get; set; }
        public string acceptBetterLine { get; set; }
        public decimal stake { get; set; }
        public string winRiskStake { get; set; }
        public int lineId { get; set; }
        public int? altLineId { get; set; }
        public int sportId { get; set; }
        public int eventId { get; set; }
        public int periodNumber { get; set; }
        public string betType { get; set; }
        public string side { get; set; }
        public string oddsFormat { get; set; }
    }
    #endregion


}
