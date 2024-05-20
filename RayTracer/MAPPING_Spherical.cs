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
	/// Spherical Mapping class.
	/// </summary>
	public class Spherical : BasicMapping
	{
		// *---------------------------------------*
		// Spherical Mapping Description
		// *---------------------------------------*

		// - Spherical class for maps, used to map a texture to a sphere

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Spherical()
		{	
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		public override void getPixelCoords(Vector3D hitPoint, int xRes, int yRes, ref int xp, ref int yp)
		{
			double x = hitPoint.x;
			double y = hitPoint.y;
			double z = hitPoint.z;

			// First calculate theta and phi
			double theta = Math.Acos(z);
			double phi = Math.Atan2(y, x);

			// Now map these to (U, V) in [0, 1] X [0, 1]
			double U;
			if (phi >= 0.0)
				U = phi / (2.0 * Math.PI);
			else
				U = 1 + phi / (2.0 * Math.PI);
			double V = 1 -theta / Math.PI;

			// Now map U and V to pixel coordinates
			// This code is the same for all mappings
			xp = (int) ((xRes - 1) * U); // xp is across
			yp = (int) ((yRes - 1) * V); // yp is up
		}
	}
}
