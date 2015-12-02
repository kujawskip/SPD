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
        public int Set { get; private set; }
        public int Y { get; set; }

        public InitialConditionCell(int X, int Y, int Value, int Set)
        {
            this.X = X;
            this.Y = Y;
            this.Set = Set;
            this.Value = Value;
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
            for (int i = 0; i < CellSets[index].Length; i++)
            {
                CellSets[index][i].Value = Value;
            }
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
                Name = "Random" + r.Next().ToString(),
                grid = InitialConditionsGrid.GenerateRandom(r,size)
            };
            return ic;
        }
    }
}
