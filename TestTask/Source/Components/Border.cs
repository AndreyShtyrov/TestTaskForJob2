using System;
using System.Collections.Generic;
using System.Windows;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestTask.Source.Interface;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace TestTask.Source.Components
{
    public class Segment : IBound
    {
        public INode Left { get; }
        public INode Right { get; }

        public Segment(INode left, INode right)
        {
            Left = left;
            Right = right;
        }

        public double CalculateAngle(INode other, bool isForward)
        {
            if (isForward)
            {
                var dir1 = Right.Direction(Left);
                var dir2 = other.Direction(Right);
                return Math.Acos(dir1.x * dir2.x + dir1.y * dir2.y);
            }
            var dir_1 = Left.Direction(Right);
            var dir_2 = other.Direction(Left);
            return Math.Acos(dir_1.x * dir_2.x + dir_1.y * dir_2.y);
        }

        public virtual Point? CrossPoint(IBound other, bool islimited = true)
        {
            if (islimited)
            {
                if (Math.Max(Left.Y, Right.Y) < Math.Min(other.Left.Y, other.Right.Y))
                    return null;
                if (Math.Min(Left.Y, Right.Y) > Math.Max(other.Left.Y, other.Right.Y))
                    return null;
                if (Math.Max(Left.X, Right.X) < Math.Min(other.Left.X, other.Right.X))
                    return null;
                if (Math.Min(Left.X, Right.X) > Math.Max(other.Left.X, other.Right.X))
                    return null;
            }
            var c11 = Right.X - Left.X;
            var c21 = Right.Y - Left.Y;
            var c12 = -(other.Right.X - other.Left.X);
            var c22 = -(other.Right.Y - other.Left.Y);
            var t1 = other.Left.X - Left.X;
            var t2 = other.Left.Y - Left.Y;
            Matrix<double> A = DenseMatrix.OfArray(new double[,] { { c11, c12 }, { c21, c22 } });
            var B = Vector<double>.Build.Dense(new double[] { t1, t2 });
            var linearRoots = A.Solve(B);
            var a = (double)linearRoots[0];
            if (double.IsInfinity(a))
                return null;

            var y = Math.Round(a * c21 + Left.Y);
            var x = Math.Round(a * c11 + Left.X);

            return islimited && !InBorders((int)x, (int)y) || !other.InBorders((int)x, (int)y)
                ? null
                : new Point((int)x, (int)y);
        }

        public virtual bool InBorders(int X, int Y)
        {
            return Math.Min(Left.X, Right.X) <= X && X <= Math.Max(Left.X, Right.X)
                && Math.Min(Left.Y, Right.Y) <= Y && Y <= Math.Max(Left.Y, Right.Y);
            //if ((left.X < right.X) && (left.X <= X || X < right.X))
            //    return true;
            //if ((left.X > right.X) && (right.X < X || X <= left.X))
            //    return true;
            //if ((left.Y < right.Y) && (left.Y <= Y || Y < right.Y))
            //    return true;
            //if ((left.Y > right.Y) && (right.Y < Y || Y <= left.Y))
            //    return true;
            //return false;
        }

        public bool Equals(IBound other)
        {
            return Left != null && Right != null && other.Left != null && other.Right != null && other.Left.Equals(Left) && other.Right.Equals(Right)
                || Left != null && other.Left != null && other.Right == null && Left.Equals(other.Left)
                || Right != null && other.Right != null && other.Left == null && Right.Equals(other.Right);
        }
    }

    public class LineData : Segment
    {
        public LineData(INode left, INode right) : base(left, right) { }
        public override bool InBorders(int X, int Y) => true;
    }

    public class Ray : Segment
    {
        public Ray(INode left, INode right) : base(left, right) { }

        public override Point? CrossPoint(IBound other, bool islimited = true)
        {
            var node = base.CrossPoint(other, false);

            if (node == null)
                return null;
            var dir1 = Left.Direction(Right);
            var dir2 = Left.Direction(new Node((int)node.Value.X, (int)node.Value.Y));
            var scalar = dir1.x * dir2.x + dir1.y * dir2.y;
            return scalar > 0 ? node : null;
        }

        public override bool InBorders(int X, int Y)
        {
            var dir1 = Left.Direction(Right);
            var dir2 = Left.Direction(new Node(X, Y));
            var scalar = dir1.x * dir2.x + dir1.y * dir2.y;
            return scalar > 0;
        }
    }

    public class Border : Segment
    {
        public Border(INode left, INode right) : base(left, right)
        {
            left.Right = this;
            right.Left = this;
        }
    }
}
