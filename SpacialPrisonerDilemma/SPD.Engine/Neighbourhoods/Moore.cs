using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine.Neighbourhoods
{
    public class Moore : INeighbourhood
    {
        int _width;
        int _height;

        public Moore(int width, int height)
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
            if (!IsValid(x, y)) throw new ArgumentException();
            if (IsValid(x, y - 1)) yield return new Coord(x, y - 1);
            if (IsValid(x + 1, y - 1)) yield return new Coord(x + 1, y - 1);

            if (IsValid(x + 1, y)) yield return new Coord(x + 1, y);
            if (IsValid(x + 1, y + 1)) yield return new Coord(x + 1, y + 1);

            if (IsValid(x, y + 1)) yield return new Coord(x, y + 1);
            if (IsValid(x - 1, y + 1)) yield return new Coord(x - 1, y + 1);

            if (IsValid(x - 1, y)) yield return new Coord(x - 1, y);
            if (IsValid(x - 1, y - 1)) yield return new Coord(x - 1, y - 1);
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            if (IsValid(x, y - 1)) yield return new Coord(x, y - 1);
            if (IsValid(x + 1, y - 1)) yield return new Coord(x + 1, y - 1);

            if (IsValid(x + 1, y)) yield return new Coord(x + 1, y);
            if (IsValid(x + 1, y + 1)) yield return new Coord(x + 1, y + 1);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }

    public class MooreTorus : INeighbourhood
    {
        int _width;
        int _height;

        public MooreTorus(int width, int height)
        {
            _width = width;
            _height = height;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;

            yield return new Coord(x, (sy - 1) % _height);
            yield return new Coord((sx + 1) % _width, (sy - 1) % _height);

            yield return new Coord((sx + 1) % _width, y);
            yield return new Coord((sx + 1) % _width, (sy + 1) % _height);

            yield return new Coord(x, (sy + 1) % _height);
            yield return new Coord((sx - 1) % _width, (sy + 1) % _height);

            yield return new Coord((sx - 1) % _width, y);
            yield return new Coord((sx - 1) % _width, (sy - 1) % _height);
        }

        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;

            yield return new Coord(x, (sy - 1) % _height);
            yield return new Coord((sx + 1) % _width, (sy - 1) % _height);

            yield return new Coord((sx + 1) % _width, y);
            yield return new Coord((sx + 1) % _width, (sy + 1) % _height);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }
}
