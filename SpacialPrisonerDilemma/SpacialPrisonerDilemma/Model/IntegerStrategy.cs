using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    public enum WhenBetray
    {
        Always,
        When1,
        When2,
        When3,
        When4,
        When5,
        When6,
        When7,
        When8,
        Never
    }

    class IntegerStrategy : IStrategy
    {
        public virtual bool Decide(Cell parent, Cell opponent)
        {
            throw new NotImplementedException();
        }

        public virtual int Treshold { get; protected set; }
        public WhenBetray StrategyType
        {
            get { return (WhenBetray)Treshold; }
            set { Treshold = (int)value; }
        }

        public IntegerStrategy(int treshold)
        {
            Treshold = treshold;
        }

        public IntegerStrategy(WhenBetray whenBetray):this((int)whenBetray)
        { }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
