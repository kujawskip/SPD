using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Engine.Neighbourhoods
{
    public class VonNeumann:INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        readonly int _dist;
        public VonNeumann(int width, int height, int distance)
        {
            _width = width;
            _height = height;
            _dist = distance;
        }
        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            for (int i = 1; i <= _dist; i++)
            {
                if (y - i > 0) yield return new Coord(x, y - i);
                if (x + i < _width) yield return new Coord(x + i, y);
                if (y + i < _height) yield return new Coord(x, y + i);
                if (x - i > 0) yield return new Coord(x - i, y);
            }
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            for (int i = 1; i <= _dist; i++)
            {
                if (y - i > 0) yield return new Coord(x, y - i);
                if (x + i < _width) yield return new Coord(x + i, y);
            }
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
        readonly int _dist;
        public VonNeumannTorus(int width, int height, int distance)
        {
            _width = width;
            _height = height;
            _dist = distance;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (x < 0 || y < 0 || x >= _width || y >= _width) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;
            for (int i = 1; i <= _dist; i++)
            {
                yield return new Coord(x, (sy - i) % _height);
                yield return new Coord((sx + i) % _width, y);
                yield return new Coord(x, (sy + i) % _height);
                yield return new Coord((sx - i) % _width, y);
            }
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
            for (int i = 1; i <= _dist; i++)
            {
                yield return new Coord(x, (sy - i) % _height);
                yield return new Coord((sx + i) % _width, y);
            }
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }
}
