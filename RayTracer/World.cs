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
using System.IO;
using System.Collections;
using System.Threading;

namespace RayTracer
{
	/// <summary>
	/// World class.
	/// </summary>
	public class World
	{
		// *---------------------------------------*
		// World Description
		// *---------------------------------------*

		// In world coordinates (Xw, Yw, Zw), y points up and z points forward
		// We also use right-handed systems

		// *---------------------------------------*
		// Global Variables
		// *---------------------------------------*

		// Lights and objects
		public ArrayList worldObjects = new System.Collections.ArrayList();
		public ArrayList worldLights = new System.Collections.ArrayList();
		
		public RGBColour[,] screenArray;		// Temp screen buffer
		public ViewPlane vp = new ViewPlane();	// View plane
		public AmbientLight ambient;			// World ambinet colour

		// *---------------------------------------*
		// Overloaded Constructor
		// *---------------------------------------*

		public World()
		{
		}
		
		// *---------------------------------------*
		// Various function(s)
		// *---------------------------------------*

		/// <summary>
		/// trace Ray - Shade if an object is hit, otherwise
		/// use the ambient colour
		/// </summary>
		public RGBColour traceRay(Ray ray, int depth) 
		{
			// Test to see if reflections maxDepth has been reached
			// If not then call the shading material which will in turn 
			// call this function again...

			if(depth > vp.maxDepth)
			{
				return(new RGBColour(0,0,0));
			}
			else
			{
				Shading s = hitObjects(ref ray);

				if(s.hitObject)
				{
					s.depth = depth;
					s.ray = ray;

					return s.material.shade(s);
				}
				else 
				{
					return ambient.c;
				}
			}
		}

		/// <summary>
		/// hit Objects - Project ray and see if it hits an object
		/// </summary>
		public Shading hitObjects(ref Ray ray)
		{
			Shading s = new Shading(this);			// Shading container
			double minDistance = double.MaxValue;	// kHugeValue
			Vector3D normal = new Vector3D();		// Returned normal
			bool hit;								// hit? an object
			double t = double.MaxValue;
			
			Ray original = new Ray();

			// Copy original ray 
			original.d = ray.d;
			original.o = ray.o;
			original.t = ray.t;

			// *---------------------------------------*
			// Iterate through all objects and find
			// the closest hit, if hit then assign
			// qualities to the shading container
			// *---------------------------------------*

			foreach(BasicObject obj in worldObjects)
			{
				hit = obj.hit(ray, ref t, ref normal);

				if (hit && t < minDistance)
				{
					minDistance	= t;
					s.hitObject = true;
					s.material = obj.material;
					s.hitPoint = original.o + t * original.d;
					//s.localHitPoint = original.o + t * original.d;
					s.localHitPoint = ray.o + t * ray.d;
					s.normal = normal.Hat();
				}

				// Restore original ray
				ray.d = original.d;
				ray.o = original.o;
				ray.t = original.t;
			}
			return s;
		}

		/// <summary>
		/// Clear temp screen buffer
		/// </summary>
		public void clearScreenArray()
		{
			// Clear Screen array
			for (int y=0; y<vp.vRes; y++) 
			{ 
				for (int x=0; x<vp.hRes; x++) 
				{ 
					screenArray[x,y].r = 0;
					screenArray[x,y].g = 0;
					screenArray[x,y].b = 0;
				} 
			}
		}

		/// <summary>
		/// Read in PLY file
		/// </summary>
		public void ReadInPLYFile(ref ArrayList triangles, string PLYFileName, BasicMaterial material, double kEpsilon)
		{
			StreamReader objReader = new StreamReader(PLYFileName);
			string sLine="";

			while (sLine != null)
			{
				sLine = objReader.ReadLine();

				if (sLine != null)
				{
					sLine = sLine.Replace("triangle { vertex","");
					sLine = sLine.Replace(" normal ","");
					sLine = sLine.Replace(" vertex ","");
					sLine = sLine.Replace("  }","");

					string[] sT = sLine.Split(" ".ToCharArray());

					ACCELERATED_Triangle triangleT = new ACCELERATED_Triangle(new Vector3D(System.Convert.ToDouble(sT[2]), System.Convert.ToDouble(sT[3]), System.Convert.ToDouble(sT[4])), new Vector3D(System.Convert.ToDouble(sT[10]), System.Convert.ToDouble(sT[11]), System.Convert.ToDouble(sT[12])), new Vector3D(System.Convert.ToDouble(sT[18]), System.Convert.ToDouble(sT[19]), System.Convert.ToDouble(sT[20])));
				
					triangleT.n1 = new Vector3D(System.Convert.ToDouble(sT[6]), System.Convert.ToDouble(sT[7]), System.Convert.ToDouble(sT[8]));
					triangleT.n2 = new Vector3D(System.Convert.ToDouble(sT[14]), System.Convert.ToDouble(sT[15]), System.Convert.ToDouble(sT[16]));
					triangleT.n3 = new Vector3D(System.Convert.ToDouble(sT[22]), System.Convert.ToDouble(sT[23]), System.Convert.ToDouble(sT[24]));

					triangleT.kEpsilon = kEpsilon;
					triangleT.material = material;
					
					triangleT.interpolate = true;
					//worldObjects.Add(triangleT);
					triangles.Add(triangleT);
				}
			}

			objReader.Close();
		}

		// *=======================================*
		// *---------------------------------------*
		// *=======================================*

		//				Build Functions

		// *=======================================*
		// *---------------------------------------*
		// *=======================================*


		// *---------------------------------------*
		// Tyranny of Spheres
		// *---------------------------------------*

