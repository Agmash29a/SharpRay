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
	/// Phong (specular) material
	/// </summary>
	public class TexturedPhong : BasicMaterial
	{		
		// *---------------------------------------*
		// Phong material Description
		// *---------------------------------------*

		// - Phong specular material, used for texturing

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public double ks;	// Specular component
		public double n;	// Specular exponent

		BasicTexture cdTex;	// Diffuse colour

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public TexturedPhong(BasicTexture cdTex)
		{
			this.cdTex = cdTex;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// shade - give this material some colour!
		/// </summary>
		public override RGBColour shade(Shading s)
		{
			// Calculate base colour
			RGBColour c	= ka * cdTex.getColour(s.localHitPoint) * ambient(s);
			
			// *---------------------------------------*
			// Illumination loop
			// *---------------------------------------*

			foreach(BasicLight lgt in s.w.worldLights)
			{
				// Calculate additive colour with diffuse and specular components
				c+= kd * cdTex.getColour(s.localHitPoint) * diffuse(lgt,s) + ks * specular(lgt,s);
			}

			return c;
		}

		/// <summary>
		/// specular - calculate the specular component
		/// </summary>
		public RGBColour specular(BasicLight light, Shading s)
		{
			RGBColour c = new RGBColour(0,0,0);			// Colour to return

			Vector3D l = light.direction(s.hitPoint);	// Light direction
			Vector3D v = -s.ray.d;						// Ray direction

			// Origin and diretion for shadow ray
			Ray shadowRay = new Ray();
			shadowRay.o = s.hitPoint;
			shadowRay.d = light.direction(s.hitPoint);

			l.Normalise();
			v.Normalise();

			double ndotl = s.normal * l; 
	
			Vector3D r = -l + 2.0 * s.normal * ndotl;	// Reflected direction

			double rdotv = r * v;

			// *---------------------------------------*
			// Check for shadows and calculate
			// the specular colour
			// *---------------------------------------*

			if (light.castsShadows && light.inShadows(shadowRay, s.w.worldObjects))
			{
				;// Do nothing
			}
			else
			{
				if (rdotv > 0.0)
				{
					c = light.cl(s) * Math.Pow(rdotv,n);
				}
			}
			return c;
		}
	}
}
