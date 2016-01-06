using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

        Mutex m = new Mutex();
        public float points;
        /// <summary>
        /// Ilość zdobytych przez komórkę punktów
        /// </summary>
        public float Points
        {
            get
            {
                m.WaitOne();
                var res = points;
                m.ReleaseMutex();
                return res;
            }
            internal set
            {
                m.WaitOne();
                points = value;
                m.ReleaseMutex();
            }
        }

        /// <summary>
        /// Aktualizacja ilości punktów bazując na ostatnich decyzjach komórki
        /// </summary>
        public void UpdatePoints()
        {
            var Lasts = GetNeighbours().Select(x => SPD.Singleton.GetSkirmish(this, x).Last);
            foreach(var last in Lasts)
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
            var max = cellList.Max(y => y.Points);
            if (c.Points == max) return c.Strategy;
            if (c.GetNeighbours().All(x => x.Strategy == c.Strategy)) return c.Strategy;
            var best = cellList.Where(x => x.Points == cellList.Max(y => y.Points)).Select(x => x.Strategy).Distinct();
            return best.First();
        }

        /// <summary>
        /// Zwróć najlepszą strategię w sąsiedztwie tej komórki
        /// </summary>
        /// <returns>Najlepsza strategia</returns>
        public IStrategy OptimizeStrategy()
        {
            var str = GetBest(this, this.GetNeighbours().Concat(new Cell[] { this }));
            return str;
        }

        internal Cell Clone()
        {
            var C = new Cell(Strategy);
            C.Points = this.Points;
            return C;
        }

        internal void Clear()
        {
            Points = 0;
        }
    }
}