		/// <summary>
		/// Build Sphere Spiral
		/// </summary>
		public void Build_TOSSpiral() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));

			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Plane
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.5,0.5,0.3);	
			phong1.ka = 0.25;	
			phong1.kd = 0.4;
			phong1.ks=0.2;
			phong1.n=400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,-12);
			plane1.n = new Vector3D(0,0,1);
		
			plane1.kEpsilon = 0.001;
			plane1.material = phong1;
			worldObjects.Add(plane1);

			// Set up spiral
			double angle, radius, x, y, xMid, yMid;

			xMid = 5;
			yMid =5;
			radius = 0.5;

			// *---------------------------------------*
			// Produce spiral
			// *---------------------------------------*

			for( int n = 1; n < 200;n++)
			{
				// Spiral formula
				angle = n * 0.1;
				radius = radius + angle * 0.01;

				x = xMid + Math.Cos(angle) * radius;
				y = yMid - Math.Sin(angle) * radius;

				// Sphere
				Phong phong2 = new Phong();
				phong2.cd = new RGBColour((double)n/400, (double)n/200 , (double)n/800);
				phong2.ka = 0.5;	
				phong2.kd = 0.4;  
				phong2.ks = 0.5;
				phong2.n = 250;

				Sphere sphere1 = new Sphere();
				sphere1.r = 1.5;
				sphere1.kEpsilon = 0.001;
				sphere1.centre = new Vector3D(x,y,-6);
				
				sphere1.material = phong2;
				worldObjects.Add(sphere1);
			}

			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(10,5.0,40);
			camera.l = new Vector3D(5.25,5.0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			camera.renderScene(this);

		}

		/// <summary>
		/// Build Sphere Helix
		/// </summary>
		public void Build_TOSHelix() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));

			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Set up helix
			double b, radius, x, y, z, temp;

			b=2;
			radius = 3;

			temp = 0.5/360;

			// *---------------------------------------*
			// Produce helix
			// *---------------------------------------*

			for( int t = 1; t < 45;t=t+1)
			{
				// Helix formula
				z = radius*Math.Cos(b*t);
				x = radius*Math.Sin(b*t);
				y = temp*t;

				// Sphere
				Phong phong1 = new Phong();
				phong1.cd = new RGBColour((double)0.5, (double)0.5 , (double)0.5);
				phong1.ka = 0.5;	
				phong1.kd = 0.4;  
				phong1.ks = 0.5;
				phong1.n = 250;

				Sphere sphere1 = new Sphere();
				sphere1.r = 0.1;
				sphere1.kEpsilon = 0.001;
				sphere1.centre = new Vector3D(x,y,z);
				
				sphere1.material = phong1;
				worldObjects.Add(sphere1);
			}
   
			// Pinhole camera
			PinHoleCamera camera= new PinHoleCamera();
			
			camera.e = new Vector3D(10,5.0,40);
			camera.l = new Vector3D(0,2,0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Sphere of Spheres
		/// </summary>
		public void Build_TOSSphere() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.5, 0.5, 0.8));
			
			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = false;
			worldLights.Add(light1);

			// Directional light
			Directional light2 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);

			// Set up shpere
			double x, y, z, radius;
		
			radius = 5;

			// *---------------------------------------*
			// Produce sphere
			// *---------------------------------------*

			for (double theta = 0; theta <=360; theta=theta+(11.25/4))
			{								
				for (double phi = 0; phi <=180; phi=phi+(11.25/4))
				{
					// Sphere formula
					x = radius * Math.Cos(VariousMath.AnglesToRads(phi)) * Math.Cos(VariousMath.AnglesToRads(theta));
					y = radius * Math.Cos(VariousMath.AnglesToRads(phi)) * Math.Sin(VariousMath.AnglesToRads(theta));
					z = radius * Math.Sin(VariousMath.AnglesToRads(phi));

					// Sphere
					Phong phong1 = new Phong();
					phong1.cd = new RGBColour(0.2, 0.3, 0.7);
					phong1.ka = 0.5;	
					phong1.kd = 0.4;  
					phong1.ks = 0;
					phong1.n = 250;

					Sphere sphere1 = new Sphere();
					sphere1.r = 4;
					sphere1.kEpsilon = 0.001;
					sphere1.centre = new Vector3D(x,y,z);
					
					sphere1.material = phong1;
					worldObjects.Add(sphere1);
				}
			}

			// Pinhole camera
			PinHoleCamera camera= new PinHoleCamera();

			camera.e = new Vector3D(0,80,5);
			camera.l = new Vector3D(0,0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Ellipsoid of Spheres
		/// </summary>
		public void Build_TOSEllipsoid() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			//Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));

			//Directional light 
			Directional light1 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = false;
			worldLights.Add(light1);

			//Set up ellipsoid
			double x,y,z,radius_x, radius_y, radius_z;
		
			radius_x =5;
			radius_y =3;
			radius_z =3;

			// *---------------------------------------*
			// Produce ellipsoid
			// *---------------------------------------*

			for (double theta = 0; theta <=360; theta=theta+11.25)
			{									
				for (double phi = 0; phi <=180; phi=phi+11.25)
				{
					// Ellipsoid formula
					x = radius_x * Math.Cos(VariousMath.AnglesToRads(phi)) * Math.Cos(VariousMath.AnglesToRads(theta));
					y = radius_y * Math.Cos(VariousMath.AnglesToRads(phi)) * Math.Sin(VariousMath.AnglesToRads(theta));
					z = radius_z * Math.Sin(VariousMath.AnglesToRads(phi));

					// Sphere
					Phong phong1 = new Phong();
					phong1.cd = new RGBColour(0.2, 0.6, 0.8);
					phong1.ka = 0.5;	
					phong1.kd = 0.4;  
					phong1.ks = 0; // no specular
					phong1.n = 250;

					Sphere sphere1 = new Sphere();
					sphere1.r = 2;
					sphere1.kEpsilon = 0.001;
					sphere1.centre = new Vector3D(x,y,z);
					
					sphere1.material = phong1;
					worldObjects.Add(sphere1);
				}
			}

			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(0,0,40);
			camera.l = new Vector3D(0,0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Torus of Spheres
		/// </summary>
		public void Build_TOSTorus() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.2, 0.5, 0.8));

			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Point light
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			worldLights.Add(light2);

			// Directional light 
			Directional light3 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light3.c = new RGBColour(1, 1, 1);
			worldLights.Add(light3);

			// Ground plane
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.75, 0.65, 0.75);	
			phong1.ka = 0.25;	
			phong1.kd = 0.4;
			phong1.ks=0.2;
			phong1.n=400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
		
			plane1.kEpsilon = 0.001;
			plane1.material = phong1;
			worldObjects.Add(plane1);

			// set up torus
			double x,y,z,radius_x, radius_y, radius_z , radius_inner, colourRamp;
			
			radius_x =4;
			radius_y =2;
			radius_z =2;
			radius_inner =3;
			
			colourRamp = 0.1;
		
			// *---------------------------------------*
			// Produce torus
			// *---------------------------------------*

			for (double theta = 0; theta <=360; theta=theta+(11.25))
			{									
				for (double phi = 0; phi <=360; phi=phi+(11.25))
				{
					// Sphere colour
					colourRamp =((theta + 1) / 500); 

					// Torus formula
					x = radius_x * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Cos(VariousMath.AnglesToRads(theta));
					y = radius_y * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Sin(VariousMath.AnglesToRads(theta));
					z = radius_z * Math.Sin(VariousMath.AnglesToRads(phi));

					// Sphere
					Phong phong2 = new Phong();
					phong2.cd = new RGBColour(0.5, colourRamp, colourRamp/2);
					phong2.ka = 0.5;	
					phong2.kd = 0.4;  
					phong2.ks = 0.5;
					phong2.n = 250;

					Sphere sphere1 = new Sphere();
					sphere1.r = 1.5;
					sphere1.kEpsilon = 0.001;
					sphere1.centre = new Vector3D(x,y,z);
					
					sphere1.material = phong2;
					worldObjects.Add(sphere1);
				}
			}

			// Pinhole camera
			PinHoleCamera camera= new PinHoleCamera();

			camera.e = new Vector3D(40,0,40);
			camera.l = new Vector3D(0,-3,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,-1,0);
			camera.calculateUVW();

			camera.renderScene(this);
		}

		// *---------------------------------------*
		// Basic Images
		// *---------------------------------------*

		/// <summary>
		/// Build SingleSphere
		/// </summary>
		public void Build_SingleSphere() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0, 0, 0));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0,0,40);
			camera.l = new Vector3D(0,0,0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = false;
			worldLights.Add(light1);

			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);

			// Red sphere
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(1, 0.8, 0.6);	
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.3;
			phong1.n = 50;

			Sphere sphere1 = new Sphere();
			sphere1.r = 5;
			sphere1.kEpsilon = 0.001;
			sphere1.centre = new Vector3D(0,0,0);
			
			sphere1.material = phong1;
			worldObjects.Add(sphere1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Plane
		/// </summary>
		public void Build_Plane() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.15, 0.3));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(10,5.5,40);
			camera.l = new Vector3D(5.25,5.0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = false;
			worldLights.Add(light1);

			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);

			// Ground plane (grey)
			Phong phong5 = new Phong();
			phong5.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong5.ka = 0.25;	
			phong5.kd = 0.4;
			phong5.ks=0.2;
			phong5.n=400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			
			plane1.kEpsilon = 0.001;
			plane1.material = phong5;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Transparent4SpheresSideView
		/// </summary>
		public void Build_Transparent4SpheresSideView() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel 
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			// Reflective depth
			vp.maxDepth = 5;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.15, 0.3));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			//camera.e = new Vector3D(0,300,-3);		// Top view 50 close, 300 far
			//camera.l = new Vector3D(5.25,0, -3);

			//camera.e = new Vector3D(0,50,-3);
			//camera.l = new Vector3D(5.25,0, -3);

			camera.e = new Vector3D(10,5.5,40);	// Side view
			camera.l = new Vector3D(5.25,5.0,0.0);

			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			worldLights.Add(light2);

			// Left sphere green
			Transparent transparent1 = new Transparent();
			transparent1.cd = new RGBColour(0.0, 0.8, 0.6);	
			transparent1.kt = 0.9;  
			transparent1.ks = 0.5;
			transparent1.kr = 0.1;
			transparent1.n = 2000;
			transparent1.ior = 1.5;

			Sphere sphere1 = new Sphere();
			sphere1.r = 3.0;
			sphere1.kEpsilon = 0.001;
			sphere1.centre = new Vector3D(0,5.0,-0.5);
			
			sphere1.material = transparent1;
			worldObjects.Add(sphere1);

			// Orange sphere 
			Reflective reflective2 = new Reflective();
			reflective2.cd = new RGBColour(1, 0.5, 0);
			reflective2.ka = 0.5;	
			reflective2.kd = 0.4;  
			reflective2.ks = 0.5;
			reflective2.kr = 0.5;
			reflective2.n = 250;

			Sphere sphere2 = new Sphere();
			sphere2.r = 3.0;
			sphere2.kEpsilon = 0.001;
			sphere2.centre = new Vector3D(4,4,-6);
			
			sphere2.material = reflective2;
			worldObjects.Add(sphere2);
				
			// Mauve sphere top
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.88, 0.2, 1.0);		
			reflective3.ka = 0.5;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.75;
			reflective3.kr = 0.75;
			reflective3.n = 250;

			Sphere sphere3 = new Sphere();
			sphere3.r = 2.0;
			sphere3.kEpsilon = 0.001;
			sphere3.centre = new Vector3D(4,8.5,-2);
			
			sphere3.material = reflective3;
			worldObjects.Add(sphere3);

			// Blue sphere at right no specular
			Phong phong4 = new Phong();
			phong4.cd = new RGBColour(0, 0, 1);	
			phong4.ka = 0.9;	
			phong4.kd = 0.5;  
			phong4.ks = 0;
			phong4.n = 1;

			Sphere sphere4 = new Sphere();
			sphere4.r = 3.0;
			sphere4.kEpsilon = 0.001;
			sphere4.centre = new Vector3D(10,6,-2);
			
			sphere4.material = phong4;
			worldObjects.Add(sphere4);

			// Ground plane (grey)
			Reflective reflective5 = new Reflective();
			reflective5.cd = new RGBColour(0.75, 0.75, 0.75);	
			reflective5.ka = 0.25;	
			reflective5.kd = 0.4;
			reflective5.ks = 0.2;
			reflective5.kr = 0.15;
			reflective5.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			
			plane1.kEpsilon = 0.001;
			plane1.material = reflective5;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Reflective4SpheresSideView
		/// </summary>
		public void Build_Reflective4SpheresSideView() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel 
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			// Reflective depth
			vp.maxDepth = 5;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.15, 0.3));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			//camera.e = new Vector3D(0,300,-3);		// Top view 50 close, 300 far
			//camera.l = new Vector3D(5.25,0, -3);

			//camera.e = new Vector3D(0,50,-3);
			//camera.l = new Vector3D(5.25,0, -3);

			camera.e = new Vector3D(10,5.5,40);	// Side view
			camera.l = new Vector3D(5.25,5.0,0.0);

			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			worldLights.Add(light2);

			// Left sphere green
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.0, 0.8, 0.6);	
			reflective1.ka = 0.2;	
			reflective1.kd = 0.5;  
			reflective1.ks = 0.2;
			reflective1.kr = 0.5;
			reflective1.n = 50;

			Sphere sphere1 = new Sphere();
			sphere1.r = 3.0;
			sphere1.kEpsilon = 0.001;
			sphere1.centre = new Vector3D(0,5.0,-0.5);
		
			sphere1.material = reflective1;
			worldObjects.Add(sphere1);

			// Orange sphere 
			Reflective reflective2 = new Reflective();
			reflective2.cd = new RGBColour(1, 0.5, 0);
			reflective2.ka = 0.3;	
			reflective2.kd = 0.4;  
			reflective2.ks = 0.3;
			reflective2.kr = 0.9;
			reflective2.n = 250;

			Sphere sphere2 = new Sphere();
			sphere2.r = 3.0;
			sphere2.kEpsilon = 0.001;
			sphere2.centre = new Vector3D(4,4,-6);
		
			sphere2.material = reflective2;
			worldObjects.Add(sphere2);
				
			// Mauve sphere top
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.88, 0.2, 1.0);		
			reflective3.ka = 0.3;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.75;
			reflective3.kr = 0.75;
			reflective3.n = 250;

			Sphere sphere3 = new Sphere();
			sphere3.r = 2.0;
			sphere3.kEpsilon = 0.001;
			sphere3.centre = new Vector3D(4,8.5,-2);
			
			sphere3.material = reflective3;
			worldObjects.Add(sphere3);

			// Blue sphere at right no specular
			Phong phong4 = new Phong();
			phong4.cd = new RGBColour(0, 0, 1);	
			phong4.ka = 0.9;	
			phong4.kd = 0.5;  
			phong4.ks = 0;
			phong4.n = 1;

			Sphere sphere4 = new Sphere();
			sphere4.r = 3.0;
			sphere4.kEpsilon = 0.001;
			sphere4.centre = new Vector3D(10,6,-2);
			
			sphere4.material = phong4;
			worldObjects.Add(sphere4);

			// Ground plane (grey)
			Reflective reflective5 = new Reflective();
			reflective5.cd = new RGBColour(0.75, 0.75, 0.75);	
			reflective5.ka = 0.25;	
			reflective5.kd = 0.4;
			reflective5.ks = 0.2;
			reflective5.kr = 0.15;
			reflective5.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			
			plane1.kEpsilon = 0.001;
			plane1.material = reflective5;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build 4SpheresSideView
		/// </summary>
		public void Build_4SpheresSideView() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.15, 0.3));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			//camera.e = new Vector3D(0,300,-3);		// Top view 50 close, 300 far
			//camera.l = new Vector3D(5.25,0, -3);

			//camera.e = new Vector3D(0,50,-3);
			//camera.l = new Vector3D(5.25,0, -3);

			camera.e = new Vector3D(10,5.5,40);	// Side view
			camera.l = new Vector3D(5.25,5.0,0.0);

			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);

			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,30,10));
			light2.c = new RGBColour(1, 1, 1);
			worldLights.Add(light2);

			// Left sphere green
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.0, 0.8, 0.6);	
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.2;
			phong1.n = 50;

			Sphere sphere1 = new Sphere();
			sphere1.r = 3.0;
			sphere1.kEpsilon = 0.001;
			sphere1.centre = new Vector3D(0,5.0,-0.5);
		
			sphere1.material = phong1;
			worldObjects.Add(sphere1);

			// Orange sphere 
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(1, 0.5, 0);
			phong2.ka = 0.5;	
			phong2.kd = 0.4;  
			phong2.ks = 0.5;
			phong2.n = 250;

			Sphere sphere2 = new Sphere();
			sphere2.r = 3.0;
			sphere2.kEpsilon = 0.001;
			sphere2.centre = new Vector3D(4,4,-6);
		
			sphere2.material = phong2;
			worldObjects.Add(sphere2);
				
			// Mauve sphere top
			Phong phong3 = new Phong();
			phong3.cd = new RGBColour(0.88, 0.2, 1.0);		
			phong3.ka = 0.5;	
			phong3.kd = 0.4;  
			phong3.ks = 0.75;
			phong3.n = 250;

			Sphere sphere3 = new Sphere();
			sphere3.r = 2.0;
			sphere3.kEpsilon = 0.001;
			sphere3.centre = new Vector3D(4,8.5,-2);
		
			sphere3.material = phong3;
			worldObjects.Add(sphere3);

			// Blue sphere at right no specular
			Phong phong4 = new Phong();
			phong4.cd = new RGBColour(0, 0, 1);	
			phong4.ka = 0.9;	
			phong4.kd = 0.5;  
			phong4.ks = 0;
			phong4.n = 1;

			Sphere sphere4 = new Sphere();
			sphere4.r = 3.0;
			sphere4.kEpsilon = 0.001;
			sphere4.centre = new Vector3D(10,6,-2);
		
			sphere4.material = phong4;
			worldObjects.Add(sphere4);

			// Ground plane (grey)
			Phong phong5 = new Phong();
			phong5.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong5.ka = 0.25;	
			phong5.kd = 0.4;
			phong5.ks=0.2;
			phong5.n=400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			
			plane1.kEpsilon = 0.001;
			plane1.material = phong5;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Transform Boxes
		/// </summary>
		public void Build_TransformBoxes() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.2, 0.4));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(2.75, 2, 10);	
			camera.l = new Vector3D(0.5, 2.0, 0.0);

			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.5, new Vector3D(-10, 20, 10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Front Box
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.0, 0.8, 0.3);	
			phong1.ka = 0.5;	
			phong1.kd = 0.6;  
			phong1.ks = 0;
			phong1.n = 50;

			GENERIC_Cube box1 = new GENERIC_Cube();
			Instance box_transform = new Instance(box1);
			box_transform.material = phong1;
			box_transform.Scale(1.5, 0.5, 1);
			box_transform.rotateX(45);
			box_transform.Translate(0,2,0);
			worldObjects.Add(box_transform);

			// Red Box
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(1, 0, 0);	
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0;
			phong2.n = 50;

			AxisAlignedBox box2 = new AxisAlignedBox(new Vector3D(-1, 0.5, -12), new Vector3D(1.5, 5, -4));
			box2.material = phong2;
			worldObjects.Add(box2);

			// Ground plane (grey)
			Phong phong3 = new Phong();
			phong3.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong3.ka = 0.25;	
			phong3.kd = 0.5;
			phong3.ks = 0;
			phong3.n = 50;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong3;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Boxes
		/// </summary>
		public void Build_Boxes() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.2, 0.4));
	
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(2.75, 2, 10);	
			camera.l = new Vector3D(0.5, 2.0, 0.0);

			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();

			// Point light 
			PointLight light1 = new PointLight(1.5, new Vector3D(-10, 20, 10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Front Box
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.0, 0.8, 0.3);	
			phong1.ka = 0.5;	
			phong1.kd = 0.6;  
			phong1.ks = 0;
			phong1.n = 0;

			AxisAlignedBox box1 = new AxisAlignedBox(new Vector3D(-1, 0.5, -1), new Vector3D(1, 3.5, 1));
			box1.material = phong1;
			worldObjects.Add(box1);

			// Red Box
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(1, 0, 0);	
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0;
			phong2.n = 50;

			AxisAlignedBox box2 = new AxisAlignedBox(new Vector3D(-1, 0.5, -12), new Vector3D(1.5, 5, -4));
			box2.material = phong2;
			worldObjects.Add(box2);

			// Ground plane (grey)
			Phong phong3 = new Phong();
			phong3.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong3.ka = 0.25;	
			phong3.kd = 0.5;
			phong3.ks = 0;
			phong3.n = 0;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong3;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}
		/// <summary>
		/// Build TransTransBox
		/// </summary>
		public void Build_TransTransBox() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
	
			// Reflective depth
			vp.maxDepth = 5;
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.2, 0.4));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(2.75, 2, 10);	
			camera.l = new Vector3D(0.5, 2.0, 0.0);
			
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-10, 20, 10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Front box transformed and transparent
	
			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0,0,0);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 1.5;
			glassPtr.kr = 0.1;
			glassPtr.kt = 0.9; 
	
			Instance boxPtr1 = new Instance(new GENERIC_Cube());
			boxPtr1.material = glassPtr;
			boxPtr1.Scale(1.5, 0.5, 1);
			boxPtr1.rotateX(45);
			boxPtr1.Translate(0, 2, 0);
			worldObjects.Add(boxPtr1);
	
			// Red Box
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(1, 0, 0);	
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0;
			phong1.n = 50;

			AxisAlignedBox box2 = new AxisAlignedBox(new Vector3D(-1, 0.5, -12), new Vector3D(1.5, 5, -4));
			box2.material = phong1;
			worldObjects.Add(box2);

			// Ground plane (grey)
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong2.ka = 0.75;	
			phong2.kd = 0.3;
			phong2.ks = 0;
			phong2.n = 50;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong2;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build TransparentBox
		/// </summary>
		public void Build_TransparentBox() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
	
			// Reflective depth
			vp.maxDepth = 5;
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.2, 0.4));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(2.75, 2, 10);	
			camera.l = new Vector3D(0.5, 2.0, 0.0);
			
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-10, 20, 10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Front box transformed and transparent

			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0,0,0);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 1.5;
			glassPtr.kr = 0.1;
			glassPtr.kt = 0.9; 
				
			AxisAlignedBox boxPtr1 = new AxisAlignedBox(new Vector3D(-1, 0.5, -1), new Vector3D(1, 3.5, 1));
			boxPtr1.material = glassPtr;
			worldObjects.Add(boxPtr1);
	
			// Red Box
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(1, 0, 0);	
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0;
			phong1.n = 50;

			AxisAlignedBox box2 = new AxisAlignedBox(new Vector3D(-1, 0.5, -12), new Vector3D(1.5, 5, -4));
			box2.material = phong1;
			worldObjects.Add(box2);

			// Ground plane (grey)
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong2.ka = 0.75;	
			phong2.kd = 0.3;
			phong2.ks = 0;
			phong2.n = 1;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong2;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build TransparentEllipsoid
		/// </summary>
		public void Build_TransparentEllipsoid() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
	
			// Reflective depth
			vp.maxDepth = 6;
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.25,new RGBColour(0.0, 0.3, 0.25));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(-8, 5.5, 40);	
			camera.l = new Vector3D(0, 4, 0);
			
			camera.d = 1200;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Lights
			PointLight light1 = new PointLight(1.5, new Vector3D(40, 50, 0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);

			PointLight lightPtr2 = new PointLight(1.5, new Vector3D(-10, 20, 10));
			lightPtr2.c = new RGBColour(1, 1, 1);
			lightPtr2.castsShadows = true;
			worldLights.Add(lightPtr2);
	
			Directional lightPtr3 = new Directional(1.5, new Vector3D(-1, 0, 0));
			lightPtr3.c = new RGBColour(1, 1, 1);
			lightPtr3.castsShadows = true;
			worldLights.Add(lightPtr3);

			// Transparent ellipsoid
			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0,0,0);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 0.75;
			glassPtr.kr = 0.1;
			glassPtr.kt = 0.9; 
	
			Sphere spherePtrtst = new Sphere();
			spherePtrtst.centre = new Vector3D(0, 0, 0);
			spherePtrtst.r = 1;

			Instance ellipsoidPtr = new Instance(spherePtrtst);
			ellipsoidPtr.material = glassPtr;
			ellipsoidPtr.Scale(6, 2, 3);
			ellipsoidPtr.rotateX(90);
			ellipsoidPtr.Translate(0, 3, 0);
			worldObjects.Add(ellipsoidPtr);
	
			// Red sphere
			Reflective	phongPtr2 = new Reflective();
			phongPtr2.cd = new RGBColour(1, 0, 0);	
			phongPtr2.ka = 0.3;	
			phongPtr2.kd = 0.3;  
			phongPtr2.ks = 0.5;
			phongPtr2.n = 2000;
			phongPtr2.kr = 0.25;

			Sphere spherePtr2 = new Sphere();
			spherePtr2.centre = new Vector3D(4, 4, -6);
			spherePtr2.r = 3;
			spherePtr2.material = phongPtr2;
			worldObjects.Add(spherePtr2);

			// Ground plane (grey)
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong2.ka = 1;	
			phong2.kd = 0.3;
			phong2.ks = 0.2;
			phong2.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong2;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build TransparentSphere
		/// </summary>
		public void Build_TransparentSphere() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
	
			// Reflective depth
			vp.maxDepth = 5;
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.25,new RGBColour(0.0, 0.3, 0.25));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(-8, 5.5, 40);	
			camera.l = new Vector3D(1, 4, 0);
			
			camera.d = 1800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Lights
			PointLight light1 = new PointLight(1.5, new Vector3D(40, 50, 0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);

			PointLight lightPtr2 = new PointLight(1.5, new Vector3D(-10, 20, 10));
			lightPtr2.c = new RGBColour(1, 1, 1);
			lightPtr2.castsShadows = true;
			worldLights.Add(lightPtr2);
	
			Directional lightPtr3 = new Directional(1.5, new Vector3D(-1, 0, 0));
			lightPtr3.c = new RGBColour(1, 1, 1);
			lightPtr3.castsShadows = true;
			worldLights.Add(lightPtr3);

			// Transparent sphere
	
			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0,0,0);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 0.75;
			glassPtr.kr = 0.1;
			glassPtr.kt = 0.9; 
	
			Sphere spherePtr1 = new Sphere();

			spherePtr1.centre = new Vector3D(0, 4.5, 0);
			spherePtr1.r = 3;
			spherePtr1.material = glassPtr;
			worldObjects.Add(spherePtr1);
	
			// Red sphere
		
			Reflective	phongPtr2 = new Reflective();
			phongPtr2.cd = new RGBColour(1, 0, 0);	
			phongPtr2.ka = 0.3;	
			phongPtr2.kd = 0.3;  
			phongPtr2.ks = 0.5;
			phongPtr2.n = 2000;
			phongPtr2.kr = 0.25;

			Sphere spherePtr2 = new Sphere();
			spherePtr2.centre = new Vector3D(4, 4, -6);
			spherePtr2.r = 3;
			spherePtr2.material = phongPtr2;
			worldObjects.Add(spherePtr2);

			// Ground plane (grey)
			Phong phong2 = new Phong();
			phong2.cd = new RGBColour(0.75, 0.75, 0.75);	
			phong2.ka = 0.25;	
			phong2.kd = 0.4;
			phong2.ks = 0.2;
			phong2.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong2;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build SpheresInTorus
		/// </summary>
		public void Build_SpheresInTorus() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 7;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.0, 0.15, 0.3));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 9, 0.1);
			camera.l = new Vector3D(0.0, 0.0, 0.0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-10, 20, 10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Glass Torus
			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0.2,0.3,0.5);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 1.1;
			glassPtr.kr = 0.3;
			glassPtr.kt = 0.7; 
			
			GENERIC_Torus obj1 = new GENERIC_Torus(2,0.5);
			
			Instance inst1 = new Instance(obj1);
			inst1.material = glassPtr;
			inst1.Translate(0,2,0);
			worldObjects.Add(inst1);
			
			// Middle sphere
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(1, 0.1, 0.3);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.4;
			reflective1.kr = 0.9;
			reflective1.n = 350;
			
			Sphere sphereMain = new Sphere();
			sphereMain.r = 1.3;
			sphereMain.kEpsilon = 0.001;
			sphereMain.material = reflective1;
			sphereMain.centre = new Vector3D(0,2,0);
			
			worldObjects.Add(sphereMain);
			
			double x,z,r;		
			r = 2;
			
			// *---------------------------------------*
			// Produce ring of spheres
			// *---------------------------------------*
			
			for (double phi = 0; phi <=360; phi=phi+(22.5/2))
			{
				x = r * Math.Cos(VariousMath.AnglesToRads(phi));
				z = r * Math.Sin(VariousMath.AnglesToRads(phi));

				Reflective reflective2 = new Reflective();
				reflective2.cd = new RGBColour(1, 0.5, 0);
				reflective2.ka = 0.3;	
				reflective2.kd = 0.4;  
				reflective2.ks = 0.3;
				reflective2.kr = 0.9;
				reflective2.n = 250;
				
				Sphere sphere1 = new Sphere();
				sphere1.r = 0.3;
				sphere1.kEpsilon = 0.001;
				sphere1.centre = new Vector3D(x,2,z);
				sphere1.material = reflective2;

				worldObjects.Add(sphere1);
			}
			
			// Plane
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.8, 0.7, 0.6);
			reflective3.ka = 0.3;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.3;
			reflective3.kr = 0.9;
			reflective3.n = 250;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflective3;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build ShinySpiral
		/// </summary>
		public void Build_ShinySpiral() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 5;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));
			
			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);
			light1.castsShadows = true;
			
			// Plane
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.5,0.5,0.3);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.2;
			reflective1.kr = 0.7;
			reflective1.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,-12);
			plane1.n = new Vector3D(0,0,1);
					
			plane1.kEpsilon = 0.001;
			plane1.material = reflective1;
			worldObjects.Add(plane1);
			
			// Set up spiral
			double angle, radius, x, y, xMid, yMid;
			
			xMid = 5;
			yMid =5;
			radius = 0.5;

			bool isReflective = false;
			
			// *---------------------------------------*
			// Produce spiral
			// *---------------------------------------*
			
			for( int n = 1; n < 200;n++)
			{
				// Spiral formula
				angle = n * 0.1;
				radius = radius + angle * 0.01;
				
				x = xMid + Math.Cos(angle) * radius;
				y = yMid - Math.Sin(angle) * radius;
				
				// Sphere
				Sphere sphere1 = new Sphere();
				sphere1.r = 1.5;
				sphere1.kEpsilon = 0.001;
				sphere1.centre = new Vector3D(x,y,-6);
				
				if (isReflective)
				{
					Transparent glassPtr = new Transparent();
					glassPtr.cd = new RGBColour((double)n/400,(double)n/200,(double)n/800);
					glassPtr.ka = 0;
					glassPtr.kd = 0;
					glassPtr.ks = 0.5;
					glassPtr.n = 2000;
					glassPtr.ior = 1.75;
					glassPtr.kr = 0.1;
					glassPtr.kt = 0.9; 
					
					sphere1.material = glassPtr;
					
					isReflective = false;
				}
				else
				{
					Reflective reflective = new Reflective();
					reflective.cd = new RGBColour((double)n/400,(double)n/200,(double)n/800);
					reflective.ka = 0.3;	
					reflective.kd = 0.4;  
					reflective.ks = 0.2;
					reflective.kr = 0.7;
					reflective.n = 350;
					
					sphere1.material = reflective;
					
					isReflective = true;
				}
				worldObjects.Add(sphere1);
			}
			
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(10,5.0,40);
			camera.l = new Vector3D(5.25,5.0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build TorusSquared
		/// </summary>
		public void Build_TorusSquared() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Reflective depth
			vp.maxDepth = 4;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));

			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Plane
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.3,0.3,0.7);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.2;
			reflective1.kr = 0.7;
			reflective1.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,-12);
			plane1.n = new Vector3D(0,0,1);
		
			plane1.kEpsilon = 0.001;
			plane1.material = reflective1;
			worldObjects.Add(plane1);

			// set up torus
			double x,y,z,radius_x, radius_y, radius_z , radius_inner, colourRamp;
			
			radius_x =4;
			radius_y =2;
			radius_z =2;
			radius_inner =3;
			
			colourRamp = 0.1;
		
			// *---------------------------------------*
			// Produce torus of torii?
			// *---------------------------------------*

			for (double theta = 0; theta <=360; theta=theta+(11.25))
			{									
				for (double phi = 0; phi <=360; phi=phi+(11.25))
				{
					// Torus colour
					colourRamp =((theta + 1) / 500); 

					// Torus formula
					x = radius_x * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Cos(VariousMath.AnglesToRads(theta));
					y = radius_y * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Sin(VariousMath.AnglesToRads(theta));
					z = radius_z * Math.Sin(VariousMath.AnglesToRads(phi));

					Reflective reflective2 = new Reflective();
					reflective2.cd = new RGBColour(0.5, colourRamp, colourRamp/2);
					reflective2.ka = 0.5;	
					reflective2.kd = 0.4;  
					reflective2.ks = 0.2;
					reflective2.kr = 0.8;
					reflective2.n = 400;

					GENERIC_Torus obj1 = new GENERIC_Torus(2,0.5);
					
					Instance inst1 = new Instance(obj1);
					inst1.material = reflective2;
					inst1.Translate(x,y,z);
					worldObjects.Add(inst1);
				}
			}

			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(40,0,40);
			camera.l = new Vector3D(0,-3,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,-1,0);
			camera.calculateUVW();

			camera.renderScene(this);
		}

		/// <summary>
		/// Build SpheresInBoxes
		/// </summary>
		public void Build_SpheresInBoxes() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 16;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.3, 0.3, 0.4));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 3, 5);
			camera.l = new Vector3D(0.0, 1, 1);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(1, 0, 0));
			light1.c = new RGBColour(1, 0.5, 0.5);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);
			
			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(-1, 0, 0));
			light2.c = new RGBColour(0.5, 0.5, 1);
			light2.p = 1.5;
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(1.0, new Vector3D(0, 10, 10));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 2.5;
			light3.castsShadows = true;
			worldLights.Add(light3);
			
			// Right Cube
			Transparent glassPtr1 = new Transparent();
			glassPtr1.cd = new RGBColour(0.2,0.3,0.5);
			glassPtr1.ka = 0;
			glassPtr1.kd = 0;
			glassPtr1.ks = 0.5;
			glassPtr1.n = 2000;
			glassPtr1.ior = 1.75;
			glassPtr1.kr = 0.1;
			glassPtr1.kt = 0.9; 
			
			GENERIC_Cube box1 = new GENERIC_Cube();
			Instance box_transform = new Instance(box1);
			box_transform.material = glassPtr1;
			box_transform.rotateY(45);
			box_transform.Translate(1.4,1,0);
			worldObjects.Add(box_transform);
			
			// Left Cube
			Transparent glassPtr2 = new Transparent();
			glassPtr2.cd = new RGBColour(0.2,0.3,0.5);
			glassPtr2.ka = 0;
			glassPtr2.kd = 0;
			glassPtr2.ks = 0.5;
			glassPtr2.n = 2000;
			glassPtr2.ior = 1.75;
			glassPtr2.kr = 0.1;
			glassPtr2.kt = 0.9; 
			
			GENERIC_Cube box2 = new GENERIC_Cube();
			Instance box_transform2 = new Instance(box1);
			box_transform2.material = glassPtr2;
			box_transform2.rotateY(-45);
			box_transform2.Translate(-1.4,1,0);
			worldObjects.Add(box_transform2);
			
			// Right Sphere
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.8, 0.5, 0.3);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.1;
			reflective1.kr = 0.9;
			reflective1.n = 350;
			
			Sphere sphereMain2 = new Sphere();
			sphereMain2.r = 1.3;
			sphereMain2.kEpsilon = 0.001;
			sphereMain2.material = reflective1;
			sphereMain2.centre = new Vector3D(1.4,1,0);
			
			worldObjects.Add(sphereMain2);
			
			// Left Sphere 
			Reflective reflective2 = new Reflective();
			reflective2.cd = new RGBColour(0.2, 0.5, 0.9);
			reflective2.ka = 0.3;	
			reflective2.kd = 0.4;  
			reflective2.ks = 0.1;
			reflective2.kr = 0.7;
			reflective2.n = 350;
			
			Sphere sphereMain = new Sphere();
			sphereMain.r = 1.3;
			sphereMain.kEpsilon = 0.001;
			sphereMain.material = reflective2;
			sphereMain.centre = new Vector3D(-1.4,1,0);
			
			worldObjects.Add(sphereMain);
			
			// Plane 
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.1, 0.25, 0.15);
			reflective3.ka = 0.3;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.3;
			reflective3.kr = 0.7;
			reflective3.n = 250;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflective3;
			worldObjects.Add(plane1);
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build Slices
		/// </summary>
		public void Build_Slices() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 8;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.3, 0.4, 0.5));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, -20, 20);	
			camera.l = new Vector3D(0.0, 0.1, 0);
			camera.d = 1000;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(0, 0, 6));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 2.5;
			light1.castsShadows = true;
			worldLights.Add(light1);
						
			//Directional light 
			Directional light2 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);
			
			// Glass slices
			for (double y=-7; y<=7;y+=2)
			{
				Transparent glassPtr = new Transparent();
				glassPtr.cd = new RGBColour(((y+6)/13),0.3,0.5);
				glassPtr.ka = 0;
				glassPtr.kd = 0;
				glassPtr.ks = 0.2;
				glassPtr.n = 2000;
				glassPtr.ior = 1.4;
				glassPtr.kr = 0.2;
				glassPtr.kt = 0.8; 
					
				GENERIC_Cube box1 = new GENERIC_Cube();
				Instance box_transform = new Instance(box1);
				box_transform.material = glassPtr;
				box_transform.Scale(8, 0.2, 4);
				box_transform.Translate(0,y,0);
				worldObjects.Add(box_transform);
			}
				
			// Blue Cylinder
			Reflective reflectiveB = new Reflective();
			reflectiveB.cd = new RGBColour(0.1, 0.1, 0.8);
			reflectiveB.ka = 0.3;	
			reflectiveB.kd = 0.4;  
			reflectiveB.ks = 0.3;
			reflectiveB.kr = 0.7;
			reflectiveB.n = 250;
				
			OpenCylinder discB  = new OpenCylinder();
			discB.centre = new Vector3D(-5,0,-1);
			discB.r = 4;
			discB.height = 16;
			discB.material = reflectiveB;
			worldObjects.Add(discB);
			
			// Red Cylinder
			Reflective reflectiveR = new Reflective();
			reflectiveR.cd = new RGBColour(0.8, 0.1, 0.1);
			reflectiveR.ka = 0.3;	
			reflectiveR.kd = 0.4;  
			reflectiveR.ks = 0.3;
			reflectiveR.kr = 0.7;
			reflectiveR.n = 250;
				
			OpenCylinder discR  = new OpenCylinder();
			discR.centre = new Vector3D(0,0,-1);
			discR.r = 4;
			discR.height = 16;
			discR.material = reflectiveR;
			worldObjects.Add(discR);
			
			// Green Cylinder
			Reflective reflectiveG = new Reflective();
			reflectiveG.cd = new RGBColour(0.1, 0.8, 0.1);
			reflectiveG.ka = 0.3;	
			reflectiveG.kd = 0.4;  
			reflectiveG.ks = 0.3;
			reflectiveG.kr = 0.7;
			reflectiveG.n = 250;
				
			OpenCylinder discG  = new OpenCylinder();
			discG.centre = new Vector3D(5,0,-1);
			discG.r = 4;
			discG.height = 16;
			discG.material = reflectiveG;
			worldObjects.Add(discG);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.5, 0.5, 0.5);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,0,1);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build Hanoi
		/// </summary>
		public void Build_Hanoi() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 12;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.3, 0.4, 0.5));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 10, 20);	
			camera.l = new Vector3D(0.0, 0.1, 0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			//Directional light 
			Directional light2 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(1.0, new Vector3D(0, 20, 0));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 1.5;
			light3.castsShadows = true;
			worldLights.Add(light3);

			// Cylinder
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.1, 0.1, 0.6);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.3;
			reflective1.kr = 0.7;
			reflective1.n = 250;
				
			OpenCylinder cyl  = new OpenCylinder();
			cyl.centre = new Vector3D(0,0,0);
			cyl.r = 1;
			cyl.height = 30;
			cyl.material = reflective1;
			worldObjects.Add(cyl);
		
			// Torus 1
			Reflective reflective2 = new Reflective();
			reflective2.cd = new RGBColour(0.6, 0.2, 0.2);
			reflective2.ka = 0.5;	
			reflective2.kd = 0.4;  
			reflective2.ks = 0.2;
			reflective2.kr = 0.8;
			reflective2.n = 400;

			GENERIC_Torus obj1 = new GENERIC_Torus(2,0.5);
					
			Instance inst1 = new Instance(obj1);
			inst1.material = reflective2;
			inst1.Scale(4,1.5,4);
			worldObjects.Add(inst1);

			// Torus 2
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.2, 0.6, 0.2);
			reflective3.ka = 0.5;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.2;
			reflective3.kr = 0.8;
			reflective3.n = 400;

			GENERIC_Torus obj2 = new GENERIC_Torus(2,0.5);
					
			Instance inst2 = new Instance(obj2);
			inst2.material = reflective3;
			inst2.Translate(0,1,0);
			inst2.Scale(3,1.5,3);
			worldObjects.Add(inst2);

			// Torus 3
			Reflective reflective4 = new Reflective();
			reflective4.cd = new RGBColour(0.2, 0.2, 0.6);
			reflective4.ka = 0.5;	
			reflective4.kd = 0.4;  
			reflective4.ks = 0.2;
			reflective4.kr = 0.8;
			reflective4.n = 400;

			GENERIC_Torus obj3 = new GENERIC_Torus(2,0.5);
					
			Instance inst3 = new Instance(obj3);
			inst3.material = reflective4;
			inst3.Translate(0,2,0);
			inst3.Scale(2,1.5,2);
			worldObjects.Add(inst3);

			// Torus 4
			Reflective reflective5 = new Reflective();
			reflective5.cd = new RGBColour(0.6, 0.2, 0.6);
			reflective5.ka = 0.5;	
			reflective5.kd = 0.4;  
			reflective5.ks = 0.2;
			reflective5.kr = 0.8;
			reflective5.n = 400;

			GENERIC_Torus obj4 = new GENERIC_Torus(2,0.5);
					
			Instance inst4 = new Instance(obj4);
			inst4.material = reflective5;
			inst4.Translate(0,3,0);
			inst4.Scale(1.5,1.5,1.5);
			worldObjects.Add(inst4);

			// Torus 5
			Reflective reflective6 = new Reflective();
			reflective6.cd = new RGBColour(0.6, 0.6, 0.2);
			reflective6.ka = 0.5;	
			reflective6.kd = 0.4;  
			reflective6.ks = 0.2;
			reflective6.kr = 0.8;
			reflective6.n = 400;

			GENERIC_Torus obj5 = new GENERIC_Torus(2,0.5);
					
			Instance inst5 = new Instance(obj5);
			inst5.material = reflective6;
			inst5.Translate(0,4,0);
			inst5.Scale(1,1.5,1);
			worldObjects.Add(inst5);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.5, 0.5, 0.5);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build BitsAndBobs
		/// </summary>
		public void Build_BitsAndBobs() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 1;
			
			// Reflective depth
			vp.maxDepth = 6;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.2, 0.1, 0.2));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 10, -10);	
			camera.l = new Vector3D(0, 0.1, 0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			//Directional light 
			Directional light2 = new Directional(2.0, new Vector3D(-1.0, 1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(1.0, new Vector3D(0, 20, 0));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 1.5;
			light3.castsShadows = true;
			worldLights.Add(light3);

			// Cylinder
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.1, 0.1, 0.6);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.3;
			reflective1.kr = 0.7;
			reflective1.n = 250;
				
			OpenCylinder cyl  = new OpenCylinder();
			cyl.centre = new Vector3D(5,2,0);
			cyl.r = 1;
			cyl.height = 5;
			cyl.material = reflective1;
			worldObjects.Add(cyl);

			// Torus 
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.2, 0.6, 0.2);
			reflective3.ka = 0.5;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.2;
			reflective3.kr = 0.8;
			reflective3.n = 400;

			GENERIC_Torus obj2 = new GENERIC_Torus(2,0.5);
					
			Instance inst2 = new Instance(obj2);
			inst2.material = reflective3;
			inst2.Translate(5,2,0);
			inst2.Scale(2,2,2);
			worldObjects.Add(inst2);

			// Front Box
			Phong phong1 = new Phong();
			phong1.cd = new RGBColour(0.0, 0.8, 0.3);	
			phong1.ka = 0.5;	
			phong1.kd = 0.6;  
			phong1.ks = 0;
			phong1.n = 50;

			GENERIC_Cube box1 = new GENERIC_Cube();
			Instance box_transform = new Instance(box1);
			box_transform.material = phong1;
			box_transform.rotateX(45);
			box_transform.Translate(-1,4,0);
			box_transform.Shear(1,2.5,1,1,1.3,1);
			worldObjects.Add(box_transform);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.3, 0.3, 0.3);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Planets
		/// </summary>
		public void Build_Planets()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
						
			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 1400;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 2;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
						
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
						
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.1, 0.1, 0.3));
							
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
									
			camera.e = new Vector3D(0,5.5,160);
			camera.l = new Vector3D(0,2,0);
			camera.d = 6800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
						
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);
						
			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,-30,10));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);

			// sphere1

			//TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\amaltheamap.bmp", new Spherical()));
			//TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\arielmap.bmp", new Spherical()));
			//TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\callisto.bmp", new Spherical()));
			TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\cytherea.bmp", new Spherical()));
			
			ti1.Scale(3,3,3);
			ti1.Translate(-10.5,5,0);

			TexturedPhong phong1 = new TexturedPhong(ti1);
		
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.3;
			phong1.n = 50;
						
			GENERIC_Sphere sphere1 = new GENERIC_Sphere();
						
			Instance sphere_transform = new Instance(sphere1);
			sphere_transform.material = phong1;
	
			sphere_transform.Scale(3,3,3);
			sphere_transform.Translate(-10.5,5,0);

			worldObjects.Add(sphere_transform);
			
			// sphere2						

			//TextureInstance ti2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\deimoscyl4.bmp", new Spherical()));
			//TextureInstance ti2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\rheamap.bmp", new Spherical()));
			//TextureInstance ti2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\europamap.bmp", new Spherical()));
			TextureInstance ti2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\ganymede.bmp", new Spherical()));
			
			ti2.Scale(3,3,3);
			ti2.Translate(-3.5,5,0);		

			TexturedPhong phong2 = new TexturedPhong(ti2);
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0.3;
			phong2.n = 50;
			
			GENERIC_Sphere sphere2 = new GENERIC_Sphere();
						
			Instance sphere_transform2 = new Instance(sphere2);
			sphere_transform2.material = phong2;

			sphere_transform2.Scale(3,3,3);
			sphere_transform2.Translate(-3.5,5,0);		

			worldObjects.Add(sphere_transform2);

			// sphere3						

			//TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\iapetuscyl2.bmp", new Spherical()));
			//TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\iocyl2.bmp", new Spherical()));
			//TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\jupiter.bmp", new Spherical()));
			TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\phoboscyl1.bmp", new Spherical()));
		
			ti3.Scale(3,3,3);
			ti3.Translate(3.5,5,0);		
		
			TexturedPhong phong3 = new TexturedPhong(ti3);
			phong3.ka = 0.5;	
			phong3.kd = 0.5;  
			phong3.ks = 0.3;
			phong3.n = 50;
			
			GENERIC_Sphere sphere3 = new GENERIC_Sphere();
						
			Instance sphere_transform3 = new Instance(sphere3);
			sphere_transform3.material = phong3;

			sphere_transform3.Scale(3,3,3);
			sphere_transform3.Translate(3.5,5,0);

			worldObjects.Add(sphere_transform3);

			// sphere4						

			//TextureInstance ti4 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\mercurymap.bmp", new Spherical()));
			//TextureInstance ti4 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\sunmap.bmp", new Spherical()));
			//TextureInstance ti4 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\neptunemap.bmp", new Spherical()));
			TextureInstance ti4 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\moonmap4k.bmp", new Spherical()));
			
			ti4.Scale(3,3,3);
			ti4.Translate(10.5,5,0);	

			TexturedPhong phong4 = new TexturedPhong(ti4);
			phong4.ka = 0.5;	
			phong4.kd = 0.5;  
			phong4.ks = 0.3;
			phong4.n = 50;
			
			GENERIC_Sphere sphere4 = new GENERIC_Sphere();
						
			Instance sphere_transform4 = new Instance(sphere4);
			sphere_transform4.material = phong4;

			sphere_transform4.Scale(3,3,3);
			sphere_transform4.Translate(10.5,5,0);

			worldObjects.Add(sphere_transform4);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.5, 0.5, 0.5);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build SpaceScape
		/// </summary>
		public void Build_SpaceScape()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 1000;
			vp.vRes = 1000;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0, 0, 0));
				
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
						
			camera.e = new Vector3D(0,0.1,0);
			camera.l = new Vector3D(0,-1,0);
			camera.d = 1200;
			camera.ra = 0;
			camera.up = new Vector3D(0,0,1);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = false;
			//worldLights.Add(light1);
			
			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,-30,10));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);
			
			// Planet\moon 
			
