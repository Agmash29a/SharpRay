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
	/// Generic Cube object class.
	/// </summary>
	public class GENERIC_Cube : BasicObject
	{
		// *---------------------------------------*
		// Generic Cube Description
		// *---------------------------------------*

		// - Build a 6 faced axis alighned box, that is a unit cube centered on the origin

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison, (p1,p0) points that make up the box
		public double kEpsilon;

		// Unit cube values
		private Vector3D p0 = new Vector3D(-1,-1,-1);
		private Vector3D p1 = new Vector3D(1,1,1);

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public GENERIC_Cube() : base() 
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
			double tHit;
			double num = 0, denom = 0;
			double tIn = -1000000;	// kHugeValuekHugeVal;
			double tOut = 1000000;	// kHugeValue;

			short faceIn = 0, faceOut = 0;

			for (short face = 1; face <= 6; face++)
			{
				switch (face)
				{
					case 1: // +x face
						num = p1.x - ray.o.x;
						denom = ray.d.x;
						break;

					case 2: // +y face
						num = p1.y - ray.o.y ;
						denom = ray.d.y ;
						break;

					case 3: // +z face
						num = p1.z - ray.o.z;
						denom = ray.d.z ;
						break;

					case 4: // -x face
						num = -p0.x + ray.o.x;
						denom = -ray.d.x;
						break;

					case 5: // -y face
						num = -p0.y + ray.o.y;
						denom = -ray.d.y ;
						break;

					case 6: // -z face
						num = -p0.z + ray.o.z;
						denom = -ray.d.z;
						break;
				}

				if (Math.Abs(denom) == 0.0)	// Ray is parallel to a face
				{
					if (num < 0.0)			// Ray is out
					{
						return false;
					}
				}
				else
				{
					tHit = num / denom;

					if (denom > 0.0)		// Exiting
					{
						if (tHit < tOut)	// A new closer exit
						{
							tOut = tHit;
							faceOut = face;
						}
					}
					else					// Entering
					{
						if (tHit > tIn)		// A new farther entrance
						{
							tIn = tHit;
							faceIn = face;
						}
					}
				}
			} // End of for loop

			if (tIn > tOut)
			{
				return false;
			}

			if (tIn > kEpsilon)				// hit from outside
			{
				tmin = tIn;
				normal = findNormal(faceIn);
				return true;
			}

			if (tOut > kEpsilon)			// hit from inside or on surface
			{
				tmin = tOut;
				normal = findNormal(faceOut);
				return true;
			}
			return false;
		}

		/// <summary>
		/// findNormal - find the normal for a particular face
		/// </summary>
		public Vector3D findNormal(int faceHit)
		{
			switch (faceHit)
			{
				case 1: return (new Vector3D(1, 0, 0));		// +x face
				case 2: return (new Vector3D(0, 1 , 0));	// +y face
				case 3: return (new Vector3D(0, 0, 1));		// +z face
				case 4: return (new Vector3D(-1, 0, 0));	// -x face
				case 5: return (new Vector3D(0, -1, 0));	// -y face
				case 6: return (new Vector3D(0, 0, -1));	// -z face
			}

			// Dummy return
			return (new Vector3D(0, 0, -1));				// -z face
		}
	}
}

