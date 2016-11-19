using System;
using System.Collections.Generic;
using SpacialPrisonerDilemma.Engine;
using SpacialPrisonerDilemma.Engine.Neighbourhoods;

namespace SPD.Engine.Neighbourhoods
{
    public class Moore : INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        readonly int _dist;

        public Moore(int width, int height, int distance=1)
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
            if (!IsValid(x, y)) throw new ArgumentException();
            for (var xi = -_dist; xi <= _dist; xi++)
            for (var yi = -_dist; yi <= _dist; yi++)
            {
                if (IsValid(x + xi, y + yi) && !(xi == 0 && yi == 0)) yield return new Coord(x + xi, y + yi);
            }
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            for (int xi = 0; xi <= _dist; xi++)
            for (int yi = -_dist; yi < 0; yi++)
            {
                if (xi == 0 && yi == 0) continue;
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);
            }
            for (int xi = 1; xi <= _dist; xi++)
            for (int yi = 0; yi <= _dist; yi++)
            {
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);
            }
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }

    public class MooreTorus : INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        private readonly int _dist;

        public MooreTorus(int width, int height, int distance = 1)
        {
            _width = width;
            _height = height;
            _dist = distance;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            var sx = x + _width;
            var sy = y + _height;

            for (var xi = -_dist; xi <= _dist; xi++)
            for (var yi = -_dist; yi <= _dist; yi++)
            {
                if (IsValid(x + xi, y + yi) && !(xi == 0 && yi == 0))
                    yield return new Coord((xi + sx) % _width, (yi + sy) % _height);
            }
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

            for (int xi = 0; xi <= _dist; xi++)
            for (int yi = -_dist; yi < 0; yi++)
            {
                if (xi == 0 && yi == 0) continue;
                yield return new Coord((x + xi + sx) % _width, (y + yi + sy) % _height);
            }
            for (int xi = 1; xi <= _dist; xi++)
            for (int yi = 0; yi <= _dist; yi++)
            {
                yield return new Coord((xi + sx) % _width, (yi + sy) % _height);
            }
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }
    }
}
