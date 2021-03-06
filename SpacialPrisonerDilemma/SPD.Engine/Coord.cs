﻿using System;

namespace SPD.Engine
{
    public class Coord : IEquatable<Coord>
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }
        public bool Equals(Coord other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object other)
        {
            return (other is Coord) && Equals(other as Coord);
        }

        public override int GetHashCode()
        {
            return X ^ Y;
        }

        public static bool operator ==(Coord X, Coord Y)
        {
            return X.Equals(Y);
        }

        public static bool operator !=(Coord X, Coord Y)
        {
            return !(X == Y);
        }


        public override string ToString()
        {
            return string.Format("({0},{1})", X, Y);
        }
    }

    public class CoordPair : IEquatable<CoordPair>
    {
        public Coord C1 { get; private set; }
        public Coord C2 { get; private set; }
        public CoordPair(Coord c1, Coord c2)
        {
            C1 = c1; C2 = c2;
        }
        public CoordPair(int x1, int y1, int x2, int y2) : this(new Coord(x1, y1), new Coord(x2, y2)) { }
        public bool Equals(CoordPair other)
        {
            return (C1.Equals(other.C1) && C2.Equals(other.C2)) || (C1.Equals(other.C2) && C2.Equals(other.C1));
        }

        public override string ToString()
        {
            return string.Format("[{0},{1}]", C1, C2);
        }
    }
}
