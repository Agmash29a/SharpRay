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
	/// Generic Sphere object class.
	/// </summary>
	public class GENERIC_Sphere : BasicObject
	{
		// *---------------------------------------*
		// Generic Sphere Description
		// *---------------------------------------*

		// - A sphere is the points (r - radius) distance from the point (c centre) 
		// - p is a point of distance r away from c
		// - The sphere is written in terms of (p-c).(p-c)-r^2=0
		// - p can be expressed in terms of t (p=o+td) which is a quadratic (At^2+Bt+C=0)
		// - The roots are then used to determine the number of hit points

		// Special case for generic sphere r = 1, cx=0,cy=0,cz=0

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison	
		public double kEpsilon;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public GENERIC_Sphere() : base() 
		{
			kEpsilon = 0.001;
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
	
			// Find the vector between the centre of the sphere and the ray origin
			Vector3D temp = ray.o;

			// The quadratic (At^2+Bt+C=0) becomes:
			// A = d.d
			double a = ray.d * ray.d;
			// B = 2(o-c).d
			double b = 2.0 * ray.o * ray.d;
			// c = (o-c).(o-c)-r^2
			double c = ray.o * ray.o - 1;
			// Quadratics can have zero, one, or two real roots depending on the value of the discriminant (D=B^2-4AC)
			double disc = b * b - 4.0 * a * c;

			// *---------------------------------------*
			// Work out hit point
			// *---------------------------------------*

			// If D<0 there is no intersection, D=1 there is 1 (tangental), D=2 there is 2 
			if (disc < 0.0)
			{
				return false;
			}
			else
			{
				double e = Math.Sqrt(disc);
				double denom = 2.0 * a;

				// Smaller root
				t = (-b - e) / denom;		

				// Did an intersection occur?
				if (t > kEpsilon)
				{
					// Hit - return normal and ray
					tmin = t;
					normal = ray.o + t * ray.d;
					return true;
				}

				// Larger root
				t = (-b + e) / denom;		

				// Did an intersection occur?
				if (t > kEpsilon)
				{
					// Hit - return normal and ray
					tmin = t;
					normal = ray.o + t * ray.d;
					return true;
				}
			}
			return false;
		}
	}
}
