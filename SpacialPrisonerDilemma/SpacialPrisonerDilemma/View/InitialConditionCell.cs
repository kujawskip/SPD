using System;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Kom�rka uk�adu pocz�tkowego
    /// </summary>
    [Serializable]
    internal class InitialConditionCell
    {
        internal int X { get; set; }
        internal int Value { get; set; }
        internal int Set { get; set; }
        internal int Y { get; set; }
        /// <summary>
        /// Konstruktor kom�rki
        /// </summary>
        /// <param name="x">Wsp�rz�dna x</param>
        /// <param name="y">Wsp�rz�dna y</param>
        /// <param name="value">Warto�� kom�rki (strategia)</param>
        /// <param name="set">Logiczny podzia� kom�rek</param>
        internal InitialConditionCell(int x, int y, int value, int set)
        {
            X = x;
            Y = y;
            Set = set;
            Value = value;
        }
        /// <summary>
        /// Zwraca kopie kom�rki
        /// </summary>
        /// <returns>Kopia kom�rki</returns>
        internal InitialConditionCell GetCopy()
        {
            return new InitialConditionCell(X,Y,Value,Set);
        }
    }
}