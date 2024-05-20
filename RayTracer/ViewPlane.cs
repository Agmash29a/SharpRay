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
	/// View plane container.
	/// </summary>
	public class ViewPlane
	{
		// *---------------------------------------*
		// ViewPlane Description
		// *---------------------------------------*

		// - View plane container
	
		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public int hRes;		// Resolution
		public int vRes;
		public int PixelSize;	// Size of pixels
		public int numSamples;	// No of jitter anti-alias samples
		public int maxDepth;	// Maximum no. of recursive ray bounces (for reflections)

		Random r =new Random();	// Random number for jitter

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*

		public ViewPlane()
		{
			maxDepth = 0;		// Default is no recursive ray bouncing 
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// sampleUnitSquare - generate random values for 
		/// point coordinates
		/// </summary>
		public Point2D sampleUnitSquare()
		{
			Point2D p = new Point2D();

			p.x = (double)r.Next(0,1000)/1000;
			p.y = (double)r.Next(0,1000)/1000;

			return p;
		}
	}
}
