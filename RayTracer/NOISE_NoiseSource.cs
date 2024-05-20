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
	/// Noise source base class.
	/// </summary>
	public class BasicNoiseSource
	{
		// *---------------------------------------*
		// BasicNoiseSource Description
		// *---------------------------------------*

		// - Noise source base class, used as a ultility class to generate
		//   various kinds of noise.

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// (kTableSize) Number of entries in value table 
		// (kTableMask) bitmask
		// (SEED) Random seed
		const int kTableSize = 256;		
		const int kTableMask = kTableSize - 1;	
		const int SEED = 5;

		// Noise parameters
		public int numOctaves;
		public double lacunarity;
		public double gain;
		
		// table of randomized indices
		static int[] perm =  { 225,155, 210,108,175,199,221,144,203,116,70,213,69,158,33,252,
								 5, 82, 173,133,222,139,174,27,9,71,90,246, 75,130, 91,191,
								 169,138, 2,151,194,235, 81,7,25,113,228,159,205,253,134,142,
								 248, 65,224,217, 22,121,229,63,89,103,96,104,156,17,201,129,
								 36, 8,165,110,237,117,231, 56,132,211,152, 20,181,111,239,218,
								 170,163, 51,172,157, 47, 80,212,176,250, 87, 49, 99,242,136,189,
								 162,115, 44, 43,124, 94,150, 16,141,247, 32, 10,198,223,255, 72,
								 53,131, 84, 57,220,197, 58, 50,208, 11,241, 28, 3,192, 62,202,
								 18,215,153, 24, 76, 41, 15,179, 39, 46, 55, 6,128,167, 23,188,
								 106, 34,187,140,164, 73,112,182,244,195,227, 13, 35, 77,196,185,
								 26,200,226,119, 31,123,168,125,249, 68,183,230,177,135,160,180,
								 12, 1,243,148,102,166, 38,238,251, 37,240,126, 64, 74,161, 40,
								 184,149,171,178,101, 66, 29, 59,146, 61,254,107, 42, 86,154, 4,
								 236,232,120, 21,233,209, 45, 98,193,114, 78, 19,206, 14,118,127,
								 48, 79,147, 85, 30,207,219, 54, 88,234,190,122, 95, 67,143,109,
								 137,214,145, 93, 92,100,245, 0,216,186, 60, 83,105, 97,204, 52
							 };

		// Noise value table
		double[] valueTable = new double[kTableSize];
		Vector3D[] vectorTable = new Vector3D[kTableSize];

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*
		
		public BasicNoiseSource()
		{
			ValueTableInit(SEED);
			VectorTableInit(SEED);
		}

		public BasicNoiseSource(int octaves)
		{
			ValueTableInit(SEED);
			VectorTableInit(SEED);
			numOctaves = octaves;
		}

		public BasicNoiseSource(int octaves, double lacunarity, double gain)
		{
			ValueTableInit(SEED);
			VectorTableInit(SEED);
			numOctaves = octaves;
			this.lacunarity = lacunarity;
			this.gain = gain;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// #DEFINES
		/// </summary>
		public int PERM(int x)
		{
			return perm[(x) & kTableMask];
		}

		public int INDEX(int ix, int iy, int iz)
		{
			return PERM((ix)+PERM((iy)+PERM(iz)));
		}

		public int FLOOR(double x)
		{
			if ((x < 0) && (x != (int) x))
			{
				return (int) x - 1;
			}
			else
			{
				return (int) x;
			}
		}

		/// <summary>
		/// Initialise the integer lattice
		/// </summary>
		public void ValueTableInit(int seed)
		{
			Random r = new Random(seed);

			for (int i = 0; i < kTableSize; i++)
			{
				valueTable[i] = 1.0 - 2.0 * r.NextDouble();
			}
		}

		/// <summary>
		/// Noise at integer lattice point
		/// </summary>
		public double ValueLattice(int ix, int iy, int iz)
		{
			int x = INDEX(ix, iy,iz);
			return valueTable[x];
		}

		/// <summary>
		/// Noise at vector integer lattice point
		/// </summary>
		public Vector3D VectorLattice(int ix, int iy, int iz)
		{
			int x = INDEX(ix, iy,iz);
			return vectorTable[x];
		}

		/// <summary>
		///  Scalar noise (to be overridden)
		/// </summary>
		public virtual double ScalarNoise (Vector3D p)
		{
			return 0.0;
		}

		/// <summary>
		///  Vector noise (to be overridden)
		/// </summary>
		public virtual Vector3D VectorNoise (Vector3D p)
		{
			return new Vector3D(0,0,0);
		}

		/// <summary>
		///  Scalar turbulence
		/// </summary>
		public double ScalarTurbulence(Vector3D p)
		{
			double turbulence = 0.0;
			double scale = 1.0;

			for (int j = 0; j < numOctaves; j++)
			{
				turbulence += Math.Abs(ScalarNoise (p * scale) / scale);
				scale *= 2;
			}

			return (turbulence);
		}

		/// <summary>
		/// Scalar Fractal sum
		/// </summary>
		public double ScalarFractalSum(Vector3D p)
		{
			double fractalSum = 0.0;
			double scale = 1.0;

			for (int j = 0; j < numOctaves; j++)
			{
				fractalSum += ScalarNoise (p * scale) / scale;
				scale *= 2;
			}

			return (fractalSum);
		}

		/// <summary>
		///  Scalar fBm
		/// </summary>
		public double ScalarfBm (Vector3D p)
		{
			double sum = 0.0;
			double frequency = 1.0;
			double amplitude = 1.0;

			for (int j = 0; j < numOctaves; j++)
			{
				sum += amplitude * ScalarNoise(frequency * p);
				frequency *= lacunarity;
				amplitude *= gain;
			}
			return (sum);
		}

		/// <summary>
		///  lerp
		/// </summary>
		public double lerp(double x, double a, double b)
		{
			return a + (b - a) * x;
		}

		public void VectorTableInit(int seed)
		{
			Random r = new Random(seed);

			for(int i = 0; i < kTableSize; i++)
			{
				// Find a random vector whose end lies inside the unit sphere
				Vector3D v;
				do
				{
					v.x = 1.0 - 2.0 * r.NextDouble();
					v.y = 1.0 - 2.0 * r.NextDouble();
					v.z = 1.0 - 2.0 * r.NextDouble();
				} while (v.x * v.x + v.y * v.y + v.z * v.z > 1.0);
				
				v.Normalise(); // Put end on the unit sphere
				vectorTable[i] = v;
			}
		}
	}
}
