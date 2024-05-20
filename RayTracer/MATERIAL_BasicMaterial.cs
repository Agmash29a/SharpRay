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
	/// Basic material.
	/// </summary>
	public class BasicMaterial
	{
		// *---------------------------------------*
		// Basic Material Description
		// *---------------------------------------*

		// - Base material class

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public double ka;		// Ambient reflection coefficient
		public double kd;		// diffuse reflection coefficient
		public RGBColour cd;	// diffuse colour	

		public BasicMaterial()
		{
		}
	
		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// shade - give this material some colour! (Override)
		/// </summary>
		public virtual RGBColour shade(Shading s)
		{
			return new RGBColour(0,0,0);
		}

		/// <summary>
		/// ambient - ambient base colour (Override - optional)
		/// </summary>
		public virtual double ambient(Shading s)
		{
			return s.w.ambient.ia;
		}

		/// <summary>
		/// diffuse - calculate the diffuse component (Override - optional)
		/// </summary>
		public virtual RGBColour diffuse(BasicLight light, Shading s)
		{
			RGBColour c = new RGBColour(0,0,0);	// Colour to return
			Ray shadowRay = new Ray();			// Shadow ray

			// Origin and diretion for shadow ray
			shadowRay.o = s.hitPoint;
			shadowRay.d = light.direction(s.hitPoint);

			// *---------------------------------------*
			// Check for shadows and calculate
			// the diffuse colour
			// *---------------------------------------*

			if (light.castsShadows && light.inShadows(shadowRay, s.w.worldObjects))
			{
				;// Do nothing
			}
			else
			{
				double ndotl = s.normal * shadowRay.d.Hat();

				if (ndotl > 0.0)
				{
					c = light.cl(s) * ndotl;
				}
			}
			return c;
		}

		/// <summary>
		/// reflectedRay - produce a reflected ray
		/// </summary>
		public virtual Ray reflectedRay(Shading s)
		{
			Ray r = new Ray();			// Reflected ray

			r.o = s.hitPoint;
			double ddotn = s.ray.d * s.normal;
			r.d = s.ray.d - 2.0 * ddotn * s.normal;

			return r;
		}
	}
}
