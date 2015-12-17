using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SpacialPrisonerDilemma.Annotations;
using SpacialPrisonerDilemma.Model;
using FontWeights = System.Windows.FontWeights;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for SPD.xaml
    /// </summary>
    public partial class SPD : Window, INotifyPropertyChanged
    {
        private Model.SPD spd;
        private int _width, _height;
        double[] payValues;
        int[,] strategies;
        private int iter;
        private int speed;
        private List<Cell[,]> History;
        PlotModel pointsmodel;
        private PlotModel countmodel;

        public PlotModel CountModel
        {
            get { return countmodel; }
            set { countmodel = value;OnPropertyChanged(); }
        }
        public PlotModel PointsModel
        {
            get
            {
                
                return pointsmodel;
            }
            set
            {
                pointsmodel = value;
                OnPropertyChanged();
            }
        }

        public PlotModel ChangeModel

        {
            get
            {


                return changemodel;
            }
            set
            {
                changemodel = value;

                OnPropertyChanged();
            }
        }
        public int Iteration
        {
            get { return iter; }
            set { iter = value; OnPropertyChanged(); UpdateImage(); }
        }
        public int Speed
        {
            get { return speed; }

            set { speed = value; OnPropertyChanged();  }
        }


        private List<Tuple<int, String>> Iterations;
        
        private List<ColumnItem> GenerateColumns(int category, double[] values)
        {
            return values.Select((t, i) => new ColumnItem(t, category) {Color = SPDBrushes.GetOxyColor(i)}).ToList();
        }
        private void ResetModels(int x=0)
        {
            int category = Iterations.Count;
            
            var d = CalculateModels(spd.GetStateByIteration(category));
         

            Iterations.Add(new Tuple<int,String>(category,""));

            foreach (var S in GenerateColumns(category,d[0]))
            {
                (CountModel.Series[0] as ColumnSeries).Items.Add(S);
          

            }
            if (category > 0)
            {
                foreach (var S in GenerateColumns(category-1, d[1]))
                {
                    (PointsModel.Series[0] as ColumnSeries).Items.Add(S);
                }
            }
          
            CountModel = CountModel;
            PointsModel = PointsModel;
           
            CountModel.InvalidatePlot(true);
            PointsModel.InvalidatePlot(true);
           
        }
        double[][] CalculateModels(Cell[,] cells)
        {
            double[] result = new double[Enum.GetValues(typeof(WhenBetray)).Length];
            double[] count = new double[result.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = 0;
                count[i] = 0;
            }
            for(int i=0;i<cells.GetLength(0);i++)
                for (int j = 0; j < cells.GetLength(1); j++)
                {
                    Cell C = cells[i, j];
                    int k = (C.Strategy as IntegerStrategy).Treshold;
                  double d =  C.Points/spd[i,j].GetNeighbours().Length;
                    result[k] += d;
                    count[k]++;
                }
            for (int i = 0; i < count.Length; i++)
            {
                if ((count[i] - Double.Epsilon) <= 0) continue;
                result[i] /= count[i];
               

            }
            double d1 = result.Sum();
            if (d1 == 0) d1 = 1;
            double d2 = count.Sum();
            if (d2 == 0) d2 = 1;
            result = result.Select(d => 100*d/d1).ToArray();
            count = count.Select(d => 100*d/d2).ToArray();
            return new[] {count, result};
        }
        public SPD(double[] PayValues,int[,] Strategies,bool torus,bool vonneumann)
        {
            DataContext = this;
            payValues = PayValues;
            strategies = Strategies;
            spd = Model.SPD.Singleton;
            Speed = 1;
            PointsModel = new PlotModel();
            CountModel = new PlotModel();
            ChangeModel = new PlotModel();
            PointsModel.Title = "Średnie wartości punktowe";
            CountModel.Title = "Liczebność strategii";
            ChangeModel.Title = "Niestabilność układu";
            Iterations = new List<Tuple<int, string>>();
           
           PointsModel.Axes.Add(new CategoryAxis(){ItemsSource=Iterations,LabelField = "Item2"});
           
           CountModel.Axes.Add(new CategoryAxis() { ItemsSource = Iterations, LabelField = "Item2" });
            PointsModel.Axes.Add(new LinearAxis(){MinimumPadding=0, AbsoluteMinimum = 0});
            CountModel.Axes.Add(new LinearAxis(){MinimumPadding = 0,AbsoluteMinimum = 0});
           
            PointsModel.Series.Add(new ColumnSeries() {ColumnWidth = 10,IsStacked = true});
            CountModel.Series.Add(new ColumnSeries() { ColumnWidth = 10, IsStacked = true });

            ChangeModel.Axes.Add(new CategoryAxis() {ItemsSource = Iterations, LabelField = "Item2"});
            ChangeModel.Axes.Add(new LinearAxis() { MinimumPadding = 0, AbsoluteMinimum = 0 });
            ChangeModel.Series.Add(new ColumnSeries() { ColumnWidth = 10, IsStacked = true });
            Model.SPD.Initialize(strategies, 10, (float)payValues[3], (float)payValues[2], (float)payValues[1], (float)payValues[0],!vonneumann,torus);

           ResetModels();

            InitializeComponent();
            Iteration = 0;
            
          
          
            var image = new Image() {Source = SPDBrushes.GenerateLegend(Legenda.Height),Width = Legenda.Width,Height = Canvas.Height};
            _width = strategies.GetLength(0);
            _height = strategies.GetLength(1);
            var image2 = new Image()
            {
                Source = GenerateImage(spd, 0, 0, strategies.GetLength(0), strategies.GetLength(1),Iteration)
                
            };
           

            Canvas.SetTop(image, 0);
            Canvas.SetLeft(image, 0);
            Legenda.Children.Add(image);
            Canvas.Children.Add(image2);
        }
        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            
            Point P = e.GetPosition(Canvas);
            double X = this.X + scale * P.X / (Canvas.Width / (strategies.GetLength(0)));
            if (X >= strategies.GetLength(0)) return;
            double Y = this.Y + scale * P.Y / (Canvas.Height / (strategies.GetLength(1)));
            if (Y >= strategies.GetLength(1)) return;
            scale += Math.Sign(-e.Delta) * 0.1;
            if (scale < 0.1) scale = 0.1;
            if (scale > 1) scale = 1;

            int width = strategies.GetLength(0);
            int height = strategies.GetLength(1);
            var nwidth = (int)(((double)width) * scale);
            var nheight = (int)(((double)height) * scale);
            int x = (int)X - (nwidth / 2);
            int y = (int)Y
                    - (nheight / 2);
            int xx = (int)X + (nwidth / 2);
            int yy = (int)Y + (nheight / 2);

            if (xx >= width) x -= (xx - width) + 1;
            if (yy >= height) y -= (yy - height) + 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            this.X = x;
            this.Y = y;
            UpdateImage();
        }
       
        public DrawingImage GenerateImage(Model.SPD spd, int X, int Y, int Width, int Height,int iteration)
        {
            double CellWidth = Canvas.Width / Width;
            double CellHeight = Canvas.Height / Height;
            FontStyle fs = FontStyles.Normal;
            FontWeight fw = FontWeights.Normal;
            FontFamily ff = new FontFamily("Arial");
            FontStretch ffs = FontStretches.Normal;


            DrawingGroup DG = new DrawingGroup();
            for (int i = X; i < X + Width; i++)
            {
                for (int j = Y; j < Y + Height; j++)
                {
                    RectangleGeometry RG = new RectangleGeometry(new Rect(new Point((i-X)*CellWidth,(j-Y)*CellHeight),new Point((i-X+1)*CellWidth,(j+1-Y)*CellHeight)));
                    GeometryDrawing gd = new GeometryDrawing();
                    gd.Brush= GetBrush((spd.GetStateByIteration(Iteration)[i,j].Strategy as IntegerStrategy).Treshold);
                    gd.Geometry = RG;
                    DG.Children.Add(gd);
                }
            }
            return new DrawingImage(DG);
        }

        private volatile bool cont = false;
      
        public int StateCount
        {
            get { return spd.CurrentIteration>0?spd.CurrentIteration-1:0; }
        }
        int delay = 16;

        Task<Tuple<int, bool>> iteration;
        private bool over = true;
        private async Task SPDLooper()
        {
             while (cont && over)
             {
                
                iteration = Task.Run(async () => await spd.IterateAsync());
                //iteration = Task.Run(() => spd.Iterate());

                await Task.WhenAll(new Task[] { iteration, Task.Delay((60/Speed)*delay) });
                //if (await iteration == 0) cont = false;
                 var i = await iteration;
                 if (over)
                 {
                     UpdateImage();
                     OnPropertyChanged("StateCount");
                     if (Iteration == StateCount - 1) Iteration++;
                     await Task.WhenAll(new Task[] {iteration, Task.Delay((60/Speed)*delay)});
                     InsertVariationColumn(i.Item1);
                     ResetModels();
                 }

             }
        }

        private void InsertVariationColumn(int x)
        {
            int i = Iterations.Count - 1;
            (ChangeModel.Series[0] as ColumnSeries).Items.Add(new ColumnItem(x,i));
            ChangeModel.InvalidatePlot(true);
            
        }
        private async Task StartSPD()
        {
            await SPDLooper();
        }

        private int X = 0, Y = 0;
        private double scale = 1;
        private void UpdateImage()
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image()
            {
                Source = GenerateImage(spd, X, Y, (int) (_width*scale), (int) (_height*scale),Iteration)
            });
        }
        private Brush[] BrushArray;
        
        private PlotModel changemodel;

        private void CreateBrushes(int count)
        {
            BrushArray = new Brush[count];
            for (int p = 0; p < count; p++)
            {
                BrushArray[p] = new SolidColorBrush(Color.FromRgb((byte)(255 - 25 * p), (byte)(50 * p>255?255:50*p), (byte)(25 * p)));
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cont = false;
            over = false;
          if (iteration!=null) iteration.Wait();
            Model.SPD.Clear();
        }

        private Brush GetBrush(int p)
        {
            return SPDBrushes.GetBrush(p);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
      
        {
            
                cont = !cont;
            if (cont == true)
            {
              
                StartStop.Content = "Stop";
                await StartSPD();
            }
            else StartStop.Content = "Start";

        //    (PointsModel.Series[0] as ColumnSeries);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private PlotModel variationmodel;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void RangeBase_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            
        }
        public static void SaveImageToFile(string filePath,Image b)
        {
            FileStream stream = new FileStream(filePath, FileMode.Create);

            DrawingVisual vis = new DrawingVisual();
            DrawingContext cont = vis.RenderOpen();
            cont.DrawImage(b.Source, new Rect(new Size(b.ActualWidth,b.ActualHeight)));
            cont.Close();

            RenderTargetBitmap rtb = new RenderTargetBitmap((int)b.ActualWidth,
                (int)b.ActualHeight, 96d, 96d, PixelFormats.Default);
            rtb.Render(vis);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);
            stream.Close();
        }
        private void ImageSave_OnClick(object sender, RoutedEventArgs e)
        {
            Image b = Canvas.Children[0] as Image;
            SaveFileDialog sfd = new SaveFileDialog {Filter = "PNG Image (*.png)|*.png"};
            var v = sfd.ShowDialog();
            if (v.HasValue && v.Value)
            {
                SaveImageToFile(sfd.FileName,b);
            }
        }

        private void CicSave_OnClick(object sender, RoutedEventArgs e)
        {
            int Iter = iter;
            SaveFileDialog sfd = new SaveFileDialog { Filter = "Initial Condition (*.cic)|*.cic" };
            var v = sfd.ShowDialog();
            if (v.HasValue && v.Value)
            {
                Cell[,] C = spd.GetStateByIteration(iter);
                InitialConditions ifc = InitialConditions.FromCellArray(C, System.IO.Path.GetFileName(sfd.FileName));
                BinaryFormatter bf = new BinaryFormatter();
                FileStream fs = new FileStream(sfd.FileName,FileMode.Create);
                bf.Serialize(fs,ifc);
                fs.Close();
            }
        }
    }

    internal static class SPDBrushes
    {
        private static  Brush[] BrushArray;
        private static OxyColor[] OxyArray;

        public static List<Image> GetBrushRectangles()
        {
            return BrushArray.Select(s =>
            {
                RectangleGeometry rg = new RectangleGeometry(new Rect(new Point(0, 0), new Point(30, 15)));
                GeometryDrawing gd = new GeometryDrawing(s, new Pen(s, 1), rg);
                DrawingImage di = new DrawingImage(gd);
                return new Image() {Source = di};
            }).ToList();
        }
        public static OxyColor GetOxyColor(int p)
        {
            return OxyArray[p];
        }
        public static void CreateBrushes(int count)
        {
            
            BrushArray = new Brush[count];
            OxyArray = new OxyColor[count];
            for (int p = 0; p < count; p++)
            {
                BrushArray[p] = new SolidColorBrush(Color.FromRgb((byte)(256 - 15 * p>255?0:255 - 10*p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p)));
                OxyArray[p] = OxyColor.FromRgb((byte)(256 - 15 * p > 255 ? 0 : 255 - 10 * p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p));
            }
        }

        public static void ModifyColor(Brush b,OxyColor o,int i)
        {
            BrushArray[i] = b;
            OxyArray[i] = o;
        }
        private static string Font;
        public static void ChangeFont(string FontName)
        {
            Font = FontName;
        }

        public static string GetFont()
        {
            return Font;
        }
        public static Brush GetBrush(int p)
        {
                return BrushArray[p];
           
            
        }
        private static string[] Descriptions;

        public static void InitialiseDescriptions()
        {
            List<string> des = new List<string>();
            des.Add("Zawsze zdradzaj");
            for (int i = 1; i < 9; i++)
            {
                des.Add(string.Format("Zdradzaj gdy {0} sąsiad{1} zdradza", i, i == 1 ? "" : "ów"));
            }
            des.Add("Zawsze wybaczaj");
            Descriptions = des.ToArray();
        }
       
        public static DrawingImage GenerateLegend(double Height)
        {

            DrawingGroup DG = new DrawingGroup();
            for (int i = 0; i < Descriptions.Length; i++)
            {
                RectangleGeometry RG = new RectangleGeometry(new Rect(new Point(0, i * (Height / Descriptions.Length)), new Point(20, (i + 1) * (Height / Descriptions.Length))));
                FormattedText text = new FormattedText(Descriptions[i],
        CultureInfo.CurrentCulture,
        FlowDirection.LeftToRight,
        new Typeface(Font),
        15,
        Brushes.Black);
                GeometryDrawing gd = new GeometryDrawing();
                gd.Brush = GetBrush(i);
                gd.Geometry = RG;
                gd.Pen = new Pen(Brushes.Black, 2);
                GeometryDrawing gd2 = new GeometryDrawing();
                gd2.Pen = new Pen(Brushes.Black, 2);
                gd2.Brush = Brushes.Black;
                gd2.Geometry = text.BuildGeometry(new Point(22, i * (Height / Descriptions.Length)));
                DG.Children.Add(gd);
                DG.Children.Add(gd2);
            }
            var D = new DrawingImage(DG);
            D.Freeze();
            return D;
        }
    }
}
