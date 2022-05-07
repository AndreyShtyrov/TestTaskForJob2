using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestTask.Source.Interface
{
    public interface IBound : IEquatable<IBound>
    {
        public INode Left { get; }
        public INode Right { get; }

        public Point? CrossPoint(IBound other, bool islimited = true);

        public double CalculateAngle(INode other, bool isForward=true);

        public bool InBorders(int X, int Y);
    }
}
