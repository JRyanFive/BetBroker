using BcWin.Common.DTO;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BcWin.Engine.FrOneOne
{
    public class FrEngine
    {
        private static string _token = string.Empty;
        private const string Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
        private const string Accept_Encoding = "gzip, deflate";
        private const string Accept_Language = "en-US,en;q=0.5";
        private const string Connection = "keep-alive";
        private const string Host = "fr1111.com";
        private const string RefererScanLive = "{0}/app/member/FT_browse/index.php?uid={1}&langx=en-us&mtype=4&league_id=&showtype=";
        private const string RefererPrepareBet = "{0}/app/member/select.php?uid={1}&langx=en-us";
        private const string RefererConfirmBet = "{0}/app/member/FT_order/FT_order_re.php?gid={1}&uid={2}&odd_f_type=M&type={3}&gnum={4}&strong=C&langx=en-us";
        private const string User_Agent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0";
        private string _urlDomain = "http://fr1111.com";

        public int TotalParseTime = 0;

        public FrEngine()
        {
         //    ServicePointManager.Expect100Continue = false;
          //  ServicePointManager.UseNagleAlgorithm = false;

        }
        public bool Login(string urlDomain, string userName, string password, string langx = "en-us")
        {
            try
            {
                urlDomain = String.IsNullOrEmpty(urlDomain) ? _urlDomain : urlDomain;
                if (urlDomain[urlDomain.Length - 1] == '/')
                {
                    urlDomain = urlDomain.Remove(urlDomain.Length - 1);
                }
                _urlDomain = urlDomain;

                string urlLogin = _urlDomain + "/app/member/new_login.php?username={0}&passwd={1}&langx={2}";
                string urlCall = String.Format(urlLogin, userName, password, langx);

                var request = CreateCommonRequest(urlCall);

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse == true && response != null)
                {
                    // Get the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var st = reader.ReadToEnd();
                        string[] arrayResult = st.Split('|');

                        if (String.IsNullOrEmpty(arrayResult[3]))
                        {
                            return false;
                        }

                        _token = arrayResult[3];

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

        private  int time = 0;
        private  string teamAId = "";
        private  string teamBId = "";

        private  string OddFullId = "";
        private  string OddHalfId = "";

        private  string typeFull = "";
        private  string typeHalf = "";
        private  float homeOdd = 0f;
        private  float awayOdd = 0f;
        private static string[] oneRow = null;

        public void ScanLive()
        {
            string url = _urlDomain + String.Format("/app/member/FT_browse/body_var.php?uid={0}&rtype=re&langx=en-us&mtype=4&page_no=0&league_id=&hot_game=", _token);

            try
            {
                HttpWebRequest request = CreateCommonRequest(url);
                //add more Referer;
                request.Referer = String.Format(RefererScanLive, _urlDomain, _token);
                //string[] oneRow = null;

                // handicap and OU fulltime
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse == true && response != null)
                {

                    // Get the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var st = reader.ReadToEnd();
                        DateTime from = DateTime.Now;

                        //doc.LoadHtml(st);
                        //HtmlNode data = doc.DocumentNode.SelectNodes("//script").Last();
                        //string[] resultStrings = data.InnerHtml.Split(new String[] { "g([" }, StringSplitOptions.None);
                        string[] resultStrings = st.Split(new String[] { "g([" }, StringSplitOptions.None);
                        if (resultStrings == null || resultStrings.Length == 1)
                        {
                            Debug.WriteLine("No DATA");
                            return;
                        }

                        #region for except last
                        for (int i = 1; i < resultStrings.Length - 1; i++)
                        {
                            oneRow = resultStrings[i].Split(',');
                            string matchId = oneRow.Last().Replace("']);\n", "").Remove(0, 1);

                            var match = FrData.LiveMatchOddBag.FirstOrDefault(m => m.MatchID == matchId);
                            if (match == null)
                            {
                                time = 0;
                                int.TryParse(oneRow[1].Replace("'", ""), out time);
                                InsertNewMatch(oneRow, matchId, time);
                            }
                            #region update match
                            else
                            {
                                UpdateOddMatch(oneRow, match);

                            }
                            #endregion
                            //TimeSpan x = DateTime.Now - from;
                            //Debug.WriteLine("First line parse: " + x.Milliseconds);
                        }
                        #endregion

                        #region handle last

                        string temp = resultStrings.Last().Split(new string[] { "]);" }, StringSplitOptions.None)[0];
                        oneRow = temp.Split(',');
                        string matchIdLast = oneRow.Last().Replace("']);\n", "").Remove(0, 1);

                        var match2 = FrData.LiveMatchOddBag.FirstOrDefault(m => m.MatchID == matchIdLast);
                        if (match2 == null)
                        {
                            time = 0;
                            int.TryParse(oneRow[1].Replace("'", ""), out time);
                            InsertNewMatch(oneRow, matchIdLast, time);
                        }
                        else
                        {
                            UpdateOddMatch(oneRow, match2);
                        }


                        #endregion

                        TimeSpan esplapse = DateTime.Now - from;
                        TotalParseTime = esplapse.Milliseconds;

                      //  Debug.WriteLine("THOI GIAN PARSE: " + esplapse.Milliseconds);
                    }


                }

            }
            catch (Exception)
            {
            }
        }

        public void UpdateOddMatch(string[] oneRow, MatchDTO match)
        {
            teamAId = oneRow[3].Replace("'", "");//Home id
            teamBId = oneRow[4].Replace("'", "");//Customer id

            OddFullId = oneRow[0].Replace("'", "");
            OddHalfId = oneRow[20].Replace("'", "");

            typeFull = oneRow[7].Replace("'", "");
            typeHalf = oneRow[21].Replace("'", "");

            // find oddMatch in match;
            #region Update home and away HDC FULLTIME
            var oddHDC_HomeFull = match.Odds.FirstOrDefault(o => o.TeamId == teamAId && o.OddID == OddFullId && o.OddType == eOddType.HCP);
            if (oddHDC_HomeFull != null)
            {
                if (oneRow[8] == "''") // Odd null
                {
                    oddHDC_HomeFull.IsDeleted = true;
                }
                else
                {
                    oddHDC_HomeFull.type = oneRow[7].Replace("'", "");
                    oddHDC_HomeFull.Odd = ConvertOddHandicap(oneRow[8], oneRow[7]);
                    oddHDC_HomeFull.HomeOdd = ConvertOddHandicap(oneRow[9]); // full hdc
                    oddHDC_HomeFull.AwayOdd = ConvertOddHandicap(oneRow[10]); // full hdc

                    oddHDC_HomeFull.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[8] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[9]);
                    awayOdd = ConvertOddHandicap(oneRow[10]);
                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddFullId,
                        type = typeFull,
                        TeamId = teamAId,
                        Odd = ConvertOddHandicap(oneRow[8], typeFull),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HCP,
                    });
                }
            }

            var oddHDC_AwayFull = match.Odds.FirstOrDefault(o => o.TeamId == teamBId && o.OddID == OddFullId && o.OddType == eOddType.HCP);
            if (oddHDC_AwayFull != null)
            {
                if (oneRow[8] == "''") // Odd null
                {
                    oddHDC_AwayFull.IsDeleted = true;
                }
                else
                {
                    oddHDC_AwayFull.type = oneRow[7].Replace("'", "");
                    oddHDC_AwayFull.Odd = ConvertOddHandicap(oneRow[8], oneRow[7]);
                    oddHDC_AwayFull.HomeOdd = ConvertOddHandicap(oneRow[9]); // full hdc
                    oddHDC_AwayFull.AwayOdd = ConvertOddHandicap(oneRow[10]); // full hdc

                    oddHDC_AwayFull.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[8] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[9]);
                    awayOdd = ConvertOddHandicap(oneRow[10]);
                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddFullId,
                        type = typeFull,
                        TeamId = teamBId,
                        Odd = ConvertOddHandicap(oneRow[8], typeFull),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HCP,
                    });
                }
            }
            #endregion

            #region Update home and away HDC HALFTIME

            var oddHDC_HomeHalf = match.Odds.FirstOrDefault(o => o.TeamId == teamAId && o.OddID == OddHalfId && o.OddType == eOddType.HalfHCP);
            if (oddHDC_HomeHalf != null)
            {
                if (oneRow[22] == "''") // Odd null
                {
                    oddHDC_HomeHalf.IsDeleted = true;
                }
                else
                {
                    oddHDC_HomeHalf.type = oneRow[21].Replace("'", "");
                    oddHDC_HomeHalf.Odd = ConvertOddHandicap(oneRow[22], oneRow[21]);
                    oddHDC_HomeHalf.HomeOdd = ConvertOddHandicap(oneRow[23]); // full hdc
                    oddHDC_HomeHalf.AwayOdd = ConvertOddHandicap(oneRow[24]); // full hdc

                    oddHDC_HomeHalf.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[22] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[23]);
                    awayOdd = ConvertOddHandicap(oneRow[24]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddHalfId,
                        type = typeHalf,
                        TeamId = teamAId,
                        Odd = ConvertOddHandicap(oneRow[22], typeHalf),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HalfHCP,

                    });
                }


            }

            var oddHDC_AwayHalf = match.Odds.FirstOrDefault(o => o.TeamId == teamBId && o.OddID == OddHalfId && o.OddType == eOddType.HalfHCP);
            if (oddHDC_AwayHalf != null)
            {
                if (oneRow[22] == "''") // Odd null
                {
                    oddHDC_AwayHalf.IsDeleted = true;
                }
                else
                {
                    oddHDC_AwayHalf.type = oneRow[21].Replace("'", "");
                    oddHDC_AwayHalf.Odd = ConvertOddHandicap(oneRow[22], oneRow[21]);
                    oddHDC_AwayHalf.HomeOdd = ConvertOddHandicap(oneRow[23]); // full hdc
                    oddHDC_AwayHalf.AwayOdd = ConvertOddHandicap(oneRow[24]); // full hdc

                    oddHDC_AwayHalf.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[22] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[23]);
                    awayOdd = ConvertOddHandicap(oneRow[24]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddHalfId,
                        type = typeHalf,
                        TeamId = teamBId,
                        Odd = ConvertOddHandicap(oneRow[22], typeHalf),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HalfHCP,

                    });
                }
            }
            #endregion

            #region Update home and away OU FULLTIME
            var oddOU_HomeFull = match.Odds.FirstOrDefault(o => o.TeamId == teamAId && o.OddID == OddFullId && o.OddType == eOddType.OU);
            if (oddOU_HomeFull != null)
            {
                if (oneRow[11] == "''") // Over null
                {
                    oddOU_HomeFull.IsDeleted = true;
                }
                else
                {
                    oddOU_HomeFull.Odd = ConvertOddHandicap(oneRow[11]);
                    oddOU_HomeFull.HomeOdd = ConvertOddHandicap(oneRow[14]); // home odd ou
                    oddOU_HomeFull.AwayOdd = ConvertOddHandicap(oneRow[13]); // away odd ou

                    oddOU_HomeFull.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[11] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[14]);
                    awayOdd = ConvertOddHandicap(oneRow[13]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddFullId,
                        TeamId = teamAId,
                        Odd = ConvertOddOU(oneRow[11]),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.OU,
                    });


                }
            }

            var oddOU_AwayFull = match.Odds.FirstOrDefault(o => o.TeamId == teamBId && o.OddID == OddFullId && o.OddType == eOddType.OU);
            if (oddOU_AwayFull != null)
            {
                if (oneRow[12] == "''") // Under null
                {
                    oddOU_AwayFull.IsDeleted = true;
                }
                else
                {
                    oddOU_AwayFull.Odd = ConvertOddHandicap(oneRow[12]);
                    oddOU_AwayFull.HomeOdd = ConvertOddHandicap(oneRow[14]); // home odd ou
                    oddOU_AwayFull.AwayOdd = ConvertOddHandicap(oneRow[13]); // away odd ou

                    oddOU_AwayFull.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[12] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[14]);
                    awayOdd = ConvertOddHandicap(oneRow[13]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddFullId,
                        TeamId = teamBId,
                        Odd = ConvertOddOU(oneRow[12]),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.OU,
                    });
                }
            }
            #endregion

            #region Update home and away OU HALFTIME
            var oddOU_HomeHalf = match.Odds.FirstOrDefault(o => o.TeamId == teamAId && o.OddID == OddFullId && o.OddType == eOddType.HalfOU);
            if (oddOU_HomeHalf != null)
            {
                if (oneRow[25] == "''") // Over null
                {
                    oddOU_HomeHalf.IsDeleted = true;
                }
                else
                {
                    oddOU_HomeHalf.Odd = ConvertOddHandicap(oneRow[25]);
                    oddOU_HomeHalf.HomeOdd = ConvertOddHandicap(oneRow[28]); // home odd ou
                    oddOU_HomeHalf.AwayOdd = ConvertOddHandicap(oneRow[27]); // away odd ou

                    oddOU_HomeHalf.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[25] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[28]);
                    awayOdd = ConvertOddHandicap(oneRow[27]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddHalfId,
                        TeamId = teamAId,
                        Odd = ConvertOddOU(oneRow[25]),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HalfOU,

                    });
                }
            }

            var oddOU_AwayHalf = match.Odds.FirstOrDefault(o => o.TeamId == teamBId && o.OddID == OddFullId && o.OddType == eOddType.HalfOU);
            if (oddOU_AwayHalf != null)
            {
                if (oneRow[26] == "''") // Under null
                {
                    oddOU_AwayHalf.IsDeleted = true;
                }
                else
                {
                    oddOU_AwayHalf.Odd = ConvertOddHandicap(oneRow[26]);
                    oddOU_AwayHalf.HomeOdd = ConvertOddHandicap(oneRow[28]); // home odd ou
                    oddOU_AwayHalf.AwayOdd = ConvertOddHandicap(oneRow[27]); // away odd ou

                    oddOU_AwayHalf.IsDeleted = false;
                }
            }
            else
            {
                if (oneRow[26] != "''")
                {
                    homeOdd = ConvertOddHandicap(oneRow[28]);
                    awayOdd = ConvertOddHandicap(oneRow[27]);

                    match.Odds.Add(new OddDTO
                    {
                        OddID = OddHalfId,
                        TeamId = teamBId,
                        Odd = ConvertOddOU(oneRow[26]),
                        HomeOdd = homeOdd,
                        AwayOdd = awayOdd,
                        OddType = eOddType.HalfOU,

                    });
                }
            }
            #endregion
        }

        public void InsertNewMatch(string[] oneRow, string matchId, int timeMatch)
        {
            var matchTemp = new MatchDTO
            {
                MatchID = matchId,
                LeagueName = oneRow[2].Replace("'", ""),
                Minutes = timeMatch,
                HomeTeamName = oneRow[5].Replace("'", ""),
                AwayTeamName = oneRow[6].Replace("'", ""),
                ServerType = eServerType.Unknown,
                Odds = new List<OddDTO>()
            };

            teamAId = oneRow[3].Replace("'", "");
            teamBId = oneRow[4].Replace("'", "");

            OddFullId = oneRow[0].Replace("'", "");
            OddHalfId = oneRow[20].Replace("'", "");

            typeFull = oneRow[7].Replace("'", "");
            typeHalf = oneRow[21].Replace("'", "");


            // handicap and OU fulltime
            if (!String.IsNullOrEmpty(typeFull))
            {
                homeOdd = ConvertOddHandicap(oneRow[9]);
                awayOdd = ConvertOddHandicap(oneRow[10]);

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddFullId,
                    type = typeFull,
                    TeamId = teamAId,
                    Odd = ConvertOddHandicap(oneRow[8], typeFull),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HCP,
                });

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddFullId,
                    type = typeFull,
                    TeamId = teamBId,
                    Odd = ConvertOddHandicap(oneRow[8], typeFull),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HCP,
                });

                homeOdd = ConvertOddHandicap(oneRow[14]);
                awayOdd = ConvertOddHandicap(oneRow[13]);

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddFullId,
                    TeamId = teamAId,
                    Odd = ConvertOddOU(oneRow[11]),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.OU,
                });

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddFullId,
                    TeamId = teamBId,
                    Odd = ConvertOddOU(oneRow[12]),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.OU,
                });

            }

            //handicap and OU half
            if (!String.IsNullOrEmpty(typeHalf))
            {
                homeOdd = ConvertOddHandicap(oneRow[23]);
                awayOdd = ConvertOddHandicap(oneRow[24]);

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddHalfId,
                    type = typeHalf,
                    TeamId = teamAId,
                    Odd = ConvertOddHandicap(oneRow[22], typeHalf),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HalfHCP,

                });

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddHalfId,
                    type = typeHalf,
                    TeamId = teamBId,
                    Odd = ConvertOddHandicap(oneRow[22], typeHalf),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HalfHCP,

                });

                homeOdd = ConvertOddHandicap(oneRow[28]);
                awayOdd = ConvertOddHandicap(oneRow[27]);

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddHalfId,
                    TeamId = teamAId,
                    Odd = ConvertOddOU(oneRow[25]),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HalfOU,

                });

                matchTemp.Odds.Add(new OddDTO
                {
                    OddID = OddHalfId,
                    TeamId = teamBId,
                    Odd = ConvertOddOU(oneRow[26]),
                    HomeOdd = homeOdd,
                    AwayOdd = awayOdd,
                    OddType = eOddType.HalfOU,

                });
            }
            FrData.LiveMatchOddBag.Add(matchTemp);

        }

        /// <summary>
        /// tại sao ko return luôn object param để confirm gọi
        /// </summary>
        /// <param name="match"></param>
        /// <param name="odd"></param>
        /// <param name="leagueId"></param>
        /// <param name="type"></param>
        /// <param name="strong"></param>
        /// <param name="odd_f_type"></param>
        /// <returns></returns>
        public CommonParamRequest PrepareBet(OddDTO odd, string type, string strong = "C", string odd_f_type = "M")
        {
            string url = _urlDomain + String.Format("/app/member/FT_order/FT_order_re.php?gid={0}&uid={1}&odd_f_type={2}&type={3}&gnum={4}&strong={5}&langx=en-us", odd.OddID, _token, odd_f_type, type, odd.TeamId, strong);

            try
            {
                HttpWebRequest request = CreateCommonRequest(url);
                //add more Referer;
                // "http://fr1111.com/app/member/select.php?uid={0}&langx=en-us";
                request.Referer = String.Format(RefererPrepareBet, _urlDomain, _token);
                var doc = new HtmlAgilityPack.HtmlDocument();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                if (request.HaveResponse == true && response != null)
                {
                    // Get the response stream
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        var st = reader.ReadToEnd();
                        doc.LoadHtml(st);
                        var list = doc.DocumentNode.Descendants("input").Where(d => d.Attributes.Contains("type") && d.Attributes["type"].Value.Contains("hidden"));
                        if (list != null && list.Any())
                        {
                            CommonParamRequest param = new CommonParamRequest
                                         {
                                             active = list.FirstOrDefault(p => p.Attributes["name"].Value == "active").Attributes["value"].Value,
                                             autoOdd = "Y",
                                             concede_r = list.FirstOrDefault(p => p.Attributes["name"].Value == "concede_r").Attributes["value"].Value,
                                             gid = list.FirstOrDefault(p => p.Attributes["name"].Value == "gid").Attributes["value"].Value,
                                             gmax_single = list.FirstOrDefault(p => p.Attributes["name"].Value == "gmax_single").Attributes["value"].Value,
                                             gmin_single = list.FirstOrDefault(p => p.Attributes["name"].Value == "gmin_single").Attributes["value"].Value,
                                             gnum = list.FirstOrDefault(p => p.Attributes["name"].Value == "gnum").Attributes["value"].Value,
                                             ioradio_r_h = list.FirstOrDefault(p => p.Attributes["name"].Value == "ioradio_r_h").Attributes["value"].Value,
                                             langx = "en-us",
                                             line_type = list.FirstOrDefault(p => p.Attributes["name"].Value == "line_type").Attributes["value"].Value,
                                             odd_f_type = "M",// list.FirstOrDefault(p => p.Attributes["name"].Value == "odd_f_type").Attributes["value"].Value,
                                             pay_type = list.FirstOrDefault(p => p.Attributes["name"].Value == "pay_type").Attributes["value"].Value,
                                             radio_r = list.FirstOrDefault(p => p.Attributes["name"].Value == "radio_r").Attributes["value"].Value,
                                             restcredit = list.FirstOrDefault(p => p.Attributes["name"].Value == "restcredit").Attributes["value"].Value,
                                             restsinglecredit = list.FirstOrDefault(p => p.Attributes["name"].Value == "restsinglecredit").Attributes["value"].Value,
                                             singlecredit = list.FirstOrDefault(p => p.Attributes["name"].Value == "singlecredit").Attributes["value"].Value,
                                             singleorder = list.FirstOrDefault(p => p.Attributes["name"].Value == "singleorder").Attributes["value"].Value,
                                             strong = "C", //list.FirstOrDefault(p => p.Attributes["name"].Value == "strong").Attributes["value"].Value,
                                             type = list.FirstOrDefault(p => p.Attributes["name"].Value == "type").Attributes["value"].Value,
                                             uid = _token,
                                             wagerstotal = list.FirstOrDefault(p => p.Attributes["name"].Value == "wagerstotal").Attributes["value"].Value,

                                         };
                            return param;

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


        /* post param
          active	1
          autoOdd	Y
          concede_r	1
          gid	36236
          gmax_single	44000
          gmin_single	50
          gnum	70002
          gold	50
          ioradio_r_h	0.81
          langx	en-us
          line_type	4
          odd_f_type	M
          pay_type	0
          radio_r	100
          restcredit	9950
          restsinglecredit	0
          singlecredit	230000
          singleorder	115000
          strong	H
          type	H
          uid	df4nhiq6m162l186
          wagerstotal	230000
       */
        public void ConfirmBet(CommonParamRequest p)
        {
            string url = _urlDomain + "/app/member/FT_order/FT_order_re.php";

            var request = CreateCommonRequest(url);
            //"http://fr1111.com/app/member/FT_order/FT_order_re.php?gid={0}&uid={1}&odd_f_type=M&type={2}&gnum={3}&strong=C&langx=en-us";
            request.Referer = String.Format(RefererConfirmBet, _urlDomain, p.gid, _token, p.type, p.gnum);

            StringBuilder postData = new StringBuilder("active=1&autoOdd=Y");
            postData.Append("&concede_r=" + p.concede_r);
            postData.Append("&gid=" + p.gid);
            postData.Append("&gmax_single=" + p.gmax_single);
            postData.Append("&gmin_single=" + p.gmin_single);
            postData.Append("&gnum=" + p.gnum);
            postData.Append("&gold=" + p.gmin_single);
            postData.Append("&ioradio_r_h=" + p.ioradio_r_h);
            postData.Append("&langx=en-us");
            postData.Append("&line_type=" + p.line_type);
            postData.Append("&odd_f_type=" + p.odd_f_type);
            postData.Append("&pay_type=" + p.pay_type);
            postData.Append("&radio_r=" + p.radio_r);
            postData.Append("&restcredit=" + p.restcredit);
            postData.Append("&restsinglecredit=" + p.restsinglecredit);
            postData.Append("&singlecredit=" + p.singlecredit);
            postData.Append("&singleorder=" + p.singleorder);
            postData.Append("&strong=" + p.strong);
            postData.Append("&type=" + p.type);
            postData.Append("&uid=" + _token);
            postData.Append("&wagerstotal=" + p.wagerstotal);

            var data = Encoding.UTF8.GetBytes(postData.ToString());

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            if (request.HaveResponse == true && response != null)
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    var result = reader.ReadToEnd();
                    //save result
                    //  this.FireLogBet(BetMessageQueue.MatchOdd, BetMessageQueue.BetType, stake, eBetStatusType.Success, serverScan);
                }
            }

        }

        #region convert 1
        // OddValue
        private float ConvertOddHandicap(string odd, string type)
        {
            // type H la Home chap
            // type C la Customer chap
            try
            {
                odd = odd.Replace("'", "");

                string[] o = odd.Split(new string[] { " / " }, StringSplitOptions.None);

                if (type == "H")
                    return (float.Parse(o[0]) + float.Parse(o[1])) / 2;

                return -(float.Parse(o[0]) + float.Parse(o[1])) / 2;
            }
            catch (Exception)
            {
                return 0f;
            }
        }

        // convert HomeOdd or AwayOdd
        private float ConvertOddHandicap(string odd)
        {
            odd = odd.Replace("'", "");


            float value = 0f;
            float.TryParse(odd, out value);
            value = value > 1 ? value - 2 : value;

            return value;
        }

        private float ConvertOddOU(string odd)
        {
            try
            {
                odd = odd.Replace("'", "");

                string[] odds = odd.Split(new string[] { " / " }, StringSplitOptions.None);
                if (odds.Length == 2)
                {
                    return (float.Parse(odds[0].Remove(0, 1)) + float.Parse(odds[1])) / 2;
                }

                return float.Parse(odds[0].Remove(0, 1));
            }
            catch (Exception)
            {
                return 0f;
            }

        }
        #endregion

        #region convert 2
        // OddValue
        //private float ConvertOddHandicap(string odd, string type)
        //{
        //    // type H la Home chap
        //    // type C la Customer chap
        //    try
        //    {
        //        string[] o = odd.Split(new string[] { " / " }, StringSplitOptions.None);

        //        if (type == "H")
        //            return (float.Parse(o[0]) + float.Parse(o[1])) / 2;

        //        return -(float.Parse(o[0]) + float.Parse(o[1])) / 2;
        //    }
        //    catch (Exception)
        //    {
        //        return 0f;
        //    }
        //}

        //// convert HomeOdd or AwayOdd
        //private float ConvertOddHandicap(string odd)
        //{
        //    float value = 0f;
        //    float.TryParse(odd, out value);
        //    value = value > 1 ? value - 2 : value;

        //    return value;
        //}

        //private float ConvertOddOU(string odd)
        //{
        //    switch (odd)
        //    {
        //        case "O0.5 / 1":
        //            return 1;

        //    }

        //    return 0f;
        //}
        #endregion


        private HttpWebRequest CreateCommonRequest(string url)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.KeepAlive = false;
            request.Proxy = null;
            request.Method = "POST";
            request.Accept = Accept;
            request.UserAgent = User_Agent;
            request.Host = Host;
            //       request.ProtocolVersion = HttpVersion.Version10;
            request.Headers.Add("Accept-Language", "en-US,en;q=0.5");
            request.Headers.Add("Accept-Encoding", "gzip, deflate");

            return request;
        }
    }

    public class LoginResponse
    {
        public int score { get; set; }
        public int score2 { get; set; }
        public string token { get; set; }
        public int score3 { get; set; }
        public string langx { get; set; }
    }

    public class CommonParamRequest
    {
        public string autoOdd { get; set; }
        public string gold { get; set; }

        public string active { get; set; }
        public string concede_r { get; set; }
        public string gid { get; set; }
        public string gmax_single { get; set; }
        public string gmin_single { get; set; }
        public string gnum { get; set; }
        public string ioradio_r_h { get; set; }
        public string langx { get; set; }
        public string line_type { get; set; }
        public string odd_f_type { get; set; }
        public string pay_type { get; set; }
        public string radio_r { get; set; }
        public string restcredit { get; set; }
        public string restsinglecredit { get; set; }
        public string singlecredit { get; set; }
        public string singleorder { get; set; }
        public string strong { get; set; }
        public string type { get; set; }
        public string uid { get; set; }
        public string wagerstotal { get; set; }
    }
}
