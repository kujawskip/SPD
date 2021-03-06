﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using SPD.Engine.Strategies;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Klasa implementująca macierz układu początkowego
    /// </summary>
    [Serializable]
    public class InitialConditionsGrid 
    {
        public DrawingImage GenerateImage(int x, int y, int width, int height, double CanvasWidth, double CanvasHeight,int replace=-1)
        {
            var cellWidth = CanvasWidth / width;
            var cellHeight = CanvasHeight / height;


            var dg = new DrawingGroup();
            for (var i = x; i < x + width; i++)
            {
                for (var j = y; j < y + height; j++)
                {

                    var rg = new RectangleGeometry(new Rect(new Point((i - x) * cellWidth, (j - y) * cellHeight), new Point((i - x + 1) * cellWidth, (j + 1 - y) * cellHeight)));
                    var gd = new GeometryDrawing
                    {
                        Brush = CellGrid[i,j].Value==replace?new SolidColorBrush(Color.FromRgb(0,0,0)):SPDAssets.GetBrush(this.CellGrid[i, j].Value),
                        Geometry = rg
                    };
                    dg.Children.Add(gd);
                }
            }
            return new DrawingImage(dg);
        }
        /// <summary>
        /// Metoda generująca macierz losowego układu
        /// </summary>
        /// <param name="r">Randomizer</param>
        /// <param name="size">Rozmiar układu</param>
        /// <param name="stateCount">Ilość stanów</param>
        /// <returns>Macierz układu</returns>
        internal static InitialConditionsGrid GenerateRandom(Random r,int size=100,int stateCount=SPDAssets.MAX)
        {
            int x = size;
            int y = size;
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[stateCount];
            for(int i=0;i<setLists.Length;i++) setLists[i]=new List<InitialConditionCell>();
            InitialConditionCell[,] grid = new InitialConditionCell[x,y];
            for(int i=0;i<x;i++)
                for (int j = 0; j < y; j++)
                {
                    int k = r.Next(setLists.Length);
                    InitialConditionCell c = new InitialConditionCell(i,j,k,k);
                    grid[i, j] = c;
                    setLists[k].Add(c);

                }
            var sets = setLists.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = grid,
                CellSets = sets
            };
            return ig;
        }
        internal InitialConditionCell[][] CellSets;
        internal InitialConditionCell[,] CellGrid;
        /// <summary>
        /// Zmiana wartości konkretnej komórki
        /// </summary>
        /// <param name="x">Współrzędna X komórki</param>
        /// <param name="y">Współrzędna Y komórki</param>
        /// <param name="value">Wartość do podmiany</param>
        internal void ChangeValue(int x, int y, int value)
        {
            CellGrid[x,y].Value = value;
        }
        /// <summary>
        /// Metoda wypełniająca komórki w tym samym secie co podana
        /// </summary>
        /// <param name="x">Współrzędna X podanej komórki</param>
        /// <param name="y">Współrzędna Y podanej komórki</param>
        /// <param name="value">Wartość do wypełnienia</param>
        internal void Fill(int x, int y, int value)
        {
            int index = CellGrid[x,y].Set;
            Fill(index,value);
        }
        /// <summary>
        /// Metoda wypełniająca cały podany set wybraną wartością
        /// </summary>
        /// <param name="index">Set</param>
        /// <param name="value">Wartość</param>
        internal void Fill(int index, int value)
        {
            for (int i = 0; i < CellSets[index].Length; i++)
            {
                CellSets[index][i].Value = value;
                
            } 
        }
        /// <summary>
        /// Metoda wypełniająca komórki o secie X wartością funkcji s(X)
        /// </summary>
        /// <param name="s">Funkcja transformująca</param>
        /// <param name="stateCount">Ilość możliwych wartości</param>
        internal void Transform(StateTransformation s,int stateCount=SPDAssets.MAX)
        {

            for(int i=0;i<stateCount;i++) Fill(i,s(i));
            List<List<InitialConditionCell>> cells = new List<List<InitialConditionCell>>();
            while (cells.Count < stateCount)
            {
                cells.Add(new List<InitialConditionCell>());
            }
            for(int i =0;i<CellGrid.GetLength(0);i++)
                for (int j = 0; j < CellGrid.GetLength(1); j++)
                {
                    CellGrid[i, j].Set = CellGrid[i, j].Value;
                    while (cells.Count < CellGrid[i, j].Set+1)
                    {
                        cells.Add(new List<InitialConditionCell>());
                    }
                    cells[CellGrid[i, j].Set].Add(CellGrid[i, j]);
                }
            CellSets = cells.Select(x => x.ToArray()).ToArray();
        }
        /// <summary>
        /// Metoda factory generująca donut
        /// </summary>
        /// <param name="size">Rozmiar układu</param>
        /// <param name="stateCount">Ilość możliwych wartości</param>
        /// <returns>Układ początkowy z donutem</returns>
        internal static InitialConditionsGrid DonutFactory(int size=30,int stateCount=SPDAssets.MAX)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size,size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[stateCount];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for(int i=0;i<size;i++)
                for (int j = 0; j < size; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i,j,-1,0);
                    for (int k = stateCount-1; k >= 0; k--)
                    {


                        if ((i - size/2)*(i - size/2) + (j - size/2)*(j - size/2) > (size*(size/4))/(k+1))
                        {
                            c.Set = k;
                        }
                    }
                    if (c.Set < 0) c.Set = stateCount;
                    ic[i, j] = c;
                    setLists[c.Set].Add(c);
                }
            var sets = setLists.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = sets
            };
            return ig;
        }

        internal static InitialConditionsGrid NowakMayFactory(int size = 31, int StateCount = SPDAssets.MAX)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[SPDAssets.MAX];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    int k=-1;
                    if (size%2 == 0)
                    {
                        if (i == size/2 || i + 1 == size/2)
                        {
                            if (j == size / 2 || j + 1 == size / 2)
                            {
                                k = 0;
                            }
                        }
                    }
                    else
                    {
                        if (i == size / 2)
                        {
                            if (j == size / 2 )
                            {
                                k = 0;
                            }
                        }
                    }
                    if (k < 0) k = StateCount - 1;
                    InitialConditionCell c = new InitialConditionCell(i,j,k,k);
                    ic[i, j] = c;
                    setLists[c.Set].Add(c);
                }
            var sets = setLists.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = sets
            };
            return ig;
        }
        /// <summary>
        /// Metoda factory generująca koło
        /// </summary>
        /// <param name="size">Rozmiar układu</param>
        /// <param name="stateCount">Ilość możliwych wartości</param>
        /// <returns>Układ początkowy z kołem</returns>
        internal static InitialConditionsGrid CircleFactory(int size=30,int stateCount=SPDAssets.MAX)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[SPDAssets.MAX];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, -1);
                    for (int k = 0;k<stateCount-1;k++)
                    {

                        
                        if ((i - size/2) * (i - size/2) + (j - size/2) * (j - size/2) <= (size/2)*(size/2) / ((k+1)*(k + 1)))
                        {
                            c.Set = k;
                        }
                    }
                    if (c.Set < 0)
                    {
                        c.Set = stateCount-1;
                    }
                    ic[i, j] = c;
                    setLists[c.Set].Add(c);
                }
            var sets = setLists.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = sets
            };
            return ig;
        }
        /// <summary>
        /// Metoda tworząca kopię układu
        /// </summary>
        /// <returns>Kopia układu</returns>
        internal InitialConditionsGrid GetCopy()
        {
            List<InitialConditionCell[]> list = new List<InitialConditionCell[]>();
            InitialConditionCell[,] grid = new InitialConditionCell[CellGrid.GetLength(0), CellGrid.GetLength(1)];
            foreach (InitialConditionCell[] t in CellSets)
            {
                List<InitialConditionCell> l = new List<InitialConditionCell>();
                foreach (InitialConditionCell t1 in t)
                {
                    var c = t1.GetCopy();
                    grid[c.X, c.Y] = c;
                    l.Add(c);
                }
                list.Add(l.ToArray());
            }
            var set = list.ToArray();
            return new InitialConditionsGrid
            {
                CellGrid = grid,
                CellSets = set
            };

        }
        /// <summary>
        /// Metoda factory generująca przekątną
        /// </summary>
        /// <param name="size">Rozmiar układu</param>
        /// <param name="stateCount">Ilość możliwych wartości</param>
        /// <returns>Układ początkowy z przekątną</returns>
        internal static InitialConditionsGrid DiagonalFactory(int size=30,int stateCount=SPDAssets.MAX)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[stateCount];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    int k = Math.Abs(i - j);
                    int m = Math.Max(ic.GetLength(0), ic.GetLength(1));
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, k * (stateCount) / m);
                   
                    
                    ic[i, j] = c;
                    setLists[c.Set].Add(c);
                }
            var sets = setLists.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = sets
            };
            return ig;
        }

        internal static InitialConditionsGrid FromCellArray(Tuple<int,float>[,] cells)
        {
            InitialConditionCell[,] arr = new InitialConditionCell[cells.GetLength(0),cells.GetLength(1)];
            var list = new List<List<InitialConditionCell>>();
            for(int i=0;i<SPDAssets.MAX;i++) list.Add(new List<InitialConditionCell>());
            for(int i=0;i<cells.GetLength(0);i++)
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    int k = (new IntegerStrategy(cells[i,j].Item1)).BetrayalThreshold;
                    arr[i,j] = new InitialConditionCell(i,j,k,k);
                    list[k].Add(arr[i, j]);
                }
            var arr2 = list.Select(l => l.ToArray()).ToArray();
            return new InitialConditionsGrid {CellGrid = arr, CellSets = arr2};
        }
    }
}
