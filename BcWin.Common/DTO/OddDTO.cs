#define DEBUG
//using iBet.Utilities;

namespace BcWin.Common.DTO
{
    public class OddDTO
    {
        public string OddID { get; set; }
        public float Odd { get; set; }

        public eOddType OddType { get; set; }
        public float HomeOdd { get; set; }
        public float AwayOdd { get; set; }

        public bool IsDeleted { get; set; }

        #region [using for Pina]

        public double? max { get; set; }
        public string TeamId { get; set; }
        public string type { get; set; }


        public string EvId { get; set; }
        public string Lvl { get; set; }
        public string Rot { get; set; }
        public string prepareLinkHome { get; set; }
        public string prepareLinkAway { get; set; }

        public int number { get; set; }
        public int lineId { get; set; }
        public int? altLineId { get; set; }

        public float PiOdd {
            get
            {
                if (OddType == eOddType.OU || OddType == eOddType.HalfOU)
                {
                    return Odd;
                }
                return Odd * -1;
            }
        }

        #endregion

        #region [using for ISN]
        public int selectionIdHome { get; set; }
        public int selectionIdAway { get; set; }
        public float oddsHome { get; set; }
        public float oddsAway { get; set; }
        public float handicapHome { get; set; }
        public float handicapAway { get; set; }
        #endregion

        #region [using for Broker]
        public bool IsChoosedHome { get; set; }
        #endregion
    }
}
