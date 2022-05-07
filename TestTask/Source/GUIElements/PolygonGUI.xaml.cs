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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TestTask.Source.Components;

namespace TestTask.Source.GUIElements
{
    /// <summary>
    /// Interaction logic for PolygonGUI.xaml
    /// </summary>
    public partial class PolygonGUI : UserControl
    {

        public List<PolygonData> selectedItems = new();

        public PolygonGUI()
        {
            InitializeComponent();
        }

        void OnKeyPressedHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox tsender)
                {
                    tsender.MoveFocus(new TraversalRequest(FocusNavigationDirection.Previous));
                }
            }
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
            if (selectedItems.Count > 1 && !selectedItems[1].IsChecked)
                selectedItems.RemoveAt(1);
            if (selectedItems.Count > 0 && !selectedItems[0].IsChecked)
                selectedItems.RemoveAt(0);
            if (selectedItems.Count >= 2)
            {
                checkBox.IsChecked = false;
            }
            var polygons = new List<PolygonData>();
            foreach(var pol in PolTree.Items)
            {
                if (pol is PolygonData tpol)
                {
                    if (tpol.IsChecked)
                        polygons.Add(tpol);
                }
            }
            selectedItems = polygons;
        }
    }
}
