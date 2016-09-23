using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BCWin.Metadata;
using BcWin.Common;
using BcWin.Processor.Interface;
using log4net;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using BcWin.Common.DTO;
using TypescriptB;
using BcWin.Common.Objects;
using BcWin.Core.EventDelegate;
using BcWin.Core.Helper;
using System.Diagnostics;

namespace BcWin.Processor
{
    public partial class SboEngine : SboHelper, IEngine
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SboEngine));
        public int Rebet { get; set; }

        public event FakeRequestEvent OnFakeRequest;

        public event UpdateDataHandler UpdateLiveDataChange;
        public event UpdateDataHandler UpdateNonLiveDataChange;
        public delegate void UpdateDataHandler(List<MatchOddDTO> m, bool isLive, int type = 0);
        public PrepareBetSbobet BetMessageQueue { get; set; }
        public PrepareBetSbobet TempBetMessageQueue { get; set; }

        public BetInfoQueue BInfoQueue { get; set; }

        public Dictionary<string, string> ParamContainer { get; set; }

        private const string userAgent = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:40.0) Gecko/20100101 Firefox/40.0";

        private const string accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        private System.Threading.Timer objLiveScanTimer;
        private System.Threading.Timer objNonLiveScanTimer;
        private System.Threading.Timer objCheckLoginTimer;

        #region constructor

        public SboEngine()
        {
            ParamContainer = new Dictionary<string, string>();
            ServerType = eServerType.Sbo;
        }

        #endregion

        public void LogOff()
        {
            this.Dispose();

            if (objCheckLoginTimer != null)
            {
                objCheckLoginTimer.Dispose();
            }
            lock (LockLive)
            {
                LiveMatchOddDatas = new List<MatchOddDTO>();
            }
            lock (LockNonLive)
            {
                NoneLiveMatchOddDatas = new List<MatchOddDTO>();
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

        public void InitEngine()
        {
            ServerType = eServerType.Sbo;
            Status = eServiceStatus.Initialized;

        }

        private void WaitCheckLoginCallBack(object obj)
        {
            CheckLogin();
        }

        #region Scan, Paser Data

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

            //EngineLogger = new EngineLogger(EngineName) { ServerType = eServerType.Sbo };

            Logger.Info("STARTED: " + UrlHost);
            Status = eServiceStatus.Started;
        }

        public void StartTest()
        {
            ProcessUpdateLiveData();
            //var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);
            //string liveVersion = "";
            //LiveMatchOddDatas = ConvertFullData(liveResponse.Result, out liveVersion, true);
            //ParamContainer["LIVE_VERSION"] = liveVersion;

            //Merge for test
            //objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
            //   SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET);
        }

        public void StartTest1()
        {
            var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);
            string liveVersion = "";
            LiveMatchOddDatas = ConvertFullData1(liveResponse.Result, out liveVersion, true);
            ParamContainer["LIVE_VERSION"] = liveVersion;

            //Merge for test
            //objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
            //   SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET);
        }
        
        public void StartScanLive()
        {
            var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);

            string liveVersion = "";
            if (!string.IsNullOrEmpty(liveResponse.Result) && !liveResponse.Result.Contains("logout"))
            {
                //string dataTest = @"$M('odds-display').onUpdate(1,[140636,1,[[[5579,'I\u200Cr\u200C\u200C\u200C\u200C\u200C\u200Ca\u200C\u200C\u200Cn\u200C \u200CP\u200C\u200C\u200Cr\u200C\u200Co\u200C\u200C\u200C\u200C League','',''],[7210,'Uzbekistan Oliy Liga','',''],[9159,'Italy Lega Pro Cup','',''],[26871,'AFC Women\'s U19 Championship (in China)','',''],[36281,'AFF U19 Championship (in Laos)','','']],[[1717363,1,5579,'Siah Jamegan','Rah Ahan','1.36296',10,'08/26/2015 21:00',1,'',3,9999],[1717364,1,5579,'T\u200Cr\u200C\u200C\u200C\u200C\u200C\u200Ca\u200C\u200C\u200Cc\u200Ct\u200Co\u200C\u200C\u200Cr\u200C\u200C \u200C\u200C\u200C\u200CSazi','E\u200Cs\u200C\u200C\u200C\u200C\u200C\u200Ct\u200C\u200C\u200Ce\u200Cg\u200Ch\u200C\u200C\u200Cl\u200C\u200Ca\u200C\u200C\u200C\u200Cl Khuzestan','1.36297',10,'08/26/2015 21:45',1,'',6,9999],[1717429,1,7210,'Buxoro FK','FK Andijan','1.36313',10,'08/26/2015 21:30',1,'',4,9999],[1717434,1,7210,'Navbahor Namangan','Bunyodkor Tashkent','1.36309',10,'08/26/2015 20:00',1,'',3,9999],[1719784,1,9159,'Ischia Isolaverde','Fidelis Andria','1.36250',10,'08/26/2015 21:00',1,'',3,9999],[1713075,1,26871,'China (w) U19','Korea DPR (w) U19','1.36266',10,'08/26/2015 20:00',1,'',2,9999],[1720023,1,36281,'Laos U19','Brunei Darussalam U19','1.36255',10,'08/26/2015 20:00',0,'TH: True 681 TSP1\nTH: True 667 TS HD2\n',2,9999]],[[1207803,1717363,0,0,0,3],[1207804,1717364,0,0,0,6],[1207869,1717429,0,1,0,4],[1207874,1717434,0,0,1,3],[1213351,1719784,0,1,1,3],[1198692,1713075,0,0,2,2],[1213941,1720023,0,5,0,2]],[[1207803,1,1,42,45,0,0,0,{1:1},{1:45,2:45,3:15,4:15}],[1207804,1,1,3,45,0,0,0,{1:1,7:1},{1:45,2:45,3:15,4:15}],[1207869,1,1,17,45,0,0,0,{1:1,7:1},{1:45,2:45,3:15,4:15}],[1207874,1,2,44,45,0,0,0,0,{1:45,2:45,3:15,4:15}],[1213351,1,1,46,45,0,0,2,{1:1},{1:45,2:45,3:15,4:15}],[1198692,1,2,44,45,0,0,0,{1:1},{1:45,2:45,3:15,4:15}],[1213941,0,2,45,45,0,0,3,0,{1:45,2:45,3:15,4:15}]],,[[28233129,[1207803,1,1,1000.00,0.25],[-0.97,0.81]],[28233131,[1207803,3,1,1000.00,1.00],[0.91,0.91]],[28233133,[1207803,5,1,500.00,0],[2.6,2.15,3.7]],[28233138,[1207804,1,1,1000.00,0.75],[-0.96,0.8]],[28233140,[1207804,3,1,1000.00,2.25],[-0.88,0.7]],[28233142,[1207804,5,1,500.00,0],[1.72,3.2,4.4]],[28233139,[1207804,7,1,500.00,0.25],[0.99,0.85]],[28233143,[1207804,8,1,500.00,0],[2.44,2.01,4.7]],[28233141,[1207804,9,1,500.00,0.75],[0.75,-0.93]],[28234565,[1207869,1,1,500.00,0.25],[0.95,0.85]],[28234567,[1207869,3,1,500.00,2.75],[0.85,0.95]],[28234566,[1207869,7,1,500.00,0.00],[0.75,-0.95]],[28234568,[1207869,9,1,500.00,1.50],[0.85,0.95]],[28234595,[1207874,1,1,500.00,-0.25],[0.25,-0.45]],[28234599,[1207874,1,1,500.00,0.00],[-0.9,0.7]],[28234597,[1207874,3,1,500.00,1.50],[-0.3,0.1]],[28266988,[1213351,1,1,1000.00,0.25],[-0.89,0.77]],[28266990,[1213351,3,1,1000.00,3.25],[-0.98,0.84]],[28266992,[1213351,5,1,500.00,0],[2.69,2.2,3.4]],[28371622,[1198692,1,1,1000.00,0.00],[-0.73,0.57]],[28166262,[1198692,3,1,1000.00,2.50],[-0.33,0.15]],[28273363,[1213941,1,1,1000.00,0.25],[-0.61,0.45]],[28273366,[1213941,3,1,1000.00,5.50],[-0.58,0.4]]],,,'Football',0],[[],[],[1720023],0],,0]);";
                //a.Start();
                LiveMatchOddDatas = ConvertFullData_New(liveResponse.Result, out liveVersion, true);
                //a.Stop();

                //b.Start();
                //LiveMatchOddDatas2 = ConvertUpdatedData_New(liveResponse.Result, out liveVersion, true);
                //b.Stop();

            }

            ParamContainer["LIVE_VERSION"] = liveVersion;

            //Merge for test
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
               SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET);
           // objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
           //7000, 7000);
        }

        public List<MatchOddDTO> GetFullLiveData()
        {
            lock (LockLive)
            {
                try
                {
                    var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);

                    if (!string.IsNullOrEmpty(liveResponse.Result))
                    {
                        lock (LockLive)
                        {
                            string liveVersion;
                            LiveMatchOddDatas = ConvertFullData(liveResponse.Result, out liveVersion, true);
                            ParamContainer["LIVE_VERSION"] = liveVersion;
                            //UpdateException(this);
                            return LiveMatchOddDatas;
                        }
                    }
                    //else
                    //{
                    //    TypeGetScan = 0;
                    //}
                }
                catch (Exception ex)
                {
                    Logger.Error(EngineName, ex);
                    //UpdateException(this, eExceptionType.RequestFail);
                }
                return null;
            }
        }

        public void StartScanLiveDriver()
        {
            var liveResponse = Get(SbobetConfig.URL_LIVE_ALL_DATA);
            string liveVersion = "";
            if (!string.IsNullOrEmpty(liveResponse.Result))
            {
                LiveMatchOddDatas = ConvertFullData(liveResponse.Result, out liveVersion, true);
            }
            ParamContainer["LIVE_VERSION"] = liveVersion;
        }

        public void StartUpdateLiveDriver()
        {
            objLiveScanTimer = new System.Threading.Timer(WaitScanCallback, true,
               SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_LIVE_SBOBET);
        }

        public void StartScanNonLiveDriver()
        {
            var todayRespone = Get(SbobetConfig.URL_TODAY_All_NON_LIVE_DATA);
            string nonVersion = "";
            NoneLiveMatchOddDatas = ConvertFullData(todayRespone.Result, out nonVersion, false);
            ParamContainer["NON_LIVE_VERSION"] = nonVersion;
            //Merge for test

        }

        public void StartUpdateNonLiveDriver()
        {
            objNonLiveScanTimer = new System.Threading.Timer(WaitScanCallback, false,
                SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET);
        }

        private void StartScanNonLive()
        {
            var todayRespone = Get(SbobetConfig.URL_TODAY_All_NON_LIVE_DATA);
            string nonVersion = "";
            NoneLiveMatchOddDatas = ConvertFullData(todayRespone.Result, out nonVersion, false);
            ParamContainer["NON_LIVE_VERSION"] = nonVersion;
            //Merge for test
            objNonLiveScanTimer = new System.Threading.Timer(WaitScanCallback, false,
               SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET, SystemConfig.TIME_GET_UPDATE_NON_LIVE_SBOBET);
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

        public void Dispose()
        {
            PauseScan();
        }

        public int TypeGetScan { get; set; } //0 get full ; 1 get update
        private void WaitScanCallback(object obj)
        {
            List<MatchOddDTO> updatedata;

            if ((bool)obj)
            {
                if (TypeGetScan == 0)
                {
                    TypeGetScan = 1;
                    updatedata = GetFullLiveData();

                    if (UpdateLiveDataChange != null)
                    {
                        UpdateLiveDataChange(updatedata, true);
                    }
                }
                else
                {
                    TypeGetScan = 0;
                    ProcessUpdateLiveData();
                }

                //updatedata = ProcessUpdateLiveData();
            }
            else
            {
                updatedata = ProcessUpdateNonLiveData();
                //ProcessUpdateNonLiveData();
                ////Process Non Live Update
                //ProcessUpdateNonLiveData();
                if (UpdateNonLiveDataChange != null)
                {
                    UpdateNonLiveDataChange(updatedata, false);
                }
            }
        }

        public List<MatchOddDTO> ProcessUpdateLiveData()
        {
            string url = SbobetConfig.URL_LIVE_UPDATE_DATA + "&v=" + ParamContainer["LIVE_VERSION"];
            var liveResponse = Get(url);
            //Logger.Debug(liveResponse.Result);
            if (liveResponse.StatusCode == HttpStatusCode.OK &&
                !string.IsNullOrEmpty(liveResponse.Result))
            {
                try
                {
                    lock (LockLive)
                    {
                        string liveVersion = ParamContainer["LIVE_VERSION"];
                        var dataConverted = ConvertUpdatedData(liveResponse.Result, out liveVersion, true);
                        ParamContainer["LIVE_VERSION"] = liveVersion;
                        return dataConverted;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error("Parse update sbo error. Message::::::" + liveResponse.Result, ex);
                }
            }
            return new List<MatchOddDTO>();
        }
        public List<MatchOddDTO> ProcessUpdateNonLiveData()
        {
            string url = SbobetConfig.URL_TODAY_UPDATE_NON_LIVE_DATA + "&v=" + ParamContainer["NON_LIVE_VERSION"];
            var nonTodayResponse = Get(url);
            //Logger.Debug(nonTodayResponse.Result);
            if (nonTodayResponse.StatusCode == HttpStatusCode.OK &&
                !string.IsNullOrEmpty(nonTodayResponse.Result))
            {
                lock (LockNonLive)
                {
                    string nonVersion = ParamContainer["NON_LIVE_VERSION"];
                    var dataConverted = ConvertUpdatedData(nonTodayResponse.Result, out nonVersion, false);
                    ParamContainer["NON_LIVE_VERSION"] = nonVersion;
                    return dataConverted;
                }
            }
            return new List<MatchOddDTO>();
        }

        public List<MatchOddDTO> ConvertFullData1(string data, out string version, bool isLive)
        {
            version = "";
            //key = ma tran, value= key de map ma tran vs ma keo
            Dictionary<string, string> eventsResultDictionary = new Dictionary<string, string>();
            //key = ma keo, value = thong tin keo
            Dictionary<string, NewMatchDTO> matchDictionary = new Dictionary<string, NewMatchDTO>();
            Dictionary<string, string> leagueDictionary = new Dictionary<string, string>();
            List<MatchOddDTO> result = new List<MatchOddDTO>();

            if (data == string.Empty)
            {
                return result;
            }

            int index = 3;
            if (isLive)
            {
                //data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("'", "");
                if (data.IndexOf("onIdle") >= 0)
                {
                    return result;
                }
                index = 2;
            }
            data = JavaScriptConvert.CleanScriptTag(data);
            data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "");
            string[] arrayData = data.Split(new[]
				    {
					    ';'
				    }, System.StringSplitOptions.None);


            for (int i = 0; i < arrayData.Length; i++)
            {
                string text = arrayData[i];
                if (text.IndexOf("onUpdate") >= 0)
                {
                    text = text + ";";
                    int _num = text.IndexOf("$M('odds-display').onUpdate") + 30;
                    int _num2 = text.IndexOf(";");
                    text = text.Substring(_num, _num2 - _num);
                    text = text.Remove(text.Length - 1);
                    JArray javaScriptArray = (JArray)JavaScriptConvert.DeserializeObject(text);
                    version = javaScriptArray[0].ToString(); // = 1284 => version
                    //int num2 = int.Parse(arrayList[1].ToString()); // 0 ==> Truc tiep, 1 ==> Khong truoc tiep
                    //bool flag = int.Parse(arrayList[2].ToString()) == 1;
                    var aaa = javaScriptArray[index].Count();
                    var bbbb = javaScriptArray[index];
                    if (javaScriptArray[index].Count() >= 8)
                    {
                        JArray javaScriptArray3 = (JArray)javaScriptArray[index];

                        //League Array
                        if (javaScriptArray3[0].ToString().Length != 0)
                        {
                            foreach (var aray30 in (JArray)javaScriptArray3[0])
                            {
                                leagueDictionary.Add(aray30[0].ToString(), aray30[1].ToString());
                            }
                        }

                        //Match Array
                        if (javaScriptArray3[1].ToString().Length != 0)
                        {
                            foreach (var aray31 in (JArray)javaScriptArray3[1])
                            {
                                //var JArray1 = enumerator.Current;
                                string leagueID = aray31[2].ToString();
                                NewMatchDTO match = new NewMatchDTO()
                                {
                                    ID = aray31.First.ToString(),
                                    LeagueID = leagueID,
                                    LeagueName = leagueDictionary[leagueID],
                                    //League = new LeagueDTO() { ID = leagueID, Name = leagueDictionary[leagueID] },
                                    HomeTeamName = CleanTeamName(aray31[3].ToString()),
                                    AwayTeamName = CleanTeamName(aray31[4].ToString()),
                                };
                                //Maybe add to list macth 
                                matchDictionary.Add(aray31.First.ToString(), match);
                            }
                        }


                        //EVENT ARRAY
                        if (javaScriptArray3[2].ToString().Length != 0)
                        {
                            foreach (var aray32 in (JArray)javaScriptArray3[2])
                            {
                                //Store key map, value is match id                                                                      
                                if ((int)aray32[2] == 0)
                                {
                                    string key = aray32[0].ToString().Trim();
                                    string value = aray32[1].ToString();
                                    if (eventsResultDictionary.ContainsKey(key) == false)
                                    {
                                        eventsResultDictionary.Add(key, value);
                                    }
                                }
                            }
                        }
                        //ODD ARRAY
                        if (javaScriptArray3[5].ToString().Length != 0)
                        {
                            foreach (var aray35 in (JArray)javaScriptArray3[5])
                            {
                                string oid = aray35[0].ToString();
                                JArray JArray_1 = (JArray)aray35[1];
                                JArray JArray_2 = (JArray)aray35[2];
                                string strKeyMap = JArray_1[0].ToString();
                                if (eventsResultDictionary.ContainsKey(strKeyMap))
                                {
                                    NewMatchDTO match = matchDictionary[eventsResultDictionary[strKeyMap]];
                                    if (match != null)
                                    {
                                        MatchOddDTO odd = ConvertMatchOddDto(oid, JArray_1[1].ToString(),
                                            JArray_1[4].ToString(), JArray_2[0].ToString(),
                                            JArray_2[1].ToString(), match.ID, match.HomeTeamName, match.AwayTeamName, match.LeagueID, match.LeagueName);
                                        if (odd != null)
                                        {
                                            result.Add(odd);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        public List<MatchOddDTO> ConvertFullData(string data, out string version, bool isLive)
        {
            try
            {
                version = "";
                //key = ma tran, value= key de map ma tran vs ma keo
                Dictionary<string, string> eventsResultDictionary = new Dictionary<string, string>();
                //key = ma keo, value = thong tin keo
                Dictionary<string, NewMatchDTO> matchDictionary = new Dictionary<string, NewMatchDTO>();
                Dictionary<string, string> leagueDictionary = new Dictionary<string, string>();
                List<MatchOddDTO> result = new List<MatchOddDTO>();

                if (data == string.Empty)
                {
                    return result;
                }

                int index = 3;
                if (isLive)
                {
                    //data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("'", "");
                    if (data.IndexOf("onIdle") >= 0)
                    {
                        return result;
                    }
                    index = 2;
                }
                data = JavaScriptConvert.CleanScriptTag(data);
                data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "");
                string[] arrayData = data.Split(new[]
				    {
					    ';'
				    }, System.StringSplitOptions.None);


                for (int i = 0; i < arrayData.Length; i++)
                {
                    string text = arrayData[i];
                    if (text.IndexOf("onUpdate") >= 0)
                    {
                        text = text + ";";
                        int _num = text.IndexOf("$M('odds-display').onUpdate") + 30;
                        int _num2 = text.IndexOf(";");
                        text = text.Substring(_num, _num2 - _num);
                        text = text.Remove(text.Length - 1);
                        JArray javaScriptArray = (JArray)JavaScriptConvert.DeserializeObject(text);
                        version = javaScriptArray[0].ToString(); // = 1284 => version
                        //int num2 = int.Parse(arrayList[1].ToString()); // 0 ==> Truc tiep, 1 ==> Khong truoc tiep
                        //bool flag = int.Parse(arrayList[2].ToString()) == 1;
                        var aaa = javaScriptArray[index].Count();
                        var bbbb = javaScriptArray[index];
                        if (javaScriptArray[index].Count() >= 8)
                        {
                            JArray javaScriptArray3 = (JArray)javaScriptArray[index];

                            //League Array
                            if (javaScriptArray3[0].ToString().Length != 0)
                            {
                                using (var enumerator = ((JArray)javaScriptArray3[0]).GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var JArray0 = enumerator.Current;
                                        leagueDictionary.Add(JArray0[0].ToString(), JArray0[1].ToString());
                                    }
                                }
                            }

                            //Match Array
                            if (javaScriptArray3[1].ToString().Length != 0)
                            {
                                using (var enumerator = ((JArray)javaScriptArray3[1]).GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var JArray1 = enumerator.Current;
                                        string leagueID = JArray1[2].ToString();
                                        NewMatchDTO match = new NewMatchDTO()
                                        {
                                            ID = JArray1.First.ToString(),
                                            LeagueID = leagueID,
                                            LeagueName = leagueDictionary[leagueID],
                                            //League = new LeagueDTO() { ID = leagueID, Name = leagueDictionary[leagueID] },
                                            HomeTeamName = CleanTeamName(JArray1[3].ToString()),
                                            AwayTeamName = CleanTeamName(JArray1[4].ToString()),
                                        };
                                        //Maybe add to list macth 
                                        matchDictionary.Add(JArray1.First.ToString(), match);
                                    }
                                }
                            }


                            //EVENT ARRAY
                            if (javaScriptArray3[2].ToString().Length != 0)
                            {
                                using (var enumerator = ((JArray)javaScriptArray3[2]).GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var JArray2 = enumerator.Current;
                                        //Store key map, value is match id                                                                      
                                        if ((int)JArray2[2] == 0)
                                        {
                                            string key = JArray2[0].ToString().Trim();
                                            string value = JArray2[1].ToString();
                                            if (eventsResultDictionary.ContainsKey(key) == false)
                                            {
                                                eventsResultDictionary.Add(key, value);
                                            }
                                        }
                                    }
                                }

                            }
                            //ODD ARRAY
                            if (javaScriptArray3[5].ToString().Length != 0)
                            {
                                using (var enumerator = ((JArray)javaScriptArray3[5]).GetEnumerator())
                                {
                                    while (enumerator.MoveNext())
                                    {
                                        var JArray5 = enumerator.Current;
                                        string oid = JArray5[0].ToString();
                                        JArray JArray_1 = (JArray)JArray5[1];
                                        JArray JArray_2 = (JArray)JArray5[2];
                                        string strKeyMap = JArray_1[0].ToString();
                                        if (eventsResultDictionary.ContainsKey(strKeyMap))
                                        {
                                            NewMatchDTO match = matchDictionary[eventsResultDictionary[strKeyMap]];
                                            if (match != null)
                                            {
                                                MatchOddDTO odd = ConvertMatchOddDto(oid, JArray_1[1].ToString(),
                                                    JArray_1[4].ToString(), JArray_2[0].ToString(),
                                                    JArray_2[1].ToString(), match.ID, match.HomeTeamName, match.AwayTeamName, match.LeagueID, match.LeagueName);
                                                if (odd != null)
                                                {
                                                    result.Add(odd);
                                                }
                                            }

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception)
            {
                version = "";
                return null;
            }

        }

        public List<MatchOddDTO> ConvertFullData_New(string dt, out string version, bool isLive)
        {
            try
            {
               
                version = "";
                //key = ma tran, value= key de map ma tran vs ma keo
                Dictionary<string, string> eventsResultDictionary = new Dictionary<string, string>();
                //key = ma keo, value = thong tin keo
                Dictionary<string, NewMatchDTO> matchDictionary = new Dictionary<string, NewMatchDTO>();
                Dictionary<string, string> leagueDictionary = new Dictionary<string, string>();
                List<MatchOddDTO> result = new List<MatchOddDTO>();

                int index = 3;
                if (isLive)
                {
                    //data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("'", "");
                    if (dt.IndexOf("onIdle") >= 0)
                    {
                        return result;
                    }
                    index = 2;
                }

                dt = JavaScriptConvert.CleanScriptTag(dt);

                StringBuilder data = new StringBuilder(dt);
                data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "");
                data = data.Remove(data.Length - 2, 2);
                data = data.Replace("$M('odds-display').onUpdate(1,", "");

                JArray javaScriptArray = (JArray)JavaScriptConvert.DeserializeObject(data.ToString());
                version = javaScriptArray[0].ToString(); // = 1284 => version

                if (javaScriptArray[index].Count() >= 8)
                {
                    // javaScriptArray[index];

                    //League Array
                    if (javaScriptArray[index][0].ToString().Length != 0)
                    {
                        using (var enumerator = ((JArray)javaScriptArray[index][0]).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var JArray0 = enumerator.Current;
                                leagueDictionary.Add(JArray0[0].ToString(), JArray0[1].ToString());
                            }
                        }
                    }

                    //Match Array
                    if (javaScriptArray[index][1].ToString().Length != 0)
                    {
                        using (var enumerator = ((JArray)javaScriptArray[index][1]).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var JArray1 = enumerator.Current;
                                string leagueID = JArray1[2].ToString();
                                NewMatchDTO match = new NewMatchDTO()
                                {
                                    ID = enumerator.Current.First.ToString(),
                                    LeagueID = leagueID,
                                    LeagueName = leagueDictionary[leagueID],
                                    //League = new LeagueDTO() { ID = leagueID, Name = leagueDictionary[leagueID] },
                                    HomeTeamName = CleanTeamName(JArray1[3].ToString()),
                                    AwayTeamName = CleanTeamName(JArray1[4].ToString()),
                                };
                                //Maybe add to list macth 
                                matchDictionary.Add(JArray1.First.ToString(), match);
                            }
                        }
                    }

                    //EVENT ARRAY
                    if (javaScriptArray[index][2].ToString().Length != 0)
                    {
                        using (var enumerator = ((JArray)javaScriptArray[index][2]).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var JArray2 = enumerator.Current;
                                //Store key map, value is match id                                                                      
                                if ((int)JArray2[2] == 0)
                                {
                                    string key = JArray2[0].ToString().Trim();
                                    string value = JArray2[1].ToString();
                                    if (eventsResultDictionary.ContainsKey(key) == false)
                                    {
                                        eventsResultDictionary.Add(key, value);
                                    }
                                }
                            }
                        }
                    }

                    //ODD ARRAY
                    if (javaScriptArray[index][5].ToString().Length != 0)
                    {
                        using (var enumerator = ((JArray)javaScriptArray[index][5]).GetEnumerator())
                        {
                            while (enumerator.MoveNext())
                            {
                                var JArray5 = enumerator.Current;
                                string oid = JArray5[0].ToString();
                                JArray JArray_1 = (JArray)JArray5[1];
                                JArray JArray_2 = (JArray)JArray5[2];
                                string strKeyMap = JArray_1[0].ToString();
                                if (eventsResultDictionary.ContainsKey(strKeyMap))
                                {
                                    NewMatchDTO match = matchDictionary[eventsResultDictionary[strKeyMap]];
                                    if (match != null)
                                    {
                                        MatchOddDTO odd = ConvertMatchOddDto(oid, JArray_1[1].ToString(),
                                            JArray_1[4].ToString(), JArray_2[0].ToString(),
                                            JArray_2[1].ToString(), match.ID, match.HomeTeamName, match.AwayTeamName, match.LeagueID, match.LeagueName);
                                        if (odd != null)
                                        {
                                            result.Add(odd);
                                        }
                                    }

                                }
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(EngineName, ex);
                version = "";
                return null;
            }

        }

        public List<MatchOddDTO> ConvertUpdatedData(string data, out string version, bool isLive)
        {
            List<MatchOddDTO> result = new List<MatchOddDTO>();

            version = "";
            //1. Get event Result list
            //2. Get odd list
            //3. foreach array tran     
            //key = key de map ma tran vs ma keo, value= ma tran
            Dictionary<string, string> eventsResultDictionary = new Dictionary<string, string>();

            //key = ma keo, value = thong tin keo
            Dictionary<string, NewMatchDTO> matchDictionary = new Dictionary<string, NewMatchDTO>();
            Dictionary<string, string> leagueDictionary = new Dictionary<string, string>();

            if (data == string.Empty)
            {
                return result;
            }

            data = JavaScriptConvert.CleanScriptTag(data);
            data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "");
            int index = 3;
            if (isLive)
            {

                if (data.IndexOf("onIdle") >= 0)
                {
                    int _num = data.IndexOf("onIdle");
                    int _num2 = data.IndexOf(";", _num);
                    data = data.Substring(_num + 7, _num2 - (_num + 8));

                    //data = data.Remove(data.Length - 1);
                    version = data.Split(new[] { ',' }, StringSplitOptions.None)[1];
                    return result;
                }
                //data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("'", "");
                index = 2;
            }
            else
            {
                //$M('odds-display').onIdle(7,[188812,0]);
                if (data.IndexOf("onIdle") >= 0)
                {
                    int _num = data.IndexOf("onIdle");
                    int _num2 = data.IndexOf(";", _num);
                    data = data.Substring(_num + 7, _num2 - (_num + 8));
                    version = data.Split(',')[1].Replace("[", "");
                }
            }

            string[] arrayData = data.Split(new[]
                    {
                        ';'
                    }, System.StringSplitOptions.None);

            for (int i = 0; i < arrayData.Length; i++)
            {
                string text = arrayData[i];
                if (text.IndexOf("onUpdate") >= 0)
                {
                    text = text + ";";
                    int _num = text.IndexOf("$M('odds-display').onUpdate") + 30;
                    int _num2 = text.IndexOf(";");
                    text = text.Substring(_num, _num2 - _num);
                    text = text.Remove(text.Length - 1);
                    JArray javaScriptArray = JavaScriptConvert.DeserializeObject(text);

                    version = javaScriptArray[0].ToString(); // = 1284 => version
                    //int num2 = int.Parse(arrayList[1].ToString()); // 0 ==> Truc tiep, 1 ==> Khong truoc tiep
                    //bool flag = int.Parse(arrayList[2].ToString()) == 1;
                    if (javaScriptArray[index].Count() >= 8)
                    {
                        JArray javaScriptArray3 = (JArray)javaScriptArray[index];


                        //League Array
                        if (javaScriptArray3[0].ToString().Length != 0)
                        {
                            using (var enumerator = ((JArray)javaScriptArray3[0]).GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var JArray0 = enumerator.Current;
                                    leagueDictionary.Add(JArray0[0].ToString(), JArray0[1].ToString());

                                }
                            }
                        }

                        //Match Array
                        if (javaScriptArray3[1].ToString().Length != 0)
                        {
                            using (var enumerator = ((JArray)javaScriptArray3[1]).GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var JArray1 = enumerator.Current;
                                    string id = JArray1.First.ToString();
                                    string leagueID = JArray1[2].ToString();
                                    NewMatchDTO match = new NewMatchDTO()
                                    {
                                        ID = id,
                                        LeagueID = leagueID,
                                        LeagueName = leagueDictionary[leagueID],
                                        //League = new LeagueDTO { ID = leagueID, Name = leagueDictionary[leagueID] },
                                        HomeTeamName = CleanTeamName(JArray1[3].ToString()),
                                        AwayTeamName = CleanTeamName(JArray1[4].ToString()),
                                    };
                                    //Maybe add to list macth 
                                    //New Match
                                    matchDictionary.Add(id, match);
                                }
                            }
                        }


                        //EVENT ARRAY
                        if (javaScriptArray3[2].ToString().Length != 0)
                        {

                            using (var enumerator = ((JArray)javaScriptArray3[2]).GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var JArray2 = enumerator.Current;
                                    //Logger.Info("SBO Key 2: " + JArray2[2].ToString());
                                    if ((int)JArray2[2] == 0)
                                    {
                                        //Logger.Info("SBO Compare OK");
                                        string key = JArray2[0].ToString().Trim();
                                        string value = JArray2[1].ToString();
                                        if (eventsResultDictionary.ContainsKey(key) == false)
                                        {
                                            eventsResultDictionary.Add(key, value);
                                            //Logger.Info("Add Event : Key - " + key + " , value - " + value);

                                        }
                                    }
                                }
                                //Logger.Info("Finish event array");
                            }
                        }

                        //Delete macthIds
                        //Return list macth ids
                        //if (javaScriptArray3[4].ToString().Length != 0)
                        //{
                        //    string[] ids = javaScriptArray3[4].ToString().Split(',');
                        //    if (isLive)
                        //    {
                        //        LiveMatchOddDatas.RemoveAll(x => ids.Contains(x.MatchID));
                        //    }
                        //    else
                        //    {
                        //        NoneLiveMatchOddDatas.RemoveAll(x => ids.Contains(x.MatchID));
                        //    }
                        //}

                        //ODD ARRAY
                        if (javaScriptArray3[5].ToString().Length != 0)
                        {
                            //List odd update chua all ma keo truoc do
                            //Neu co tran moi khi live, makeo se map voi eventsResultDictionary
                            //Khi update, check makeo trong list match dto de update
                            //[24636656,,[0.26,-0.34]] => makeo cu
                            //[24664631,[1005873,7,1,500.00,0.00],[-0.63,0.47]] => ma keo moi ==> check trong eventsResultDictionary de get matran
                            using (var enumerator = ((JArray)javaScriptArray3[5]).GetEnumerator())
                            {
                                while (enumerator.MoveNext())
                                {
                                    var JArray5 = enumerator.Current;
                                    //Check kq tra ve phai co 3 phan tu
                                    if (JavaScriptConvert.DeserializeObject(JArray5.ToString()).Count() == 3)
                                    {
                                        string oId = JArray5[0].ToString();
                                        //Check oid trong live macth, if have
                                        //     - check array 1 (odd infor) => if have => update odd change, 
                                        //otherwise, get old odd in live macthodd
                                        // update home/away in array 2.
                                        //if no => new odd for new macth.

                                        MatchOddDTO oddObj =
                                            isLive ? LiveMatchOddDatas.FirstOrDefault(x => x.OddID == oId)
                                            : NoneLiveMatchOddDatas.FirstOrDefault(x => x.OddID == oId);
                                        if (oddObj != null)
                                        {
                                            bool isChanged = false;
                                            if (JArray5[1].ToString().Length != 0
                                                && JavaScriptConvert.DeserializeObject(JArray5[1].ToString()).Count() == 5
                                                 )
                                            {
                                                JArray oddArray = JavaScriptConvert.DeserializeObject(JArray5[1].ToString());
                                                //if (oddArray[0].ToString().Length != 0 && oddArray[1].ToString().Length != 0 && oddArray[4].ToString().Length != 0)
                                                if (oddArray[4].ToString().Length != 0)
                                                {
                                                    //Update Odd
                                                    oddObj.Odd = (float)oddArray[4];
                                                    isChanged = true;
                                                }
                                            }
                                            if (JArray5[2].ToString().Length != 0
                                                && JavaScriptConvert.DeserializeObject(JArray5[2].ToString()).Count() == 2)
                                            {
                                                JArray ha = JavaScriptConvert.DeserializeObject(JArray5[2].ToString());
                                                if (ha[0].ToString().Length != 0 && ha[1].ToString().Length != 0)
                                                {
                                                    oddObj.HomeOdd = (float)ha[0];
                                                    oddObj.AwayOdd = (float)ha[1];
                                                    isChanged = true;
                                                }
                                            }
                                            if (isChanged)
                                            {
                                                //Todo: need to check this object has been update to LiveMatchOddDatas or not
                                                result.Add(oddObj);//Add to list update change
                                            }

                                        }
                                        else
                                        {
                                            //New Odd
                                            //make sure new oid have right structure
                                            if (JArray5[1].ToString().Length != 0
                                                && JavaScriptConvert.DeserializeObject(JArray5[1].ToString()).Count() == 5
                                                && JArray5[2].ToString().Length != 0
                                                && JavaScriptConvert.DeserializeObject(JArray5[2].ToString()).Count() == 2)
                                            {
                                                JArray oddArray = JavaScriptConvert.DeserializeObject(JArray5[1].ToString());

                                                string keyMap = oddArray[0].ToString();

                                                if (eventsResultDictionary.ContainsKey(keyMap)
                                                    && matchDictionary.ContainsKey(eventsResultDictionary[keyMap]))
                                                {
                                                    ////TODO: NEED TO CHECK
                                                    NewMatchDTO match = matchDictionary[eventsResultDictionary[keyMap]];
                                                    if (match != null)
                                                    {
                                                        JArray JArray_1 = (JArray)JArray5[1];
                                                        JArray JArray_2 = (JArray)JArray5[2];

                                                        MatchOddDTO odd = ConvertMatchOddDto(oId, JArray_1[1].ToString(),
                                                            JArray_1[4].ToString(), JArray_2[0].ToString(),
                                                            JArray_2[1].ToString(), match.ID, match.HomeTeamName, match.AwayTeamName, match.LeagueID, match.LeagueName);

                                                        if (odd != null)
                                                        {
                                                            result.Add(odd);
                                                            //if (isLive)
                                                            //{
                                                            //    if (!LiveMatchOddDatas.Any(x => x.OddID == oId))
                                                            //    {
                                                            //        LiveMatchOddDatas.Add(odd);
                                                            //    }
                                                            //}
                                                            //else
                                                            //{
                                                            //    if (!NoneLiveMatchOddDatas.Any(x => x.OddID == oId))
                                                            //    {
                                                            //        NoneLiveMatchOddDatas.Add(odd);
                                                            //    }
                                                            //}
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Delete Odd
                        //if (javaScriptArray3[6].ToString().Length != 0)
                        //{
                        //    //Tra ve list ma keo de xoa keo, khi da zo
                        //    //TODO
                        //    string[] ids = javaScriptArray3[6].ToString().Split(',');
                        //    if (isLive)
                        //    {
                        //        LiveMatchOddDatas.RemoveAll(x => ids.Contains(x.MatchID));
                        //    }
                        //    else
                        //    {
                        //        NoneLiveMatchOddDatas.RemoveAll(x => ids.Contains(x.MatchID));
                        //    }
                        //}
                    }

                }
            }

            return result;
        }

        public List<MatchOddDTO> ConvertUpdatedData_New(string dt, out string version, bool isLive)
        {
            StringBuilder data = new StringBuilder(dt);
            List<MatchOddDTO> result = new List<MatchOddDTO>();

            version = "";
            //1. Get event Result list
            //2. Get odd list
            //3. foreach array tran     
            //key = key de map ma tran vs ma keo, value= ma tran
            Dictionary<string, string> eventsResultDictionary = new Dictionary<string, string>();

            //key = ma keo, value = thong tin keo
            Dictionary<string, NewMatchDTO> matchDictionary = new Dictionary<string, NewMatchDTO>();
            Dictionary<string, string> leagueDictionary = new Dictionary<string, string>();


            //   data = JavaScriptConvert.CleanScriptTag(data);
            data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("\n", "");

            int index = 3;
            if (isLive)
            {
                if (dt.IndexOf("onIdle") >= 0)
                {
                    int _num = dt.IndexOf("onIdle");
                    int _num2 = dt.IndexOf(";", _num);
                    dt = dt.Substring(_num + 7, _num2 - (_num + 8));

                    //data = data.Remove(data.Length - 1);
                    version = dt.Split(new[] { ',' }, StringSplitOptions.None)[1];
                    return result;
                }
                //data = data.Replace("\\\\u200C", "").Replace("\\u200C", "").Replace("'", "");
                index = 2;
            }
            else
            {
                //$M('odds-display').onIdle(7,[188812,0]);
                //if (dt.IndexOf("onIdle") >= 0)
                //{
                //    int _num = dt.IndexOf("onIdle");
                //    int _num2 = dt.IndexOf(";", _num);
                //    data = data.Substring(_num + 7, _num2 - (_num + 8));
                //    version = data.Split(',')[1].Replace("[", "");
                //}
            }

            data = data.Remove(data.Length - 2, 2);
            data = data.Replace("$M('odds-display').onUpdate(1,", "");

            JArray javaScriptArray = JavaScriptConvert.DeserializeObject(data.ToString());

            version = javaScriptArray[0].ToString(); // = 1284 => version
            if (javaScriptArray[index].Count() >= 8)
            {
                //League Array
                if (javaScriptArray[index][0].ToString().Length != 0)
                {
                    using (var enumerator = ((JArray)javaScriptArray[index][0]).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            leagueDictionary.Add(enumerator.Current[0].ToString(), enumerator.Current[1].ToString());
                        }
                    }
                }

                //Match Array
                if (javaScriptArray[index][1].ToString().Length != 0)
                {
                    using (var enumerator = ((JArray)javaScriptArray[index][1]).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var JArray1 = enumerator.Current;
                            string id = JArray1.First.ToString();
                            string leagueID = JArray1[2].ToString();
                            NewMatchDTO match = new NewMatchDTO()
                            {
                                ID = id,
                                LeagueID = leagueID,
                                LeagueName = leagueDictionary[leagueID],
                                //League = new LeagueDTO { ID = leagueID, Name = leagueDictionary[leagueID] },
                                HomeTeamName = CleanTeamName(JArray1[3].ToString()),
                                AwayTeamName = CleanTeamName(JArray1[4].ToString()),
                            };
                            //Maybe add to list macth 
                            //New Match
                            matchDictionary.Add(id, match);
                        }
                    }
                }

                //EVENT ARRAY
                if (javaScriptArray[index][2].ToString().Length != 0)
                {
                    using (var enumerator = ((JArray)javaScriptArray[index][2]).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var JArray2 = enumerator.Current;
                            //Logger.Info("SBO Key 2: " + JArray2[2].ToString());
                            if ((int)JArray2[2] == 0)
                            {
                                //Logger.Info("SBO Compare OK");
                                string key = JArray2[0].ToString().Trim();
                                string value = JArray2[1].ToString();
                                if (eventsResultDictionary.ContainsKey(key) == false)
                                {
                                    eventsResultDictionary.Add(key, value);
                                    //Logger.Info("Add Event : Key - " + key + " , value - " + value);

                                }
                            }
                        }
                        //Logger.Info("Finish event array");
                    }
                }

                //ODD ARRAY
                if (javaScriptArray[index][5].ToString().Length != 0)
                {
                    //List odd update chua all ma keo truoc do
                    //Neu co tran moi khi live, makeo se map voi eventsResultDictionary
                    //Khi update, check makeo trong list match dto de update
                    //[24636656,,[0.26,-0.34]] => makeo cu
                    //[24664631,[1005873,7,1,500.00,0.00],[-0.63,0.47]] => ma keo moi ==> check trong eventsResultDictionary de get matran
                    using (var enumerator = ((JArray)javaScriptArray[index][5]).GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var JArray5 = enumerator.Current;
                            //Check kq tra ve phai co 3 phan tu
                            if (JavaScriptConvert.DeserializeObject(JArray5.ToString()).Count() == 3)
                            {
                                string oId = JArray5[0].ToString();
                                //Check oid trong live macth, if have
                                //     - check array 1 (odd infor) => if have => update odd change, 
                                //otherwise, get old odd in live macthodd
                                // update home/away in array 2.
                                //if no => new odd for new macth.

                                MatchOddDTO oddObj = isLive ? LiveMatchOddDatas.FirstOrDefault(x => x.OddID == oId)
                                                            : NoneLiveMatchOddDatas.FirstOrDefault(x => x.OddID == oId);
                                if (oddObj != null)
                                {
                                    bool isChanged = false;
                                    if (JArray5[1].ToString().Length != 0
                                        && JavaScriptConvert.DeserializeObject(JArray5[1].ToString()).Count() == 5
                                         )
                                    {
                                        JArray oddArray = JavaScriptConvert.DeserializeObject(JArray5[1].ToString());
                                        //if (oddArray[0].ToString().Length != 0 && oddArray[1].ToString().Length != 0 && oddArray[4].ToString().Length != 0)
                                        if (oddArray[4].ToString().Length != 0)
                                        {
                                            //Update Odd
                                            oddObj.Odd = (float)oddArray[4];
                                            isChanged = true;
                                        }
                                    }
                                    if (JArray5[2].ToString().Length != 0
                                        && JavaScriptConvert.DeserializeObject(JArray5[2].ToString()).Count() == 2)
                                    {
                                        JArray ha = JavaScriptConvert.DeserializeObject(JArray5[2].ToString());
                                        if (ha[0].ToString().Length != 0 && ha[1].ToString().Length != 0)
                                        {
                                            oddObj.HomeOdd = (float)ha[0];
                                            oddObj.AwayOdd = (float)ha[1];
                                            isChanged = true;
                                        }
                                    }
                                    if (isChanged)
                                    {
                                        //Todo: need to check this object has been update to LiveMatchOddDatas or not
                                        result.Add(oddObj);//Add to list update change
                                    }

                                }
                                else
                                {
                                    //New Odd
                                    //make sure new oid have right structure
                                    if (JArray5[1].ToString().Length != 0
                                        && JavaScriptConvert.DeserializeObject(JArray5[1].ToString()).Count() == 5
                                        && JArray5[2].ToString().Length != 0
                                        && JavaScriptConvert.DeserializeObject(JArray5[2].ToString()).Count() == 2)
                                    {
                                        JArray oddArray = JavaScriptConvert.DeserializeObject(JArray5[1].ToString());

                                        string keyMap = oddArray[0].ToString();

                                        if (eventsResultDictionary.ContainsKey(keyMap)
                                            && matchDictionary.ContainsKey(eventsResultDictionary[keyMap]))
                                        {
                                            ////TODO: NEED TO CHECK
                                            NewMatchDTO match = matchDictionary[eventsResultDictionary[keyMap]];
                                            if (match != null)
                                            {
                                                JArray JArray_1 = (JArray)JArray5[1];
                                                JArray JArray_2 = (JArray)JArray5[2];

                                                MatchOddDTO odd = ConvertMatchOddDto(oId, JArray_1[1].ToString(),
                                                    JArray_1[4].ToString(), JArray_2[0].ToString(),
                                                    JArray_2[1].ToString(), match.ID, match.HomeTeamName, match.AwayTeamName, match.LeagueID, match.LeagueName);

                                                if (odd != null)
                                                {
                                                    result.Add(odd);

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion

        #region Process Bet

        //http://t0x313uroa0e.asia.currybread.com/web-root/restricted/ticket/ticket.aspx?
        //loginname=e9b36d3855a3014490a0ff0bb97179b0
        //&id=28325902
        //&op=h => cuoc doi nha, a => doi khach
        //&odds=0.87
        //&hdpType=0 => keo dong banh, 1: keo Home chap, 2:keo Away chap
        //&isor=0&isLive=0
        //&betpage=10 ==> 10: trong page TRUC TIEP, 18 => Trong page HOM NAY
        //&style=1 => keo ma lai

        public PrepareBetDTO PrepareBet(MatchOddDTO matchOdd, eBetType betType, bool isLive, float ibetOddDef)
        {
            //Logger.InfoFormat("SBO repare {0}/{1} - {2} Ibet odd: {3} Sbo odd {4} }} Pick {5}", matchOdd.HomeTeamName,
            //    matchOdd.AwayTeamName, matchOdd.OddType, ibetOddDef, matchOdd.Odd, betType);
            //Logger.Info("SBO: START->PrepareBet");
            string oid = matchOdd.OddID;
            string op;
            float betOdd;
            //float fBetOdd;
            switch (betType)
            {
                case eBetType.Home:
                    op = "h";
                    betOdd = matchOdd.HomeOdd;
                    //fBetOdd = matchOdd.HomeOdd;
                    break;
                case eBetType.Away:
                    op = "a";
                    betOdd = matchOdd.AwayOdd;
                    //fBetOdd = matchOdd.AwayOdd;
                    break;
                default:
                    throw new Exception("PrepareBet => FAIL : Unknow betType param");
            }
            //string hdpType;
            //if (matchOdd.Odd > 0)
            //{
            //    hdpType = "1";
            //}
            //else if (matchOdd.Odd < 0)
            //{
            //    hdpType = "2";
            //}
            //else { hdpType = "0"; }
            string betpage = isLive ? "10" : "18";
            string url = "web-root/restricted/ticket/ticket.aspx?loginname=" + SboLoginName +
                "&id=" + oid + "&op=" + op + "&odds=" + betOdd +
                "&hdpType=&isor=0&isLive=0&betpage=" + betpage + "&style=1";
            //"&hdpType=" + hdpType + "&isor=0&isLive=0&betpage=" + betpage + "&style=1";

            PrepareBetSbobet prepareBetDto = new PrepareBetSbobet(matchOdd);
            var result = Get(url);
            //DataTimePrepare = DateTime.Now;
            //if (result.StatusCode == HttpStatusCode.OK && !string.IsNullOrEmpty(result.Result))
            if (!string.IsNullOrEmpty(result.Result))
            {
                string data = result.Result;
                //Xu li bi crash
                //$M('ticket').onOddsEventClosed(6)
                //if (data.IndexOf("onOddsEventClosed") >= 0)
                //{
                //    //Todo:

                //}

                if (data.IndexOf("$M('ticket').onUpdate") >= 0)
                {
                    //Logger.Info(EngineName + " PREPARE SBO DATA : " + data);
                    int n1 = data.IndexOf('[');
                    int n2 = data.LastIndexOf(']');
                    data = data.Substring(n1, (n2 - n1) + 1);
                    data = data.Replace("\\", "");
                    data = JavaScriptConvert.CleanScriptTag(data);
                    JArray array = JavaScriptConvert.DeserializeObject(data);
                    if (array.ToString().Length != 0)
                    {
                        var oddDefRaw = JavaScriptConvert.DeserializeObject(array[6].ToString());
                        var oddDefData = oddDefRaw[0].ToString()
                       .Replace(" ", "")
                       .Replace("\r", "")
                       .Replace("\n", "")
                       .Replace("[", "")
                       .Replace("]", "")
                       .Replace("\'", "").Replace("\"", "");
                        float oddDefNew = Convert.ToSingle(oddDefData.Split(new[] { ':' }, StringSplitOptions.None)[1]);

                        if (HasOddDefNoChange(matchOdd.OddType, betType, ibetOddDef, oddDefNew))
                        {
                            prepareBetDto.MinBet = (int)array[12];
                            prepareBetDto.MaxBet = (int)array[13];
                            prepareBetDto.BetType = betType;
                            prepareBetDto.MatchOdd = matchOdd;
                            prepareBetDto.IsLive = isLive;
                            if (isLive)
                            {
                                int hScore = (int)array[23];
                                int aScore = (int)array[24];
                                prepareBetDto.HasScore = (hScore != 0 || aScore != 0);
                                prepareBetDto.HomeScore = hScore;
                                prepareBetDto.AwayScore = aScore;
                            }
                            float odd = (float)array[9];
                            //prepareBetDto.HasChangeOdd = odd != fBetOdd;
                            if (betType == eBetType.Home)
                            {
                                matchOdd.HomeOdd = odd;
                            }
                            else
                            {
                                matchOdd.AwayOdd = odd;
                            }
                            matchOdd.NewUpdateOdd = odd;
                            prepareBetDto.NewOdd = odd;

                            //BetProcessSbobet betProcessSbobet = new BetProcessSbobet();
                            //betProcessSbobet.BetCount = array[19].ToString();
                            //betProcessSbobet.BetPage = array[20].ToString();
                            //betProcessSbobet.TimeProcessBet = GetDateTimeZone();

                            //prepareBetDto.BetProcessSbobet = betProcessSbobet;

                            prepareBetDto.BetCount = array[19].ToString();
                            prepareBetDto.BetPage = array[20].ToString();
                            prepareBetDto.TimeProcessBet = GetDateTimeZone();

                            //BetQueue.Add(new MatchBag(matchOdd.MatchID, matchOdd.Odd, betOdd, prepareBetDto, BetQueue));
                            BetMessageQueue = prepareBetDto;
                            //TempBetMessageQueue = prepareBetDto;
                            //Logger.Info("SBO: END->PrepareBet");
                            return prepareBetDto;
                        }
                        else
                        {
                            matchOdd.Odd = oddDefNew;
                        }

                    }
                }
            }
            else
            {
                UpdateException(this, eExceptionType.RequestFail);
            }
            //Logger.Error(EngineName + ":::PrepareBet Status: " + result.StatusDescription + "   :::PrepareBet MESSAGE: " + result.Result);
            //Logger.ErrorFormat("SBO: END->PrepareBet : FAIL Status {0} Message {1}", result.StatusDescription,
            //    result.Result);
            return null;
        }

        private bool HasOddDefNoChange(eOddType oddType, eBetType betType, float oldDef, float newDef)
        {
            if (oddType == eOddType.OU || oddType == eOddType.HalfOU)
            {
                return Math.Abs(oldDef).Equals(Math.Abs(newDef));
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
        }
        #endregion

        #region Utility functions

        public bool ConfirmBet(int stake, float ibetOddDef, bool isLive = false, eServerScan serverScan = eServerScan.Local)
        {
            //SBO CONFIRM MESSAGE: $M('ticket').onOddsEventClosed(6);
            try
            {
                TempBetMessageQueue = BetMessageQueue;

                string firstBetResult = ConfirmBetRequest(stake);
                if (firstBetResult.IndexOf("onUpdateBetCreditFromTicket()") >= 0)
                {
                    this.FireLogBet(TempBetMessageQueue.MatchOdd, TempBetMessageQueue.BetType, stake, eBetStatusType.Success, serverScan);
                    //BetMessageQueue = null;
                    return true;
                }
                bool isSuccess = false;
                //for (int i = 0; i < Rebet; i++)

                //var matchBet=
                for (int i = 0; i < Rebet; i++)
                {
                    if (!string.IsNullOrEmpty(firstBetResult) && firstBetResult.IndexOf("onOddsEventClosed") < 0)
                    {
                        if (firstBetResult.IndexOf("$M('ticket').resetOProcessing()") >= 0)
                        {
                            int n1 = firstBetResult.IndexOf('[');
                            int n2 = firstBetResult.LastIndexOf(']');
                            firstBetResult = firstBetResult.Substring(n1, (n2 - n1) + 1);
                            firstBetResult = firstBetResult.Replace("\\", "");
                            firstBetResult = JavaScriptConvert.CleanScriptTag(firstBetResult);
                            JArray array = (JArray)JavaScriptConvert.DeserializeObject(firstBetResult);
                            TempBetMessageQueue.BetCount = array[19].ToString();
                            TempBetMessageQueue.BetPage = array[20].ToString();
                            TempBetMessageQueue.TimeProcessBet = GetDateTimeZone();
                            //this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, false);
                            string second = ConfirmBetRequest(stake);
                            if (second.IndexOf("onUpdateBetCreditFromTicket()") >= 0)
                            {
                                isSuccess = true;
                                this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, eBetStatusType.MissOddSbo, serverScan);
                                //BetMessageQueue = null;
                                break;
                            }
                            if (second.IndexOf("$M('ticket').resetOProcessing()") >= 0 || second.IndexOf("onOddsEventClosed") >= 0)
                            {
                                //this.FireLogBet(TempBetMessageQueue.MatchOdd, TempBetMessageQueue.BetType, stake, false);
                                firstBetResult = second;
                            }

                        }
                        //else
                        //{
                        //    this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, false);
                        //}
                    }
                    else //Event close
                    {
                        //Logger.Info("BET SUCCESS!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                        Thread.Sleep(450);
                        Logger.Info("REBET CLOSE: " + i);
                        //TempBetMessageQueue.TimeProcessBet = GetDateTimeZone();
                        string second = ConfirmBetRequestWhenCloseEvent(TempBetMessageQueue, stake, isLive, ibetOddDef);

                        if (!string.IsNullOrEmpty(second))
                        {
                            if (second.IndexOf("onUpdateBetCreditFromTicket()") >= 0)
                            {
                                isSuccess = true;
                                this.FireLogBet(TempBetMessageQueue.MatchOdd, TempBetMessageQueue.BetType, stake, eBetStatusType.MissOddSbo, serverScan);
                                //TempBetMessageQueue = null;
                                break;
                            }
                            if (second.IndexOf("$M('ticket').resetOProcessing()") >= 0)
                            {
                                //this.FireLogBet(TempBetMessageQueue.MatchOdd, TempBetMessageQueue.BetType, stake, false);
                                firstBetResult = second;
                            }
                        }
                    }

                }

                if (!isSuccess)
                {
                    this.FireLogBet(TempBetMessageQueue.MatchOdd, TempBetMessageQueue.BetType, stake, eBetStatusType.Fail, serverScan);
                }

                return isSuccess;
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return false;
            }

            //BetMessageQueue = null;
            ////TODO:QUOC TEST
            // BetMessageQueue = null;

        }
       
        public string ConfirmBetRequest(int stake)
        {
            // http://328ef3532200.asia.currybread.com/web-root/restricted/ticket/confirm.aspx?loginname=014393ea8a1362a175b2deb2e9a3f567&sameticket=0
            //&betcount=0&stake=50&ostyle=1&stakeInAuto=50&betpage=18&acceptIfAny=0&autoProcess=0&autoRefresh=0
            //&oid=21053845&timeDiff=6085
            string url = "web-root/restricted/ticket/confirm.aspx?loginname=" + SboLoginName + "&sameticket=0"
                    + "&betcount=" + TempBetMessageQueue.BetCount
                    + "&stake=" + stake
                    + "&ostyle=1&stakeInAuto=" + stake
                    + "&betpage=" + TempBetMessageQueue.BetPage
                    + "&acceptIfAny=0&autoProcess=0&autoRefresh=0"
                    + "&oid=" + TempBetMessageQueue.MatchOdd.OddID
                    + "&timeDiff=" + GenerateTimeDiff(TempBetMessageQueue.TimeProcessBet);
            SendResponse result = Get(url);
            Logger.Info("MESSAGE: " + result.Result);
            //Logger.Info("SBO CONFIRM MESSAGE: " + result.Result);
            return result.Result;
        }

        public string ConfirmBetRequestWhenCloseEvent(PrepareBetSbobet prepare, int stake, bool isLive, float oddDef)
        {
            // http://328ef3532200.asia.currybread.com/web-root/restricted/ticket/confirm.aspx?loginname=014393ea8a1362a175b2deb2e9a3f567&sameticket=0
            //&betcount=0&stake=50&ostyle=1&stakeInAuto=50&betpage=18&acceptIfAny=0&autoProcess=0&autoRefresh=0
            //&oid=21053845&timeDiff=6085

            MatchOddDTO sboMatchTarget;
            ////TODO:QUOCLE NEED CHECK OddDef
            if (isLive)
            {
                lock (LockLive)
                {
                    sboMatchTarget = LiveMatchOddDatas.FirstOrDefault(m =>
                        m.MatchID == prepare.MatchOdd.MatchID && m.Odd == oddDef &&
                        m.OddType == prepare.MatchOdd.OddType);
                }
            }
            else
            {
                lock (LockNonLive)
                {
                    sboMatchTarget = NoneLiveMatchOddDatas.FirstOrDefault(m =>
                        m.MatchID == prepare.MatchOdd.MatchID && m.Odd == oddDef &&
                        m.OddType == prepare.MatchOdd.OddType);
                }
            }

            if (sboMatchTarget != null)
            {///todo:
                var preparese = PrepareBet(sboMatchTarget, prepare.BetType, isLive, oddDef);
                if (preparese != null)
                {
                    TempBetMessageQueue = BetMessageQueue;
                    string url = "web-root/restricted/ticket/confirm.aspx?loginname=" + SboLoginName + "&sameticket=0"
                        + "&betcount=" + TempBetMessageQueue.BetCount
                        + "&stake=" + stake
                        + "&ostyle=1&stakeInAuto=" + stake
                        + "&betpage=" + TempBetMessageQueue.BetPage
                        + "&acceptIfAny=0&autoProcess=0&autoRefresh=0"
                        + "&oid=" + TempBetMessageQueue.MatchOdd.OddID
                        + "&timeDiff=" + GenerateTimeDiff(TempBetMessageQueue.TimeProcessBet);
                    SendResponse result = Get(url);
                    Logger.Info("MESSAGE: " + result.Result);
                    //Logger.Info("SBO CONFIRM MESSAGE: " + result.Result);
                    return result.Result;
                }
            }

            return null;
        }

        public string GetBetList()
        {
            var result = Get(SbobetConfig.URL_BET_LIST);
            if (result.StatusCode == HttpStatusCode.OK &&
               !string.IsNullOrEmpty(result.Result))
            {
                return result.Result;
            }
            return "";
        }

        //http://kjo6ir80x346.asia.currybread.com/web-root/restricted/betlist/bet-list-all.aspx?d=2015-04-26&option=1&p=sb
        public string GetStatement(DateTime date)
        {
            //"yyyy-MM-dd
            var result = Get(string.Format(SbobetConfig.URL_BET_LIST_ALL, date.ToString("yyyy-MM-dd")));
            if (result.StatusCode == HttpStatusCode.OK &&
               !string.IsNullOrEmpty(result.Result))
            {
                return result.Result;
            }
            return "";
        }

        public float UpdateAvailabeCredit()
        {
            AvailabeCredit = GetAvailabeCredit();
            return AvailabeCredit;
        }

        /// <summary>
        /// Get the current Availabe Credit 
        /// </summary>
        /// <returns>float</returns>
        public float GetAvailabeCredit()
        {
            //$M('page-head').onUpdateBetCreditCallback('70.90');
            float credit = 0;
            var result = Get(SbobetConfig.URL_BALANCE_CREDIT);
            if (result.StatusCode == HttpStatusCode.OK &&
               !string.IsNullOrEmpty(result.Result))
            {
                string value = "$M('page-head').onUpdateBetCreditCallback";
                string data = result.Result;
                if (data.IndexOf(value) >= 0)
                {
                    int n1 = value.Length + 2;
                    int n2 = data.Length;
                    data = data.Substring(n1, (n2 - 3) - n1);
                    float.TryParse(data, out credit);
                }
            }
            else
            {
                UpdateException(this, eExceptionType.LoginFail);
            }
            return credit;

        }

        public bool CheckLogin()
        {
            bool isSuccess = false;
            if (!string.IsNullOrEmpty(UrlHost))
            {
                var result = Get(SbobetConfig.URL_MINI_BET_LIST);
                if (result.StatusCode == HttpStatusCode.OK &&
                   !string.IsNullOrEmpty(result.Result))
                {
                    if (result.Result.IndexOf("$M('mini-bet-list')") >= 0)
                    {
                        isSuccess = true;
                    }
                    else
                    {
                        Logger.Error("MESSAGE: " + result.Result);
                        //Logger.ErrorFormat("CheckLogin : FAIL ==> message status:{0} | message result:{1}",
                        //    result.StatusDescription, result.Result);
                        UpdateException(this, eExceptionType.LoginFail);
                    }
                }
            }
            return isSuccess;
        }

        public AccProfileDTO GetAccountProfile()
        {
            AccProfileDTO result = new AccProfileDTO();
            result.UrlHost = this.Host;
            result.Username = this.UserName;
            result.AvailabeCredit = GetAvailabeCredit();
            return result;

        }
        #endregion

        #region private methods

        private SendResponse Get(string urlRequest)
        {
            urlRequest = string.Concat("http://", this.Host, "/", urlRequest);
            //string refer = string.Format(SbobetConfig.URL_LOGIN_NAME_REFER, Host, SboLoginName);
            var msg = SendSbo(urlRequest, "GET", userAgent, CookieContainer, null,
                UrlHost, Host, accept);

            if (string.IsNullOrEmpty(msg.Result))
            {
                Logger.Error("REQUEST FAIL, MESSAGE EMTRY. RequestURL::::" + urlRequest);
                UpdateException(this, eExceptionType.RequestFail);
                return msg;
            }

            if (msg.Result.Contains("ogout") || msg.Result.Contains("OGOUT") || msg.Result.Contains("ErrorCode"))
            {
                Logger.Error("REQUEST LOGOUT -> MESSAGE: " + msg.Result);
                UpdateException(this, eExceptionType.LoginFail);
                return msg;
            }

            UpdateException(this);
            return msg;
        }

        private MatchOddDTO ConvertMatchOddDto(string id, string oddType, string odd, string home, string away, string matchId, string homeTeamName, string awayTeamName, string leagueID, string leagueName)
        {
            eOddType type = ConvertOddType(oddType);

            if (type == eOddType.Unknown || home == "" || away == "")
            {
                return null;
            }
            return new MatchOddDTO()
            {
                OddID = id,
                Odd = float.Parse(odd),
                OddType = type,
                HomeOdd = float.Parse(home),
                AwayOdd = float.Parse(away),
                HomeTeamName = homeTeamName,
                AwayTeamName = awayTeamName,
                ServerType = eServerType.Sbo,
                LeagueID = leagueID,
                LeagueName = leagueName,
                MatchID = matchId
            };
        }

        private static eOddType ConvertOddType(string oddType)
        {
            eOddType type = eOddType.Unknown;
            switch (oddType)
            {
                case "1":
                    type = eOddType.HCP;
                    break;
                case "2":
                    type = eOddType.Unknown;
                    break;
                case "3":
                    type = eOddType.OU;
                    break;
                case "5":
                    //oddDto.Type = eOddType.HCP; Keo chau au
                    break;
                case "7":
                    type = eOddType.HalfHCP;
                    break;
                case "8":
                    // oddDto.Type = eOddType.HCP; Keo chau au
                    break;
                case "9":
                    type = eOddType.HalfOU;
                    break;
                default:
                    type = eOddType.Unknown;
                    break;
            }
            return type;
        }

        private string GenerateTimeDiff(DateTime dt)
        {
            TimeSpan difference = GetDateTimeZone() - dt;
            return Math.Round(difference.TotalMilliseconds, 0).ToString();
        }

        #endregion

        public void TryFakeRequest(string url)
        {
            if (OnFakeRequest != null)
            {
                OnFakeRequest(url);
            }
        }
    }


}
