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
using System.Collections;

namespace RayTracer
{
	/// <summary>
	/// Grid class.
	/// </summary>
	public class Grid : BasicObject
	{
		// *---------------------------------------*
		// UniformGrid Description
		// *---------------------------------------*

		// - Uniform Grid for an acceleration scheme (for triangles)
		// - Some code structure borrowed from Filip Hron's Acceleration shceme

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public ArrayList[] triGrid;

		// GRID main Bounding Box p0 (x0,y0,z0) and p1 (x1,y1,z1)
		public double x0,y0,z0;
		public double x1,y1,z1;
		
		int nx, ny, nz;

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public Grid()
		{
			// Initialise Grid
			x0 = -10;
			y0 = -10;
			z0 = -10;

			x1 = 10;
			y1 = 10;
			z1 = 10;
		}

		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// Initialise - set up grid 
		/// </summary>
		public virtual void Initialise(ArrayList gridObjects, double m)
		{
			// calculate Wx, Wy, and Wz and nx, ny, nz
			double Wx = x1 - x0;
			double Wy = y1 - y0;
			double Wz = z1 - z0;

			double s = Math.Pow((Wx * Wy * Wz / gridObjects.Count), 1.0/3.0);

			nx = (int) Math.Round(m * Wx / s);
			ny = (int) Math.Round(m * Wy / s);
			nz = (int) Math.Round(m * Wz / s);

			// Make grid
			triGrid = new ArrayList[nx * ny * nz];

			// min and max in "subRectangle" space
			int[] subRectangleMin = new int[3];
			int[] subRectangleMax = new int[3];

			// Iterate through each triangle
			foreach(ACCELERATED_Triangle obj in gridObjects)
			{
				//Map local P0
				MapWorldToGrid(obj.x0, obj.y0, obj.z0, subRectangleMin);
				//Map local P1
				MapWorldToGrid(obj.x1, obj.y1, obj.z1, subRectangleMax);

				// Create sub-box to enclose triangle 
				for (int ix = subRectangleMin[0]; ix <= subRectangleMax[0]; ++ix)
				{
					for (int iy = subRectangleMin[1]; iy <= subRectangleMax[1]; ++iy)
					{
						for (int iz = subRectangleMin[2]; iz <= subRectangleMax[2]; ++iz)
						{
							int index = ix + nx * iy + nx * ny * iz;

							if (triGrid[index] == null)
							{
								triGrid[index] = new ArrayList();
							}
							triGrid[index].Add(obj);
						}
					}
				}
			}
		}

		/// <summary>
		/// MapWorldToGrid 
		/// </summary>
		public void MapWorldToGrid(double x, double y, double z, int[] subRectangle)
		{
			double c1 = ((x - x0) * nx / (x1 - x0));
			double c2 = ((y - y0) * ny / (y1 - y0));
			double c3 = ((z - z0) * nz / (z1 - z0));

			subRectangle[0] = (int) (c1);
			subRectangle[1] = (int) (c2);
			subRectangle[2] = (int) (c3);

			// Clamp subRectangle
			if (subRectangle[0] < 0) 
			{
				subRectangle[0] = 0;
			} 
			else
			{
				if (subRectangle[0] > (nx-1))
				{
					subRectangle[0] = nx - 1;
				}
			}

			if (subRectangle[1] < 0) 
			{
				subRectangle[1] = 0;
			} 
			else
			{
				if (subRectangle[1] > (nx-1))
				{
					subRectangle[1] = nx - 1;
				}
			}

			if (subRectangle[2] < 0) 
			{
				subRectangle[2] = 0;
			} 
			else
			{
				if (subRectangle[2] > (nx-1))
				{
					subRectangle[2] = nx - 1;
				}
			}
		}

