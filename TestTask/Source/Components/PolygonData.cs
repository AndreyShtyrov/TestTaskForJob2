using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Linq;
using System.Windows.Media;
using System.Windows.Shapes;
using TestTask.Source.Interface;
using System.ComponentModel;


namespace TestTask.Source.Components
{
    public class PolygonData: INotifyPropertyChanged
    {

        private List<IBound> _bounds;
        public ObservableCollection<INode> nodes
        { get; }

        public List<IBound> bounds
        { get { return _bounds; } }
        public SolidColorBrush Brush
        { get; }

        private bool isChecked = false;

        public bool IsChecked
        {
            set
            {
                isChecked = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsChecked"));
            }
            get
            {
                return isChecked;
            }
        }

        public string State
        {
            get
            {
                if (IsClosed)
                    return "Close";
                else
                    return "Open";
            }
        }

        private bool _IsClosed = false;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsClosed
        { get { return _IsClosed; } }

        public Polyline GUIView
        { get; }

        public void DeleteNode(INode toDelete)
        {
            int toDeleteIdx = -1;


            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Equals(toDelete))
                {
                    toDeleteIdx = i;
                    break;
                }
            }
            (nodes[toDeleteIdx] as Node).OnChangeNode = null;

            if (toDeleteIdx < 0)
                return;
            Border newBorder = new Border(nodes[toDeleteIdx].Prev(), nodes[toDeleteIdx].Next());
            nodes.RemoveAt(toDeleteIdx);

