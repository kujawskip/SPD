using System;
using System.Collections.Generic;
using System.Linq;

namespace SpacialPrisonerDilemma.Model
{
    /// <summary>
    /// Klasa implementująca starcie między dwiema komórkami
    /// </summary>
    internal class Skirmish
    {
        Tuple<Cell, Cell> cells;
        List<Tuple<bool, bool>> story;

        private Cell c1 { get { return cells.Item1; } }
        private Cell c2 { get { return cells.Item2; } }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="c1">Komórka c1</param>
        /// <param name="c2">Przeciwnik komórki c1</param>
        public Skirmish(Cell c1, Cell c2)
        {
            cells = new Tuple<Cell, Cell>(c1, c2);
            story = new List<Tuple<bool, bool>>();
        }

        /// <summary>
        /// Wykonaj pojedynczy ruch komórek przeciw sobie
        /// </summary>
        internal void SingleMove()
        {
            currentStep = new Tuple<bool, bool>(c1.Decide(c2), c2.Decide(c1));
        }

        Tuple<bool, bool> currentStep;

        /// <summary>
        /// Dodaj ostatni ruch do zbioru ruchów wykonanych w aktualnym kroku automatu
        /// </summary>
        internal void EndStep()
        {
            if (currentStep == null) throw new Exception();
            story.Add(currentStep);
            currentStep = null;
        }

        /// <summary>
        /// Akcesor do deyzji podjętych przez komórki w zadanym ruchu.
        /// </summary>
        /// <param name="step">Indeks ruchu</param>
        /// <returns>Para par komórka-decyzja</returns>
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

        /// <summary>
        /// Akcesor do wyniku ostatniego ruchu
        /// </summary>
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

        /// <summary>
        /// Wyczyszczenie klasy starcia pomiędzy krokami automatu
        /// </summary>
        public void Clear()
        {
            story.Clear();
        }
    }
}
