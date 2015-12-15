using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SpacialPrisonerDilemma.View;

namespace SpacialPrisonerDilemma
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private ValidationErrors _error;
        private int ColorPickerIndex = 0;
        private InitialConditions ic;
        public enum ValidationErrors
        {
            None,
            ParseError,
            ValueError
        }
        
        private bool canvalidate = false;
        public MainWindow()
        {

            InitializeComponent();
            this.DataContext = this;
            Error = ValidationErrors.None;
            canvalidate = true;
            SPDBrushes.CreateBrushes(10);
            SPDBrushes.ChangeFont("Arial");
            SPDBrushes.InitialiseDescriptions();

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

        private static readonly string[] ErrorMessages = new string[] { "", "Błąd przetwarzania", "Błąd wartości" };

        public string ErrorMessage
        {
            get { return ErrorMessages[(int)Error]; }
        }

        public bool NoError
        {
            get { return Error == ValidationErrors.None && ic != null; }
        }

        public bool ShowErrorMessage
        {
            get { return !NoError; }
        }
        private void NotifyPropertyChanged(string s)
        {
            List<string> Properties = new List<string>();

            if (s == "Error")
            {
                Properties.Add("ErrorMessage");
                Properties.Add("NoError");

            }
            if (s == "NoError")
            {
                Properties.Add("ShowErrorMessage");
            }
            foreach (var p in Properties)
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
        private bool validatesToPair(string text)
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
            if (!validatesToPair(text)) return false;
            var array = text.Substring(1, text.Length - 2).Split(',');
            bool flag = double.TryParse(array[0].Trim(), out var1);
            if (!flag) return false;
            flag = double.TryParse(array[1].Trim(), out var2);
            if (!flag) return false;
            return true;

        }
        private void TryValidate()
        {
            if (!canvalidate) return;
            Validate();

        }

        private double[] Validate()
        {
            double[] array = new double[6];
            var sarray = new string[] {BothBetray.Text, FirstBetrays.Text, NobodyBetrays.Text,SecondBetrays.Text};
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
            // array[3] = x2 array[4] = x1 array[2] = x3 array[1] = x4
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
            if (validatesToPair(SecondBetrays.Text))
            {
                FirstBetrays.Text = SwitchText(SecondBetrays.Text);
            }
        }

        private void FirstBetrays_OnLostFocus(object sender, RoutedEventArgs e)
        {
            if (validatesToPair(FirstBetrays.Text))
            {
                SecondBetrays.Text = SwitchText(FirstBetrays.Text);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            double[] d = Validate();
            SPD spd = new SPD(d, Transform(ic.grid));
            spd.ShowDialog();
        }

        public int[,] Transform(InitialConditionsGrid ig)
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
            InitialCondition IC = new InitialCondition();
            var b = IC.ShowDialog();
            if (b.HasValue && b.Value)
            {
                ic = IC.Condition;
                NotifyPropertyChanged("NoError");
            }

        }

        private void Color_OnClick(object sender, RoutedEventArgs e)
        {
           ColorPicker CP = new ColorPicker(ColorPickerIndex);
            var b = CP.ShowDialog();
            if (b.HasValue && b.Value)
            {
                ColorPickerIndex = CP.ID;
                
            }
        }

        private void Font_OnClick(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
