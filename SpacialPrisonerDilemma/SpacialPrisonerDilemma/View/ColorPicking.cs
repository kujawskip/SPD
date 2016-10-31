using System;
using System.Linq;
using System.Windows.Media;
using OxyPlot;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Klasa realizuj¹ca wybór koloru
    /// </summary>
    public class ColorPicking
    {
        private readonly Func<Tuple<int,int>, byte>[] _functions;
        private readonly String _s;
        private int size;
        /// <summary>
        /// Opis wyboru
        /// </summary>
        /// <returns>Tekst zawieraj¹cy opis wyboru</returns>
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

                r = 11;
                g = 0;
                b = 11;

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
            { //podczerwieñ

                r = g = b = 0;

            }

            return new Tuple<int,int,int>(r,g,b);

        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów cytrusowych
        /// </summary>
        /// <returns>Wybór kolorów cytrusowych</returns>
        public static ColorPicking CitrusFactory()
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) ((p.Item2-1-p.Item1)<(p.Item2/2)-1?64*(p.Item2 -1 - p.Item1):(p.Item1==0)?180:(p.Item1==p.Item2/2)?230:250),
                p => (byte) ((p.Item2-1-p.Item1)<(p.Item2/2)+1?255 - 2*p.Item1 + ((p.Item1==p.Item2/2)?10:0) :255 - 60*((p.Item2-1-p.Item1)-p.Item2/2)),
                p => (byte) ((p.Item2-1-p.Item1)<(p.Item2/2)+1?2*p.Item1 -  ((p.Item1==p.Item2/2)?10:0) :200 + 12*((p.Item2-1-p.Item1)-p.Item2/2))
            };
          
            String s = "Kolory cytrusowe";
            return new ColorPicking(f,s);
        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów têczy
        /// </summary>
        /// <returns>Wybór kolorów têczy</returns>
        public static ColorPicking RainbowFactory()
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (((double)kolory_teczy((int)((double)p.Item1/p.Item2*110)).Item1)*(Math.Truncate((double)p.Item1*110/p.Item2))),
                p => (byte) (((double)kolory_teczy((int)((double)p.Item1/p.Item2*110)).Item2)*(Math.Truncate((double)p.Item1*110/p.Item2))),
                p => (byte)(((double)kolory_teczy((int)((double)p.Item1/p.Item2*110)).Item3)*(Math.Truncate((double)p.Item1*110/p.Item2)))
            };
            String s = "Kolory têczy";
            return new ColorPicking(f, s);
        }
        /// <summary>
        /// Metoda factory dla wyboru odcieni szaroœci
        /// </summary>
        /// <returns>Wybór odcieni szaroœci</returns>
        public static ColorPicking GrayScaleFactory()
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (((double) 255*p.Item1/(p.Item2+1))),
                p => (byte) (((double) 255*p.Item1/(p.Item2+1))),
                p => (byte) (((double) 255*p.Item1/(p.Item2+1)))
            };
            String s = "Odcienie szaroœci";
            return new ColorPicking(f, s);
        }
        /// <summary>
        /// Metoda factory dla wyboru odwrotnoœci kolorów standardowych
        /// </summary>
        /// <returns>Wybór odwrotnoœci kolorów standardowych</returns>
        public static ColorPicking ReverseRegularPickingFactory()
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p =>(byte)(255 -  (byte) ((256 - p.Item2*p.Item1 > 255 ? 0 : 255 - p.Item1*p.Item2))),
                p => (byte) (50*p.Item1 > 255 ? 0 :255- 50*p.Item1),
                p => (byte) (255 - 250*p.Item1/p.Item2)
            };





            var s = "Odwrócony standardowy zestaw kolorów";
            return new ColorPicking(f, s);
        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów standardowych
        /// </summary>
        /// <returns>Wybór kolorów standardowych</returns>
        public static ColorPicking RegularPickingFactory()
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (256 - p.Item2*p.Item1 > 255 ? 0 : 255 - p.Item1*p.Item2),
                p => (byte) (50*p.Item1 > 255 ? 255 : 50*p.Item1),
                p => (byte) (25*p.Item1)
            };





            var s = "Standardowy zestaw kolorów";
            return new ColorPicking(f, s);
        }
        private ColorPicking(Func<Tuple<int,int>, byte>[] functions, string name)
        {
            _functions = functions.ToArray();
            _s = name;
            size = SPDAssets.MAX;
        }
        /// <summary>
        /// Metoda generuje oxycolor o indeksie i wg. metody wyboru
        /// </summary>
        /// <param name="i">indeks koloru</param>
        /// <returns>OxyColor</returns>
        public OxyColor GenerateOxyColor(int i)
        {
            var T = GenerateColor(i);
            return OxyColor.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4);
        }
       
        private Tuple<byte, byte, byte, byte> GenerateColor(int i)
        {
            return new Tuple<byte, byte, byte, byte>(_functions[0](new Tuple<int, int>(i, size)), _functions[1](new Tuple<int, int>(i, size)), _functions[2](new Tuple<int, int>(i, size)), _functions[3](new Tuple<int, int>(i, size)));
        }
        /// <summary>
        /// Metoda generuje brush o indeksie i wg. metody wyboru
        /// </summary>
        /// <param name="i">indeks koloru</param>
        /// <returns>Brush</returns>
        internal Brush GenerateBrush(int i)
        {
            var T = GenerateColor(i);
            return new SolidColorBrush(Color.FromArgb(T.Item1, T.Item2, T.Item3, T.Item4));
        }
    }
}