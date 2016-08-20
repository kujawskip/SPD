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
        readonly Tuple<Cell, Cell> _cells;
        readonly List<Tuple<bool, bool>> _story;

        private Cell C1 { get { return _cells.Item1; } }
        private Cell C2 { get { return _cells.Item2; } }

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="c1">Komórka c1</param>
        /// <param name="c2">Przeciwnik komórki c1</param>
        public Skirmish(Cell c1, Cell c2)
        {
            _cells = new Tuple<Cell, Cell>(c1, c2);
            _story = new List<Tuple<bool, bool>>();
        }

        /// <summary>
        /// Wykonaj pojedynczy ruch komórek przeciw sobie
        /// </summary>
        internal void SingleMove()
        {
            _currentStep = new Tuple<bool, bool>(C1.Decide(C2), C2.Decide(C1));
        }

        Tuple<bool, bool> _currentStep;

        /// <summary>
        /// Dodaj ostatni ruch do zbioru ruchów wykonanych w aktualnym kroku automatu
        /// </summary>
        internal void EndStep()
        {
            if (_currentStep == null) throw new Exception();
            _story.Add(_currentStep);
            _currentStep = null;
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
                        new Tuple<Cell, bool>(C1, _story[step].Item1),
                        new Tuple<Cell, bool>(C2, _story[step].Item2)
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
                if(_story.Count==0)
                    return new Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>>(
                        new Tuple<Cell, bool>(C1, false),
                        new Tuple<Cell, bool>(C2, false)
                        );
                else
                    return new Tuple<Tuple<Cell, bool>, Tuple<Cell, bool>>(
                        new Tuple<Cell, bool>(C1, _story.Last().Item1), 
                        new Tuple<Cell, bool>(C2, _story.Last().Item2)
                        );
            }
        }

        /// <summary>
        /// Wyczyszczenie klasy starcia pomiędzy krokami automatu
        /// </summary>
        public void Clear()
        {
            _story.Clear();
        }
    }
}