		/// <summary>
		/// Grid's hit function
		/// </summary>
		public override bool hit(Ray ray, ref double tMin, ref Vector3D normal)
		{
			double ox = ray.o.x;
			double oy = ray.o.y;
			double oz = ray.o.z;
			double dx = ray.d.x;
			double dy = ray.d.y;
			double dz = ray.d.z;

			double txMin, tyMin, tzMin;
			double txMax, tyMax, tzMax;

			double a = 1.0 / dx;
			if (a >= 0)
			{
				txMin = (x0 - ox) * a;
				txMax = (x1 - ox) * a;
			}
			else
			{
				txMin = (x1 - ox) * a;
				txMax = (x0 - ox) * a;
			}

			double b = 1.0 / dy;
			if (b >= 0)
			{
				tyMin = (y0 - oy) * b;
				tyMax = (y1 - oy) * b;
			}
			else
			{
				tyMin = (y1 - oy) * b;
				tyMax = (y0 - oy) * b;
			}

			double c = 1.0 / dz;
			if (c >= 0)
			{
				tzMin = (z0 - oz) * c;
				tzMax = (z1 - oz) * c;
			}
			else
			{
				tzMin = (z1 - oz) * c;
				tzMax = (z0 - oz) * c;
			}
			double t0, t1;
			
			if (txMin > tyMin)
				t0 = txMin;
			else
				t0 = tyMin;
			
			if (tzMin > t0)
				t0 = tzMin;
			
			if (txMin < tyMax)
				t1 = txMax;
			else
				t1 = tyMax;

			if (tzMax < t0)
				t1 = tzMax;

			if (t0 > t1)
			{
				return false;
			}

			// calculates the initial hit point and
			// calculates the initial cell coordinates, and x, y , a nd z increments

			Vector3D p = ray.o + t0 * ray .d; // Initial hitpoint
			
			// Initial cell coordinates
			int ix = (int)Clamp((p.x - x0) * nx / (x1 - x0), 0, nx - 1);
			int iy = (int)Clamp((p.y - y0) * ny / (y1 - y0), 0, ny - 1);
			int iz = (int)Clamp((p.z - z0) * nz / (z1 - z0), 0, nz - 1);
			
			// Ray parameter increments per cell in the x, y, and z directions
			double dtx = (txMax - txMin) / nx;
			double dty = (tyMax - tyMin) / ny;
			double dtz = (tzMax - tzMin) / nz;

			double txNext, tyNext, tzNext;
			
			int ixStep, iyStep, izStep;
			int ixStop, iyStop, izStop;

			if (dx > 0)
			{
				txNext = txMin + (ix + 1) * dtx;
				ixStep = 1;
				ixStop = nx;
			}
			else
			{
				txNext = txMin + (nx - ix) * dtx;
				ixStep = - 1;
				ixStop = - 1;
			}

			if (dy > 0)
			{
				tyNext = tyMin + (iy + 1) * dty ;
				iyStep = 1;
				iyStop = ny;
			}
			else
			{
				tyNext = tyMin + (ny - iy) * dty;
				iyStep = - 1;
				iyStop = - 1;
			}

			if (dz > 0)
			{
				tzNext = tzMin + (iz + 1) * dtz;
				izStep = 1;
				izStop = nz;
			}
			else
			{
				tzNext = tzMin + (nz - iz) * dtz;
				izStep = - 1;
				izStop = - 1;
			}

			// Iterate through the grid
			while (true)
			{
				if ((txNext < tyNext) && (txNext < tzNext))
				{
					if (CheckArray(ray, ref tMin, ref normal, ix + nx * iy + nx * ny * iz))
					{
						//if (tMin < txNext)
							return true;
					}

					txNext += dtx;
					ix += ixStep;
					if (ix == ixStop)
						return (false);
				}
				else
				{
					if (tyNext < tzNext)
					{
						if (CheckArray(ray, ref tMin, ref normal, ix + nx * iy + nx * ny * iz))
						{
							//if (tMin < tyNext)
								return true;
						}

						tyNext += dty;
						iy += iyStep;

						if (iy == iyStop)
							return (false);
					}
					else
					{
						if (CheckArray(ray, ref tMin, ref normal, ix + nx * iy + nx * ny * iz))
						{
							//if (tMin < tzNext)
								return true;
						}

						tzNext += dtz;
						iz += izStep;

						if (iz == izStop)
							return (false);
					}
				}
			}
		}

		/// <summary>
		/// CheckArray
		/// </summary>
		public bool CheckArray(Ray ray, ref double tMin, ref Vector3D normal, int index)
		{
			double t = double.MaxValue;
			bool hitFound = false;

			if (triGrid[index]!= null)
			{
				if (triGrid[index].Count > 0)
				{
					// Check this grid cell's triangle array
					foreach(ACCELERATED_Triangle obj in triGrid[index])
					{
						if (obj.hit(ray, ref t, ref normal))
						{
							if (t < tMin)
							{
								tMin = t;
								this.material = obj.material;
								hitFound = true;
							}
						}
					}
				}
			}
			if (hitFound)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Clamp 
		/// </summary>
		double Clamp(double x, double min, double max)
		{
			if (x < min)
			{
				return (min);
			}
			else
			{
				if (x > max)
				{
					return (max);
				}
				else
				{
					return (x);
				}
			}
		}
	}
}

