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
using System.Security.Cryptography;
using BCWin.Engine.PinnacleSports2;
using System.Threading;
using System.ComponentModel;
using log4net;

namespace BcWin.Engine.PinnacleSports2
{
    public class PiEngine : PiHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PiEngine));
        public List<string> Params = new List<string>();
        public int Rebet { get; set; }
        public bool IsScanCompleted = false;

        public string Referal = "https://members.pinnaclesports.com/Sportsbook/Asia/en-GB/Bet/Soccer/Live/Simple/null/null/Asian/SportsbookAsia/Malay/77/";
        public string Param = "{}";
        private const string ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
        private const string User_Agent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private const string Accept_Encoding = "gzip, deflate";
        private const string Accept_Language = "en-US,en;q=0.5";
        private string _authorization;
        private System.Threading.Timer objLiveScanTimer;
        public List<MatchPiDTO> _matchs = new List<MatchPiDTO>();

        public void InitEngine()
        {
            ServerType = eServerType.Isn;
            Status = eServiceStatus.Initialized;
        }

        public void StartScanEngine(eScanType scanType)
        {
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true, 7000, 7000);
            //WaitScanCallback(null); // for debug
            Logger.Info("STARTED: " + UrlHost);
            Status = eServiceStatus.Started;
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

        public void LogOff()
        {

            if (objLiveScanTimer != null)
            {
                objLiveScanTimer.Dispose();
            }

            lock (LockLive)
            {
                _matchs.Clear();
            }
            //lock (LockNonLive)
            //{
            //    NoneLiveMatchOddDatas = new List<MatchOddDTO>();
            //}

            this.RemoveCookie();
            UrlHost = string.Empty;
            Host = string.Empty;
            AvailabeCredit = 0;
            CashBalance = 0;
            Status = eServiceStatus.Unknown;
            AccountStatus = eAccountStatus.Offline;
            FireLogOffEvent();
            ExceptionCount = 0;
            this.Dispose();
            //UpdateException(this);
        }

        public void Logout()
        {
            try
            {
                string url = "https://members.pinnaclesports.com/Sportsbook/User/Logout";

                var re = SendPinna(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal);
                AccountStatus = eAccountStatus.Offline;

            }
            catch (Exception)
            {
                AccountStatus = eAccountStatus.Offline;

            }

        }

        private void WaitScanCallback(object obj)
        {
            ScanLive();
        }

        public bool Login(string url, string userName, string password)
        {
            try
            {
                LoginUrl = "https://www.pinnaclesports.com";
                CookieContainer = new System.Net.CookieContainer();
                Host = "www.pinnaclesports.com";
                string Referal = "https://www.pinnaclesports.com/en/";

                var aaa = Send302Https(url, "GET", User_Agent, null, null, Referal, Host, Accept);

                string param = "fakeusernameremembered={0}&fakepasswordremembered={1}&CustomerId={2}&Password={3}&AppId=Classic";
                param = String.Format(param, "", "", userName, password);
                url += "/login/authenticate/Classic/en-GB";

                CookieContainer = BindCookieContainer(new Uri(url), aaa.SetCookie);

                var re = Send302Https(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param), Referal, Host, Accept, ContentType);

                if (re != null && re.Result != null && re.Result.Contains("DesktopLogin"))
                {
                    UserName = userName;
                    Password = password;

                    string credentials = String.Format("{0}:{1}", userName, password);
                    byte[] bytes = Encoding.UTF8.GetBytes(credentials);
                    string base64 = Convert.ToBase64String(bytes);
                    _authorization = String.Concat("Basic ", base64);

                    Host = "members.pinnaclesports.com";

                    re = Send302Https("https://members.pinnaclesports.com/Sportsbook/Asia/DesktopLogin", "GET", User_Agent, CookieContainer, null, Referal + "rtn", Host, Accept, ContentType);
                    if (re.StatusCode == HttpStatusCode.ServiceUnavailable)
                    {
                        AccountStatus = eAccountStatus.Offline;
                        return false;
                    }


                    var data = SendPinna("https://members.pinnaclesports.com/Sportsbook/Asia/en-US/Bet/Soccer/Today/Single/null/null/Asian/SportsbookAsia/Malay/77", "GET", User_Agent, CookieContainer, null, Host, Referal + "rtn", Accept, ContentType);
                    SelectLines();

                    FisrtScanLive();
                    Thread.Sleep(2000);
                    // recall to get more;
                    FisrtScanLive();


                    AccountStatus = eAccountStatus.Online;
                    return true;

                }
                AccountStatus = eAccountStatus.Offline;

                return false;
            }
            catch (Exception)
            {
                AccountStatus = eAccountStatus.Offline;

                return false;
            }
        }

        public bool TryLogin()
        {
            try
            {
                for (int i = 0; i < 4; i++)
                {
                    Logger.Info("Loop " + i + " Begin TryLogin : " + UserName);

                    if (AccountStatus == eAccountStatus.Online)
                    {
                        return true;
                    }

            
                    if (Login(LoginUrl, UserName, Password))
                    {
                        return true;
                    }

                    Logger.Info("Loop " + i + " End TryLogin : " + UserName);
                    Thread.Sleep(15000);


                }
                AccountStatus = eAccountStatus.Offline;
                return false;

            }
            catch (Exception)
            {
             
                AccountStatus = eAccountStatus.Offline;
                return false;
            }
        }

        public void UpdateOdd(Game game)
        {
            foreach (var gameLine in game.GameLines)
            {
                //handle Match
                //if (game.UnchangedGames.Any(p=>p== gameLine.EvId))
                //{
                //    continue;
                //}

                var match = _matchs.FirstOrDefault(p => p.MatchID == gameLine.EvId);
                if (match == null)
                {
                    match = new MatchPiDTO
                    {
                        MatchID = gameLine.EvId,
                        HomeTeamName = CleanTeamName(gameLine.TmH.Txt),
                        AwayTeamName = CleanTeamName(gameLine.TmA.Txt),
                        //    LeagueName = game.DisplayName,
                        LeagueID = game.LeagueId,
                        Odds = new List<OddDTO>()
                    };

                    //PiData.LiveMatchOddBag.Add(match);
                    _matchs.Add(match);
                }

                // hander WagerMap of gameLine
                if (!String.IsNullOrEmpty(gameLine.WagerMap[0]) && gameLine.WagerMap[0].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[1]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[2]);
                    var href2 = aNode2.Attributes["href"].Value;

                    var odd = match.Odds.FirstOrDefault(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HCP);
                    if (odd == null)
                    {
                        match.Odds.Add(new OddDTO
                        {
                            // evId lvl and Rot is primary key
                            //EvId = gameLine.EvId,
                            Lvl = gameLine.Lvl,
                            Rot = gameLine.Rot,
                            prepareLinkHome = href1,
                            prepareLinkAway = href2,
                            HomeOdd = float.Parse(aNode1.InnerText),
                            AwayOdd = float.Parse(aNode2.InnerText),
                            OddType = eOddType.HCP,
                            Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]) // thang nao chap th do am
                        });
                    }
                    else
                    {
                        odd.prepareLinkHome = href1;
                        odd.prepareLinkAway = href2;
                        odd.HomeOdd = float.Parse(aNode1.InnerText);
                        odd.AwayOdd = float.Parse(aNode2.InnerText);
                        odd.Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]);// thang nao chap th do am

                    }

                }
                else
                {
                    match.Odds.RemoveAll(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HCP);

                }

                if (!String.IsNullOrEmpty(gameLine.WagerMap[3]) && gameLine.WagerMap[3].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[4]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[5]);
                    var href2 = aNode2.Attributes["href"].Value;

                    var odd = match.Odds.FirstOrDefault(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.OU);
                    if (odd == null)
                    {
                        match.Odds.Add(new OddDTO
                        {
                            // evId lvl and Rot is primary key
                            // EvId = gameLine.EvId,
                            Lvl = gameLine.Lvl,
                            Rot = gameLine.Rot,
                            prepareLinkHome = href1,
                            prepareLinkAway = href2,
                            HomeOdd = float.Parse(aNode1.InnerText),
                            AwayOdd = float.Parse(aNode2.InnerText),
                            OddType = eOddType.OU,
                            Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]) // thang nao chap th do am
                        });
                    }
                    else
                    {
                        odd.prepareLinkHome = href1;
                        odd.prepareLinkAway = href2;
                        odd.HomeOdd = float.Parse(aNode1.InnerText);
                        odd.AwayOdd = float.Parse(aNode2.InnerText);
                        odd.Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]);// thang nao chap th do am

                    }

                }
                else
                {
                    match.Odds.RemoveAll(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.OU);

                }

                if (!String.IsNullOrEmpty(gameLine.WagerMap[6]) && gameLine.WagerMap[6].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[7]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[8]);
                    var href2 = aNode2.Attributes["href"].Value;

                    var odd = match.Odds.FirstOrDefault(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HalfHCP);
                    if (odd == null)
                    {
                        match.Odds.Add(new OddDTO
                        {
                            // evId lvl and Rot is primary key
                            //   EvId = gameLine.EvId,
                            Lvl = gameLine.Lvl,
                            Rot = gameLine.Rot,
                            prepareLinkHome = href1,
                            prepareLinkAway = href2,
                            HomeOdd = float.Parse(aNode1.InnerText),
                            AwayOdd = float.Parse(aNode2.InnerText),
                            OddType = eOddType.HalfHCP,
                            Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]) // thang nao chap th do am
                        });
                    }
                    else
                    {
                        odd.prepareLinkHome = href1;
                        odd.prepareLinkAway = href2;
                        odd.HomeOdd = float.Parse(aNode1.InnerText);
                        odd.AwayOdd = float.Parse(aNode2.InnerText);
                        odd.Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]);// thang nao chap th do am

                    }

                }
                else
                {
                    match.Odds.RemoveAll(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HalfHCP);

                }

                if (!String.IsNullOrEmpty(gameLine.WagerMap[9]) && gameLine.WagerMap[9].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[10]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[11]);
                    var href2 = aNode2.Attributes["href"].Value;

                    var odd = match.Odds.FirstOrDefault(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HalfOU);
                    if (odd == null)
                    {
                        match.Odds.Add(new OddDTO
                        {
                            // evId lvl and Rot is primary key
                            //  EvId = gameLine.EvId,
                            Lvl = gameLine.Lvl,
                            Rot = gameLine.Rot,
                            prepareLinkHome = href1,
                            prepareLinkAway = href2,
                            HomeOdd = float.Parse(aNode1.InnerText),
                            AwayOdd = float.Parse(aNode2.InnerText),
                            OddType = eOddType.HalfOU,
                            Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]) // thang nao chap th do am
                        });
                    }
                    else
                    {
                        odd.prepareLinkHome = href1;
                        odd.prepareLinkAway = href2;
                        odd.HomeOdd = float.Parse(aNode1.InnerText);
                        odd.AwayOdd = float.Parse(aNode2.InnerText);
                        odd.Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]);// thang nao chap th do am

                    }
                }
                else
                {
                    match.Odds.RemoveAll(p => p.Lvl == gameLine.Lvl && p.Rot == gameLine.Rot && p.OddType == eOddType.HalfOU);

                }
            }



        }

        public void HandleMatchForFisrtScan(Game game)
        {

            foreach (var gameLine in game.GameLines)
            {
                //handle Match
                var match = _matchs.FirstOrDefault(p => p.MatchID == gameLine.EvId);
                if (match == null)
                {
                    match = new MatchPiDTO
                    {
                        MatchID = gameLine.EvId,
                        HomeTeamName = CleanTeamName(gameLine.TmH.Txt),
                        AwayTeamName = CleanTeamName(gameLine.TmA.Txt),
                        //LeagueName = game.DisplayName,
                        LeagueID = game.LeagueId,
                        Odds = new List<OddDTO>()
                    };

                    //PiData.LiveMatchOddBag.Add(match);
                    _matchs.Add(match);
                }

                // hander WagerMap of gameLine
                if (!String.IsNullOrEmpty(gameLine.WagerMap[0]) && gameLine.WagerMap[0].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[1]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[2]);
                    var href2 = aNode2.Attributes["href"].Value;
                    ///Sportsbook/Bet/Add/491910102/0/0/0/-1/-0.700?line=-0.25&t1s=0&t2s=0&t1rc=0&t2rc=0
                    match.Odds.Add(new OddDTO
                    {
                        // evId lvl and Rot is primary key
                        EvId = gameLine.EvId,
                        Lvl = gameLine.Lvl,
                        Rot = gameLine.Rot,
                        prepareLinkHome = href1,
                        prepareLinkAway = href2,
                        HomeOdd = float.Parse(aNode1.InnerText),
                        AwayOdd = float.Parse(aNode2.InnerText),
                        OddType = eOddType.HCP,
                        Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0]) // thang nao chap th do am
                    });
                }

                if (!String.IsNullOrEmpty(gameLine.WagerMap[3]) && gameLine.WagerMap[3].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[4]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[5]);
                    var href2 = aNode2.Attributes["href"].Value;

                    ///Sportsbook/Bet/Add/491910102/0/0/0/-1/-0.700?line=-0.25&t1s=0&t2s=0&t1rc=0&t2rc=0

                    match.Odds.Add(new OddDTO
                    {
                        EvId = gameLine.EvId,
                        Lvl = gameLine.Lvl,
                        Rot = gameLine.Rot,
                        prepareLinkHome = href1,
                        prepareLinkAway = href2,
                        HomeOdd = float.Parse(aNode1.InnerText),
                        AwayOdd = float.Parse(aNode2.InnerText),
                        OddType = eOddType.OU,
                        Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0])
                    });

                }

                if (!String.IsNullOrEmpty(gameLine.WagerMap[6]) && gameLine.WagerMap[6].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[7]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[8]);
                    var href2 = aNode2.Attributes["href"].Value;

                    ///Sportsbook/Bet/Add/491910102/0/0/0/-1/-0.700?line=-0.25&t1s=0&t2s=0&t1rc=0&t2rc=0

                    match.Odds.Add(new OddDTO
                    {
                        EvId = gameLine.EvId,
                        Lvl = gameLine.Lvl,
                        Rot = gameLine.Rot,
                        prepareLinkHome = href1,
                        prepareLinkAway = href2,
                        HomeOdd = float.Parse(aNode1.InnerText),
                        AwayOdd = float.Parse(aNode2.InnerText),
                        OddType = eOddType.HalfHCP,
                        Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0])
                    });

                }


                if (!String.IsNullOrEmpty(gameLine.WagerMap[9]) && gameLine.WagerMap[9].Length < 10)
                {
                    var aNode1 = HtmlNode.CreateNode(gameLine.WagerMap[10]);
                    var href1 = aNode1.Attributes["href"].Value;

                    var aNode2 = HtmlNode.CreateNode(gameLine.WagerMap[11]);
                    var href2 = aNode2.Attributes["href"].Value;

                    ///Sportsbook/Bet/Add/491910102/0/0/0/-1/-0.700?line=-0.25&t1s=0&t2s=0&t1rc=0&t2rc=0

                    match.Odds.Add(new OddDTO
                    {
                        EvId = gameLine.EvId,
                        Lvl = gameLine.Lvl,
                        Rot = gameLine.Rot,
                        prepareLinkHome = href1,
                        prepareLinkAway = href2,
                        HomeOdd = float.Parse(aNode1.InnerText),
                        AwayOdd = float.Parse(aNode2.InnerText),
                        OddType = eOddType.HalfOU,
                        Odd = float.Parse(href1.Split(new string[] { "line=" }, StringSplitOptions.None)[1].Split('&')[0])
                    });

                }
            }



        }

        public string GetParamAllLine()
        {
            string result = "";
            foreach (var item in Params)
            {
                result += "\"" + item + "\":" + "\"3\",";
            }

            if (result.Length > 0)
            {
                result = result.Remove(result.Length - 1);
            }

            return "{" + result + "}";
        }


    
        public void ScanLive()
        {
          //  Logger.Info("Pina Start scan at: " + DateTime.Now.ToString("hh:mm:ss"));
            string error = "";
            SendResponse re = new SendResponse();
            try
            {
                //  IsScanCompleted = false;
                //Stopwatch a = new Stopwatch();
                //a.Start();

                string url = "https://members.pinnaclesports.com/Sportsbook/Asia/GetUpdates/?marketType=Live&timeStamp={0}&selectedEventFilterId=295&selectedPeriodNumber=null&selectedTime=null";

                string param = "selectedLeagueIds={0}&buySellLevels={1}";
                param = String.Format(param, "{}", GetParamAllLine());
                url = String.Format(url, DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss"));
                re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param), Host, Accept, Referal, ContentType);
                if (re==null)
                {
                    Logger.Error("re null");  
                }
                error = re.Result;
                ObjectData data = JsonConvert.DeserializeObject<ObjectData>(re.Result);
                var games = data.Sport.Markets[0].games["GamesContainers"];

                //1 game la 1 league
                lock (LockLive)
                {
                    foreach (var item in games)
                    {
                        var game = JsonConvert.DeserializeObject(item.ElementAt(0).ToString(), typeof(Game)) as Game;

                        //if (game.GameLines == null || game.GameLines.Count == 0)
                        //{
                        //    continue;
                        //}

                        //if (!Params.Any(p => p == game.LeagueId))
                        //{
                        //    Params.Add(game.LeagueId);
                        //}

                        //handle match

                        UpdateOdd(game);

                    }

                }
                //_matchs.RemoveAll(p => p.Odds.Count == 0);
                //foreach (var item in _matchs)
                //{
                //    Debug.WriteLine(item.HomeTeamName + "-" + item.AwayTeamName + " have " + item.Odds.Count);
                //}
                //a.Stop();
                //Debug.WriteLine(a.ElapsedMilliseconds);
            //    Logger.Info("Pina End scan at: " + DateTime.Now.ToString("hh:mm:ss"));

            }
            catch (Exception ex)
            {
                ExceptionCount++;
                Logger.Error("ScanLive: " + ex.Message);
                Logger.Error("Data return: " + error);

                if (!String.IsNullOrEmpty(re.Result) && re.Result.Contains("expired"))
                {
                    UpdateException(this, eExceptionType.LoginFail);
                    AccountStatus = eAccountStatus.Offline;
                }
                else if (ExceptionCount > 5)
                {
                    ExceptionCount = 0;
                    UpdateException(this, eExceptionType.LoginFail);
                    AccountStatus = eAccountStatus.Offline;
                }
                
            }
            //finally
            //{
            //    IsScanCompleted = true;
            //}
        }

        private bool FirstScan = true;
        public void FisrtScanLive()
        {
            SendResponse re = new SendResponse();
            try
            {

                lock (LockLive)
                {

                    string url = @"https://members.pinnaclesports.com/Sportsbook/Asia/en-US/GetLines/Soccer/Live/3/null/null/Asian/SportsbookAsia/Malay/77/false/?buySellLevels=" + GetParamAllLine();
                    re = SendPinna(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, ContentType);

                    ObjectData data = JsonConvert.DeserializeObject<ObjectData>(re.Result);
                    var games = data.Sport.Markets[0].games["GamesContainers"];

                    foreach (var item in games)
                    {
                        var game = JsonConvert.DeserializeObject(item.ElementAt(0).ToString(), typeof(Game)) as Game;

                        if (game.GameLines == null || game.GameLines.Count == 0)
                        {
                            continue;
                        }

                        if (!Params.Any(p => p == game.LeagueId))
                        {
                            Params.Add(game.LeagueId);
                        }

                        //handle match
                        if (!FirstScan)
                        {
                            HandleMatchForFisrtScan(game);

                        }

                    }
                    FirstScan = false;
                    _matchs.RemoveAll(p => p.Odds.Count == 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("ScanLive: " + ex.Message);
                if (!String.IsNullOrEmpty(re.Result) && re.Result.Contains("expired"))
                {
                    UpdateException(this, eExceptionType.LoginFail);
                    AccountStatus = eAccountStatus.Offline;
                }
            }
        }

        public bool SelectLines()
        {
            try
            {
                string url = "https://members.pinnaclesports.com/Sportsbook/AsiaSession/SelectLines";
                string param = "selectedEventFilterId=295&selectedPeriodNumber=&selectedTime=&selectedTimeForParlays=0&selectedLineViewType=Single&selectedLeagueIds={}";
                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param), Host, Accept, Referal, ContentType);
                if (re.Result.Equals("done"))
                {
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public PrepareBetDTO PrepareBet(MatchPiDTO match, OddDTO odd, eBetType betType, float ibetOddDef)
        {
            try
            {
                //  Stopwatch st = new Stopwatch();
                //  st.Start();
                string url = "https://members.pinnaclesports.com" + (betType == eBetType.Home ? odd.prepareLinkHome : odd.prepareLinkAway);

                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes("test=test"), Host, Accept, Referal, ContentType);
                if (!String.IsNullOrEmpty(re.Result))
                {
                    var a = JsonConvert.DeserializeObject<ResponsePrepare>(re.Result);
                    if (a.PendingTicket != null)
                    {
                        float oddReturne = Math.Abs(float.Parse(a.PendingTicket.TicketItem.EventLine.FormattedLine));
                        if (Math.Abs(ibetOddDef) != oddReturne)
                        {
                            return null;
                        }

                        if (Math.Abs(odd.Odd) == oddReturne)
                        {

                            PrepareBetDTO result = new PrepareBetDTO();
                            result.PendingTicket = a.PendingTicket;
                            result.NewOdd = a.PendingTicket.TicketItem.EventLine.Price.Value;
                            if (betType == eBetType.Home)
                            {
                                odd.HomeOdd = result.NewOdd;
                                odd.prepareLinkHome = a.PendingTicket.RefreshLink;
                            }
                            else
                            {
                                odd.AwayOdd = result.NewOdd;
                                odd.prepareLinkAway = a.PendingTicket.RefreshLink;
                            }
                            //  st.Stop();
                            //  Debug.WriteLine(st.ElapsedMilliseconds);
                            return result;

                        }
                        else
                        {
                            if (betType == eBetType.Home)
                            {
                                odd.HomeOdd = a.PendingTicket.TicketItem.EventLine.Price.Value;
                                odd.prepareLinkHome = a.PendingTicket.RefreshLink;
                            }
                            else
                            {
                                odd.AwayOdd = a.PendingTicket.TicketItem.EventLine.Price.Value;
                                odd.prepareLinkAway = a.PendingTicket.RefreshLink;
                            }

                            odd.Odd = float.Parse(a.PendingTicket.TicketItem.EventLine.FormattedLine);
                        }

                    }
                }


                return null;
            }
            catch (Exception)
            {

                return null;

            }


        }

        public bool ConfirmBet(PendingTicket pTicket, int stake, MatchPiDTO match, OddDTO odd, eBetType betType, eBetStatusType logType, bool isAcceptBetter = false)
        {
            try
            {
                //Stopwatch st = new Stopwatch();
              //  st.Start();
                string url = "https://members.pinnaclesports.com/Sportsbook/BetTicket/PlaceBet";

                var param = new StringBuilder("PendingTicket.TicketSource=" + pTicket.TicketSource);
                param.Append("&PendingTicket.UniqueId=").Append(pTicket.UniqueId)
                .Append("&PendingTicket.TicketItem.BallPercent=").Append(pTicket.TicketItem.BallPercent)
                .Append("&PendingTicket.TicketItem.BetType=").Append(pTicket.TicketItem.BetType)
                .Append("&PendingTicket.TicketItem.BuySellLevel=").Append(pTicket.TicketItem.BuySellLevel)
                .Append("&PendingTicket.TicketItem.BuySellId=").Append(pTicket.TicketItem.BuySellId)
                .Append("&PendingTicket.TicketItem.DisplayPick=").Append(pTicket.TicketItem.DisplayPick)
                .Append("&PendingTicket.TicketItem.EventId=").Append(pTicket.TicketItem.EventId)
                .Append("&PendingTicket.TicketItem.EventFilterType=").Append(pTicket.TicketItem.EventFilterType)
                .Append("&PendingTicket.TicketItem.EventFilterValue=").Append(pTicket.TicketItem.EventFilterValue)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPick=").Append(pTicket.TicketItem.EventLine.FormattedPick)
                .Append("&PendingTicket.TicketItem.EventLine.Price=").Append(pTicket.TicketItem.EventLine.Price)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPrice=").Append(pTicket.TicketItem.EventLine.FormattedPrice)
                .Append("&PendingTicket.TicketItem.EventLine.Line=").Append(pTicket.TicketItem.EventLine.Line)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedLine=").Append(pTicket.TicketItem.EventLine.FormattedLine)
                .Append("&PendingTicket.TicketItem.EventStatus=").Append(pTicket.TicketItem.EventStatus)
                .Append("&PendingTicket.TicketItem.League=").Append(pTicket.TicketItem.League)
                .Append("&PendingTicket.TicketItem.LeagueId=").Append(pTicket.TicketItem.LeagueId)
                .Append("&PendingTicket.TicketItem.LineDescription=").Append(pTicket.TicketItem.LineDescription)
                .Append("&PendingTicket.TicketItem.LineId=").Append(pTicket.TicketItem.LineId)
                .Append("&PendingTicket.TicketItem.LineTypeLabel=").Append(pTicket.TicketItem.LineTypeLabel)
                .Append("&PendingTicket.TicketItem.MarketId=").Append(pTicket.TicketItem.MarketId)
                .Append("&PendingTicket.TicketItem.MaxRisk=").Append(pTicket.TicketItem.MaxRisk)
                .Append("&PendingTicket.TicketItem.MaxRiskAmount=").Append(pTicket.TicketItem.MaxRiskAmount)
                .Append("&PendingTicket.TicketItem.MinRisk=").Append(pTicket.TicketItem.MinRisk)
                .Append("&PendingTicket.TicketItem.MinRiskAmount=").Append(pTicket.TicketItem.MinRiskAmount)
                .Append("&PendingTicket.TicketItem.MaxWin=").Append(pTicket.TicketItem.MaxWin)
                .Append("&PendingTicket.TicketItem.MaxWinAmount=").Append(pTicket.TicketItem.MaxWinAmount)
                .Append("&PendingTicket.TicketItem.MinWin=").Append(pTicket.TicketItem.MinWin)
                .Append("&PendingTicket.TicketItem.MinWinAmount=").Append(pTicket.TicketItem.MinWinAmount)
                .Append("&PendingTicket.TicketItem.OverUnder=").Append(pTicket.TicketItem.OverUnder)
                .Append("&PendingTicket.TicketItem.Period=").Append(pTicket.TicketItem.Period)
                .Append("&PendingTicket.TicketItem.PeriodDescription=").Append(pTicket.TicketItem.PeriodDescription)
                .Append("&PendingTicket.TicketItem.PeriodShortDescription=").Append(pTicket.TicketItem.PeriodShortDescription)
                .Append("&PendingTicket.TicketItem.Pick=").Append(pTicket.TicketItem.Pick)
                .Append("&PendingTicket.TicketItem.Sport=").Append(pTicket.TicketItem.Sport)
                .Append("&PendingTicket.TicketItem.SportId=").Append(pTicket.TicketItem.SportId)
                .Append("&PendingTicket.TicketItem.SportType=").Append(pTicket.TicketItem.SportType)
                .Append("&PendingTicket.TicketItem.StartDate=").Append(pTicket.TicketItem.StartDate)
                .Append("&PendingTicket.TicketItem.Team1FavoriteCss=").Append(pTicket.TicketItem.Team1FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team1Id=").Append(pTicket.TicketItem.Team1Id)
                .Append("&PendingTicket.TicketItem.Team1Name=").Append(pTicket.TicketItem.Team1Name)
                .Append("&PendingTicket.TicketItem.Team1Pitcher=").Append(pTicket.TicketItem.Team1Pitcher)
                .Append("&PendingTicket.TicketItem.Team1RedCards=").Append(pTicket.TicketItem.Team1RedCards)
                .Append("&PendingTicket.TicketItem.Team1Score=").Append(pTicket.TicketItem.Team1Score)
                .Append("&PendingTicket.TicketItem.Team2FavoriteCss=").Append(pTicket.TicketItem.Team2FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team2Id=").Append(pTicket.TicketItem.Team2Id)
                .Append("&PendingTicket.TicketItem.Team2Name=").Append(pTicket.TicketItem.Team2Name)
                .Append("&PendingTicket.TicketItem.Team2Pitcher=").Append(pTicket.TicketItem.Team2Pitcher)
                .Append("&PendingTicket.TicketItem.Team2RedCards=").Append(pTicket.TicketItem.Team2RedCards)
                .Append("&PendingTicket.TicketItem.Team2Score=").Append(pTicket.TicketItem.Team2Score)
                .Append("&PendingTicket.TicketItem.Team1PitcherChecked=").Append(pTicket.TicketItem.Team1PitcherChecked)
                .Append("&PendingTicket.TicketItem.Team2PitcherChecked=").Append(pTicket.TicketItem.Team2PitcherChecked)
                .Append("&riskwin=").Append(pTicket.TicketItem.RiskOrWin == 0 ? 'r' : 'w')
                .Append("&PendingTicket.TicketItem.StakeAmount=").Append(stake)
                .Append("&PendingTicket.BetMaximum=").Append(pTicket.BetMaximum)
                .Append("&PendingTicket.AcceptBetterLines=").Append(isAcceptBetter)
                .Append("&InDangerZone=").Append(pTicket.TicketItem.InDangerZone)
                .Append("&betTicketLineType=").Append(pTicket.TicketItem.LineTypeLabel.Equals("Handicap") == true ? "spread" : "total")
                .Append("&ticketPlaced=true")
                .Append("&isRisk=").Append(pTicket.TicketItem.RiskOrWin == 0 ? "true" : "false")
                .Append("&isTestDrive=");

                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, ContentType);
              //  st.Stop();
              //  Debug.WriteLine(st.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(re.Result))
                {
                    var a = JsonConvert.DeserializeObject<ResponseConfirm>(re.Result);
                    // 3: success
                    // 10: zone dangerous
                    Logger.Info(re.Result);
                    if (a.Notification.Type == 3 || a.Notification.Type == 10 || a.Notification.Message.Contains("pending") || a.Notification.Message.Contains("waiting") )
                    {
                        FireLogBet(match, odd, betType, stake, logType, eServerScan.Local);

                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool ConfirmBet2(PendingTicket pTicket, int stake, MatchPiDTO match, OddDTO odd, eBetType betType, eBetStatusType logType, bool isAcceptBetter = false)
        {
            try
            {
                //Stopwatch st = new Stopwatch();
                //  st.Start();
                string url = "https://members.pinnaclesports.com/Sportsbook/BetTicket/PlaceBet";

                var param = new StringBuilder("PendingTicket.TicketSource=" + pTicket.TicketSource);
                param.Append("&PendingTicket.UniqueId=").Append(pTicket.UniqueId)
                .Append("&PendingTicket.TicketItem.BallPercent=").Append(pTicket.TicketItem.BallPercent)
                .Append("&PendingTicket.TicketItem.BetType=").Append(pTicket.TicketItem.BetType)
                .Append("&PendingTicket.TicketItem.BuySellLevel=").Append(pTicket.TicketItem.BuySellLevel)
                .Append("&PendingTicket.TicketItem.BuySellId=").Append(pTicket.TicketItem.BuySellId)
                .Append("&PendingTicket.TicketItem.DisplayPick=").Append(pTicket.TicketItem.DisplayPick)
                .Append("&PendingTicket.TicketItem.EventId=").Append(pTicket.TicketItem.EventId)
                .Append("&PendingTicket.TicketItem.EventFilterType=").Append(pTicket.TicketItem.EventFilterType)
                .Append("&PendingTicket.TicketItem.EventFilterValue=").Append(pTicket.TicketItem.EventFilterValue)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPick=").Append(pTicket.TicketItem.EventLine.FormattedPick)
                .Append("&PendingTicket.TicketItem.EventLine.Price=").Append(pTicket.TicketItem.EventLine.Price)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPrice=").Append(pTicket.TicketItem.EventLine.FormattedPrice)
                .Append("&PendingTicket.TicketItem.EventLine.Line=").Append(pTicket.TicketItem.EventLine.Line)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedLine=").Append(pTicket.TicketItem.EventLine.FormattedLine)
                .Append("&PendingTicket.TicketItem.EventStatus=").Append(pTicket.TicketItem.EventStatus)
                .Append("&PendingTicket.TicketItem.League=").Append(pTicket.TicketItem.League)
                .Append("&PendingTicket.TicketItem.LeagueId=").Append(pTicket.TicketItem.LeagueId)
                .Append("&PendingTicket.TicketItem.LineDescription=").Append(pTicket.TicketItem.LineDescription)
                .Append("&PendingTicket.TicketItem.LineId=").Append(pTicket.TicketItem.LineId)
                .Append("&PendingTicket.TicketItem.LineTypeLabel=").Append(pTicket.TicketItem.LineTypeLabel)
                .Append("&PendingTicket.TicketItem.MarketId=").Append(pTicket.TicketItem.MarketId)
                .Append("&PendingTicket.TicketItem.MaxRisk=").Append(pTicket.TicketItem.MaxRisk)
                .Append("&PendingTicket.TicketItem.MaxRiskAmount=").Append(pTicket.TicketItem.MaxRiskAmount)
                .Append("&PendingTicket.TicketItem.MinRisk=").Append(pTicket.TicketItem.MinRisk)
                .Append("&PendingTicket.TicketItem.MinRiskAmount=").Append(pTicket.TicketItem.MinRiskAmount)
                .Append("&PendingTicket.TicketItem.MaxWin=").Append(pTicket.TicketItem.MaxWin)
                .Append("&PendingTicket.TicketItem.MaxWinAmount=").Append(pTicket.TicketItem.MaxWinAmount)
                .Append("&PendingTicket.TicketItem.MinWin=").Append(pTicket.TicketItem.MinWin)
                .Append("&PendingTicket.TicketItem.MinWinAmount=").Append(pTicket.TicketItem.MinWinAmount)
                .Append("&PendingTicket.TicketItem.OverUnder=").Append(pTicket.TicketItem.OverUnder)
                .Append("&PendingTicket.TicketItem.Period=").Append(pTicket.TicketItem.Period)
                .Append("&PendingTicket.TicketItem.PeriodDescription=").Append(pTicket.TicketItem.PeriodDescription)
                .Append("&PendingTicket.TicketItem.PeriodShortDescription=").Append(pTicket.TicketItem.PeriodShortDescription)
                .Append("&PendingTicket.TicketItem.Pick=").Append(pTicket.TicketItem.Pick)
                .Append("&PendingTicket.TicketItem.Sport=").Append(pTicket.TicketItem.Sport)
                .Append("&PendingTicket.TicketItem.SportId=").Append(pTicket.TicketItem.SportId)
                .Append("&PendingTicket.TicketItem.SportType=").Append(pTicket.TicketItem.SportType)
                .Append("&PendingTicket.TicketItem.StartDate=").Append(pTicket.TicketItem.StartDate)
                .Append("&PendingTicket.TicketItem.Team1FavoriteCss=").Append(pTicket.TicketItem.Team1FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team1Id=").Append(pTicket.TicketItem.Team1Id)
                .Append("&PendingTicket.TicketItem.Team1Name=").Append(pTicket.TicketItem.Team1Name)
                .Append("&PendingTicket.TicketItem.Team1Pitcher=").Append(pTicket.TicketItem.Team1Pitcher)
                .Append("&PendingTicket.TicketItem.Team1RedCards=").Append(pTicket.TicketItem.Team1RedCards)
                .Append("&PendingTicket.TicketItem.Team1Score=").Append(pTicket.TicketItem.Team1Score)
                .Append("&PendingTicket.TicketItem.Team2FavoriteCss=").Append(pTicket.TicketItem.Team2FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team2Id=").Append(pTicket.TicketItem.Team2Id)
                .Append("&PendingTicket.TicketItem.Team2Name=").Append(pTicket.TicketItem.Team2Name)
                .Append("&PendingTicket.TicketItem.Team2Pitcher=").Append(pTicket.TicketItem.Team2Pitcher)
                .Append("&PendingTicket.TicketItem.Team2RedCards=").Append(pTicket.TicketItem.Team2RedCards)
                .Append("&PendingTicket.TicketItem.Team2Score=").Append(pTicket.TicketItem.Team2Score)
                .Append("&PendingTicket.TicketItem.Team1PitcherChecked=").Append(pTicket.TicketItem.Team1PitcherChecked)
                .Append("&PendingTicket.TicketItem.Team2PitcherChecked=").Append(pTicket.TicketItem.Team2PitcherChecked)
                .Append("&riskwin=").Append(pTicket.TicketItem.RiskOrWin == 0 ? 'r' : 'w')
                .Append("&PendingTicket.TicketItem.StakeAmount=").Append(stake)
                .Append("&PendingTicket.BetMaximum=").Append(pTicket.BetMaximum)
                .Append("&PendingTicket.AcceptBetterLines=").Append(isAcceptBetter)
                .Append("&InDangerZone=").Append(pTicket.TicketItem.InDangerZone)
                .Append("&betTicketLineType=").Append(pTicket.TicketItem.LineTypeLabel.Equals("Handicap") == true ? "spread" : "total")
                .Append("&ticketPlaced=true")
                .Append("&isRisk=").Append(pTicket.TicketItem.RiskOrWin == 0 ? "true" : "false")
                .Append("&isTestDrive=");

                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, ContentType);
                //  st.Stop();
                //  Debug.WriteLine(st.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(re.Result))
                {
                    var a = JsonConvert.DeserializeObject<ResponseConfirm>(re.Result);
                    // 3: success
                    // 10: zone dangerous
                    if (a.Notification.Type == 3 || a.Notification.Type == 10 || a.Notification.Message.Contains("pending"))
                    {
                        FireLogBet(match, odd, betType, stake, logType, eServerScan.Local);

                        return true;
                    }
                    else
                    {
                        Logger.Error(re.Result);
                        lock (LockLive)
                        {

                            float firstOdd = odd.Odd;
                            for (int i = 0; i < Rebet; i++)
                            {
                                //Logger.Error("Try Prepare ISN Before: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                                var prepareNew = PrepareBet(match, odd, betType, firstOdd);
                                //Logger.Error("Try Prepare ISN After: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                                if (prepareNew != null)
                                {
                                    var ok = TryConfirmBet2((PendingTicket )prepareNew.PendingTicket,stake, match, odd, betType);
                                    if (ok)
                                    {
                                        Logger.Info("Re Bet Pina  OK at " + i);
                                        return true;
                                    }
                                }

                                Thread.Sleep(2000);

                                var newMatch = _matchs.FirstOrDefault(p => p.MatchID == match.ID);
                                if (newMatch != null)
                                {
                                    var newOdd = newMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && odd.OddType == p.OddType);
                                    if (newOdd != null)
                                    {
                                        prepareNew = PrepareBet(newMatch, newOdd, betType, firstOdd);
                                        var ok = TryConfirmBet2((PendingTicket)prepareNew.PendingTicket,stake, newMatch, newOdd, betType);
                                        if (ok)
                                        {
                                            Logger.Info("Re Bet ISN  OK at " + i);
                                            return true;
                                        }
                                    }
                                }
                                Thread.Sleep(2000);
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        
        public bool TryConfirmBet(PendingTicket pTicket, int stake, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                string url = "https://members.pinnaclesports.com/Sportsbook/BetTicket/PlaceBet";

                var param = new StringBuilder("PendingTicket.TicketSource=" + pTicket.TicketSource);
                param.Append("&PendingTicket.UniqueId=").Append(pTicket.UniqueId)
                .Append("&PendingTicket.TicketItem.BallPercent=").Append(pTicket.TicketItem.BallPercent)
                .Append("&PendingTicket.TicketItem.BetType=").Append(pTicket.TicketItem.BetType)
                .Append("&PendingTicket.TicketItem.BuySellLevel=").Append(pTicket.TicketItem.BuySellLevel)
                .Append("&PendingTicket.TicketItem.BuySellId=").Append(pTicket.TicketItem.BuySellId)
                .Append("&PendingTicket.TicketItem.DisplayPick=").Append(pTicket.TicketItem.DisplayPick)
                .Append("&PendingTicket.TicketItem.EventId=").Append(pTicket.TicketItem.EventId)
                .Append("&PendingTicket.TicketItem.EventFilterType=").Append(pTicket.TicketItem.EventFilterType)
                .Append("&PendingTicket.TicketItem.EventFilterValue=").Append(pTicket.TicketItem.EventFilterValue)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPick=").Append(pTicket.TicketItem.EventLine.FormattedPick)
                .Append("&PendingTicket.TicketItem.EventLine.Price=").Append(pTicket.TicketItem.EventLine.Price)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPrice=").Append(pTicket.TicketItem.EventLine.FormattedPrice)
                .Append("&PendingTicket.TicketItem.EventLine.Line=").Append(pTicket.TicketItem.EventLine.Line)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedLine=").Append(pTicket.TicketItem.EventLine.FormattedLine)
                .Append("&PendingTicket.TicketItem.EventStatus=").Append(pTicket.TicketItem.EventStatus)
                .Append("&PendingTicket.TicketItem.League=").Append(pTicket.TicketItem.League)
                .Append("&PendingTicket.TicketItem.LeagueId=").Append(pTicket.TicketItem.LeagueId)
                .Append("&PendingTicket.TicketItem.LineDescription=").Append(pTicket.TicketItem.LineDescription)
                .Append("&PendingTicket.TicketItem.LineId=").Append(pTicket.TicketItem.LineId)
                .Append("&PendingTicket.TicketItem.LineTypeLabel=").Append(pTicket.TicketItem.LineTypeLabel)
                .Append("&PendingTicket.TicketItem.MarketId=").Append(pTicket.TicketItem.MarketId)
                .Append("&PendingTicket.TicketItem.MaxRisk=").Append(pTicket.TicketItem.MaxRisk)
                .Append("&PendingTicket.TicketItem.MaxRiskAmount=").Append(pTicket.TicketItem.MaxRiskAmount)
                .Append("&PendingTicket.TicketItem.MinRisk=").Append(pTicket.TicketItem.MinRisk)
                .Append("&PendingTicket.TicketItem.MinRiskAmount=").Append(pTicket.TicketItem.MinRiskAmount)
                .Append("&PendingTicket.TicketItem.MaxWin=").Append(pTicket.TicketItem.MaxWin)
                .Append("&PendingTicket.TicketItem.MaxWinAmount=").Append(pTicket.TicketItem.MaxWinAmount)
                .Append("&PendingTicket.TicketItem.MinWin=").Append(pTicket.TicketItem.MinWin)
                .Append("&PendingTicket.TicketItem.MinWinAmount=").Append(pTicket.TicketItem.MinWinAmount)
                .Append("&PendingTicket.TicketItem.OverUnder=").Append(pTicket.TicketItem.OverUnder)
                .Append("&PendingTicket.TicketItem.Period=").Append(pTicket.TicketItem.Period)
                .Append("&PendingTicket.TicketItem.PeriodDescription=").Append(pTicket.TicketItem.PeriodDescription)
                .Append("&PendingTicket.TicketItem.PeriodShortDescription=").Append(pTicket.TicketItem.PeriodShortDescription)
                .Append("&PendingTicket.TicketItem.Pick=").Append(pTicket.TicketItem.Pick)
                .Append("&PendingTicket.TicketItem.Sport=").Append(pTicket.TicketItem.Sport)
                .Append("&PendingTicket.TicketItem.SportId=").Append(pTicket.TicketItem.SportId)
                .Append("&PendingTicket.TicketItem.SportType=").Append(pTicket.TicketItem.SportType)
                .Append("&PendingTicket.TicketItem.StartDate=").Append(pTicket.TicketItem.StartDate)
                .Append("&PendingTicket.TicketItem.Team1FavoriteCss=").Append(pTicket.TicketItem.Team1FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team1Id=").Append(pTicket.TicketItem.Team1Id)
                .Append("&PendingTicket.TicketItem.Team1Name=").Append(pTicket.TicketItem.Team1Name)
                .Append("&PendingTicket.TicketItem.Team1Pitcher=").Append(pTicket.TicketItem.Team1Pitcher)
                .Append("&PendingTicket.TicketItem.Team1RedCards=").Append(pTicket.TicketItem.Team1RedCards)
                .Append("&PendingTicket.TicketItem.Team1Score=").Append(pTicket.TicketItem.Team1Score)
                .Append("&PendingTicket.TicketItem.Team2FavoriteCss=").Append(pTicket.TicketItem.Team2FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team2Id=").Append(pTicket.TicketItem.Team2Id)
                .Append("&PendingTicket.TicketItem.Team2Name=").Append(pTicket.TicketItem.Team2Name)
                .Append("&PendingTicket.TicketItem.Team2Pitcher=").Append(pTicket.TicketItem.Team2Pitcher)
                .Append("&PendingTicket.TicketItem.Team2RedCards=").Append(pTicket.TicketItem.Team2RedCards)
                .Append("&PendingTicket.TicketItem.Team2Score=").Append(pTicket.TicketItem.Team2Score)
                .Append("&PendingTicket.TicketItem.Team1PitcherChecked=").Append(pTicket.TicketItem.Team1PitcherChecked)
                .Append("&PendingTicket.TicketItem.Team2PitcherChecked=").Append(pTicket.TicketItem.Team2PitcherChecked)
                .Append("&riskwin=").Append(pTicket.TicketItem.RiskOrWin == 0 ? 'r' : 'w')
                .Append("&PendingTicket.TicketItem.StakeAmount=").Append(stake)
                .Append("&PendingTicket.BetMaximum=").Append(pTicket.BetMaximum)
                .Append("&PendingTicket.AcceptBetterLines=True")//.Append(pTicket.AcceptBetterLines)
                .Append("&InDangerZone=").Append(pTicket.TicketItem.InDangerZone)
                .Append("&betTicketLineType=").Append(pTicket.TicketItem.LineTypeLabel.Equals("Handicap") == true ? "spread" : "total")
                .Append("&ticketPlaced=true")
                .Append("&isRisk=").Append(pTicket.TicketItem.RiskOrWin == 0 ? "true" : "false")
                .Append("&isTestDrive=");

                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, ContentType);
               // st.Stop();
                //Debug.WriteLine(st.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(re.Result))
                {
                    var a = JsonConvert.DeserializeObject<ResponseConfirm>(re.Result);
                    // 3: success
                    // 10: zone dangerous
                    if (a.Notification.Type == 3 || a.Notification.Type == 10 || a.Notification.Message.Contains("pending"))
                    {
                        FireLogBet(match, odd, betType, stake, eBetStatusType.BetAgainPina, eServerScan.Local);

                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        public bool TryConfirmBet2(PendingTicket pTicket, int stake, MatchPiDTO match, OddDTO odd, eBetType betType)
        {
            try
            {
                Stopwatch st = new Stopwatch();
                st.Start();
                string url = "https://members.pinnaclesports.com/Sportsbook/BetTicket/PlaceBet";

                var param = new StringBuilder("PendingTicket.TicketSource=" + pTicket.TicketSource);
                param.Append("&PendingTicket.UniqueId=").Append(pTicket.UniqueId)
                .Append("&PendingTicket.TicketItem.BallPercent=").Append(pTicket.TicketItem.BallPercent)
                .Append("&PendingTicket.TicketItem.BetType=").Append(pTicket.TicketItem.BetType)
                .Append("&PendingTicket.TicketItem.BuySellLevel=").Append(pTicket.TicketItem.BuySellLevel)
                .Append("&PendingTicket.TicketItem.BuySellId=").Append(pTicket.TicketItem.BuySellId)
                .Append("&PendingTicket.TicketItem.DisplayPick=").Append(pTicket.TicketItem.DisplayPick)
                .Append("&PendingTicket.TicketItem.EventId=").Append(pTicket.TicketItem.EventId)
                .Append("&PendingTicket.TicketItem.EventFilterType=").Append(pTicket.TicketItem.EventFilterType)
                .Append("&PendingTicket.TicketItem.EventFilterValue=").Append(pTicket.TicketItem.EventFilterValue)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPick=").Append(pTicket.TicketItem.EventLine.FormattedPick)
                .Append("&PendingTicket.TicketItem.EventLine.Price=").Append(pTicket.TicketItem.EventLine.Price)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedPrice=").Append(pTicket.TicketItem.EventLine.FormattedPrice)
                .Append("&PendingTicket.TicketItem.EventLine.Line=").Append(pTicket.TicketItem.EventLine.Line)
                .Append("&PendingTicket.TicketItem.EventLine.FormattedLine=").Append(pTicket.TicketItem.EventLine.FormattedLine)
                .Append("&PendingTicket.TicketItem.EventStatus=").Append(pTicket.TicketItem.EventStatus)
                .Append("&PendingTicket.TicketItem.League=").Append(pTicket.TicketItem.League)
                .Append("&PendingTicket.TicketItem.LeagueId=").Append(pTicket.TicketItem.LeagueId)
                .Append("&PendingTicket.TicketItem.LineDescription=").Append(pTicket.TicketItem.LineDescription)
                .Append("&PendingTicket.TicketItem.LineId=").Append(pTicket.TicketItem.LineId)
                .Append("&PendingTicket.TicketItem.LineTypeLabel=").Append(pTicket.TicketItem.LineTypeLabel)
                .Append("&PendingTicket.TicketItem.MarketId=").Append(pTicket.TicketItem.MarketId)
                .Append("&PendingTicket.TicketItem.MaxRisk=").Append(pTicket.TicketItem.MaxRisk)
                .Append("&PendingTicket.TicketItem.MaxRiskAmount=").Append(pTicket.TicketItem.MaxRiskAmount)
                .Append("&PendingTicket.TicketItem.MinRisk=").Append(pTicket.TicketItem.MinRisk)
                .Append("&PendingTicket.TicketItem.MinRiskAmount=").Append(pTicket.TicketItem.MinRiskAmount)
                .Append("&PendingTicket.TicketItem.MaxWin=").Append(pTicket.TicketItem.MaxWin)
                .Append("&PendingTicket.TicketItem.MaxWinAmount=").Append(pTicket.TicketItem.MaxWinAmount)
                .Append("&PendingTicket.TicketItem.MinWin=").Append(pTicket.TicketItem.MinWin)
                .Append("&PendingTicket.TicketItem.MinWinAmount=").Append(pTicket.TicketItem.MinWinAmount)
                .Append("&PendingTicket.TicketItem.OverUnder=").Append(pTicket.TicketItem.OverUnder)
                .Append("&PendingTicket.TicketItem.Period=").Append(pTicket.TicketItem.Period)
                .Append("&PendingTicket.TicketItem.PeriodDescription=").Append(pTicket.TicketItem.PeriodDescription)
                .Append("&PendingTicket.TicketItem.PeriodShortDescription=").Append(pTicket.TicketItem.PeriodShortDescription)
                .Append("&PendingTicket.TicketItem.Pick=").Append(pTicket.TicketItem.Pick)
                .Append("&PendingTicket.TicketItem.Sport=").Append(pTicket.TicketItem.Sport)
                .Append("&PendingTicket.TicketItem.SportId=").Append(pTicket.TicketItem.SportId)
                .Append("&PendingTicket.TicketItem.SportType=").Append(pTicket.TicketItem.SportType)
                .Append("&PendingTicket.TicketItem.StartDate=").Append(pTicket.TicketItem.StartDate)
                .Append("&PendingTicket.TicketItem.Team1FavoriteCss=").Append(pTicket.TicketItem.Team1FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team1Id=").Append(pTicket.TicketItem.Team1Id)
                .Append("&PendingTicket.TicketItem.Team1Name=").Append(pTicket.TicketItem.Team1Name)
                .Append("&PendingTicket.TicketItem.Team1Pitcher=").Append(pTicket.TicketItem.Team1Pitcher)
                .Append("&PendingTicket.TicketItem.Team1RedCards=").Append(pTicket.TicketItem.Team1RedCards)
                .Append("&PendingTicket.TicketItem.Team1Score=").Append(pTicket.TicketItem.Team1Score)
                .Append("&PendingTicket.TicketItem.Team2FavoriteCss=").Append(pTicket.TicketItem.Team2FavoriteCss)
                .Append("&PendingTicket.TicketItem.Team2Id=").Append(pTicket.TicketItem.Team2Id)
                .Append("&PendingTicket.TicketItem.Team2Name=").Append(pTicket.TicketItem.Team2Name)
                .Append("&PendingTicket.TicketItem.Team2Pitcher=").Append(pTicket.TicketItem.Team2Pitcher)
                .Append("&PendingTicket.TicketItem.Team2RedCards=").Append(pTicket.TicketItem.Team2RedCards)
                .Append("&PendingTicket.TicketItem.Team2Score=").Append(pTicket.TicketItem.Team2Score)
                .Append("&PendingTicket.TicketItem.Team1PitcherChecked=").Append(pTicket.TicketItem.Team1PitcherChecked)
                .Append("&PendingTicket.TicketItem.Team2PitcherChecked=").Append(pTicket.TicketItem.Team2PitcherChecked)
                .Append("&riskwin=").Append(pTicket.TicketItem.RiskOrWin == 0 ? 'r' : 'w')
                .Append("&PendingTicket.TicketItem.StakeAmount=").Append(stake)
                .Append("&PendingTicket.BetMaximum=").Append(pTicket.BetMaximum)
                .Append("&PendingTicket.AcceptBetterLines=True")//.Append(pTicket.AcceptBetterLines)
                .Append("&InDangerZone=").Append(pTicket.TicketItem.InDangerZone)
                .Append("&betTicketLineType=").Append(pTicket.TicketItem.LineTypeLabel.Equals("Handicap") == true ? "spread" : "total")
                .Append("&ticketPlaced=true")
                .Append("&isRisk=").Append(pTicket.TicketItem.RiskOrWin == 0 ? "true" : "false")
                .Append("&isTestDrive=");

                var re = SendPinna(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, ContentType);
                st.Stop();
                Debug.WriteLine(st.ElapsedMilliseconds);
                if (!String.IsNullOrEmpty(re.Result))
                {
                    var a = JsonConvert.DeserializeObject<ResponseConfirm>(re.Result);
                    // 3: success
                    // 10: zone dangerous
                    if (a.Notification.Type == 3 || a.Notification.Type == 10 || a.Notification.Message.Contains("pending"))
                    {
                        FireLogBet(match, odd, betType, stake, eBetStatusType.MissOddPina, eServerScan.Local);

                        return true;
                    }
                }

                return false;
            }
            catch (Exception)
            {
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
                    AvailabeCredit = (float)ob.availableBalance;
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
                Logger.Error("GetBetList: " + ex.Message);
                return null;
            }

        }
        public List<Bet> GetStatement(DateTime dt)
        {
            try
            {
                var stringBetList = GetBetList(_authorization, dt.AddDays(-1), dt);
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




    }

    public class ObjectData
    {
        public Sport Sport { get; set; }
    }

    public class Sport
    {
        //public int SportId { get; set; }
        //public string SportName { get; set; }
        //public int? SortOrder { get; set; }
        public List<Market> Markets { get; set; }
        //public int LineViewType { get; set; }
        //public int CssViewType { get; set; }
        //public bool IsParlay { get; set; }
        //public object Sports { get; set; }
        //public object Times { get; set; }

        public List<Period> Periods { get; set; }
        // everything else gets stored here

    }
    public class TmH
    {
        //public string Fc { get; set; }
        public string Txt { get; set; }
        //public string Pd { get; set; }
        //public string Ex { get; set; }
        //public string Sc { get; set; }
    }

    public class TmA
    {
        //public string Fc { get; set; }
        public string Txt { get; set; }
        //public string Pd { get; set; }
        //public string Ex { get; set; }
        //public string Sc { get; set; }
    }

    public class GameLine
    {
        public string Rot { get; set; }
        public string EvId { get; set; }
        public string Lvl { get; set; }
        public TmH TmH { get; set; }
        public TmA TmA { get; set; }
        //public string Alt { get; set; }
        //public string DispDate { get; set; }
        //public string Date { get; set; }
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
        //public string DisplayName { get; set; }
        //public string PeriodDesc { get; set; }
        public List<GameLine> GameLines { get; set; }
        public List<string> UnchangedGames { get; set; }
        //public List<BuySellLevel> BuySellLevels { get; set; }
        public bool IsVisible { get; set; }
        //public int SportType { get; set; }
        public string LeagueId { get; set; }
        //public string Name { get; set; }
        //public int? SortOrder { get; set; }
        //public object MaxWagerLimit { get; set; }
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
        public float? Line { get; set; }
        public float? Price { get; set; }
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
        public double MaxRiskAmount { get; set; }
        public string MinRisk { get; set; }
        public double MinRiskAmount { get; set; }
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


    #region for api
    public class BalanceResponse
    {
        public float? availableBalance { get; set; }
        public float? outstandingTransactions { get; set; }
        public float? givenCredit { get; set; }
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

    #endregion
}
