namespace BcWin.Common.DTO
{
    public class AccProfileDTO
    {
        public string UrlHost { get; set; }
        //public string LoginName { get; set; } //Tên đăng nhập        
        //public string AccountID { get; set; } //Số tài khỏan
        public string Username { set; get; }
        //public string CashBalance { get; set; } //Tiền mặt
        //public string OutstandingTxn { get; set; } //Cược đang chờ kết qủa        
        //public string GivenCredit { get; set; } //Số credit đang co
        public float AvailabeCredit { get; set; } //Availabe Credit / Số credit có thể cược
        public float CashBalance { get; set; }
    }
}
