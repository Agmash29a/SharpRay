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
	/// CheckerBoard Texutre.
	/// </summary>
	public class CheckerBoardTexture : BasicTexture
	{
		// *---------------------------------------*
		// CheckerBoard Texutre Description
		// *---------------------------------------*

		// - CheckerBoard Texutre class

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// (size) Size, (colour1, colour2) checker colours
		public double size;
		public RGBColour colour1;
		public RGBColour colour2;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public CheckerBoardTexture(double size, RGBColour colour1, RGBColour colour2)
		{
			this.size = size;
			this.colour1 = colour1;
			this.colour2 = colour2;
		}
	
		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// getColour - return colour given a hitpoint
		/// </summary>
		public override RGBColour getColour(Vector3D hitPoint)
		{
			bool U = posmod(hitPoint.x, 2 * size) < size;
			bool V = posmod(hitPoint.y, 2 * size) < size;
			bool W = posmod(hitPoint.z, 2 * size) < size;
			
			if (U ^ V ^ W)
				return(colour1);
			else
				return(colour2);
		}

		/// <summary>
		/// poss mod - positive modulus function
		/// </summary>
		private double posmod (double x, double a)
		{
			double value = x % a;

			if (value < 0.0)
				value += a;
			return (value);
		}
	}
}