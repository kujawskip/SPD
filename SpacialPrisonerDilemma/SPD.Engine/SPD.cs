using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpacialPrisonerDilemma.Engine;
using SpacialPrisonerDilemma.Engine.Neighbourhoods;
using SpacialPrisonerDilemma.Engine.Strategies;

namespace SPD.Engine
{
    public enum OptimizationKind
    {
        Absolute, //Raw points matter
        Relative //Each cell uses it's own matrix to calculate neighbours' effectiveness
    }

    public enum SituationKind
    {
        BothBetrayed,
        OpponentBetrayed,
        Betrayal,
        NoneBetrayed
    }

    public class SPD
    {
        private readonly ConcurrentDictionary<Coord, Tuple<Coord, bool>[]> _decisions =
            new ConcurrentDictionary<Coord, Tuple<Coord, bool>[]>();

        private readonly List<Tuple<int, int[,]>> _history = new List<Tuple<int, int[,]>>();

        private readonly ConcurrentDictionary<Coord, Coord[]> _neighbours = new ConcurrentDictionary<Coord, Coord[]>();

        private ConcurrentDictionary<Coord, IStrategy> _newStrategies = new ConcurrentDictionary<Coord, IStrategy>();

        private readonly ConcurrentDictionary<Coord, float> _points = new ConcurrentDictionary<Coord, float>();
        private ConcurrentDictionary<Coord, IStrategy> _strategies = new ConcurrentDictionary<Coord, IStrategy>();

        private readonly ConcurrentDictionary<int, Coord[]> _threadConcernes = new ConcurrentDictionary<int, Coord[]>();

        private ConcurrentDictionary<Coord, Tuple<Coord, SituationMatrix>[]> _situationHistory = new ConcurrentDictionary<Coord, Tuple<Coord, SituationMatrix>[]>();
        public SPD(Func<Coord, PointMatrix> mFunc, INeighbourhood neighbourhood, int[,] initialConfiguration,
            IDictionary<int, IStrategy> possibleStrategies, int stepNum, int threadNum = 1,
            OptimizationKind optimizationKind = OptimizationKind.Absolute)
        {
            if (threadNum <= 0) throw new ArgumentException();
            OptimizatioKind = optimizationKind;
            Width = initialConfiguration.GetLength(0);
            Height = initialConfiguration.GetLength(1);
            Matrix = mFunc;
            ThreadCount = threadNum;
            StepsPerIteration = stepNum;
            CurrentIteration = 0;
            var concernes = new List<Coord>[ThreadCount];
            for (var i = 0; i < ThreadCount; i++)
                concernes[i] = new List<Coord>();

            var index = 0;

            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                {
                    var key = new Coord(x, y);
                    _strategies.AddOrUpdate(key, possibleStrategies[initialConfiguration[x, y]],
                        (c, s) => possibleStrategies[initialConfiguration[x, y]]);
                    var neigh = neighbourhood.GetNeighbours(key).ToArray();
                    _neighbours.AddOrUpdate(key, neigh.ToArray(),
                        (a, b) => b.Union(neigh).ToArray());
                    concernes[index].Add(key);
                    index = (index + 1) % ThreadCount;
                }
            for (var i = 0; i < ThreadCount; i++)
                _threadConcernes.AddOrUpdate(i, concernes[i].ToArray(), (a, b) => concernes[i].ToArray());

            ProcessHistory(initialConfiguration);
        }

        public SPD(PointMatrix m, INeighbourhood neighbourhood, int[,] initialConfiguration,
            IDictionary<int, IStrategy> possibleStrategies, int stepNum, int threadNum = 1,
            OptimizationKind optimizationKind = OptimizationKind.Absolute)
            : this(
                coord => m, neighbourhood, initialConfiguration, possibleStrategies, stepNum, threadNum,
                optimizationKind)
        {
        }

