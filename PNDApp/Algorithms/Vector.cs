using System;
using System.Windows;

namespace PNDApp.Algorithms
{
    /// <summary>
    /// Represents a two-dimensional vector.
    /// </summary>
    public class Vector
    {
        /// <summary>
        /// Gets or sets the X component of the vector.
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets the Y component of the vector.
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Initialises a new <see cref="Vector"/> object with the specified coordinates.
        /// </summary>
        public Vector(Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        /// <summary>
        /// Initialises a new <see cref="Vector"/> object with the specified magnitude and direction.
        /// </summary>
        public Vector(double magnitude, double direction)
        {
            X = magnitude * Math.Cos((Math.PI / 180.0) * direction);
            Y = magnitude * Math.Sin((Math.PI / 180.0) * direction);
        }

        /// <summary>
        /// Adds two vectors together.
        /// </summary>
        public static Vector operator +(Vector v1, Vector v2)
        {
            var x = v1.X + v2.X;
            var y = v1.Y + v2.Y;
            return new Vector(new Point(x, y));
        }

        /// <summary>
        /// Multiplies a vector by a scalar value.
        /// </summary>
        public static Vector operator *(Vector v, double multiplier)
        {
            var x = v.X*multiplier;
            var y = v.Y * multiplier;
            return new Vector(new Point(x, y));
        }

        /// <summary>
        /// Returns a <see cref="Point"/> that is equivalent to the vector.
        /// </summary>
        public Point toPoint()
        {
            return new Point(X, Y);
        }

        /// <summary>
        /// Returns vector's magnitude.
        /// </summary>
        public double Lenght
        {
            get { return Math.Sqrt(X*X + Y*Y); }
        }

        /// <summary>
        /// Returns a string representation of the vector.
        /// </summary>
        public override string ToString()
        {
            return String.Format("({0}, {1})", X, Y);
        }
    }
}
