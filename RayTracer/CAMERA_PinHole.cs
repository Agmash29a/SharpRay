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
	/// Pinhole camera.
	/// </summary>
	public class PinHoleCamera : BasicCamera
	{
		// *---------------------------------------*
		// Pinhole Camera Description
		// *---------------------------------------*

		// - A pinhole camera

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public int d;	// Distance between the eye point and the window

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public PinHoleCamera() : base()
		{
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// RenderScene function, build the scene from the camera's perspective
		/// </summary>
		public override void renderScene(World w)
		{
			Ray	ray = new Ray();	// Primary ray
			RGBColour pixelColour = new RGBColour();	// The final colour

			Point2D pp;		// Jitter sample point
			Point2D sp;		// Jitter sample point

			ray.o = e;		// Pinhole ray origin
			
			// *---------------------------------------*
			// Iterate through each point in the view 
			// plane
			// *---------------------------------------*

			for(int row = 0; row<w.vp.vRes; row++)
			{
				for(int col = 0; col<w.vp.hRes; col++)
				{
					// *---------------------------------------*
					// Trace ray and run through jitter samples
					// *---------------------------------------*

					for (int j = 0; j < w.vp.numSamples; j++)
					{
						// A value of 1 means no anti-alias
						if (w.vp.numSamples > 1)
						{
							sp = w.vp.sampleUnitSquare();
						}
						else
						{
							sp.x = 1;
							sp.y = 1;
						}

						pp.x = w.vp.PixelSize * (col - w.vp.hRes / 2.0 + sp.x);
						pp.y = w.vp.PixelSize * (row - w.vp.vRes / 2.0 + sp.y);

						ray.d = rayDirection(pp);

						pixelColour += w.traceRay(ray, 0);
					}

					pixelColour /= w.vp.numSamples;

					// Fill in temp screen buffer
					w.screenArray[col,row]= new RGBColour(pixelColour.r,pixelColour.g,pixelColour.b);
					
					// Restore Colour
					pixelColour.r = 0;
					pixelColour.g = 0;
					pixelColour.b = 0;
				}
			}
		}

		/// <summary>
		/// rayDirection - calculate the direction of the ray (relative
		/// to the orthomormal basis) 
		/// </summary>
		public Vector3D rayDirection(Point2D p)
		{
			Vector3D dir = (p.x * u) + (p.y * v) - (d * w);
			dir.Normalise();

			return dir;
		}
	}
}