        public Func<Coord, PointMatrix> Matrix { get; }
        public int CurrentIteration { get; private set; }
        public int Width { get; }
        public int Height { get; }
        public int StepsPerIteration { get; }
        public int ThreadCount { get; }
        public OptimizationKind OptimizatioKind { get; }
        public Coord[] Neighbours(int x, int y)
        {
            return _neighbours[new Coord(x, y)];
        }
        public async Task<SPDResult> IterateAsync()
        {
            return await Task.Run(() => Iterate());
        }
        public SPDResult Iterate()
        {
            for (var i = 0; i < StepsPerIteration; i++)
            {
                Parallel.For(0, ThreadCount, DecideForMany);
                Parallel.For(0, ThreadCount, PosprocessForMany);
                Parallel.For(0, ThreadCount, EndStepMany);
            }
            Parallel.For(0, ThreadCount, ClearForMany);
            Parallel.For(0, ThreadCount, OptimizeForMany);
            _strategies = _newStrategies;
            var strategyConfig = ExtractToArray(_strategies);
            var result = new SPDResult(ExtractToArray(_points), strategyConfig, ProcessHistory(strategyConfig));
            _points.Clear();
            _decisions.Clear();
            _newStrategies = new ConcurrentDictionary<Coord, IStrategy>();
            _situationHistory = new ConcurrentDictionary<Coord, Tuple<Coord, SituationMatrix>[]>();
            CurrentIteration++;
            return result;
        }

        private bool ProcessHistory(int[,] strategyConfig)
        {
            var result = false;
            var b = GetHashOf(strategyConfig);
            if (_history.Any(x => x.Item1 == b))
                result = CheckHistory(strategyConfig);
            _history.Add(new Tuple<int, int[,]>(b, strategyConfig));
            return result;
        }

        private bool CheckHistory(int[,] strategyConfig)
        {
            return _history.Reverse<Tuple<int, int[,]>>().Any(x => CompareCodes(x.Item2, strategyConfig));
        }

        private bool CompareCodes(int[,] config1, int[,] config2)
        {
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    if (config1[x, y] != config2[x, y]) return false;
            return true;
        }

        private int GetHashOf(int[,] strategyConfig)
        {
            var result = 1;
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    result = unchecked(result + (x + x * y) * strategyConfig[x, y]);
            return result;
        }

        private int[,] ExtractToArray(ConcurrentDictionary<Coord, IStrategy> dict)
        {
            var r = new int[Width, Height];
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    r[x, y] = dict.GetOrAdd(new Coord(x, y), a => default(IStrategy)).StrategyCode;
            return r;
        }

        private T[,] ExtractToArray<T>(ConcurrentDictionary<Coord, T> dict)
        {
            var r = new T[Width, Height];
            for (var x = 0; x < Width; x++)
                for (var y = 0; y < Height; y++)
                    r[x, y] = dict.GetOrAdd(new Coord(x, y), a => default(T));
            return r;
        }

        private IStrategy GetBestFor(Coord c)
        {
            var result = _strategies.GetOrAdd(c, default(IStrategy));
            var treshold = _points.GetOrAdd(c, 0);
            if (OptimizatioKind == OptimizationKind.Absolute)
            {
                foreach (var n in _neighbours.GetOrAdd(c, new Coord[0]))
                {
                    var p = _points.GetOrAdd(n, 0);
                    if (p > treshold)
                    {
                        treshold = p;
                        result = _strategies.GetOrAdd(n, default(IStrategy)).GetCopy();
                    }
                }
            }
            else
            {
                var matrix = Matrix(c);
                foreach (var tuple in _situationHistory.GetOrAdd(c, new Tuple<Coord, SituationMatrix>[0]))
                {
                    var p = tuple.Item2.BothBetrayedCount * matrix.BothBetrayed
                            + tuple.Item2.BetrayedCount * matrix.BetrayedOther
                            + tuple.Item2.NoneBetrayedCount * matrix.NoneBetrayed
                            + tuple.Item2.WasBetrayedCount * matrix.WasBetrayed;
                    if (p > treshold)
                    {
                        treshold = p;
                        result = _strategies.GetOrAdd(tuple.Item1, default(IStrategy)).GetCopy();
                    }
                }
            }
            return result;
        }

        private void OptimizeForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
            {
                var s = GetBestFor(c);
                if (!_newStrategies.TryAdd(c, s))
                    throw new NotSupportedException();
            }
        }

