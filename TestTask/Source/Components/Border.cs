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
        public INode left { get ; }
        public INode right { get ; }

        public Segment(INode left, INode right)
        {
            this.left = left;
            this.right = right;
        }

        public double CalculateAngle(INode other, bool isForward)
        {
            if (isForward)
            {
                var dir1 = right.Direction(left);
                var dir2 = other.Direction(right);
                return Math.Acos(dir1.Item1 * dir2.Item1 + dir1.Item2 * dir2.Item2);
            }
            var dir_1 = left.Direction(right);
            var dir_2 = other.Direction(left);
            return Math.Acos(dir_1.Item1 * dir_2.Item1 + dir_1.Item2 * dir_2.Item2);
        }

        public virtual Nullable<Point> CrossPoint(IBound other, bool islimited = true)
        {
            if (islimited)
            {
                if (Math.Max(left.Y, right.Y) < Math.Min(other.left.Y, other.right.Y))
                    return null;
                if (Math.Min(left.Y, right.Y) > Math.Max(other.left.Y, other.right.Y))
                    return null;
                if (Math.Max(left.X, right.X) < Math.Min(other.left.X, other.right.X))
                    return null;
                if (Math.Min(left.X, right.X) > Math.Max(other.left.X, other.right.X))
                    return null;
            }
            var c11 = right.X - left.X;
            var c21 = right.Y - left.Y;
            var c12 = -(other.right.X - other.left.X);
            var c22 = -(other.right.Y - other.left.Y);
            var t1 = other.left.X - left.X;
            var t2 = other.left.Y - left.Y;
            Matrix<Double> A = DenseMatrix.OfArray(new double[,] { { c11, c12 }, { c21, c22 } });
            Vector<Double> B = Vector<Double>.Build.Dense(new double[] { t1, t2 });
            var linearRoots = A.Solve(B);
            var a = (double)linearRoots[0];
            if (double.IsInfinity(a))
                return null;

            var y = Math.Round(a * c21 + left.Y);
            var x = Math.Round(a * c11 + left.X);
            if (islimited)
            {
                if (!InBorders((int)x, (int)y))
                    return null;
            }
            if (!other.InBorders((int)x, (int)y))
                return null;
            return new Point((int)x, (int)y);
        }

        public virtual bool InBorders(int X, int Y)
        {
            if (Math.Min(left.X, right.X) > X || X > Math.Max(left.X, right.X))
                return false;
            if (Math.Min(left.Y, right.Y) > Y || Y > Math.Max(left.Y, right.Y))
                return false;
            return true;
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
            if (left != null && right != null)
            {
                if (other.left != null && other.right != null)
                {
                    if (other.left.Equals(left) && other.right.Equals(right))
                        return true;
                }
                return false;
            }
            if (left != null && other.left != null)
            {
                if (other.right != null)
                    return false;
                if (left.Equals(other.left))
                    return true;
                return false;
            }
            if (right != null && other.right != null)
            {
                if (other.left != null)
                    return false;
                if (right.Equals(other.right))
                    return true;
                return false;
            }
            return false;
        }
    }

    public class LineData : Segment
    {

        public LineData(INode left, INode right): base(left, right)
        { }


        public override bool InBorders(int X, int Y)
        {
            return true;
        }
    }


    public class Ray: Segment
    {
        public Ray(INode left, INode right) : base(left, right)
        { }

        public override Point? CrossPoint(IBound other, bool islimited = true)
        {
            var node = base.CrossPoint(other, false);

            if (node == null)
                return null;
            var dir1 = left.Direction(right);
            var dir2 = left.Direction(new Node((int)node.Value.X, (int)node.Value.Y));
            var scalar = dir1.Item1 * dir2.Item1 + dir1.Item2 * dir2.Item2;
            if (scalar > 0)
                return node;
            else
                return null;
        }

        public override bool InBorders(int X, int Y)
        {
            var dir1 = left.Direction(right);
            var dir2 = left.Direction(new Node(X, Y));
            var scalar = dir1.Item1 * dir2.Item1 + dir1.Item2 * dir2.Item2;
            if (scalar > 0)
                return true;
            else
                return false;
        }
    }


    public class Border:Segment
    {

        public Border(INode left, INode right): base(left, right)
        {
            left.Right = this;
            right.Left = this;
        }

    }
}
