using System;

namespace Algebra
{
    public class Vector2D
    {
        public Vector2D()
        {
            this.X = 0d;
            this.Y = 0d;
        }
        
        public Vector2D(double val)
        {
            this.X = val;
            this.Y = val;
        }
        
        public Vector2D(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
        
        public double X { get; private set; }
        public double Y { get; private set; }
        public double Magnitude { get { return Math.Sqrt(this.X * this.X + this.Y * this.Y); } }
        public double Direction { 
            get {
                return this.Y >= 0 ? Math.Atan2(this.Y, this.X) : 
                    2 * Math.PI + Math.Atan2(this.Y, this.X);
            }
        }
        
        public double Dot(Vector2D vec)
        {
            return this.X * vec.X + this.Y * vec.Y;
        }
        
        public Matrix ToMatrix()
        {
            return new Matrix(new[,] { { this.X, this.Y } });
        }
        
        public static Vector2D operator +(Vector2D vec1, Vector2D vec2)
        {
            double x = vec1.X + vec2.X;
            double y = vec1.Y + vec2.Y;
            return new Vector2D(x, y);
        }
        
        public static Vector2D operator -(Vector2D vec1, Vector2D vec2)
        {
            double x = vec1.X - vec2.X;
            double y = vec1.Y - vec2.Y;
            return new Vector2D(x, y);
        }
    }
}
