using System.ComponentModel;
using System.Drawing;

namespace BcWin.Common.Objects
{
    public class AccountSummary
    {
        [Browsable(false)]
        public int TabCode { get; set; }

        public string TabName { get; set; }
        public Bitmap Status { get; set; }

        public string Success
        {
            get { return IbetSuccess + " - " + SboSuccess; }
        }
        public string Miss
        {
            //get { return IbetMiss + " - " + SboMiss; }
            get { return SboFail.ToString(); }
        }
        public string RebetLoss
        {
            //get { return IbetRebet + " - " + SboFail; }
            get { return IbetRebet.ToString(); }
        }

        public string RebetSbo
        {
            //get { return IbetRebet + " - " + SboFail; }
            get { return SboMiss.ToString(); }
        }

        [Browsable(false)]
        public int IbetFail { get; set; }

        [Browsable(false)]
        public int IbetSuccess { get; set; }
        [Browsable(false)]
        public int SboSuccess { get; set; }
        [Browsable(false)]
        public int IbetMiss { get; set; }
        [Browsable(false)]
        public int SboMiss { get; set; }
        [Browsable(false)]
        public int IbetRebet { get; set; }
        [Browsable(false)]
        public int SboRebet { get; set; }

        [Browsable(false)]
        public int SboFail { get; set; }
    }
}