            _bounds = new List<IBound>();
            foreach (var node in nodes)
            {
                _bounds.Add(node.Right);
            }
        }

        public void IncludeNode(INode first, INode second, INode toInclude)
        {
            int includedBorder = -1;
            for (int i = 0; i < bounds.Count; i++)
            {
                if (bounds[i].left.Equals(first) && bounds[i].right.Equals(second))
                {
                    includedBorder = i;
                    break;
                }
                if (bounds[i].right.Equals(first) && bounds[i].left.Equals(second))
                {
                    includedBorder = i;
                    break;
                }
            }
            Node newNode = new Node(toInclude.X, toInclude.Y);
            var _b = new Border(bounds[includedBorder].left, newNode);
            _b = new Border(newNode, bounds[includedBorder].right);
            newNode.OnChangeNode = OnChangeNodeCollection;
            var startNode = nodes[0];
            var _nodes = startNode.NodeSequentce();
            nodes.Clear();
            foreach (var node in _nodes)
            {
                nodes.Add(node);
            }

            _bounds = new List<IBound>();
            foreach (var node in nodes)
            {
                _bounds.Add(node.Right);
            }
        }

        public void IncludeNodeList(List<INode> newNodes)
        {
            while (newNodes.Count > 0)
            {
                List<INode> atSameBound = new List<INode>();
                atSameBound.Add(newNodes[0]);
                for (int i = 1; i < newNodes.Count; i++)
                {
                    if (atSameBound[0].Prev().Equals(newNodes[i].Prev())
                        && atSameBound[0].Next().Equals(newNodes[i].Next()))
                        atSameBound.Add(newNodes[i]);
                }
                var left = atSameBound[0].Prev();
                atSameBound = atSameBound.OrderBy(x => left.VectorLengthSquare(x)).ToList<INode>();
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
            nodes = new ObservableCollection<INode>();
            _bounds = new List<IBound>();
            GUIView = new Polyline();
            GUIView.StrokeThickness = 2;
            GUIView.Stroke = Brush;
            nodes.CollectionChanged += OnChangeNodeCollection;
        }

        public PolygonData()
        {
            Brush = new SolidColorBrush(Colors.Black);
            nodes = new ObservableCollection<INode>();
            _bounds = new List<IBound>();
            GUIView = new Polyline();
            GUIView.StrokeThickness = 2;
            GUIView.Stroke = Brush;
        }

        public void AddNode(int X, int Y)
        {
            if (_IsClosed is true)
                return;
            if (nodes.Count > 2)
            {
                if (Math.Abs(Y - nodes[0].Y) < 5 && Math.Abs(X - nodes[0].X) < 5)
                {
                    CloseFigure();
                    return;
                }
            }
            Node newNode = new Node(X, Y);
            newNode.OnChangeNode = OnChangeNodeCollection;
            if (nodes.Count == 0)
            {
                nodes.Add(newNode);
                return;
            }
            var prevNode = nodes[nodes.Count - 1];
            Border newBound = new Border(prevNode, newNode);
            nodes.Add(newNode);
            bounds.Add(newBound);
        }

        public bool CheckNewBoundCrossPrevBorders(int X, int Y)
        {
            if (nodes.Count == 0 || bounds.Count == 0)
                return false;
            var prevNode = new Node(nodes[nodes.Count - 1].X, nodes[nodes.Count - 1].Y);
            var nextNode = new Node(X, Y);
            var ray = new Ray(prevNode, nextNode);
            for(int i = 0; i < bounds.Count - 1; i++)
            {
                var crp = ray.CrossPoint(bounds[i]);
                if (crp.HasValue)
                    return true;
            }
            return false;
        }

        public PolygonData Copy()
        {
            var pol = new PolygonData();
            foreach (var node in nodes)
            {
                pol.AddNode(node.X, node.Y);
            }
            if (IsClosed)
                pol.CloseFigure();
            return pol;
        }

        public void CloseFigure()
        {
            if (_IsClosed is true)
                return;
            if (nodes.Count < 3)
                return;
            _IsClosed = true;
            Border newBound = new Border(nodes[nodes.Count - 1], nodes[0]);
            bounds.Add(newBound);
            OnChangeNodeCollection(new object(), new EventArgs());
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("State"));
        }

        public bool CheckPointOutOfPolygon(INode node)
        {
            int minleft = nodes[0].X;
            int maxright = nodes[0].X;
            int maxtop = nodes[0].Y;
            int minbotton = nodes[0].Y;

            for (int i = 1; i < nodes.Count; i++)
            {
                if (nodes[i].X < minleft)
                    minleft = nodes[i].X;
                if (nodes[i].X > maxright)
                    maxright = nodes[i].X;
                if (nodes[i].Y < minbotton)
                    minbotton = nodes[i].Y;
                if (nodes[i].Y > maxtop)
                    maxtop = nodes[i].Y;
            }
            if (minleft > node.X || node.X > maxright)
                return true;
            if (minbotton > node.Y || node.Y > maxtop)
                return true;
            List<Ray> rays = new List<Ray>();
            var newNode = new Node(node.X, maxtop);
            rays.Add(new Ray(node, newNode));
            newNode = new Node(node.X, minbotton);
            rays.Add(new Ray(node, newNode));
            newNode = new Node(minleft, node.Y);
            rays.Add(new Ray(node, newNode));
            newNode = new Node(maxright, node.Y);
            rays.Add(new Ray(node, newNode));
            for (int j = 0; j < rays.Count; j++)
            {
                int countCross = 0;
                bool isCrossVertex = false;
                for (int i = 0; i < bounds.Count; i++)
                {
                    var crp = rays[j].CrossPoint(bounds[i], false);
                    if (crp != null)
                    {
                        var crossedVertex = getEqualNode(nodes.ToList<INode>(), new Node((int)crp.Value.X, (int)crp.Value.Y));
                        if (crossedVertex != null)
                            isCrossVertex = true;
                        countCross += 1;
                    }
                }
                if (isCrossVertex)
                    continue;
                if (countCross == 0)
                    return true;
                if (countCross % 2 == 0)
                    return true;
                if (countCross % 2 == 1)
                    return false;
            }
            return false;
        }

        public Tuple<List<INode>, List<INode>> CalculateCrossPoints(PolygonData other)
        {
            List<INode> newNodes1 = new List<INode>();
            List<INode> newNodes2 = new List<INode>();
            foreach (var bound1 in bounds)
            {
                foreach (var bound2 in other.bounds)
                {
                    var point = bound1.CrossPoint(bound2);
                    if (point != null)
                    {
                        var node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound1.left, node);
                        node.Right = new Segment(node, bound1.right);
                        newNodes1.Add(node);
                        node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound2.left, node);
                        node.Right = new Segment(node, bound2.right);
                        newNodes2.Add(node);
                    }
                }
            }
            return new Tuple<List<INode>, List<INode>>(newNodes1, newNodes2);
        }

        public List<INode> AddCrossPoints(PolygonData other)
        {
            List<INode> result = new List<INode>();
            List<INode> newNodes1 = new List<INode>();
            List<INode> newNodes2 = new List<INode>();
            foreach (var bound1 in bounds)
            {
                foreach (var bound2 in other.bounds)
                {
                    var point = bound1.CrossPoint(bound2);
                    if (point != null)
                    {
                        var node = new Node((int)point.Value.X, (int)point.Value.Y);
                        result.Add(node);
                        node.Left = new Segment(bound1.left, node);
                        node.Right = new Segment(node, bound1.right);
                        newNodes1.Add(node);
                        node = new Node((int)point.Value.X, (int)point.Value.Y);
                        node.Left = new Segment(bound2.left, node);
                        node.Right = new Segment(node, bound2.right);
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
            int sumX = 0;
            int sumY = 0;
            foreach (var node in nodes)
            {
                sumX += node.X;
                sumY += node.Y;
            }
            var x = (int)Math.Round(((double)sumX) / (double)nodes.Count);
            var y = (int)Math.Round(((double)sumY) / (double)nodes.Count);
            return new Node(x, y);
        }

        public double TriangleSquare(INode n1, INode n2, INode n3)
        {
            var a = (double)n1.VectorLengthSquare(n2);
            var b = (double)n2.VectorLengthSquare(n3);
            var v1 = n2.Direction(n1);
            var v2 = n2.Direction(n3);
            var angle = Math.Acos(v1.Item1 * v2.Item1 + v1.Item2 * v2.Item2);
            return Math.Sqrt(a) * Math.Sqrt(b) * Math.Sin(angle) / 2;
        }

        public double CalculateConvexS()
        {
            var centerMass = CalculateCenterMass();
            var firstNode = nodes[0];
            var currentNode = firstNode.Next();
            var prevNode = firstNode;
            double S = 0;
            while (true)
            {
                S += TriangleSquare(currentNode, centerMass, prevNode);
                prevNode = currentNode;
                currentNode = currentNode.Next();
                if (prevNode.Equals(firstNode))
                    break;
            }
            return S;
        }

        private bool check(List<INode> list, INode _node)
        {
            foreach (var node in list)
            {
                if (node.Equals(_node))
                    return true;
            }
            return false;
        }

        private INode getEqualNode(List<INode> list, INode _node)
        {
            foreach (var node in list)
            {
                if (node.Equals(_node))
                    return node;
            }
            return null;
        }

        public double CalculateS()
        {
            PolygonData mainPolygon = null;
            List<PolygonData> convexPolygons = new List<PolygonData>();
            var firstNode = nodes[0];
            var minY = firstNode.Y;
            if (nodes.Count == 3)
                return TriangleSquare(nodes[0], nodes[1], nodes[2]);
            foreach (var node in nodes)
            {
                if (node.Y < minY)
                    firstNode = node;
            }
            mainPolygon = new PolygonData();
            var nodelist = firstNode.NodeSequentce();
            for (int i = 0; i < nodelist.Count; i++)
                mainPolygon.AddNode(nodelist[i].X, nodelist[i].Y);
            mainPolygon.CloseFigure();
            if (nodes.Count == 4)
            {
                if ((nodes[0].X == nodes[2].X) && (nodes[1].Y == nodes[3].Y))
                {
                    return mainPolygon.CalculateConvexS();
                }
            }


            firstNode = mainPolygon.nodes[0];
            var currentNod = firstNode.Next().Next();
            var prevNode = firstNode;
            List<INode> excludedPoints = new List<INode>();
            bool connectedExcludedSeq = false;
            while (true)
            {
                if (mainPolygon.nodes.Count == 3)
                    break;
                var middlePointX = (int)((currentNod.X - prevNode.X) / 5 + prevNode.X);
                var middlePointY = (int)((currentNod.Y - prevNode.Y) / 5 + prevNode.Y);
                Node middleNode = new Node(middlePointX, middlePointY);
                if (firstNode.Equals(currentNod.Prev()))
                    break;
                if (mainPolygon.CheckPointOutOfPolygon(middleNode))
                {
                    if (!connectedExcludedSeq)
                    {
                        if (excludedPoints.Count > 0)
                        {
                            var outPolygon = new PolygonData();
                            for (int i = 0; i < excludedPoints.Count; i++)
                            {
                                outPolygon.AddNode(excludedPoints[i].X, excludedPoints[i].Y);
                            }
                            outPolygon.CloseFigure();
                            convexPolygons.Add(outPolygon);
                        }
                        excludedPoints = new List<INode>();
                        connectedExcludedSeq = true;
                    }
                    if (!check(excludedPoints, prevNode))
                        excludedPoints.Add(prevNode);
                    if (!check(excludedPoints, currentNod.Prev()))
                        excludedPoints.Add(currentNod.Prev());
                    if (!check(excludedPoints, currentNod))
                        excludedPoints.Add(currentNod);
                    mainPolygon.DeleteNode(currentNod.Prev());
                    currentNod = currentNod.Next();
                    continue;
                }
                connectedExcludedSeq = false;
                prevNode = prevNode.Next();
                currentNod = currentNod.Next();
            }

            if (excludedPoints.Count > 0)
            {
                var outPolygon = new PolygonData();
                for (int i = 0; i < excludedPoints.Count; i++)
                {
                    outPolygon.AddNode(excludedPoints[i].X, excludedPoints[i].Y);
                }
                outPolygon.CloseFigure();
                convexPolygons.Add(outPolygon);
            }

            double S = mainPolygon.CalculateConvexS();
            foreach (var polygon in convexPolygons)
            {
                S -= polygon.CalculateConvexS();
            }
            if (this.nodes.Count == 4 && S < 0)
            {
                S = -S;
            }
            return S;
        }

        private void OnChangeNodeCollection(object sender, EventArgs e)
        {
            GUIView.Points.Clear();
            PointCollection points = new PointCollection();
            foreach (var node in nodes)
            {
                points.Add(new Point(node.X, node.Y));

            }
            if (IsClosed)
                points.Add(new Point(nodes[0].X, nodes[0].Y));
            GUIView.Points = points;
        }

        public void Update()
        {
            GUIView.Points.Clear();
            PointCollection points = new PointCollection();
            foreach (var node in nodes)
            {
                points.Add(new Point(node.X, node.Y));

            }
            if (IsClosed)
                points.Add(new Point(nodes[0].X, nodes[0].Y));
            GUIView.Points = points;
        }
    }
}
