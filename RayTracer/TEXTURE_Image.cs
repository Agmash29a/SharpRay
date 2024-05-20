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
	/// Image Texutre.
	/// </summary>
	public class ImageTexture : BasicTexture
	{
		// *---------------------------------------*
		// Image Texutre Description
		// *---------------------------------------*

		// - Image Texutre class

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// (hRes) Horizontal, (vRes) Vertical resolution, (bitmap) Local bitmap, (mappingPtr) local map pointer
		public int xRes;
		public int yRes;
		private Bitmap bitmap;
		private BasicMapping mappingPtr;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public ImageTexture(string imagePath, BasicMapping mappingPtr)
		{
			bitmap = new Bitmap(imagePath);

			this.xRes = bitmap.Width;
			this.yRes = bitmap.Height;

			this.mappingPtr = mappingPtr;
		}
	
		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// getColour - return colour given a hitpoint
		/// </summary>
		public override RGBColour getColour(Vector3D hitPoint)
		{
			int xp = 0;
			int yp = 0;

			mappingPtr.getPixelCoords(hitPoint, xRes, yRes, ref xp, ref yp);

			Color colour = bitmap.GetPixel(xp, yp);

			return new RGBColour((double) colour.R / 255,(double) colour.G / 255,(double) colour.B / 255);
		}
	}
}


