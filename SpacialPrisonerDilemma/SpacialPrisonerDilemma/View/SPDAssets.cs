using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Klasa obsługująca elementy pomocnicze dla okien programu
    /// </summary>
    internal static class SPDAssets
    {
        private static Brush[] _brushArray;
        private static OxyColor[] _oxyArray;
        public const int MAX = 62;
        /// <summary>
        /// Metoda zwraca prostokąty zawierające kolory wykresu
        /// </summary>
        /// <returns>Lista prostokątów</returns>
		public static List<Image> GetBrushRectangles(int count=MAX)
        {
			return GetBrushRectangles(count,(x)=>(x));
        }
		 /// <summary>
        /// Metoda zwraca prostokąty zawierające kolory wykresu
        /// </summary>
		/// <param name="stateCount"> Ilość kolorów </param>
		/// <param name="SF">Predykat wyboru kolorów</param>"
        /// <returns>Lista prostokątów</returns>
        public static List<Image> GetBrushRectangles(int stateCount,StateTransformation SF)
        {
			List<Brush> brushes = new List<Brush>();
			for(int i=0;i<stateCount;i++) brushes.Add(_brushArray[SF(i)]);
            return brushes.Select(s =>
            {
                var rg = new RectangleGeometry(new Rect(new Point(0, 0), new Point(30, 15)));
                var gd = new GeometryDrawing(s, new Pen(s, 1), rg);
                var di = new DrawingImage(gd);
                return new Image { Source = di };
            }).ToList();
        }
        /// <summary>
        /// Metoda zwraca OxyColor dla strategii o indeksie p
        /// </summary>
        /// <param name="p">indeks strategii</param>
        /// <returns>OxyColor zawierający kolor dla tej strategii</returns>
        public static OxyColor GetOxyColor(int p)
        {
            return _oxyArray[p];
        }
        /// <summary>
        /// Metoda inicjalizująca generująca pędzle i OxyColory
        /// </summary>
        /// <param name="count">Ilość kolorów do wygenerowania</param>
        public static void CreateBrushes(int count=MAX)
        {

            _brushArray = new Brush[count];
            _oxyArray = new OxyColor[count];
            for (var p = 0; p < count; p++)
            {
                _brushArray[p] = new SolidColorBrush(Color.FromRgb((byte)(256 - 15 * p > 255 ? 0 : 255 - 10 * p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p)));
                _oxyArray[p] = OxyColor.FromRgb((byte)(256 - 15 * p > 255 ? 0 : 255 - 10 * p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p));
            }
        }
        /// <summary>
        /// Metoda modyfikuje kolor o indeksie i przypisując mu brush i oxycolor
        /// </summary>
        /// <param name="b">brush</param>
        /// <param name="o">oxycolor</param>
        /// <param name="i">indeks</param>
        public static void ModifyColor(Brush b, OxyColor o, int i)
        {
            _brushArray[i] = b;
            _oxyArray[i] = o;
        }
        private static string _font;
        /// <summary>
        /// Metoda zmienia czcionke na czcionkę o podanej nazwie
        /// </summary>
        /// <param name="fontName">nazwa czcionki</param>
        public static void ChangeFont(string fontName)
        {
            _font = fontName;
        }
        /// <summary>
        /// Metoda zwraca nazwe aktualnej czcionki
        /// </summary>
        /// <returns>nazwa czcionki</returns>
        public static string GetFont()
        {
            return _font;
        }
        /// <summary>
        /// Metoda zwraca brush i indeksie p
        /// </summary>
        /// <param name="p">indeks brusha</param>
        /// <returns>Brush o indeksie p</returns>
        public static Brush GetBrush(int p)
        {
            return _brushArray[p];


        }
        private static string[] _descriptions;
        /// <summary>
        /// Metoda inicjalizuje opisy strategii
        /// </summary>
        public static void InitialiseDescriptions()
        {
            var des = new List<string> {"Zawsze zdradzaj"};
            for (var i = 1; i < MAX-1; i++)
            {
                des.Add(string.Format("Zdradzaj gdy {0} sąsiad{1} zdradza", i, i == 1 ? "" : "ów"));
            }
            des.Add("Zawsze wybaczaj");
            _descriptions = des.ToArray();
        }
		 /// <summary>
        /// Metoda generuje obrazek legendy
        /// </summary>
        /// <param name="height">Wysokość canvasa dla legendy</param>
		/// <param name="stateCount">Ilość kolorów</param>
		/// <param name="SF">Predykat wyboru kolorów</param>
        /// <returns>Obrazek legendy</returns>
        public static Image GenerateLegend(double height,int stateCount,StateTransformation SF)
        {

            var dg = new DrawingGroup();
            var text = new FormattedText(_descriptions[SF(0)],
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(_font),
                    (-3.0 * stateCount + 810) / 52,
                    Brushes.Black);
            var gd2 = new GeometryDrawing
            {
                Pen = new Pen(Brushes.Black, 1),
                Brush = Brushes.Black,
                Geometry = text.BuildGeometry(new Point(22, 0))
            };
            dg.Children.Add(gd2);
            text = new FormattedText(_descriptions[SF(stateCount - 1)],
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(_font),
                    (-3.0 * stateCount + 810) / 52,
                    Brushes.Black);
            gd2 = new GeometryDrawing
            {
                Pen = new Pen(Brushes.Black, 1),
                Brush = Brushes.Black,
                Geometry = text.BuildGeometry(new Point(22, (stateCount - 1)* (height / stateCount)))
            };
            dg.Children.Add(gd2);
            for (var i = 0; i < stateCount; i++)
            {
                var rg = new RectangleGeometry(new Rect(new Point(0,i * (height / stateCount)), new Point(20,(i + 1) * (height / stateCount))));
                
                var gd = new GeometryDrawing
                {
                    Brush = GetBrush(SF(i)),
                    Geometry = rg,
                    Pen = new Pen(Brushes.Black, 0)
                };
              
                dg.Children.Add(gd);
                
            }
            var d = new DrawingImage(dg);
            d.Freeze();
		     return new Image()
		     {
		         Source = d,
                 
		     };
            
        }
        /// <summary>
        /// Metoda generuje obrazek legendy
        /// </summary>
        /// <param name="height">Wysokość canvasa dla legendy</param>
        /// <returns>Obrazek legendy</returns>
        public static Image GenerateLegend(double height)
        {

            return GenerateLegend(height,MAX,x=>x);
        }

        public static Image GenerateLegend(double height, int stateCount)
        {
            return GenerateLegend(height,stateCount,(x)=>(x>=(stateCount-1)?SPDAssets.MAX-1:x))
            ;
        }
    }
}