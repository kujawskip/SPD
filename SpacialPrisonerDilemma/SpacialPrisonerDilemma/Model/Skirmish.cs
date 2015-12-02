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

        private Cell c1 { get { return cells.Item1; } }
        private Cell c2 { get { return cells.Item2; } }

        public Skirmish(Cell c1, Cell c2)
        {
            cells = new Tuple<Cell, Cell>(c1, c2);
            story = new List<Tuple<bool, bool>>();
        }

        internal void SingleMove()
        {
            currentStep = new Tuple<bool, bool>(c1.Decide(c2), c2.Decide(c1));
        }

        Tuple<bool, bool> currentStep;
        internal void EndStep()
        {
            story.Add(currentStep);
            currentStep = null;
        }

        public Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>> this[int step]
        {
            get
            {
                return new Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>>(
                        new Tuple<Cell, bool>(c1, story[step].Item1),
                        new Tuple<Cell, bool>(c2, story[step].Item2)
                        );
            }
        }

        public Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>> Last
        {
            get
            {
                if(story.Count==0)
                    return new Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>>(
                        new Tuple<Cell, bool>(c1, false),
                        new Tuple<Cell, bool>(c2, false)
                        );
                else
                    return new Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>>(
                        new Tuple<Cell, bool>(c1, story.Last().Item1), 
                        new Tuple<Cell, bool>(c2, story.Last().Item2)
                        );
            }
        }

        public void Clear()
        {
            story.Clear();
        }
    }
}
