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
	/// CubicNoiseSource class.
	/// </summary>
	public class CubicNoiseSource : BasicNoiseSource
	{
		// *---------------------------------------*
		// CubicNoiseSource Description
		// *---------------------------------------*

		// - CubicNoiseSource class, used to generate cubic noise

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*

		public CubicNoiseSource() : base()
		{
		}

		public CubicNoiseSource(int octaves) : base(octaves)
		{
		}

		public CubicNoiseSource(int octaves, double lacunarity, double gain) : base (octaves, lacunarity, gain)
		{
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		///  Cubic Scalar noise 
		/// </summary>
		public override double ScalarNoise (Vector3D p)
		{
			int ix, iy, iz;
			double fx, fy, fz;

			double[] xknots = new double[4];
			double[] yknots = new double[4];
			double[] zknots = new double[4];

			ix = FLOOR(p.x);
			fx = p.x - ix;
			iy = FLOOR(p.y);
			fy = p.y - iy;
			iz = FLOOR(p.z);
			fz = p.z - iz;

			for (int k = -1; k <= 2; k++)
			{
				for (int j = -1; j <= 2; j++)
				{
					for (int i = -1; i <= 2; i++)
						xknots[i+1] = ValueLattice(ix+i,iy+j,iz+k);
					yknots[j+1] = FourKnotSpline (fx, xknots);
				}
				zknots[k+1] = FourKnotSpline (fy, yknots);
			}
			return (FourKnotSpline (fz, zknots));
		}

		/// <summary>
		///  Cubic Vector noise 
		/// </summary>
		public override Vector3D VectorNoise(Vector3D p)
		{
			int ix, iy, iz;
			double fx, fy, fz;

			Vector3D[] xknots = new Vector3D[4];
			Vector3D[] yknots = new Vector3D[4];
			Vector3D[] zknots = new Vector3D[4];

			ix = FLOOR(p.x);
			fx = p.x - ix;
			iy = FLOOR(p.y);
			fy = p.y - iy;
			iz = FLOOR(p.z);
			fz = p.z - iz;

			for (int k = -1; k <= 2; k++)
			{
				for (int j = -1; j <= 2; j++)
				{
					for (int i = -1; i <= 2; i++)
						xknots[i+1] = VectorLattice(ix+i,iy+j,iz+k);
					yknots[j+1] = VectorFourKnotSpline(fx, xknots);
				}
				zknots[k+1] = VectorFourKnotSpline(fy, yknots);
			}
			return (VectorFourKnotSpline(fz, zknots));
		}

		/// <summary>
		/// FourKnotSpline 
		/// </summary>
		double FourKnotSpline(double x, double[] knots)
		{
			double c3 = -0.5*knots[0] + 1.5*knots[1] - 1.5*knots[2] + 0.5*knots[3];
			double c2 = knots[0] - 2.5*knots[1] + 2.0*knots[2] - 0.5*knots[3];
			double c1 = 0.5*(-knots[0] + knots[2]);
			double c0 = knots[1];

			return ((c3*x + c2)*x + c1)*x + c0;
		}

		/// <summary>
		/// VectorFourKnotSpline 
		/// </summary>
		Vector3D VectorFourKnotSpline(double x, Vector3D[] knots)
		{
			Vector3D c3 = -0.5*knots[0] + 1.5*knots[1] - 1.5*knots[2] + 0.5*knots[3];
			Vector3D c2 = knots[0] - 2.5*knots[1] + 2.0*knots[2] - 0.5*knots[3];
			Vector3D c1 = 0.5*(-knots[0] + knots[2]);
			Vector3D c0 = knots[1];

			return ((c3*x + c2)*x + c1)*x + c0;
		}
	}
}

