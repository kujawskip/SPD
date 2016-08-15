using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine.Neighbourhoods
{
    public interface INeighbourhood
    {
        IEnumerable<Coord> GetNeighbours(int x, int y);
        IEnumerable<Coord> GetNeighbours(Coord c);

        IEnumerable<Coord> GetHalfNeighbours(int x, int y);
        IEnumerable<Coord> GetHalfNeighbours(Coord c);
    }
}
