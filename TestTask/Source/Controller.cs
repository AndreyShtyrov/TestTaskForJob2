using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using TestTask.Source.Components;
using TestTask.Source.Interface;

namespace TestTask.Source
{
    public class Controller
    {
        public SolidColorBrush FirstPolygonBorderColor;
        public SolidColorBrush SecondPolygonBorderColor;
        public PolygonData FirstRectangle;
        public PolygonData SecondRectangle;

        public ObservableCollection<PolygonData> PolygonDatas { get; }
        public int ChoosenRectangle { get; } = 0;

        private static Controller _instance;

        public static Controller instance
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new Controller();
                    return _instance;
                }
                return _instance;
            }
        }

        public static SolidColorBrush GenerateBush(int index)
        {
            var brush = new SolidColorBrush();
            switch (index)
            {
                case 0: { brush.Color = Colors.Black; break; }
                case 1: { brush.Color = Colors.Red; break; }
                case 2: { brush.Color = Colors.Green; break; }
                case 3: { brush.Color = Colors.Blue; break; }
                case 4: { brush.Color = Colors.Yellow; break; }
                case 5: { brush.Color = Colors.GreenYellow; break; }
                default: { brush.Color = Colors.Black; break; }
            }
            return brush;
        }

        private Controller()
        {
            FirstPolygonBorderColor = new SolidColorBrush();
            SecondPolygonBorderColor = new SolidColorBrush();
            FirstPolygonBorderColor.Color = Colors.Black;
            SecondPolygonBorderColor.Color = Colors.Red;
            PolygonDatas = new ObservableCollection<PolygonData>();
        }

        private static bool check(List<INode> list, INode _node)
        {
            foreach (var node in list)
            {
                if (node.Equals(_node))
                    return true;
            }
            return false;
        }

        private static INode getEqualNode(List<INode> list, INode _node)
        {
            foreach (var node in list)
            {
                if (node.Equals(_node))
                    return node;
            }
            return null;
        }

        public PolygonData CreateCrossPolygon(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count >= 2)
            {
                FirstRectangle = polygons[0].Copy();
                SecondRectangle = polygons[1].Copy();
            }
            else
            {
                return new PolygonData();
            }
            var unitedPoints = new List<INode>();
            foreach (var node in FirstRectangle.Nodes)
            {
                var res = SecondRectangle.CheckPointOutOfPolygon(node);

                if (!res)
                    unitedPoints.Add(node);

            }
            foreach (var node in SecondRectangle.Nodes)
            {
                var res = FirstRectangle.CheckPointOutOfPolygon(node);
                if (!res)
                    unitedPoints.Add(node);
            }
            var crossPoints = FirstRectangle.AddCrossPoints(SecondRectangle);
            if (unitedPoints.Count == 0)
            {
                return new PolygonData();
            }
            unitedPoints.AddRange(crossPoints);
            var prevPoints = SortNodeByConnections(unitedPoints);
            var result = new PolygonData(GenerateBush(2));
            for (var i = 0; i < prevPoints.Count; i++)
            {
                result.AddNode(prevPoints[i].X, prevPoints[i].Y);
            }
            FirstRectangle = null;
            SecondRectangle = null;
            return result;
        }

        private List<INode> SortNodeByConnections(List<INode> unitedPoints)
        {
            var prevPoints = new List<INode>();
            INode prevPoint = null;
            if (check(FirstRectangle.Nodes.ToList(), unitedPoints[0]))
                prevPoint = getEqualNode(FirstRectangle.Nodes.ToList(), unitedPoints[0]);
            else
                prevPoint = getEqualNode(SecondRectangle.Nodes.ToList(), unitedPoints[0]);
            prevPoints.Add(prevPoint);
            var j = 1;
            while (j < unitedPoints.Count)
            {
                var isFound = false;
                foreach (var node in unitedPoints)
                {
                    if (check(prevPoints, node))
                        continue;
                    var _point = getEqualNode(FirstRectangle.Nodes.ToList(), node);
                    if (_point != null)
                    {
                        if (_point.Prev().Equals(prevPoint) || _point.Next().Equals(prevPoint))
                        {
                            prevPoint = _point;
                            prevPoints.Add(prevPoint);
                            j += 1;
                            isFound = true;
                            break;
                        }
                    }
                    _point = getEqualNode(SecondRectangle.Nodes.ToList(), node);
                    if (_point != null)
                    {
                        if (_point.Prev().Equals(prevPoint) || _point.Next().Equals(prevPoint))
                        {
                            prevPoint = _point;
                            j += 1;
                            prevPoints.Add(prevPoint);
                            isFound = true;
                            break;
                        }
                    }
                }
                if (!isFound)
                    break;
            }
            return prevPoints;
        }

        public PolygonData CreateUnitPolygon(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count >= 2)
            {
                FirstRectangle = polygons[0].Copy();
                SecondRectangle = polygons[1].Copy();
            }
            else
            {
                return new PolygonData();
            }
            var unitedPoints = new List<INode>();
            foreach (var node in FirstRectangle.Nodes)
            {
                var res = SecondRectangle.CheckPointOutOfPolygon(node);

                if (res)
                    unitedPoints.Add(node);

            }
            foreach (var node in SecondRectangle.Nodes)
            {
                var res = FirstRectangle.CheckPointOutOfPolygon(node);
                if (res)
                    unitedPoints.Add(node);
            }
            var crossPoints = FirstRectangle.AddCrossPoints(SecondRectangle);
            if (crossPoints.Count == 0)
            {
                return new PolygonData(GenerateBush(2));
            }
            unitedPoints.AddRange(crossPoints);
            var prevPoints = SortNodeByConnections(unitedPoints);
            var result = new PolygonData(GenerateBush(2));
            for (var i = 0; i < prevPoints.Count; i++)
            {
                result.AddNode(prevPoints[i].X, prevPoints[i].Y);
            }
            FirstRectangle = null;
            SecondRectangle = null;
            return result;
        }

        public Tuple<List<Point>, List<IBound>, List<int>> CalculateCrossPoints(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count >= 2)
            {
                FirstRectangle = polygons[0].Copy();
                SecondRectangle = polygons[1].Copy();
            }
            var supportLines = new List<IBound>();
            var crossintCounts = new List<int>();
            if (FirstRectangle is null || SecondRectangle is null)
                return new Tuple<List<Point>, List<IBound>, List<int>>
                    (new List<Point>(), new List<IBound>(), new List<int>());
            if (!(FirstRectangle.IsClosed && SecondRectangle.IsClosed))
                return new Tuple<List<Point>, List<IBound>, List<int>>
                    (new List<Point>(), new List<IBound>(), new List<int>());
            var result = new List<Point>();
            var _result = FirstRectangle.CalculateCrossPoints(SecondRectangle);
            foreach (var node in _result.Item1)
            {
                result.Add(node.Point);
            }

            foreach (var node in FirstRectangle.Nodes)
            {
                var res = SecondRectangle.CheckPointOutOfPolygon(node);

                if (!res)
                {
                    result.Add(new Point(node.X, node.Y));
                }
            }
            foreach (var node in SecondRectangle.Nodes)
            {
                var res = FirstRectangle.CheckPointOutOfPolygon(node);
                if (!res)
                {
                    result.Add(new Point(node.X, node.Y));
                }
            }
            FirstRectangle = null;
            SecondRectangle = null;
            return new Tuple<List<Point>, List<IBound>, List<int>>(result, supportLines, crossintCounts);

        }

        public void Update()
        {
            foreach (var polygon in PolygonDatas)
            {
                polygon.Update();
            }
        }
    }
}
