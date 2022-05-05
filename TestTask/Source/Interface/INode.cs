using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Source.Interface
{
    public interface INode
    {
        public int X
        { get; set; }
        public int Y
        { get; set; }

        public IBound right
        { get; set; }

        public Point Point
        { get; }

        public IBound left
        { get; set; }

        public INode Next();

        public List<INode> NodeSeuqents();
        

        public Tuple<double, double> Direction(INode other);

        public int SquarVectorLength(INode other);

        public INode Prev();

        public bool Equals(INode other);

    }
}
