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
	/// Directional Light.
	/// </summary>
	public class Directional : BasicLight
	{		
		// *---------------------------------------*
		// Directional Description
		// *---------------------------------------*

		// - A simle directional light

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public double p;	// Power of the light
		public Vector3D d;	// Direction of the lighht

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Directional(double p, Vector3D d)
		{
			this.p = p;
			this.d = d;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// cl - calculate the power of the light
		/// </summary>
		public override RGBColour cl(Shading s)
		{
			return (p * c);
		}

		/// <summary>
		/// direction - return the direction of the light source
		/// </summary>
		public override Vector3D direction(Vector3D h)
		{
			// All light is projected in 1 direction
			return d;
		}

		
		/// <summary>
		/// inShadows - project backwards shadow rays from hit points
		/// if the ray hits an object then the original hit point is in shadow
		/// </summary>
		public override bool inShadows(Ray ray, ArrayList worldObjects)
		{
			Vector3D normal = new Vector3D(0,0,0);
			double tmin = double.MaxValue;

			// *---------------------------------------*
			// Check objects in way of the shadow ray
			// *---------------------------------------*

			Ray original = new Ray();

			// Copy original ray
			original.d = ray.d;
			original.o = ray.o;
			original.t = ray.t;

			foreach(BasicObject obj in worldObjects)
			{
				// t is omitted
				if (obj.hit(ray, ref tmin, ref normal)) 
				{
					return true;
				}

				// Restore original ray
				ray.d = original.d;
				ray.o = original.o;
				ray.t = original.t;
			}
			return false;
		}
	}
}
