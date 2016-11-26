using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using SpacialPrisonerDilemma.Model;
using System.Windows.Controls.Primitives;
using SPD.Engine;

namespace SpacialPrisonerDilemma.View
{
    
    /// <summary>
    /// Interaction logic for InitialCondition.xaml
    /// </summary>
    public partial class PointMatrixPicker : INotifyPropertyChanged
    {
        public class MatrixDescription
        {
            

            public string Description { get; set; }
            public Image Color { get; set; }
        }
        private readonly Dictionary<Tuple<string, bool>, Func<bool, int, int,bool, InitialConditions>> _conditions;
        private readonly List<Tuple<string, Tuple<string, bool>>> _conditionNames;
        private PointMatrixPick _condition;
		private readonly int Mode;
        private int Size;
        public bool StandardPointCounting
        {
            get { return !_condition.ModifiedPointCounting; }
            set { _condition.ModifiedPointCounting = !value; NotifyPropertyChanged("StandardPointCounting"); }
        }
        private int MatrixCount
        {
            get { return _condition.Matrices.Count; }
        }

        public List<MatrixDescription> MatrixDescriptions
        {
            get
            {
                return _condition.Matrices.Select((PointMatrix x,int t)=>(new MatrixDescription(){Description=x.ToString(),Color=BrushRectangles[t]}))
                .ToList();
                
            }
        }
        private Operation _selectedOperation;
        private List<Image> BrushRectangles;
        /// <summary>
        /// Konstruktor okna warunków początkowych
        /// </summary>
       
        /// <summary>
        /// Konstruktor okna warunków początkowych
        /// </summary>
        internal PointMatrixPicker(PointMatrix matrix ,int Size,PointMatrixPick condition = null)
        {
           
            
            _condition= condition ?? PointMatrixPick.SingularMatrixCondition(matrix, Size);
            BrushRectangles = SPDAssets.GetBrushRectangles((int)MatrixCount);
            if (Condition.Size != Size)
            {
                _condition = Condition.Resize(Size);
            }
            this.Size = Size;
            InitializeComponent();
           
            
            _selectedOperation = Operation.None;
         
            _comboBox.ItemsSource = BrushRectangles;
            _comboBox.SelectedIndex = 0;
           
            DataContext = this;
            _conditionNames = new List<Tuple<string,Tuple<string,bool> > >();
            _conditions = new Dictionary<Tuple<string,bool>, Func<bool, int,int,bool,InitialConditions>>();
            foreach (var T in new[] {false, true})
            {
                _conditions.Add(new Tuple<string, bool>("Donut", T), InitialConditions.DonutFactory);
                _conditions.Add(new Tuple<string, bool>("Circle", T), InitialConditions.CircleFactory);
                _conditions.Add(new Tuple<string,bool>("Diagonal",T),InitialConditions.DiagonalFactory);

            }
            _conditionNames.AddRange(
                _conditions.Select(
                    k =>
                        new Tuple<string, Tuple<string, bool>>(k.Value(k.Key.Item2, 1,10,false).Name,
                            new Tuple<string, bool>(k.Key.Item1, k.Key.Item2))));
            ComboBoxCopy.ItemsSource = _conditionNames.Select(s=>s.Item1);
            _canvalidate = true;
            Condition = Condition;

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
        private DrawingImage GenerateImage(int x, int y, int width, int height)
        {
            
            var ic = PointMatrixPick.CreateICFromPick(Condition);
            return ic.Grid.GenerateImage(x, y, width, height, Canvas.Width, Canvas.Height);
        }
      
        private void NotifyPropertyChanged(string s)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(s));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        internal PointMatrixPick Condition
        {
            get { return _condition; }
            private set 
            {
                _condition = value;
                UpdateScreen();
                NotifyPropertyChanged("ConditionLoaded");
                NotifyPropertyChanged("MatrixCount");
                NotifyPropertyChanged("MatrixDescriptions");
                NotifyPropertyChanged("StandardPointCounting");
                NotifyPropertyChanged("CanAdd");
                NotifyPropertyChanged("CanDelete");
            }
        }

