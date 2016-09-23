using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Core;
using BcWin.Core.EventDelegate;
using BCWin.Metadata;
using BcWin.Core.Helper;
using BcWin.Processor.Interface;
using BcWin.Processor.Properties;
using HtmlAgilityPack;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using TypescriptB;
using mshtml;

namespace BcWin.Processor
{
    public partial class IbetSubEngine : IbetHelper, IEngine
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetSubEngine));


        public event UpdateDataHandler UpdateLiveDataChange;
        public event UpdateDataHandler UpdateNonLiveDataChange;
        public delegate void UpdateDataHandler(List<MatchOddDTO> m, bool isLive);

        public Dictionary<string, ParamRequest> ParamContainer { get; set; }
        public PrepareBetIbet BetMessageQueue { get; set; }

        private const string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        private System.Threading.Timer objLiveScanTimer;
        private System.Threading.Timer objNonLiveScanTimer;
        private System.Threading.Timer objCheckLoginTimer;

        //public event EventHandler OnReConnectFail;
        //public event EventHandler OnReConnectSuccess;

        #region [constructor]
        public IbetSubEngine()
        {
            ParamContainer = new Dictionary<string, ParamRequest>();
            //LastPrepareTime = DateTime.Now;
            ServerType = eServerType.Ibet;
        }
        #endregion

        public void InitEngine()
        {
            ServerType = eServerType.Ibet;
            Status = eServiceStatus.Initialized;
        }

        public void StartScanEngine(eScanType scanType)
        {
            switch (scanType)
            {
                case eScanType.Live:
                    StartScanLive();
                    break;
                case eScanType.NonLive:
                    StartScanNonLive();
                    break;
                case eScanType.All:
                    StartScanLive();
                    StartScanNonLive();
                    break;
            }

            //EngineLogger = new EngineLogger(EngineName) { ServerType = eServerType.Ibet };
            ScanType = scanType;
            Logger.Info("STARTED: " + UrlHost);
            Status = eServiceStatus.Started;
        }

        public void StartScanLiveDriver(bool isRestart = false)
        {
            ////Get Live data
            string urlInitLodds =
                IbetConfig.URL_INIT_LODDS_MARKET
                + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue + "&_=" + Utils.GetUnixTimestamp();
            ParamContainer["LIVE_CT"] = new ParamRequest("CT", string.Empty);
            string updateTimeLive;

            var lOddMessage = Get(urlInitLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            var liveBag = ConvertFullData(lOddMessage.Result, out updateTimeLive, true);

            if (liveBag.Any())
            {
                DataContainer.LiveMatchOddBag = liveBag;
            }

            //if (liveBag.Any())
            //{
            //    //Logger.Info("SET LIVE BAG DATA !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //    DataContainer.LiveMatchOddBag = liveBag;
            //}

            ParamContainer["LIVE_CT"].KeyValue = updateTimeLive;
            //objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
            //    SystemConfig.TIME_GET_UPDATE_LIVE_IBET, SystemConfig.TIME_GET_UPDATE_LIVE_IBET);
        }

        public void StartUpdateLiveDriver(int interval)
        {
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
                interval, interval);
        }

        private void StartScanLive()
        {
            ////Get Live data
            string urlInitLodds =
                IbetConfig.URL_INIT_LODDS_MARKET
                + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue + "&_=" + Utils.GetUnixTimestamp();
            ParamContainer["LIVE_CT"] = new ParamRequest("CT", string.Empty);
            string updateTimeLive;

            var lOddMessage = Get(urlInitLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            //var lOddMessage = GetClient(urlInitLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            DataContainer.LiveMatchOddBag = ConvertFullData(lOddMessage.Result, out updateTimeLive, true);
            //if (DataContainer.LiveMatchOddBag==null)
            //{
            //    Logger.Info("SET LIVE BAG DATA !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
            //    DataContainer.LiveMatchOddBag = liveBag;
            //}

            ParamContainer["LIVE_CT"].KeyValue = updateTimeLive;
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
                SystemConfig.TIME_GET_UPDATE_LIVE_IBET, SystemConfig.TIME_GET_UPDATE_LIVE_IBET);
        }

        private void StartScanNonLive()
        {
            ////Get non Live data
            string urlInitDodds =
                IbetConfig.URL_INIT_DODDS_MARKET
                + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue + "&_=" + Utils.GetUnixTimestamp();
            ParamContainer["NON_LIVE_CT"] = new ParamRequest("CT", string.Empty);
            string updateTimeNonLive;

            var dOddMessage = Get(urlInitDodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            DataContainer.NonLiveMatchOddBag = ConvertFullData(dOddMessage.Result, out updateTimeNonLive, false);
            ParamContainer["NON_LIVE_CT"].KeyValue = updateTimeNonLive;

            //Start Timer update non live
            objNonLiveScanTimer = new System.Threading.Timer(WaitScanCallback, false,
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET, SystemConfig.TIME_GET_UPDATE_NON_LIVE_IBET);
        }

        public void PauseScan()
        {
            if (objLiveScanTimer != null)
            {
                objLiveScanTimer.Dispose();
            }
            if (objNonLiveScanTimer != null)
            {
                objNonLiveScanTimer.Dispose();
            }
            Status = eServiceStatus.Paused;
        }

        public void PauseToReConnect()
        {
            if (objLiveScanTimer != null)
            {
                objLiveScanTimer.Dispose();
            }
            if (objNonLiveScanTimer != null)
            {
                objNonLiveScanTimer.Dispose();
            }
            if (objCheckLoginTimer != null)
            {
                objCheckLoginTimer.Dispose();
            }
            Status = eServiceStatus.Paused;
        }

        public void Dispose()
        {
            PauseScan();
        }

        public void ProcessUpdateLiveData()
        {
            string urlUpdateLodds = string.Concat(
                           IbetConfig.URL_UPDATE_LODDS_MARKET,
                           ParamContainer["LIVE_CT"].KeyName + "=" + HttpUtility.UrlEncode(ParamContainer["LIVE_CT"].KeyValue),
                           "&key=", GetKey(UserName, this.Host, "lodds", null, ParamContainer["LIVE_CT"].KeyValue, "U"),
                           "&" + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue,
                           "&_=" + Utils.GetUnixTimestamp());

            var updateLiveOddMessage = Get(urlUpdateLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");

            try
            {
                if (updateLiveOddMessage.StatusCode == HttpStatusCode.OK &&
                    !string.IsNullOrEmpty(updateLiveOddMessage.Result))
                {
                    string updateTime;
                    ConvertUpdateData(updateLiveOddMessage.Result, out updateTime,
                        DataContainer.LiveMatchOddBag);
                    if (!ParamContainer.ContainsKey("LIVE_CT"))
                    {
                        ParamContainer["LIVE_CT"] = new ParamRequest("CT", updateTime);
                    }
                    else
                    {
                        ParamContainer["LIVE_CT"].KeyValue = updateTime;
                    }

                    UpdateException(this);
                }
                else
                {
                    //Logger.Error("IBET: END->ProcessUpdateLiveData -> FAIL");
                    //Logger.Error("MESSAGE : " + updateLiveOddMessage.Result + " URL REQUEST:: " + urlUpdateLodds);
                    UpdateException(this, eExceptionType.RequestFail);
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
                Logger.Error("MESSAGE : " + updateLiveOddMessage.Result, ex);
                //UpdateException(this, eExceptionType.RequestFail);
            }
            //UpdateException(this, eExceptionType.RequestFail);
        }

        public void ProcessUpdateNonLiveData()
        {
            try
            {
                lock (LockNonLive)
                {
                    string urlUpdateDodds =
                        IbetConfig.URL_UPDATE_DODDS_MARKET
                        + ParamContainer["NON_LIVE_CT"].KeyName + "=" +
                        HttpUtility.UrlEncode(ParamContainer["NON_LIVE_CT"].KeyValue)
                        + "&" + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue
                        + "&_=" + Utils.GetUnixTimestamp();

                    //Logger.Info("IBET: START->ProcessUpdateNonLiveData -> URL:::" + urlUpdateDodds);
                    var updateLiveOddMessage = Get(urlUpdateDodds, IbetConfig.URL_NEW_MARKET,
                        "application/x-www-form-urlencoded");

                    if (updateLiveOddMessage.StatusCode == HttpStatusCode.OK &&
                        !string.IsNullOrEmpty(updateLiveOddMessage.Result))
                    {
                        string updateTime;
                        ConvertUpdateData(updateLiveOddMessage.Result, out updateTime,
                             DataContainer.NonLiveMatchOddBag);
                        ParamContainer["NON_LIVE_CT"].KeyValue = updateTime;
                        UpdateException(this);
                    }
                    else
                    {
                        //Logger.Error("IBET: END->ProcessUpdateNonLiveData -> FAIL");
                        Logger.Error("MESSAGE: " + updateLiveOddMessage.Result);
                        UpdateException(this, eExceptionType.RequestFail);
                    }
                }
            }
            catch (Exception ex)
            {
                //UpdateException(this, eExceptionType.RequestFail);
                Logger.Error(ex);
            }
            //UpdateException(this, eExceptionType.RequestFail);
        }

        public void LogOff()
        {
            this.Dispose();

            if (objCheckLoginTimer != null)
            {
                objCheckLoginTimer.Dispose();
            }

            this.RemoveCookie();
            UrlHost = string.Empty;
            Host = string.Empty;
            AvailabeCredit = 0;
            CashBalance = 0;
            Status = eServiceStatus.Unknown;
            AccountStatus = eAccountStatus.Offline;
            FireLogOffEvent();
            UpdateException(this);
        }

        public void UpdateAvailabeCredit()
        {
            AvailabeCredit = GetAvailabeCredit();
        }

        public BlockingCollection<MatchDTO> ConvertFullData(string data, out string updateTime, bool isLive)
        {
            BlockingCollection<MatchDTO> list = new BlockingCollection<MatchDTO>();
            updateTime = string.Empty;
            if (!string.IsNullOrEmpty(data))
            {
                //list = new List<MatchOddDTO>();
                data = JavaScriptConvert.CleanScriptTag(data);
                string[] arrayData = data.Split(new[] { ';' }, StringSplitOptions.None);

                string jarrayName = isLive ? "Nl" : "Nt";
                //string[] array2 = array;
                for (int i = 0; i < arrayData.Length; i++)
                {
                    string text = arrayData[i];

                    if (text.StartsWith(jarrayName))
                    {
                        string value = text.Split(new string[]
                        {
                            "]="
                        }, StringSplitOptions.None)[1];

                        this.ConvertNewMatch(value, ref list);
                    }
                    else if (text.StartsWith("window"))
                    {
                        updateTime = text.Substring(43, 19);
                    }
                }
            }
            return list;
        }

        public void ConvertUpdateData(string data, out string updateTime, BlockingCollection<MatchDTO> sourceData)
        {
            updateTime = string.Empty;
            data = JavaScriptConvert.CleanScriptTag(data);
            string[] arrayData = data.Split(new[] { ';' }, StringSplitOptions.None);

            for (int i = 0; i < arrayData.Length; i++)
            {
                string textData = arrayData[i];
                if (textData.StartsWith("uOl"))
                {
                    string valueNewOdd = textData.Split(new[]
							{
								"]="
							}, StringSplitOptions.None)[1];


                    JArray jArrayNewOdds = JavaScriptConvert.DeserializeObject(valueNewOdd);
                    int oddTypeValue = (int)jArrayNewOdds[1];

                    if (!IbetConfig.ODDTYPE_VALUES_UPDATED.Contains(oddTypeValue))
                    {
                        continue;
                    }

                    if (string.IsNullOrEmpty((string)jArrayNewOdds[4]) || string.IsNullOrEmpty((string)jArrayNewOdds[4]))
                    {
                        continue;
                    }

                    MatchDTO oldMatchOddDto =
                        sourceData.FirstOrDefault(mo => mo.MatchID == jArrayNewOdds[0].ToString());

                    if (oldMatchOddDto != null)
                    {
                        if (oldMatchOddDto.Odds == null)
                        {
                            oldMatchOddDto.Odds = new List<OddDTO>(){new OddDTO()
                            {
                                OddID = jArrayNewOdds[2].ToString()
                            }};
                        }
                        var oddDto = oldMatchOddDto.Odds.FirstOrDefault(o => o.OddID == jArrayNewOdds[2].ToString());

                        switch (oddTypeValue)
                        {
                            case 1:
                                oddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString(), jArrayNewOdds[6].ToString());
                                oddDto.OddType = eOddType.HCP;
                                break;
                            case 3:
                                oddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString());
                                oddDto.OddType = eOddType.OU;
                                break;
                            case 7:
                                oddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString(), jArrayNewOdds[6].ToString());
                                oddDto.OddType = eOddType.HalfHCP;
                                break;
                            case 8:
                                oddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString());
                                oddDto.OddType = eOddType.HalfOU;
                                break;
                        }
                        //oddDto.HomeOdd = (float)jArrayNewOdds[4];
                        //oddDto.AwayOdd = (float)jArrayNewOdds[5];
                        float hOdd;
                        float aOdd;
                        float.TryParse(jArrayNewOdds[4].ToString(), out hOdd);
                        float.TryParse(jArrayNewOdds[5].ToString(), out aOdd);
                        oddDto.HomeOdd = hOdd;
                        oddDto.AwayOdd = aOdd;
                    }
                    else
                    {
                        Logger.Warn("??????????????????KHONG TIM THAY THONG TIN DATA MATCH");
                    }
                }
                else if (textData.StartsWith("uLl"))
                {
                    string valueNewTime = textData.Split(new[] { "]=" }, StringSplitOptions.None)[1];


                    JArray jArrayTime = JavaScriptConvert.DeserializeObject(valueNewTime);

                    int min;
                    var timeType = ConvertMatchTime(jArrayTime[1].ToString(), out min);

                    sourceData.Where(x => x.MatchID == jArrayTime[0].ToString()).Select(m =>
                    {
                        m.TimeType = timeType;
                        m.Minutes = min;
                        return m;
                    }).ToList();
                }
                else if (textData.StartsWith("Insl"))
                {
                    string valueNewMatch = textData.Split(new[]
                        {
                            "]="
                        }, StringSplitOptions.None)[1];
                    this.ConvertNewMatch(valueNewMatch, ref sourceData);
                }
                else if (textData.StartsWith("var Dell"))
                {
                    JArray jArrayDeleteId = JavaScriptConvert.DeserializeObject(textData.Split(new[]
                        {
                            '='
                        }, StringSplitOptions.None)[1]);
                    var deletedIds = jArrayDeleteId.Select(deletedId => (string)deletedId);

                    sourceData.Where(s => deletedIds.Contains(s.MatchID)).Select(c =>
                    {
                        c.IsDeleted = true;
                        c.Odds.Select(o =>
                        {
                            o.IsDeleted = true;
                            return o;
                        }).ToList();
                        return c;
                    }).ToList();

                    // sourceData.RemoveAll(x => deletedIds.Contains(x.MatchID));

                }
                else if (textData.StartsWith("window"))
                {
                    updateTime = textData.Substring(43, 19);
                }
            }
        }

        public bool CheckLogin()
        {
            if (!string.IsNullOrEmpty(UrlHost) && !string.IsNullOrEmpty(Host))
            {
                return true;
            }
            return false;
        }

        public AccProfileDTO GetAccountProfile()
        {
            AccProfileDTO accProfileDto = new AccProfileDTO();
            accProfileDto.UrlHost = Host;
            accProfileDto.Username = UserName;

            string accRequestParam = "accountUpdate=mini";
            var result = Post(IbetConfig.URL_GET_ACCOUNT_INFO, IbetConfig.URL_LEFT_ALL_IN_ONE, accRequestParam,
                "text/html; charset=utf-8");

            if (result.StatusCode == HttpStatusCode.OK &&
               !string.IsNullOrEmpty(result.Result))
            {
                var avIndex = result.Result.IndexOf("txt_betcredit=");
                var avIndexEnd = result.Result.IndexOf(";", avIndex);
                var a2 = result.Result.Substring(avIndex + 16, avIndexEnd - (avIndex + 17));
                accProfileDto.AvailabeCredit = Convert.ToSingle(a2);
            }

            return accProfileDto;
        }

        public float GetAvailabeCredit()
        {
            //$M('page-head').onUpdateBetCreditCallback('70.90');
            float credit = 0;
            string accRequestParam = "accountUpdate=mini";
            var result = Post(IbetConfig.URL_GET_ACCOUNT_INFO, IbetConfig.URL_LEFT_ALL_IN_ONE, accRequestParam,
                "text/html; charset=utf-8");

            if (result.StatusCode == HttpStatusCode.OK &&
               !string.IsNullOrEmpty(result.Result))
            {
                var avIndex = result.Result.IndexOf("txt_betcredit=");
                var avIndexEnd = result.Result.IndexOf(";", avIndex);
                var a2 = result.Result.Substring(avIndex + 16, avIndexEnd - (avIndex + 17));
                return Convert.ToSingle(a2);
            }
            else
            {
                UpdateException(this, eExceptionType.RequestFail);
                //return 0;
            }
            return credit;

        }

        public string GetBetList()
        {
            string urlGetBetList = IbetConfig.URL_GET_BETLIST;

            var msg = Get(urlGetBetList, null, "text/html; charset=utf-8");
            return msg.Result;
        }

        public string GetStatement(DateTime date)
        {
            var fdate = date.ToString("M/d/yyyy");
            string urlGetBetList = IbetConfig.URL_GET_STATEMENT +
                                   "type=SB&datatype=1&fdate=" + fdate;
            //string urlGetBetListRefer = IbetConfig.URL_GET_STATEMENT_REFER +
            //                            "datatype=3&fdate=" + today;
            var msg = Get(urlGetBetList, null, "text/html; charset=utf-8");
            return msg.Result;
        }

        private void CheckLoginRequest()
        {
            try
            {
                //string paramLogin = "key=login&username=" + UserName;
                string paramLogin = "key=" + GetKey(UserName, this.Host, "login") + "&username=" + UserName;
                var loginMessage = Post(IbetConfig.URL_CHECK_LOGIN, IbetConfig.URL_CHECK_LOGIN_REFER, paramLogin,
                    "application/x-www-form-urlencoded");
                if (loginMessage.StatusCode == HttpStatusCode.OK)
                {
                    if (string.IsNullOrEmpty(loginMessage.Result))
                    {
                        Logger.Error("REQUEST FAIL, MESSAGE EMTRY" + IbetConfig.URL_CHECK_LOGIN + paramLogin);
                        UpdateException(this, eExceptionType.RequestFail);
                    }
                    else if (loginMessage.Result.Contains("logout") || loginMessage.Result.Contains("closeAllWindows"))
                    {
                        Logger.Error("REQUEST LOGOUT -> MESSAGE: " + loginMessage.Result);

                        UpdateException(this, eExceptionType.LoginFail);
                        return;
                    }
                    UpdateException(this);
                }
                else
                {
                    Logger.Error(string.Format("REQUEST FAIL. REQUEST STATUS: {0}, MESSAGE: {1}",
                        loginMessage.StatusDescription, loginMessage.Result));
                    UpdateException(this, eExceptionType.RequestFail);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private SendResponse Get(string urlRequest, string urlRefer = "", string contentType = "")
        {
            urlRequest = this.UrlHost + urlRequest;
            if (!string.IsNullOrEmpty(urlRefer))
            {
                urlRefer = this.UrlHost + urlRefer;
            }

            return SendIbet(urlRequest, "GET", userAgent, CookieContainer,
                null, Host, accept, urlRefer, contentType);
        }

        private SendResponse Post(string urlRequest, string urlRefer, string param, string contentType = "")
        {
            byte[] byteArrayData = Encoding.UTF8.GetBytes(param);
            urlRequest = this.UrlHost + urlRequest;
            urlRefer = this.UrlHost + urlRefer;
            return SendIbet(urlRequest, "POST", userAgent, CookieContainer,
                byteArrayData, Host, accept, urlRefer, contentType);
        }

        #region [private methods]

        private const int ODD_INDEX = 1;
        private void ConvertNewMatch(string data, ref  BlockingCollection<MatchDTO> matchsSource)
        {
            JArray javaScriptArray = JavaScriptConvert.DeserializeObject(data);
            //MatchDTO matchDTO;

            MatchDTO ma = matchsSource.FirstOrDefault(m => m.ID == javaScriptArray[1].ToString());

            if (ma == null)
            {
                ma = new MatchDTO();
                int minute;
                ma.ID = javaScriptArray[1].ToString();
                ma.MatchID = javaScriptArray[0].ToString();
                ma.ServerType = eServerType.Ibet;
                ma.TimeType = ConvertMatchTime(javaScriptArray[12].ToString(), out  minute);
                ma.Minutes = minute;
                ma.HomeTeamName = CleanTeamName(javaScriptArray[6].ToString());
                ma.AwayTeamName = CleanTeamName(javaScriptArray[7].ToString());
                ma.Odds = new List<OddDTO>();

                if (javaScriptArray[4].ToString() == string.Empty)
                {
                    var lMa = matchsSource.Last();
                    ma.LeagueID = lMa.LeagueID;
                    ma.LeagueName = lMa.LeagueName;
                }
                else
                {
                    ma.LeagueID = javaScriptArray[4].ToString();
                    ma.LeagueName = javaScriptArray[5].ToString();
                }

                matchsSource.Add(ma);
            }

            ma.IsDeleted = false;


            //if (javaScriptArray[3].ToString() == "1")
            //{

            //    string objId;
            //    string leagueID;
            //    string leagueName;
            //    int minute;
            //    eTimeMatchType timeType;
            //    if (javaScriptArray[4].ToString() == string.Empty)
            //    {
            //        //MatchOddDTO matchDTO2 = matchsSource[matchsSource.Count - 1];
            //        //MatchOddDTO matchDTO2 = matchsSource.FirstOrDefault(m => m.ID == javaScriptArray[1].ToString()
            //        //    && m.IsDeleted);
            //        //objId = matchDTO2.ID;
            //        //leagueID = matchDTO2.LeagueID;
            //        //leagueName = matchDTO2.LeagueName;
            //        //timeType = matchDTO2.TimeType;
            //        //minute = matchDTO2.Minutes;
            //    }
            //    else
            //    {
            //        leagueID = javaScriptArray[4].ToString();
            //        leagueName = javaScriptArray[5].ToString();
            //        timeType = ConvertMatchTime(javaScriptArray[12].ToString(), out  minute);
            //    }
            //    matchDTO = new MatchDTO();


            //    //matchDTO = new MatchDTO(javaScriptArray[1].ToString(), javaScriptArray[0].ToString(),
            //    //    eServerType.Ibet, leagueID, leagueName,
            //    //    CleanTeamName(javaScriptArray[6].ToString()), CleanTeamName(javaScriptArray[7].ToString()),
            //    //     timeType, minute);
            //    //matchsSource.Add(matchDTO);
            //}
            //else
            //{
            //    matchDTO = new MatchDTO();
            //    //var oldMatchDTO = matchsSource[matchsSource.Count - 1];
            //    //matchDTO = new MatchOddDTO(oldMatchDTO.MatchID, oldMatchDTO.ServerType, oldMatchDTO.LeagueID,
            //    //    oldMatchDTO.LeagueName, oldMatchDTO.HomeTeamName, oldMatchDTO.AwayTeamName, oldMatchDTO.TimeType, oldMatchDTO.Minutes);
            //    //matchsSource.Add(matchDTO);
            //}

            //var newObj = Utils.Clone(matchDTO);
            ////FULLTIME Chau A
            if (javaScriptArray[26 + ODD_INDEX].ToString().Length != 0)
            {
                UpdateOdd(ma, javaScriptArray[26 + ODD_INDEX].ToString(),
                     eOddType.HCP,
                     javaScriptArray[27 + ODD_INDEX].ToString(), javaScriptArray[28 + ODD_INDEX].ToString(),
                     javaScriptArray[29 + ODD_INDEX].ToString(), javaScriptArray[30 + ODD_INDEX].ToString());
                //matchsSource.Add(newObj1);
            }
            ////FULLTIME TAI XIU
            if (javaScriptArray[31 + ODD_INDEX].ToString().Length != 0)
            {
                UpdateOdd(ma, javaScriptArray[31 + ODD_INDEX].ToString(),
                    eOddType.OU,
                    javaScriptArray[32 + ODD_INDEX].ToString(), javaScriptArray[33 + ODD_INDEX].ToString(),
                    javaScriptArray[34 + ODD_INDEX].ToString());
                //matchsSource.Add(newObj1);
            }
            /////H1 Chau A
            if (javaScriptArray[39 + ODD_INDEX].ToString().Length != 0)
            {
                UpdateOdd(ma, javaScriptArray[39 + ODD_INDEX].ToString(),
                    eOddType.HalfHCP,
                    javaScriptArray[40 + ODD_INDEX].ToString(), javaScriptArray[41 + ODD_INDEX].ToString(),
                    javaScriptArray[42 + ODD_INDEX].ToString(), javaScriptArray[43 + ODD_INDEX].ToString());
                //matchsSource.Add(newObj1);
            }
            /////H1 TAI XIU
            if (javaScriptArray[44 + ODD_INDEX].ToString().Length != 0)
            {
                UpdateOdd(ma, javaScriptArray[44 + ODD_INDEX].ToString(),
                    eOddType.HalfOU,
                    javaScriptArray[45 + ODD_INDEX].ToString(), javaScriptArray[46 + ODD_INDEX].ToString(),
                    javaScriptArray[47 + ODD_INDEX].ToString());
                //matchsSource.Add(newObj1);
            }
        }

        private void UpdateOdd(MatchDTO matchDTO, string oddID, eOddType oddType, string odd, string home, string away, string checkHorA = "")
        {
            float homeOdd = 0f;
            float awayOdd = 0f;
            float.TryParse(home, out homeOdd);
            float.TryParse(away, out awayOdd);
            float oddValue = ConvertOdd(odd, checkHorA);

            OddDTO odDto = matchDTO.Odds.FirstOrDefault(m => m.OddID == oddID);
            if (odDto == null)
            {
                odDto = new OddDTO();
                matchDTO.Odds.Add(odDto);
            }
            odDto.IsDeleted = false;
            odDto.OddID = oddID;
            odDto.OddType = oddType;
            odDto.Odd = oddValue;
            odDto.HomeOdd = homeOdd;
            odDto.AwayOdd = awayOdd;
        }

        private float ConvertOdd(string oldOdd, string checkHorA = "")
        {
            float oddValue = 0f;

            if (oldOdd.Contains('-'))
            {
                var oddSplit = oldOdd.Split(new char[] { '-' }, StringSplitOptions.None);
                oddValue = (float.Parse(oddSplit[0]) + float.Parse(oddSplit[1])) / 2;
                oddValue = Math.Abs(oddValue);
                if (checkHorA == "a")
                {
                    oddValue = oddValue * -1;
                }
            }
            else
            {
                float.TryParse(oldOdd, out oddValue);
            }

            return oddValue;
        }

        private eTimeMatchType ConvertMatchTime(string value, out int minutes)
        {
            minutes = 0;
            if (!string.IsNullOrEmpty(value))
            {
                //var timeType = eTimeMatchType.Undefined;
                switch (value)
                {
                    case "H.Time":
                        return eTimeMatchType.Break;
                    default:
                        if (value.StartsWith("1H"))
                        {
                            minutes = ConvertMinutesMatch(value);
                            return eTimeMatchType.H1;
                        }

                        if (value.StartsWith("2H"))
                        {
                            minutes = ConvertMinutesMatch(value);
                            return eTimeMatchType.H2;
                        }

                        return eTimeMatchType.Undefined;
                    //break;
                }
            }

            return eTimeMatchType.Undefined;
        }

        private int ConvertMinutesMatch(string value)
        {
            value = value.Replace("\'", "");
            var values = value.Split();
            return Convert.ToInt32(values[1]);
        }

        private void WaitScanCallback(object obj)
        {
            //UpdateDataChange(new UpdatedData(){DeleteMatchIDs = new List<string>(){"123"}});
            //var aaa = Get(IbetConfig.URL_NEW_MARKET);
            //this.waitingStorage.Scans(this.config.Timeout);
            if ((bool)obj)
            {
                ////Process Live Update
                ProcessUpdateLiveData();
                //UpdateLiveDataChange(ProcessUpdateLiveData(), true);
            }
            else
            {
                ////Process Non Live Update
                ProcessUpdateNonLiveData();
                //UpdateNonLiveDataChange(ProcessUpdateNonLiveData(), false);
            }
        }

        private void CheckLoginCallback(object obj)
        {
            CheckLoginRequest();
        }

        #endregion
    }
}
