using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.Helper;
using BCWin.Engine.Isn.Models;
using log4net;
using Newtonsoft.Json;
using System;
using BcWin.Core.Utils;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BcWin.Captcha;
using System.Security.Cryptography;
using System.Diagnostics;

namespace BCWin.Engine.Isn
{
    public class IsnEngine : IsnHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IsnEngine));

        public event UpdateDataHandler UpdateLiveDataChange;
        public delegate void UpdateDataHandler(List<MatchIsnDTO> m, bool isLive, int type = 0);

        public int Rebet { get; set; }
        //private string _apiKey = "";
        //private string _memberToken = "";
        //private string _userId = "";
        //private string _lastRequestKey = "";
        private const string User_Agent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/5…ML, like Gecko) Chrome/46.0.2490.86 Safari/537.36";
        private const string Accept = "application/json, text/javascript, */*; q=0.01";
        private string X_fp ="617860d9ebd3ab357107b93a6e48e374";
        private const string Accept_Encoding = "gzip, deflate";
        private string Origin = "http://isn01.com";
        private string Referal = "http://isn01.com/";
        private const string Accept_Language = "en-US";
        //  private const string Host = "isn01.com";
        //    private CookieContainer _cookieContainer;
        public UserInfo _userCredential;
        public List<MatchIsnDTO> _matchs = new List<MatchIsnDTO>();
        private System.Threading.Timer objLiveScanTimer;
        private System.Threading.Timer objCheckLoginTimer;


        public bool Login(string url, string userName, string password, string code)
        {
            try
            {
                if (CookieContainer == null)
                {
                    Logger.Error("Get capcha first");
                    return false;
                }
                UserName = userName;
                Password = password;
                LoginUrl = url;

                url += "membersite/resource/member/login";

                Referal = LoginUrl + "membersite/en-US/ui/";// using for all coapi

                string referal = LoginUrl + "membersite/login.jsp";
                string param = String.Format("language=en_US&username={0}&password={1}&code={2}&buId=1&dummyLang=", userName, password, code);


                var re = SendIsn(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param), Host, Accept, referal, "application/x-www-form-urlencoded; charset=UTF-8");


                if (re != null && re.Result != null && !re.Result.StartsWith("i18nMsg.login.validation"))
                {
                    var info = JsonConvert.DeserializeObject<UserInfo>(re.Result);
                    if (info != null)
                    {
                        UserName = userName;
                        _userCredential = info;
                        objCheckLoginTimer = new System.Threading.Timer(CheckReLogin, true, 0, 10000);
                        Logger.Info("Login Success: " + re.SetCookie);
                        AccountStatus = eAccountStatus.Online;
                        return true;
                    }
                }
                objCheckLoginTimer = null;
                AccountStatus = eAccountStatus.Offline;

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Login FAIL: " + ex.Message);
                AccountStatus = eAccountStatus.Offline;
                UserName = "";
                Password = "";
                return false;
            }
        }

        public bool TryLogin(string url, string userName, string password)
        {
            try
            {
                //var a = ConvertToUnixTime(DateTime.Now);
                //var b = ConvertToUnixTime2(DateTime.Now);
                for (int i = 0; i < 4; i++)
                {
                    if (CookieContainer != null)
                    {
                        CookieContainer = null;
                    }

                    string code = "";
                    while (code == null || code.Length < 4)
                    {

                        Bitmap capchaBmp = PreLogin(url);
                        code = new CaptchaEngine().DeCaptcha(capchaBmp);
                        Thread.Sleep(1000);
                    }

                    if (Login(url, userName, password, code))
                    {
                        return true;
                    }
                    Thread.Sleep(15000);
                }

                return false;

            }
            catch (Exception ex)
            {
                Logger.Error("TryLogin: " + ex.Message);
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

                    string code = "";
                    while (code == null || code.Length < 4)
                    {
                        Bitmap capchaBmp = PreLogin(LoginUrl);
                        code = new CaptchaEngine().DeCaptcha(capchaBmp);
                        Thread.Sleep(1000);
                    }

                    if (Login(LoginUrl, UserName, Password, code))
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
                objCheckLoginTimer = null;
                AccountStatus = eAccountStatus.Offline;
                return false;
            }
        }

        public void Logout()
        {
            try
            {
                string url = LoginUrl + "membersite/en-US/resource/member/logout";

                var re = SendIsn(url, "POST", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8");
                AccountStatus = eAccountStatus.Offline;
                objCheckLoginTimer = null;
            }
            catch (Exception)
            {
                AccountStatus = eAccountStatus.Offline;
                objCheckLoginTimer = null;
            }

        }

        public float UpdateAvailabeCredit()
        {
            try
            {
                CheckLogin();
                AvailabeCredit = _userCredential.betCredit;
                return AvailabeCredit;
            }
            catch (Exception ex)
            {
                Logger.Error("UpdateAvailabeCredit: " + ex.Message);
                return 0;
            }

        }

        public List<TicketStatement> GetStatement(DateTime dt)
        {
            //GET membersite/en-US/resource/statement/1/1443153600000?_=1443717897354
            string url = LoginUrl + "membersite/en-US/resource/statement/1/" + ConvertToUnixTime(dt) + "?_=" + ConvertToUnixTime(DateTime.Now);

            var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8");
            if (res != null && !String.IsNullOrEmpty(res.Result))
            {
                var data = JsonConvert.DeserializeObject<StatementResponse>(res.Result);
                if (data != null && data.tickets != null && data.tickets.Count != 0)
                {
                    return data.tickets;
                }
            }
            return null;
        }

        public List<NormalList> GetBetList()
        {

            string url = LoginUrl + "membersite/en-US/resource/bp/betList/match/4/1?_=" + ConvertToUnixTime(DateTime.Now);

            var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8");
            if (res != null && !String.IsNullOrEmpty(res.Result))
            {
                var data = JsonConvert.DeserializeObject<BetListResponse>(res.Result);
                if (data != null && data.groupList != null && data.groupList.Count != 0)
                {
                    List<NormalList> result = new List<NormalList>();
                    foreach (var item in data.groupList)
                    {
                        var normalList = GetBetListByEventId(item.eventId);
                        if (normalList != null)
                        {
                            result.AddRange(normalList);
                        }
                    }
                    return result;
                }
            }
            return null;

        }

        public List<NormalList> GetBetListByEventId(string id)
        {
            //http://isn01.com/membersite/vi-VN/resource/bp/betList/match/4/205583/1?_=1444288432224
            string url = LoginUrl + "membersite/en-US/resource/bp/betList/match/4/" + id + "/1?_=" + ConvertToUnixTime(DateTime.Now);

            var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8");
            if (res != null && !String.IsNullOrEmpty(res.Result))
            {
                var data = JsonConvert.DeserializeObject<BetListResponse>(res.Result);
                if (data != null && data.normalList != null && data.normalList.Count != 0)
                {
                    return data.normalList;
                }
            }
            return null;
        }
        private void CheckReLogin(object state)
        {
            CheckLogin();
            GetTime();
       //     PendingLastTenBets();
        }

        public Bitmap PreLogin(string url)
        {
            try
            {
                //go first to get 
                CookieContainer = new System.Net.CookieContainer();
                LoginUrl = url;
                Host = LoginUrl.Replace("http://", "").Replace("/", "");
                Origin = LoginUrl.Remove(LoginUrl.Length - 1);

                var a = SendIsn(url + "membersite/login.jsp", "GET", User_Agent, CookieContainer, null, Host, Accept);
                Logger.Info("Cookie app call PRELOGIN " + a.SetCookie);
                if (a != null && !String.IsNullOrEmpty(a.SetCookie))
                {
                    //  string contentType= 
                    BindCookieContainer(new Uri(url + "membersite/login.jsp"), a.SetCookie, CookieContainer);
                    string referal = url + "membersite/login.jsp";
                    var bm = GetImage(url + "membersite/ui/captcha?w=80&h=20&fs=18&_" + ConvertToUnixTime(DateTime.Now), CookieContainer, Host, referal);
                    if (bm != null)
                    {
                        Logger.Error("Get Capcha OK");
                        return bm;
                    }
                }
                Logger.Error("Get Capcha FAIL: ");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error("Get Capcha FAIL: " + ex.Message);
                return null;
            }
        }

        public bool CheckLogin()
        {
            string resString = "";
            try
            {
                string url = LoginUrl + "membersite/en-US/resource/member?_=" + ConvertToUnixTime(DateTime.Now);

                var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal);
                if ((int)res.StatusCode == 429 || (int)res.StatusCode == 504)
                {
                    return true;
                }
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    resString = res.Result;
                    var userInfo = JsonConvert.DeserializeObject<UserInfo>(res.Result);
                    if (userInfo != null & userInfo.memberId != 0)
                    {
                        _userCredential = userInfo;
                        AccountStatus = eAccountStatus.Online;

                        return true;

                    }
                }

                else
                {
                    objCheckLoginTimer = null;
                    UpdateException(this, eExceptionType.LoginFail);
                    AccountStatus = eAccountStatus.Offline;
                }

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR when CheckLogin: " + ex.Message);
                Logger.Error(resString);
                AccountStatus = eAccountStatus.Offline;
                objCheckLoginTimer = null;
                //objLiveScanTimer = null;
                UpdateException(this, eExceptionType.LoginFail);
                //try login
                return false;
            }

        }

        public void PendingLastTenBets()
        {
            string resString = "";
            try
            {
                string url = LoginUrl + "membersite/en-US/resource/bp/pendingAndLastTenBets?_=" + ConvertToUnixTime(DateTime.Now);

                var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal);
         
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR PendingLastTenBets" + ex.Message);

            }

        }

        public void GetTime()
        {
            try
            {
                string url = LoginUrl + "membersite/en-US/resource/events/time?_=" + ConvertToUnixTime(DateTime.Now);

                var re = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", "http://isn01.com",X_fp);

            }
            catch (Exception ex)
            {
                Logger.Error("GetTime: " + ex.Message);
            }

        }

        public void ScanLiveOdds()
        {
            //Logger.Info("ISN Start scan at: " + DateTime.Now.ToString("hh:mm:ss"));
            string resString = "";
            try
            {
                lock (LockLive)
                {
                    string url = LoginUrl + "membersite/en-US/resource/events/refresh/1/1/3/3/0/4/0/1?_=" + ConvertToUnixTime(DateTime.Now);

                    var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8","",X_fp);
                    if (res != null && !String.IsNullOrEmpty(res.Result))
                    {
                        resString = res.Result;

                        var data = JsonConvert.DeserializeObject<List<DataOddsResponse>>(res.Result);
                        foreach (var league in data[0].leagues)
                        {
                            foreach (var @event in league.events)
                            {

                                var match = _matchs.FirstOrDefault(p => p.MatchID == @event.id);
                                #region insert new match
                                if (match == null)
                                {
                                    match = new MatchIsnDTO
                                   {
                                       LeagueID = league.id,
                                       LeagueName = league.name,
                                       MatchID = @event.id,
                                       score = @event.score,
                                       eventPitcherId = @event.eventPitcherId,
                                       HomeTeamName = CoreEngine.CleanTeamNameMore(@event.descriptions[0].teamName),
                                       AwayTeamName = CoreEngine.CleanTeamNameMore(@event.descriptions[1].teamName),
                                       Odds = new List<OddDTO>()
                                   };

                                    //insert HDP
                                    if (@event.markets[1].lines != null && @event.markets[1].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[1].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }

                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HCP,
                                            });
                                        }
                                    }

                                    //insert Half HDP
                                    if (@event.markets[4].lines != null && @event.markets[4].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[4].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }
                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HalfHCP,
                                            });
                                        }
                                    }

                                    //insert OU
                                    if (@event.markets[2].lines != null && @event.markets[2].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[2].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }
                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.OU,
                                            });
                                        }
                                    }

                                    //insert Half OU
                                    if (@event.markets[5].lines != null && @event.markets[5].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[5].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }

                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HalfOU,
                                            });
                                        }
                                    }
                                    if (match.Odds.Count != 0)
                                    {
                                        _matchs.Add(match);

                                    }
                                }
                                #endregion
                                #region update match
                                else
                                {
                                    match.score = @event.score;
                                    match.eventPitcherId = @event.eventPitcherId;
                                    match.Odds.Clear();

                                    //insert HDP
                                    if (@event.markets[1].lines != null && @event.markets[1].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[1].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }
                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HCP,
                                            });
                                        }
                                    }

                                    //insert Half HDP
                                    if (@event.markets[4].lines != null && @event.markets[4].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[4].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }
                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HalfHCP,
                                            });
                                        }
                                    }

                                    //insert OU
                                    if (@event.markets[2].lines != null && @event.markets[2].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[2].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }
                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.OU,
                                            });
                                        }
                                    }

                                    //insert Half OU
                                    if (@event.markets[5].lines != null && @event.markets[5].lines.Count != 0)
                                    {
                                        foreach (var line in @event.markets[5].lines)
                                        {
                                            if (line.selections == null || line.selections.Count == 0)
                                            {
                                                continue;
                                            }

                                            match.Odds.Add(new OddDTO
                                            {
                                                selectionIdHome = line.selections[0].id,
                                                selectionIdAway = line.selections[1].id,
                                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                                Odd = line.selections[0].handicap,
                                                oddsHome = line.selections[0].odds,
                                                oddsAway = line.selections[1].odds,
                                                handicapHome = line.selections[0].handicap,
                                                handicapAway = line.selections[1].handicap,
                                                OddType = eOddType.HalfOU,
                                            });
                                        }
                                    }

                                    if (match.Odds.Count == 0)
                                    {
                                        _matchs.Remove(match);

                                    }
                                }
                                #endregion
                            }
                        }

                        if (UpdateLiveDataChange != null)
                        {
                            _matchs.Shuffle();
                            //Logger.Info("ISN End scan at: " + DateTime.Now.ToString("hh:mm:ss"));

                            UpdateLiveDataChange(_matchs, true);
                        }
                    }

                    //CheckLogin();
                    //GetTime();

                }
            }
            catch (Exception ex)
            {
                Logger.Error("ERROR SCAN LIVE: " + ex.Message);
                Logger.Error(resString);
                //   objCheckLoginTimer = null;
                //return res2;
            }

        }



        public PrepareBetDTO GetPrepareLimit(int selectionId)
        {
            //http://isn01.com/membersite/vi-VN/resource/bp/singlebetlimit/11053076/3/4?_=1443194358204

            string url = LoginUrl + "membersite/en-US/resource/bp/singlebetlimit/" + selectionId + "/3/4?_=" + ConvertToUnixTime(DateTime.Now);

            var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8","",X_fp);
            if (res != null && !String.IsNullOrEmpty(res.Result))
            {
                var result = JsonConvert.DeserializeObject<PrepareLimitResponse>(res.Result);
                if (result != null)
                {
                    PrepareBetDTO prepareBetDto = new PrepareBetDTO(true);
                    prepareBetDto.MinBet = result.minBet;
                    prepareBetDto.MaxBet = result.maxBet;

                    return prepareBetDto;
                }
            }
            return null;
        }

        public PrepareBetDTO GetPrepareLimit2(int selectionId)
        {
            PrepareBetDTO prepareBetDto = new PrepareBetDTO(true);
            prepareBetDto.MinBet = 4;
            prepareBetDto.MaxBet = 15;

            return prepareBetDto;

        }

        public int GetMax(int selectionId)
        {
            try
            {
                string url = LoginUrl + "membersite/en-US/resource/bp/singlebetlimit/" + selectionId + "/3/4?_=" + ConvertToUnixTime(DateTime.Now);

                var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8","",X_fp);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    //  Logger.Error("GetMax: " + res.Result);
                    var result = JsonConvert.DeserializeObject<PrepareLimitResponse>(res.Result);
                    if (result != null)
                    {
                        return result.maxBet;

                    }
                }
                return 0;
            }
            catch (Exception ex)
            {
                Logger.Error("GetMax: " + ex.Message);
                return 0;
            }

        }

        public PrepareBetDTO PrepareBet(MatchIsnDTO match, OddDTO odd, eBetType type, bool isLive, float oddDef)
        {
            try
            {
                //  http://isn01.com/membersite/vi-VN/resource/bp/singlebetticket/10819604/3/4?_=1443164300681
                string url = LoginUrl + "membersite/en-US/resource/bp/singlebetticket/" + (type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway) + "/3/4?_=" + ConvertToUnixTime(DateTime.Now);

                //PrepareBetDTO prepareDTO = GetPrepareLimit((type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway));
                //if (prepareDTO == null)
                //{
                //    return null;
                //}

                var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8","",X_fp);
                if (res.StatusCode== HttpStatusCode.Redirect)
                {
                    UpdateException(this, eExceptionType.LoginFail);
                    return null;
                }

                // Logger.Info("PrepareBet: " + res);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    var result = JsonConvert.DeserializeObject<PrepareResponse>(res.Result);
                    if (result != null)
                    {
                        //Debug.WriteLine(res.Result);
                        if (Math.Abs(oddDef) == Math.Abs(result.handicap))
                        {
                            //      Logger.Info("Prepare ISN OK: " + res.Result);
                            PrepareBetDTO prepareDTO = new PrepareBetDTO(true);
                            prepareDTO.BetType = type;

                            if (type == eBetType.Home)
                            {
                                odd.oddsHome = result.decimalOdds;
                                odd.HomeOdd = result.odds;
                            }
                            else
                            {
                                odd.oddsAway = result.decimalOdds;
                                odd.AwayOdd = result.odds;
                            }

                            match.score = result.score;
                            prepareDTO.NewOdd = result.odds;

                            return prepareDTO;
                        }
                        else
                        {
                            odd.Odd = result.handicap;
                        }
                    }
                }
                else
                {
                    UpdateException(this, eExceptionType.RequestFail);
                    Logger.Info("Prepare ISN FAIL: " + res.Result);

                }

                return null;
            }
            catch (Exception ex)
            {
                Logger.Info("Prepare ISN FAIL(EXCEPTION): " + ex.Message);
                return null;
            }

        }

        public bool ConfirmBet(int stake, MatchIsnDTO match, OddDTO odd, eBetType type) //selectionId
        {
            try
            {
                StringBuilder param = new StringBuilder("selectionId=" + (type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway));
                param.Append("&odds=" + (type == eBetType.Home ? odd.oddsHome : odd.oddsAway))
                .Append("&nativeOdds=" + (type == eBetType.Home ? odd.HomeOdd : odd.AwayOdd))
                .Append("&oddsFormat=4&handicap=" + odd.Odd)
                .Append("&score=" + match.score)
                .Append("&eventPitcherId=" + match.eventPitcherId)
               .Append("&stake=" + stake)
               .Append("&username=" + _userCredential.memberId);

                //Logger.Info(param.ToString());
                //username:158246
                // http://isn01.com/membersite/vi-VN/resource/bp/placebet?ts=1443165000404:5267
                // Random random = new Random();
                //    string url = LoginUrl + "membersite/vi-VN/resource/bp/placebet?ts=" + ConvertToUnixTime(DateTime.Now) + ":" + Math.Floor(random.NextDouble() * 10001);
                string url = LoginUrl + "membersite/en-US/resource/bp/placebet";

                var res = SendIsn(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", Origin,X_fp);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    Logger.Info(res.Result);
                    var result = JsonConvert.DeserializeObject<ConfirmResponse>(res.Result);
                    try
                    {
                        Logger.Info(res.Result);
                    }
                    catch (Exception)
                    {

                    }

                    if (result != null && result.respCode == 0)
                    {
                        FireLogBet(match, odd, type, stake, eBetStatusType.Success, eServerScan.Local);
                        //Logger.Info("Bet ISN OK");
                        return true;
                    }
                    else
                    {
                        // try confirm
                        lock (LockLive)
                        {
                            Random rd = new Random();
                        

                            float firstOdd = odd.Odd;
                            for (int i = 0; i < Rebet; i++)
                            {
                                //Logger.Error("Try Prepare ISN Before: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                                var prepareNew = PrepareBet(match, odd, type, true, firstOdd);
                                //Logger.Error("Try Prepare ISN After: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                                if (prepareNew != null)
                                {
                                    var ok = TryConfirmBet(stake, match, odd, type);
                                    if (ok)
                                    {
                                        Logger.Info("Re Bet ISN  OK at " + i);
                                        return true;
                                    }
                                }

                                Thread.Sleep(rd.Next(1,10));

                                var newMatch = _matchs.FirstOrDefault(p => p.MatchID == match.ID);
                                if (newMatch != null)
                                {
                                    var newOdd = newMatch.Odds.FirstOrDefault(p => p.Odd == firstOdd && odd.OddType == p.OddType);
                                    if (newOdd!=null)
                                    {
                                        prepareNew = PrepareBet(newMatch, newOdd, type, true, firstOdd);
                                        if (prepareNew!=null)
                                        {
                                            var ok = TryConfirmBet(stake, newMatch, newOdd, type);
                                            if (ok)
                                            {
                                                Logger.Info("Re Bet ISN  OK at " + i);
                                                return true;
                                            }
                                        }
                                     
                                    }
                                }
                                Thread.Sleep(rd.Next(1, 10));
                            }
                        }

                    }
                }

                Logger.Error("Bet ISN FAIL: " + res.Result);

                FireLogBet(match, odd, type, stake, eBetStatusType.Fail, eServerScan.Local);

                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error ISN ConfirmBet: " + ex.Message);
                return false;
            }

        }

        public bool ConfirmBet2(int stake, MatchIsnDTO match, OddDTO odd, eBetType type, eBetStatusType logType, bool acceptBetter = false) //selectionId
        {
            try
            {
                StringBuilder param = new StringBuilder("selectionId=" + (type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway));
                param.Append("&odds=" + (type == eBetType.Home ? odd.oddsHome : odd.oddsAway))
                .Append("&nativeOdds=" + (type == eBetType.Home ? odd.HomeOdd : odd.AwayOdd))
                .Append("&oddsFormat=4&handicap=").Append(odd.Odd)
                .Append("&acceptBetterOdds=").Append(acceptBetter)
                .Append("&score=" + match.score)
                .Append("&eventPitcherId=" + match.eventPitcherId)
               .Append("&stake=" + stake)
               .Append("&username=" + _userCredential.memberId);

                //Logger.Info(param.ToString());
                //username:158246
                // http://isn01.com/membersite/vi-VN/resource/bp/placebet?ts=1443165000404:5267
                // Random random = new Random();
                //    string url = LoginUrl + "membersite/vi-VN/resource/bp/placebet?ts=" + ConvertToUnixTime(DateTime.Now) + ":" + Math.Floor(random.NextDouble() * 10001);
                string url = LoginUrl + "membersite/en-US/resource/bp/placebet";

                var res = SendIsn(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", Origin,X_fp);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    var result = JsonConvert.DeserializeObject<ConfirmResponse>(res.Result);
                    if (result != null && result.respCode == 0)
                    {
                        FireLogBet(match, odd, type, stake, logType, eServerScan.Local);
                        //Logger.Info("Bet ISN OK");
                        return true;
                    }
                }


                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error ISN ConfirmBet: " + ex.Message);
                return false;
            }

        }



        public bool TryConfirmBet(int stake, MatchIsnDTO match, OddDTO odd, eBetType type)
        {
            try
            {
                //Logger.Error("TryBet ISN Before: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                //var odd2 = GetNewestOddByMatch(match, odd);
                //if (odd2 == null)
                //{
                //    return false;
                //}
                //Logger.Error("TryBet ISN After: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);



                StringBuilder param = new StringBuilder("selectionId=" + (type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway));
                param.Append("&odds=").Append((type == eBetType.Home ? odd.oddsHome : odd.oddsAway))
                .Append("&nativeOdds=").Append((type == eBetType.Home ? odd.HomeOdd : odd.AwayOdd))
                .Append("&oddsFormat=4&handicap=").Append(odd.Odd)
                .Append("&acceptBetterOdds=true&score=").Append(match.score)
                .Append("&eventPitcherId=").Append(match.eventPitcherId)
               .Append("&stake=").Append(stake)
               .Append("&username=").Append(_userCredential.memberId);
                // Logger.Info("Try : " + param.ToString());

                //Random random = new Random();
                //string url = LoginUrl + "membersite/en-US/resource/bp/placebet?ts=" + ConvertToUnixTime(DateTime.Now) + ":" + Math.Floor(random.NextDouble() * 10001);
                string url = LoginUrl + "membersite/en-US/resource/bp/placebet";

                var res = SendIsn(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", Origin,X_fp);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    var result = JsonConvert.DeserializeObject<ConfirmResponse>(res.Result);
                    if (result != null && result.respCode == 0)
                    {
                        FireLogBet(match, odd, type, stake, eBetStatusType.MissOddIsn, eServerScan.Local);
                        //Logger.Info("TryBet ISN OK");
                        return true;
                    }
                }

                //  Logger.Error("TryBet ISN FAIL: " + res.Result);
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        // ko xài
        public bool TryConfirmBet2(int stake, MatchIsnDTO match, OddDTO odd, eBetType type)
        {
            try
            {
                //Logger.Error("TryBet ISN Before: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);
                //var odd2 = GetNewestOddByMatch(match, odd);
                //if (odd2 == null)
                //{
                //    return false;
                //}
                //Logger.Error("TryBet ISN After: " + match.HomeTeamName + "-" + match.AwayTeamName + " " + odd.Odd + "|" + odd.HomeOdd + "|" + odd.AwayOdd);



                StringBuilder param = new StringBuilder("selectionId=" + (type == eBetType.Home ? odd.selectionIdHome : odd.selectionIdAway));
                param.Append("&odds=" + (type == eBetType.Home ? odd.oddsHome : odd.oddsAway))
                .Append("&nativeOdds=" + (type == eBetType.Home ? odd.HomeOdd : odd.AwayOdd))
                .Append("&oddsFormat=4")
               .Append("&handicap=" + odd.Odd)
                .Append("&score=" + match.score + "&acceptBetterOdds=true")
                .Append("&eventPitcherId=" + match.eventPitcherId)
               .Append("&stake=" + stake)
               .Append("&username=" + _userCredential.memberId);
                // Logger.Info("Try : " + param.ToString());

                //Random random = new Random();
                //string url = LoginUrl + "membersite/en-US/resource/bp/placebet?ts=" + ConvertToUnixTime(DateTime.Now) + ":" + Math.Floor(random.NextDouble() * 10001);
                string url = LoginUrl + "membersite/en-US/resource/bp/placebet";

                var res = SendIsn(url, "POST", User_Agent, CookieContainer, Encoding.UTF8.GetBytes(param.ToString()), Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", Origin,X_fp);
                if (res != null && !String.IsNullOrEmpty(res.Result))
                {
                    var result = JsonConvert.DeserializeObject<ConfirmResponse>(res.Result);
                    if (result != null && result.respCode == 0)
                    {
                        FireLogBet(match, odd, type, stake, eBetStatusType.BetAgainstIbet, eServerScan.Local);
                        //Logger.Info("TryBet ISN OK");
                        return true;
                    }
                }

                //  Logger.Error("TryBet ISN FAIL: " + res.Result);
                return false;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private OddDTO GetNewestOddByMatch(MatchIsnDTO match, OddDTO odd)
        {
            OddDTO odd2 = null;
            string url = LoginUrl + "membersite/en-US/resource/events/refresh/1/1/3/3/0/4/0/1?_=" + ConvertToUnixTime(DateTime.Now);
            string Referal = "http://isn01.com/membersite/en-US/ui/";
            var res = SendIsn(url, "GET", User_Agent, CookieContainer, null, Host, Accept, Referal, "application/x-www-form-urlencoded; charset=UTF-8", "http://isn01.com");
            if (res != null && !String.IsNullOrEmpty(res.Result))
            {

                var data = JsonConvert.DeserializeObject<List<DataOddsResponse>>(res.Result);

                var league = data[0].leagues.FirstOrDefault(p => p.id == match.LeagueID);
                if (league != null)
                {
                    var @event = league.events.FirstOrDefault(p => p.id == match.MatchID);
                    if (@event != null)
                    {

                        Line line = null;
                        if (odd.OddType == eOddType.HCP)
                        {
                            line = @event.markets[1].lines.FirstOrDefault(p => p.selections != null && p.selections.Count != 0 && p.selections[0].handicap == odd.Odd);
                            if (line == null)
                            {
                                return null;
                            }
                            odd2 = new OddDTO
                            {
                                selectionIdHome = line.selections[0].id,
                                selectionIdAway = line.selections[1].id,
                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                Odd = line.selections[0].handicap,
                                oddsHome = line.selections[0].odds,
                                oddsAway = line.selections[1].odds,
                                handicapHome = line.selections[0].handicap,
                                handicapAway = line.selections[1].handicap,
                                OddType = eOddType.HCP,
                            };
                        }
                        else if (odd.OddType == eOddType.HalfHCP)
                        {
                            line = @event.markets[4].lines.FirstOrDefault(p => p.selections != null && p.selections.Count != 0 && Math.Abs(p.selections[0].handicap) == odd.Odd);
                            if (line == null)
                            {
                                return null;
                            }
                            odd2 = new OddDTO
                            {
                                selectionIdHome = line.selections[0].id,
                                selectionIdAway = line.selections[1].id,
                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                Odd = line.selections[0].handicap,
                                oddsHome = line.selections[0].odds,
                                oddsAway = line.selections[1].odds,
                                handicapHome = line.selections[0].handicap,
                                handicapAway = line.selections[1].handicap,
                                OddType = eOddType.HalfHCP,
                            };
                        }
                        else if (odd.OddType == eOddType.OU)
                        {
                            line = @event.markets[2].lines.FirstOrDefault(p => p.selections != null && p.selections.Count != 0 && Math.Abs(p.selections[0].handicap) == odd.Odd);
                            if (line == null)
                            {
                                return null;
                            }
                            odd2 = new OddDTO
                            {
                                selectionIdHome = line.selections[0].id,
                                selectionIdAway = line.selections[1].id,
                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                Odd = line.selections[0].handicap,
                                oddsHome = line.selections[0].odds,
                                oddsAway = line.selections[1].odds,
                                handicapHome = line.selections[0].handicap,
                                handicapAway = line.selections[1].handicap,
                                OddType = eOddType.OU,
                            };
                        }
                        else
                        {
                            line = @event.markets[5].lines.FirstOrDefault(p => p.selections != null && p.selections.Count != 0 && Math.Abs(p.selections[0].handicap) == odd.Odd);
                            if (line == null)
                            {
                                return null;
                            }

                            odd2 = new OddDTO
                            {
                                selectionIdHome = line.selections[0].id,
                                selectionIdAway = line.selections[1].id,
                                HomeOdd = float.Parse(line.selections[0].odds_formatted),
                                AwayOdd = float.Parse(line.selections[1].odds_formatted),
                                Odd = line.selections[0].handicap,
                                oddsHome = line.selections[0].odds,
                                oddsAway = line.selections[1].odds,
                                handicapHome = line.selections[0].handicap,
                                handicapAway = line.selections[1].handicap,
                                OddType = eOddType.HalfOU,
                            };
                        }

                    }
                }


            }


            return odd2;
        }

        /// <summary>
        /// no using - using for API
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        //public bool Login(string userName, string password)
        //{
        //    try
        //    {
        //        string res = Login(userName, password);
        //        if (!String.IsNullOrEmpty(res))
        //        {
        //            var obj = JsonConvert.DeserializeObject<LoginResponse>(res);
        //            _memberToken = obj.memberToken;
        //            _userId = obj.userId;
        //            _lastRequestKey = obj.lastRequestKey;

        //            var resApiKey = GetAPIKey(_memberToken, _userId);
        //            if (!String.IsNullOrEmpty(resApiKey))
        //            {
        //                _apiKey = JsonConvert.DeserializeObject<string>(resApiKey);

        //                return true;
        //            }


        //        }
        //        return false;
        //    }
        //    catch (Exception)
        //    {
        //        return false;
        //    }
        //}


        public void InitEngine()
        {
            ServerType = eServerType.Isn;
            Status = eServiceStatus.Initialized;

        }

        public void LogOff()
        {
            this.Dispose();

            if (objCheckLoginTimer != null)
            {
                objCheckLoginTimer.Dispose();
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
            objCheckLoginTimer = null;
            //UpdateException(this);
        }

        public void StartScanEngine(eScanType scanType)
        {
            switch (scanType)
            {
                case eScanType.Live:
                    objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true, 0, 7000);
                    break;

            }

            //EngineLogger = new EngineLogger(EngineName) { ServerType = eServerType.Sbo };

            Logger.Info("STARTED: " + UrlHost);
            Status = eServiceStatus.Started;
        }

        private void WaitScanCallback(object state)
        {
            ScanLiveOdds();
        }

        public void PauseScan()
        {
            if (objLiveScanTimer != null)
            {
                objLiveScanTimer.Dispose();
            }

            Status = eServiceStatus.Paused;
        }

        public void Dispose()
        {
            PauseScan();
        }

        public UserInfo GetAccountProfile()
        {
            var ok = CheckLogin();
            if (ok)
            {
                return _userCredential;
            }

            return null;
        }
    }
}
