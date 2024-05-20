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
	public class TexturedReflective : TexturedPhong
	{		
		// *---------------------------------------*
		// Textured Reflective (Phong) material Description
		// *---------------------------------------*

		// - A Textured Reflective mirror type material that inherits from textured phong

		// - WARNING - this material only WORKS in certain circumstances, it may not look any
		//	           good if it itself is relflected

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		BasicTexture Tex;	// Reflective Map

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public TexturedReflective(BasicTexture Tex, BasicTexture cdTex) : base (cdTex)
		{	
			// This class has 2 textures (cdTex also)
			this.Tex = Tex;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// shade - give this material some colour!
		/// </summary>
		public override RGBColour shade(Shading s)
		{
			RGBColour c = base.shade(s);	// Calculate direct shading

			// Check Reflective map
			RGBColour colour = Tex.getColour(s.localHitPoint);

			double kr = (colour.r + colour.g + colour.b) / 3;

			if (kr > 0)
			{
				Ray ray = reflectedRay(s);
				c += kr * s.w.traceRay(ray, s.depth + 1);
			}
			return c;
		}
	}
}
