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
	/// Vector3D utility class
	/// </summary>
	public struct Vector3D
	{
		// *---------------------------------------*
		// Vector3D Description
		// *---------------------------------------*

		// - A vector is a directed line segment, u =(ux,uy,uz)
		// - Affine transforms multiply a vector by a 4x4 matrix u’ = mu where m is the matrix
		// - Length (magnitude) = |u|

		// Vector Operations
		// -----------------
		// Let u = (ux,uy,uz) and v = (vx,vy,vz) (2 vectors)
		// All operations returns vectors except the dot product

		// u + v = (ux + vx, uy + vy, uz + vz)
		// u – v = (ux – vx, uv – vy, uz – vz)
		// au = (aux, auy, auz)
		// ua = (aux, auy, auz)
		// u / a = (ux/a, uy/a, uz/a)
		// u . v = uxvx + uyvy + uzvz (dot product – returns a scalar single value)
		// u x v = (uyvz – uzvy, uzvx – uxvz, uxvy – uyvx) (cross product)
		// –u = (-ux, -uy, -uz)
		// u += v = (ux + vx, uy + vy, uz + vz)
		// u -= v = (ux – vx, uy – vy, uz – vz)
		// |u| = sqrt(ux*ux + uy*uy + uz*uz)

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// x, y, and z coordinates
		public double x, y, z;

		// *---------------------------------------*
		// Overloaded constructor
		// *---------------------------------------*

		public Vector3D(Vector3D v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
		}

		public Vector3D(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// *---------------------------------------*
		// Normalisation and Magnitude routines
		// *---------------------------------------*

		/// <summary>
		/// Normalise this instance vector, that is - convert it to a unit vector, which has length 1
		/// To do this, divide each component by the length of the vector
		/// </summary>
		public void Normalise()
		{
			double d = Math.Sqrt(x*x + y*y + z*z);
			x/= d; 
			y/= d; 
			z/= d;
		}

		/// <summary>
		/// Normalise a new vector instance and return the instance (leave the original, but copy the values)
		/// This is called hat because unit vectors have a circumfles ^ on top
		/// </summary>
		public Vector3D Hat()
		{
			Vector3D v = new Vector3D(this);

			double d = Math.Sqrt(x*x + y*y + z*z);
			v.x /= d;
			v.y /= d; 
			v.z /= d;

			return v;
		}

		/// <summary>
		/// Magnitude of this vector u |u|
		/// </summary>
		public double Magnitude()
		{
			return Math.Sqrt(x*x + y*y + z*z);
		}

		// *---------------------------------------*
		// Vector operations, operators are 
		// overloaded
		// *---------------------------------------*
	
		/// <summary>
		/// Vector addition u + v 
		/// </summary>
		public static Vector3D operator + (Vector3D u, Vector3D v)
		{
			return new Vector3D(u.x + v.x, u.y + v.y, u.z + v.z);
		}

		/// <summary>
		/// Vector subtraction u - v
		/// </summary>
		public static Vector3D operator - (Vector3D u, Vector3D v)
		{
			return new Vector3D(u.x - v.x, u.y - v.y, u.z - v.z);
		}

		/// <summary>
		/// Vector multiplication by a scalar ua
		/// </summary>
		public static Vector3D operator * (Vector3D u, double a)
		{
			return new Vector3D(a*u.x, a*u.y, a*u.z);
		}

		/// <summary>
		/// Vector multiplication by a scalar au
		/// </summary>
		public static Vector3D operator * (double a, Vector3D u)
		{
			return new Vector3D(a*u.x, a*u.y, a*u.z);
		}

		/// <summary>
		/// Vector division by a scalar u / a
		/// </summary>
		public static Vector3D operator / (Vector3D u, double a)
		{
			return new Vector3D(u.x/a, u.y/a, u.z/a);
		}

		/// <summary>
		/// Dot product u . v
		/// </summary>
		public static double operator * (Vector3D u, Vector3D v)
		{
			return u.x*v.x + u.y*v.y + u.z*v.z;
		}

		/// <summary>
		/// Cross product u x v (we use ^ for the cross product)
		/// </summary>
		public static Vector3D operator ^ (Vector3D u, Vector3D v)
		{
			return new Vector3D(u.y*v.z - u.z*v.y, u.z*v.x - u.x*v.z, u.x*v.y - u.y*v.x);
		}

		/// <summary>
		/// Vector negation -u
		/// </summary>
		public static Vector3D operator - (Vector3D u)
		{
			return new Vector3D(-u.x, -u.y, -u.z);
		}
	}
}