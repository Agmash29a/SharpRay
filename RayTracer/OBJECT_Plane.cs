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
	/// Plane object
	/// </summary>
	public class Plane : BasicObject
	{
		// *---------------------------------------*
		// Plane Description
		// *---------------------------------------*

		// - Planes are defined by specifying a point (a) in the plane and a normal (n) to the plane (its tilt)
		// - Planes, (p – a) . n = 0 is Ax + By + Cz + D = 0
		// - Substituting --> (o + td – a) . n = 0, Which gives us linear equation t = (a – o) . n / (d . n)
		
		// - Since linear equations have a single solution (remember linear = no powers) – rays only have single intersections with the plane
		// - Calculate numerical value of t, don’t substitute back straight away (more efficient)
		// - May have to check for division by 0 if the plane is parallel with the view plane
		
		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison, (a) point in the plane, (n) normal to the plane 
		public double kEpsilon;
		//public Point3D a;
		public Vector3D a;
		//public Normal3D n;
		public Vector3D n;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Plane() : base() 
		{
			kEpsilon = 0.001;
			Vector3D a = new Vector3D(0, 0, 0);
			Vector3D n = new Vector3D(0, 0, 0);
		}

		// *---------------------------------------*
		// Hit function(s)
		// *---------------------------------------*

		/// <summary>
		/// hit function, we look for the smallest value of t between kEpsilon and infinity
		/// </summary>
		/// public override bool hit(Ray ray, ref Normal3D normal)
		public override bool hit(Ray ray, ref double tmin, ref Vector3D normal)
		{
			// - Planes are defined by specifying a point (a) in the plane and a normal (n) to the plane (its tilt)
			// - Planes, (p – a) . n = 0 is Ax + By + Cz + D = 0
			// - Substituting --> (o + td – a) . n = 0, Which gives us linear equation t = (a – o) . n / (d . n)
			t = (a - ray.o) * n /(ray.d * n);

			// Did an intersection occur?
			if (t > kEpsilon)
			{
				// Hit - return normal and ray
				tmin = t;
				normal = n;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
