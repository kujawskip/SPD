using System;
using System.Linq;
using System.Windows.Media;
using OxyPlot;

namespace SpacialPrisonerDilemma.View
{
    public class ColorPicking
    {
        private readonly Func<int, byte>[] _functions;
        private readonly String _s;
        public override string ToString()
        {
            return _s;
        }
        static Tuple<int, int, int> kolory_teczy(int l)
        {

            int r, g, b;

            double d = 256.0 / 20.0;

            if (l < 0)
            { //nadfiolet

                r = g = b = 0;

            }
            else if (l < 20)
            {

                r = (int)(255 - d * l);

                g = 0;

                b = 255;

            }
            else if (l < 40)
            {

                r = 0;

                g =(int)( d * (l - 20));

                b = 255;

            }
            else if (l < 60)
            {

                r = 0;

                g = 255;

                b = (int)(255 - d * (l - 40));

            }
            else if (l < 80)
            {

                r = (int)(d * (l - 60));

                g = 255;

                b = 0;

            }
            else if (l < 100)
            {

                r = 255;

                g = (int)(255 - d * (l - 80));

                b = 0;

            }
            else
            { //podczerwie�

                r = g = b = 0;

            }

            return new Tuple<int,int,int>(r,g,b);

        }

        public static ColorPicking CitrusFactory()
        {
            Func<int, byte>[] f = {
                p => 255,
                p => (byte) ((9-p)<4?64*(9-p):(p==0)?180:250),
                p => (byte) ((9-p)<6?255:255 - 60*((9-p)-5)),
                p => (byte) ((9-p)<6?0:200 + 12*((9-p)-5))
            };
          
            String s = "Kolory cytrusowe";
            return new ColorPicking(f,s);
        }
        public static ColorPicking RainbowFactory()
        {
            Func<int, byte>[] f = {
                p => 255,
                p => (byte) kolory_teczy(p*11).Item1,
                p => (byte) kolory_teczy(p*11).Item2,
                p => (byte) kolory_teczy(p*11).Item3
            };
            String s = "Kolory t�czy";
            return new ColorPicking(f, s);
        }
        public static ColorPicking GrayScaleFactory()
        {
            Func<int, byte>[] f = {
                p => 255,
                p => (byte) (((double) 255*p/11)),
                p => (byte) (((double) 255*p/11)),
                p => (byte) (((double) 255*p/11))
            };
            String s = "Odcienie szaro�ci";
            return new ColorPicking(f, s);
        }
        public static ColorPicking ReverseRegularPickingFactory()
        {
            Func<int, byte>[] f = {
                p => 255,
                p =>(byte)(255 -  (byte) (256 - 15*p > 255 ? 30 : 255 - 10*p)),
                p => (byte) (50*p > 255 ? 0 :255- 50*p),
                p => (byte) (255 - 25*p)
            };





            var s = "Odwr�cony standardowy zestaw kolor�w";
            return new ColorPicking(f, s);
        }
        public static ColorPicking RegularPickingFactory()
        {
            Func<int, byte>[] f = {
                p => 255,
                p => (byte) (256 - 15*p > 255 ? 0 : 255 - 10*p),
                p => (byte) (50*p > 255 ? 255 : 50*p),
                p => (byte) (25*p)
            };





            var s = "Standardowy zestaw kolor�w";
            return new ColorPicking(f, s);
        }
        private ColorPicking(Func<int, byte>[] functions, string name)
        {
            _functions = functions.ToArray();
            _s = name;
        }

        public OxyColor GenerateOxyColor(int i)
        {
            var T = GenerateColor(i);
            return OxyColor.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4);
        }
        private Tuple<byte, byte, byte, byte> GenerateColor(int i)
        {
            return new Tuple<byte, byte, byte, byte>(_functions[0](i), _functions[1](i), _functions[2](i), _functions[3](i));
        }

        internal Brush GenerateBrush(int i)
        {
            var T = GenerateColor(i);
            return new SolidColorBrush(Color.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4));
        }
    }
}