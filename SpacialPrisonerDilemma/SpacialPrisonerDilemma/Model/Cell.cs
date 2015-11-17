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
            throw new NotImplementedException();
        }

        public bool Decide(Cell opponent)
        {
            return Strategy.Decide(this, opponent);
        }

        public int CurrentPoints { get; private set; }

        public int GetPoints()
        {
            throw new NotImplementedException();
        }

        public static IStrategy GetBest(Cell[] cellList)
        {
            throw new NotImplementedException();
        }

        public void OptimizeStrategy()
        {
            throw new NotImplementedException();
        }
    }
}
