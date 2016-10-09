using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Engine
{
    public class PointMatrix
    {
        public float NoneBetrayed { get; private set; }
        public float WasBetrayed { get; private set; }
        public float BetrayedOther { get; private set; }
        public float BothBetrayed { get; private set; }

        public PointMatrix(float none, float was, float betrayed, float both)
        {
           // throw new NotImplementedException("Validation");
            NoneBetrayed = none;
            WasBetrayed = was;
            BetrayedOther = betrayed;
            BothBetrayed = both;
        }

        public void GetPoints(bool firstBetrayed, bool secondBetrayed, out float firstPoints, out float secondPoints)
        {
            if (firstBetrayed && secondBetrayed) firstPoints = secondPoints = BothBetrayed;
            else if (!firstBetrayed && !secondBetrayed) firstPoints = secondPoints = NoneBetrayed;
            else if(firstBetrayed)
            {
                firstPoints = BetrayedOther;
                secondPoints = WasBetrayed;
            }
            else
            {
                firstPoints = WasBetrayed;
                secondPoints = BetrayedOther;
            }
        }
    }
}
