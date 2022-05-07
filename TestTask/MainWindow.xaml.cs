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
        private Polyline rectangle3;
        
        private List<Ellipse> ellipses = new List<Ellipse>();
        public MainWindow()
        {
            InitializeComponent();
            rectangle3 = new Polyline();
            Field.Children.Add(rectangle3);
            PolygonTreeView.DataContext = Controller.instance.PolygonDatas;
            MouseDown += OnMouseDown;
        }

        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton is MouseButtonState.Pressed)
            {
                var pos = e.GetPosition(Field);
                if (pos.X < 0 || pos.Y < 0)
                    return;
                if (pos.X > Field.ActualWidth || pos.Y > Field.ActualHeight)
                    return;
                var selected = PolygonTreeView.PolTree.SelectedItem;
                if (selected == null)
                    return;
                if (selected is PolygonData tselected)
                {
                    if (tselected.CheckNewBoundCrossPrevBorders((int)pos.X, (int)pos.Y))
                        return;
                    tselected.AddNode((int)pos.X, (int)pos.Y);
                }
                    
            }
        }

        private void CrosPoints_Click(object sender, RoutedEventArgs e)
        {
            var blueBrush = new SolidColorBrush();
            blueBrush.Color = Colors.Blue;
            var yBrush = new SolidColorBrush();
            yBrush.Color = Colors.Yellow;
            if (PolygonTreeView.selectedItems.Count != 2)
            {
                MessageBox.Show("You should choose two polygons");
                return;
            }
            var pointsLines = Controller.instance.CalculateCrossPoints(PolygonTreeView.selectedItems);
            var points = pointsLines.Item1;
            foreach (var point in points)
            {
                var ellipse = new Ellipse();
                ellipse.Stroke = blueBrush;
                ellipse.Width = 4;
                ellipse.Height = 4;
                Canvas.SetLeft(ellipse, (int)point.X - 2);
                Canvas.SetTop(ellipse, (int)point.Y - 2);
                Field.Children.Add(ellipse);
                ellipses.Add(ellipse);
            }
            foreach (var bound in pointsLines.Item2)
            {
                var p1 = new Point(bound.Left.X, bound.Left.Y);
                var p2 = new Point(bound.Right.X, bound.Right.Y);
                var line = new Line();
                line.X1 = bound.Left.X;
                line.X2 = bound.Right.X;
                line.Y1 = bound.Left.Y;
                line.Y2 = bound.Right.Y;
                line.Stroke = yBrush;
                Field.Children.Add(line);


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
            var pinkBrush = new SolidColorBrush();
            pinkBrush.Color = Colors.Pink;
            Field.Children.Remove(rectangle3);
            rectangle3 = new Polyline();
            rectangle3.Stroke = pinkBrush;
            rectangle3.StrokeThickness = 3;
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
            if (newPol.nodes.Count == 0)
                return;
            var points = new PointCollection();
            foreach (var node in newPol.nodes)
            {
                points.Add(new Point(node.X, node.Y));
            }
            
            points.Add(new Point(newPol.nodes[0].X, newPol.nodes[0].Y));
            rectangle3.Points = points;
            Field.Children.Add(rectangle3);
        }

        private void CreateUnitPol_Click(object sender, RoutedEventArgs e)
        {
            var pinkBrush = new SolidColorBrush();
            pinkBrush.Color = Colors.Pink;
            Field.Children.Remove(rectangle3);
            rectangle3 = new Polyline();
            rectangle3.Stroke = pinkBrush;
            rectangle3.StrokeThickness = 3;
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
            if (newPol.nodes.Count == 0)
                return;
            var points = new PointCollection();
            foreach (var node in newPol.nodes)
            {
                points.Add(new Point(node.X, node.Y));
            }
            points.Add(new Point(newPol.nodes[0].X, newPol.nodes[0].Y));
            rectangle3.Points = points;
            Field.Children.Add(rectangle3);
        }

        private void CalculateS1_Click(object sender, RoutedEventArgs e)
        {
            var firstPolygon = Controller.instance.PolygonDatas[0];
            if (firstPolygon is null)
                return;
            if (!firstPolygon.IsClosed)
                return;
            var s = firstPolygon.CalculateS();
            SqLabel.Content = s.ToString();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var idx = Controller.instance.PolygonDatas.Count;
            var polygonData = new PolygonData(Controller.instance.GenerateBush(idx));
            Controller.instance.PolygonDatas.Add(polygonData);
            Field.Children.Add(polygonData.GUIView);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Field.Children.Clear();
            Controller.instance.PolygonDatas.Clear();
            rectangle3 = new Polyline();
            Field.Children.Add(rectangle3);
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
            Field.Children.Remove(rectangle3);
            rectangle3 = new Polyline();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Controller.instance.Update();
        }
    }
}
