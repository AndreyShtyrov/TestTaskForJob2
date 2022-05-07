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
using System.Collections.ObjectModel;
using TestTask.Source;
using TestTask.Source.Components;

namespace TestTask
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Polyline rectangle3 = null;
        
        private readonly List<Ellipse> ellipses = new();
        public MainWindow()
        {
            InitializeComponent();
            PolygonTreeView.DataContext = Controller.instance.PolygonDatas;
            MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton is not MouseButtonState.Pressed)
                return;
            var pos = e.GetPosition(Field);
            if (pos.X < 0 || pos.Y < 0 || pos.X > Field.ActualWidth || pos.Y > Field.ActualHeight)
                return;
            var selected = PolygonTreeView.PolTree.SelectedItem;
            if (selected is PolygonData tselected && !tselected.CheckNewBoundCrossPrevBorders((int)pos.X, (int)pos.Y))
                tselected.AddNode((int)pos.X, (int)pos.Y);
        }

        private void CrosPoints_Click(object sender, RoutedEventArgs e)
        {
            if (PolygonTreeView.selectedItems.Count != 2)
            {
                MessageBox.Show("You should choose two polygons");
                return;
            }
            var pointsLines = Controller.instance.CalculateCrossPoints(PolygonTreeView.selectedItems);
            var points = pointsLines.Item1;
            foreach (var point in points)
            {
                var ellipse = new Ellipse
                {
                    Stroke = new SolidColorBrush(Colors.Blue),
                    Width = 4,
                    Height = 4
                };
                Canvas.SetLeft(ellipse, (int)point.X - 2);
                Canvas.SetTop(ellipse, (int)point.Y - 2);
                Field.Children.Add(ellipse);
                ellipses.Add(ellipse);
            }
            foreach (var bound in pointsLines.Item2)
            {
                Field.Children.Add(new Line
                {
                    X1 = bound.Left.X,
                    X2 = bound.Right.X,
                    Y1 = bound.Left.Y,
                    Y2 = bound.Right.Y,
                    Stroke = new SolidColorBrush(Colors.Yellow)
                });
            }
            for (var i = 0; i < pointsLines.Item2.Count; i++)
            {
                var bound = pointsLines.Item2[i];
                var label = new Label();
                Canvas.SetLeft(label, bound.Left.X + 8);
                Canvas.SetTop(label, bound.Left.Y);
                label.Content = pointsLines.Item3[i].ToString();
                Field.Children.Add(label);
            }
        }

        private void CreatCrosPol_Click(object sender, RoutedEventArgs e)
        {
            if (PolygonTreeView.selectedItems.Count != 2)
            {
                MessageBox.Show("You Should choose two polgydon");
                return;
            }
            if (!PolygonTreeView.selectedItems[0].IsClosed && !PolygonTreeView.selectedItems[1].IsClosed)
            {
                MessageBox.Show("Both polygons should be closed");
                return;
            }
            var newPol = Controller.instance.CreateCrossPolygon(PolygonTreeView.selectedItems);
            if (newPol.Nodes.Count == 0)
                return;
            var points = new PointCollection();
            foreach (var node in newPol.Nodes)
                points.Add(new Point(node.X, node.Y));
            
            points.Add(new Point(newPol.Nodes[0].X, newPol.Nodes[0].Y));
            if (rectangle3 != null)
                Field.Children.Remove(rectangle3);
            rectangle3 = new Polyline
            {
                Stroke = new SolidColorBrush(Colors.Pink),
                StrokeThickness = 3,
                Points = points
            };
            Field.Children.Add(rectangle3);
        }

        private void CreateUnitPol_Click(object sender, RoutedEventArgs e)
        {
            if (PolygonTreeView.selectedItems.Count != 2)
            {
                MessageBox.Show("You Should choose two polgydon");
                return;
            }
            if (!PolygonTreeView.selectedItems[0].IsClosed && !PolygonTreeView.selectedItems[1].IsClosed)
            {
                MessageBox.Show("Both polygons should be closed");
                return;
            }
            var newPol = Controller.instance.CreateUnitPolygon(PolygonTreeView.selectedItems);
            if (newPol.Nodes.Count == 0)
                return;
            var points = new PointCollection();
            foreach (var node in newPol.Nodes)
                points.Add(new Point(node.X, node.Y));

            points.Add(new Point(newPol.Nodes[0].X, newPol.Nodes[0].Y));
            if (rectangle3 != null)
                Field.Children.Remove(rectangle3);
            rectangle3 = new Polyline
            {
                Stroke = new SolidColorBrush(Colors.Pink),
                StrokeThickness = 3,
                Points = points
            };
            Field.Children.Add(rectangle3);
        }

        private void CalculateS1_Click(object sender, RoutedEventArgs e)
        {
            var firstPolygon = Controller.instance.PolygonDatas[0];
            if (firstPolygon is null || !firstPolygon.IsClosed)
                return;
            var s = firstPolygon.CalculateS();
            SqLabel.Content = s.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var idx = Controller.instance.PolygonDatas.Count;
            var polygonData = new PolygonData(Controller.GenerateBush(idx));
            Controller.instance.PolygonDatas.Add(polygonData);
            Field.Children.Add(polygonData.GUIView);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Field.Children.Clear();
            Controller.instance.PolygonDatas.Clear();
            rectangle3 = null;
            PolygonTreeView.DataContext = Controller.instance.PolygonDatas;
            PolygonTreeView.selectedItems.Clear();
        }

        private void DeleteCrosPoints_Click(object sender, RoutedEventArgs e)
        {
            foreach(var ellipse in ellipses)
            {
                Field.Children.Remove(ellipse);
            }
            ellipses.Clear();
            if (rectangle3 != null)
                Field.Children.Remove(rectangle3);
            rectangle3 = null;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Controller.instance.Update();
        }
    }
}