        public bool ConditionLoaded
        {
            get { return Condition != null; }
        }
        private void UpdateScreen()
        {
            Canvas.Children.Clear();
            var ic = PointMatrixPick.CreateICFromPick(Condition);
            Canvas.Children.Add(new Image
            {
                Source =
                    GenerateImage(_x, _y, (int)(ic.Grid.CellGrid.GetLength(0) * _scale),
                        (int)(ic.Grid.CellGrid.GetLength(1) * _scale))
               
            });
        }

        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ResetScale();
            Condition = PointMatrixPick.CreatePickFromIC(InitialConditions.GenerateRandom(Size, (int)MatrixCount),Condition);
            ComboBoxCopy.SelectedIndex = -1;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private double _scale = 1;
        private int _x, _y;
        private MainWindow.ValidationErrors Error;
        private bool _canvalidate=false;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           ReCreateCondition();
        }

        public void ReCreateCondition()
        {
            ResetScale();
            if (ComboBoxCopy.SelectedIndex < 0) return;
            Condition = PointMatrixPick.CreatePickFromIC(
                 _conditions[_conditionNames[ComboBoxCopy.SelectedIndex].Item2](
                 _conditionNames[ComboBoxCopy.SelectedIndex].Item2.Item2, Size, (int)MatrixCount,false), Condition);
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
            
            if(_comboBox.SelectedIndex<0) return;
            var p = e.GetPosition(Canvas);
            var ic = PointMatrixPick.CreateICFromPick(Condition);
            var x =_x + _scale*p.X/(Canvas.Width/(ic.Grid.CellGrid.GetLength(0)));
            if (x >= ic.Grid.CellGrid.GetLength(0) ) return;
            var y =_y + _scale*p.Y/(Canvas.Height/(ic.Grid.CellGrid.GetLength(1)));
            if (y >= ic.Grid.CellGrid.GetLength(1) ) return;
            if (Operation.Check == _selectedOperation)
            {
                ic.Grid.CellGrid[(int) x, (int) y].Value = InitialConditions.GetTransformation((int)MatrixCount)(_comboBox.SelectedIndex);
            }
            else
            {
                var k = ic.Grid.CellGrid[(int) x, (int) y].Value;
                for(var i=0;i<ic.Grid.CellGrid.GetLength(0);i++)
                    for (var j = 0; j < ic.Grid.CellGrid.GetLength(1); j++)
                    {
                        if (ic.Grid.CellGrid[i, j].Value == k) ic.Grid.CellGrid[i, j].Value = InitialConditions.GetTransformation((int)MatrixCount)(_comboBox.SelectedIndex);
                    }
            }
            Condition = PointMatrixPick.CreatePickFromIC(ic,Condition);
        }

        
		private void RandomSize_DragCompleted(object sender, DragCompletedEventArgs e)
		{
            ResetScale();
            if (Condition==null) return;
			if (ComboBoxCopy.SelectedIndex < 0) 
			{
                Condition = PointMatrixPick.CreatePickFromIC(InitialConditions.GenerateRandom(Size, (int)MatrixCount), Condition);
                return;
			}
            Condition = PointMatrixPick.CreatePickFromIC(
                          _conditions[_conditionNames[ComboBoxCopy.SelectedIndex].Item2](
                          _conditionNames[ComboBoxCopy.SelectedIndex].Item2.Item2, Size, (int)MatrixCount,false), Condition);
        }
        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Condition == null) return;
            var ic = PointMatrixPick.CreateICFromPick(Condition);
            var p = e.GetPosition(Canvas);
            var X = _x + _scale * p.X / (Canvas.Width / (ic.Grid.CellGrid.GetLength(0)));
            if (X >= ic.Grid.CellGrid.GetLength(0)) return;
            var Y = _y + _scale * p.Y / (Canvas.Height / (ic.Grid.CellGrid.GetLength(1)));
            if (Y >= ic.Grid.CellGrid.GetLength(1)) return;
            _scale += Math.Sign(-e.Delta)*0.1;
            if (_scale < 0.1) _scale = 0.1;
            if (_scale > 1) _scale = 1;
           
            var width = ic.Grid.CellGrid.GetLength(0);
            var height = ic.Grid.CellGrid.GetLength(1);
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
            Condition = PointMatrixPick.CreatePickFromIC(ic, Condition);
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
                Condition = PointMatrixPick.CreatePickFromIC(obj as InitialConditions, Condition);
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
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TryValidate();
            NotifyPropertyChanged("CanAdd");
        }

        private bool TryValidate(string text, out double var1)
        {
           

            bool flag = double.TryParse(text, out var1);
            if (!flag)
            {
                flag = double.TryParse(text.Replace(".", ",").Trim(), out var1);
                if (!flag) return false;
            }
            
            return true;

        }
        private void TryValidate()
        {
            if (!_canvalidate) return;
            Validate();

        }

        private double[] Validate()
        {
            double[] array = new double[4];
            var sarray = new[] { FirstBetrays.Text, NobodyBetrays.Text, BothBetray.Text, SecondBetrays.Text };
            int i = 0;
            foreach (var s in sarray)
            {
                double d1, d2;
                bool flag = TryValidate(s, out d1);
                if (!flag)
                {
                    Error = MainWindow.ValidationErrors.ParseError;
                    return null;
                }

                if (i >= array.Length) continue;
                array[i] = d1;

                i++;
            }
            if (2 * array[1] <= (array[3] + array[0]) || !(array[3] <= array[2] && array[2] <= array[1] && array[1] <= array[0]))
            {

                Error = MainWindow.ValidationErrors.ValueError;
                return null;
            }
            Error = MainWindow.ValidationErrors.None;
            return new[] { array[2], array[0], array[3], array[1] };
        }

        private string SwitchText(string text)
        {
            string newtext = text.Substring(1, text.Length - 2);

            var array = newtext.Split(',');
            for (int i = 0; i < array.Length; i++) array[i] = array[i].Trim();
            newtext = string.Format("({0} , {1})", array[1], array[0]);
            return newtext;
        }
        private bool ValidatesToPair(string text)
        {
            return Regex.Match(text,
                Regex.Escape("(") + Regex.Escape(" ") + "*" + "[0-9]+(" + Regex.Escape(".") + "[0-9]*)? , [0-9]+(" + Regex.Escape(".") +
                "[0-9]*)?" + Regex.Escape(" ") + "*" + Regex.Escape(")")).Success;
        }
        private void SecondBetrays_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (ValidatesToPair(SecondBetrays.Text))
            {
                FirstBetrays.Text = SwitchText(SecondBetrays.Text);
            }
        }

        private void FirstBetrays_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (ValidatesToPair(FirstBetrays.Text))
            {
                SecondBetrays.Text = SwitchText(FirstBetrays.Text);
            }
        }

        public bool CanAdd
        {
            get
            {
                if (!_canvalidate) return false;
                 var D = Validate();
                if (D == null) return false;
                var Matrix = new PointMatrix((float) D[3],(float) D[2],(float) D[1],(float) D[0]);
                return MatrixDescriptions.Count(M => M.Description == Matrix.ToString())==0;
            }
        }
        public bool CanDelete
        {
            get { return MatrixDescriptions.Count > 1; }
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var D = Validate();
            if (D == null)
            {
                MessageBox.Show(MainWindow.ErrorMessages[(int) Error]);
            }
            else
            {
                Condition.Matrices.Add(new PointMatrix((float) D[3],(float) D[2],(float) D[1],(float) D[0]));
                BrushRectangles = SPDAssets.GetBrushRectangles(MatrixCount);
                _comboBox.ItemsSource = BrushRectangles;
                Condition = Condition;
            }
        }

        private void MenuItemDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MatricesListBox.SelectedIndex == -1) return;
            var MatrixDescription = (MatricesListBox.SelectedItems[0] as MatrixDescription);
            var L = Condition.Matrices;
            var M = Condition.Matrices.Where(m => m.ToString() != MatrixDescription.Description).ToList();
            for (int i = 0; i < Condition.Indices.GetLength(0); i++)
            {
                for (int j = 0; j < Condition.Indices.GetLength(1); j++)
                {
                    var I = Condition.Indices[i, j];
                    if (Condition.Matrices[I].ToString() == MatrixDescription.Description)
                    {
                        Condition.Indices[i, j] = 0;

                    }
                    else
                    {
                        Condition.Indices[i, j] = M.FindIndex(m => m.ToString() == L[I].ToString());

                    }
                }
            }
            Condition.Matrices = M;
            BrushRectangles = SPDAssets.GetBrushRectangles(MatrixCount);
            _comboBox.ItemsSource = BrushRectangles;
            Condition = Condition;


        }

        private void _comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}