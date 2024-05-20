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
	/// LinearNoiseSource class.
	/// </summary>
	public class LinearNoiseSource : BasicNoiseSource
	{
		// *---------------------------------------*
		// LinearNoiseSource Description
		// *---------------------------------------*

		// - LinearNoiseSource class, used to generate linear noise

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*

		public LinearNoiseSource() : base()
		{
		}

		public LinearNoiseSource(int octaves) : base(octaves)
		{
		}

		public LinearNoiseSource(int octaves, double lacunarity, double gain) : base (octaves, lacunarity, gain)
		{
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		///  Scalar noise 
		/// </summary>
		public override double ScalarNoise (Vector3D p)
		{
			double fx, fy, fz;
			double[,,] d = new double[2,2,2];

			double x0, x1, x2, x3, y0, y1, z0;
			int ix, iy, iz;	

			ix = (int) FLOOR(p.x);
			fx = p.x - ix;
			iy = (int) FLOOR(p.y);
			fy = p.y - iy;
			iz = (int) FLOOR(p.z);
			fz = p.z - iz;

			for (int k = 0; k <= 1; k++)
				for (int j = 0; j <= 1; j++)
					for (int i = 0; i <= 1; i++)
						d[k,j,i] = ValueLattice(ix+i,iy+j,iz+k);

			// Interpolate in x direction
			x0 = lerp(fx, d[0,0,0], d[0,0,1]);
			x1 = lerp(fx, d[0,1,0], d[0,1,1]);
			x2 = lerp(fx, d[1,0,0], d[1,0,1]);
			x3 = lerp(fx, d[1,1,0], d[1,1,1]);

			// Interpolate in y direction
			y0 = lerp(fy, x0, x1);
			y1 = lerp(fy, x2, x3);

			// Interpolate in z direction
			z0 = lerp(fz, y0, y1);

			return (z0);
		}
	}
}
