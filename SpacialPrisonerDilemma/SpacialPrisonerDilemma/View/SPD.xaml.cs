using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using SpacialPrisonerDilemma.Annotations;
using SpacialPrisonerDilemma.Engine;
using SpacialPrisonerDilemma.Engine.Neighbourhoods;
using SpacialPrisonerDilemma.Model;
using CategoryAxis = OxyPlot.Axes.CategoryAxis;
using ColumnSeries = OxyPlot.Series.ColumnSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;


namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for SPD.xaml
    /// </summary>
    public partial class SPD : INotifyPropertyChanged
    {
        private readonly Engine.SPD _spd;
        private readonly int _width;
        private readonly int _height;
        readonly int[,] _strategies;
        private int _iter;
        private int _speed;

        private PlotModel _pointsmodel;
        private PlotModel _countmodel;
        /// <summary>
        /// Model liczebności strategii
        /// </summary>
        public PlotModel CountModel
        {
            get { return _countmodel; }
            set { _countmodel = value; OnPropertyChanged(); }
        }
        /// <summary>
        /// Model wyników punktowych
        /// </summary>
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
        /// <summary>
        /// Model wyników sum punktów
        /// </summary>
        public PlotModel SumModel
        {
            get
            {

                return _summodel;
            }
            set
            {
                _summodel = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Model wariancji układu
        /// </summary>
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
        /// <summary>
        /// Aktualna iteracja
        /// </summary>
        public int Iteration
        {
            get { return _iter; }
            set { _iter = value; OnPropertyChanged(); UpdateImage(); }
        }
        /// <summary>
        /// Szybkość wyświetlania
        /// </summary>
        public int Speed
        {
            get { return _speed; }

            set { _speed = value; OnPropertyChanged(); }
        }


        private readonly List<Tuple<int, String>> _iterations;

        private void SavePlot(OxyPlot.PlotModel PlotKey)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "PNG|*.png",
                FileName = DateTime.Now.ToString("yy-MM-dd")+ DateTime.Now.GetHashCode() + ".png"
            };
            var b = sfd.ShowDialog();
            if (!b.HasValue || !b.Value) return;
            using (var stream = File.Create(sfd.FileName))
            {
                    
                PngExporter.Export(PlotKey, stream, 600, 400,OxyColor.FromArgb(255,255,255,255));
            }
        }

       
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

            var d = CalculateModels(GetStateByIteration(category));

            
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
                var sum = _sumPoints.ToArray();
                double v = sum.Sum();
                if (Math.Abs(v) < double.Epsilon * 100) v = 1;
                sum = sum.Select(k => 100 * k / v).ToArray();
                foreach (var s in GenerateColumns(category - 1, sum))
                {
                    var columnSeries = SumModel.Series[0] as ColumnSeries;
                    if (columnSeries != null)
                        columnSeries.Items.Add(s);
                }
            }

            CountModel = CountModel;
            PointsModel = PointsModel;
            SumModel = SumModel;
            CountModel.InvalidatePlot(true);
            PointsModel.InvalidatePlot(true);
            SumModel.InvalidatePlot(true);

        }

        private readonly double[] _sumPoints;
        /// <summary>
        /// Metoda wyliczająca dane do wykresów na podstawie stanu automatu
        /// </summary>
        double[][] CalculateModels(Tuple<int,float>[,] cells)
        {
            var result = new double[Enum.GetValues(typeof(WhenBetray)).Length];
            var count = new double[result.Length];
            var sum = new double[result.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = 0;
                count[i] = 0;
                
            }
            for (var i = 0; i < cells.GetLength(0); i++)
                for (var j = 0; j < cells.GetLength(1); j++)
                {
                    var c = cells[i, j];
                    var integerStrategy = new Engine.Strategies.IntegerStrategy(c.Item1);

                    if (integerStrategy != null)
                    {
                        var k = integerStrategy.BetrayalThreshold;
                        double d = (double)c.Item2/_spd.Neighbours(i,j).Length;
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
            for (int i = 0; i < result.Length; i++) _sumPoints[i] += result[i];
            count = count.Select(d => 100 * d / d2).ToArray();
            return new[] { count, result };
        }

        public Dictionary<int, Engine.Strategies.IStrategy> GenerateIntegerStrategies(int count)
        {
            Dictionary<int, Engine.Strategies.IStrategy> result = new Dictionary<int, Engine.Strategies.IStrategy>();
            for (int i = 0; i <= count; i++)
            {
                result.Add(i, new Engine.Strategies.IntegerStrategy(i));
            }
            return result;
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
            float[,] fakePoints = new float[strategies.GetLength(0), strategies.GetLength(1)];
            AddHistory(strategies,fakePoints);
            _sumPoints = new double[Enum.GetValues(typeof (WhenBetray)).Length];
            for (int i = 0; i < _sumPoints.Length; i++) _sumPoints[i] = 0;
            var payValues = PayValues;
            _strategies = strategies;

#if DEBUG
            var threadNum = 1;
#else
            var threadNum = 16;
#endif
            _spd =
                new Engine.SPD(
                    new PointMatrix((float) payValues[3], (float) payValues[2], (float) payValues[1],
                        (float) payValues[0]),
                    vonneumann
                        ? (INeighbourhood) new Moore(strategies.GetLength(0), strategies.GetLength(1))
                        : (INeighbourhood) new VonNeumann(strategies.GetLength(0), strategies.GetLength(1)), strategies,
                    GenerateIntegerStrategies(vonneumann ? 5 : 9), 10, threadNum);
            Speed = 1;
            PointsModel = new PlotModel();
            CountModel = new PlotModel();
            ChangeModel = new PlotModel();
            SumModel = new PlotModel();
            PointsModel.Title = "Średnie wartości punktowe";
            CountModel.Title = "Liczebność strategii";
            ChangeModel.Title = "Niestabilność układu";
            SumModel.Title = "Punkty dla strategii zagregowane";
            _iterations = new List<Tuple<int, string>>();

            PointsModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });

            CountModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });
            PointsModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });
            CountModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });

            PointsModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });
            CountModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });

            ChangeModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });
            ChangeModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });
            ChangeModel.Series.Add(new ColumnSeries {ColumnWidth = 10, IsStacked = true});
                 SumModel.Axes.Add(new CategoryAxis { ItemsSource = _iterations, LabelField = "Item2" });
            SumModel.Axes.Add(new LinearAxis { MinimumPadding = 0, AbsoluteMinimum = 0 });
            SumModel.Series.Add(new ColumnSeries { ColumnWidth = 10, IsStacked = true });
       

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
        private DrawingImage GenerateImage(Engine.SPD spd, int x, int y, int width, int height)
        {
            var cellWidth = Canvas.Width / width;
            var cellHeight = Canvas.Height / height;
            

            var dg = new DrawingGroup();
            var C = GetStateByIteration(Iteration);
            for (var i = x; i < x + width; i++)
            {
                for (var j = y; j < y + height; j++)
                {
                    var rg = new RectangleGeometry(new Rect(new Point((i - x) * cellWidth, (j - y) * cellHeight), new Point((i - x + 1) * cellWidth, (j + 1 - y) * cellHeight)));
                  
                    var gd = new GeometryDrawing
                    {
                        Brush =
                            GetBrush(new Engine.Strategies.IntegerStrategy(C[i,j].Item1).BetrayalThreshold),
                        Geometry = rg
                    };
                    dg.Children.Add(gd);
                }
            }
            return new DrawingImage(dg);
        }

        private volatile bool _cont;
        /// <summary>
        /// Ilość iteracji
        /// </summary>
        public int StateCount
        {
            get { return _spd.CurrentIteration > 0 ? _spd.CurrentIteration - 1 : 0; }
        }

        readonly int _delay = 16;

        Task<SPDResult> _iteration;
        private Task Looper;
        private bool _over = true;

        public Tuple<int,float>[,] GetStateByIteration(int iteration)
        {
            return History[iteration];
        }

        private List<Tuple<int,float>[,]> History = new List<Tuple<int,float>[,]>();
        public void AddHistory(int[,] strategies, float[,] points)
        {
            Tuple<int,float>[,] state = new Tuple<int,float>[strategies.GetLength(0), strategies.GetLength(1)];
            for (int i = 0; i < strategies.GetLength(0); i++)
            {
                for (int j = 0; j < strategies.GetLength(1); j++)
                {
                    state[i, j] = new Tuple<int,float>(strategies[i, j], points[i, j]);
                }
            }
            History.Add(state);
        }

        public int GetVariance(int iteration)
        {
            int variance = 0;
            if (iteration == 0) return variance;
            var state1 = GetStateByIteration(iteration - 1);
            var state2 = GetStateByIteration(iteration);
            for (int i = 0; i < state1.GetLength(0); i++)
            {
                for (int j = 0; j < state1.GetLength(1); j++)
                {
                    variance += state1[i, j].Item1 == state2[i, j].Item1 ? 0 : 1;
                }
            }
            return variance;
        }

        private void SPDSynchronousLooper()
        {
             while (_cont && _over)
             {



                 var i = _spd.Iterate();
                if (_over) AddHistory(i.strategyConfig,i.v1);
                if (_over) UpdateImage();
                if (_over) OnPropertyChanged("StateCount");
                if (_over) if (Iteration == StateCount - 1) Iteration++;
              
              
                if (_over) InsertVariationColumn(GetVariance(History.Count-1));
                if (_over) UpdateModels();
                
                if (!i.v2) continue;
                var b = MessageBox.Show("Wykryto stabilizacje,przerwać?", "Stabilizacja układu",
                    MessageBoxButton.YesNo);
                if (b == MessageBoxResult.Yes)
                {
                    _over = false;
                }
               
            }
        }
        /// <summary>
        /// Metoda realizująca pętle symulacji
        /// </summary>
        /// <returns>Task pętli</returns>
        private async Task SPDLooper()
        {
            // _cont - pauza, _over - koniec obliczeń
            while (_cont && _over)
            {
              
                _iteration = Task.Run(async () => await _spd.IterateAsync());
                await Task.WhenAll(_iteration, Task.Delay((60 / Speed) * _delay));
                var i = await _iteration;
                if (_over) AddHistory(i.strategyConfig,i.v1);
                if (_over) UpdateImage();
                if (_over) OnPropertyChanged("StateCount");
                if (_over) if (Iteration == StateCount - 1) Iteration++;
                if (_over) await Task.WhenAll(_iteration, Task.Delay((60 / Speed) * _delay));
              
                if (_over) InsertVariationColumn(GetVariance(History.Count-1));
                if (_over) UpdateModels();
                
                if (!i.v2) continue;
                var b = MessageBox.Show("Wykryto stabilizacje,przerwać?", "Stabilizacja układu",
                    MessageBoxButton.YesNo);
                if (b == MessageBoxResult.Yes)
                {
                    _over = false;
                }
               
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

        private void StartSynchronousSPD()
        {
            SPDSynchronousLooper();
        }
        private async Task StartSPD()
        {
            Looper = SPDLooper();
            await Looper;
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

            if (Looper != null) Looper.Wait();
            if (_iteration != null) _iteration.Wait();
            var pl = Model.SPD.ClearAndGetLog();
            if (!PerformanceCheck.IsChecked.HasValue || !PerformanceCheck.IsChecked.Value) return;
            

            if (pl.StepTimes.Length == 0) return;
             MessageBox.Show(string.Format("Mediana: {0}\nŚrednia: {1}\nMaksymalny czas kroku: {2}\nMinimalny czas kroku: {3}",
                       pl.Median, pl.Average, pl.MaxStepTime, pl.MinStepTime));

        }

        private Brush GetBrush(int p)
        {
            return SPDAssets.GetBrush(p);
        }

        private async Task WaitForIteration()
        {
            if (Looper != null) await Looper;
            if (_iteration != null) await _iteration;
          
            StartStop.Content = "Start";
            StartStop.IsEnabled = true;
        }

        private bool Threading = false;
        private async void Button_Click(object sender, RoutedEventArgs e)
        {

            _cont = !_cont;
            if (_cont)
            {

                 StartStop.Content = "Stop";
                 if (Threading) await StartSPD();
                 else StartSynchronousSPD();

            }
            else
            {
                StartStop.Content = "Zatrzymywanie...";
                StartStop.IsEnabled = false;
                if(Threading) await WaitForIteration();
            }

           
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private PlotModel _summodel;
       

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Metoda zapisuje wykres automatu do pliku
        /// </summary>
        /// <param name="filePath">ścieżka pliku do zapisania</param>
        /// <param name="b">Wykres automatu</param>
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
            var c = GetStateByIteration(_iter);
            var ifc = InitialConditions.FromCellArray(c, Path.GetFileName(sfd.FileName));
            var bf = new BinaryFormatter();
            var fs = new FileStream(sfd.FileName, FileMode.Create);
            bf.Serialize(fs, ifc);
            fs.Close();
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var s =( (sender as MenuItem).Parent as ContextMenu).PlacementTarget as PlotView;
            
            SavePlot(s.Model);
        }
    }
}
