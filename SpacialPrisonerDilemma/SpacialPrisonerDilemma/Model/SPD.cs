using SpacialPrisonerDilemma.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    /// <summary>
    /// Implementacja obliczeń przestrzennego dylematu więźnia
    /// </summary>
    public class SPD
    {
        /// <summary>
        /// Ilość wątków, na których przeprowadzane będą obliczenia
        /// </summary>
        public const int ThreadCount = 16;

        private PerformanceLog Log;

        /// <summary>
        /// Akcesor komórek automatu
        /// </summary>
        /// <param name="i">Indeks kolumny</param>
        /// <param name="j">Indeks wiersza</param>
        /// <returns>Komórka o podanych współrzędnych</returns>
        public Cell this[int i, int j]
        {
            get
            {
                return cells[i, j];
            }
            set
            {
                cells[i, j] = value;
            }
        }

        private static T[] ReduceDim<T>(T[,] input)
        {
            var result = new List<T>();
            for (int i = 0; i < input.GetLength(0); i++)
                for (int j = 0; j < input.GetLength(1); j++)
                    result.Add(input[i, j]);
            return result.ToArray();
        }

        private static SPD singleton;
        public static SPD Singleton
        {
            get
            {
                if (singleton == null) singleton = new SPD();
                return singleton;
            }
        }

        public SPD()
        {
            skirmishes = new Dictionary<Tuple<Cell, Cell>, Skirmish>();
            history = new List<Tuple<int, Cell[,]>>();
            coords = new Dictionary<Cell, Tuple<int, int>>();
            cells = new Cell[0, 0];
        }

        /// <summary>
        /// Obsługa odwołań do macierzy wypłat
        /// </summary>
        /// <param name="myDecision"></param>
        /// <param name="opponentsDecision"></param>
        /// <returns>Ilość punktów otrzymanych po podjęciu decyzji myDecision przeciw decyzji opponentsDecision</returns>
        internal float GetAward(bool myDecision, bool opponentsDecision)
        {
            if (myDecision)
            {
                if (opponentsDecision)
                    return BothBetrayedPoints;
                else
                    return WasntBetrayedPoints;
            }
            else
            {
                if (opponentsDecision)
                    return WasBetrayedPoints;
                else
                    return NoneBetrayedPoints;
            }
        }
        /// <summary>
        /// Wypłata dla zdradzonego
        /// </summary>
        public float WasBetrayedPoints
        { get; private set; }
        /// <summary>
        /// Wypłata dla zdradzającego
        /// </summary>
        public float WasntBetrayedPoints
        { get; private set; }
        /// <summary>
        /// Wypłata jeśli do zdrady nie doszło
        /// </summary>
        public float NoneBetrayedPoints
        { get; private set; }
        /// <summary>
        /// Wypłata, gdy obie strony zdradziły
        /// </summary>
        public float BothBetrayedPoints
        { get; private set; }

        /// <summary>
        /// Zainicjowanie instancji danymi początkowymi
        /// </summary>
        /// <param name="initialConfig">Konfiguracja początkowa automatu</param>
        /// <param name="stepsPerIteration">Ilość decyzji podejmowanych między kolejnymi krokami automatu</param>
        /// <param name="noneBetrayed">Wypłata jeśli do zdrady nie doszło</param>
        /// <param name="wasBetrayed">Wypłata dla zdradzonego</param>
        /// <param name="wasntBetrayed">Wypłata dla zdradzającego</param>
        /// <param name="bothBetrayed">Wypłata, gdy obie strony zdradziły</param>
        /// <param name="moore">true - sąsiedztwo Moore'a, false - sąsiedztwo von Neumanna</param>
        /// <param name="torus">true - symulacja na torusie, false - symulacja na ograniczonej płaszczyźnie</param>
        public static void Initialize(IStrategy[,] initialConfig, int stepsPerIteration, float noneBetrayed, float wasBetrayed, float wasntBetrayed, float bothBetrayed, bool moore = true, bool torus = false)
        {
            var allocStart = DateTime.Now;
            Singleton.moore = moore;
            Singleton.torus = torus;
            Singleton.NoneBetrayedPoints = noneBetrayed;
            Singleton.WasBetrayedPoints = wasBetrayed;
            Singleton.WasntBetrayedPoints = wasntBetrayed;
            Singleton.BothBetrayedPoints = bothBetrayed;
            Singleton.cells = new Cell[initialConfig.GetLength(0), initialConfig.GetLength(1)];
            for (int i = 0; i < initialConfig.GetLength(0); i++)
                for (int j = 0; j < initialConfig.GetLength(1); j++)
                {
                    
                    Singleton.cells[i, j] = new Cell(initialConfig[i, j]);
                    Singleton.coords.Add(Singleton.cells[i, j], new Tuple<int, int>(i, j));
                }
            Singleton.StepCount = stepsPerIteration;
            Singleton.CacheToHistory();
            for (int x = 0; x < Singleton.cells.GetLength(0); x++)
                for (int y = 0; y < Singleton.cells.GetLength(1); y++)
                {
                    var key = new Tuple<Cell, Cell>(Singleton.GetCell(x, y), Singleton.GetCell(x, y - 1));
                    if (key.Item2 != null)
                        Singleton.skirmishes.Add(key, new Skirmish(key.Item1, key.Item2));
                    if (moore)
                    {
                        key = new Tuple<Cell, Cell>(Singleton.GetCell(x, y), Singleton.GetCell(x + 1, y - 1));
                        if (key.Item2 != null)
                            Singleton.skirmishes.Add(key, new Skirmish(key.Item1, key.Item2));
                    }
                    key = new Tuple<Cell, Cell>(Singleton.GetCell(x, y), Singleton.GetCell(x + 1, y));
                    if (key.Item2 != null)
                        Singleton.skirmishes.Add(key, new Skirmish(key.Item1, key.Item2));
                    if (moore)
                    {
                        key = new Tuple<Cell, Cell>(Singleton.GetCell(x, y), Singleton.GetCell(x + 1, y + 1));
                        if (key.Item2 != null)
                            Singleton.skirmishes.Add(key, new Skirmish(key.Item1, key.Item2));
                    }
                }
            Singleton.Batches = Singleton.BatchCells();
            var allocEnd = DateTime.Now;
            Singleton.Log = new PerformanceLog(allocStart, allocEnd);
        }

        /// <summary>
        /// Wyczyszczenie singletona
        /// </summary>
        /// <returns>Raport wydajności</returns>
        public static PerformanceLog ClearAndGetLog()
        {
            var log = Singleton.Log;
            singleton = null;
            return log;
        }

        void ForEachCell(Action<Cell> action)
        {
            for (int x = 0; x < Singleton.cells.GetLength(0); x++)
                for (int y = 0; y < Singleton.cells.GetLength(1); y++)
                {
                    action(cells[x, y]);
                }
        }

        T[,] ForEachCell<T>(Func<Cell, T> func)
        {
            var result = new T[cells.GetLength(0), cells.GetLength(1)];
            for (int x = 0; x < Singleton.cells.GetLength(0); x++)
                for (int y = 0; y < Singleton.cells.GetLength(1); y++)
                    result[x, y] = func(cells[x, y]);
            return result;
        }
        /// <summary>
        /// Zainicjowanie instancji danymi początkowymi
        /// </summary>
        /// <param name="initialConfig">Konfiguracja początkowa automatu</param>
        /// <param name="stepsPerIteration">Ilość decyzji podejmowanych między kolejnymi krokami automatu</param>
        /// <param name="noneBetrayed">Wypłata jeśli do zdrady nie doszło</param>
        /// <param name="wasBetrayed">Wypłata dla zdradzonego</param>
        /// <param name="wasntBetrayed">Wypłata dla zdradzającego</param>
        /// <param name="bothBetrayed">Wypłata, gdy obie strony zdradziły</param>
        /// <param name="moore">true - sąsiedztwo Moore'a, false - sąsiedztwo von Neumanna</param>
        /// <param name="torus">true - symulacja na torusie, false - symulacja na ograniczonej płaszczyźnie</param>
        public static void Initialize(int[,] initialConfig, int stepsPerIteration, float noneBetrayed, float wasBetrayed, float wasntBetrayed, float bothBetrayed, bool moore = true, bool torus = false)
        {
            var str = new IStrategy[initialConfig.GetLength(0), initialConfig.GetLength(1)];
            for (int i = 0; i < initialConfig.GetLength(0); i++)
                for (int j = 0; j < initialConfig.GetLength(1); j++)
                    str[i, j] = IntegerStrategy.Strategies[initialConfig[i, j]];
            Initialize(str, stepsPerIteration, noneBetrayed, wasBetrayed, wasntBetrayed, bothBetrayed, moore, torus);
        }

        /// <summary>
        /// Zainicjowanie instancji danymi początkowymi
        /// </summary>
        /// <param name="initialConfig">Konfiguracja początkowa automatu</param>
        /// <param name="stepsPerIteration">Ilość decyzji podejmowanych między kolejnymi krokami automatu</param>
        /// <param name="noneBetrayed">Wypłata jeśli do zdrady nie doszło</param>
        /// <param name="wasBetrayed">Wypłata dla zdradzonego</param>
        /// <param name="wasntBetrayed">Wypłata dla zdradzającego</param>
        /// <param name="bothBetrayed">Wypłata, gdy obie strony zdradziły</param>
        /// <param name="moore">true - sąsiedztwo Moore'a, false - sąsiedztwo von Neumanna</param>
        /// <param name="torus">true - symulacja na torusie, false - symulacja na ograniczonej płaszczyźnie</param>
        public static void Initialize(WhenBetray[,] initialConfig, int stepsPerIteration, float noneBetrayed, float wasBetrayed, float wasntBetrayed, float bothBetrayed, bool moore = true, bool torus = false)
        {
            var str = new IStrategy[initialConfig.GetLength(0), initialConfig.GetLength(1)];
            for (int i = 0; i < initialConfig.GetLength(0); i++)
                for (int j = 0; j < initialConfig.GetLength(1); j++)
                    str[i, j] = IntegerStrategy.Strategies[(int)initialConfig[i, j]];
            Initialize(str, stepsPerIteration, noneBetrayed, wasBetrayed, wasntBetrayed, bothBetrayed, moore, torus);
        }

        bool moore;
        bool torus;
        List<Cell[]> Batches;
        Cell[,] cells;
        internal Dictionary<Tuple<Cell, Cell>, Skirmish> skirmishes;
        Dictionary<Cell, Tuple<int, int>> coords;
        List<Tuple<int,Cell[,]>> history;

        /// <summary>
        /// Zapisanie konfiguracji w historii i spradzenie, czy otrzymaliśmy układ stabilny
        /// </summary>
        /// <returns>True gdy układ jest stabilby, w przeciwnym przypadku false</returns>
        protected bool CacheToHistory()
        {
            var stateCopy = ForEachCell(x => x.Clone());
            var hashes = ForEachCell(x => x.Strategy.GetHashCode());
            int hash = 0;
            for (int i = 0; i < hashes.GetLength(0); i++)
                for (int j = 0; j < hashes.GetLength(1); j++)
                    hash = unchecked(i * j * hashes[i, j] + hash);
            var repeated = history.Where(x => x.Item1 == hash).Select(x => x.Item2).Aggregate(false, (current, x) => current = current || ArrayEquals(x, stateCopy));
            history.Add(new Tuple<int, Cell[,]>(hash, stateCopy));
            return repeated;
        }

        private bool ArrayEquals(Cell[,] tab1, Cell[,] tab2)
        {
            if (tab1 == null && tab2 == null) return true;
            if (tab1 == null || tab2 == null) return false;
            if (tab1.GetLength(0) != tab2.GetLength(0)) return false;
            if (tab1.GetLength(1) != tab2.GetLength(1)) return false;
            for (int i = 0; i < tab1.GetLength(0); i++)
                for (int j = 0; j < tab1.GetLength(1); j++)
                    if (tab1[i, j].Strategy != tab2[i, j].Strategy) return false;
            return true;
        }

        /// <summary>
        /// Implementacja podjęcia pojedynczej decyzji przez każdą komórkę
        /// </summary>
        protected void Step()
        {
            ForEachCell(x =>
            {
                var Skirmishes = from keyVal in singleton.skirmishes
                                 where keyVal.Key.Item1 == x
                                 select keyVal.Value;
                foreach (var s in Skirmishes) s.SingleMove();
            });
            foreach (var skirmish in skirmishes.Values)
                skirmish.EndStep();
            ForEachCell(x =>
            {
                x.UpdatePoints();
            });
        }

        List<Cell> GetCells(int minX, int minY, int maxX, int maxY)
        {
            var res = new List<Cell>();
            for (int i = minX; i < maxX; i++)
                for (int j = minY; j < maxY; j++)
                    res.Add(cells[i, j]);
            return res;
        }

        List<Cell[]> BatchCells()
        {
            var result = new List<Cell[]>();
            var batch = new List<Cell>();
            var allThreads = ThreadCount;
            var lCount = cells.Length / allThreads;
            if (lCount * allThreads < cells.Length) lCount++;
            for (int y = 0; y < cells.GetLength(1); y++)
                for (int x = 0; x < cells.GetLength(0); x++)
                {
                    batch.Add(cells[x, y]);
                    if(batch.Count>=lCount)
                    {
                        result.Add(batch.ToArray());
                        batch.Clear();
                    }
                }
            if (batch.Count > 0)
            {
                result.Add(batch.ToArray());
                batch.Clear();
            }
            return result;
        }
        /// <summary>
        /// Implementacja podjęcia pojedynczej decyzji przez każdą komórkę
        /// </summary>
        /// <returns>Task powiązany z realizacją powyższego zadania</returns>
        protected async Task StepAsync()
        {
            var tasks = Batches.Select(x => Task.Run(() =>
            {
                foreach (Cell c in x)
                {
                    var c1 = c;
                    var skir = from keyVal in Singleton.skirmishes
                               where keyVal.Key.Item1 == c1
                               select keyVal.Value;
                    foreach (var s in skir)
                        s.SingleMove();
                }
            }));
            await Task.WhenAll(tasks);
            foreach (var skirmish in skirmishes.Values)
                skirmish.EndStep();
            tasks = Batches.Select(x => Task.Run(() =>
            {
                foreach (Cell c in x)
                {
                    c.UpdatePoints();
                }
            }));
            GC.Collect();
            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Implementacja pojedynczego kroku automatu.
        /// </summary>
        /// <returns>Ilość komórek, które zmieniły strategię oraz informacja, czy układ jest stabilny</returns>
        protected internal Tuple<int, bool> Iterate()
        {
            var stepStart = DateTime.Now;
            for (int i = 0; i < StepCount; i++)
                Step();
            int changed = 0;
            var newStr = ForEachCell(c => c.OptimizeStrategy());
            for (int x = 0; x < Singleton.cells.GetLength(0); x++)
                for (int y = 0; y < Singleton.cells.GetLength(1); y++)
                    if (cells[x, y].Strategy != newStr[x, y])
                    {
                        cells[x, y].Strategy = newStr[x, y];
                        changed++;
                    }
            ForEachCell(x =>
            {
                x.Clear();
                foreach (var s in skirmishes.Where(s => s.Key.Item1 == x))
                {
                    s.Value.Clear();
                }
            });
            var stable = CacheToHistory();
            var stepEnd = DateTime.Now;
            Log.NewStepStart(stepStart);
            Log.NewStepEnd(stepEnd);
            return new Tuple<int, bool>(changed, stable);
        }
        /// <summary>
        /// Implementacja asynchroniczna pojedynczego kroku automatu.
        /// </summary>
        /// <returns>Ilość komórek, które zmieniły strategię oraz informacja, czy układ jest stabilny</returns>
        public async Task<Tuple<int, bool>> IterateAsync()
        {
            var stepStart = DateTime.Now;
            for (int i = 0; i < StepCount; i++)
            {
                await StepAsync();
            }

            var tasks = Batches.Select(x => Task.Run(() =>
             {
                 return x.Select(c => new Tuple<Cell, IStrategy>(c, c.OptimizeStrategy())).ToArray();
             })).ToArray();
            await Task.WhenAll(tasks);
            var optimizing = tasks.Select(x => Task.Run(() =>
             {
                 int res = 0;
                 foreach (var tuple in x.Result)
                 {
                     if (tuple.Item1.Strategy != tuple.Item2)
                     {
                         tuple.Item1.Strategy = tuple.Item2;
                         res++;
                     }
                 }
                 return res;
             }));
            var opt = optimizing.ToArray();
            await Task.WhenAll(opt);
            int changed = opt.Sum(x => x.Result);
            var repeating = CacheToHistory();
            var tasks2 = Batches.Select(x => Task.Run(() =>
              {
                  foreach (Cell c in x)
                  {
                      c.Clear();
                      foreach(var s in skirmishes.Where(s => s.Key.Item1 == c))
                      {
                          s.Value.Clear();
                      }
                  }
              }));

            await Task.WhenAll(tasks2);
            var stepEnd = DateTime.Now;
            Log.NewStepStart(stepStart);
            Log.NewStepEnd(stepEnd);
            return new Tuple<int, bool>(changed, repeating);
        }

        /// <summary>
        /// Numer ostatniego obiczonego kroku automatu
        /// </summary>
        public int CurrentIteration
        {
            get
            {
                return history.Count;
            }
        }

        /// <summary>
        /// Ilość decyzji podejmowanych przez pojedynczą komórkę pomiędzy krokami automatu
        /// </summary>
        public int StepCount { get; private set; }

        internal Skirmish GetSkirmish(Cell c1, Cell c2)
        {
            var t = new Tuple<Cell, Cell>(c1, c2);
            return GetSkirmish(t);
        }

        internal Skirmish GetSkirmish(Tuple<Cell,Cell> pair)
        {
            if (skirmishes.ContainsKey(pair)) return skirmishes[pair];
            var t = new Tuple<Cell, Cell>(pair.Item2, pair.Item1);
            if (skirmishes.ContainsKey(t)) return skirmishes[t];
            return null;
        }

        internal Cell[] GetNeighbours(Cell c)
        {
            var result = new List<Cell>();
            var coord = coords[c];
            var x = coord.Item1;
            var y = coord.Item2;

            result.Add(GetCell(x, y - 1)); //Up
            if (moore)
                result.Add(GetCell(x + 1, y - 1));
            result.Add(GetCell(x + 1, y)); //Right
            if (moore)
                result.Add(GetCell(x + 1, y + 1));

            result.Add(GetCell(x, y + 1)); //Down
            if (moore)
                result.Add(GetCell(x - 1, y + 1));
            result.Add(GetCell(x - 1, y)); //Left
            if (moore)
                result.Add(GetCell(x - 1, y - 1));

            return result.Where(cell => cell != null).ToArray();
        }

        
        private Cell GetCell(int x, int y)
        {
            if (!torus && (x < 0 || x >= cells.GetLength(0) || y < 0 || y >= cells.GetLength(1))) return null;
            while (x < 0)
                x += cells.GetLength(0);
            x = x % cells.GetLength(0);
            while (y < 0)
                y += cells.GetLength(1);
            y = y % cells.GetLength(1);
            return cells[x, y];
        }

        /// <summary>
        /// Dostęp do przeszłych stanów automatu
        /// </summary>
        /// <param name="i">Numer kroku automatu</param>
        /// <returns>Stan automatu w zadanym kroku</returns>
        public Cell[,] GetStateByIteration(int i)
        {
            return history[i].Item2;
        }
    }
}
