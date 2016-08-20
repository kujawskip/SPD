using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SpacialPrisonerDilemma.Model
{
    /// <summary>
    /// Implementacja komórki automatu komórkowego
    /// </summary>
    public class Cell
    {
       /// <summary>
       /// Inicjalizacja komórki
       /// </summary>
       /// <param name="strategy">Strategia początkowa komórki</param>
        public Cell(IStrategy strategy)
        {
            Strategy = strategy;
        }

        /// <summary>
        /// Akcesor do aktualnej strategii komórki
        /// </summary>
        public IStrategy Strategy
        { get; internal set; }

        /// <summary>
        /// Dostęp do sąsiadów komórki
        /// </summary>
        /// <returns>Tablica komórek sąsiadujących</returns>
        public Cell[] GetNeighbours()
        {
            return SPD.Singleton.GetNeighbours(this);
        }

        /// <summary>
        /// Implementacja podejmowania decyzji przez komórkę
        /// </summary>
        /// <param name="opponent">Komórka będąca przeciwnikiem</param>
        /// <returns>True jeśli zdradza, false w przeciwnym wypadku</returns>
        public bool Decide(Cell opponent)
        {
            return Strategy.Decide(this, opponent);
        }

        readonly Mutex _m = new Mutex();
        public float points;
        /// <summary>
        /// Ilość zdobytych przez komórkę punktów
        /// </summary>
        public float Points
        {
            get
            {
                _m.WaitOne();
                var res = points;
                _m.ReleaseMutex();
                return res;
            }
            internal set
            {
                _m.WaitOne();
                points = value;
                _m.ReleaseMutex();
            }
        }

        /// <summary>
        /// Aktualizacja ilości punktów bazując na ostatnich decyzjach komórki
        /// </summary>
        public void UpdatePoints()
        {
            var lasts = GetNeighbours().Select(x => SPD.Singleton.GetSkirmish(this, x).Last);
            foreach(var last in lasts)
            {
                bool myDecision;
                bool opponentsDecision;
                Decompose(this, last, out myDecision, out opponentsDecision);
                Points += SPD.Singleton.GetAward(myDecision, opponentsDecision);
            }
        }

        private void Decompose(Cell cell, Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>> last, out bool myDecision, out bool opponentsDecision)
        {
            if (last.Item1.Item1 == cell)
            {
                myDecision = last.Item1.Item2;
                opponentsDecision = last.Item2.Item2;
            }
            else if (last.Item2.Item1 == cell)
            {
                myDecision = last.Item2.Item2;
                opponentsDecision = last.Item1.Item2;
            }
            else throw new ArgumentException();
        }

        /// <summary>
        /// Znajdź najlepszą strategię spośród podanych komórek względem centralnej.
        /// W przypadku remisu punktów wybierana jest centralna (o ile ma najwięcej punktów), następnie priorytety maleją zgodnie z ruchem wskazówek zegara.
        /// </summary>
        /// <param name="c">Komórka centralna</param>
        /// <param name="cellList">Sąsiedzi</param>
        /// <returns>Najlepsza strategia</returns>
        public static IStrategy GetBest(Cell c, IEnumerable<Cell> cellList)
        {
            var enumerable = cellList as Cell[] ?? cellList.ToArray();
            var max = enumerable.Max(y => y.Points);
            if (c.Points == max) return c.Strategy;
            if (c.GetNeighbours().All(x => x.Strategy == c.Strategy)) return c.Strategy;
            var best = enumerable.Where(x => x.Points == enumerable.Max(y => y.Points)).Select(x => x.Strategy).Distinct();
            return best.First();
        }

        /// <summary>
        /// Zwróć najlepszą strategię w sąsiedztwie tej komórki
        /// </summary>
        /// <returns>Najlepsza strategia</returns>
        public IStrategy OptimizeStrategy()
        {
            var str = GetBest(this, GetNeighbours().Concat(new[] { this }));
            return str;
        }

        internal Cell Clone()
        {
           
            return new Cell(Strategy) {Points = Points};
        }

        internal void Clear()
        {
            Points = 0;
        }
    }
}
