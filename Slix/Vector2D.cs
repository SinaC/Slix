using System;

namespace Slix
{
    public class Vector2D
    {
        public static Vector2D NullObject = new Vector2D(0, 0);

        public double X { get; set; }
        public double Y { get; set; }

        public Vector2D()
            : this(0, 0)
        {
        }

        public Vector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vector2D operator -(Vector2D v)
        {
            return new Vector2D(-v.X, -v.Y);
        }

        public static Vector2D operator *(Vector2D v, double d)
        {
            return new Vector2D(v.X*d, v.Y*d);
        }

        public static Vector2D operator *(double d, Vector2D v)
        {
            return new Vector2D(v.X*d, v.Y*d);
        }

        public static Vector2D operator /(Vector2D v, double d)
        {
            return new Vector2D(v.X/d, v.Y/d);
        }

        public static Vector2D operator +(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X + w.X, v.Y + w.Y);
        }

        public static Vector2D operator -(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X - w.X, v.Y - w.Y);
        }

        public static double DotProduct(Vector2D v, Vector2D w)
        {
            return v.X*w.X + v.Y*w.Y;
        }

        //cross product, in 2d, is a scalar since we know it points in the Z direction
        public static double CrossProduct(Vector2D v1, Vector2D v2)
        {
            return (v1.X*v2.Y) - (v1.Y*v2.X);
        }

        public static Vector2D Rotate(Vector2D v, double angle)
        {
            double cs = Math.Cos(angle);
            double sn = Math.Sin(angle);
            double x = cs * v.X - sn * v.Y;
            double y = sn * v.X + cs * v.Y;
            return new Vector2D(x, y);
        }

        public Vector2D Project(Vector2D v)
        {
            //project this vector on to v
            //projected vector = (this dot v) * v;
            double thisDotV = DotProduct(this, v);
            return v * thisDotV;
        }

        public Vector2D Project(Vector2D v, out double mag)
        {
            //project this vector on to v, return signed magnatude
            //projected vector = (this dot v) * v;
            double thisDotV = DotProduct(this, v);
            mag = thisDotV;
            return v * thisDotV;
        }

        public void Normalize()
        {
            double invLength = 1.0/Length;
            X *= invLength;
            Y *= invLength;
        }

        public double Length
        {
            get { return Math.Sqrt(Length2); }
        }

        public double Length2
        {
            get { return X*X + Y*Y; }
        }

        public override string ToString()
        {
            return String.Format("{0:F10};{1:F7}", X, Y);
        }
    }
}
