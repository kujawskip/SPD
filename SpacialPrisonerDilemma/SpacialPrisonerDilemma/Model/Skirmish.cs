using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacialPrisonerDilemma.Model
{
    internal class Skirmish
    {
        Tuple<Cell, Cell> cells;
        List<Tuple<bool, bool>> story;

        public Skirmish(Cell c1, Cell c2)
        {
            cells = new Tuple<Cell, Cell>(c1, c2);
        }

        void SingleMove()
        {
            throw new NotImplementedException();
        }

        public Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>> this[int step]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>> Last
        {
            get { throw new NotImplementedException(); }
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
