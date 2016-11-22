using System.Collections.Concurrent;

namespace SPD.Engine.Strategies
{
    public class IntegerStrategy : IStrategy
    {
        public int BetrayalThreshold { get; private set; }

        public int StrategyCode
        {
            get { return BetrayalThreshold*100; }
        }

        ConcurrentDictionary<long, int> val = new ConcurrentDictionary<long, int>();

        private bool _isAggressive;

        public IntegerStrategy(int threshold)
        {
            BetrayalThreshold = threshold;
            _isAggressive = threshold == 0;
            val.Clear();
        }
        public void Clear()
        {
            val.Clear();
            _isAggressive = BetrayalThreshold == 0;
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
            val.Clear();
        }

        public override string ToString()
        {
            int v;
            val.TryGetValue(0, out v);
            return $"Threshold {BetrayalThreshold}, betrayed by {v}";
        }
    }
}