        private void ClearForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
                _strategies.GetOrAdd(c, default(IStrategy)).Clear();
        }

        private void EndStepMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
                _strategies.GetOrAdd(c, default(IStrategy)).EndStep();
        }

        private void PosprocessForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
            {
                var decisions = _decisions.GetOrAdd(c, co => new Tuple<Coord, bool>[0]);
                foreach (var d in decisions)
                {
                    _strategies.GetOrAdd(d.Item1, co => null).PostProcess(c, d.Item2);

                    var opp = _decisions.GetOrAdd(d.Item1, co => new Tuple<Coord, bool>[0]).First(dd => dd.Item1 == c);
                    float f1, f2;
                    GetPointsOf(d, opp, out f1, out f2);
                    _points.AddOrUpdate(c, f1, (a, b) => b + f1);
                    _points.AddOrUpdate(opp.Item1, f2, (a, b) => b + f2);
                    SituationMatrix s1, s2;
                    GetSituationKind(d.Item2, opp.Item2, out s1, out s2);
                    var t1 = new Tuple<Coord, SituationMatrix>(opp.Item1, s1);
                    var t2 = new Tuple<Coord, SituationMatrix>(c, s2);
                    _situationHistory.AddOrUpdate(c, new[] {t1}, (k, v) => UpdateSituation(v, t1));
                    _situationHistory.AddOrUpdate(opp.Item1, new[] {t2}, (k, v) => UpdateSituation(v, t2));
                }
            }
        }

        private Tuple<Coord, SituationMatrix>[] UpdateSituation(Tuple<Coord, SituationMatrix>[] values, Tuple<Coord, SituationMatrix> v)
        {
            var item = values.SingleOrDefault(x => x.Item1 == v.Item1);
            if (item == null)
                return values.Concat(new[] {v}).ToArray();
            else
            {
                item.Item2.Add(v.Item2);
                return values.Where(x => x.Item1 != item.Item1).Concat(new[] {item}).ToArray(); //TODO: czy potrzeba aż tak skomplikowanie? Czy wystarczy referencyjnie?
            }
        }

        private void GetSituationKind(bool firstBetrayed, bool secondBetrayed, out SituationMatrix s1,
            out SituationMatrix s2)
        {
            if (firstBetrayed && secondBetrayed)
            {
                s1 = new SituationMatrix {BothBetrayedCount = 1};
                s2 = new SituationMatrix {BothBetrayedCount = 1};
            }
            else if (!firstBetrayed && !secondBetrayed)
            {
                s1 = new SituationMatrix { NoneBetrayedCount = 1 };
                s2 = new SituationMatrix { NoneBetrayedCount = 1 };
            }
            else if (firstBetrayed)
            {
                s1 = new SituationMatrix { BetrayedCount = 1 };
                s2 = new SituationMatrix { WasBetrayedCount = 1 };
            }
            else
            {
                s1 = new SituationMatrix { WasBetrayedCount = 1 };
                s2 = new SituationMatrix { BetrayedCount = 1 };
            }
        }

        private void GetPointsOf(Tuple<Coord, bool> c1, Tuple<Coord, bool> c2, out float p1, out float p2)
        {
            var pm1 = Matrix(c1.Item1);
            var pm2 = Matrix(c2.Item1);
            float temp;
            pm1.GetPoints(c1.Item2, c2.Item2, out p1, out temp);
            pm2.GetPoints(c1.Item2, c2.Item2, out temp, out p2);
        }

        private void DecideForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
            {
                Coord[] neighbours;
                IStrategy s;
                if (!_neighbours.TryGetValue(c, out neighbours) || !_strategies.TryGetValue(c, out s))
                    throw new NotSupportedException();
                foreach (var n in neighbours)
                {
                    var b = s.Decide(n);
                    var nextVal = new[] {new Tuple<Coord, bool>(n, b)};
                    _decisions.AddOrUpdate(c, k => nextVal,
                        (k, v) => v.Concat(nextVal).ToArray());
                    IStrategy opps;
                    if (!_strategies.TryGetValue(n, out opps))
                        throw new NotSupportedException();
                    var oppb = opps.Decide(c);
                    var oppNextVal = new[] {new Tuple<Coord, bool>(c, oppb)};
                    _decisions.AddOrUpdate(n, oppNextVal,
                        (k, v) => v.Concat(oppNextVal).ToArray());
                }
            }
        }
    }

    internal class SituationMatrix
    {
        public int WasBetrayedCount { get; set; }
        public int BetrayedCount { get; set; }
        public int NoneBetrayedCount { get; set; }
        public int BothBetrayedCount { get; set; }

        public SituationMatrix Add(SituationMatrix other) =>
            new SituationMatrix
            {
                WasBetrayedCount = WasBetrayedCount + other.WasBetrayedCount,
                BetrayedCount = BetrayedCount + other.BetrayedCount,
                BothBetrayedCount = BothBetrayedCount + other.BothBetrayedCount,
                NoneBetrayedCount = NoneBetrayedCount + other.NoneBetrayedCount
            };
    }
}