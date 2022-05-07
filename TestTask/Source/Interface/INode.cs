using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Source.Interface
{
    public interface INode : IEquatable<INode>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public IBound Right { get; set; }
        public IBound Left { get; set; }

        public Point Point { get; }

        public INode Prev();
        public INode Next();

        public List<INode> NodeSequentce();


        public (double x, double y) Direction(INode other);

        public int VectorLengthSquare(INode other);

    }
}
