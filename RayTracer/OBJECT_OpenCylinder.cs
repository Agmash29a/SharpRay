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
	/// OpenCylinder object class.
	/// </summary>
	public class OpenCylinder : BasicObject
	{
		// *---------------------------------------*
		// Cylinder Description
		// *---------------------------------------*

		// A cylinder - 1 of 3 possible types, where normals are calculated differently

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison, (r) Radius of the sphere	
		public double r;		
		public double kEpsilon;
		public Vector3D centre;		//Centre of the object
		public double height;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public OpenCylinder() : base() 
		{
			r = 0.0;
			kEpsilon = 0.001;
			centre = new Vector3D();
			height = 0;
		}

		// *---------------------------------------*
		// Hit function(s)
		// *---------------------------------------*

		/// <summary>
		/// hit function, we look for the smallest value of t between kEpsilon and infinity
		/// </summary>
		public override bool hit(Ray ray, ref double tmin, ref Vector3D normal)
		{
			double ox = ray.o.x;
			double oy = ray.o.y;
			double oz = ray.o.z;
			double dx = ray.d.x;
			double dy = ray.d.y;
			double dz = ray.d.z;

			double temp1 = ox - centre.x;
			double temp2 = oz - centre.z;

			double a = dx * dx + dz * dz;
			double b = 2.0 * (temp1 * dx + temp2 * dz);
			double c = temp1 * temp1 + temp2 * temp2 - r * r;

			double disc = b * b - 4.0 * a * c;

			if (disc < 0.0)
			{
				return false;
			}
			else
			{
				double e = Math.Sqrt(disc);
				double denom = 2.0 * a;

				t = (-b -e) / denom;			// Smaller root

				if (t > kEpsilon)
				{
					double yhit = oy + t * dy;

					if (Math.Abs(yhit - centre.y) < height / 2.0)
					{
						tmin = t;
						normal.x = (temp1 + t * dx) /r;
						normal.y = 0.0;
						normal.z = (temp2 + t * dz) / r;
						return true;
					}
				}

				t = (-b + e) / denom;			// Larger root

				if (t > kEpsilon)
				{
					double yhit = oy + t * dy;

					if (Math.Abs(yhit - centre.y) < height / 2.0)
					{
						tmin = t;
						normal.x = (temp1 + t * dx) /r;
						normal.y = 0.0;
						normal.z = (temp2 + t * dz) / r;
						return true;
					}
				}
			}
			return false;
		}
	}
}
