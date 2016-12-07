using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPD.Engine.Neighbourhoods
{
    public class Taxi:INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        readonly int _dist;

        public Taxi(int width, int height, int distance)
        {
            _width = width;
            _height = height;
            _dist = distance;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            for (int xi = 0; xi < _dist; xi++)
            for (int yi = 1; yi <= _dist - xi; yi++)
            {
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);
                if (IsValid(x - xi, y - yi)) yield return new Coord(x - xi, y - yi);
            }

            for (int yi = 0; yi < _dist; yi++)
            for (int xi = 1; xi <= _dist - yi; xi++)
            {
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);
                if (IsValid(x - xi, y - yi)) yield return new Coord(x - xi, y - yi);
            }
        }

        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            for (int xi = 0; xi < _dist; xi++)
            for (int yi = 1; yi <= _dist - xi; yi++)
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);

            for (int yi = 0; yi < _dist; yi++)
            for (int xi = 1; xi <= _dist - yi; xi++)
                if (IsValid(x + xi, y + yi)) yield return new Coord(x + xi, y + yi);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        bool IsValid(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }
    }

    public class TaxiTorus:INeighbourhood
    {
        readonly int _width;
        readonly int _height;
        readonly int _dist;
        public TaxiTorus(int width, int height, int distance)
        {
            _width = width;
            _height = height;
            _dist = distance;
        }

        public IEnumerable<Coord> GetNeighbours(int x, int y)
        {
            int sx = x + _width;
            int sy = y + _height;
            for (int xi = 0; xi < _dist; xi++)
            for (int yi = 1; yi <= _dist - xi; yi++)
            {
                yield return new Coord((sx + xi) % _width, (sy + yi) % _height);
                yield return new Coord((sx - xi) % _width, (sy - yi) % _height);
            }

            for (int yi = 0; yi < _dist; yi++)
            for (int xi = 1; xi <= _dist - yi; xi++)
            {
                yield return new Coord((sx + xi) % _width, (sy + yi) % _height);
                yield return new Coord((sx - xi) % _width, (sy - yi) % _height);
            }
        }

        public IEnumerable<Coord> GetNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }

        public IEnumerable<Coord> GetHalfNeighbours(int x, int y)
        {
            int sx = x + _width;
            int sy = y + _height;
            for (int xi = 0; xi < _dist; xi++)
            for (int yi = 1; yi <= _dist - xi; yi++)
                yield return new Coord((sx + xi) % _width, (sy + yi) % _height);

            for (int yi = 0; yi < _dist; yi++)
            for (int xi = 1; xi <= _dist - yi; xi++)
                yield return new Coord((sx + xi) % _width, (sy + yi) % _height);
        }

        public IEnumerable<Coord> GetHalfNeighbours(Coord c)
        {
            return GetNeighbours(c.X, c.Y);
        }
    }
}
