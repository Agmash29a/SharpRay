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
	/// Summary description for Ray.
	/// </summary>
	public struct Ray
	{
		// *---------------------------------------*
		// Ray Description
		// *---------------------------------------*

		// - A ray is an infinite straight line, with a point (o) at the origin, a unit vector (d) as direction, and t = 0 at the origin
		// – arbitrary point r on a ray, p = o + t d. These points are always in world coordinates.
		
		// - 4 types of rays exist: Primary, Secondary, Shadow, and Light
		//		. Primary – start at pixels
		//		. Secondary – start at ray-object hit points (reflected or transmitted)
		//		. Shadow – shadows
		//		. Light – light sources, some global illumination
		
		// We look for the hit point where t is the smallest positive number (e.g. E (Epsilon) = 10^-6) (not 0 – for shadows and recursion). 
		// Ray origin can be inside objects ,on the surface, anywhere – t can be positive, negative, 0, etc.
		// Each ray-object intersection function returns a Boolean when the ray hits with at least 1 value of t > E 

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// Origin (o), Direction (d), and t (nearest to hit point)
		//public Point3D o;
		public Vector3D o;
		public Vector3D d;
		public double t;
	}
}