//			TextureInstance tia = new TextureInstance(new ImageTexture(@"", new Spherical()));
//			tia.Translate(0,0,0);
//
//			TexturedPhong phong1 = new TexturedPhong(tia);
//			
//			phong1.ka = 0.5;	
//			phong1.kd = 0.5;  
//			phong1.ks = 0.0;
//			phong1.n = 50;
//			
//			GENERIC_Sphere sphere1 = new GENERIC_Sphere();
//			
//			Instance sphere_transform = new Instance(sphere1);
//			sphere_transform.material = phong1;
//			sphere_transform.Translate(0,0,0);
//			worldObjects.Add(sphere_transform);
			
			// Star Field
			TextureInstance ti = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\star_tychocyl1.bmp", new Spherical()));
			
			//ti.Scale(2,2,2);

			TexturedPhong phong2 = new TexturedPhong(ti);
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0.3;
			phong2.n = 50;

			GENERIC_Sphere sphere2 = new GENERIC_Sphere();
			
			Instance sphere_transform2 = new Instance(sphere2);
			sphere_transform2.material = phong2;
			//sphere_transform2.Scale(2,2,2);
			worldObjects.Add(sphere_transform2);
			
			// Moon\Planet
			TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\iocyl2.bmp", new Spherical()));

			ti3.Translate(1,0,0);
			ti3.Scale(0.02,0.02,0.02);
			
			TexturedPhong phong3 = new TexturedPhong(ti3);
			phong3.ka = 0.5;	
			phong3.kd = 0.5;  
			phong3.ks = 0.3;
			phong3.n = 50;

			GENERIC_Sphere sphere3 = new GENERIC_Sphere();
			
			Instance sphere_transform3 = new Instance(sphere3);
			sphere_transform3.material = phong3;
			sphere_transform3.Translate(1,0,0);
			sphere_transform3.Scale(0.02,0.02,0.02);
			worldObjects.Add(sphere_transform3);

			// Moon\Planet
			TextureInstance ti4 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\europamap.bmp", new Spherical()));

			ti4.Translate(-1,0,1);
			ti4.Scale(0.05,0.05,0.05);
			
			TexturedPhong phong4 = new TexturedPhong(ti4);
			phong4.ka = 0.5;	
			phong4.kd = 0.5;  
			phong4.ks = 0.3;
			phong4.n = 50;

			GENERIC_Sphere sphere4 = new GENERIC_Sphere();
			
			Instance sphere_transform4 = new Instance(sphere4);
			sphere_transform4.material = phong4;
			sphere_transform4.Translate(-1,0,1);
			sphere_transform4.Scale(0.05,0.05,0.05);
			worldObjects.Add(sphere_transform4);

			// Moon\Planet
			TextureInstance ti5 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\callisto.bmp", new Spherical()));
	
			ti5.Translate(0,0,-2);
			ti5.Scale(0.03,0.03,0.03);

			TexturedPhong phong5 = new TexturedPhong(ti5);
			phong5.ka = 0.5;	
			phong5.kd = 0.5;  
			phong5.ks = 0.3;
			phong5.n = 50;

			GENERIC_Sphere sphere5 = new GENERIC_Sphere();
			
			Instance sphere_transform5 = new Instance(sphere5);
			sphere_transform5.material = phong5;
			sphere_transform5.Translate(0,0,-2);
			sphere_transform5.Scale(0.03,0.03,0.03);
			worldObjects.Add(sphere_transform5);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build Marble
		/// </summary>
		public void Build_Marble()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
						
			// 600 * 400 with 16 rows per-pixel
			vp.hRes = 1400;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 2;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
						
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
						
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.1, 0.1, 0.3));
							
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
									
			camera.e = new Vector3D(0,5.5,160);
			camera.l = new Vector3D(0,2,0);
			camera.d = 6800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
						
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(-5,20,0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);
						
			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(10,-30,10));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);

			// sphere1

			TextureInstance ti1 = new TextureInstance(new Marble(@"..\..\gfx\ramps\MarbleRamp.bmp", 5, 6, 5, 0.5));
			
			ti1.Scale(3,3,3);
			ti1.Translate(-10.5,5,0);

			TexturedPhong phong1 = new TexturedPhong(ti1);
		
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.3;
			phong1.n = 50;
						
			GENERIC_Sphere sphere1 = new GENERIC_Sphere();
						
			Instance sphere_transform = new Instance(sphere1);
			sphere_transform.material = phong1;
	
			sphere_transform.Scale(3,3,3);
			sphere_transform.Translate(-10.5,5,0);

			worldObjects.Add(sphere_transform);
			
			// sphere2						

			TextureInstance ti2 = new TextureInstance(new LinearNoise(20.0, 2.0, 6.5, 4.0, 0.5, 1));

			ti2.Scale(3,3,3);
			ti2.Translate(-3.5,5,0);		

			TexturedPhong phong2 = new TexturedPhong(ti2);
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0.3;
			phong2.n = 50;
			
			GENERIC_Sphere sphere2 = new GENERIC_Sphere();
						
			Instance sphere_transform2 = new Instance(sphere2);
			sphere_transform2.material = phong2;

			sphere_transform2.Scale(3,3,3);
			sphere_transform2.Translate(-3.5,5,0);		

			worldObjects.Add(sphere_transform2);

			// sphere3						

			TextureInstance ti3 = new TextureInstance(new fBm(5, 0.3, 0.8, 0.1, 0.2, 6, 4, 0.25, 1));
		
			ti3.Scale(3,3,3);
			ti3.Translate(3.5,5,0);		
		
			TexturedPhong phong3 = new TexturedPhong(ti3);
			phong3.ka = 0.5;	
			phong3.kd = 0.5;  
			phong3.ks = 0.3;
			phong3.n = 50;
			
			GENERIC_Sphere sphere3 = new GENERIC_Sphere();
						
			Instance sphere_transform3 = new Instance(sphere3);
			sphere_transform3.material = phong3;

			sphere_transform3.Scale(3,3,3);
			sphere_transform3.Translate(3.5,5,0);

			worldObjects.Add(sphere_transform3);

			// sphere4						

			TextureInstance ti4 = new TextureInstance(new Marble_Procedural(20.0, 5, 1.2, 0.1));
			
			ti4.Scale(3,3,3);
			ti4.Translate(10.5,5,0);	

			TexturedPhong phong4 = new TexturedPhong(ti4);
			phong4.ka = 0.5;	
			phong4.kd = 0.5;  
			phong4.ks = 0.3;
			phong4.n = 50;
			
			GENERIC_Sphere sphere4 = new GENERIC_Sphere();
						
			Instance sphere_transform4 = new Instance(sphere4);
			sphere_transform4.material = phong4;

			sphere_transform4.Scale(3,3,3);
			sphere_transform4.Translate(10.5,5,0);

			worldObjects.Add(sphere_transform4);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.5, 0.5, 0.5);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,1,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build FractalScape
		/// </summary>
		public void Build_FractalCylinders()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 1;
			
			// Reflective depth
			vp.maxDepth = 20;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.3, 0.4, 0.5));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 20, -10);	
			camera.l = new Vector3D(0.0, 0.1, 0);
			camera.d = 1000;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(0, 0, 6));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 2.5;
			light1.castsShadows = true;
			worldLights.Add(light1);
						
			//Directional light 
			Directional light2 = new Directional(1.0, new Vector3D(-1.0, -1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = false;
			worldLights.Add(light2);
			
			// Glass slices
			for (double y=-7; y<=7;y+=2)
			{
				Transparent glassPtr = new Transparent();
				glassPtr.cd = new RGBColour(((y+6)/13),0.3,0.5);
				glassPtr.ka = 0;
				glassPtr.kd = 0;
				glassPtr.ks = 0.2;
				glassPtr.n = 2000;
				glassPtr.ior = 1.4;
				glassPtr.kr = 0.2;
				glassPtr.kt = 0.8; 
					
				GENERIC_Cube box1 = new GENERIC_Cube();
				Instance box_transform = new Instance(box1);
				box_transform.material = glassPtr;
				box_transform.Scale(8, 0.2, 4);
				box_transform.Translate(0,y,0);
				worldObjects.Add(box_transform);
			}
			
			// Cylinder1
			TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\fractals\JSCool.bmp", new Cylindrical()));
			ti1.Scale(1,8,1);

			TexturedPhong phong1 = new TexturedPhong(ti1);
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.3;
			phong1.n = 50;

			OpenCylinder discG  = new OpenCylinder();
			discG.centre = new Vector3D(0,0,0);
			discG.r = 1;
			discG.height = 1;

			Instance discG_transform = new Instance(discG);
			discG_transform.Scale(1,8,1);
			discG_transform.material = phong1;
			worldObjects.Add(discG_transform);

			// Cylinder2
			TextureInstance ti2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\fractals\Set_Zoom2_150its.bmp", new Cylindrical()));
			ti2.Scale(1,8,1);
			ti2.Translate(-2.5,0,0);

			TexturedPhong phong2 = new TexturedPhong(ti2);
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0.3;
			phong2.n = 50;

			OpenCylinder discB  = new OpenCylinder();
			discB.centre = new Vector3D(0,0,0);
			discB.r = 1;
			discB.height = 1;

			Instance discB_transform = new Instance(discB);
			discB_transform.Scale(1,8,1);
			discB_transform.Translate(-2.5,0,0);
			discB_transform.material = phong2;
			worldObjects.Add(discB_transform);

			// Cylinder3
			TextureInstance ti3 = new TextureInstance(new ImageTexture(@"..\..\Gfx\fractals\JS_Zoom1.bmp", new Cylindrical()));
			ti3.Scale(1,8,1);
			ti3.Translate(2.5,0,0);

			TexturedPhong phong3 = new TexturedPhong(ti3);
			phong3.ka = 0.5;	
			phong3.kd = 0.5;  
			phong3.ks = 0.3;
			phong3.n = 50;

			OpenCylinder discR  = new OpenCylinder();
			discR.centre = new Vector3D(0,0,0);
			discR.r = 1;
			discR.height = 1;

			Instance discR_transform = new Instance(discR);
			discR_transform.Scale(1,8,1);
			discR_transform.Translate(2.5,0,0);
			discR_transform.material = phong3;
			worldObjects.Add(discR_transform);

			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.5, 0.5, 0.5);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,0,1);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			//worldObjects.Add(plane1);
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build TexturedSpiral
		/// </summary>
		public void Build_TexturedSpiral()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 5;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));
			
			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			worldLights.Add(light1);
			light1.castsShadows = true;
			
			// Plane
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.5,0.5,0.3);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.2;
			reflective1.kr = 0.7;
			reflective1.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,-12);
			plane1.n = new Vector3D(0,0,1);
					
			plane1.kEpsilon = 0.001;
			plane1.material = reflective1;
			worldObjects.Add(plane1);
			
			// Set up spiral
			double angle, radius, x, y, xMid, yMid;
			
			xMid = 5;
			yMid =5;
			radius = 0.5;

			bool isReflective = false;
			
			// *---------------------------------------*
			// Produce spiral
			// *---------------------------------------*
			
			for( int n = 1; n < 200;n++)
			{
				// Spiral formula
				angle = n * 0.1;
				radius = radius + angle * 0.01;
				
				x = xMid + Math.Cos(angle) * radius;
				y = yMid - Math.Sin(angle) * radius;
				
				// Sphere
				Sphere sphere1 = new Sphere();
				sphere1.r = 1.5;
				sphere1.kEpsilon = 0.001;
				sphere1.centre = new Vector3D(x,y,-6);
				
				if (isReflective)
				{
					Transparent glassPtr = new Transparent();
					glassPtr.cd = new RGBColour((double)n/400,(double)n/200,(double)n/800);
					glassPtr.ka = 0;
					glassPtr.kd = 0;
					glassPtr.ks = 0.5;
					glassPtr.n = 2000;
					glassPtr.ior = 1.75;
					glassPtr.kr = 0.1;
					glassPtr.kt = 0.9; 
					
					sphere1.material = glassPtr;
					
					isReflective = false;
				}
				else
				{
					// Add texture
					TextureInstance ti = new TextureInstance(new fBm(5, (double)n/400,(double)n/200,(double)n/800, 0.2, 6, 4, 0.25, 1));	
			
					TexturedPhong phong2 = new TexturedPhong(ti);
					phong2.ka = 0.5;	
					phong2.kd = 0.5;  
					phong2.ks = 0.3;
					phong2.n = 50;
					
					sphere1.material = phong2;
					
					isReflective = true;
				}
				worldObjects.Add(sphere1);
			}
			
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(10,5.0,40);
			camera.l = new Vector3D(5.25,5.0,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			camera.renderScene(this);
		}

		/// <summary>
		/// Build Entropy
		/// </summary>
		public void Build_Entropy()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
			
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 5;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(0.5,new RGBColour(0.3, 0.3, 0.4));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 4, 10);
			camera.l = new Vector3D(0.0, 3, 0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			// Point light 
			PointLight light1 = new PointLight(1.0, new Vector3D(1, 0, 0));
			light1.c = new RGBColour(1, 0.5, 0.5);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);
			
			// Point light 
			PointLight light2 = new PointLight(1.0, new Vector3D(-1, 0, 0));
			light2.c = new RGBColour(0.5, 0.5, 1);
			light2.p = 1.5;
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(1.0, new Vector3D(0, 10, 10));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 2.5;
			light3.castsShadows = true;
			worldLights.Add(light3);
			
			// Right sphere
			TextureInstance tiRf = new TextureInstance(new ImageTexture(@"..\..\Gfx\Maps\earth.bmp", new Spherical()));
			TextureInstance tiRf2 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\callisto.bmp", new Spherical()));

			tiRf.Scale(3,3,3);
			tiRf.Translate(3,4,-6);

			tiRf2.Scale(3,3,3);
			tiRf2.Translate(3,4,-6);

			TexturedReflective tphong2 = new TexturedReflective(tiRf, tiRf2);
						
			tphong2.ka = 0.5;	
			tphong2.kd = 0.5;
			tphong2.ks = 0.3;
			tphong2.n = 50;
			
			GENERIC_Sphere sphere2 = new GENERIC_Sphere();
			
			Instance sphere_transform2 = new Instance(sphere2);
			sphere_transform2.material = tphong2;
			sphere_transform2.Scale(3,3,3);
			sphere_transform2.Translate(3,4,-6);
			worldObjects.Add(sphere_transform2);

			// Left sphere
			TextureInstance tiRfa = new TextureInstance(new ImageTexture(@"..\..\Gfx\Maps\earth.bmp", new Spherical()));
			TextureInstance tiRf2a = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\mercurymap.bmp", new Spherical()));

			tiRfa.Scale(3,3,3);
			tiRfa.Translate(-3,4,-6);
			tiRfa.rotateX(0.5);

			tiRf2a.Scale(3,3,3);
			tiRf2a.Translate(-3,4,-6);
			tiRfa.rotateX(0.5);

			TexturedReflective tphong2a = new TexturedReflective(tiRfa, tiRf2a);
			
			tphong2a.ka = 0.5;	
			tphong2a.kd = 0.5;
			tphong2a.ks = 0.3;
			tphong2a.n = 50;
			
			GENERIC_Sphere sphereTR = new GENERIC_Sphere();
			
			Instance sphere_transformTR = new Instance(sphereTR);
			sphere_transformTR.material = tphong2a;
			sphere_transformTR.Scale(3,3,3);
			sphere_transformTR.Translate(-3,4,-6);
			sphere_transformTR.rotateX(0.5);
			worldObjects.Add(sphere_transformTR);

			// Glass Torus
			Transparent glassPtr = new Transparent();
			glassPtr.cd = new RGBColour(0.2,0.3,0.5);
			glassPtr.ka = 0;
			glassPtr.kd = 0;
			glassPtr.ks = 0.5;
			glassPtr.n = 2000;
			glassPtr.ior = 1.1;
			glassPtr.kr = 0.3;
			glassPtr.kt = 0.7; 
			
			GENERIC_Torus obj1 = new GENERIC_Torus(2,0.5);
			
			Instance inst1 = new Instance(obj1);
			inst1.material = glassPtr;
			inst1.Translate(0,3,0);
			worldObjects.Add(inst1);

			// Big Ellipse
			TextureInstance ti1 = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\sunmap.bmp", new Spherical()));

			ti1.Scale(6,3,3);
			ti1.Translate(0,8,0);

			TexturedPhong phong1 = new TexturedPhong(ti1);
		
			phong1.ka = 0.5;	
			phong1.kd = 0.5;  
			phong1.ks = 0.3;
			phong1.n = 50;
						
			GENERIC_Sphere sphere1 = new GENERIC_Sphere();
						
			Instance sphere_transform = new Instance(sphere1);
			sphere_transform.material = phong1;
	
			sphere_transform.Scale(6,3,3);
			sphere_transform.Translate(0,8,0);

			worldObjects.Add(sphere_transform);

			// Right Cube
			TextureInstance ti2 = new TextureInstance(new Marble(@"..\..\gfx\ramps\SandstoneRamp.bmp", 5, 6, 5, 0.5));

			TexturedPhong phong2 = new TexturedPhong(ti2);
		
			phong2.ka = 0.5;	
			phong2.kd = 0.5;  
			phong2.ks = 0.3;
			phong2.n = 50;

			GENERIC_Cube box1 = new GENERIC_Cube();
			Instance box_transform = new Instance(box1);
			box_transform.material = phong2;
			box_transform.rotateY(45);
			box_transform.Translate(1.4,1,0);
			worldObjects.Add(box_transform);
			
			// Left Cube
			TextureInstance ti3 = new TextureInstance(new fBm(3.5, 0.5, 0.1, 0.2, 0.2, 6, 4, 0.25, 1));
		
			ti3.rotateY(-45);
			ti3.Translate(-1.4,1,0);		
		
			TexturedPhong phong3 = new TexturedPhong(ti3);
			phong3.ka = 0.5;	
			phong3.kd = 0.5;  
			phong3.ks = 0.3;
			phong3.n = 50;
			
			GENERIC_Cube box2 = new GENERIC_Cube();
			Instance box_transform2 = new Instance(box1);
			box_transform2.material = phong3;
			box_transform2.rotateY(-45);
			box_transform2.Translate(-1.4,1,0);
			worldObjects.Add(box_transform2);

			// Plane 
			Reflective reflective3 = new Reflective();
			reflective3.cd = new RGBColour(0.1, 0.25, 0.15);
			reflective3.ka = 0.3;	
			reflective3.kd = 0.4;  
			reflective3.ks = 0.3;
			reflective3.kr = 0.7;
			reflective3.n = 250;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflective3;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		
		/// <summary>
		/// Build Torus Of Triangles
		/// </summary>
		public void Build_TorusOfTriangles()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*

			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;

			// Reflective depth
			vp.maxDepth = 4;

			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();

			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*

			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.0, 0, 0));

			// Point light
			PointLight light1 = new PointLight(1.0, new Vector3D(1,5,30));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);

			// Plane
			Reflective reflective1 = new Reflective();
			reflective1.cd = new RGBColour(0.3,0.3,0.7);
			reflective1.ka = 0.3;	
			reflective1.kd = 0.4;  
			reflective1.ks = 0.2;
			reflective1.kr = 0.7;
			reflective1.n = 400;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,-12);
			plane1.n = new Vector3D(0,0,1);
		
			plane1.kEpsilon = 0.001;
			plane1.material = reflective1;
			worldObjects.Add(plane1);

			// set up torus
			double x,y,z,radius_x, radius_y, radius_z , radius_inner, colourRamp;
			
			radius_x =4;
			radius_y =2;
			radius_z =2;
			radius_inner =3;
			
			colourRamp = 0.1;

			// *---------------------------------------*
			// Produce torus of triangles
			// *---------------------------------------*

			Grid gridPtr = new Grid();

			gridPtr.x0 = -14;
			gridPtr.y0 = -10;
			gridPtr.z0 = -10;
			
			gridPtr.x1 = 14;
			gridPtr.y1 = 10;
			gridPtr.z1 = 10;

			ArrayList triangles = new ArrayList();

			for (double theta = 0; theta <=360; theta=theta+(11.25/2))
			{									
				for (double phi = 0; phi <=360; phi=phi+(11.25/2))
				{
					// Torus colour
					colourRamp =((theta + 1) / 500); 

					// Torus formula
					x = radius_x * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Cos(VariousMath.AnglesToRads(theta));
					y = radius_y * (radius_inner + Math.Cos(VariousMath.AnglesToRads(phi))) * Math.Sin(VariousMath.AnglesToRads(theta));
					z = radius_z * Math.Sin(VariousMath.AnglesToRads(phi));

					Reflective reflective2 = new Reflective();
					reflective2.cd = new RGBColour(0.5, colourRamp, colourRamp/2);
					reflective2.ka = 0.5;	
					reflective2.kd = 0.4;  
					reflective2.ks = 0.2;
					reflective2.kr = 0.8;
					reflective2.n = 400;

					ACCELERATED_Triangle triangleT = new ACCELERATED_Triangle(new Vector3D(x,y,z), new Vector3D(x + 0.5, y + 0.5,z), new  Vector3D(x + 1, y +1, z + 1));
					triangleT.kEpsilon = 0.001;
					triangleT.material = reflective2;
					
					triangles.Add(triangleT); // Add the triangle to the grid
				}
			}

			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();

			camera.e = new Vector3D(40,0,40);
			camera.l = new Vector3D(4,-1,0.0);
			camera.d = 1400;
			camera.ra = 0;
			camera.up = new Vector3D(0,-1,0);
			camera.calculateUVW();

			gridPtr.Initialise(triangles, 2.15);
			worldObjects.Add(gridPtr); // Add the grid to the world

			camera.renderScene(this);
		}

		/// <summary>
		/// Build SimpleVenusDemilo
		/// </summary>
		public void Build_SimpleVenusDemilo()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 2;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.2, 0.1, 0.2));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 10, 10);	
			//camera.e = new Vector3D(0, 10, -10);
			camera.l = new Vector3D(0, 2, 0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			//Directional light 
			Directional light2 = new Directional(2.0, new Vector3D(-1.0, 1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(2.0, new Vector3D(0, 10, 10));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 1.5;
			light3.castsShadows = true;
			worldLights.Add(light3);

			// Point light 
			PointLight light1 = new PointLight(2.0, new Vector3D(0, 10, -10));
			light1.c = new RGBColour(1, 1, 1);
			light1.p = 1.5;
			light1.castsShadows = true;
			worldLights.Add(light1);

			Reflective reflectiveT = new Reflective();
			reflectiveT.cd = new RGBColour(1, 0.1, 0.3);
			reflectiveT.ka = 0.3;	
			reflectiveT.kd = 0.4;  
			reflectiveT.ks = 0.4;
			reflectiveT.kr = 0.9;
			reflectiveT.n = 350;

			ACCELERATED_Triangle triangle = new ACCELERATED_Triangle(new Vector3D(0,0,0), new Vector3D(3,3,0), new  Vector3D(6,6,6));
			triangle.kEpsilon = 0.001;
			triangle.material = reflectiveT;

			Grid gridPtr = new Grid();

			ArrayList triangles = new ArrayList();

			ReadInPLYFile(ref triangles, @"..\..\gfx\ply\venusdemilo.txt", reflectiveT, 0.001);
			
			gridPtr.x0 = -7;
			gridPtr.y0 = -7;
			gridPtr.z0 = -7;
			
			gridPtr.x1 = 7;
			gridPtr.y1 = 7;
			gridPtr.z1 = 7;

			gridPtr.Initialise(triangles, 2.15);
			worldObjects.Add(gridPtr); // Add the grid to the world
			// Plane
			
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.3, 0.3, 0.3);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}

		/// <summary>
		/// Build TexturedVenusDemilo
		/// </summary>
		public void Build_TexturedVenusDemilo()
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
		
			// 600 * 600 with 16 rows per-pixel
			vp.hRes = 600;
			vp.vRes = 600;
			vp.PixelSize = 1;
			vp.numSamples = 16;
			
			// Reflective depth
			vp.maxDepth = 2;
			
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
			
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
			
			// Ambient light
			ambient = new AmbientLight(1.0,new RGBColour(0.2, 0.1, 0.2));
					
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
			
			camera.e = new Vector3D(0, 10, -10);	
			camera.l = new Vector3D(0, 2, 0);
			camera.d = 800;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
			
			//Directional light 
			Directional light2 = new Directional(2.0, new Vector3D(-1.0, 1.0, 1.0));
			light2.c = new RGBColour(1, 1, 1);
			light2.castsShadows = true;
			worldLights.Add(light2);
			
			// Point light 
			PointLight light3 = new PointLight(1.0, new Vector3D(0, 20, 0));
			light3.c = new RGBColour(1, 1, 1);
			light3.p = 1.5;
			light3.castsShadows = true;
			worldLights.Add(light3);

			TextureInstance ti = new TextureInstance(new ImageTexture(@"..\..\Gfx\Mercator\ganymede.bmp", new Spherical()));
			//TextureInstance ti = new TextureInstance(new Marble(@"..\..\gfx\ramps\MarbleRamp.bmp", 5, 6, 5, 0.5));
			
			TexturedPhong phong = new TexturedPhong(ti);
			phong.ka = 0.5;	
			phong.kd = 0.5;  
			phong.ks = 0.3;
			phong.n = 50;

			Grid gridPtr = new Grid();

			ArrayList triangles = new ArrayList();

			ReadInPLYFile(ref triangles, @"..\..\gfx\ply\venusdemilo.txt", phong, 0.001);
			
			gridPtr.x0 = -7;
			gridPtr.y0 = -7;
			gridPtr.z0 = -7;
			
			gridPtr.x1 = 7;
			gridPtr.y1 = 7;
			gridPtr.z1 = 7;

			gridPtr.Initialise(triangles, 2.15);
			worldObjects.Add(gridPtr); // Add the grid to the world
			
			// Plane
			Reflective reflectiveP = new Reflective();
			reflectiveP.cd = new RGBColour(0.3, 0.3, 0.3);
			reflectiveP.ka = 0.3;	
			reflectiveP.kd = 0.4;  
			reflectiveP.ks = 0.2;
			reflectiveP.kr = 0.8;
			reflectiveP.n = 400;
			
			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,-9,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.kEpsilon = 0.001;
			plane1.material = reflectiveP;
			worldObjects.Add(plane1);

			camera.renderScene(this);
		}
		
		/// <summary>
		/// Build Scratch Build
		/// </summary>
		public void Build_ScratchBuild() 
		{
			// *---------------------------------------*
			// Set up view plane and screen buffer
			// *---------------------------------------*
						
			// 400 * 400 with 16 rows per-pixel
			vp.hRes = 400;
			vp.vRes = 400;
			vp.PixelSize = 1;
			vp.numSamples = 16;
				
			// Reflective depth
			vp.maxDepth = 5;
			// Clear temp screen buffer
			screenArray = new RGBColour[vp.hRes,vp.vRes];
			this.clearScreenArray();
						
			// *---------------------------------------*
			// Set up scene
			// *---------------------------------------*
						
			// Ambient light
			ambient = new AmbientLight(0.25,new RGBColour(0.0, 0.3, 0.25));
							
			// Pinhole camera
			PinHoleCamera camera = new PinHoleCamera();
						
			camera.e = new Vector3D(-8, 5.5, 40);	
			camera.l = new Vector3D(0, 0, 0);
						
			camera.d = 7000;
			camera.ra = 0;
			camera.up = new Vector3D(0,1,0);
			camera.calculateUVW();
						
			// Lights
			PointLight light1 = new PointLight(1.5, new Vector3D(40, 50, 0));
			light1.c = new RGBColour(1, 1, 1);
			light1.castsShadows = true;
			worldLights.Add(light1);
			
			PointLight lightPtr2 = new PointLight(1.5, new Vector3D(-10, 20, 10));
			lightPtr2.c = new RGBColour(1, 1, 1);
			lightPtr2.castsShadows = true;
			worldLights.Add(lightPtr2);
				
			Directional lightPtr3 = new Directional(1.5, new Vector3D(-1, 0, 0));
			lightPtr3.c = new RGBColour(1, 1, 1);
			lightPtr3.castsShadows = true;
			worldLights.Add(lightPtr3);
			
			TexturedPhong phong = new TexturedPhong(new CheckerBoardTexture(0.2,new RGBColour(0.4,0.8,0.5), new RGBColour(0,0,0)));
			
			phong.ka = 0.5;	
			phong.kd = 0.5;
			phong.ks = 0.3;
			phong.n = 50;

			Plane plane1 = new Plane();
			plane1.a = new Vector3D(0,0,0);
			plane1.n = new Vector3D(0,1,0);
			plane1.material = phong;

			worldObjects.Add(plane1);
			
			camera.renderScene(this);
		}
	}
}