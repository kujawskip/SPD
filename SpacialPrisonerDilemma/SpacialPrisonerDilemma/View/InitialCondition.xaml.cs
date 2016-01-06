using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    
    /// <summary>
    /// Interaction logic for InitialCondition.xaml
    /// </summary>
    public partial class InitialCondition : INotifyPropertyChanged
    {
        private readonly Dictionary<Tuple<string, bool>, Func<bool, int, InitialConditions>> _conditions;
        private readonly List<Tuple<string, Tuple<string, bool>>> _conditionNames;
        private InitialConditions _condition;
        private Operation _selectedOperation;
        
        
        public InitialCondition()
        {
            InitializeComponent();
            _selectedOperation = Operation.None;
            ComboBox.ItemsSource = SPDAssets.GetBrushRectangles();
            ComboBox.SelectedIndex = 0;
            DataContext = this;
            _conditionNames = new List<Tuple<string,Tuple<string,bool> > >();
            _conditions = new Dictionary<Tuple<string,bool>, Func<bool, int,InitialConditions>>();
            foreach (var T in new[] {false, true})
            {
                _conditions.Add(new Tuple<string, bool>("Donut", T), InitialConditions.DonutFactory);
                _conditions.Add(new Tuple<string, bool>("Circle", T), InitialConditions.CircleFactory);
                _conditions.Add(new Tuple<string,bool>("Diagonal",T),InitialConditions.DiagonalFactory);

            }
            _conditionNames.AddRange(
                _conditions.Select(
                    k =>
                        new Tuple<string, Tuple<string, bool>>(k.Value(k.Key.Item2, 1).Name,
                            new Tuple<string, bool>(k.Key.Item1, k.Key.Item2))));
            ComboBoxCopy.ItemsSource = _conditionNames.Select(s=>s.Item1);
            var image2 = new Image
            {
                Source = SPDAssets.GenerateLegend(Legend.Height),
                Stretch = Stretch.Fill
            };
       
            Legend.Children.Add(image2);
        }
        public enum Operation
        {
            None,
            Fill,
            Check
        }
        public DrawingImage GenerateImage(InitialConditionsGrid grid, int x, int y, int width, int height)
        {
            var cellWidth = Canvas.Width/width;
            var cellHeight = Canvas.Height/height;
          
           
            var dg = new DrawingGroup();
            for (var i = x; i < x + width; i++)
            {
                for (var j = y; j < y + height; j++)
                {
                  
                    var rg = new RectangleGeometry(new Rect(new Point((i - x) * cellWidth, (j - y) * cellHeight), new Point((i - x + 1) * cellWidth, (j + 1 - y) * cellHeight)));
                    var gd = new GeometryDrawing
                    {
                        Brush = SPDAssets.GetBrush(Condition.Grid.CellGrid[i, j].Value),
                        Geometry = rg
                    };
                    dg.Children.Add(gd);
                }
            }
            return new DrawingImage(dg);
        }
        public InitialConditions LoadConditions(string path)
        {
            var fs = new FileStream(path,FileMode.Open);
            var bf = new BinaryFormatter();
            try
            {
                var obj = bf.Deserialize(fs);
                fs.Close();
                return (InitialConditions)obj;
            }
            catch (Exception)
            {
                fs.Close();

                return null;
            }
         
            
        }

        private void NotifyPropertyChanged(string s)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(s));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public InitialConditions Condition
        {
            get { return _condition; }
            private set { _condition = value;
                UpdateScreen();
             NotifyPropertyChanged("ConditionLoaded");
            }
        }

        public bool ConditionLoaded
        {
            get { return Condition != null; }
        }
        private void UpdateScreen()
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image
            {
                Source =
                    GenerateImage(Condition.Grid, _x,_y, (int)(Condition.Grid.CellGrid.GetLength(0)*_scale),
                        (int)(Condition.Grid.CellGrid.GetLength(1)*_scale))
               
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Condition = InitialConditions.GenerateRandom((int)RandomSize.Value);
            ComboBoxCopy.SelectedIndex = -1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private double _scale = 1;
        private int _x, _y;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ComboBoxCopy.SelectedIndex < 0) return;
            Condition =
                _conditions[_conditionNames[ComboBoxCopy.SelectedIndex].Item2](
                    _conditionNames[ComboBoxCopy.SelectedIndex].Item2.Item2, (int) RandomSize.Value);
        }


        private void Canvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Operation.None == _selectedOperation) return;
            
            if(ComboBox.SelectedIndex<0) return;
            var p = e.GetPosition(Canvas);
            var ic = Condition;
            var x =_x + _scale*p.X/(Canvas.Width/(ic.Grid.CellGrid.GetLength(0)));
            if (x >= ic.Grid.CellGrid.GetLength(0) ) return;
            var y =_y + _scale*p.Y/(Canvas.Height/(ic.Grid.CellGrid.GetLength(1)));
            if (y >= ic.Grid.CellGrid.GetLength(1) ) return;
            if (Operation.Check == _selectedOperation)
            {
                ic.Grid.CellGrid[(int) x, (int) y].Value = ComboBox.SelectedIndex;
            }
            else
            {
                var k = ic.Grid.CellGrid[(int) x, (int) y].Value;
                for(var i=0;i<ic.Grid.CellGrid.GetLength(0);i++)
                    for (var j = 0; j < ic.Grid.CellGrid.GetLength(1); j++)
                    {
                        if (ic.Grid.CellGrid[i, j].Value == k) ic.Grid.CellGrid[i, j].Value = ComboBox.SelectedIndex;
                    }
            }
            Condition = ic;
        }

        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Condition == null) return;
            var ic = Condition;
            var p = e.GetPosition(Canvas);
            var X = _x + _scale * p.X / (Canvas.Width / (ic.Grid.CellGrid.GetLength(0)));
            if (X >= ic.Grid.CellGrid.GetLength(0)) return;
            var Y = _y + _scale * p.Y / (Canvas.Height / (ic.Grid.CellGrid.GetLength(1)));
            if (Y >= ic.Grid.CellGrid.GetLength(1)) return;
            _scale += Math.Sign(-e.Delta)*0.1;
            if (_scale < 0.1) _scale = 0.1;
            if (_scale > 1) _scale = 1;
           
            var width = Condition.Grid.CellGrid.GetLength(0);
            var height = Condition.Grid.CellGrid.GetLength(1);
            var nwidth = (int) (width*_scale);
            var nheight = (int) (height*_scale);
            var x = (int) X - (nwidth/2);
            var y = (int) Y
                    - (nheight/2);
            var xx = (int) X + (nwidth/2);
            var yy = (int) Y + (nheight/2);
          
            if (xx >= width) x -= (xx - width) + 1;
            if (yy >= height) y -= (yy - height) + 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            _x = x;
            _y = y;
            Condition = ic;
        }

        private void ButtonBase_OnClick1(object sender, RoutedEventArgs e)
        {
            var bf = new BinaryFormatter();
            var ofd = new OpenFileDialog
            {
                Filter = "Initial Condition File (*.cic)|*.cic",
                Multiselect = false
            };

            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fs = new FileStream(ofd.FileName,FileMode.Open);
                var obj = bf.Deserialize(fs);
                Condition = obj as InitialConditions;
            }
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            if (Condition == null) return;
            var bf = new BinaryFormatter();
            var ofd = new SaveFileDialog { Filter = "Initial Condition File (*.cic)|*.cic" };
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fs = new FileStream(ofd.FileName, FileMode.Create);
                bf.Serialize(fs,Condition);
               
            }
        }

        private void Legend_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
          var p =  e.GetPosition(Legend);
            var d = p.Y/(Legend.Height/Enum.GetValues(typeof(WhenBetray)).Length);
            ComboBox.SelectedIndex = (int) d;
        }

        private void RadioPixel_OnChecked(object sender, RoutedEventArgs e)
        {
            if (RadioFill.IsChecked.HasValue && RadioFill.IsChecked.Value)
            {
                _selectedOperation = Operation.Fill;
                return;
            }
            if (RadioPixel.IsChecked.HasValue && RadioPixel.IsChecked.Value)
            {
                _selectedOperation = Operation.Check;
                return;
            }
            _selectedOperation = Operation.None;
            
        }
    }
}
