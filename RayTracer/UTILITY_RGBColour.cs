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
	/// RGBColour Utility class
	/// </summary>
	public struct RGBColour
	{
		// *---------------------------------------*
		// RGBColour Description
		// *---------------------------------------*		

		// The RGBColour class stores doubles for red, green, and blue as an ordered triple.
		// Objects of this class represent colours in the RGB colour space. this gives us the following values:
 
		// -> White = (1,1,1)
		// -> Black = (0,0,0)
		// -> Red = (1,0,0)
		// -> Green = (0,1,0)
		// -> Blue = (0,0,1)
		// -> Yellow = (1,1,0)
		// -> Dark Green = (0,0.25,0)
		// -> etc.
 
		// The class also exposes Properties for the integer versions of the floating point representations
		// of r,g, and b. 

		// IMPORTANT: We are not checking in or out bound values to see if they lie within the correct 
		// Numeric ranges - this is done for speed.
		
		// RGB Operations (all return colours)
		// -----------------------------------
		// Let c = (r,g,b), c1 = (r1,g1,b1) (2 points) and c2 = (r2,g2,b2) 

		// c1 + c2 = (r1 + r2, g1 + g2, b1 + b2) 
		// ac = (ar,ag,ab)
		// ca = (ar,ag,ab)
		// c1 * c2 = (r1r2,g1g2,b1b2)
		// c^P = (r^P,g^P,b^P)

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		 //Private instances of Red, Green, and Blue
		public double r, g, b;

		// *---------------------------------------*
		// Overloaded constructor
		// *---------------------------------------*

		public RGBColour(RGBColour c)
		{
			r = c.r;
			g = c.g;
			b = c.b;
		}

		public RGBColour(double r,  double g,  double b)
		{
			this.r = r;
			this.g = g;
			this.b = b;
		}

		// *---------------------------------------*
		// Integer values of the colours
		// *---------------------------------------*
	
		/// <summary>
		/// Get the Red component (as an int)
		/// </summary>
		public int RedInteger
		{
			get
			{
				int ir = (int)(255 * r);
//				ir = ir > 255 ? 0: ir;
				ir = ir > 255 ? 255 : ir;
				return ir < 0 ? 0 : ir;
			}
		}

		/// <summary>
		/// Get the Green component (as an int)
		/// </summary>
		public int GreenInteger 
		{
			get
			{
				int ig = (int)(255 * g);
//				ig = ig > 255 ? 0 : ig;
				ig = ig > 255 ? 255 : ig;
				return ig < 0 ? 0 : ig;
			}
		}

		/// <summary>
		/// Get the Blue component (as a int)
		/// </summary>
		public int BlueInteger 
		{
			get
			{
				int ib = (int)(255 * b);
				ib = ib > 255 ? 255 : ib;
//				ib = ib > 255 ? 0 : ib;
				return ib < 0 ? 0 : ib;
			}
		}

		// *---------------------------------------*
		// Colour operations, operators are 
		// overloaded
		// *---------------------------------------*
	
		/// <summary>
		/// Colour addition c1 + c2 
		/// </summary>
		public static RGBColour operator + (RGBColour c1, RGBColour c2)
		{
			return new RGBColour(c1.r + c2.r, c1.g + c2.g, c1.b + c2.b);
		}
		/// <summary>
		/// Colour multiplication by a scalar ac
		/// </summary>
		public static RGBColour operator * (RGBColour c, double a)
		{
			return new RGBColour(c.r*a, c.g*a, c.b*a);
		}
		/// <summary>
		/// Colour multiplication by a scalar ca
		/// </summary>
		public static RGBColour operator * (double a, RGBColour c)
		{
			return new RGBColour(c.r*a, c.g*a, c.b*a);
		}
		/// <summary>
		/// Colour multiplication c1 * c2 
		/// </summary>
		public static RGBColour operator * (RGBColour c1, RGBColour c2)
		{
			return new RGBColour(c1.r * c2.r, c1.g * c2.g, c1.b * c2.b);
		}
		/// <summary>
		/// Colour to a power c^P 
		/// </summary>
		public static RGBColour operator ^ (RGBColour c, double P)
		{
			return new RGBColour(Math.Pow(c.r,P), Math.Pow(c.g,P), Math.Pow(c.b,P));
		}

		/// <summary>
		/// Colour division by a scalar 
		/// </summary>
		public static RGBColour operator / (RGBColour c1, int a)
		{
			return new RGBColour(c1.r/a, c1.g/a, c1.b/a);
		}
	}
}