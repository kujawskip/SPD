using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SPD.Engine;

namespace SpacialPrisonerDilemma.View
{
    public class PointMatrixPick
    {
        public List<PointMatrix> Matrices;
        public int[,] Indices;
        public bool ModifiedPointCounting;
        public System.Func<Coord, PointMatrix> Function
        {
            get { return x => (Matrices[Indices[x.X, x.Y]]); }
        }

        public static PointMatrixPick SingularMatrixCondition(PointMatrix M, int size1,int size2)
        {
            PointMatrixPick p = new PointMatrixPick
            {
                Indices = new int[size1, size2],
                Matrices = new List<PointMatrix> {M}
            };
            for (int i = 0; i < size1; i++) for (int j = 0; j < size2; j++) p.Indices[i, j] = 0;
            return p;
        }

        public int Size
        {
            get { return Indices.GetLength(0); }
        }

        

        private static readonly Random random = new Random();
        public PointMatrixPick Resize(int Size)
        {
            PointMatrixPick result = new PointMatrixPick
            {
                Matrices = Matrices.ToList(),
                Indices = new int[Size, Size]
            };

            double d = (double)Size / this.Size;
            for (int i = 0; i < Size; i++)
            {
                double index = i / d;
                int k = (int)Math.Floor(index);
                int l = k + 1;
                double prob = 1 - (index - k);
                int I = random.NextDouble() < prob ? k : l;
                for (int j = 0; j < Size; j++)
                {
                    index = i / d;
                    k = (int)Math.Floor(index);
                    l = k + 1;
                    prob = 1 - (index - k);
                    int J = random.NextDouble() < prob ? k : l;
                    result.Indices[i, j] = Indices[I, J];
                }
            }
            return result;
        }
        public static PointMatrixPick CreatePickFromIC(InitialConditions IC,PointMatrixPick _pick)
        {
            PointMatrixPick Condition=new PointMatrixPick();
            Condition.Matrices = _pick.Matrices.ToList();
            Condition.Indices = new int[IC.Grid.CellGrid.GetLength(0), IC.Grid.CellGrid.GetLength(1)];
            for (int i = 0; i < IC.Grid.CellGrid.GetLength(0); i++)
            {
                for (int j = 0; j < IC.Grid.CellGrid.GetLength(1); j++)
                {
                    Condition.Indices[i, j] = IC.Grid.CellGrid[i, j].Value >= Condition.Matrices.Count ? Condition.Matrices.Count - 1 : IC.Grid.CellGrid[i, j].Value;
                }
            }
            return Condition;
        }

        public DrawingImage GenerateImage(int x,int y,int width,int height,double CanvasWidth,double CanvasHeight)
        {
            var IC = CreateICFromPick(this);
            var di = IC.Grid.GenerateImage(x, y, width, height, CanvasWidth, CanvasHeight);
            DrawingGroup dg = di.Drawing as DrawingGroup;
            if (dg == null) return di;
            int h = 20;
            for (int i = 0; i < Matrices.Count; i++)
            {
                RectangleGeometry rg =
                    new RectangleGeometry(new Rect(new Point(0, 10 + CanvasHeight + h*i),
                        new Point(10, 10 + CanvasHeight + h*(i + 1))));
                var gd = new GeometryDrawing
                {
                    Brush = SPDAssets.GetBrush(i),
                    Geometry = rg,
                    Pen = new Pen(Brushes.Black, 0.5)
                };
                var text = new FormattedText(Matrices[i].ToString(),
                   CultureInfo.CurrentCulture,
                   FlowDirection.LeftToRight,
                   new Typeface(SPDAssets.GetFont()),
                   10,
                   Brushes.Black);
                var gd2 = new GeometryDrawing
                {
                    Pen = new Pen(Brushes.Black, 1),
                    Brush = Brushes.Black,
                    Geometry = text.BuildGeometry(new Point(11, 10 + (h/2) + CanvasHeight + h * i))
                };
                dg.Children.Add(gd);
                dg.Children.Add(gd2);
            }
            return new DrawingImage(dg);
        }
        public static InitialConditions CreateICFromPick(PointMatrixPick Condition)
        {
            var IC = new InitialConditions();
            var IG = new InitialConditionsGrid();
            IG.CellGrid = new InitialConditionCell[Condition.Indices.GetLength(0), Condition.Indices.GetLength(1)];
            for (int i = 0; i < Condition.Indices.GetLength(0); i++)
            {
                for (int j = 0; j < Condition.Indices.GetLength(1); j++)
                {
                    IG.CellGrid[i, j] = new InitialConditionCell(i, j, Condition.Indices[i, j], Condition.Indices[i, j]);
                }
            }
            IC.Grid = IG;
            return IC;
        }
    }
}
