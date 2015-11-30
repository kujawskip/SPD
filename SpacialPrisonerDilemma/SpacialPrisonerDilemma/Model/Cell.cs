using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    public class Cell
    {
        public Cell(IStrategy strategy)
        {
            Strategy = strategy;
        }

        public IStrategy Strategy
        { get; private set; }

        public Cell[] GetNeighbours()
        {
            return SPD.Singleton.GetNeighbours(this);
        }

        public bool Decide(Cell opponent)
        {
            return Strategy.Decide(this, opponent);
        }

        public float Points
        {
            get; internal set;
        }

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

        public static IStrategy GetBest(IEnumerable<Cell> cellList)
        {
            return cellList.First(x => x.Points == cellList.Max(y => y.Points)).Strategy;
        }

        public void OptimizeStrategy()
        {
            Strategy = GetBest(GetNeighbours().Concat(new Cell[] { this }));
        }

        internal Cell Clone()
        {
            return new Cell(Strategy);
        }

        internal void Clear()
        {
            Points = 0;
        }
    }
}
