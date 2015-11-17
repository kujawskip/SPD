using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    class SPD
    {
        private static SPD singleton;
        public static SPD Singleton
        {
            get
            {
                if (singleton == null) singleton = new SPD();
                return singleton;
            }
        }

        public static void Initialize(int[,] initialConfig, int stepsPerIteration)
        {

        }

        public static void Initialize(WhenBetray[,] initialConfig, int stepsPerIteration)
        {

        }

        Cell[,] cells;
        Dictionary<Tuple<Cell, Cell>, Skirmish> skirmishes;
        List<Cell[,]> history;

        protected void Step()
        {
            throw new NotImplementedException();
        }

        protected void Iterate()
        {

        }

        public int CurrentIteration { get; protected set; }

        Skirmish GetSkirmish(Cell c1, Cell c2)
        {
            throw new NotImplementedException();
        }

        Cell[] GetNeighbours(Cell c)
        {
            throw new NotImplementedException();
        }

        Cell[,] GetStateByIteration(int i)
        {
            throw new NotImplementedException();
        }
    }
}
