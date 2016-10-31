using System;
using System.Collections.Generic;

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
            for (int i = 1; i <= _dist; i++)
            {
                if (IsValid(x, y - i)) yield return new Coord(x, y - i);
                if (IsValid(x + i, y - i)) yield return new Coord(x + i, y - i);

                if (IsValid(x + i, y)) yield return new Coord(x + i, y);
                if (IsValid(x + i, y + i)) yield return new Coord(x + i, y + i);

                if (IsValid(x, y + i)) yield return new Coord(x, y + i);
                if (IsValid(x - i, y + i)) yield return new Coord(x - i, y + i);

                if (IsValid(x - i, y)) yield return new Coord(x - i, y);
                if (IsValid(x - i, y - i)) yield return new Coord(x - i, y - i);
            }
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            if (!IsValid(x, y)) throw new ArgumentException();
            for (int i = 1; i <= _dist; i++)
            {
                if (IsValid(x, y - i)) yield return new Coord(x, y - i);
                if (IsValid(x + i, y - i)) yield return new Coord(x + i, y - i);

                if (IsValid(x + i, y)) yield return new Coord(x + i, y);
                if (IsValid(x + i, y + i)) yield return new Coord(x + i, y + i);
            }
        }
        public override string ToString()
        {
            return string.Format("Moore({0})", _dist);
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

            for(int i=1; i<=_dist; i++)
            {

                yield return new Coord(x, (sy - i) % _height);
                yield return new Coord((sx + i) % _width, (sy - i) % _height);

                yield return new Coord((sx + i) % _width, y);
                yield return new Coord((sx + i) % _width, (sy + i) % _height);

                yield return new Coord(x, (sy + i) % _height);
                yield return new Coord((sx - i) % _width, (sy + i) % _height);

                yield return new Coord((sx - i) % _width, y);
                yield return new Coord((sx - i) % _width, (sy - i) % _height);
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

            for (int i = 1; i <= _dist; i++)
            {

                yield return new Coord(x, (sy - i) % _height);
                yield return new Coord((sx + i) % _width, (sy - i) % _height);

                yield return new Coord((sx + i) % _width, y);
                yield return new Coord((sx + i) % _width, (sy + i) % _height);
            }
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetHalfNeighbours(c.X, c.Y);
        }

        public override string ToString()
        {
            return string.Format("MooreTorus({0})", _dist);
        }
    }
}
