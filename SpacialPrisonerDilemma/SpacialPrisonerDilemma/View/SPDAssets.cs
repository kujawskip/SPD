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
    /// Klasa obs³uguj¹ca elementy pomocnicze dla okien programu
    /// </summary>
    internal static class SPDAssets
    {
        private static Brush[] _brushArray;
        private static OxyColor[] _oxyArray;

        public static List<Image> GetBrushRectangles()
        {
            return _brushArray.Select(s =>
            {
                var rg = new RectangleGeometry(new Rect(new Point(0, 0), new Point(30, 15)));
                var gd = new GeometryDrawing(s, new Pen(s, 1), rg);
                var di = new DrawingImage(gd);
                return new Image() { Source = di };
            }).ToList();
        }
        public static OxyColor GetOxyColor(int p)
        {
            return _oxyArray[p];
        }
        public static void CreateBrushes(int count)
        {

            _brushArray = new Brush[count];
            _oxyArray = new OxyColor[count];
            for (var p = 0; p < count; p++)
            {
                _brushArray[p] = new SolidColorBrush(Color.FromRgb((byte)(256 - 15 * p > 255 ? 0 : 255 - 10 * p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p)));
                _oxyArray[p] = OxyColor.FromRgb((byte)(256 - 15 * p > 255 ? 0 : 255 - 10 * p), (byte)(50 * p > 255 ? 255 : 50 * p), (byte)(25 * p));
            }
        }

        public static void ModifyColor(Brush b, OxyColor o, int i)
        {
            _brushArray[i] = b;
            _oxyArray[i] = o;
        }
        private static string _font;
        public static void ChangeFont(string fontName)
        {
            _font = fontName;
        }

        public static string GetFont()
        {
            return _font;
        }
        public static Brush GetBrush(int p)
        {
            return _brushArray[p];


        }
        private static string[] _descriptions;

        public static void InitialiseDescriptions()
        {
            var des = new List<string> {"Zawsze zdradzaj"};
            for (var i = 1; i < 9; i++)
            {
                des.Add(string.Format("Zdradzaj gdy {0} s¹siad{1} zdradza", i, i == 1 ? "" : "ów"));
            }
            des.Add("Zawsze wybaczaj");
            _descriptions = des.ToArray();
        }

        public static DrawingImage GenerateLegend(double height)
        {

            var dg = new DrawingGroup();
            for (var i = 0; i < _descriptions.Length; i++)
            {
                var rg = new RectangleGeometry(new Rect(new Point(0,8*i+ i * (height / _descriptions.Length)), new Point(20,8*i+ (i + 1) * (height / _descriptions.Length))));
                var text = new FormattedText(_descriptions[i],
                    CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(_font),
                    15,
                    Brushes.Black);
                var gd = new GeometryDrawing
                {
                    Brush = GetBrush(i),
                    Geometry = rg,
                    Pen = new Pen(Brushes.Black, 2)
                };
                var gd2 = new GeometryDrawing
                {
                    Pen = new Pen(Brushes.Black, 2),
                    Brush = Brushes.Black,
                    Geometry = text.BuildGeometry(new Point(22,8*i+ i*(height/_descriptions.Length)))
                };
                dg.Children.Add(gd);
                dg.Children.Add(gd2);
            }
            var d = new DrawingImage(dg);
            d.Freeze();
            return d;
        }
    }
}