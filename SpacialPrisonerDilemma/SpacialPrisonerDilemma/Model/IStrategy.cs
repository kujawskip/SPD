namespace SpacialPrisonerDilemma.Model
{
    public interface IStrategy
    {
        /// <summary>
        /// Implementacja podejmowania decyzji przez strategię
        /// </summary>
        /// <param name="parent">Komórka decydująca</param>
        /// <param name="opponent">Komórka, przeciw której podejmowana jest decyzja</param>
        /// <returns>True jeśli zdrada, false w przeciwnym przypadku</returns>
        bool Decide(Cell parent, Cell opponent);
    }
}
