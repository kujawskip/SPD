using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    [Serializable]
    public class InitialConditionCell
    {
        public int X { get; set; }
        public int Value { get; set; }
        public int Set { get; internal set; }
        public int Y { get; set; }

        public InitialConditionCell(int X, int Y, int Value, int Set)
        {
            this.X = X;
            this.Y = Y;
            this.Set = Set;
            this.Value = Value;
        }

        public InitialConditionCell GetCopy()
        {
            return new InitialConditionCell(X,Y,Value,Set);
        }
    }
    [Serializable]
     public class InitialConditionsGrid 
    {
        internal static InitialConditionsGrid GenerateRandom(Random r,int size=100)
        {
            int x = size;
            int y = size;
            List<InitialConditionCell>[] sets = new List<InitialConditionCell>[(int)WhenBetray.Never+1];
            for(int i=0;i<sets.Length;i++) sets[i]=new List<InitialConditionCell>();
            InitialConditionCell[,] Grid = new InitialConditionCell[x,y];
            for(int i=0;i<x;i++)
                for (int j = 0; j < y; j++)
                {
                    int k = r.Next(sets.Length);
                    InitialConditionCell c = new InitialConditionCell(i,j,k,k);
                    Grid[i, j] = c;
                    sets[k].Add(c);

                }
            var Sets = sets.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = Grid,
                CellSets = Sets
            };
            return ig;
        }
        public InitialConditionCell[][] CellSets;
        public InitialConditionCell[,] CellGrid;
        public void ChangeValue(int X, int Y, int Value)
        {
            CellGrid[X,Y].Value = Value;
        }

        public void Fill(int X, int Y, int Value)
        {
            int index = CellGrid[X,Y].Set;
            Fill(index,Value);
        }

        public void Fill(int index, int Value)
        {
            for (int i = 0; i < CellSets[index].Length; i++)
            {
                CellSets[index][i].Value = Value;
            } 
        }

        internal static InitialConditionsGrid DonutFactory(int size=30)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size,size];
            List<InitialConditionCell>[] sets = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
            for (int i = 0; i < sets.Length; i++) sets[i] = new List<InitialConditionCell>();
            for(int i=0;i<size;i++)
                for (int j = 0; j < size; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i,j,-1,0);
                    for (int k = 9; k >= 0; k--)
                    {


                        if ((i - size/2)*(i - size/2) + (j - size/2)*(j - size/2) > (size*(size/4))/(k+1))
                        {
                            c.Set = k;
                        }
                    }
                    if (c.Set < 0) c.Set = 9;
                    ic[i, j] = c;
                    sets[c.Set].Add(c);
                }
            var Sets = sets.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = Sets
            };
            return ig;
        }
        internal static InitialConditionsGrid CrossFactory()
        {
            InitialConditionCell[,] ic = new InitialConditionCell[30, 30];
            List<InitialConditionCell>[] sets = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
            for (int i = 0; i < sets.Length; i++) sets[i] = new List<InitialConditionCell>();
            for(int i=0;i<30; i++)
                for (int j = 0; j < 30; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, -1);
                    if (i < 10 && j < 10) c.Set = (i + j) / 2;
                    if ((i < 10 && j > 20) || (i > 20 && j < 10)) c.Set = (i + j - 10) / 2;
                    if (i > 20 && j > 20) c.Set = (i + j - 20) / 2;
                    ic[i, j] = c;
                    sets[c.Set].Add(c);
                }
            var Sets = sets.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = Sets
            };
            return ig;
        }
        internal static InitialConditionsGrid CircleFactory(int size=30)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] sets = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
            for (int i = 0; i < sets.Length; i++) sets[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, -1);
                    for (int k = 0;k<9;k++)
                    {

                        
                        if ((i - size/2) * (i - size/2) + (j - size/2) * (j - size/2) <= (size/2)*(size/2) / ((k+1)*(k + 1)))
                        {
                            c.Set = k;
                        }
                    }
                    if (c.Set < 0)
                    {
                        c.Set = 9;
                    }
                    ic[i, j] = c;
                    sets[c.Set].Add(c);
                }
            var Sets = sets.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = Sets
            };
            return ig;
        }

        internal InitialConditionsGrid GetCopy()
        {
            InitialConditionCell[][] Set;
            List<InitialConditionCell[]> list = new List<InitialConditionCell[]>();
            InitialConditionCell[,] Grid = new InitialConditionCell[CellGrid.GetLength(0), CellGrid.GetLength(1)];
            for (int i = 0; i < CellSets.Length;i++)
            {
                List<InitialConditionCell> L = new List<InitialConditionCell>();
                for (int j = 0; j < CellSets[i].Length; j++)
                {
                    var C = CellSets[i][j].GetCopy();
                    Grid[C.X, C.Y] = C;
                    L.Add(C);

                }
                list.Add(L.ToArray());
            }
            Set = list.ToArray();
            return new InitialConditionsGrid()
            {
                CellGrid = Grid,
                CellSets = Set
            };

        }

        internal static InitialConditionsGrid DiagonalFactory(int size=30)
        {
            InitialConditionCell[,] ic = new InitialConditionCell[size, size];
            List<InitialConditionCell>[] sets = new List<InitialConditionCell>[(int)WhenBetray.Never + 1];
            for (int i = 0; i < sets.Length; i++) sets[i] = new List<InitialConditionCell>();
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                {
                    int k = Math.Abs(i - j);
                    int m = Math.Max(ic.GetLength(0), ic.GetLength(1));
                    InitialConditionCell c = new InitialConditionCell(i, j, 0, k * 9 / m);
                   
                    
                    ic[i, j] = c;
                    sets[c.Set].Add(c);
                }
            var Sets = sets.Select(a => a.ToArray()).ToArray();
            InitialConditionsGrid ig = new InitialConditionsGrid
            {
                CellGrid = ic,
                CellSets = Sets
            };
            return ig;
        }

        public static InitialConditionsGrid FromCellArray(Cell[,] cells)
        {
            InitialConditionCell[,] arr = new InitialConditionCell[cells.GetLength(0),cells.GetLength(1)];
            List<
           List<InitialConditionCell>> List;
            List = new List<List<InitialConditionCell>>();
            for(int i=0;i<(Enum.GetValues(typeof(WhenBetray))).Length;i++) List.Add(new List<InitialConditionCell>());
            for(int i=0;i<cells.GetLength(0);i++)
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    int k = (cells[i, j].Strategy as IntegerStrategy).Treshold;
                    arr[i,j] = new InitialConditionCell(i,j,k,k);
                    List[k].Add(arr[i, j]);
                }
            var arr2 = List.Select(l => l.ToArray()).ToArray();
            return new InitialConditionsGrid() {CellGrid = arr, CellSets = arr2};
        }
    }

    [Serializable]
    public class InitialConditions
    {
        public InitialConditionsGrid grid;
        public String Name;

        internal static InitialConditions GenerateRandom(int size=100)
        {
            Random r = new Random();
            var ic = new InitialConditions
            {
                Name = "Losowy" + r.Next().ToString(),
                grid = InitialConditionsGrid.GenerateRandom(r,size)
            };
            return ic;
        }
        internal static InitialConditions CircleFactory(bool reversed=false,int size=30)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.CircleFactory(size);
            if (reversed)
            {
                for (int i = 0; i < 10; i++) ig.Fill(i, i);
            }
            else
            {
                for (int i = 0; i < 10; i++) ig.Fill(i, 9 - i);
            }
            var ic = new InitialConditions
            {
                Name = "Koło " + (reversed ? "- odwrócone kolory" : ""),
                grid = ig
            };
            return ic;
        }
        internal static InitialConditions DonutFactory(bool reversed=false,int size=30)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.DonutFactory(size);
            if (reversed)
            {
                for(int i=0;i<10;i++) ig.Fill(i,i);
            }
            else
            {
                for (int i = 0; i < 10; i++) ig.Fill(i,9 - i);
            }
            var ic = new InitialConditions
            {
                Name = "Donut " + (reversed ? "- odwrócone kolory" : ""),
                grid = ig
            };
            return ic;
        }
        internal static InitialConditions CrossFactory(bool reversed=false,int size=30)
        {
            throw new NotImplementedException();
        }

        internal static InitialConditions DiagonalFactory(bool reversed=false,int size=30)
        {
            InitialConditionsGrid ig = InitialConditionsGrid.DiagonalFactory(size);
            if (reversed)
            {
                for (int i = 0; i < 10; i++) ig.Fill(i, i);
            }
            else
            {
                for (int i = 0; i < 10; i++) ig.Fill(i, 9 - i);
            }
            var ic = new InitialConditions
            {
                Name = "Przekątna " + (reversed ? "- odwrócone kolory" : ""),
                grid = ig
            };
            return ic; 
        }

        internal InitialConditions GetCopy()
        {
            return new InitialConditions() {Name = this.Name, grid = this.grid.GetCopy()};

        }

        public static InitialConditions FromCellArray(Cell[,] cells, string getFileName)
        {
            InitialConditionsGrid ICG = InitialConditionsGrid.FromCellArray(cells);
            
            return new InitialConditions(){Name = getFileName,grid = ICG};
        }
    }
}
