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
using System.Windows.Controls.Primitives;

namespace SpacialPrisonerDilemma.View
{
    
    /// <summary>
    /// Interaction logic for InitialCondition.xaml
    /// </summary>
    public partial class InitialCondition : INotifyPropertyChanged
    {
        private readonly Dictionary<Tuple<string, bool>, Func<bool, int, int, InitialConditions>> _conditions;
        private readonly List<Tuple<string, Tuple<string, bool>>> _conditionNames;
        private InitialConditions _condition;
		private readonly int Mode;
        private Operation _selectedOperation;
        private int _tooltip;

        public int ToolTipID
        {
            get { return _tooltip; }
            set
            {
                _tooltip = value;
                NotifyPropertyChanged("ToolTipID");
                NotifyPropertyChanged("ToolTipDescription");
            }
        }
        public string ToolTipDescription
        {
            get
            {
                if (ToolTipID < 0) return "";
                return SPDAssets.GetDescription(ToolTipID, Mode);
            }
        }
        /// <summary>
        /// Konstruktor okna warunków początkowych
        /// </summary>
        internal InitialCondition(int _mode=SPDAssets.MAX,InitialConditions condition=null)
        {
            _tooltip = -1;
            InitializeComponent();

            Mode = _mode;
            _selectedOperation = Operation.None;
            ComboBox.ItemsSource = SPDAssets.GetBrushRectangles(Mode,InitialConditions.GetTransformation(Mode));
            ComboBox.SelectedIndex = 0;
            DataContext = this;
            _conditionNames = new List<Tuple<string,Tuple<string,bool> > >();
            _conditions = new Dictionary<Tuple<string,bool>, Func<bool, int,int,InitialConditions>>();
            foreach (var T in new[] {false, true})
            {
                _conditions.Add(new Tuple<string, bool>("Donut", T), InitialConditions.DonutFactory);
                _conditions.Add(new Tuple<string, bool>("Circle", T), InitialConditions.CircleFactory);
                _conditions.Add(new Tuple<string,bool>("Diagonal",T),InitialConditions.DiagonalFactory);
                _conditions.Add(new Tuple<string, bool>("NowakMay", T), InitialConditions.NowakMayFactory);
            }
            _conditionNames.AddRange(
                _conditions.Select(
                    k =>
                        new Tuple<string, Tuple<string, bool>>(k.Value(k.Key.Item2, 1,10).Name,
                            new Tuple<string, bool>(k.Key.Item1, k.Key.Item2))));
            ComboBoxCopy.ItemsSource = _conditionNames.Select(s=>s.Item1);
            var D = SPDAssets.GenerateLegend(Legend.Height, Mode, InitialConditions.GetTransformation(Mode));
            D.Stretch = Stretch.Fill;
       
            Legend.Children.Add(D);
            if (condition != null) Condition = condition;
        }
        /// <summary>
        /// Możliwe operacje na układzie początkowym
        /// </summary>
        public enum Operation
        {
            None,
            Fill,
            Check
        }
        private DrawingImage GenerateImage( int x, int y, int width, int height)
        {
            return Condition.Grid.GenerateImage(x, y, width, height, Canvas.Width, Canvas.Height,ToolTipID);
        }
      
        private void NotifyPropertyChanged(string s)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(s));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal InitialConditions Condition
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
            if (Canvas == null || Condition == null) return;
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image
            {
                Source =
                    GenerateImage( _x,_y, (int)(Condition.Grid.CellGrid.GetLength(0)*_scale),
                        (int)(Condition.Grid.CellGrid.GetLength(1)*_scale))
               
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetScale();
            Condition = InitialConditions.GenerateRandom((int)RandomSize.Value,Mode);
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
            ResetScale();
            if (ComboBoxCopy.SelectedIndex < 0) return;
            Condition =
                 _conditions[_conditionNames[ComboBoxCopy.SelectedIndex].Item2](
                 _conditionNames[ComboBoxCopy.SelectedIndex].Item2.Item2, (int)RandomSize.Value, Mode);
        }

        private void ResetScale()
        {
            _x = 0;
            _y = 0;
            _scale = 1;
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
                ic.Grid.CellGrid[(int) x, (int) y].Value = InitialConditions.GetTransformation(Mode)(ComboBox.SelectedIndex);
            }
            else
            {
                var k = ic.Grid.CellGrid[(int) x, (int) y].Value;
                for(var i=0;i<ic.Grid.CellGrid.GetLength(0);i++)
                    for (var j = 0; j < ic.Grid.CellGrid.GetLength(1); j++)
                    {
                        if (ic.Grid.CellGrid[i, j].Value == k) ic.Grid.CellGrid[i, j].Value = InitialConditions.GetTransformation(Mode)(ComboBox.SelectedIndex);
                    }
            }
            Condition = ic;
        }
		private void RandomSize_DragCompleted(object sender, DragCompletedEventArgs e)
		{
            ResetScale();
            if (Condition==null) return;
			if (ComboBoxCopy.SelectedIndex < 0) 
			{
				Condition = InitialConditions.GenerateRandom((int)RandomSize.Value,Mode);
                return;
			}
            Condition =
                          _conditions[_conditionNames[ComboBoxCopy.SelectedIndex].Item2](
                          _conditionNames[ComboBoxCopy.SelectedIndex].Item2.Item2, (int)RandomSize.Value, Mode);
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
            var d = p.Y/(Legend.ActualHeight/(Mode));
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

        private void Legend_OnMouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(Legend);
            var d = p.Y / (Legend.ActualHeight / (Mode));
            bool b = (int)d == ToolTipID;
            ToolTipID = (int)d;
            if (!b) UpdateScreen();
        }

        private void Legend_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ToolTipID = -1;
            UpdateScreen();
        }

        private void Canvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(Canvas);
            var X = p.X / _scale;
            var Y = p.Y / _scale;
            X = X - _x;
            Y = Y - _y;
            X = X / Canvas.Width;
            X = X * Condition.Grid.CellGrid.GetLength(0);
            if (X >= Condition.Grid.CellGrid.GetLength(0)) return;
            Y = Y / Canvas.Height;
            Y = Y * Condition.Grid.CellGrid.GetLength(0);
            if (Y >= Condition.Grid.CellGrid.GetLength(0)) return;
            var C = Condition.Grid.CellGrid;

            var c = C[(int)X, (int)Y];
            var b = c.Value == ToolTipID;
            ToolTipID = c.Value;
            if (!b) UpdateScreen();

        }

        private void Canvas_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ToolTipID = -1;
            UpdateScreen();
        }
    }
}