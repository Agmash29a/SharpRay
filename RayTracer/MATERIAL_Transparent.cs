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
	/// Reflective (specular) material
	/// </summary>
	public class Transparent : Phong
	{		
		// *---------------------------------------*
		// Transparent (Phong) material Description
		// *---------------------------------------*

		// - A Transparent type material that inherits from phong

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public double kr;	// Local Specular exponent
		public double kt;	// Transmission coefficient
		public double ior;	// Index of refraction

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Transparent()
		{
			ka = 0;
			kd = 0;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// totalInternalReflection - test for TIR
		/// </summary>
		public bool totalInternalReflection(Shading s)
		{
			Vector3D outward = new Vector3D(-s.ray.d);	// Points away from the hit point
			outward.Normalise();						// Ray may have been transformed

			double cosTheta1 = outward * s.normal;
			double eta = ior;

			if (cosTheta1<0.0)							// Ray hits inside of object
			{
				eta = 1.0 / eta;
			}
			return (1.0 - (1.0 - cosTheta1 * cosTheta1) / (eta * eta) < 0.0);
		}

		/// <summary>
		/// transmittedRay - Calculate transmitted ray
		/// </summary>
		public Ray transmittedRay(Shading s)
		{
			Ray transmittedRay = new Ray();
			transmittedRay.o = s.hitPoint;
			Vector3D normal = s.normal;

			Vector3D outward = new Vector3D(-s.ray.d);
			outward.Normalise();
			double cosTheta1 = outward * normal;
			double eta = ior;

			if (cosTheta1 < 0.0)		// ray hits inside of object
			{
				cosTheta1 = -cosTheta1;	// change sign of cos theta 1
				normal = -normal;		// reverse direction of normal
				eta = 1.0 / eta;		// invert ior
			}

			double temp = 1.0 - (1.0 - cosTheta1 * cosTheta1) / (eta * eta);
			double cosTheta2 = Math.Sqrt(temp);
			transmittedRay.d = s.ray.d / eta - (cosTheta2 - cosTheta1 / eta) * normal;

			return transmittedRay;
		}

		/// <summary>
		/// shade - give this material some colour!
		/// </summary>
		public override RGBColour shade(Shading s)
		{
			RGBColour c = base.shade(s);					// Calculate direct shading

			Ray ray = reflectedRay(s);
			Ray tray = transmittedRay(s);

			if (totalInternalReflection(s))
			{
				c += s.w.traceRay(ray, s.depth + 1);	// kr = 1
			}
			else
			{
				c += kr * s.w.traceRay(ray, s.depth + 1);
				c += kt * s.w.traceRay(tray, s.depth + 1);
			}
			return c;
		}
	}
}

