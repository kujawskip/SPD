using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using SPD.Engine;
using SPD.Engine.Neighbourhoods;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        public enum Neighbourhoods
        {
            Moore,
            VonNeumann,
            Mixed,
            Taxi
        }
        public enum Shape
        {
            Płaski,
            Torus
        }

      
        private ValidationErrors _error;
        private int _colorPickerIndex;
        private InitialConditions _ic;
        /// <summary>
        /// Możliwe błędy walidacji
        /// </summary>
        public enum ValidationErrors
        {
            None,
            ParseError,
            ValueError
        }

        private bool _advancedPointMatrix;
        private PointMatrixPick pointMatrix;
        public bool AdvancedPointMatrix
        {
            get { return _advancedPointMatrix; }
            set
            {
                _advancedPointMatrix = value; 
                NotifyPropertyChanged("NoAdvancedError");
                NotifyPropertyChanged("AdvancedPointMatrix");
            }
        }
        private readonly bool _canvalidate;
        /// <summary>
        /// Konstruktor okna głównego
        /// </summary>
        int GetNeighboursCount(Neighbourhoods _neighbourhood, int Size)
        {
            switch (_neighbourhood)
            {
                case Neighbourhoods.Mixed:
                    return GetNeighboursCount(Neighbourhoods.Moore, Size) + GetNeighboursCount(Neighbourhoods.VonNeumann, Size);
                case Neighbourhoods.Moore:
                    return (2 * Size + 1) * (2 * Size + 1) - 1;
                case Neighbourhoods.VonNeumann:
                    return 4 * Size;
                case Neighbourhoods.Taxi:
                   return 2*Size*(Size + 1);
            }
            throw new ArgumentException("Unrecognisable neighbourhood");
        }

        INeighbourhood GetNeighbourhood(Neighbourhoods _neighbourhood, Shape _shape, int Size, int _width, int _height)
        {
            switch (_neighbourhood)
            {
                case Neighbourhoods.Mixed:
                    return
                        new Mixed(GetNeighbourhood(Neighbourhoods.Moore, _shape, Size,_width,_height), GetNeighbourhood(Neighbourhoods.VonNeumann, _shape, Size,_width,_height));
                case Neighbourhoods.Moore:
                    switch (_shape)
                    {
                        case Shape.Płaski:
                            return new Moore(_width, _height, Size);
                        case Shape.Torus:
                            return new MooreTorus(_width, _height, Size);
                    }
                    break;
                case Neighbourhoods.VonNeumann:
                    switch (_shape)
                    {
                        case Shape.Płaski:
                            return new VonNeumann(_width, _height, Size);
                        case Shape.Torus:
                            return new VonNeumannTorus(_width, _height, Size);
                    }
                    break;
                case Neighbourhoods.Taxi:
                    switch (_shape)
                    {
                        case Shape.Płaski:
                            return new Taxi(_width, _height, Size);
                        case Shape.Torus:
                            return new TaxiTorus(_width, _height, Size);
                    }
                    break;
            }
            throw new ArgumentException("Unrecognisable neighbourhood");
        }
        public MainWindow()
        {
            SPDAssets.CreateBrushes();
            SPDAssets.ChangeFont("Arial");
            SPDAssets.InitialiseDescriptions();
            _colorPicking = ColorPicking.RegularPickingFactory(10);
            InitializeComponent();

            InitialConditions.Initialise();
            ShapeBox.ItemsSource = Enum.GetValues(typeof(Shape));
            NeighbourBox.ItemsSource = Enum.GetValues(typeof(Neighbourhoods));
            ShapeBox.SelectedItem = Shape.Płaski;
            NeighbourBox.SelectedItem = Neighbourhoods.Moore;

            DataContext = this;
            Error = ValidationErrors.None;
            _canvalidate = true;
        

        }

        private ValidationErrors Error
        {
            get { return _error; }
            set
            {
                _error = value;
                NotifyPropertyChanged("Error");
            }
        }

        public static readonly string[] ErrorMessages = { "", "Błąd przetwarzania", "Błąd wartości" };
        /// <summary>
        /// Opis błędu walidacji
        /// </summary>
        public string ErrorMessage
        {
            get { return ErrorMessages[(int)Error]; }
        }
        /// <summary>
        /// Czy wystąpił błąd walidacji
        /// </summary>
        public bool NoError
        {
            get { return Error == ValidationErrors.None && _ic != null; }
        }
        /// <summary>
        /// Czy wyświetlić opis błedu
        /// </summary>
        public bool ShowErrorMessage
        {
            get { return !NoError; }
        }
        private void NotifyPropertyChanged(string s)
        {
            List<string> properties = new List<string>();

            if (s == "Error")
            {
                properties.Add("ErrorMessage");
                properties.Add("NoError");
                properties.Add("NoAdvancedError");

            }
            if (s == "NoError")
            {
                properties.Add("NoAdvancedError");
            }
            if (s == "NoAdvancedError")
            {
                properties.Add("ShowErrorMessage");
            }
           
            foreach (var p in properties)
            {
                NotifyPropertyChanged(p);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(s));
            }
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
            string pattern = Regex.Escape("(") + Regex.Escape(" ") + "*" + "[0-9]+(" + Regex.Escape(".") +
                             "[0-9]*)? , [0-9]+(" + Regex.Escape(".") +
                             "[0-9]*)?" + Regex.Escape(" ") + "*" + Regex.Escape(")");
            return Regex.Match(text,
                pattern).Success;
        }
        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            TryValidate();
        }

        private bool TryValidate(string text, out double var1, out double var2)
        {
            var1 = -1;
            var2 = -1;
            if (!ValidatesToPair(text)) return false;
            var array = text.Substring(1, text.Length - 2).Split(',');
            
            bool flag = double.TryParse(array[0].Trim(), out var1);

            if (!flag)
            {
                flag = double.TryParse(array[0].Replace(".", ",").Trim(), out var1);
                if(!flag) return false;
            }
            flag = double.TryParse(array[1].Trim(),out var2);
            if (!flag)
            {
                flag = double.TryParse(array[1].Replace(".", ",").Trim(), out var2);
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
            double[] array = new double[6];
            var sarray = new[] {BothBetray.Text, FirstBetrays.Text, NobodyBetrays.Text,SecondBetrays.Text};
            int i = 0;
            foreach (var s in sarray)
            {
                double d1, d2;
                bool flag = TryValidate(s, out d1, out d2);
                if (!flag)
                {
                   Error = ValidationErrors.ParseError;
                    return null;
                }
                
                if (i >= array.Length) continue;
                array[i] = d1;
                array[i + 1] = d2;
                i += 2;
            }
            if (array[1]!=array[0] || array[4]!=array[5] || 2*array[4]<=(array[2]+array[3]) || !(array[3]<=array[1]&&array[1]<=array[4]&&array[4]<=array[2]))
                {
                    
                    Error = ValidationErrors.ValueError;
                    return null;
                }
            Error = ValidationErrors.None;
            return new []{array[1],array[2],array[3],array[4]};
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double[] d = Validate();
        
            PointMatrix baseMatrix = new PointMatrix((float) d[3],(float) d[2],(float) d[1],(float) d[0]);
            SPDView spdView = new SPDView(AdvancedPointMatrix?pointMatrix:PointMatrixPick.SingularMatrixCondition(baseMatrix,_ic.Grid.CellGrid.GetLength(0)), Transform(_ic.Grid), GetNeighboursCount((Neighbourhoods)NeighbourBox.SelectedItem, (int)Slider1.Value), GetNeighbourhood((Neighbourhoods)NeighbourBox.SelectedItem, (Shape)ShapeBox.SelectedItem, (int)Slider1.Value, _ic.Grid.CellGrid.GetLength(0), _ic.Grid.CellGrid.GetLength(1)));
            spdView.ShowDialog();
        }

        private bool _advancedMatrixAccepted;
        public bool AdvancedMatrixAccepted
        {
            get { return _advancedMatrixAccepted; }
            set { _advancedMatrixAccepted = value;
                NotifyPropertyChanged("NoAdvancedError");
            }
        }

        public bool NoAdvancedError
        {
            get
            {
                return NoError && (!AdvancedPointMatrix || AdvancedMatrixAccepted);
            }
        }
        private int[,] Transform(InitialConditionsGrid ig)
        {
            var ar = ig.CellGrid;
            int[,] result = new int[ar.GetLength(0), ar.GetLength(1)];
            for (int i = 0; i < ar.GetLength(0); i++)
                for (int j = 0; j < ar.GetLength(1); j++)
                    result[i, j] = ar[i, j].Value;
            return result;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var ic = new InitialCondition(2+GetNeighboursCount((Neighbourhoods)NeighbourBox.SelectedItem,(int)Slider1.Value),_ic);
            var b = ic.ShowDialog();
            if (!b.HasValue || !b.Value) return;
            if (pointMatrix != null && ic.Condition.Grid.CellGrid.GetLength(0) != pointMatrix.Size)
            {
                AdvancedMatrixAccepted = false;
            }
            _ic = ic.Condition;
            NotifyPropertyChanged("NoError");
            NotifyPropertyChanged("NoAdvancedError");
        }

        private ColorPicking _colorPicking;
        private void Color_OnClick(object sender, RoutedEventArgs e)
        {
            var cp = new ColorPicker(_colorPickerIndex, 2+GetNeighboursCount((Neighbourhoods)NeighbourBox.SelectedItem, (int)Slider1.Value));
            var b = cp.ShowDialog();
            if (b.HasValue && b.Value)
            {
                _colorPickerIndex = cp.Id;
                _colorPicking = cp.Box.SelectedItem as ColorPicking;

            }
        }

        private void Font_OnClick(object sender, RoutedEventArgs e)
        {
            var fp = new FontPicker(SPDAssets.GetFont(), 2 + GetNeighboursCount((Neighbourhoods)NeighbourBox.SelectedItem, (int)Slider1.Value));
           fp.ShowDialog();
          
        }

        private void UpdateColors()
        {
            if (NeighbourBox == null) return;
            if (Slider1 == null) return;
            if (_colorPicking == null) return;
            if (NeighbourBox.SelectedItem == null) return;
            _colorPicking.ChangeSize(2+GetNeighboursCount((Neighbourhoods)NeighbourBox.SelectedItem, (int)Slider1.Value));
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _ic = null;
            NotifyPropertyChanged("NoError");
            UpdateColors();

        }

        private void AdvancedMatrix_OnClick(object sender, RoutedEventArgs e)
        {
            AdvancedPointMatrix = true;
            var d = Validate();
            if (d == null)
            {
                MessageBox.Show("Podstawowa Macierz musi być poprawna przed przejściem do zaawansowanych ustawień");
            }
            var D = d.Select(x=>(float)x).ToArray();
            PointMatrixPicker picker = new PointMatrixPicker(new PointMatrix(D[3],D[2],D[1],D[0]),_ic.Grid.CellGrid.GetLength(0),pointMatrix);
            var b = picker.ShowDialog();
            if (b.HasValue && b.Value)
            {
                AdvancedMatrixAccepted = true;
                pointMatrix = picker.Condition;
            }
        }

        private void Slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateColors();
        }
    }

}