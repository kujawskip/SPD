using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine.Strategies
{
    class IntegerStrategy : IStrategy
    {
        public int BetrayalThreshold { get; private set; }

        public int StrategyCode
        {
            get
            {
                return BetrayalThreshold * 100;
            }
        }

        ConcurrentDictionary<long, int> val = new ConcurrentDictionary<long, int>();

        bool _isAggressive = false;

        public IntegerStrategy(int threshold)
        {
            BetrayalThreshold = threshold;
            val.AddOrUpdate(0, 0, (a, b) => 0);
        }
        public void Clear()
        {
            val.AddOrUpdate(0, 0, (a, b) => 0);
            _isAggressive = false;
        }

        public bool Decide(Coord opponent)
        {
            return _isAggressive;
        }

        public IStrategy GetCopy()
        {
            return new IntegerStrategy(BetrayalThreshold);
        }

        public void PostProcess(Coord opponent, bool opponentBetrayed)
        {
            if (opponentBetrayed)
                val.AddOrUpdate(0, 1, (a, b) => b+1);
        }

        public void EndStep()
        {
            _isAggressive = val.GetOrAdd(0,0) >= BetrayalThreshold;
            val.AddOrUpdate(0, 0, (a, b) => 0);
        }
    }
}
