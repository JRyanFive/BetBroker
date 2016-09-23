namespace BcWin.Common.DTO
{
    public class NewMatchDTO : BaseDTO
    {
        public string HomeTeamName { get; set; }
        public string AwayTeamName { get; set; }
        public string LeagueID { get; set; }

        //public LeagueDTO League
        //{
        //    get;
        //    set;
        //}
        //public string LeagueName
        //{
        //    get
        //    {
        //        string result;
        //        if (this.League == null)
        //        {
        //            result = "";
        //        }
        //        else
        //        {
        //            result = this.League.Name;
        //        }
        //        return result;
        //    }
        //}

        public string LeagueName { get; set; }
    }

}
