using SPD.Engine.Neighbourhoods;
using SPD.Engine.Strategies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine
{
    public class SPD
    {
        public PointMatrix Matrix { get; }
        public int CurrentIteration { get; private set; } = 0;
        public int Width { get; }
        public int Height { get; }

        ConcurrentDictionary<Coord, IStrategy> _strategies;

        ConcurrentDictionary<Coord, float> _points = new ConcurrentDictionary<Coord, float>();

        ConcurrentDictionary<Coord, Coord[]> _neighbours = new ConcurrentDictionary<Coord, Coord[]>();

        ConcurrentDictionary<int, Coord[]> _threadConcernes = new ConcurrentDictionary<int, Coord[]>();

        ConcurrentDictionary<Coord, Tuple<Coord, bool>[]> _decisions = new ConcurrentDictionary<Coord, Tuple<Coord, bool>[]>();

        public int StepsPerIteration { get; }

        public int ThreadCount { get; }

        public SPD(PointMatrix m, INeighbourhood neighbourhood, int[,] initialConfiguration, IDictionary<int, IStrategy> possibleStrategies, int stepNum, int threadNum = 1)
        {
            if (threadNum <= 0) throw new ArgumentException();
            Width = initialConfiguration.GetLength(0);
            Height = initialConfiguration.GetLength(1);
            Matrix = m;
            ThreadCount = threadNum;
            StepsPerIteration = stepNum;

            var concernes = new List<Coord>[ThreadCount];
            for (int i = 0; i < ThreadCount; i++)
                concernes[i] = new List<Coord>();

            int index = 0;

            for(int x=0; x<Width; x++)
                for(int y=0; y<Height; y++)
                {
                    var key = new Coord(x, y);
                    _strategies.AddOrUpdate(key, possibleStrategies[initialConfiguration[x, y]], (c, s) => possibleStrategies[initialConfiguration[x, y]]);
                    _neighbours.AddOrUpdate(key, neighbourhood.GetNeighbours(key).ToArray(), (a, b) => neighbourhood.GetNeighbours(key).ToArray());
                    concernes[index].Add(key);
                    index = (index + 1) % ThreadCount;
                }
            for (int i = 0; i < ThreadCount; i++)
                _threadConcernes.AddOrUpdate(i, concernes[i].ToArray(), (a, b) => concernes[i].ToArray());

            ProcessHistory(initialConfiguration);
        }


        public async Task<SPDResult> IterateAsync()
        {
            return await Task.Run(() => Iterate());
        }

        public SPDResult Iterate()
        {
            for (int i = 0; i < StepsPerIteration; i++)
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
            CurrentIteration++;
            return result;
        }

        List<Tuple<int, int[,]>> _history = new List<Tuple<int, int[,]>>();

        private bool ProcessHistory(int[,] strategyConfig)
        {
            var result = false;
            var b = GetHashOf(strategyConfig);
            if(_history.Any(x=>x.Item1==b))
            {
                result = CheckHistory(strategyConfig);
            }
            _history.Add(new Tuple<int, int[,]>(b, strategyConfig));
            return result;
            
        }

        private bool CheckHistory(int[,] strategyConfig)
        {
            return _history.Reverse<Tuple<int, int[,]>>().Any(x => CompareCodes(x.Item2, strategyConfig));
        }

        private bool CompareCodes(int[,] config1, int[,] config2)
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (config1[x, y] != config2[x, y]) return false;
            return true;
        }

        private int GetHashOf(int[,] strategyConfig)
        {
            int result = 1;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    result = unchecked(result + ((x + x * y) * strategyConfig[x, y]));
            return result;
        }

        int[,] ExtractToArray(ConcurrentDictionary<Coord, IStrategy> dict)
        {
            var r = new int[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    r[x, y] = dict.GetOrAdd(new Coord(x, y), a => default(IStrategy)).StrategyCode;
            return r;
        }

        T[,] ExtractToArray<T>(ConcurrentDictionary<Coord, T> dict)
        {
            var r = new T[Width, Height];
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    r[x, y] = dict.GetOrAdd(new Coord(x, y), a => default(T));
            return r;
        }

        IStrategy GetBestFor(Coord c)
        {
            IStrategy result = _strategies.GetOrAdd(c, default(IStrategy));
            float treshold = _points.GetOrAdd(c, 0);
            foreach(var n in _neighbours.GetOrAdd(c, new Coord[0]))
            {
                var p = _points.GetOrAdd(n, 0);
                if (p > treshold)
                {
                    treshold = p;
                    result = _strategies.GetOrAdd(n, default(IStrategy)).GetCopy();
                }
            }
            return result;
        }

        ConcurrentDictionary<Coord, IStrategy> _newStrategies = new ConcurrentDictionary<Coord, IStrategy>();
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
            {
                _strategies.GetOrAdd(c, default(IStrategy)).Clear();
            }
        }

        private void EndStepMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
            {
                _strategies.GetOrAdd(c, default(IStrategy)).EndStep();
            }
        }

        private void PosprocessForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach (var c in concernes)
            {
                Tuple<Coord, bool>[] decisions = _decisions.GetOrAdd(c, co => new Tuple<Coord, bool>[0]);
                foreach (var d in decisions)
                {
                    _strategies.GetOrAdd(d.Item1, co => null).PostProcess(c, d.Item2);

                    var opp = _decisions.GetOrAdd(d.Item1, co => new Tuple<Coord, bool>[0]).First(dd => dd.Item1 == c);
                    float f1;
                    float f2;
                    Matrix.GetPoints(d.Item2, opp.Item2, out f1, out f2);
                    _points.AddOrUpdate(c, f1, (a, b) => b + f1);
                    _points.AddOrUpdate(opp.Item1, f2, (a, b) => b + f2);
                }
            }
        }

        private void DecideForMany(int obj)
        {
            Coord[] concernes;
            if (!_threadConcernes.TryGetValue(obj, out concernes))
                throw new NotSupportedException();
            foreach(var c in concernes)
            {
                Coord[] neighbours;
                IStrategy s;
                if (!_neighbours.TryGetValue(c, out neighbours) || !_strategies.TryGetValue(c, out s))
                    throw new NotSupportedException();
                foreach (var n in neighbours)
                {
                    var b = s.Decide(n);
                    var nextVal = new Tuple<Coord, bool>[] { new Tuple<Coord, bool>(n, b) };
                    _decisions.AddOrUpdate(c, k => nextVal,
                        (k, v) => v.Concat(nextVal).ToArray());
                }
            }
        }
    }
}
