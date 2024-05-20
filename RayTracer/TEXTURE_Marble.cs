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
using System.Drawing;

namespace RayTracer
{
	/// <summary>
	/// Marble Texutre.
	/// </summary>
	public class Marble : BasicTexture
	{
		// *---------------------------------------*
		// Marble Texutre Description
		// *---------------------------------------*

		// - Marble Texture class

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// The Noise
		public CubicNoiseSource cubicNoiseSource; 

		// The physical "size" of the noise
		public double noiseSize = 1;

		// The amount of noise
		public double Ta = 0;

		// Pixel related variables
		public int xp = 1;
		public int xRes;

		// The ramp bitmap
		private Bitmap bitmap;

		// Noise brightness
		public double noiseBrightness = 0;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Marble(string imagePath, double noiseSize, int numOctaves, double Ta, double noiseBrightness)
		{
			cubicNoiseSource =  new CubicNoiseSource(numOctaves);
			this.noiseSize = noiseSize;
			this.Ta = Ta;
			cubicNoiseSource.numOctaves = numOctaves;

			bitmap = new Bitmap(imagePath);

			this.xRes = bitmap.Width;
			
			this.noiseBrightness = noiseBrightness;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// getColour - return colour given a hitpoint
		/// </summary>
		public override RGBColour getColour(Vector3D hitPoint)
		{
			Vector3D newPoint = Transform(hitPoint);
			
			double y = newPoint.y + Ta * cubicNoiseSource.ScalarFractalSum(newPoint);
		
			Color col = new Color();

			double V = (1.0 + Math.Sin(y)) / 2.0;
			
			xp = (int) Math.Floor(V * xRes);
			col = bitmap.GetPixel(xp, 1);

			return new RGBColour(noiseBrightness  + (double) col.R / 255 * V , noiseBrightness  + (double) col.G / 255 * V, noiseBrightness  + (double) col.B / 255 * V);
		}

		/// <summary>
		/// Transform - modify with noise size
		/// </summary>
		public Vector3D Transform(Vector3D hitPoint)
		{
			return (new Vector3D(hitPoint.x * noiseSize ,hitPoint.y * noiseSize ,hitPoint.z * noiseSize));
		}
	}
}