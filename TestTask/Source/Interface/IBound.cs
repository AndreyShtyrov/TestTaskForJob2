using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Source.Interface
{
    public interface IBound
    {
        public INode left
        { get; }

        public INode right
        { get; }


        public Nullable<Point> CrossPoint(IBound other, Boolean islimited = true);

        public double CalculateAngle(INode other, bool isForward=true);

        public bool InBorders(int X, int Y);

        public bool Equals(IBound other);
    }
}
