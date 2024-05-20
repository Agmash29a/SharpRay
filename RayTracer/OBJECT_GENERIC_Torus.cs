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
	/// Generic Torus object class.
	/// </summary>
	public class GENERIC_Torus : BasicObject
	{
		// *---------------------------------------*
		// Torus Description
		// *---------------------------------------*

		// A torus (doughnut) centered on the origin (y axis goes through the centre of the hole) 

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (kEpsilon) Eplison, (a,b) points that make up the torus	
		public double a;	// distance from origin	
		public double b;	// radius
		public double kEpsilon;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public GENERIC_Torus(double a, double b) : base() 
		{
			kEpsilon = 0.001;
			
			this.a = a;
			this.b = b;
		}

		// *---------------------------------------*
		// Hit function(s)
		// *---------------------------------------*

		/// <summary>
		/// hit function, we look for the smallest value of t between kEpsilon and infinity
		/// </summary>
		public override bool hit(Ray ray, ref double tmin, ref Vector3D normal)
		{
			// --------------------------------------------------------
			// Build Quartic polynomial At^4 + Bt^3 + Ct^2 + Dt + E = 0
			// --------------------------------------------------------

			Vector3D d = ray.d;
			Vector3D o = ray.o;
			
			// Build parts of equation

			// (dx^2 + dy^2 + dz^2)
			double temp1 = (d.x * d.x + d.y * d.y + d.z * d.z);
			// (oxdx + oydy + ozdz)
			double temp2 = (o.x * d.x + o.y * d.y + o.z * d.z);
			// [ox^2 + oy^2 + oz^2 - (a^2 + b^2)]
			double temp3 = (o.x * o.x + o.y * o.y + o.z * o.z - (a * a + b * b));

			// Build A,B,C,D,E

			// A = (dx^2 + dy^2 + dz^2)^2
			double A = temp1 * temp1;
			// B = 4(dx^2 + dy^2 + dz^2)(oxdx + oydy + ozdz)
			double B = 4 * temp1 * temp2;
			// C = 2(dx^2 + dy^2 + dz^2)[ox^2 + oy^2 + oz^2 - (a^2 + b^2)] + 4(oxdx + oydy + ozdz)^2 + 4a^2dy^2
			double C = 2 * temp1 * temp3 + 4 * temp2 * temp2 + 4 * a * a * d.y * d.y;
			// D = 4[ox^2 + oy^2 + oz^2 - (a^2 + b^2)](oxdx + oydy + ozdz) + 8a^2oydy
			double D = 4 * temp3 * temp2 + 8 * a * a * o.y * d.y;
			// E = [ox^2 + oy^2 + oz^2 - (a^2 + b^2)]^2 - 4a^2(b^2 - o.y^2)
			double E = temp3 * temp3 - 4 * a * a * (b * b - o.y * o.y);
			
			// -----------------------
			// Find Roots (check hit)
			// -----------------------

			double[] s = new double[4];		// Solutions
			double[] c = {E,D,C,B,A};		// Coefficients
			
			double numRoots = VariousMath.SolveQuartic(c, s);
			
			if (numRoots > 0)
			{
				double lowestT = double.MaxValue;

				for (int i=0; i<numRoots; i++)
				{
					if ((s[i] < lowestT) && (s[i] > kEpsilon))
					{
						lowestT = s[i];
					}
				}

				// Check for hits
				if (lowestT != double.MaxValue)
				{
					// -----------------------
					// Calculate normal
					// -----------------------

					// p = o + t d
					Vector3D HP = o + lowestT * d;

					// nx = 4x[x^2 + y^2 + z^2 - (a^2 + b^2)]
					normal.x = 4 * HP.x * (HP.x * HP.x + HP.y * HP.y + HP.z * HP.z - (a * a + b * b));
					// ny = 4y[x^2 + y^2 + z^2 - (a^2 + b^2) + 2a^2]
					normal.y = 4 * HP.y * (HP.x * HP.x + HP.y * HP.y + HP.z * HP.z - (a * a + b * b) + 2 * a * a);
					// nz = 4z[x^2 + y^2 + z^2 - (a^2 + b^2)]
					normal.z = 4 * HP.z * (HP.x * HP.x + HP.y * HP.y + HP.z * HP.z - (a * a + b * b));

					normal.Normalise();

					tmin = lowestT;
					
					return true;
				}
			}
			// No hits
			return false;
		}
	}
}

