using System.Collections.Generic;

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
