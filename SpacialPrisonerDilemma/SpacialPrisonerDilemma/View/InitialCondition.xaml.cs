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
using Microsoft.Win32;
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
            ComboBox.ItemsSource = Enum.GetValues(typeof (WhenBetray)).Cast<WhenBetray>();
            ComboBox.SelectedIndex = 0;
            this.DataContext = this;
            ConditionNames = new List<string>();
            Conditions = new Dictionary<string, InitialConditions>();
            var ic1 = InitialConditions.DonutFactory();
            var ic2 = InitialConditions.DonutFactory(true);
            ConditionNames.AddRange(new[]{ic1.Name,ic2.Name});
            Conditions.Add(ic1.Name, ic1);
            Conditions.Add(ic2.Name, ic2);
            ic1 = InitialConditions.CircleFactory();
            ic2 = InitialConditions.CircleFactory(true);
            ConditionNames.AddRange(new[] { ic1.Name, ic2.Name });
            Conditions.Add(ic1.Name, ic1);
            Conditions.Add(ic2.Name, ic2);
            ic1 = InitialConditions.DiagonalFactory();
            ic2 = InitialConditions.DiagonalFactory(true);
            ConditionNames.AddRange(new[] { ic1.Name, ic2.Name });
            Conditions.Add(ic1.Name, ic1);
            Conditions.Add(ic2.Name, ic2);
            ComboBox_Copy.ItemsSource = ConditionNames;
            var image2 = new Image()
            {
                Source = SPDBrushes.GenerateLegend(Legend.Height)
            };
       
            Legend.Children.Add(image2);
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
            Condition = InitialConditions.GenerateRandom((int)RandomSize.Value);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            this.Close();
        }

        private double scale = 1;
        private int X = 0, Y = 0;

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Condition = Conditions[ConditionNames[ComboBox_Copy.SelectedIndex]].GetCopy();
        }


        private void Canvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!(ChangeCell.IsChecked.HasValue && ChangeCell.IsChecked.Value)) return;
            if(ComboBox.SelectedIndex<0) return;
            Point P = e.GetPosition(Canvas);
            InitialConditions IC = Condition;
            double X =this.X + scale*P.X/(Canvas.Width/(IC.grid.CellGrid.GetLength(0)));
            if (X >= IC.grid.CellGrid.GetLength(0) ) return;
            double Y =this.Y + scale*P.Y/(Canvas.Height/(IC.grid.CellGrid.GetLength(1)));
            if (Y >= IC.grid.CellGrid.GetLength(1) ) return;
            IC.grid.CellGrid[(int) X, (int) Y].Value = ComboBox.SelectedIndex;
            Condition = IC;
        }

        private void Canvas_OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Condition == null) return;
            var IC = Condition;
            Point P = e.GetPosition(Canvas);
            double X = this.X + scale * P.X / (Canvas.Width / (IC.grid.CellGrid.GetLength(0)));
            if (X >= IC.grid.CellGrid.GetLength(0)) return;
            double Y = this.Y + scale * P.Y / (Canvas.Height / (IC.grid.CellGrid.GetLength(1)));
            if (Y >= IC.grid.CellGrid.GetLength(1)) return;
            scale += Math.Sign(-e.Delta)*0.1;
            if (scale < 0.1) scale = 0.1;
            if (scale > 1) scale = 1;
           
            int width = Condition.grid.CellGrid.GetLength(0);
            int height = Condition.grid.CellGrid.GetLength(1);
            var nwidth = (int) (((double) width)*scale);
            var nheight = (int) (((double) height)*scale);
            int x = (int) X - (nwidth/2);
            int y = (int) Y
                    - (nheight/2);
            int xx = (int) X + (nwidth/2);
            int yy = (int) Y + (nheight/2);
          
            if (xx >= width) x -= (xx - width) + 1;
            if (yy >= height) y -= (yy - height) + 1;
            if (x < 0) x = 0;
            if (y < 0) y = 0;
            this.X = x;
            this.Y = y;
            Condition = IC;
        }

        private void ButtonBase_OnClick1(object sender, RoutedEventArgs e)
        {
            BinaryFormatter bf = new BinaryFormatter();
            OpenFileDialog ofd = new OpenFileDialog {Filter = "Initial Condition File (*.cic)|*.cic"};
            ofd.Multiselect = false;
            
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FileStream fs = new FileStream(ofd.FileName,FileMode.Open);
                var obj = bf.Deserialize(fs);
                Condition = obj as InitialConditions;
            }
        }

        private void ButtonBase_OnClick2(object sender, RoutedEventArgs e)
        {
            if (Condition == null) return;
            BinaryFormatter bf = new BinaryFormatter();
            SaveFileDialog ofd = new SaveFileDialog { Filter = "Initial Condition File (*.cic)|*.cic" };
            var result = ofd.ShowDialog();
            if (result.HasValue && result.Value)
            {
                FileStream fs = new FileStream(ofd.FileName, FileMode.Create);
                bf.Serialize(fs,Condition);
               
            }
        }
    }
}
