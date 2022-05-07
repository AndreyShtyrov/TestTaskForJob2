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
        private static readonly object _instanceLock = new object();

        public static Controller Instance
        {
            get
            {
                if (_instance is not null)
                    return _instance;
                lock (_instanceLock)
                {
                    if (_instance is null)
                        _instance = new Controller();
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

        public PolygonData CreateCrossPolygon(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count < 2)
                return new PolygonData();
            FirstRectangle = polygons[0].Copy();
            SecondRectangle = polygons[1].Copy();

            var unitedPoints = new List<INode>();
            unitedPoints.AddRange(FirstRectangle.Nodes.Where(node => !SecondRectangle.CheckPointOutOfPolygon(node)));
            unitedPoints.AddRange(SecondRectangle.Nodes.Where(node => !FirstRectangle.CheckPointOutOfPolygon(node)));

            var crossPoints = FirstRectangle.AddCrossPoints(SecondRectangle);

            unitedPoints.AddRange(crossPoints);
            if (unitedPoints.Count == 0)
                return new PolygonData();

            var prevPoints = SortNodeByConnections(unitedPoints);
            var result = new PolygonData(GenerateBush(2));
            foreach (var point in prevPoints)
                result.AddNode(point.X, point.Y);
            FirstRectangle = null;
            SecondRectangle = null;
            return result;
        }

        private List<INode> SortNodeByConnections(List<INode> unitedPoints)
        {
            var prevPoint = FirstRectangle.Nodes.FirstOrDefault((_node) => _node.Equals(unitedPoints[0]))
                ?? SecondRectangle.Nodes.FirstOrDefault((_node) => _node.Equals(unitedPoints[0]));
            var prevPoints = new List<INode> { prevPoint };
            var j = 1;
            while (j < unitedPoints.Count)
            {
                var isFound = false;
                foreach (var node in unitedPoints.Where<INode>(node => !prevPoints.Contains(node)))
                {
                    var _point = FirstRectangle.Nodes.FirstOrDefault<INode>((_node) => _node.Equals(node));
                    if (_point != null && (_point.Prev().Equals(prevPoint) || _point.Next().Equals(prevPoint)))
                    {
                        prevPoint = _point;
                        prevPoints.Add(prevPoint);
                        j += 1;
                        isFound = true;
                        break;
                    }
                    _point = SecondRectangle.Nodes.FirstOrDefault((_node) => _node.Equals(node));
                    if (_point != null && (_point.Prev().Equals(prevPoint) || _point.Next().Equals(prevPoint)))
                    {
                        prevPoint = _point;
                        j += 1;
                        prevPoints.Add(prevPoint);
                        isFound = true;
                        break;
                    }
                }
                if (!isFound)
                    break;
            }
            return prevPoints;
        }

        public PolygonData CreateUnitPolygon(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count < 2)
                return new PolygonData();

            FirstRectangle = polygons[0].Copy();
            SecondRectangle = polygons[1].Copy();
            var unitedPoints = new List<INode>();
            unitedPoints.AddRange(FirstRectangle.Nodes.Where(node => SecondRectangle.CheckPointOutOfPolygon(node)));
            unitedPoints.AddRange(SecondRectangle.Nodes.Where(node => FirstRectangle.CheckPointOutOfPolygon(node)));

            var crossPoints = FirstRectangle.AddCrossPoints(SecondRectangle);
            if (crossPoints.Count == 0)
                return new PolygonData(GenerateBush(2));
            unitedPoints.AddRange(crossPoints);

            var prevPoints = SortNodeByConnections(unitedPoints);
            var result = new PolygonData(GenerateBush(2));
            for (var i = 0; i < prevPoints.Count; i++)
                result.AddNode(prevPoints[i].X, prevPoints[i].Y);
            FirstRectangle = null;
            SecondRectangle = null;
            return result;
        }

        public (List<Point> result, List<IBound> supportLines, List<int> crossintCounts) CalculateCrossPoints(List<PolygonData> polygons)
        {
            if (PolygonDatas.Count >= 2)
            {
                FirstRectangle = polygons[0].Copy();
                SecondRectangle = polygons[1].Copy();
            }

            if (FirstRectangle is null || SecondRectangle is null ||
                !(FirstRectangle.IsClosed && SecondRectangle.IsClosed))
                return (new List<Point>(), new List<IBound>(), new List<int>());

            var supportLines = new List<IBound>();
            var crossintCounts = new List<int>();
            var result = new List<Point>();
            result.AddRange(FirstRectangle.CalculateCrossPoints(SecondRectangle).Item1.Select(node => node.Point));
            result.AddRange(FirstRectangle.Nodes.Where(node => !SecondRectangle.CheckPointOutOfPolygon(node)).Select(node => new Point(node.X, node.Y)));
            result.AddRange(SecondRectangle.Nodes.Where(node => !FirstRectangle.CheckPointOutOfPolygon(node)).Select(node => new Point(node.X, node.Y)));
            FirstRectangle = null;
            SecondRectangle = null;
            return (result, supportLines, crossintCounts);

        }

        public void Update()
        {
            foreach (var polygon in PolygonDatas)
                polygon.Update();
        }
    }
}
