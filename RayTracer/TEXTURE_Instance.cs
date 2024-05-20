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
	/// Texture instance class.
	/// </summary>
	public class TextureInstance : BasicTexture
	{
		// *---------------------------------------*
		// Texture Instance Description
		// *---------------------------------------*

		// - An "instance" of an texture, used for "intrinsicly" transforming textures
		
		// - For transforms, a transformation matix is built up of (several) transforms
		// - The hitpoint itself is transformed (using points)

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// -> The float type can represent values ranging from approximately 1.5 × 10-45 to 3.4 × 1038 with a precision of 7 digits.
		// -> The double type can represent values ranging from approximately 5.0 × 10-324 to 1.7 × 10308 with a precision of 15-16 digits.
		
		// (invMatrix) Inverse Matrix, (texturePtr) Texture Pointer	
		public Matrix3D invMatrix;
		public BasicTexture texturePtr;

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*

		public TextureInstance(BasicTexture texturePtr) : base() 
		{
			invMatrix = new Matrix3D();
			this.texturePtr = texturePtr;
			invMatrix.MakeIdentityMatrix();
		}
	
		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// getColour - return colour given a hitpoint
		/// </summary>
		public override RGBColour getColour(Vector3D hitPoint)
		{
			// Save original hitPoint

			Vector3D rayCopy = new Vector3D(hitPoint.x, hitPoint.y, hitPoint.z);
			
			transformAsPoint(ref rayCopy);
			return texturePtr.getColour(rayCopy);
		}

		// *---------------------------------------*
		// Transformation helper functions
		// *---------------------------------------*

		/// <summary>
		/// transformAsPoint
		/// </summary>
		private void transformAsPoint(ref Vector3D o)
		{
			//ox' = m00ox + m01oy + m02oz + m03.
			//oy' = m10ox + m11oy + m12oz + m13.
			//oz' = m20ox + m21oy + m22oz + m23.
			
			double ox;
			double oy;
			double oz;

			ox = invMatrix.m[0,0] * o.x + invMatrix.m[0,1] * o.y + invMatrix.m[0,2] * o.z + invMatrix.m[0,3];
			oy = invMatrix.m[1,0] * o.x + invMatrix.m[1,1] * o.y + invMatrix.m[1,2] * o.z + invMatrix.m[1,3];
			oz = invMatrix.m[2,0] * o.x + invMatrix.m[2,1] * o.y + invMatrix.m[2,2] * o.z + invMatrix.m[2,3];

			o.x = ox;
			o.y = oy;
			o.z = oz;
		}

		// *---------------------------------------*
		// Affine Transformations
		// *---------------------------------------*

		/// <summary>
		/// rotateX
		/// </summary>
		public void rotateX(double r)
		{
			Matrix3D invXRot = new Matrix3D(); // temporary x axis rotation matrix
			invXRot.MakeIdentityMatrix();

			double sinTheta;
			double cosTheta;

			if (r!=0.0)
			{
				sinTheta = Math.Sin(VariousMath.AnglesToRads(r));
				cosTheta = Math.Cos(VariousMath.AnglesToRads(r));
				invXRot.m[1,1] = cosTheta;
				invXRot.m[2,1] = -sinTheta;
				invXRot.m[1,2] = sinTheta;
				invXRot.m[2,2] = cosTheta;
				invMatrix = invMatrix * invXRot; // multiply on right
			}
		}
	
		/// <summary>
		/// rotateY
		/// </summary>
		public void rotateY(double r)
		{
			Matrix3D invYRot = new Matrix3D(); // temporary y axis rotation matrix
			invYRot.MakeIdentityMatrix();

			double sinTheta;
			double cosTheta;

			if (r!=0.0)
			{
				sinTheta = Math.Sin(VariousMath.AnglesToRads(r));
				cosTheta = Math.Cos(VariousMath.AnglesToRads(r));
				invYRot.m[0,0] = cosTheta;
				invYRot.m[0,2] = -sinTheta;
				invYRot.m[2,0] = sinTheta;
				invYRot.m[2,2] = cosTheta;
				invMatrix = invMatrix * invYRot; // multiply on right
			}
		}
			
		/// <summary>
		/// rotateZ
		/// </summary>
		public void rotateZ(double r)
		{
			Matrix3D invZRot = new Matrix3D(); // temporary z axis rotation matrix
			invZRot.MakeIdentityMatrix();

			double sinTheta;
			double cosTheta;

			if (r!=0.0)
			{
				sinTheta = Math.Sin(VariousMath.AnglesToRads(r));
				cosTheta = Math.Cos(VariousMath.AnglesToRads(r));
				invZRot.m[0,0] = cosTheta;
				invZRot.m[0,1] = sinTheta;
				invZRot.m[1,0] = -sinTheta;
				invZRot.m[1,1] = cosTheta;
				invMatrix = invMatrix * invZRot; // multiply on right
			}
		}

		/// <summary>
		/// Translate
		/// </summary>
		public void Translate(double dx, double dy, double dz)
		{
			Matrix3D inv = new Matrix3D(); // temporary matrix
			inv.MakeIdentityMatrix();

			inv.m[0,3] = -dx;
			inv.m[1,3] = -dy;
			inv.m[2,3] = -dz;

			invMatrix = invMatrix * inv; // multiply on right
		}

		/// <summary>
		/// Scale
		/// </summary>
		public void Scale(double a, double b, double c)
		{
			Matrix3D inv = new Matrix3D(); // temporary matrix
			inv.MakeIdentityMatrix();

			inv.m[0,0] = 1.0/a;
			inv.m[1,1] = 1.0/b;
			inv.m[2,2] = 1.0/c;

			invMatrix = invMatrix * inv; // multiply on right
		}

		/// <summary>
		/// Shear
		/// </summary>
		public void Shear(double hyx,
			double hzx,
			double hxy,
			double hzy,
			double hxz,
			double hyz)
		{
			Matrix3D inv = new Matrix3D(); // temporary matrix
			inv.MakeIdentityMatrix();

			// Determinant of M - the shearing matrix
			double D = 1.0 - hxy * hyx - hxz * hzx - hyz * hzy + hxy * hyz * hzx + hxz * hyx * hzy;
			
			// Inverse of D
			double Dinv = 1.0/D;

			// Shearing matrix (Inverse of M)
			inv.m[0,0] = 1.0 - hyz * hzy;
			inv.m[0,1] = -hyx + hyz * hzx;
			inv.m[0,2] = -hzx + hyx * hzy;

			inv.m[1,0] = -hxy + hxz * hzy;
			inv.m[1,1] = 1 - hxz * hzx;
			inv.m[1,2] = - hzy + hxy * hzx;

			inv.m[2,0] = -hxz + hxy * hyz;
			inv.m[2,1] = -hyz + hxz * hyx;
			inv.m[2,2] = 1 - hxy * hyx;

			inv.m[3,3] = D;

			invMatrix = (invMatrix * (inv * Dinv)); // multiply on right
		}
	}
}

