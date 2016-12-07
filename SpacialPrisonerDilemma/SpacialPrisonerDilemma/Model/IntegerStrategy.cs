using System.Linq;

namespace SpacialPrisonerDilemma.Model
{
    /// <summary>
    /// Enumeracja rodzajów strategii typu IntegerStrategy
    /// </summary>
    public enum WhenBetray
    {
        Always,
        When1,
        When2,
        When3,
        When4,
        When5,
        When6,
        When7,
        When8,
        Never
    }

    /// <summary>
    /// Implementacja strategii zdradzającej gdy w poprzednim ruchu została zdradzona przez zadaną liczbę sąsiadów.
    /// </summary>
    class IntegerStrategy : IStrategy
    {
        /// <summary>
        /// Próg, po którym komórka zdradza
        /// </summary>
        public virtual int Treshold { get; protected set; }

        /// <summary>
        /// Rodzaj strategii
        /// </summary>
        public WhenBetray StrategyType
        {
            get { return (WhenBetray)Treshold; }
            set { Treshold = (int)value; }
        }

        /// <summary>
        /// Zbiór wszystkich możliwych strategii tego typu
        /// </summary>
        public static IntegerStrategy[] Strategies
        {
            get
            {
                return new[]
                {
                    new IntegerStrategy(0),
                    new IntegerStrategy(1),
                    new IntegerStrategy(2),
                    new IntegerStrategy(3),
                    new IntegerStrategy(4),
                    new IntegerStrategy(5),
                    new IntegerStrategy(6),
                    new IntegerStrategy(7),
                    new IntegerStrategy(8),
                    new IntegerStrategy(9),
                };
            }
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="treshold">Próg, po którym komórka zdradza</param>
        public IntegerStrategy(int treshold)
        {
            Treshold = treshold;
        }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="whenBetray">Rodzaj strategii</param>
        public IntegerStrategy(WhenBetray whenBetray):this((int)whenBetray)
        { }

        public override string ToString()
        {
            return "IntegrtStrategy " + ((WhenBetray) Treshold).ToString();
        }

        /// <summary>
        /// Implementacja podejmowania decyzji przez strategię
        /// </summary>
        /// <param name="parent">Komórka decydująca</param>
        /// <param name="opponent">Komórka, przeciw której podejmowana jest decyzja</param>
        /// <returns>True jeśli zdrada, false w przeciwnym przypadku</returns>
        public bool Decide(Cell parent, Cell opponent)
        {
            var decs = (from neighbour in parent.GetNeighbours()
                        let skirmish = SPD.Singleton.GetSkirmish(parent, neighbour)
                        let last = skirmish != null ? skirmish.Last : null
                        let decision = last != null && (last.Item1.Item1 == parent ? last.Item2.Item2 : last.Item1.Item2)
                        select decision);
            var res = decs.Count(x => x == true) >= Treshold;
            return res;
        }
    }
}
