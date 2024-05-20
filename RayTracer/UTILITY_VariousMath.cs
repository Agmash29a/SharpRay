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
	/// Various math functions
	/// </summary>
	public class VariousMath
	{		
		// *---------------------------------------*
		// VariousMath Description
		// *---------------------------------------*		

		// - Various math functions, all methods are STATIC so
		// - no instances need be made

		// *---------------------------------------*
		// Global Variables (and constants)
		// *---------------------------------------*		
		public const double EQN_EPS = 1e-30;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		static VariousMath()
		{
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// Angles to Rads - convert angles (deg) to Rads
		/// </summary>
		public static double AnglesToRads(double angle)
		{
			return Math.PI * angle / 180.0;
		}

		
		/// <summary>
		/// Cube Root
		/// </summary>
		public static double cbrt(double x)
		{
			double y = 1.0/3.0;

			if (x>0.0)
			{
				return Math.Pow(x,y);
			}
			else if (x<0.0)
			{
				// Watch out for negative values!
				return -Math.Pow(-x,y);
			}
			else
			{
				return 0.0;
			}
		}

		/// <summary>
		/// Is Zero
		/// </summary>
		public static bool IsZero(double x)
		{
			/* epsilon surrounding for near zero values */
			if (x > -EQN_EPS && x < EQN_EPS)
			{
				return true;
			}
			return false;
		}

		/*
		*  Roots3And4.c
		*
		*  Utility functions to find cubic and quartic roots,
		*  coefficients are passed like this:
		*
		*      c[0] + c[1]*x + c[2]*x^2 + c[3]*x^3 + c[4]*x^4 = 0
		*
		*  The functions return the number of non-complex roots and
		*  put the values into the s array.
		*
		*  Author:         Jochen Schwarze (schwarze@isa.de)
		*
		*  Jan 26, 1990    Version for Graphics Gems
		*  Oct 11, 1990    Fixed sign problem for negative q's in SolveQuartic
		*  	    	    (reported by Mark Podlipec),
		*  	    	    Old-style function definitions,
		*  	    	    IsZero() as a macro
		*  Nov 23, 1990    Some systems do not declare Math.Acos() and cbrt() in
		*                  <math.h>, though the functions exist in the library.
		*                  If large coefficients are used, EQN_EPS should be
		*                  reduced considerably (e.g. to 1E-30), results will be
		*                  correct but multiple roots might be reported more
		*                  than once.
		*----------------------------------------------
		* April 19, 2004 Converted to C# by Riley Perry
		*----------------------------------------------
		*/

		public static int SolveQuadric(double[] c, double[] s)
			//double c[ 3 ];
			//double s[ 2 ];
		{
			double p, q, D;

			/* normal form: x^2 + px + q = 0 */

			p = c[ 1 ] / (2 * c[ 2 ]);
			q = c[ 0 ] / c[ 2 ];

			D = p * p - q;

			if (IsZero(D))
			{
				s[ 0 ] = - p;
				return 1;
			}
			else if (D < 0)
			{
				return 0;
			}
			else if (D > 0)
			{
				double sqrt_D = Math.Sqrt(D);

				s[ 0 ] =   sqrt_D - p;
				s[ 1 ] = - sqrt_D - p;
				return 2;
			}
			
			// Dummy return
			return 0;
		}

		public static int SolveCubic(double[] c, double[] s)
			//double c[ 4 ];
			//double s[ 3 ];
		{
			int     i, num;
			double  sub;
			double  A, B, C;
			double  sq_A, p, q;
			double  cb_p, D;

			/* normal form: x^3 + Ax^2 + Bx + C = 0 */

			A = c[ 2 ] / c[ 3 ];
			B = c[ 1 ] / c[ 3 ];
			C = c[ 0 ] / c[ 3 ];

			/*  substitute x = y - A/3 to eliminate quadric term:
			x^3 +px + q = 0 */

			sq_A = A * A;
			p = 1.0/3 * (- 1.0/3 * sq_A + B);
			q = 1.0/2 * (2.0/27 * A * sq_A - 1.0/3 * A * B + C);

			/* use Cardano's formula */

			cb_p = p * p * p;
			D = q * q + cb_p;

			if (IsZero(D))
			{
				if (IsZero(q)) /* one triple solution */
				{
					s[ 0 ] = 0;
					num = 1;
				}
				else /* one single and one double solution */
				{
					double u = cbrt(-q);
					s[ 0 ] = 2 * u;
					s[ 1 ] = - u;
					num = 2;
				}
			}
			else if (D < 0) /* Casus irreducibilis: three real solutions */
			{
				double phi = 1.0/3 * Math.Acos(-q / Math.Sqrt(-cb_p));
				double t = 2 * Math.Sqrt(-p);

				s[ 0 ] =   t * Math.Cos(phi);
				s[ 1 ] = - t * Math.Cos(phi + Math.PI / 3);
				s[ 2 ] = - t * Math.Cos(phi - Math.PI / 3);
				num = 3;
			}
			else /* one real solution */
			{
				double sqrt_D = Math.Sqrt(D);
				double u = cbrt(sqrt_D - q);
				double v = - cbrt(sqrt_D + q);

				s[ 0 ] = u + v;
				num = 1;
			}

			/* resubstitute */

			sub = 1.0/3 * A;

			for (i = 0; i < num; ++i)
				s[ i ] -= sub;

			return num;
		}

		public static int SolveQuartic(double[] c, double[] s)
			//double c[ 5 ]; 
			//double s[ 4 ];
		{
			double[]  coeffs = new double[ 4 ];
			double  z, u, v, sub;
			double  A, B, C, D;
			double  sq_A, p, q, r;
			int     i, num;

			/* normal form: x^4 + Ax^3 + Bx^2 + Cx + D = 0 */

			A = c[ 3 ] / c[ 4 ];
			B = c[ 2 ] / c[ 4 ];
			C = c[ 1 ] / c[ 4 ];
			D = c[ 0 ] / c[ 4 ];

			/*  substitute x = y - A/4 to eliminate cubic term:
			x^4 + px^2 + qx + r = 0 */

			sq_A = A * A;
			p = - 3.0/8 * sq_A + B;
			q = 1.0/8 * sq_A * A - 1.0/2 * A * B + C;
			r = - 3.0/256*sq_A*sq_A + 1.0/16*sq_A*B - 1.0/4*A*C + D;

			if (IsZero(r))
			{
				/* no absolute term: y(y^3 + py + q) = 0 */

				coeffs[ 0 ] = q;
				coeffs[ 1 ] = p;
				coeffs[ 2 ] = 0;
				coeffs[ 3 ] = 1;

				num = SolveCubic(coeffs, s);

				s[ num++ ] = 0;
			}
			else
			{
				/* solve the resolvent cubic ... */

				coeffs[ 0 ] = 1.0/2 * r * p - 1.0/8 * q * q;
				coeffs[ 1 ] = - r;
				coeffs[ 2 ] = - 1.0/2 * p;
				coeffs[ 3 ] = 1;

				SolveCubic(coeffs, s);

				/* ... and take the one real solution ... */

				z = s[ 0 ];

				/* ... to build two quadric equations */

				u = z * z - r;
				v = 2 * z - p;

				if (IsZero(u))
					u = 0;
				else if (u > 0)
					u = Math.Sqrt(u);
				else
					return 0;

				if (IsZero(v))
					v = 0;
				else if (v > 0)
					v = Math.Sqrt(v);
				else
					return 0;

				coeffs[ 0 ] = z - u;
				coeffs[ 1 ] = q < 0 ? -v : v;
				coeffs[ 2 ] = 1;

				num = SolveQuadric(coeffs, s);

				coeffs[ 0 ]= z + u;
				coeffs[ 1 ] = q < 0 ? v : -v;
				coeffs[ 2 ] = 1;
 
				// *------------------------------------------*
				// Calc s + num

				int oldnum = num;

				double[] sPlusnum = new double[s.Length - num];
				
				for (int it=0; it<(s.Length - num); it++)
				{
					sPlusnum[it] = s[it+num];
				}
				// *------------------------------------------*

				num += SolveQuadric(coeffs, sPlusnum);

				// *------------------------------------------*

				for (int it=0; it<(s.Length - oldnum); it++)
				{
					s[it+oldnum] = sPlusnum[it];
				}

				// *------------------------------------------*
			}

			/* resubstitute */

			sub = 1.0/4 * A;

			for (i = 0; i < num; ++i)
				s[ i ] -= sub;

			return num;
		}
	}
}
