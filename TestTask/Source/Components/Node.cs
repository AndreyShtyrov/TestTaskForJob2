using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TestTask.Source.Interface;

namespace TestTask.Source.Components
{
    public class Node : INode
    {
        private int _x;
        private int _y;
        public int X
        {
            get => _x;
            set
            {
                _x = value;
                OnChangeNode?.Invoke(this, new EventArgs());
            }
        }
        public int Y
        {
            get => _y;
            set
            {
                _y = value;
                if (OnChangeNode != null)
                {
                    OnChangeNode(this, new EventArgs());
                }
            }
        }
        public OnChangeNode OnChangeNode = null;

        public Point Point => new(X, Y);
        public IBound Right { get; set; }
        public IBound Left { get; set; }
        public Node(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public INode Prev() => Left.Left;
        public INode Next() => Right.Right;

        public (double x, double y) Direction(INode other)
        {
            var lngth = Math.Sqrt(VectorLengthSquare(other));
            var x = (X - other.X) / lngth;
            var y = (Y - other.Y) / lngth;
            return (x, y);
        }

        public int VectorLengthSquare(INode other) => (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);

        public bool Equals(INode other) => other != null && other.X == X && other.Y == Y;
        public override bool Equals(object other) => other is INode otherNode && Equals(otherNode);

        public List<INode> NodeSequence()
        {
            var startNode = this;
            var nodes = new List<INode>() { startNode };
            var nextNode = Next();
            if (nextNode.Next() is null || startNode.Equals(nextNode))
                return nodes;
            nodes.Add(nextNode);
            nextNode = nextNode.Next();
            while (nextNode != null && !startNode.Equals(nextNode))
            {
                nodes.Add(nextNode);
                nextNode = nextNode.Next();
            }
            return nodes;
        }

        public static bool operator ==(Node lhs, Node rhs) => lhs is null && rhs is null || lhs.Equals(rhs);
        public static bool operator !=(Node lhs, Node rhs) => !(lhs == rhs);
    }

    public delegate void OnChangeNode(object sender, EventArgs e);
}
