using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for SPD.xaml
    /// </summary>
    public partial class SPD : Window
    {
        private Model.SPD spd;
        private int _width, _height;
        double[] payValues;
        int[,] strategies;

        public SPD(double[] PayValues,int[,] Strategies)
        {
            payValues = PayValues;
            strategies = Strategies;
            InitializeComponent();
           
            spd = Model.SPD.Singleton;
            Model.SPD.Initialize(strategies, 10, (float)payValues[3], (float)payValues[2], (float)payValues[1], (float)payValues[0]);
            var image = new Image() {Source = SPDBrushes.GenerateLegend(Legenda.Height),Width = Legenda.Width,Height = Canvas.Height};
            _width = strategies.GetLength(0);
            _height = strategies.GetLength(1);
            var image2 = new Image()
            {
                Source = GenerateImage(spd, 0, 0, strategies.GetLength(0), strategies.GetLength(1))
            };
            Canvas.SetTop(image, 0);
            Canvas.SetLeft(image, 0);
            Legenda.Children.Add(image);
            Canvas.Children.Add(image2);
        }

       
        public DrawingImage GenerateImage(Model.SPD spd, int X, int Y, int Width, int Height)
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
                    gd.Brush= GetBrush((spd[i,j].Strategy as IntegerStrategy).Treshold);
                    gd.Geometry = RG;
                    DG.Children.Add(gd);
                }
            }
            return new DrawingImage(DG);
        }

        private volatile bool cont = false;

        int delay = 1000;
        Task<int> iteration;
        private async Task SPDLooper()
        {
             while (cont)
             {
                iteration = Task.Run(async () => await spd.IterateAsync());
                if (await iteration == 0) cont = false;
                    UpdateImage();
                //await Task.Delay(delay);
             }
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
                Source = GenerateImage(spd, X, Y, (int) (_width*scale), (int) (_height*scale))
            });
        }
        private Brush[] BrushArray;

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
                await StartSPD();
                StartStop.Content = "Stop";
            }
            else StartStop.Content = "Start";
        }
    }

    internal static class SPDBrushes
    {
        private static  Brush[] BrushArray;

        public static void CreateBrushes(int count)
        {
            
            BrushArray = new Brush[count];
            for (int p = 0; p < count; p++)
            {
                BrushArray[p] = new SolidColorBrush(Color.FromRgb((byte)(256 - 15 * p>255?0:255 - 10*p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p)));
            }
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
        new Typeface("Arial"),
        16,
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
