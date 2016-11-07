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
        static double hue2rgb(double p,double q, double t)
        {

           if(t < 0) t += 1;
            if(t > 1) t -= 1;
            if(t < ((double)1)/6) return p + (q - p) * 6 * t;
            if (t < ((double)1) / 2) return q;
            if (t < ((double)2) / 3) return p + (q - p) * (((double)2) / 3 - t) * 6;
            return p;

        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów cytrusowych
        /// </summary>
        /// <returns>Wybór kolorów cytrusowych</returns>
        public static ColorPicking CitrusFactory(int size)
        {
            Func<Tuple<int, int>, byte>[] f = {
                p => 255,
                p => (byte) 111,
                p => (byte) (p.Item1<p.Item2/2?(p.Item1*((double)510)/(p.Item2)):((double)255)),
                p => (byte) (p.Item1>p.Item2/2?((p.Item1-(p.Item2/2))*((double)510)/(p.Item2)):((double)0))
                
            };
          
            String s = "Kolory cytrusowe";
            return new ColorPicking(f, s, size);
        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów têczy
        /// </summary>
        /// <returns>Wybór kolorów têczy</returns>
        public static ColorPicking RainbowFactory(int size)
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (255*hue2rgb(1,Math.Sqrt(2)-1,((double)p.Item1
                    )/(Math.Sqrt(2)*p.Item2) + ((double)1)/3)),
                p => (byte) (255*hue2rgb(1,Math.Sqrt(2)-1,((double)p.Item1)/(Math.Sqrt(2)*p.Item2))) ,
                p => (byte)(255*hue2rgb(1,Math.Sqrt(2)-1,((double)p.Item1)/(Math.Sqrt(2)*p.Item2) - ((double)1)/3))
            };
            String s = "Kolory têczy";
            return new ColorPicking(f, s, size);
        }
        /// <summary>
        /// Metoda factory dla wyboru odcieni szaroœci
        /// </summary>
        /// <returns>Wybór odcieni szaroœci</returns>
        public static ColorPicking GrayScaleFactory(int size)
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (((double) 255*p.Item1/(p.Item2+1))),
                p => (byte) (((double) 255*p.Item1/(p.Item2+1))),
                p => (byte) (((double) 255*p.Item1/(p.Item2+1)))
            };
            String s = "Odcienie szaroœci";
            return new ColorPicking(f, s, size);
        }
        /// <summary>
        /// Metoda factory dla wyboru odwrotnoœci kolorów standardowych
        /// </summary>
        /// <returns>Wybór odwrotnoœci kolorów standardowych</returns>
        public static ColorPicking ReverseRegularPickingFactory(int size)
        {
            Func<Tuple<int, int>, byte>[] f = {
                p => 255,
                p => (byte) (255 - (p.Item1<p.Item2/2?(p.Item1*((double)510)/(p.Item2)):((double)255))),
                p => (byte) (255-(p.Item1>p.Item2/2?((p.Item1-(p.Item2/2))*((double)510)/(p.Item2)):((double)0))),
                p => (byte) 144
            };





            var s = "Odwrócony standardowy zestaw kolorów";
            return new ColorPicking(f, s, size);
        }
        /// <summary>
        /// Metoda factory dla wyboru kolorów standardowych
        /// </summary>
        /// <returns>Wybór kolorów standardowych</returns>
        public static ColorPicking RegularPickingFactory(int size)
        {
            Func<Tuple<int,int>, byte>[] f = {
                p => 255,
                p => (byte) (p.Item1<p.Item2/2?(p.Item1*((double)510)/(p.Item2)):((double)255)),
                p => (byte) (p.Item1>p.Item2/2?((p.Item1-(p.Item2/2))*((double)510)/(p.Item2)):((double)0)),
                p => (byte) 111
            };





            var s = "Standardowy zestaw kolorów";
            return new ColorPicking(f, s, size);
        }
        private ColorPicking(Func<Tuple<int,int>, byte>[] functions, string name,int _size=SPDAssets.MAX)
        {
            _functions = functions.ToArray();
            _s = name;
            size = _size;
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

        internal void ChangeSize(int p)
        {
            size = p;
            ModifyColors();
        }

        internal void ModifyColors()
        {
            for (int i = 0; i < SPDAssets.MAX; i++)
            {
                SPDAssets.ModifyColor(GenerateBrush(i), GenerateOxyColor(i), i);
            }
        }
    }
}