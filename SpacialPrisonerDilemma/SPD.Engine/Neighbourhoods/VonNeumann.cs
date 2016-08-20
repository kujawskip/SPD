using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine.Neighbourhoods
{
    class VonNeumann:INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        public VonNeumann(int width, int height)
        {
            _width = width;
            _height = height;
        }
        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            if (y - 1 > 0) yield return new Coord(x, y - 1);
            if (x + 1 < _width) yield return new Coord(x + 1, y);
            if (y + 1 < _height) yield return new Coord(x, y + 1);
            if (x - 1 > 0) yield return new Coord(x - 1, y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            if (y - 1 > 0) yield return new Coord(x, y - 1);
            if (x + 1 < _width) yield return new Coord(x + 1, y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }

    public class VonNeumannTorus : INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        public VonNeumannTorus(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;
            yield return new Coord(x, (sy - 1) % _height);
            yield return new Coord((sx + 1) % _width, y);
            yield return new Coord(x, (sy + 1) % _height);
            yield return new Coord((sx - 1) % _width, y);
        }

        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;
            yield return new Coord(x, (sy - 1) % _height);
            yield return new Coord((sx + 1) % _width, y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }
}
