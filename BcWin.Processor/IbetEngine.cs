using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using BcWin.Common;
using BcWin.Common.DTO;
using BcWin.Common.Objects;
using BcWin.Contract;
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
    public partial class IbetEngine : IbetHelper, IEngine
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(IbetEngine));


        //public event UpdateDataHandler UpdateLiveDataChange;
        //public event UpdateDataHandler UpdateNonLiveDataChange;
        //public delegate void UpdateDataHandler(List<MatchOddDTO> m, bool isLive);

        public Dictionary<string, ParamRequest> ParamContainer { get; set; }
        public PrepareBetIbet BetMessageQueue { get; set; }
        public List<BetAgainstTransaction> BetAgainstTransactions { get; set; }
   
        private const string userAgent =
            "Mozilla/5.0 (Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.130 Safari/537.36";
        //private const string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0";
        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        private System.Threading.Timer objLiveScanTimer;
        private System.Threading.Timer objNonLiveScanTimer;
        private System.Threading.Timer objCheckLoginTimer;

        //public event EventHandler OnReConnectFail;
        //public event EventHandler OnReConnectSuccess;

        #region [constructor]
        public IbetEngine()
        {
            BetAgainstTransactions = new List<BetAgainstTransaction>();
            ParamContainer = new Dictionary<string, ParamRequest>();
            //LastPrepareTime = DateTime.Now;
            ////LastPrepareFail = DateTime.Now.AddHours(-1);
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
            if (CookieContainer == null)
            {
                ReLogin();
            }

            //var handler = new HttpClientHandler
            //{
            //    CookieContainer = CookieContainer,
            //    AutomaticDecompression = DecompressionMethods.GZip,
            //    UseProxy = false,
            //};
            //httpClient = new HttpClient(handler, false);
            ////httpClient.MaxResponseContentBufferSize = 256000;
            //httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:36.0) Gecko/20100101 Firefox/36.0");
            //httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            //httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
            //var urlRefer = this.UrlHost + IbetConfig.URL_LEFT_ALL_IN_ONE;
            //httpClient.DefaultRequestHeaders.Add("Referer", urlRefer);

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

            if (BetAgainstTransactions.Any())
            {
                Logger.Info("BAT DAU DAT NGUOC IBET SAU KHI KHOI DONG>>>>>>>>>>>>>>>>>" + UrlHost);
                var tran = BetAgainstTransactions.First();

                MatchOddDTO matchOdd;
                if (tran.IsLive)
                {
                    matchOdd =
                        LiveMatchOddDatas.FirstOrDefault(
                            m => m.Odd == tran.MatchOdd.Odd && m.OddType == tran.MatchOdd.OddType &&
                                                                           m.HomeTeamName == tran.MatchOdd.HomeTeamName);
                }
                else
                {
                    matchOdd =
                        NoneLiveMatchOddDatas.FirstOrDefault(
                            m => m.Odd == tran.MatchOdd.Odd && m.OddType == tran.MatchOdd.OddType &&
                                                                           m.HomeTeamName == tran.MatchOdd.HomeTeamName);
                }

                if (matchOdd != null)
                {
                    try
                    {
                        var prepareBet = PrepareBet(matchOdd, tran.BetType, tran.IsLive, tran.OddCompare, true);
                        string ibetConfirmMsg;
                        if (prepareBet.HomeScore == tran.HomeScore
                        && prepareBet.AwayScore == tran.AwayScore)
                        {
                            ConfirmBet(tran.Stake, out ibetConfirmMsg, true);
                        }
                        else
                        {
                            Logger.Info("BET MGUOC IBET FAIL....TY SO THAY DOI....");
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                    }
                }
                else
                {
                    Logger.Error("????????????? KHONG TIM THAY THONG TIN TRAN DAU DAT NGUOC");
                }

                BetAgainstTransactions.RemoveAll(m => true);
            }
            ScanType = scanType;
            Logger.Info("STARTED: " + UrlHost);
            Status = eServiceStatus.Started;
        }

        public void StartScanTEST()
        {
            ////Get Live data
            string urlInitLodds =
                IbetConfig.URL_INIT_LODDS_MARKET
                + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue + "&_=" + Utils.GetUnixTimestamp();
            ParamContainer["LIVE_CT"] = new ParamRequest("CT", string.Empty);
            string updateTimeLive;

            var lOddMessage = Get(urlInitLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            //var lOddMessage = GetClient(urlInitLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
            LiveMatchOddDatas = ConvertFullData(lOddMessage.Result, out updateTimeLive, true);
            ParamContainer["LIVE_CT"].KeyValue = updateTimeLive;
            //objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
            //    SystemConfig.TIME_GET_UPDATE_LIVE_IBET, SystemConfig.TIME_GET_UPDATE_LIVE_IBET);
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
            LiveMatchOddDatas = ConvertFullData(lOddMessage.Result, out updateTimeLive, true);
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
            NoneLiveMatchOddDatas = ConvertFullData(dOddMessage.Result, out updateTimeNonLive, false);
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

        //public void ReStartEngine(eScanType scanType)
        //{
        //    //StartScanEngine(scanType);
        //    objCheckLoginTimer = new System.Threading.Timer(CheckLoginCallback, null, SystemConfig.TIME_CHECK_LOGIN_IBET / 2,
        //        SystemConfig.TIME_CHECK_LOGIN_IBET);
        //}

        public void Dispose()
        {
            PauseScan();
        }

        //private string LastPrepareMatchId { get; set; }
        //private string LastPrepareOddId { get; set; }
        //private DateTime LastPrepareTime { get; set; }
        //private DateTime LastPrepareFail { get; set; }
        //private float LastBetOdd { get; set; }
        public PrepareBetDTO PrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float sboOddDef, bool isBetAgainst = false)
        {
            eOddType oType = matchOdd.OddType;

            string bpType;
            float betOdd;
            switch (betType)
            {
                case eBetType.Home:
                    bpType = "h";
                    betOdd = matchOdd.HomeOdd;
                    break;
                case eBetType.Away:
                    bpType = "a";
                    betOdd = matchOdd.AwayOdd;
                    break;
                default:
                    throw new Exception("PrepareBet => FAIL : Unknow betType param");
            }
            //LastBetOdd = betOdd;
            string oddType;
            switch (oType)
            {
                case eOddType.HCP:
                    oddType = "1";
                    break;
                case eOddType.OU:
                    oddType = "3";
                    break;
                case eOddType.HalfHCP:
                    oddType = "7";
                    break;
                case eOddType.HalfOU:
                    oddType = "8";
                    break;
                default:
                    throw new Exception("PrepareBet => FAIL : Unknow matchOdd->OddType param");
            }
            var bpMatch = string.Concat(matchOdd.OddID, "_", bpType, "_", betOdd, "_", oddType);
            //var key = GetKey(this.UserName, bpMatch);
            StringBuilder urlPrepareBet = new StringBuilder(UrlHost);
            urlPrepareBet.Append(IbetConfig.URL_BET_PREPARE);
            urlPrepareBet.Append("bp_Match=");
            urlPrepareBet.Append(bpMatch);
            urlPrepareBet.Append("&UN=");
            urlPrepareBet.Append(UserName);
            urlPrepareBet.Append("&bp_ssport=&chk_BettingChange=4&");
            urlPrepareBet.Append(ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue);
            urlPrepareBet.Append("&bp_key=");
            urlPrepareBet.Append(GetKey(UserName, Host, "bet", "58557844_h_0.92_3"));
            //urlPrepareBet.Append("&bp_key=bet");
            //+ "&bp_key=" + key
            urlPrepareBet.Append("&_=" + Utils.GetUnixTimestamp());

            //var messagePrepareBet = Get(urlPrepareBet.ToString(), IbetConfig.URL_LEFT_ALL_IN_ONE,
            //    "application/x-www-form-urlencoded");

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(urlPrepareBet.ToString());
                httpRequest.Referer = string.Concat(this.UrlHost, IbetConfig.URL_LEFT_ALL_IN_ONE);
                httpRequest.KeepAlive = true;
                httpRequest.Timeout = 10000;
                httpRequest.CookieContainer = this.CookieContainer;
                //httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                httpRequest.Method = "GET";
                httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpRequest.UserAgent = userAgent;
                httpRequest.AllowAutoRedirect = false;
                httpRequest.ServicePoint.Expect100Continue = false;
                httpRequest.Proxy = null;
                //httpRequest.AutomaticDecompression = DecompressionMethods.GZip;


                HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();

                //Stream responseStream = response.GetResponseStream();
                Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);

                string result;

                using (StreamReader reader = new StreamReader(responseStream))
                {
                    result = reader.ReadToEnd();
                }

                if (response != null)
                {
                    response.Close();
                }

                if (isBetAgainst)
                {
                    Logger.InfoFormat("IBET BET AGAINST: Message {0} ", result);
                }

                if (!string.IsNullOrEmpty(result))
                {
                    PrepareBetIbet prepareBetobj = ConvertPrepareBet(result, matchOdd);

                    if (prepareBetobj != null)
                    {
                        prepareBetobj.IsSuccess = true;
                        //Logger.InfoFormat("STATUS [{0}] NEW DEF>>> {1}", prepareBetobj.BetProcessIbet.lbloddsStatus
                        //    , prepareBetobj.BetProcessIbet.lblBetKindValue);
                        if (HasOddDefNoChange(matchOdd.OddType, betType, sboOddDef, prepareBetobj.BetProcessIbet.lblBetKindValue))
                        {
                            //Logger.DebugFormat("IBET STATUS {0}", prepareBetobj.BetProcessIbet.lbloddsStatus);
                            if (prepareBetobj.BetProcessIbet.lbloddsStatus == "running")
                            {
                                if (betType == eBetType.Home)
                                {
                                    //Logger.DebugFormat("HOME OLD ODD {0} NEW ODD {1}", matchOdd.HomeOdd, prepareBetobj.NewOdd);
                                    matchOdd.HomeOdd = prepareBetobj.NewOdd;
                                }
                                else
                                {
                                    //Logger.DebugFormat("AWAY OLD ODD {0} NEW ODD {1}", matchOdd.AwayOdd, prepareBetobj.NewOdd);
                                    matchOdd.AwayOdd = prepareBetobj.NewOdd;
                                }

                                prepareBetobj.IsRunning = true;
                                prepareBetobj.BetType = betType;
                                prepareBetobj.MatchOdd = matchOdd;
                                //prepareBetobj.IsLive = isLive;

                                //BetQueue.Add(new MatchBag(matchOdd.MatchID, matchOdd.Odd, betOdd, prepareBetobj, BetQueue));
                                BetMessageQueue = prepareBetobj;
                                //Logger.Info("IBET: END->PrepareBet");
                                return prepareBetobj;
                            }

                            if (prepareBetobj.BetProcessIbet.lbloddsStatus == "closeprice" || prepareBetobj.BetProcessIbet.lbloddsStatus == "suspend")
                            {
                                ////prepareBetobj.NewOdd = 0;
                                matchOdd.HomeOdd = 0;
                                matchOdd.AwayOdd = 0;
                                return new PrepareBetDTO(true);
                            }

                            //else
                            //{
                            //    matchOdd.HomeOdd = 0;
                            //    matchOdd.AwayOdd = 0;
                            //    return null;
                            //}

                        }
                        else
                        {
                            matchOdd.Odd = prepareBetobj.BetProcessIbet.lblBetKindValue;
                            if (betType == eBetType.Home)
                            {
                                //Logger.DebugFormat("HOME OLD ODD {0} NEW ODD {1}", lastMatch.HomeOdd, prepareBetobj.NewOdd);
                                matchOdd.HomeOdd = prepareBetobj.NewOdd;
                            }
                            else
                            {
                                //Logger.DebugFormat("AWAY OLD ODD {0} NEW ODD {1}", lastMatch.AwayOdd, prepareBetobj.NewOdd);
                                matchOdd.AwayOdd = prepareBetobj.NewOdd;
                            }
                        }

                        return prepareBetobj;
                    }
                }
                //////else
                //////{
                //////    ///LastPrepareFail = DateTime.Now;
                //////    //Logger.Error("PREPARE FAIL. URL " + urlPrepareBet);
                //////    //UpdateException(this, eExceptionType.RequestFail);
                //////}

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                //UpdateException(this, eExceptionType.RequestFail);
            }


            //EngineLogger.Error("MESSAGE: " + messagePrepareBet.Result);

            //if (countPrepare == 1)
            //{
            //    countPrepare++;
            //    return PrepareBet(matchOdd, betType, isLive);
            //}
            //countPrepare = 1;
            return new PrepareBetDTO(false);
        }

        public PrepareBetDTO PrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float sboOddDef, string bpMatch)
        {
            StringBuilder urlPrepareBet = new StringBuilder(UrlHost);
            urlPrepareBet.Append(string.Concat(IbetConfig.URL_BET_PREPARE, "FromConfimBet=yes&bp_Match="));
            urlPrepareBet.Append(bpMatch);
            urlPrepareBet.Append("&UN=");
            urlPrepareBet.Append(UserName);
            urlPrepareBet.Append("&bp_ssport=&chk_BettingChange=4&");
            urlPrepareBet.Append(ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue);
            urlPrepareBet.Append("&bp_key=bet");
            //+ "&bp_key=" + key
            urlPrepareBet.Append("&_=" + Utils.GetUnixTimestamp());

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(urlPrepareBet.ToString());
            httpRequest.Referer = string.Concat(this.UrlHost, IbetConfig.URL_LEFT_ALL_IN_ONE);
            httpRequest.KeepAlive = true;
            httpRequest.Timeout = 10000;
            httpRequest.CookieContainer = this.CookieContainer;
            httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            httpRequest.Method = "GET";
            httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            httpRequest.UserAgent = userAgent;
            httpRequest.AllowAutoRedirect = false;
            httpRequest.ServicePoint.Expect100Continue = false;
            httpRequest.Proxy = null;
            //httpRequest.AutomaticDecompression = DecompressionMethods.GZip;


            HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();

            Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);

            string result;

            using (StreamReader reader = new StreamReader(responseStream))
            {
                result = reader.ReadToEnd();
            }

            response.Close();

            Logger.Info("MESSAGE GOI LAN 2 " + result);
            try
            {
                if (!string.IsNullOrEmpty(result))
                {
                    PrepareBetIbet prepareBetobj = ConvertPrepareBet(result, matchOdd);

                    if (prepareBetobj != null)
                    {
                        prepareBetobj.IsSuccess = true;
                        if (HasOddDefNoChange(matchOdd.OddType, betType, sboOddDef, prepareBetobj.BetProcessIbet.lblBetKindValue))
                        {
                            if (prepareBetobj.BetProcessIbet.lbloddsStatus == "running")
                            {
                                if (betType == eBetType.Home)
                                {
                                    matchOdd.HomeOdd = prepareBetobj.NewOdd;
                                }
                                else
                                {
                                    matchOdd.AwayOdd = prepareBetobj.NewOdd;
                                }

                                prepareBetobj.IsRunning = true;
                                prepareBetobj.BetType = betType;
                                prepareBetobj.MatchOdd = matchOdd;

                                BetMessageQueue = prepareBetobj;
                                return prepareBetobj;
                            }

                            if (prepareBetobj.BetProcessIbet.lbloddsStatus == "closeprice" || prepareBetobj.BetProcessIbet.lbloddsStatus == "suspend")
                            {
                                matchOdd.HomeOdd = 0;
                                matchOdd.AwayOdd = 0;
                                return new PrepareBetDTO(true);
                            }
                        }
                        else
                        {
                            matchOdd.Odd = prepareBetobj.BetProcessIbet.lblBetKindValue;
                            if (betType == eBetType.Home)
                            {
                                matchOdd.HomeOdd = prepareBetobj.NewOdd;
                            }
                            else
                            {
                                matchOdd.AwayOdd = prepareBetobj.NewOdd;
                            }
                        }

                        return prepareBetobj;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }

            return new PrepareBetDTO(false);
        }

        public PrepareBetDTO PrepareBetTest(MatchOddDTO matchOdd, eBetType betType, bool isLive, bool second)
        {
            eOddType oType = matchOdd.OddType;

            string bpType;
            float betOdd;
            switch (betType)
            {
                case eBetType.Home:
                    bpType = "h";
                    betOdd = matchOdd.HomeOdd;
                    break;
                case eBetType.Away:
                    bpType = "a";
                    betOdd = matchOdd.AwayOdd;
                    break;
                default:
                    throw new Exception("PrepareBet => FAIL : Unknow betType param");
            }
            //LastBetOdd = betOdd;
            string oddType;
            switch (oType)
            {
                case eOddType.HCP:
                    oddType = "1";
                    break;
                case eOddType.OU:
                    oddType = "3";
                    break;
                case eOddType.HalfHCP:
                    oddType = "7";
                    break;
                case eOddType.HalfOU:
                    oddType = "8";
                    break;
                default:
                    throw new Exception("PrepareBet => FAIL : Unknow matchOdd->OddType param");
            }
            var bpMatch = string.Concat(matchOdd.OddID, "_", bpType, "_", betOdd, "_", oddType);
            //var key = GetKey(this.UserName, bpMatch);
            StringBuilder urlPrepareBet = new StringBuilder(UrlHost);
            urlPrepareBet.Append(IbetConfig.URL_BET_PREPARE);
            if (second)
            {
                urlPrepareBet.Append("autoLoad=yes&");
            }
            urlPrepareBet.Append("bp_Match=");
            urlPrepareBet.Append(bpMatch);
            urlPrepareBet.Append("&UN=");
            urlPrepareBet.Append(UserName);
            urlPrepareBet.Append("&bp_ssport=1&chk_BettingChange=4&");
            urlPrepareBet.Append(ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue);
            urlPrepareBet.Append("&bp_key=bet");
            //+ "&bp_key=" + key
            urlPrepareBet.Append("&_=" + Utils.GetUnixTimestamp());


            //var messagePrepareBet = Get(urlPrepareBet.ToString(), IbetConfig.URL_LEFT_ALL_IN_ONE,
            //    "application/x-www-form-urlencoded");

            try
            {
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(urlPrepareBet.ToString());
                httpRequest.Referer = string.Concat(this.UrlHost, IbetConfig.URL_LEFT_ALL_IN_ONE);
                httpRequest.KeepAlive = true;
                if (!second)
                {
                    httpRequest.Timeout = 6600;
                }
                else
                {
                    httpRequest.Timeout = 2000;
                }
                //httpRequest.Headers.Add("X-Requested-With", "XMLHttpRequest");
                httpRequest.CookieContainer = this.CookieContainer;
                //httpRequest.ContentType = "text/html; charset=utf-8";
                //httpRequest.Headers.Add(HttpRequestHeader.ContentType, "text/html; charset=utf-8");
                httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
                
                //httpRequest.Headers.Add(HttpRequestHeader.AcceptLanguage, "en-us");
                httpRequest.Method = "GET";
                httpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                httpRequest.UserAgent = userAgent;
                httpRequest.AllowAutoRedirect = false;
                httpRequest.ServicePoint.Expect100Continue = false;
                httpRequest.AutomaticDecompression = DecompressionMethods.GZip;

                HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();

                //Stream responseStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);

                string result;

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }

                if (response != null)
                {
                    response.Close();
                    //responseStream.Close();
                }

                if (!string.IsNullOrEmpty(result))
                {
                    PrepareBetIbet prepareBetobj = ConvertPrepareBet(result, matchOdd);

                    if (prepareBetobj != null)
                    {
                        prepareBetobj.IsSuccess = true;

                        //Logger.DebugFormat("IBET STATUS {0}", prepareBetobj.BetProcessIbet.lbloddsStatus);
                        if (prepareBetobj.BetProcessIbet.lbloddsStatus == "running")
                        {
                            if (betType == eBetType.Home)
                            {
                                //Logger.DebugFormat("HOME OLD ODD {0} NEW ODD {1}", matchOdd.HomeOdd, prepareBetobj.NewOdd);
                                matchOdd.HomeOdd = prepareBetobj.NewOdd;
                            }
                            else
                            {
                                //Logger.DebugFormat("AWAY OLD ODD {0} NEW ODD {1}", matchOdd.AwayOdd, prepareBetobj.NewOdd);
                                matchOdd.AwayOdd = prepareBetobj.NewOdd;
                            }

                            prepareBetobj.IsRunning = true;
                            prepareBetobj.BetType = betType;
                            prepareBetobj.MatchOdd = matchOdd;
                            //prepareBetobj.IsLive = isLive;

                            //BetQueue.Add(new MatchBag(matchOdd.MatchID, matchOdd.Odd, betOdd, prepareBetobj, BetQueue));
                            BetMessageQueue = prepareBetobj;
                            //Logger.Info("IBET: END->PrepareBet");
                            return prepareBetobj;
                        }

                        if (prepareBetobj.BetProcessIbet.lbloddsStatus == "closeprice" || prepareBetobj.BetProcessIbet.lbloddsStatus == "suspend")
                        {
                            ////prepareBetobj.NewOdd = 0;
                            matchOdd.HomeOdd = 0;
                            matchOdd.AwayOdd = 0;
                            return new PrepareBetDTO(true);
                        }

                        return prepareBetobj;
                    }
                }


            }
            catch (Exception ex)
            {
                throw;
            }

            return new PrepareBetDTO(false);
        }

        private bool HasOddDefNoChange(eOddType oddType, eBetType betType, float oldDef, float newDef)
        {
            if (oddType == eOddType.OU || oddType == eOddType.HalfOU)
            {
                return oldDef == newDef;
            }
            else
            {
                if (betType == eBetType.Home)
                {
                    return oldDef + newDef == 0;
                }
                else
                {
                    return oldDef == newDef;
                }
            }

            //if (oType == eOddType.OU || oType == eOddType.HalfOU 
            //    || (oldDef >= 0 && newDef >= 0) || (oldDef < 0 && newDef < 0))
            //{
            //    return !oldDef.Equals(newDef);
            //}

            //return oldDef + newDef != 0;
        }

        private PrepareBetIbet ConvertPrepareBet(string rawData, MatchOddDTO matchOdd)
        {
            //Logger.Info(EngineName + " Prepare Msg: " + rawData);
            PrepareBetIbet prepareBetDto = new PrepareBetIbet(matchOdd);
            var msgResult = JavaScriptConvert.CleanScriptTag(rawData);
            string[] arrayData = msgResult.Split(new[]
				{
					';'
				}, StringSplitOptions.None);
            foreach (string text in arrayData)
            {
                if (text.StartsWith("var Data"))
                {
                    string value = text.Split(new[]
                    {
                        "Data="
                    }, StringSplitOptions.None)[1];
                    var betProcessIbet = JavaScriptConvert.DeserializeObject<BetProcessIbet>(value);

                    prepareBetDto.MinBet = Convert.ToInt32(betProcessIbet.hiddenMinBet.Replace(",", ""));
                    prepareBetDto.MaxBet = Convert.ToInt32(betProcessIbet.hiddenMaxBet.Replace(",", ""));
                    //prepareBetDto.BetType = betType;
                    //prepareBetDto.MatchOdd = matchOdd;
                    //prepareBetDto.IsLive = isLive;

                    //if (!betProcessIbet.lblOddsValue.Equals(betProcessIbet.hiddenOddsRequest))
                    //{
                    //prepareBetDto.HasChangeOdd = !betProcessIbet.lblOddsValue.Equals(betProcessIbet.hiddenOddsRequest);
                    //}
                    //prepareBetDto.OddDef = betProcessIbet.lblBetKindValue;

                    prepareBetDto.NewOdd = betProcessIbet.lblOddsValue;

                    if (betProcessIbet.lblScoreValue != "")
                    {
                        prepareBetDto.HasScore = true;
                        var score = betProcessIbet.lblScoreValue.Replace("[", "").Replace("]", "");
                        var scoreArray = score.Split(new[] { '-' }, StringSplitOptions.None);
                        prepareBetDto.HomeScore = Convert.ToInt32(scoreArray[0]);
                        prepareBetDto.AwayScore = Convert.ToInt32(scoreArray[1]);
                    }
                    prepareBetDto.BetProcessIbet = betProcessIbet;
                    return prepareBetDto;
                }
            }
            return null;
        }

        public bool ConfirmBet(int stake, out string ibetMsg, bool betAgainst = false,
           eServerScan serverScan = eServerScan.Local)
        {
            bool isSuccess = false;
            ibetMsg = null;
            var firstBetMsg = ConfirmBetRequest(stake, betAgainst);

            if (firstBetMsg.Contains("Success") || firstBetMsg.Contains("BListMini"))
            {
                isSuccess = true;
            }
            else if (firstBetMsg.Contains("BetC"))
            {
                ibetMsg = firstBetMsg;
            }
            //Logger.Info("IBET CONFIRM MESSAGE: " + firstBetMsg);
            if (isSuccess)
            {
                if (betAgainst)
                {
                    this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, eBetStatusType.BetAgainstIbet, serverScan);
                }
                else
                {
                    this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, eBetStatusType.Success, serverScan);
                }
            }
            else
            {
                this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, eBetStatusType.Fail, serverScan);
            }
            BetMessageQueue = null;
            return isSuccess;
        }

        public bool ConfirmBetTest(int stake, PrepareBetIbet prepareBet)
        {
            bool isSuccess = false;
            var firstBetMsg = ConfirmBetRequestTest(stake, prepareBet);

            if (firstBetMsg.Contains("Success") || firstBetMsg.Contains("BListMini"))
            {
                isSuccess = true;
            }

            BetMessageQueue = null;
            return isSuccess;
        }

        private string ConfirmBetRequestTest(int stake, PrepareBetIbet prepareBet)
        {
            string paramConfirm;

            paramConfirm =
                   "FScreen=&HorseBPBetKey=&HorseBPstake=&siteType=&stakeRequest=";

            paramConfirm = paramConfirm
                 + "&BPBetKey=" + prepareBet.BetProcessIbet.hiddenBetKey
                 + "&MAXBET=" + prepareBet.BetProcessIbet.hiddenMaxBet
                 + "&MINBET=" + prepareBet.BetProcessIbet.hiddenMinBet
                 + "&bettype=" + prepareBet.BetProcessIbet.hiddenBetType
                 + "&areyousuremesg=" + ParamContainer["areyousuremesg"].KeyValue
                 + "&areyousuremesg1=" + ParamContainer["areyousuremesg1"].KeyValue
                 + "&betconfirmmesg=" + ParamContainer["betconfirmmesg"].KeyValue
                 + "&btnBPSubmit=" + ParamContainer["btnBPSubmit"].KeyValue
                 + "&hidStake10=" + ParamContainer["hidStake10"].KeyValue
                 + "&hidStake2=" + ParamContainer["hidStake2"].KeyValue
                 + "&hidStake20=" + ParamContainer["hidStake20"].KeyValue
                 + "&highermaxmesg=" + ParamContainer["highermaxmesg"].KeyValue
                 + "&incorrectStakeMesg=" + ParamContainer["incorrectStakeMesg"].KeyValue
                 + "&lowerminmesg=" + ParamContainer["lowerminmesg"].KeyValue
                 + "&oddChange1=" + ParamContainer["oddChange1"].KeyValue
                 + "&oddChange2=" + ParamContainer["oddChange2"].KeyValue
                 + "&oddsWarning=" + ParamContainer["oddsWarning"].KeyValue
                 + "&oddsType=" + prepareBet.BetProcessIbet.hiddenOddsType
                 + "&sporttype=" + prepareBet.BetProcessIbet.hiddenSportType
                 + "&oddsRequest=" + prepareBet.BetProcessIbet.lblOddsValue
                 + "&username=" + UserName
                 + "&BPstake=" + String.Format("{0:n0}", stake);
            //Logger.Info(EngineName + " ConfirmBetRequest: " + paramConfirm);
            var lconfirmBetMessage = Post(IbetConfig.URL_BET_CONFIRM, IbetConfig.URL_LEFT_ALL_IN_ONE, paramConfirm,
                "application/x-www-form-urlencoded");
            Logger.Info("MESSAGE: " + lconfirmBetMessage.Result);
            //Logger.Info("IBET CONFIRM MESSAGE: " + lconfirmBetMessage.Result);
            return lconfirmBetMessage.Result;
        }

        private string ConfirmBetRequest(int stake, bool betAgainst)
        {
            StringBuilder paramRequest = new StringBuilder("BPstake=");
            paramRequest.Append(String.Format("{0:n0}", stake));
            paramRequest.Append("&stakeRequest=&oddsRequest=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.lblOddsValue);
            paramRequest.Append("&oddChange1=Odds+has+changed+from&oddChange2=to&MINBET=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.hiddenMinBet);
            paramRequest.Append("&MAXBET=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.hiddenMaxBet);
            paramRequest.Append("&bettype=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.hiddenBetType);
            paramRequest.Append("&lowerminmesg=Your+stake+is+lower+than+minimun+bet%21%21%21&highermaxmesg=Your+stake+is+higher+than+maximum+bet%21%21%21&areyousuremesg=Are+you+sure+you+want+process+the+bet%3F&incorrectStakeMesg=Incorrect+Stake.&oddsWarning=WARNING%21%21%21+WE+HAVE+GIVEN+A+NEW+ODDS+%26+NEW+STAKE%21%21%21&betconfirmmesg=Please+click+OK+to+confirm+the+bet%3F&siteType=&hidStake10=Stake+must+be+in+multiples+of+10&hidStake20=Stake+must+be+in+multiples+of+20&sporttype=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.hiddenSportType);
            paramRequest.Append("&username=");
            paramRequest.Append(UserName);
            paramRequest.Append("&oddsType=");
            paramRequest.Append(BetMessageQueue.BetProcessIbet.hiddenOddsType);
            byte[] byteArrayData = Encoding.UTF8.GetBytes(paramRequest.ToString());

            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(string.Concat(this.UrlHost, IbetConfig.URL_BET_CONFIRM));
            httpRequest.Referer = string.Concat(this.UrlHost, IbetConfig.URL_LEFT_ALL_IN_ONE);
            httpRequest.KeepAlive = true;
            httpRequest.CookieContainer = this.CookieContainer;
            httpRequest.Timeout = 10000;
            httpRequest.Proxy = null;
            httpRequest.ContentType = "application/x-www-form-urlencoded";
            httpRequest.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            httpRequest.Method = "POST";
            httpRequest.Accept = "text/plain, */*; q=0.01";
            httpRequest.UserAgent = userAgent;
            httpRequest.AllowAutoRedirect = false;
            httpRequest.ServicePoint.Expect100Continue = false;
            httpRequest.ContentLength = byteArrayData.Length;
            httpRequest.AutomaticDecompression = DecompressionMethods.GZip;
            // Write data  
            using (Stream stream = httpRequest.GetRequestStream())
            {
                stream.Write(byteArrayData, 0, byteArrayData.Length);
            }

            HttpWebResponse response = (HttpWebResponse)httpRequest.GetResponse();

            string result;

            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                result = reader.ReadToEnd();
            }

            if (response != null)
            {
                response.Close();
            }

            //var lconfirmBetMessage = Post(IbetConfig.URL_BET_CONFIRM, IbetConfig.URL_LEFT_ALL_IN_ONE, paramConfirm.ToString(),
            //    "application/x-www-form-urlencoded");
            Logger.Info("MESSAGE: " + result);
            //Logger.Info("IBET CONFIRM MESSAGE: " + lconfirmBetMessage.Result);
            return result;
        }

        public List<MatchOddDTO> ProcessUpdateLiveData()
        {
            try
            {
                lock (LockLive)
                {
                    string urlUpdateLodds = string.Concat(
                        IbetConfig.URL_UPDATE_LODDS_MARKET,
                        ParamContainer["LIVE_CT"].KeyName + "=" + HttpUtility.UrlEncode(ParamContainer["LIVE_CT"].KeyValue),
                        "&key=", GetKey(UserName, this.Host, "lodds", null, ParamContainer["LIVE_CT"].KeyValue, "U"),
                        "&" + ParamContainer["K"].KeyName + "=" + ParamContainer["K"].KeyValue,
                        "&_=" + Utils.GetUnixTimestamp());
                    //Logger.Info("IBET: START->ProcessUpdateLiveData -> URL:::" + urlUpdateLodds);
                    var updateLiveOddMessage = Get(urlUpdateLodds, IbetConfig.URL_NEW_MARKET, "application/x-www-form-urlencoded");
                    if (updateLiveOddMessage.StatusCode == HttpStatusCode.OK &&
                        !string.IsNullOrEmpty(updateLiveOddMessage.Result))
                    {
                        string updateTime;
                        var dataConvert = ConvertUpdateData(updateLiveOddMessage.Result, out updateTime, ref LiveMatchOddDatas);
                        ParamContainer["LIVE_CT"].KeyValue = updateTime;
                        UpdateException(this);
                        return dataConvert;
                    }
                    else
                    {
                        //Logger.Error("IBET: END->ProcessUpdateLiveData -> FAIL");
                        Logger.Error("MESSAGE : " + updateLiveOddMessage.Result + " URL REQUEST:: " + urlUpdateLodds);
                        UpdateException(this, eExceptionType.RequestFail);
                    }
                }
            }
            catch (Exception ex)
            {
                //Logger.Error(ex);
                Logger.Error(ex);
                //UpdateException(this, eExceptionType.RequestFail);
                return null;
            }
            //UpdateException(this, eExceptionType.RequestFail);
            return null;
        }

        public List<MatchOddDTO> ProcessUpdateNonLiveData()
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
                        var dataConverted = ConvertUpdateData(updateLiveOddMessage.Result, out updateTime,
                            ref NoneLiveMatchOddDatas);
                        ParamContainer["NON_LIVE_CT"].KeyValue = updateTime;
                        UpdateException(this);
                        return dataConverted;
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
            return null;
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
            ExceptionCount = 0;
            //UpdateException(this);
        }

        public float UpdateAvailabeCredit()
        {
            AvailabeCredit = GetAvailabeCredit();
            return AvailabeCredit;
        }

        public List<MatchOddDTO> ConvertFullData(string data, out string updateTime, bool isLive)
        {
            System.Collections.Generic.List<MatchOddDTO> list = new List<MatchOddDTO>();
            updateTime = string.Empty;
            if (data != string.Empty)
            {
                //list = new List<MatchOddDTO>();
                data = JavaScriptConvert.CleanScriptTag(data);
                string[] arrayData = data.Split(new[]
				{
					';'
				}, System.StringSplitOptions.None);

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
                        }, System.StringSplitOptions.None)[1];

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

        public List<MatchOddDTO> ConvertUpdateData(string data, out string updateTime, ref List<MatchOddDTO> sourceData)
        {
            //Logger.Info(UrlHost + "----" + data);
            List<MatchOddDTO> updateData = new List<MatchOddDTO>();
            updateTime = string.Empty;
            //List<MatchDTO> newMatchs = new List<MatchDTO>();
            //List<UpdatedOdd> newOdds = new List<UpdatedOdd>();
            //List<string> deletedIds = new List<string>();

            data = JavaScriptConvert.CleanScriptTag(data);
            string[] arrayData = data.Split(new[]
				{
					';'
				}, System.StringSplitOptions.None);

            for (int i = 0; i < arrayData.Length; i++)
            {
                string textData = arrayData[i];
                if (textData.StartsWith("uOl"))
                {
                    string valueNewOdd = textData.Split(new[]
							{
								"]="
							}, System.StringSplitOptions.None)[1];


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

                    MatchOddDTO oldMatchOddDto =
                        sourceData.FirstOrDefault(mo => mo.MatchID == jArrayNewOdds[0].ToString()
                                                        && mo.OddID == jArrayNewOdds[2].ToString());

                    //UpdatedOdd newOddUpdated = new UpdatedOdd();
                    //newOddUpdated.MatchID = jArrayNewOdds[0].ToString();
                    //newOddUpdated.OddID = jArrayNewOdds[2].ToString();
                    if (oldMatchOddDto != null)
                    {
                        switch (oddTypeValue)
                        {
                            case 1:
                                oldMatchOddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString(), jArrayNewOdds[6].ToString());
                                oldMatchOddDto.OddType = eOddType.HCP;
                                break;
                            case 3:
                                oldMatchOddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString());
                                oldMatchOddDto.OddType = eOddType.OU;
                                break;
                            case 7:
                                oldMatchOddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString(), jArrayNewOdds[6].ToString());
                                oldMatchOddDto.OddType = eOddType.HalfHCP;
                                break;
                            case 8:
                                oldMatchOddDto.Odd = ConvertOdd(jArrayNewOdds[3].ToString());
                                oldMatchOddDto.OddType = eOddType.HalfOU;
                                break;
                        }
                        oldMatchOddDto.HomeOdd = (float)jArrayNewOdds[4];
                        oldMatchOddDto.AwayOdd = (float)jArrayNewOdds[5];

                        updateData.Add(oldMatchOddDto);
                    }
                }
                else if (textData.StartsWith("uLl"))
                {
                    string valueNewTime = textData.Split(new[]
							{
								"]="
							}, StringSplitOptions.None)[1];


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
                        }, System.StringSplitOptions.None)[1];
                    this.ConvertNewMatch(valueNewMatch, ref sourceData);
                }
                else if (textData.StartsWith("var Dell"))
                {
                    JArray jArrayDeleteId = JavaScriptConvert.DeserializeObject(textData.Split(new[]
                        {
                            '='
                        }, StringSplitOptions.None)[1]);
                    var deletedIds = jArrayDeleteId.Select(deletedId => (string)deletedId);
                    sourceData.RemoveAll(x => deletedIds.Contains(x.MatchID));
                    //deletedIds.AddRange(jArrayDeleteId.Select(deletedId => (string)deletedId));
                }
                else if (textData.StartsWith("window"))
                {
                    updateTime = textData.Substring(43, 19);
                }
            }

            return updateData;
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
                UpdateException(this, eExceptionType.LoginFail);
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

        private HttpClient httpClient;

        private Task<string> GetClient(string urlRequest, string urlRefer = "", string contentType = "")
        {
            urlRequest = this.UrlHost + urlRequest;
            //if (!string.IsNullOrEmpty(urlRefer))
            //{
            //    urlRefer = this.UrlHost + urlRefer;
            //    httpClient.DefaultRequestHeaders.Add("Referer", urlRefer);
            //}

            //if (!string.IsNullOrEmpty(contentType))
            //{
            //    //httpClient.DefaultRequestHeaders.Add("Content-Type", contentType);
            //    //httpClient.c
            //}
            var response = httpClient.GetAsync(urlRequest);
            //httpClient.co
            // response.EnsureSuccessStatusCode();

            //  responseBodyAsText = await response.Content.ReadAsStringAsync()
            return response.Result.Content.ReadAsStringAsync();
        }

        private string PostWebRequest(string urlRequest, string urlRefer = "", string contentType = "")
        {
            urlRequest = this.UrlHost + urlRequest;
            WebRequest request = WebRequest.Create(urlRequest);
            request.Method = "GET";
            request.ContentType = contentType;
            //request.Headers.c
            WebResponse response = request.GetResponse();

            Stream dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
            dataStream.Close();
            return responseFromServer;
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
        private void ConvertNewMatch(string data, ref List<MatchOddDTO> matchsSource)
        {
            JArray javaScriptArray = (JArray)JavaScriptConvert.DeserializeObject(data);
            MatchOddDTO matchDTO;

            if (javaScriptArray[3].ToString() == "1")
            {
                string leagueID;
                string leagueName;
                int minute;
                eTimeMatchType timeType;
                if (javaScriptArray[4].ToString() == string.Empty)
                {
                    MatchOddDTO matchDTO2 = matchsSource[matchsSource.Count - 1];
                    leagueID = matchDTO2.LeagueID;
                    leagueName = matchDTO2.LeagueName;
                    timeType = matchDTO2.TimeType;
                    minute = matchDTO2.Minutes;
                }
                else
                {
                    leagueID = javaScriptArray[4].ToString();
                    leagueName = javaScriptArray[5].ToString();
                    timeType = ConvertMatchTime(javaScriptArray[12].ToString(), out  minute);
                }
                matchDTO = new MatchOddDTO(javaScriptArray[0].ToString(), eServerType.Ibet, leagueID, leagueName,
                   CleanTeamName(javaScriptArray[6].ToString()), CleanTeamName(javaScriptArray[7].ToString()),
                   timeType, minute);
                //matchsSource.Add(matchDTO);
            }
            else
            {
                var oldMatchDTO = matchsSource[matchsSource.Count - 1];
                matchDTO = new MatchOddDTO(oldMatchDTO.MatchID, oldMatchDTO.ServerType, oldMatchDTO.LeagueID,
                    oldMatchDTO.LeagueName, oldMatchDTO.HomeTeamName, oldMatchDTO.AwayTeamName, oldMatchDTO.TimeType, oldMatchDTO.Minutes);
                //matchsSource.Add(matchDTO);
            }
            //var newObj = Utils.Clone(matchDTO);
            ////FULLTIME Chau A
            if (javaScriptArray[26 + ODD_INDEX].ToString().Length != 0)
            {
                var newObj1 = Utils.Clone(matchDTO);
                UpdateOdd(ref newObj1, javaScriptArray[26 + ODD_INDEX].ToString(),
                     eOddType.HCP,
                     javaScriptArray[27 + ODD_INDEX].ToString(), javaScriptArray[28 + ODD_INDEX].ToString(),
                     javaScriptArray[29 + ODD_INDEX].ToString(), javaScriptArray[30 + ODD_INDEX].ToString());
                matchsSource.Add(newObj1);
            }
            ////FULLTIME TAI XIU
            if (javaScriptArray[31 + ODD_INDEX].ToString().Length != 0)
            {
                var newObj1 = Utils.Clone(matchDTO);
                UpdateOdd(ref newObj1, javaScriptArray[31 + ODD_INDEX].ToString(),
                    eOddType.OU,
                    javaScriptArray[32 + ODD_INDEX].ToString(), javaScriptArray[33 + ODD_INDEX].ToString(),
                    javaScriptArray[34 + ODD_INDEX].ToString());
                matchsSource.Add(newObj1);
            }
            /////H1 Chau A
            if (javaScriptArray[39 + ODD_INDEX].ToString().Length != 0)
            {
                var newObj1 = Utils.Clone(matchDTO);
                UpdateOdd(ref newObj1, javaScriptArray[39 + ODD_INDEX].ToString(),
                    eOddType.HalfHCP,
                    javaScriptArray[40 + ODD_INDEX].ToString(), javaScriptArray[41 + ODD_INDEX].ToString(),
                    javaScriptArray[42 + ODD_INDEX].ToString(), javaScriptArray[43 + ODD_INDEX].ToString());
                matchsSource.Add(newObj1);
            }
            /////H1 TAI XIU
            if (javaScriptArray[44 + ODD_INDEX].ToString().Length != 0)
            {
                var newObj1 = Utils.Clone(matchDTO);
                UpdateOdd(ref newObj1, javaScriptArray[44 + ODD_INDEX].ToString(),
                    eOddType.HalfOU,
                    javaScriptArray[45 + ODD_INDEX].ToString(), javaScriptArray[46 + ODD_INDEX].ToString(),
                    javaScriptArray[47 + ODD_INDEX].ToString());
                matchsSource.Add(newObj1);
            }
        }

        private void UpdateOdd(ref MatchOddDTO matchOddDTO, string oddID, eOddType oddType, string odd, string home, string away, string checkHorA = "")
        {
            float homeOdd = 0f;
            float awayOdd = 0f;
            float.TryParse(home, out homeOdd);
            float.TryParse(away, out awayOdd);
            float oddValue = ConvertOdd(odd, checkHorA);

            matchOddDTO.OddID = oddID;
            matchOddDTO.OddType = oddType;
            matchOddDTO.Odd = oddValue;
            matchOddDTO.HomeOdd = homeOdd;
            matchOddDTO.AwayOdd = awayOdd;
        }

        private float ConvertOdd(string oldOdd, string checkHorA = "")
        {
            float oddValue = 0f;

            if (oldOdd.Contains('-'))
            {
                var oddSplit = oldOdd.Split('-');
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

        private void BindBetParam(string msgData)
        {
            switch (msgData)
            {
                case "Ex":
                    ParamContainer["areyousuremesg"] = new ParamRequest("Are you sure you want process the bet?");
                    ParamContainer["areyousuremesg1"] = new ParamRequest("Your previous bet is still processing, are you sure you want to bet ?");
                    ParamContainer["betconfirmmesg"] = new ParamRequest("Please click OK to confirm the bet?");
                    ParamContainer["btnBPSubmit"] = new ParamRequest("Process Bet");
                    ParamContainer["hidStake10"] = new ParamRequest("Stake must be in multiples of 10");
                    ParamContainer["hidStake2"] = new ParamRequest("Stake must be in multiples of 2");
                    ParamContainer["hidStake20"] = new ParamRequest("Stake must be in multiples of 20");
                    ParamContainer["highermaxmesg"] = new ParamRequest("Your stake is higher than maximum bet!!!");
                    ParamContainer["incorrectStakeMesg"] = new ParamRequest("Incorrect Stake.");
                    ParamContainer["lowerminmesg"] = new ParamRequest("Your stake is lower than minimun bet!!!");
                    ParamContainer["oddChange1"] = new ParamRequest("Odds has changed from");
                    ParamContainer["oddChange2"] = new ParamRequest("to");
                    ParamContainer["oddsWarning"] = new ParamRequest("WARNING!!! WE HAVE GIVEN A NEW ODDS & NEW STAKE!!!");
                    break;
                default:
                    try
                    {
                        var docrootLeftInOne = new HtmlAgilityPack.HtmlDocument();
                        docrootLeftInOne.LoadHtml(msgData);
                        var areyousuremesg = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='areyousuremesg']")
                            .Attributes["value"].Value;
                        ParamContainer["areyousuremesg"] = new ParamRequest(areyousuremesg);
                        var areyousuremesg1 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='areyousuremesg1']")
                            .Attributes["value"].Value;
                        ParamContainer["areyousuremesg1"] = new ParamRequest(areyousuremesg1);
                        var betconfirmmesg = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='betconfirmmesg']")
                            .Attributes["value"].Value;
                        ParamContainer["betconfirmmesg"] = new ParamRequest(betconfirmmesg);
                        var btnBPSubmit = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='text' and @name='btnBPSubmit']")
                            .Attributes["value"].Value;
                        ParamContainer["btnBPSubmit"] = new ParamRequest(btnBPSubmit);
                        var hidStake10 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='hidStake10']")
                            .Attributes["value"].Value;
                        ParamContainer["hidStake10"] = new ParamRequest(hidStake10);
                        var hidStake2 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='hidStake2']")
                            .Attributes["value"].Value;
                        ParamContainer["hidStake2"] = new ParamRequest(hidStake2);
                        var hidStake20 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='hidStake20']")
                            .Attributes["value"].Value;
                        ParamContainer["hidStake20"] = new ParamRequest(hidStake20);
                        var highermaxmesg = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='highermaxmesg']")
                            .Attributes["value"].Value;
                        ParamContainer["highermaxmesg"] = new ParamRequest(highermaxmesg);
                        var incorrectStakeMesg = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='incorrectStakeMesg']")
                            .Attributes["value"].Value;
                        ParamContainer["incorrectStakeMesg"] = new ParamRequest(incorrectStakeMesg);
                        var lowerminmesg = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='lowerminmesg']")
                            .Attributes["value"].Value;
                        ParamContainer["lowerminmesg"] = new ParamRequest(lowerminmesg);
                        var oddChange1 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='oddChange1']")
                            .Attributes["value"].Value;
                        ParamContainer["oddChange1"] = new ParamRequest(oddChange1);
                        var oddChange2 = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='oddChange2']")
                            .Attributes["value"].Value;
                        ParamContainer["oddChange2"] = new ParamRequest(oddChange2);
                        var oddsWarning = docrootLeftInOne.DocumentNode.SelectSingleNode("//input[@type='hidden' and @name='oddsWarning']")
                            .Attributes["value"].Value;
                        ParamContainer["oddsWarning"] = new ParamRequest(oddsWarning);
                    }
                    catch (Exception)
                    {
                        goto case "Ex";
                    }
                    break;
            }
        }
        #endregion
    }
}
