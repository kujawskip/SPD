using System;

namespace SpacialPrisonerDilemma.View
{
    [Serializable]
    public class InitialConditionCell
    {
        public int X { get; set; }
        public int Value { get; set; }
        public int Set { get; internal set; }
        public int Y { get; set; }

        public InitialConditionCell(int x, int y, int value, int set)
        {
            X = x;
            Y = y;
            Set = set;
            Value = value;
        }

        public InitialConditionCell GetCopy()
        {
            return new InitialConditionCell(X,Y,Value,Set);
        }
    }
}