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
        public virtual int Treshold { get; protected set; }
        public WhenBetray StrategyType
        {
            get { return (WhenBetray)Treshold; }
            set { Treshold = (int)value; }
        }
        public static IntegerStrategy[] Strategies
        {
            get
            {
                return new IntegerStrategy[]
                {
                    new IntegerStrategy(0),
                    new IntegerStrategy(1),
                    new IntegerStrategy(2),
                    new IntegerStrategy(3),
                    new IntegerStrategy(4),
                    new IntegerStrategy(5),
                    new IntegerStrategy(6),
                    new IntegerStrategy(7),
                    new IntegerStrategy(8),
                    new IntegerStrategy(9),
                };
            }
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

        public bool Decide(Cell parent, Cell opponent)
        {
            return (from neighbour in parent.GetNeighbours()
                    let skirmish = SPD.Singleton.GetSkirmish(parent, neighbour)
                    let last = skirmish != null ? skirmish.Last : null
                    let decision = last == null ? false : last.Item1.Item1 == parent ? last.Item2.Item2 : last.Item1.Item2
                    select decision).Count(x => x == true) >= Treshold;
        }
    }
}
