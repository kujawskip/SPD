using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

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
            VonNeumann
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
        
        private bool _canvalidate;
        /// <summary>
        /// Konstruktor okna głównego
        /// </summary>
        public MainWindow()
        {

            InitializeComponent();
            InitialConditions.Initialise();
            ShapeBox.ItemsSource = Enum.GetValues(typeof(Shape));
            NeighbourBox.ItemsSource = Enum.GetValues(typeof(Neighbourhoods));
            ShapeBox.SelectedItem = Shape.Płaski;
            NeighbourBox.SelectedItem = Neighbourhoods.Moore;
            DataContext = this;
            Error = ValidationErrors.None;
            _canvalidate = true;
            SPDAssets.CreateBrushes(10);
            SPDAssets.ChangeFont("Arial");
            SPDAssets.InitialiseDescriptions();

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

        private static readonly string[] ErrorMessages = { "", "Błąd przetwarzania", "Błąd wartości" };
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

            }
            if (s == "NoError")
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
            return Regex.Match(text,
                Regex.Escape("(") + Regex.Escape(" ") + "*" + "[0-9]+(" + Regex.Escape(".") + "[0-9]*)? , [0-9]+(" + Regex.Escape(".") +
                "[0-9]*)?" + Regex.Escape(" ") + "*" + Regex.Escape(")")).Success;
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
            if (!flag) return false;
            flag = double.TryParse(array[1].Trim(), out var2);
            if (!flag) return false;
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
            if (array[1]!=array[0] || array[4]!=array[5] || 2*array[4]<=(array[2]+array[3]) || !(array[3]<array[1]&&array[1]<array[4]&&array[4]<array[2]))
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
            SPD spd = new SPD(d, Transform(_ic.Grid),(Shape)ShapeBox.SelectedItem == Shape.Torus,Neighbourhoods.VonNeumann==(Neighbourhoods)NeighbourBox.SelectedItem);
            spd.ShowDialog();
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
            var ic = new InitialCondition(Neighbourhoods.VonNeumann==(Neighbourhoods)NeighbourBox.SelectedItem);
            var b = ic.ShowDialog();
            if (!b.HasValue || !b.Value) return;
            _ic = ic.Condition;
            NotifyPropertyChanged("NoError");
        }

        private void Color_OnClick(object sender, RoutedEventArgs e)
        {
           var cp = new ColorPicker(_colorPickerIndex);
            var b = cp.ShowDialog();
            if (b.HasValue && b.Value)
            {
                _colorPickerIndex = cp.Id;
                
            }
        }

        private void Font_OnClick(object sender, RoutedEventArgs e)
        {
           var fp = new FontPicker(SPDAssets.GetFont());
           fp.ShowDialog();
          
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }

}