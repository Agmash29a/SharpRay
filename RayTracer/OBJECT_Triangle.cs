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
	/// Triangle object class.
	/// </summary>
	public class Triangle : BasicObject
	{
		// *---------------------------------------*
		// Triangle Description
		// *---------------------------------------*

		// - A triangle's unit normal is, n = (b-a)^(c-a)/|(b-a)^(c-a)|
		// - where a,b, and c are the 3 points in the triangle
		// - The trianlge sits on a plane, we then define infinite straight lines
		// - that pass through two sets of points (we can extaroplate the third)

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison, (v1-v3) points that make up the triangle (n) normal	
		public Vector3D n;		
		public double kEpsilon;
		public Vector3D v1,v2,v3;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Triangle(Vector3D v1, Vector3D v2, Vector3D v3) : base() 
		{
			this.v1 = v1;
			this.v2 = v2;
			this.v3 = v3;

			kEpsilon = 0.001;

			// Normalise now for speed
			n = (v2-v1)^(v3-v1);
			n.Normalise();
		}

		// *---------------------------------------*
		// Hit function(s)
		// *---------------------------------------*

		/// <summary>
		/// hit function, we look for the smallest value of t between kEpsilon and infinity
		/// </summary>
		public override bool hit(Ray ray, ref double tmin, ref Vector3D normal)
		{
			t=0.0;

			double a = v1.x-v2.x, b = v1.x-v3.x, c = ray.d.x, d = v1.x-ray.o.x;
			double e = v1.y-v2.y, f = v1.y-v3.y, g = ray.d.y, h = v1.y-ray.o.y;
			double i = v1.z-v2.z, j = v1.z-v3.z, k = ray.d.z, l = v1.z-ray.o.z;
			
			double m = f * k - g * j, n = h * k - g * l, p = f * l - h * j;
			double q = g * i - e * k, r = e * l - h * i, s = e * j - f * i;
			
			double e1 = d * m - b * n - c * p;
			double e2 = a * n + d * q + c * r;
			
			double D = a * m + b * q + c * s;
			double beta = e1 / D;
			double gamma = e2 / D;

			if (beta + gamma < 1.0 && beta > 0.0 && gamma > 0.0)
			{
				double e3 = a * p - b * r + d * s;
				t = e3 / D;

				if (t > kEpsilon)
				{
					tmin = t;
					normal = this.n; // main n, not the local
					return true;
				}
			}
			return false;
		}
	}
}

