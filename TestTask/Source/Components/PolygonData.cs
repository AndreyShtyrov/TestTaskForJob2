using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using TestTask.Source.Interface;
using System.ComponentModel;
using System.Xml.Linq;


namespace TestTask.Source.Components
{
    public class PolygonData : INotifyPropertyChanged
    {
        public ObservableCollection<INode> Nodes { get; }
        public List<IBound> Bounds { get; private set; }
        public SolidColorBrush Brush { get; }

        private bool isChecked = false;

        public bool IsChecked
        {
            set
            {
                isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsChecked)));
            }
            get => isChecked;
        }

        public string State => IsClosed ? "Close" : "Open";

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsClosed { get; private set; } = false;

        public Polyline GUIView { get; }

        public void DeleteNode(INode toDelete)
        {
            var toDeleteIdx = Nodes.IndexOf(toDelete);
            (Nodes[toDeleteIdx] as Node).OnChangeNode = null;

            if (toDeleteIdx < 0)
                return;
            _ = new Border(Nodes[toDeleteIdx].Prev(), Nodes[toDeleteIdx].Next());
            Nodes.RemoveAt(toDeleteIdx);

            Bounds = Nodes.Select(node => node.Right).ToList();
        }

        public void IncludeNode(INode first, INode second, INode toInclude)
        {
            var includedBorder = Bounds.First(bound =>
                bound.Left.Equals(first) && bound.Right.Equals(second) ||
                bound.Right.Equals(first) && bound.Left.Equals(second));

            var newNode = new Node(toInclude.X, toInclude.Y);
            _ = new Border(includedBorder.Left, newNode);
            _ = new Border(newNode, includedBorder.Right);
            newNode.OnChangeNode = OnChangeNodeCollection;
            var startNode = Nodes[0];
            var _nodes = startNode.NodeSequence();
            Nodes.Clear();
            foreach (var node in _nodes)
                Nodes.Add(node);

            Bounds = new List<IBound>();
            foreach (var node in Nodes)
                Bounds.Add(node.Right);
        }

        public void IncludeNodeList(List<INode> newNodes)
        {
            while (newNodes.Count > 0)
            {
                var atSameBound = new List<INode> { newNodes[0] };
                for (var i = 1; i < newNodes.Count; i++)
                {
                    if (atSameBound[0].Prev().Equals(newNodes[i].Prev())
                        && atSameBound[0].Next().Equals(newNodes[i].Next()))
                        atSameBound.Add(newNodes[i]);
                }
                var left = atSameBound[0].Prev();
                atSameBound = atSameBound.OrderBy(x => left.VectorLengthSquare(x)).ToList();
                var right = atSameBound[0].Next();
                foreach (var node in atSameBound)
                {
                    IncludeNode(left, right, node);
                    left = node;
                }
                foreach (var node in atSameBound)
                {
                    newNodes.Remove(node);
                }
            }
        }

        public PolygonData(SolidColorBrush brush)
        {
            Brush = brush;
            Nodes = new ObservableCollection<INode>();
            Bounds = new List<IBound>();
            GUIView = new Polyline
            {
                StrokeThickness = 2,
                Stroke = Brush
            };
            Nodes.CollectionChanged += OnChangeNodeCollection;
        }

        public PolygonData()
        {
            Brush = new SolidColorBrush(Colors.Black);
            Nodes = new ObservableCollection<INode>();
            Bounds = new List<IBound>();
            GUIView = new Polyline
            {
                StrokeThickness = 2,
                Stroke = Brush
            };
        }

        public void AddNode(int X, int Y)
        {
            if (IsClosed)
                return;
            if (Nodes.Count > 2)
            {
                if (Math.Abs(Y - Nodes[0].Y) < 5 && Math.Abs(X - Nodes[0].X) < 5)
                {
                    CloseFigure();
                    return;
                }
            }
            var newNode = new Node(X, Y)
            {
                OnChangeNode = OnChangeNodeCollection
            };
            if (Nodes.Count == 0)
            {
                Nodes.Add(newNode);
                return;
            }
            var prevNode = Nodes[^1];
            var newBound = new Border(prevNode, newNode);
            Nodes.Add(newNode);
            Bounds.Add(newBound);
        }

        public bool CheckNewBoundCrossPrevBorders(int X, int Y)
        {
            if (Nodes.Count == 0 || Bounds.Count == 0)
                return false;
            var prevNode = new Node(Nodes[^1].X, Nodes[^1].Y);
            var nextNode = new Node(X, Y);
            var ray = new Ray(prevNode, nextNode);
            return Bounds.Any(bound => ray.CrossPoint(bound).HasValue);
        }

        public PolygonData Copy()
        {
            var pol = new PolygonData();
            foreach (var node in Nodes)
                pol.AddNode(node.X, node.Y);
            if (IsClosed)
                pol.CloseFigure();
            return pol;
        }

        public void CloseFigure()
        {
            if (IsClosed || Nodes.Count < 3)
                return;
            IsClosed = true;
            var newBound = new Border(Nodes[^1], Nodes[0]);
            Bounds.Add(newBound);
            OnChangeNodeCollection(this, new EventArgs());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
        }

        public bool CheckPointOutOfPolygon(INode node)
        {
            var minleft = Nodes[0].X;
            var maxright = Nodes[0].X;
            var maxtop = Nodes[0].Y;
            var minbotton = Nodes[0].Y;

            for (var i = 1; i < Nodes.Count; i++)
            {
                if (Nodes[i].X < minleft)
                    minleft = Nodes[i].X;
                if (Nodes[i].X > maxright)
                    maxright = Nodes[i].X;
                if (Nodes[i].Y < minbotton)
                    minbotton = Nodes[i].Y;
                if (Nodes[i].Y > maxtop)
                    maxtop = Nodes[i].Y;
            }
            if (minleft > node.X || node.X > maxright || minbotton > node.Y || node.Y > maxtop)
                return true;
            var rays = new List<Ray>
            {
                new Ray(node, new Node(node.X, maxtop)),
                new Ray(node, new Node(node.X, minbotton)),
                new Ray(node, new Node(minleft, node.Y)),
                new Ray(node, new Node(maxright, node.Y))
            };
            foreach (var ray in rays)
            {
                var countCross = 0;
                var isCrossVertex = false;
                foreach (var bound in Bounds)
                {
                    var crp = ray.CrossPoint(bound, false);
                    if (crp != null)
                    {
                        if (Nodes.Any(node => node.X == (int)crp.Value.X && node.Y == (int)crp.Value.Y))
                            isCrossVertex = true;
                        countCross += 1;
                    }
                }
                if (isCrossVertex)
                    continue;
                return countCross % 2 == 0;
            }
            return false;
        }

        public Tuple<List<INode>, List<INode>> CalculateCrossPoints(PolygonData other)
        {
            var newNodes1 = new List<INode>();
            var newNodes2 = new List<INode>();
            foreach (var bound1 in Bounds)
            {
                foreach (var bound2 in other.Bounds)
                {
                    var point = bound1.CrossPoint(bound2);
                    if (point != null)
                    {
                        var node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound1.Left, node);
                        node.Right = new Segment(node, bound1.Right);
                        newNodes1.Add(node);
                        node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound2.Left, node);
                        node.Right = new Segment(node, bound2.Right);
                        newNodes2.Add(node);
                    }
                }
            }
            return new Tuple<List<INode>, List<INode>>(newNodes1, newNodes2);
        }

        public List<INode> AddCrossPoints(PolygonData other)
        {
            var result = new List<INode>();
            var newNodes1 = new List<INode>();
            var newNodes2 = new List<INode>();
            foreach (var bound1 in Bounds)
            {
                foreach (var bound2 in other.Bounds)
                {
                    var point = bound1.CrossPoint(bound2);
                    if (point != null)
                    {
                        var node = new Node((int)point.Value.X, (int)point.Value.Y);
                        result.Add(node);
                        node.Left = new Segment(bound1.Left, node);
                        node.Right = new Segment(node, bound1.Right);
                        newNodes1.Add(node);
                        node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound2.Left, node);
                        node.Right = new Segment(node, bound2.Right);
                        newNodes2.Add(node);
                    }
                }
            }
            IncludeNodeList(newNodes1);
            other.IncludeNodeList(newNodes2);
            return result;
        }

        public INode CalculateCenterMass()
        {
            var sumX = 0;
            var sumY = 0;
            foreach (var node in Nodes)
            {
                sumX += node.X;
                sumY += node.Y;
            }
            var x = (int)Math.Round(sumX / (double)Nodes.Count);
            var y = (int)Math.Round(sumY / (double)Nodes.Count);
            return new Node(x, y);
        }

        public static double TriangleSquare(INode n1, INode n2, INode n3)
        {
            var a = (double)n1.VectorLengthSquare(n2);
            var b = (double)n2.VectorLengthSquare(n3);
            var v1 = n2.Direction(n1);
            var v2 = n2.Direction(n3);
            var angle = Math.Acos(v1.x * v2.x + v1.y * v2.y);
            return Math.Sqrt(a) * Math.Sqrt(b) * Math.Sin(angle) / 2;
        }

        public double CalculateConvexS()
        {
            var centerMass = CalculateCenterMass();
            var firstNode = Nodes[0];
            var currentNode = firstNode.Next();
            var prevNode = firstNode;
            double S = 0;
            do
            {
                S += TriangleSquare(currentNode, centerMass, prevNode);
                prevNode = currentNode;
                currentNode = currentNode.Next();
            } while (!prevNode.Equals(firstNode));
            return S;
        }

        public double CalculateS()
        {
            var convexPolygons = new List<PolygonData>();
            var firstNode = Nodes[0];
            var minY = firstNode.Y;
            if (Nodes.Count == 3)
                return TriangleSquare(Nodes[0], Nodes[1], Nodes[2]);
            foreach (var node in Nodes)
            {
                if (node.Y < minY)
                    firstNode = node;
            }
            var mainPolygon = new PolygonData();
            var nodelist = firstNode.NodeSequence();
            for (var i = 0; i < nodelist.Count; i++)
                mainPolygon.AddNode(nodelist[i].X, nodelist[i].Y);
            mainPolygon.CloseFigure();
            if (Nodes.Count == 4 && (Nodes[0].X == Nodes[2].X) && (Nodes[1].Y == Nodes[3].Y))
            {
                return mainPolygon.CalculateConvexS();
            }

            firstNode = mainPolygon.Nodes[0];
            var currentNode = firstNode.Next().Next();
            var prevNode = firstNode;
            var excludedPoints = new List<INode>();
            var connectedExcludedSeq = false;
            while (true)
            {
                if (mainPolygon.Nodes.Count == 3)
                    break;
                var middlePointX = (currentNode.X - prevNode.X) / 5 + prevNode.X;
                var middlePointY = (currentNode.Y - prevNode.Y) / 5 + prevNode.Y;
                var middleNode = new Node(middlePointX, middlePointY);
                if (firstNode.Equals(currentNode.Prev()))
                    break;
                if (mainPolygon.CheckPointOutOfPolygon(middleNode))
                {
                    if (!connectedExcludedSeq)
                    {
                        if (excludedPoints.Count > 0)
                        {
                            var outPolygon = new PolygonData();
                            for (var i = 0; i < excludedPoints.Count; i++)
                            {
                                outPolygon.AddNode(excludedPoints[i].X, excludedPoints[i].Y);
                            }
                            outPolygon.CloseFigure();
                            convexPolygons.Add(outPolygon);
                        }
                        excludedPoints = new List<INode>();
                        connectedExcludedSeq = true;
}
                    excludedPoints.AddRange(new[] { prevNode, currentNode.Prev(), currentNode }
                        .Where(node => !excludedPoints.Contains(node)));
                    mainPolygon.DeleteNode(currentNode.Prev());
                    currentNode = currentNode.Next();
                    continue;
                }
                connectedExcludedSeq = false;
                prevNode = prevNode.Next();
                currentNode = currentNode.Next();
            }

            if (excludedPoints.Count > 0)
            {
                var outPolygon = new PolygonData();
                for (var i = 0; i < excludedPoints.Count; i++)
                    outPolygon.AddNode(excludedPoints[i].X, excludedPoints[i].Y);
                outPolygon.CloseFigure();
                convexPolygons.Add(outPolygon);
            }

            var S = mainPolygon.CalculateConvexS() - convexPolygons.Sum(polygon => polygon.CalculateConvexS());
            if (Nodes.Count == 4 && S < 0)
                S = -S;
            return S;
        }

        private void OnChangeNodeCollection(object sender, EventArgs e) => Update();

        public void Update()
        {
            GUIView.Points.Clear();
            var points = new PointCollection();
            foreach (var node in Nodes)
                points.Add(new Point(node.X, node.Y));
            if (IsClosed)
                points.Add(new Point(Nodes[0].X, Nodes[0].Y));
            GUIView.Points = points;
        }
    }
}
