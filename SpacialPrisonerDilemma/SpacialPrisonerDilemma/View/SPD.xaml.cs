using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
    public partial class SPD : INotifyPropertyChanged
    {
        private readonly Model.SPD _spd;
        private readonly int _width;
        private readonly int _height;
        readonly int[,] _strategies;
        private int _iter;
        private int _speed;

        private PlotModel _pointsmodel;
        private PlotModel _countmodel;

        public PlotModel CountModel
        {
            get { return _countmodel; }
            set { _countmodel = value; OnPropertyChanged(); }
        }
        public PlotModel PointsModel
        {
            get
            {

                return _pointsmodel;
            }
            set
            {
                _pointsmodel = value;
                OnPropertyChanged();
            }
        }

        public PlotModel ChangeModel
        {
            get
            {
                return _changemodel;
            }
            set
            {
                _changemodel = value;

                OnPropertyChanged();
            }
        }
        public int Iteration
        {
            get { return _iter; }
            set { _iter = value; OnPropertyChanged(); UpdateImage(); }
        }
        public int Speed
        {
            get { return _speed; }

            set { _speed = value; OnPropertyChanged(); }
        }


        private readonly List<Tuple<int, String>> _iterations;


        private List<ColumnItem> GenerateColumns(int category, double[] values)
        {
            return values.Select((t, i) => new ColumnItem(t, category) { Color = SPDAssets.GetOxyColor(i) }).ToList();
        }
        /// <summary>
        /// Metoda aktualizująca wykresy
        /// </summary>

        private void UpdateModels()
        {
            var category = _iterations.Count;

            var d = CalculateModels(_spd.GetStateByIteration(category));


            _iterations.Add(new Tuple<int, String>(category, ""));

            foreach (var s in GenerateColumns(category, d[0]))
            {
                var columnSeries = CountModel.Series[0] as ColumnSeries;
                if (columnSeries != null) columnSeries.Items.Add(s);
            }
            if (category > 0)
            {
                foreach (var s in GenerateColumns(category - 1, d[1]))
                {
                    var columnSeries = PointsModel.Series[0] as ColumnSeries;
                    if (columnSeries != null)
                        columnSeries.Items.Add(s);
                }
            }

            CountModel = CountModel;
            PointsModel = PointsModel;

            CountModel.InvalidatePlot(true);
            PointsModel.InvalidatePlot(true);

        }
        /// <summary>
        /// Metoda wyliczająca dane do wykresów na podstawie stanu automatu
        /// </summary>

        double[][] CalculateModels(Cell[,] cells)
        {
            var result = new double[Enum.GetValues(typeof(WhenBetray)).Length];
            var count = new double[result.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = 0;
                count[i] = 0;
            }
            for (var i = 0; i < cells.GetLength(0); i++)
                for (var j = 0; j < cells.GetLength(1); j++)
                {
                    var c = cells[i, j];
                    var integerStrategy = c.Strategy as IntegerStrategy;
                    if (integerStrategy != null)
                    {
                        var k = integerStrategy.Treshold;
                        double d = c.Points / _spd[i, j].GetNeighbours().Length;
                        result[k] += d;
                        count[k]++;
                    }
                }
            for (var i = 0; i < count.Length; i++)
            {
                if ((count[i] - Double.Epsilon) <= 0) continue;
                result[i] /= count[i];


            }
            var d1 = result.Sum();
            if (Math.Abs(d1) < double.Epsilon*100) d1 = 1;
            var d2 = count.Sum();
            if (Math.Abs(d2) < double.Epsilon * 100) d2 = 1;
            result = result.Select(d => 100 * d / d1).ToArray();
            count = count.Select(d => 100 * d / d2).ToArray();
            return new[] { count, result };
        }
        /// <summary>
        /// Konstruktor okna realizującego symulacje
        /// </summary>
        /// <param name="PayValues">Tablica zawierająca informacje wyekstrahowane z macierzy wypłat </param>
        /// <param name="strategies">Tablica zawierająca początkowe strategie w automacie</param>
        /// <param name="torus">Zmienna informująca czy obliczenia automatu realizowane są na torusie</param>
        /// <param name="vonneumann">Zmienna informująca czy obliczenia automatu realizowane są z sąsiedztwem Von Neumanna</param>
        public SPD(double[] PayValues, int[,] strategies, bool torus, bool vonneumann)
        {
            DataContext = this;
            var payValues = PayValues;
            _strategies = strategies;
            _spd = Model.SPD.Singleton;
            Speed = 1;
            PointsModel = new PlotModel();
            CountModel = new PlotModel();
            ChangeModel = new PlotModel();
            PointsModel.Title = "Średnie wartości punktowe";
            CountModel.Title = "Liczebność strategii";
            ChangeModel.Title = "Niestabilność układu";
            _iterations = new List<Tuple<int, string>>();

            PointsModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });

            CountModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });
            PointsModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });
            CountModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });

            PointsModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });
            CountModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });

            ChangeModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });
            ChangeModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });
            ChangeModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });
            Model.SPD.Initialize(_strategies, 10, (float)payValues[3], (float)payValues[2], (float)payValues[1], (float)payValues[0], !vonneumann, torus);

            UpdateModels();

            InitializeComponent();
            Iteration = 0;



            var image = new Image { Source = SPDAssets.GenerateLegend(Legenda.Height), Width = Legenda.Width, Height = Canvas.Height };
            _width = _strategies.GetLength(0);
            _height = _strategies.GetLength(1);
            var image2 = new Image
            {
                Source = GenerateImage(_spd, 0, 0, _strategies.GetLength(0), _strategies.GetLength(1))

            };


            Canvas.SetTop(image, 0);
            Canvas.SetLeft(image, 0);
            Legenda.Children.Add(image);
            Canvas.Children.Add(image2);
        }
        /// <summary>
        /// Obsługa funkcjonalności zoom
        /// </summary>

        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {

            var p = e.GetPosition(Canvas);
            var X = _x + _scale * p.X / (Canvas.Width / (_strategies.GetLength(0)));
            if (X >= _strategies.GetLength(0)) return;
            var Y = _y + _scale * p.Y / (Canvas.Height / (_strategies.GetLength(1)));
            if (Y >= _strategies.GetLength(1)) return;
            _scale += Math.Sign(-e.Delta) * 0.1;
            if (_scale < 0.1) _scale = 0.1;
            if (_scale > 1) _scale = 1;

            var width = _strategies.GetLength(0);
            var height = _strategies.GetLength(1);
            var nwidth = (int)(width * _scale);
            var nheight = (int)(height * _scale);
            var x = (int)X - (nwidth / 2);
            var y = (int)Y
                    - (nheight / 2);
            var xx = (int)X + (nwidth / 2);
            var yy = (int)Y + (nheight / 2);

            if (xx >= width) x -= (xx - width) + 1;
            if (yy >= height) y -= (yy - height) + 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            _x = x;
            _y = y;
            UpdateImage();
        }

        /// <summary>
        /// Metoda rysująca wykres automatu bądź jego fragment.
        /// </summary>
        /// <param name="spd">Odwołanie do instancji SPD</param>
        /// <param name="x">Współrzedna X lewej górnej komórki wykresu do wyświetlenia</param>
        /// <param name="y">Współrzędna Y lewej górnej komórki wykresu do wyświetlenia</param>
        /// <param name="width">Ile kolumn automatu będzie wyrysowanych</param>
        /// <param name="height">Ile wierszy automatu będzie wyrysowanych</param>
        /// <returns>Wyrysowany wykres</returns>
        private DrawingImage GenerateImage(Model.SPD spd, int x, int y, int width, int height)
        {
            var cellWidth = Canvas.Width / width;
            var cellHeight = Canvas.Height / height;
            

            var dg = new DrawingGroup();
            for (var i = x; i < x + width; i++)
            {
                for (var j = y; j < y + height; j++)
                {
                    var rg = new RectangleGeometry(new Rect(new Point((i - x) * cellWidth, (j - y) * cellHeight), new Point((i - x + 1) * cellWidth, (j + 1 - y) * cellHeight)));
                    var gd = new GeometryDrawing
                    {
                        Brush =
                            GetBrush(((IntegerStrategy) spd.GetStateByIteration(Iteration)[i, j].Strategy).Treshold),
                        Geometry = rg
                    };
                    dg.Children.Add(gd);
                }
            }
            return new DrawingImage(dg);
        }

        private volatile bool _cont;

        public int StateCount
        {
            get { return _spd.CurrentIteration > 0 ? _spd.CurrentIteration - 1 : 0; }
        }

        readonly int _delay = 16;

        Task<Tuple<int, bool>> _iteration;
        private bool _over = true;
        private async Task SPDLooper()
        {
            while (_cont && _over)
            {

                _iteration = Task.Run(async () => await _spd.IterateAsync());
                await Task.WhenAll(_iteration, Task.Delay((60 / Speed) * _delay));
                var i = await _iteration;
                if (_over) UpdateImage();
                if (_over) OnPropertyChanged("StateCount");
                if (_over) if (Iteration == StateCount - 1) Iteration++;
                if (_over) await Task.WhenAll(_iteration, Task.Delay((60 / Speed) * _delay));
                if (_over) InsertVariationColumn(i.Item1);
                if (_over) UpdateModels();


            }

        }

        private void InsertVariationColumn(int x)
        {
            var i = _iterations.Count - 1;
            var columnSeries = ChangeModel.Series[0] as ColumnSeries;
            if (columnSeries != null)
                columnSeries.Items.Add(new ColumnItem(x, i));
            ChangeModel.InvalidatePlot(true);

        }
        private async Task StartSPD()
        {
            await SPDLooper();
        }

        private int _x, _y;
        private double _scale = 1;
        private void UpdateImage()
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image
            {
                Source = GenerateImage(_spd, _x, _y, (int)(_width * _scale), (int)(_height * _scale))
            });
        }

        private PlotModel _changemodel;

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _cont = false;
            _over = false;
            if (_iteration != null) _iteration.Wait();
            if (!PerformanceCheck.IsChecked.HasValue || !PerformanceCheck.IsChecked.Value) return;
            var pl = Model.SPD.ClearAndGetLog();
            if (pl.StepTimes.Length == 0) return;
            MessageBox.Show(string.Format("Mediana: {0} Średnia: {1} Max Step Time: {2} Min Step Time: {3}",
                       pl.Median, pl.Average, pl.MaxStepTime, pl.MinStepTime));
        }

        private Brush GetBrush(int p)
        {
            return SPDAssets.GetBrush(p);
        }
        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            _cont = !_cont;
            if (_cont)
            {

                StartStop.Content = "Stop";
                await StartSPD();
            }
            else StartStop.Content = "Start";

           
        }

        public event PropertyChangedEventHandler PropertyChanged;
       

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public static void SaveImageToFile(string filePath, Image b)
        {
            var stream = new FileStream(filePath, FileMode.Create);

            var vis = new DrawingVisual();
            var cont = vis.RenderOpen();
            cont.DrawImage(b.Source, new Rect(new Size(b.ActualWidth, b.ActualHeight)));
            cont.Close();

            var rtb = new RenderTargetBitmap((int)b.ActualWidth,
                (int)b.ActualHeight, 96d, 96d, PixelFormats.Default);
            rtb.Render(vis);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            encoder.Save(stream);
            stream.Close();
        }
        private void ImageSave_OnClick(object sender, RoutedEventArgs e)
        {
            var b = Canvas.Children[0] as Image;
            var sfd = new SaveFileDialog { Filter = "PNG Image (*.png)|*.png" };
            var v = sfd.ShowDialog();
            if (v.HasValue && v.Value)
            {
                SaveImageToFile(sfd.FileName, b);
            }
        }

        private void CicSave_OnClick(object sender, RoutedEventArgs e)
        {
            var sfd = new SaveFileDialog { Filter = "Initial Condition (*.cic)|*.cic" };
            var v = sfd.ShowDialog();
            if (!v.HasValue || !v.Value) return;
            var c = _spd.GetStateByIteration(_iter);
            var ifc = InitialConditions.FromCellArray(c, Path.GetFileName(sfd.FileName));
            var bf = new BinaryFormatter();
            var fs = new FileStream(sfd.FileName, FileMode.Create);
            bf.Serialize(fs, ifc);
            fs.Close();
        }
    }
}
