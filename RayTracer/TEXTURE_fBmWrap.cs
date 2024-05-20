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
	/// fBm Wrap Texutre.
	/// </summary>
	public class fBmWrap : BasicTexture
	{
		// *---------------------------------------*
		// fBmWrap Texutre Description
		// *---------------------------------------*

		// - fBmWrap Texutre class

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// The Noise
		public CubicNoiseSource cubicNoiseSource; 

		// The physical "size" of the noise
		public double noiseSize = 1;

		// Noise colour
		public double noiseModifierR = 1;
		public double noiseModifierG = 1;
		public double noiseModifierB = 1;

		// Noise brightness
		public double noiseBrightness = 0;

		// 2ndry colour modifier
		public double c = 1;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public fBmWrap(double noiseSize, double noiseModifierR, double noiseModifierG, double noiseModifierB, double noiseBrightness, int numOctaves, double lacunarity, double gain, double c)
		{
			cubicNoiseSource =  new CubicNoiseSource(numOctaves);
			this.noiseSize = noiseSize;
			this.noiseModifierR = noiseModifierR;
			this.noiseModifierG = noiseModifierG;
			this.noiseModifierB = noiseModifierB;
			this.noiseBrightness = noiseBrightness;
			cubicNoiseSource.numOctaves = numOctaves;
			cubicNoiseSource.gain = gain;
			cubicNoiseSource.lacunarity = lacunarity;
			this.c = c;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// getColour - return colour given a hitpoint
		/// </summary>
		public override RGBColour getColour(Vector3D hitPoint)
		{
			double g = 2 * cubicNoiseSource.ScalarfBm(Transform(hitPoint));
			g = g - Math.Floor(g);

			return new RGBColour(g * c * noiseModifierR + noiseBrightness, g * c * noiseModifierG + noiseBrightness, g * c * noiseModifierB + noiseBrightness);
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