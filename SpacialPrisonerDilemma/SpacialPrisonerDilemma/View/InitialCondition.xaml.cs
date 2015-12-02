using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
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
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for InitialCondition.xaml
    /// </summary>
    public partial class InitialCondition : Window, INotifyPropertyChanged
    {
        private Dictionary<string, InitialConditions> Conditions;
        private List<string> ConditionNames;
        private InitialConditions _condition;

        public InitialCondition()
        {
            InitializeComponent();
            this.DataContext = this;
            ConditionNames = new List<string>();
            Conditions = new Dictionary<string, InitialConditions>();
        }
        
        public DrawingImage GenerateImage(InitialConditionsGrid Grid, int X, int Y, int Width, int Height)
        {
            double CellWidth = Canvas.Width/Width;
            double CellHeight = Canvas.Height/Height;
            FontStyle fs = FontStyles.Normal;
            FontWeight fw = FontWeights.Normal;
            FontFamily ff = new FontFamily("Arial");
            FontStretch ffs = FontStretches.Normal;
            
           
            DrawingGroup DG = new DrawingGroup();
            for (int i = X; i < X + Width; i++)
            {
                for (int j = Y; j < Y + Height; j++)
                {
                    RectangleGeometry RG = new RectangleGeometry(new Rect(new Point((i - X) * CellWidth, (j - Y) * CellHeight), new Point((i - X + 1) * CellWidth, (j + 1 - Y) * CellHeight)));
                    GeometryDrawing gd = new GeometryDrawing();
                    gd.Brush = SPDBrushes.GetBrush(Condition.grid.CellGrid[i, j].Value);
                    gd.Geometry = RG;
                    DG.Children.Add(gd);
                }
            }
            return new DrawingImage(DG);
        }
        public InitialConditions LoadConditions(string path)
        {
            FileStream fs = new FileStream(path,FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                var obj = bf.Deserialize(fs);
                fs.Close();
                return (InitialConditions)obj;
            }
            catch (Exception)
            {
                fs.Close();

                return null;
            }
         
            
        }

        private void NotifyPropertyChanged(string s)
        {
            List<string> Properties = new List<string>();

          
            foreach (var p in Properties)
            {
                NotifyPropertyChanged(p);
            }
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(s));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public InitialConditions Condition
        {
            get { return _condition; }
            private set { _condition = value;
                UpdateScreen();
            }
        }

        private void UpdateScreen()
        {
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image()
            {
                Source =
                    GenerateImage(Condition.grid, X,Y, (int)(Condition.grid.CellGrid.GetLength(0)*scale),
                        (int)(Condition.grid.CellGrid.GetLength(1)*scale))
               
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Condition = InitialConditions.GenerateRandom();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private double scale = 1;
        private int X = 0, Y = 0;

      
    }
}
