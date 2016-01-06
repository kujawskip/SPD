using System;
using System.Collections.Generic;
using System.Linq;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    [Serializable]
     public class InitialConditionsGrid 
    {
        internal static InitialConditionsGrid GenerateRandom(Random r,int size=100,int stateCount=10)
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
        public InitialConditionCell[][] CellSets;
        public InitialConditionCell[,] CellGrid;
        public void ChangeValue(int x, int y, int value)
        {
            CellGrid[x,y].Value = value;
        }

        public void Fill(int x, int y, int value)
        {
            int index = CellGrid[x,y].Set;
            Fill(index,value);
        }

        public void Fill(int index, int value)
        {
            for (int i = 0; i < CellSets[index].Length; i++)
            {
                CellSets[index][i].Value = value;
            } 
        }

        internal void Transform(StateTransformation s,int stateCount=10)
        {
            for(int i=0;i<stateCount;i++) Fill(i,s(i));
        }
        internal static InitialConditionsGrid DonutFactory(int size=30,int stateCount=10)
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
        internal static InitialConditionsGrid CrossFactory()
        {
            InitialConditionCell[,] ic = new InitialConditionCell[30, 30];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for(int i=0;i<30; i++)
                for (int j = 0; j < 30; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, -1);
                    if (i < 10 && j < 10) c.Set = (i + j) / 2;
                    if ((i < 10 && j > 20) || (i > 20 && j < 10)) c.Set = (i + j - 10) / 2;
                    if (i > 20 && j > 20) c.Set = (i + j - 20) / 2;
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
        internal static InitialConditionsGrid CircleFactory(int size=30,int stateCount=10)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
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

        internal InitialConditionsGrid GetCopy()
        {
            InitialConditionCell[][] set;
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
            set = list.ToArray();
            return new InitialConditionsGrid
            {
                CellGrid = grid,
                CellSets = set
            };

        }

        internal static InitialConditionsGrid DiagonalFactory(int size=30,int stateCount=10)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] setLists = new List<InitialConditionCell>[stateCount];
            for (int i = 0; i < setLists.Length; i++) setLists[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    int k = Math.Abs(i - j);
                    int m = Math.Max(ic.GetLength(0), ic.GetLength(1));
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, k * (stateCount-1) / m);
                   
                    
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

        public static InitialConditionsGrid FromCellArray(Cell[,] cells)
        {
            InitialConditionCell[,] arr = new InitialConditionCell[cells.GetLength(0),cells.GetLength(1)];
            var list = new List<List<InitialConditionCell>>();
            for(int i=0;i<(Enum.GetValues(typeof(WhenBetray))).Length;i++) list.Add(new List<InitialConditionCell>());
            for(int i=0;i<cells.GetLength(0);i++)
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    int k = ((IntegerStrategy) cells[i, j].Strategy).Treshold;
                    arr[i,j] = new InitialConditionCell(i,j,k,k);
                    list[k].Add(arr[i, j]);
                }
            var arr2 = list.Select(l => l.ToArray()).ToArray();
            return new InitialConditionsGrid {CellGrid = arr, CellSets = arr2};
        }
    }
}
