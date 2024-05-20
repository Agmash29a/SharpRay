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
	/// Normal3D utility class
	/// </summary>
	public struct Normal3D
	{
		// *---------------------------------------*
		// Normal3D Description
		// *---------------------------------------*

		// - Like vectors (directed line segments) – but different in the way that they are always perpendicular to object surfaces and must remain perpendicular when the object is transformed. 
		// - There’s lots that can’t be done with normals, but a dot product of a normal and a vector needs to be done, anmongst other ops.
		
		// Normal Operations
		// -----------------
		// Let n = (nx,ny,nz) (a normal), u = (ux,uy,uz) (a vector), a = (ax,zy,az) (a point)

		// –n = (-nx,-ny-nz) – gives a normal
		// n . u = nxux + nyuy + nzuz – gives a scalar
		// u . n = nxux + nyuy + nzuz – gives a scalar
		// a . n = nxax + nyay + nzaz – gives a scalar
		// an = (anx,any,anz) – gives a normal
		// na = (anx,any,anz) – gives a normal
		// n + u = (nx+ux, ny+uy, nz+uz) – gives a vector
		// u + n = (ux+nx, uy+ny, uz+nz) – gives a vector
		// n = u = (ux,uy,uz) – normal reference 

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

		public Normal3D(Normal3D n)
		{
			x = n.x;
			y = n.y;
			z = n.z;
		}

		public Normal3D(double x, double y, double z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		// *---------------------------------------*
		// Normalisation routines
		// *---------------------------------------*

		/// <summary>
		/// Normalise this instance normal, that is - convert it to a unit normal, which has length 1
		/// </summary>
		public void Normalise()
		{
			double d = Math.Sqrt(x*x + y*y + z*z);
			x/= d; 
			y/= d; 
			z/= d;
		}

		/// <summary>
		/// Normalise a new normal instance and return the instance (leave the original, but copy the values)
		/// This is called hat because unit vectors have a circumfles ^ on top
		/// </summary>
		public Normal3D Hat()
		{
			Normal3D n = new Normal3D(this);

			double d = Math.Sqrt(x*x + y*y + z*z);
			n.x /= d;
			n.y /= d; 
			n.z /= d;

			return n;
		}

		// *---------------------------------------*
		// Normal operations, operators are 
		// overloaded
		// *---------------------------------------*
	
		/// <summary>
		/// Normal negation -n
		/// </summary>
		public static Normal3D operator - (Normal3D n)
		{
			return new Normal3D(-n.x, -n.y, -n.z);
		}

		/// <summary>
		/// Dot product n . u
		/// </summary>
		public static double operator * (Normal3D n, Vector3D u)
		{
			return n.x*u.x + n.y*u.y + n.z*u.z;
		}
		/// <summary>
		/// Dot product u . n
		/// </summary>
		public static double operator * (Vector3D u, Normal3D n)
		{
			return n.x*u.x + n.y*u.y + n.z*u.z;
		}
		/// <summary>
		/// Dot product a . n
		/// </summary>
		public static double operator * (Point3D a, Normal3D n)
		{
			return n.x*a.x + n.y*a.y + n.z*a.z;
		}
		/// <summary>
		/// Normal multiplication by a scalar na
		/// </summary>
		public static Normal3D operator * (Normal3D n, double a)
		{
			return new Normal3D(a*n.x, a*n.y, a*n.z);
		}

		/// <summary>
		/// Normal multiplication by a scalar an
		/// </summary>
		public static Normal3D operator * (double a, Normal3D n)
		{
			return new Normal3D(a*n.x, a*n.y, a*n.z);
		}

		/// <summary>
		/// Normal + Vector addition n + u 
		/// </summary>
		public static Normal3D operator + (Normal3D n, Vector3D u)
		{
			return new Normal3D(n.x + u.x, n.y + u.y, n.z + u.z);
		}
	
		/// <summary>
		/// Vector + Normal addition u + n 
		/// </summary>
		public static Normal3D operator + (Vector3D u, Normal3D n)
		{
			return new Normal3D(n.x + u.x, n.y + u.y, n.z + u.z);
		}

		/// <summary>
		/// Explicit Conversion for Vector3D to Normal3D
		/// </summary>
		public static explicit operator Normal3D (Vector3D v)
		{
			return new Normal3D(v.x, v.y, v.z);
		}
	}
}