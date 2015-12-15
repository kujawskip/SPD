using System;
using System.Collections.Generic;
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
using OxyPlot;
using OxyPlot.Series;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{

    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : Window
    {
        public int ID { get; set; }
        private int id;
        public ColorPicker(int index)
        {
            InitializeComponent();
            Left.Items.Add(ColorPicking.RegularPickingFactory());
            Left.Items.Add(ColorPicking.ReverseRegularPickingFactory());
            Left.Items.Add(ColorPicking.GrayScaleFactory());
            Left.Items.Add(ColorPicking.RainbowFactory());
            Left.Items.Add(ColorPicking.CitrusFactory());
            Left.SelectedIndex = index;
            ID = index;
            id = index;
        }

        public void ChangeColors(ColorPicking p)
        {
            for (int i = 0; i < Enum.GetValues(typeof(WhenBetray)).Length; i++)
            {
                SPDBrushes.ModifyColor(p.GenerateBrush(i), p.GenerateOxyColor(i), i);
            }

        }
        private void Akceptuj_Click(object sender, RoutedEventArgs e)
        {
            ID = id;

            ChangeColors(Left.Items[ID] as ColorPicking);
            this.DialogResult = true;
            this.Close();
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeColors(Left.SelectedItem as ColorPicking);
            id = Left.SelectedIndex;
            Image I = new Image() { Source = SPDBrushes.GenerateLegend(Canvas.Height) };
            Canvas.Children.Clear();
            Canvas.Children.Add(I);
            ChangeColors(Left.Items[ID] as ColorPicking);
        }
    }
    public class ColorPicking
    {
        private Func<int, byte>[] Functions;
        private String S;
        public override string ToString()
        {
            return S;
        }
        static Tuple<int, int, int> kolory_teczy(int L)
        {

            int r, g, b;

            double d = 256.0 / 20.0;

            if (L < 0)
            { //nadfiolet

                r = g = b = 0;

            }
            else if (L < 20)
            {

                r = (int)(255 - d * L);

                g = 0;

                b = 255;

            }
            else if (L < 40)
            {

                r = 0;

                g =(int)( d * (L - 20));

                b = 255;

            }
            else if (L < 60)
            {

                r = 0;

                g = 255;

                b = (int)(255 - d * (L - 40));

            }
            else if (L < 80)
            {

                r = (int)(d * (L - 60));

                g = 255;

                b = 0;

            }
            else if (L < 100)
            {

                r = 255;

                g = (int)(255 - d * (L - 80));

                b = 0;

            }
            else
            { //podczerwień

                r = g = b = 0;

            }

            return new Tuple<int,int,int>(r,g,b);

        }

        public static ColorPicking CitrusFactory()
        {
            Func<int, byte>[] F = new Func<int, byte>[]
            {
                p => 255,
                p => (byte) ((9-p)<4?64*(9-p):(p==0)?180:250),
                p => (byte) ((9-p)<6?255:255 - 60*((9-p)-5)),
                p => (byte) ((9-p)<6?0:200 + 12*((9-p)-5))
            };
          
            String S = "Kolory cytrusowe";
            return new ColorPicking(F,S);
        }
        public static ColorPicking RainbowFactory()
        {
            Func<int, byte>[] F = new Func<int, byte>[]
            {
                p => 255,
                p => (byte) kolory_teczy(p*11).Item1,
                p => (byte) kolory_teczy(p*11).Item2,
                p => (byte) kolory_teczy(p*11).Item3
            };
            String S = "Kolory tęczy";
            return new ColorPicking(F, S);
        }
        public static ColorPicking GrayScaleFactory()
        {
            Func<int, byte>[] F = new Func<int, byte>[]
            {
                p => 255,
                p => (byte) (((double) 255*p/11)),
                p => (byte) (((double) 255*p/11)),
                p => (byte) (((double) 255*p/11))
            };
            String S = "Odcienie szarości";
            return new ColorPicking(F, S);
        }
        public static ColorPicking ReverseRegularPickingFactory()
        {
            Func<int, byte>[] F = new Func<int, byte>[]
            {
                p => 255,
                p =>(byte)(255 -  (byte) (256 - 15*p > 255 ? 30 : 255 - 10*p)),
                p => (byte) (50*p > 255 ? 0 :255- 50*p),
                p => (byte) (255 - 25*p)
            };





            var S = "Odwrócony standardowy zestaw kolorów";
            return new ColorPicking(F, S);
        }
        public static ColorPicking RegularPickingFactory()
        {
            Func<int, byte>[] F = new Func<int, byte>[]
            {
                p => 255,
                p => (byte) (256 - 15*p > 255 ? 0 : 255 - 10*p),
                p => (byte) (50*p > 255 ? 255 : 50*p),
                p => (byte) (25*p)
            };





            var S = "Standardowy zestaw kolorów";
            return new ColorPicking(F, S);
        }
        private ColorPicking(Func<int, byte>[] Functions, string name)
        {
            this.Functions = Functions.ToArray();
            S = name;
        }

        public OxyColor GenerateOxyColor(int i)
        {
            var T = GenerateColor(i);
            return OxyColor.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4);
        }
        private Tuple<byte, byte, byte, byte> GenerateColor(int i)
        {
            return new Tuple<byte, byte, byte, byte>(Functions[0](i), Functions[1](i), Functions[2](i), Functions[3](i));
        }

        internal Brush GenerateBrush(int i)
        {
            var T = GenerateColor(i);
            return new SolidColorBrush(Color.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4));
        }
    }
}
