using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpacialPrisonerDilemma.Engine;
using SpacialPrisonerDilemma.Engine.Neighbourhoods;

namespace SPD.Engine.Neighbourhoods
{
    public class Mixed:INeighbourhood
    {
        private INeighbourhood[] n;
        public Mixed(params INeighbourhood[] neighbourhoods)
        {
            n = neighbourhoods;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            return n.SelectMany(z => z.GetNeighbours(x, y));
        }

        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            return n.SelectMany(z => z.GetHalfNeighbours(x, y));
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }

        public override string ToString()
        {
            return n.Aggregate("", (s, x) => string.Format("{0}+{1}", s, x));
        }
    }
}
