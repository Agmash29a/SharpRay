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
using System.Collections;

namespace RayTracer
{
	/// <summary>
	/// Compuond object class.
	/// </summary>
	public class Compound : BasicObject
	{
		// *---------------------------------------*
		// Compound Description
		// *---------------------------------------*

		// - A container for groups of objects

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// Lights and objects
		public ArrayList compoundObjects = new System.Collections.ArrayList();

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Compound() : base() 
		{
		}

		// *---------------------------------------*
		// Hit function(s)
		// *---------------------------------------*

		/// <summary>
		/// hit function, we look for the smallest value of t between kEpsilon and infinity
		/// </summary>
		public override bool hit(Ray ray, ref double tmin, ref Vector3D normal)
		{
			t = 0.0;

			Ray original = new Ray();

			// Copy original ray
			original.d = ray.d;
			original.o = ray.o;
			original.t = ray.t;

			Vector3D n = normal;
			bool hit = false;

			// Iterate through each object in the compuond list
			foreach(BasicObject obj in compoundObjects)
			{
				// Find hits
				if (obj.hit(ray, ref t, ref n) && (t < tmin))
				{
					hit = true;
					tmin = t;
					normal = n;
					material = obj.material;
				}

				// Restore original ray
				ray.d = original.d;
				ray.o = original.o;
				ray.t = original.t;
			}
			return hit;
		}
	}
}