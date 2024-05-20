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
	/// Object base class.
	/// </summary>
	public class BasicCamera
	{
		// *---------------------------------------*
		// Basic Camera Description
		// *---------------------------------------*

		// - This is the base class for all cameras
		
		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		public Vector3D u,v,w;		// Orthonormal basis vectors
		public Vector3D e;			// Eye point
		public Vector3D l;			// Lookat point
		public Vector3D vd;			// View direction
		public Vector3D up;			// Up vector

		public double ra;			// roll angle

		// *---------------------------------------*
		// Constructor
		// *---------------------------------------*

		public BasicCamera()
		{
		}

		// *---------------------------------------*
		// Various functions
		// *---------------------------------------*

		/// <summary>
		/// RenderScene function (to be overridden), build the scene from the camera's perspective
		/// </summary>
		public virtual void renderScene(World w)
		{
		}

		/// <summary>
		/// calculateUVW function (to be overridden), calculate orthonormal basis
		/// </summary>
		public void calculateUVW()
		{
			// w vector (view direction)
			w = e - l;
			w.Normalise();

			// u vector (perpendicular to v and w)
			u = up ^ w;
			u.Normalise();

			// v points up as a default, twisted round via roll vector (ra)
			v = w ^ u;
		}
	}
}
