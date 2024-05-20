//000000000000000000000000000000000000000000000000000000000000000000000
//000000000000000000011000000000011000000000000000000000000000000000000
//000000011111100000101000110000111000011000000000001111110000000000000
//000000100001100001010000110001110000110000000000001000111100000000000
//000011000001100010100001110011010000110000000000001100001100000000000
//000011000011000111000000100010100001110000001110001100111000010000000
//000110000011001110100000100011000000100000110100001111000001111000000
//000110000000001101100111100110000101100000110001111100000010011011000
//000011000000111110111001111001111001100011111110011111001100010100000
//000011111111001100000001100001100001111110001000010001110000111000000
//000000011000000000000000000000000001111000000000010000011100111000000
//000000000000000000000000000000000000000000000000000000000001011000000
//000000000000000000000000000000000000000000000000000000000010110000000
//(c) 2004 by Riley T. Perry - Chillers of Entropy

using System;

namespace RayTracer
{
	/// <summary>
	/// Point3D Utility class.
	/// </summary>
	public struct Point3D
	{
		// *---------------------------------------*
		// Point3D Description
		// *---------------------------------------*

		// - Each point has location vector (x,y,z) but points are different
		// - Points can be subtracted – the result is the vector that joins them
		// - Distance can be calculated by 2 points
		// - A vector can be added to the point which gives a point 
		// - Points under affine transformations are different to vectors, translation affects points

		// Point Operations
		// ----------------
		// Let a = (ax,ay,az) and b = (bx,by,bz) (2 points) u = (u1,u2,u3) (vector) m = a matrix

		// a + u = (ax+ux,ay+uy,az+uz) – a point
		// a – u = (ax-ux,ay-uy,az-uz) – a point
		// a - b = (bx-ax,by-ay,bz-az) – a vector
		// |a – b| = [(ax-bx)^2+(ay-by)^2+(az-bz)^2]^1/2 - a scalar (we can use Vector3D.Magnitude() for this)
		// a’ = ma - a point

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.

		//x, y, and z coordinates
		public double x, y, z;

		// *---------------------------------------*
		// Overloaded constructor
		// *---------------------------------------*

		public Point3D(Point3D p)
		{
			x = p.x;
			y = p.y;
			z = p.z;
		}

		public Point3D(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// *---------------------------------------*
		// Point operations, operators are 
		// overloaded
		// *---------------------------------------*
	
		/// <summary>
		/// Point + Vector addition a + u 
		/// </summary>
		public static Point3D operator + (Point3D a, Vector3D u)
		{
			return new Point3D(a.x + u.x, a.y + u.y, a.z + u.z);
		}
			
		/// <summary>
		/// Point - Vector addition a - u 
		/// </summary>
		public static Point3D operator - (Point3D a, Vector3D u)
		{
			return new Point3D(a.x - u.x, a.y - u.y, a.z - u.z);
		}
		
		/// <summary>
		/// Point - Point (get vector) a - b 
		/// </summary>
		public static Vector3D operator - (Point3D a, Point3D b)
		{
			return new Vector3D(b.x - a.x, b.y - a.y, b.z - a.z);
		}
		
		/// <summary>
		/// Explicit Conversion for Point3D to Vector3D
		/// </summary>
		public static explicit operator Vector3D (Point3D p)
		{
			return new Vector3D(p.x, p.y, p.z);
		}
	}
}