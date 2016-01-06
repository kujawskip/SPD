using System;
using System.Windows;
using System.Windows.Controls;
using SpacialPrisonerDilemma.Model;

namespace SpacialPrisonerDilemma.View
{

    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker
    {
        public int Id { get; set; }
        private int _id;
        public ColorPicker(int index)
        {
            InitializeComponent();
            Box.Items.Add(ColorPicking.RegularPickingFactory());
            Box.Items.Add(ColorPicking.ReverseRegularPickingFactory());
            Box.Items.Add(ColorPicking.GrayScaleFactory());
            Box.Items.Add(ColorPicking.RainbowFactory());
            Box.Items.Add(ColorPicking.CitrusFactory());
            Box.SelectedIndex = index;
            Id = index;
            _id = index;
        }

        public void ChangeColors(ColorPicking p)
        {
            for (int i = 0; i < Enum.GetValues(typeof(WhenBetray)).Length; i++)
            {
                SPDAssets.ModifyColor(p.GenerateBrush(i), p.GenerateOxyColor(i), i);
            }

        }
        private void Akceptuj_Click(object sender, RoutedEventArgs e)
        {
            Id = _id;

            ChangeColors(Box.Items[Id] as ColorPicking);
            DialogResult = true;
            Close();
        }

        private void Anuluj_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Left_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeColors(Box.SelectedItem as ColorPicking);
            _id = Box.SelectedIndex;
            Image I = new Image { Source = SPDAssets.GenerateLegend(Canvas.Height) };
            Canvas.Children.Clear();
            Canvas.Children.Add(I);
            ChangeColors(Box.Items[Id] as ColorPicking);
        }
    }
}
