using System;
using System.Windows;
using System.Windows.Controls;

namespace SpacialPrisonerDilemma.View
{
    /// <summary>
    /// Interaction logic for FontPicker.xaml
    /// </summary>
    public partial class FontPicker
    {
        private String _typeFace;
        /// <summary>
        /// Konstruktor klasy wyboru czcionki
        /// </summary>
        /// <param name="tf">Nazwa aktualnie wybranej czcionki</param>
        public FontPicker(string tf)
        {
            _typeFace = tf;
            InitializeComponent();
            foreach (var v in new[] {"Arial", "Helvetica", "Times New Roman"}) Box.Items.Add(v);
            if (Box.Items.Contains(tf))
            {
                Box.SelectedIndex = Box.Items.IndexOf(tf);
            }
        }

        private void Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           SPDAssets.ChangeFont(Box.SelectedItem.ToString());
            Canvas.Children.Clear();
            var D = SPDAssets.GenerateLegend(Canvas.Height);
            Canvas.Children.Add(D);
            SPDAssets.ChangeFont(_typeFace);

        }

        private void Akceptuj_Click(object sender, RoutedEventArgs e)
        {
            _typeFace = Box.SelectedItem.ToString();
            SPDAssets.ChangeFont(_typeFace);
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
