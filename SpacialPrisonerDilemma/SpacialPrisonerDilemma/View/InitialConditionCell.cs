using System;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Komórka uk³adu pocz¹tkowego
    /// </summary>
    [Serializable]
    internal class InitialConditionCell
    {
        internal int X { get; set; }
        internal int Value { get; set; }
        internal int Set { get; set; }
        internal int Y { get; set; }
        /// <summary>
        /// Konstruktor komórki
        /// </summary>
        /// <param name="x">Wspó³rzêdna x</param>
        /// <param name="y">Wspó³rzêdna y</param>
        /// <param name="value">Wartoœæ komórki (strategia)</param>
        /// <param name="set">Logiczny podzia³ komórek</param>
        internal InitialConditionCell(int x, int y, int value, int set)
        {
            X = x;
            Y = y;
            Set = set;
            Value = value;
        }
        /// <summary>
        /// Zwraca kopie komórki
        /// </summary>
        /// <returns>Kopia komórki</returns>
        internal InitialConditionCell GetCopy()
        {
            return new InitialConditionCell(X,Y,Value,Set);
        }
    }
}