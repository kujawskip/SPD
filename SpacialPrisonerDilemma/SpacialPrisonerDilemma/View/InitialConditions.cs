using System;
using System.Collections.Generic;
using System.Linq;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Klasa reprezentuj�ca uk�ad pocz�tkowy
    /// </summary>
    [Serializable]
    internal class InitialConditions
    {
        internal InitialConditionsGrid Grid;
        internal String Name;
        private static Dictionary<Tuple<int, bool>, StateTransformation> Transformations;
        /// <summary>
        /// Inicjalizacja obiekt�w statycznych
        /// </summary>
        internal static void Initialise()
        {
            Transformations = new Dictionary<Tuple<int, bool>, StateTransformation>
            {
                {new Tuple<int, bool>(10, false), x => (x)},
                {new Tuple<int, bool>(6, false), x => (x >= 5 ? 9 : x)}
            };
            Dictionary<Tuple<int, bool>, StateTransformation> tempDictionary =
                new Dictionary<Tuple<int, bool>, StateTransformation>();
            foreach (var kv in Transformations.Where(kv => !kv.Key.Item2))
            {
                var kv1 = kv;
                tempDictionary.Add(new Tuple<int, bool>(kv.Key.Item1,true),x=>(kv1.Value(kv1.Key.Item1-1-x)) );
            }
            foreach (var kv in tempDictionary)
            {
                Transformations.Add(kv.Key, kv.Value);

            }
        }
        /// <summary>
        /// Metoda generuj�ca losowy uk�ad
        /// </summary>
        /// <param name="size">Rozmiar uk�adu</param>
        /// <param name="stateCount">Ilo�� mo�liwych warto�ci kom�rek</param>
        /// <returns>Uk�ad pocz�tkowy</returns>
        internal static InitialConditions GenerateRandom(int size=100,int stateCount=10)
        {
            Random r = new Random();
            var Grid = InitialConditionsGrid.GenerateRandom(r, size, stateCount);
            Grid.Transform(Transformations[new Tuple<int, bool>(stateCount,false)],stateCount);
            var ic = new InitialConditions
            {
                Name = "Losowy" + r.Next(),
               Grid = Grid
            };
            return ic;
        }
        /// <summary>
        /// Metoda factory generuj�ca ko�o
        /// </summary>
        /// <param name="reversed">Czy warto�ci maj� by� odwr�cone</param>
        /// <param name="size">Rozmiar</param>
        /// <param name="stateCount">Ilo�� mo�liwych warto�ci kom�rek</param>
        /// <returns>Uk�ad pocz�tkowy z ko�em</returns>
        internal static InitialConditions CircleFactory(bool reversed=false,int size=30,int stateCount = 30)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.CircleFactory(size,stateCount);
            ig.Transform(Transformations[new Tuple<int, bool>(stateCount, reversed)],stateCount);
            var ic = new InitialConditions
            {
                Name = "Ko�o " + (reversed ? "- odwr�cone kolory" : ""),
                Grid = ig
            };
            return ic;
        }
        /// <summary>
        /// Metoda factory generuj�ca donut
        /// </summary>
        /// <param name="reversed">Czy warto�ci maj� by� odwr�cone</param>
        /// <param name="size">Rozmiar</param>
        /// <param name="stateCount">Ilo�� mo�liwych warto�ci kom�rek</param>
        /// <returns>Uk�ad pocz�tkowy z donutem</returns>
        internal static InitialConditions DonutFactory(bool reversed=false,int size=30,int stateCount=10)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.DonutFactory(size,stateCount);
            ig.Transform(Transformations[new Tuple<int, bool>(stateCount, reversed)],stateCount);
            var ic = new InitialConditions
            {
                Name = "Donut " + (reversed ? "- odwr�cone kolory" : ""),
                Grid = ig
            };
            return ic;
        }

        /// <summary>
        /// Metoda factory generuj�ca przek�tn�
        /// </summary>
        /// <param name="reversed">Czy warto�ci maj� by� odwr�cone</param>
        /// <param name="size">Rozmiar</param>
        /// <param name="stateCount">Ilo�� mo�liwych warto�ci kom�rek</param>
        /// <returns>Uk�ad pocz�tkowy z przek�tn�</returns>
        internal static InitialConditions DiagonalFactory(bool reversed=false,int size=30,int stateCount=10)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.DiagonalFactory(size,stateCount);
            ig.Transform(Transformations[new Tuple<int, bool>(stateCount,reversed)],stateCount);
            var ic = new InitialConditions
            {
                Name = "Przek�tna " + (reversed ? "- odwr�cone kolory" : ""),
                Grid = ig
            };
            return ic; 
        }

        internal InitialConditions GetCopy()
        {
            return new InitialConditions {Name = Name, Grid = Grid.GetCopy()};

        }

        internal static InitialConditions FromCellArray(Cell[,] cells, string getFileName="FileLoaded")
        {
            InitialConditionsGrid icg = InitialConditionsGrid.FromCellArray(cells);
            
            return new InitialConditions {Name = getFileName,Grid = icg};
        }
    }
}