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

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for FontPicker.xaml
    /// </summary>
    public partial class FontPicker : Window
    {
        private String TypeFace;
        public FontPicker(string TF)
        {
            TypeFace = TF;
            InitializeComponent();
            foreach (var V in new[] {"Arial", "Helvetica", "Times New Roman"}) Left.Items.Add(V);
            if (Left.Items.Contains(TF))
            {
                Left.SelectedIndex = Left.Items.IndexOf(TF);
            }
        }

        private void Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           SPDBrushes.ChangeFont(Left.SelectedItem.ToString());
            Canvas.Children.Clear();
            Canvas.Children.Add(new Image() {Source = SPDBrushes.GenerateLegend(Canvas.Height)});
            SPDBrushes.ChangeFont(TypeFace);

        }

        private void Akceptuj_Click(object sender, RoutedEventArgs e)
        {
            TypeFace = Left.SelectedItem.ToString();
            SPDBrushes.ChangeFont(TypeFace);
            DialogResult = true;
            Close();
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
