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
        public int X { 
            get
            {
                return _x;
            }
            set {
                _x = value;
                if (OnChangeNode != null)
                { 
                    OnChangeNode(this, new EventArgs());
                }
            } }
        public int Y
        {
            get
            {
                return _y;
            }
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

        public event OnChangeNode testEvent;

        public Point Point
        {
            get { return new Point(X, Y); }
        }
        public IBound right { get; set; }
        public IBound left { get; set; }
        public Node(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public INode Next()
        {
            return right.right;
        }

        public Tuple<double, double> Direction(INode other)
        {
            var lngth = Math.Sqrt((X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y));
            var x = (double)(X - other.X) / lngth;
            var y = (double)(Y - other.Y) / lngth;
            return new Tuple<double, double>(x, y);
        }

        public int SquarVectorLength(INode other)
        {
            return (X - other.X) * (X - other.X) + (Y - other.Y) * (Y - other.Y);
        }

        public INode Prev()
        {
            return left.left;
        }

        public bool Equals(INode other)
        {
            if (other.X == X && other.Y == Y)
                return true;
            return false;
        }

        public List<INode> NodeSeuqents()
        {
            var startNode = this;
            List<INode> nodes = new List<INode>() { startNode };
            var nextNode = Next();
            if (nextNode.Next() is null)
                return nodes;
            if (startNode.Equals(nextNode))
                return nodes;
            nodes.Add(Next());
            while (true)
            {
                nextNode = nextNode.Next();
                if (nextNode == null)
                    break;
                if (startNode.Equals(nextNode))
                    break;
                nodes.Add(nextNode);    
            }
            return nodes;
        }

        public static bool operator ==(Node lhs, Node rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(Node lhs, Node rhs) => !(lhs == rhs);
    }

    public delegate void OnChangeNode(object sender, EventArgs e);
}
