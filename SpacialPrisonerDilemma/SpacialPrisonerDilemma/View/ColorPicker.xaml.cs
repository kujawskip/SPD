using System.Windows;
using System.Windows.Controls;

namespace SpacialPrisonerDilemma.View
{

    /// <summary>
    /// Interaction logic for ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker
    {
        public int Id { get; set; }
        private int _id;
        private readonly int _stateCount;
        public ColorPicker(int index,int stateCount)
        {
            InitializeComponent();
            _stateCount = stateCount;
            Box.Items.Add(ColorPicking.RegularPickingFactory(stateCount));
            Box.Items.Add(ColorPicking.ReverseRegularPickingFactory(stateCount));
            Box.Items.Add(ColorPicking.GrayScaleFactory(stateCount));
            Box.Items.Add(ColorPicking.RainbowFactory(stateCount));
            Box.Items.Add(ColorPicking.CitrusFactory(stateCount));
            Box.SelectedIndex = index;
            
            Id = index;
            _id = index;
        }

        public void ChangeColors(ColorPicking p)
        {
            p.ModifyColors();

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
            var D = SPDAssets.GenerateLegend(Canvas.Height, _stateCount);
           
            Canvas.Children.Clear();
            Canvas.Children.Add(D);
            ChangeColors(Box.Items[Id] as ColorPicking);
        }
    }
}
