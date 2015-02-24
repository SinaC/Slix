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
            return new Vector2D(v.X * d, v.Y * d);
        }

        public static Vector2D operator *(double d, Vector2D v)
        {
            return new Vector2D(v.X * d, v.Y * d);
        }

        public static Vector2D operator /(Vector2D v, double d)
        {
            return new Vector2D(v.X / d, v.Y / d);
        }

        public static Vector2D operator +(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X + w.X, v.Y + w.Y);
        }

        public static Vector2D operator -(Vector2D v, Vector2D w)
        {
            return new Vector2D(v.X - w.X, v.Y - w.Y);
        }

        public static Vector2D Rotate(Vector2D v, double angle)
        {
            double cs = Math.Cos(angle);
            double sn = Math.Sin(angle);
            double x = sn * v.X + cs * v.Y;
            double y = cs * v.X - sn * v.Y;
            return new Vector2D(x, y);
        }

        public void Normalize()
        {
            double invLength = 1.0 / Length;
            X *= invLength;
            Y *= invLength;
        }

        public double Length
        {
            get { return Math.Sqrt(Length2); }
        }

        public double Length2
        {
            get { return X * X + Y * Y; }
        }

        public static double Dot(Vector2D v, Vector2D w)
        {
            return v.X * w.X + v.Y * w.Y;
        }

        public override string ToString()
        {
            return String.Format("{0:F6};{1:F7}", X, Y);
        }
    }
}
