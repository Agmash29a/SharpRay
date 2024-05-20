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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;

namespace RayTracer
{
	/// <summary>
	/// The main form for the application
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		// World and Build
		private string buildFunction;
		public World world;

		// Flow control bools
		public bool myThreadFree = true;
		public bool renderImage = false;
		public bool rendering = false;

		// Threads
		public Thread renderThread;
		
		// Constants
		const string THREAD_RENDER_NAME = "Rendering Thread";
		const string RENDERING = "Entropy SharpRay - Rendering";
		const string RENDERING_FINISHED = "Entropy SharpRay - Finished Rendering";

		const int WORLD_HRES = 600;
		const int WORLD_VRES = 400;
		const int WORLD_PIXELSIZE = 1;
		const int WORLD_NUMSAMPLES = 16;

		// Windows controls
		private RayTracer.Viewer CUSTOM_Viewer;
		private System.Windows.Forms.MainMenu MENU_Main;
		private System.Windows.Forms.Timer timer1;
		private System.ComponentModel.IContainer components;

		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem menuItem4;
		private System.Windows.Forms.MenuItem menuItem5;
		private System.Windows.Forms.MenuItem menuItem6;
		private System.Windows.Forms.MenuItem menuItem7;
		private System.Windows.Forms.MenuItem menuItem8;
		private System.Windows.Forms.MenuItem menuItem9;
		private System.Windows.Forms.MenuItem menuItem10;
		private System.Windows.Forms.MenuItem menuItem12;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.MenuItem menuItem15;
		private System.Windows.Forms.MenuItem menuItem16;
		private System.Windows.Forms.MenuItem menuItem17;
		private System.Windows.Forms.MenuItem menuItem19;
		private System.Windows.Forms.MenuItem menuItem20;
		private System.Windows.Forms.MenuItem menuItem23;
		private System.Windows.Forms.MenuItem menuItem24;
		private System.Windows.Forms.MenuItem menuItem25;
		private System.Windows.Forms.MenuItem menuItem26;
		private System.Windows.Forms.MenuItem menuItem14;
		private System.Windows.Forms.MenuItem menuItem18;
		private System.Windows.Forms.MenuItem menuItem28;
		private System.Windows.Forms.MenuItem menuItem29;
		private System.Windows.Forms.MenuItem menuItem30;
		private System.Windows.Forms.MenuItem menuItem31;
		private System.Windows.Forms.MenuItem menuItem32;
		private System.Windows.Forms.MenuItem menuItem33;
		private System.Windows.Forms.MenuItem menuItem34;
		private System.Windows.Forms.MenuItem menuItem35;
		private System.Windows.Forms.MenuItem menuItem36;
		private System.Windows.Forms.MenuItem menuItem37;
		private System.Windows.Forms.MenuItem menuItem38;
		private System.Windows.Forms.MenuItem menuItem39;
		private System.Windows.Forms.MenuItem menuItem21;
		private System.Windows.Forms.MenuItem menuItem27;
		private System.Windows.Forms.MenuItem menuItem41;
		private System.Windows.Forms.MenuItem menuItem42;
		private System.Windows.Forms.MenuItem menuItem43;
		private System.Windows.Forms.MenuItem menuItem44;
		private System.Windows.Forms.MenuItem menuItem45;
		private System.Windows.Forms.MenuItem menuItem46;
		private System.Windows.Forms.MenuItem menuItem47;
		private System.Windows.Forms.MenuItem menuItem48;
		private System.Windows.Forms.MenuItem menuItem11;


		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.CUSTOM_Viewer = new RayTracer.Viewer();
			this.MENU_Main = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItem4 = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.menuItem6 = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.menuItem20 = new System.Windows.Forms.MenuItem();
			this.menuItem25 = new System.Windows.Forms.MenuItem();
			this.menuItem13 = new System.Windows.Forms.MenuItem();
			this.menuItem7 = new System.Windows.Forms.MenuItem();
			this.menuItem15 = new System.Windows.Forms.MenuItem();
			this.menuItem16 = new System.Windows.Forms.MenuItem();
			this.menuItem17 = new System.Windows.Forms.MenuItem();
			this.menuItem24 = new System.Windows.Forms.MenuItem();
			this.menuItem11 = new System.Windows.Forms.MenuItem();
			this.menuItem12 = new System.Windows.Forms.MenuItem();
			this.menuItem31 = new System.Windows.Forms.MenuItem();
			this.menuItem32 = new System.Windows.Forms.MenuItem();
			this.menuItem33 = new System.Windows.Forms.MenuItem();
			this.menuItem34 = new System.Windows.Forms.MenuItem();
			this.menuItem35 = new System.Windows.Forms.MenuItem();
			this.menuItem36 = new System.Windows.Forms.MenuItem();
			this.menuItem37 = new System.Windows.Forms.MenuItem();
			this.menuItem38 = new System.Windows.Forms.MenuItem();
			this.menuItem26 = new System.Windows.Forms.MenuItem();
			this.menuItem10 = new System.Windows.Forms.MenuItem();
			this.menuItem14 = new System.Windows.Forms.MenuItem();
			this.menuItem19 = new System.Windows.Forms.MenuItem();
			this.menuItem23 = new System.Windows.Forms.MenuItem();
			this.menuItem18 = new System.Windows.Forms.MenuItem();
			this.menuItem28 = new System.Windows.Forms.MenuItem();
			this.menuItem29 = new System.Windows.Forms.MenuItem();
			this.menuItem30 = new System.Windows.Forms.MenuItem();
			this.menuItem39 = new System.Windows.Forms.MenuItem();
			this.menuItem21 = new System.Windows.Forms.MenuItem();
			this.menuItem27 = new System.Windows.Forms.MenuItem();
			this.menuItem41 = new System.Windows.Forms.MenuItem();
			this.menuItem42 = new System.Windows.Forms.MenuItem();
			this.menuItem43 = new System.Windows.Forms.MenuItem();
			this.menuItem44 = new System.Windows.Forms.MenuItem();
			this.menuItem45 = new System.Windows.Forms.MenuItem();
			this.menuItem46 = new System.Windows.Forms.MenuItem();
			this.menuItem47 = new System.Windows.Forms.MenuItem();
			this.menuItem8 = new System.Windows.Forms.MenuItem();
			this.menuItem9 = new System.Windows.Forms.MenuItem();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.menuItem48 = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// CUSTOM_Viewer
			// 
			this.CUSTOM_Viewer.BackColor = System.Drawing.Color.Black;
			this.CUSTOM_Viewer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CUSTOM_Viewer.Image = ((System.Drawing.Image)(resources.GetObject("CUSTOM_Viewer.Image")));
			this.CUSTOM_Viewer.ImageSizeMode = RayTracer.SizeMode.RatioStretch;
			this.CUSTOM_Viewer.Location = new System.Drawing.Point(0, 0);
			this.CUSTOM_Viewer.Name = "CUSTOM_Viewer";
			this.CUSTOM_Viewer.Size = new System.Drawing.Size(408, 272);
			this.CUSTOM_Viewer.TabIndex = 4;
			// 
			// MENU_Main
			// 
			this.MENU_Main.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem1,
																					  this.menuItem2,
																					  this.menuItem3,
																					  this.menuItem8});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem4});
			this.menuItem1.Text = "File";
			// 
			// menuItem4
			// 
			this.menuItem4.Index = 0;
			this.menuItem4.Text = "Save Image";
			this.menuItem4.Click += new System.EventHandler(this.menuItem4_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem5,
																					  this.menuItem6});
			this.menuItem2.Text = "Mode";
			// 
			// menuItem5
			// 
			this.menuItem5.Checked = true;
			this.menuItem5.Index = 0;
			this.menuItem5.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.menuItem5.Text = "Ratio Stretch";
			this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
			// 
			// menuItem6
			// 
			this.menuItem6.Index = 1;
			this.menuItem6.Shortcut = System.Windows.Forms.Shortcut.F2;
			this.menuItem6.Text = "Scrollable";
			this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 2;
			this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem20,
																					  this.menuItem25,
																					  this.menuItem15,
																					  this.menuItem31,
																					  this.menuItem26,
																					  this.menuItem39,
																					  this.menuItem45});
			this.menuItem3.Text = "Render";
			// 
			// menuItem20
			// 
			this.menuItem20.Index = 0;
			this.menuItem20.Text = "Scratch Build";
			this.menuItem20.Click += new System.EventHandler(this.menuItem20_Click);
			// 
			// menuItem25
			// 
			this.menuItem25.Index = 1;
			this.menuItem25.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem13,
																					   this.menuItem7});
			this.menuItem25.Text = "Primitives";
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 0;
			this.menuItem13.Text = "Plane";
			this.menuItem13.Click += new System.EventHandler(this.menuItem13_Click);
			// 
			// menuItem7
			// 
			this.menuItem7.Index = 1;
			this.menuItem7.Text = "Sphere";
			this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
			// 
			// menuItem15
			// 
			this.menuItem15.Index = 2;
			this.menuItem15.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem16,
																					   this.menuItem17,
																					   this.menuItem24,
																					   this.menuItem11,
																					   this.menuItem12});
			this.menuItem15.Text = "Tyranny of Spheres";
			// 
			// menuItem16
			// 
			this.menuItem16.Index = 0;
			this.menuItem16.Text = "Ellipsoid";
			this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click);
			// 
			// menuItem17
			// 
			this.menuItem17.Index = 1;
			this.menuItem17.Text = "Torus";
			this.menuItem17.Click += new System.EventHandler(this.menuItem17_Click);
			// 
			// menuItem24
			// 
			this.menuItem24.Index = 2;
			this.menuItem24.Text = "Sphere";
			this.menuItem24.Click += new System.EventHandler(this.menuItem24_Click);
			// 
			// menuItem11
			// 
			this.menuItem11.Index = 3;
			this.menuItem11.Text = "Helix";
			this.menuItem11.Click += new System.EventHandler(this.menuItem11_Click);
			// 
			// menuItem12
			// 
			this.menuItem12.Index = 4;
			this.menuItem12.Text = "Spiral";
			this.menuItem12.Click += new System.EventHandler(this.menuItem12_Click);
			// 
			// menuItem31
			// 
			this.menuItem31.Index = 3;
			this.menuItem31.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem32,
																					   this.menuItem33,
																					   this.menuItem34,
																					   this.menuItem35,
																					   this.menuItem36,
																					   this.menuItem37,
																					   this.menuItem38});
			this.menuItem31.Text = "Glass and Metal";
			// 
			// menuItem32
			// 
			this.menuItem32.Index = 0;
			this.menuItem32.Text = "Spheres in Boxes";
			this.menuItem32.Click += new System.EventHandler(this.menuItem32_Click);
			// 
			// menuItem33
			// 
			this.menuItem33.Index = 1;
			this.menuItem33.Text = "Spheres in Torus";
			this.menuItem33.Click += new System.EventHandler(this.menuItem33_Click);
			// 
			// menuItem34
			// 
			this.menuItem34.Index = 2;
			this.menuItem34.Text = "Shiny Spiral";
			this.menuItem34.Click += new System.EventHandler(this.menuItem34_Click);
			// 
			// menuItem35
			// 
			this.menuItem35.Index = 3;
			this.menuItem35.Text = "Torus Squared";
			this.menuItem35.Click += new System.EventHandler(this.menuItem35_Click);
			// 
			// menuItem36
			// 
			this.menuItem36.Index = 4;
			this.menuItem36.Text = "Slices";
			this.menuItem36.Click += new System.EventHandler(this.menuItem36_Click);
			// 
			// menuItem37
			// 
			this.menuItem37.Index = 5;
			this.menuItem37.Text = "Hanoi";
			this.menuItem37.Click += new System.EventHandler(this.menuItem37_Click);
			// 
			// menuItem38
			// 
			this.menuItem38.Index = 6;
			this.menuItem38.Text = "Bits and Bobs";
			this.menuItem38.Click += new System.EventHandler(this.menuItem38_Click);
			// 
			// menuItem26
			// 
			this.menuItem26.Index = 4;
			this.menuItem26.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem10,
																					   this.menuItem14,
																					   this.menuItem19,
																					   this.menuItem23,
																					   this.menuItem18,
																					   this.menuItem28,
																					   this.menuItem29,
																					   this.menuItem30});
			this.menuItem26.Text = "Spheres and Boxes";
			// 
			// menuItem10
			// 
			this.menuItem10.Index = 0;
			this.menuItem10.Text = "4 Spheres - Side View";
			this.menuItem10.Click += new System.EventHandler(this.menuItem10_Click);
			// 
			// menuItem14
			// 
			this.menuItem14.Index = 1;
			this.menuItem14.Text = "Boxes";
			this.menuItem14.Click += new System.EventHandler(this.menuItem14_Click);
			// 
			// menuItem19
			// 
			this.menuItem19.Index = 2;
			this.menuItem19.Text = "ReflectiveSpheres";
			this.menuItem19.Click += new System.EventHandler(this.menuItem19_Click);
			// 
			// menuItem23
			// 
			this.menuItem23.Index = 3;
			this.menuItem23.Text = "TransformedBox";
			this.menuItem23.Click += new System.EventHandler(this.menuItem23_Click);
			// 
			// menuItem18
			// 
			this.menuItem18.Index = 4;
			this.menuItem18.Text = "TransTransBox";
			this.menuItem18.Click += new System.EventHandler(this.menuItem18_Click);
			// 
			// menuItem28
			// 
			this.menuItem28.Index = 5;
			this.menuItem28.Text = "TransparentBox";
			this.menuItem28.Click += new System.EventHandler(this.menuItem28_Click);
			// 
			// menuItem29
			// 
			this.menuItem29.Index = 6;
			this.menuItem29.Text = "TransparentEllipsoid";
			this.menuItem29.Click += new System.EventHandler(this.menuItem29_Click);
			// 
			// menuItem30
			// 
			this.menuItem30.Index = 7;
			this.menuItem30.Text = "TransparentSphere";
			this.menuItem30.Click += new System.EventHandler(this.menuItem30_Click);
			// 
			// menuItem39
			// 
			this.menuItem39.Index = 5;
			this.menuItem39.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem21,
																					   this.menuItem27,
																					   this.menuItem41,
																					   this.menuItem42,
																					   this.menuItem43,
																					   this.menuItem44});
			this.menuItem39.Text = "Textures";
			// 
			// menuItem21
			// 
			this.menuItem21.Index = 0;
			this.menuItem21.Text = "Planets and Moons";
			this.menuItem21.Click += new System.EventHandler(this.menuItem21_Click);
			// 
			// menuItem27
			// 
			this.menuItem27.Index = 1;
			this.menuItem27.Text = "SpaceScape";
			this.menuItem27.Click += new System.EventHandler(this.menuItem27_Click);
			// 
			// menuItem41
			// 
			this.menuItem41.Index = 2;
			this.menuItem41.Text = "Marble and Noise";
			this.menuItem41.Click += new System.EventHandler(this.menuItem41_Click);
			// 
			// menuItem42
			// 
			this.menuItem42.Index = 3;
			this.menuItem42.Text = "Fractal Cylinders";
			this.menuItem42.Click += new System.EventHandler(this.menuItem42_Click);
			// 
			// menuItem43
			// 
			this.menuItem43.Index = 4;
			this.menuItem43.Text = "Textured Spiral";
			this.menuItem43.Click += new System.EventHandler(this.menuItem43_Click);
			// 
			// menuItem44
			// 
			this.menuItem44.Index = 5;
			this.menuItem44.Text = "Entropy";
			this.menuItem44.Click += new System.EventHandler(this.menuItem44_Click);
			// 
			// menuItem45
			// 
			this.menuItem45.Index = 6;
			this.menuItem45.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					   this.menuItem46,
																					   this.menuItem47,
																					   this.menuItem48});
			this.menuItem45.Text = "Accelerated Triangles";
			// 
			// menuItem46
			// 
			this.menuItem46.Index = 0;
			this.menuItem46.Text = "Simple Venus Demilo";
			this.menuItem46.Click += new System.EventHandler(this.menuItem46_Click);
			// 
			// menuItem47
			// 
			this.menuItem47.Index = 1;
			this.menuItem47.Text = "Textured Venus Demilo";
			this.menuItem47.Click += new System.EventHandler(this.menuItem47_Click);
			// 
			// menuItem8
			// 
			this.menuItem8.Index = 3;
			this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.menuItem9});
			this.menuItem8.Text = "View";
			// 
			// menuItem9
			// 
			this.menuItem9.Index = 0;
			this.menuItem9.Shortcut = System.Windows.Forms.Shortcut.F5;
			this.menuItem9.Text = "Show Current Render";
			this.menuItem9.Click += new System.EventHandler(this.menuItem9_Click);
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Interval = 1000;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// menuItem48
			// 
			this.menuItem48.Index = 2;
			this.menuItem48.Text = "Torus of Triangles";
			this.menuItem48.Click += new System.EventHandler(this.menuItem48_Click);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(408, 272);
			this.Controls.Add(this.CUSTOM_Viewer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.MENU_Main;
			this.Name = "MainForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Entropy SharpRay";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		// *---------------------------------------*
		// Render Functions
		// *---------------------------------------*

		/// <summary>
		/// StartRender - Spin off render thread
		/// </summary>
		private void StartRender()
		{
			// If The thread is not already rendereing then spin off
			if (myThreadFree)
			{
				myThreadFree = false;

				this.Text = RENDERING;

				renderThread = new Thread(new ThreadStart(this.RenderThread));
				renderThread.Name = THREAD_RENDER_NAME;   // looks nice in Output window
				renderThread.Start();
			}
		}

		/// <summary>
		/// RenderThread - Set up the world and build
		/// </summary>
		public void RenderThread()
		{
			// New world
			world = new World();

			rendering = true;

			// Select build function and build
			switch(buildFunction)
			{
				case "Sphere":
					world.Build_SingleSphere();
					break;
				case "4 Spheres - Side View":
					world.Build_4SpheresSideView();
					break;
				case "TOS - Helix":
					world.Build_TOSHelix();
					break;
				case "TOS - Spiral":
					world.Build_TOSSpiral();
					break;
				case "Plane":
					world.Build_Plane();
					break;
				case "TOS - Torus":
					world.Build_TOSTorus();
					break;
				case "TOS - Ellipsoid":
					world.Build_TOSEllipsoid();
					break;
				case "TOS - Sphere":
					world.Build_TOSSphere();
					break;
				case "Reflective - 4 Spheres - Side View":
					world.Build_Reflective4SpheresSideView();
					break;
				case "Scratch Build":
					world.Build_ScratchBuild();
					break;
				case "Transform Boxes":
					world.Build_TransformBoxes();
					break;
				case "Boxes":
					world.Build_Boxes();
					break;
				case "TransTransBox":
					world.Build_TransTransBox();
					break;
				case "TransparentBox":
					world.Build_TransparentBox();
					break;
				case "TransparentEllipsoid":
					world.Build_TransparentEllipsoid();
					break;
				case "TransparentSphere":
					world.Build_TransparentSphere();
					break;
				case "SpheresInBoxes":
					world.Build_SpheresInBoxes();
					break;
				case "SpheresInTorus":
					world.Build_SpheresInTorus();
					break;
				case "ShinySpiral":
					world.Build_ShinySpiral();
					break;
				case "TorusSquared":
					world.Build_TorusSquared();
					break;
				case "Slices":
					world.Build_Slices();
					break;
				case "Hanoi":
					world.Build_Hanoi();
					break;
				case "BitsAndBobs":
					world.Build_BitsAndBobs();
					break;
				case "Planets":
					world.Build_Planets();
					break;
				case "SpaceScape":
					world.Build_SpaceScape();
					break;
				case "Marble":
					world.Build_Marble();
					break;
				case "FractalCylinders":
					world.Build_FractalCylinders();
					break;
				case "TexturedSpiral":
					world.Build_TexturedSpiral();
					break;
				case "Entropy":
					world.Build_Entropy();
					break;
				case "SimpleVenusDemilo":
					world.Build_SimpleVenusDemilo();
					break;
				case "TexturedVenusDemilo":
					world.Build_TexturedVenusDemilo();
					break;
				case "TorusOfTriangles":
					world.Build_TorusOfTriangles();
					break;
			}

			// Rendering is finished
			//this.Text = RENDERING_FINISHED;
			renderImage=true;
			myThreadFree = true;
			rendering = false;
		}

		// *---------------------------------------*
		// Timer Code
		// *---------------------------------------*
		/// <summary>
		/// timer1_Tick - Check for changes in renderImage flag
		/// and render to screen if necessary
		/// </summary>
		private void timer1_Tick(object sender, System.EventArgs e)
		{

			// If the renderImage flag is set then copy temp buffer to picture box
			if (renderImage) 
			{
				Bitmap bitmap = new Bitmap(world.vp.hRes,world.vp.vRes); 
			
				CUSTOM_Viewer.Image = bitmap; 
				
				for (int x=0; x<bitmap.Width; x++) 
				{ 
					for (int y=0; y<bitmap.Height; y++) 
					{ 
						bitmap.SetPixel(x,(bitmap.Height-1) - y,Color.FromArgb(world.screenArray[x,y].RedInteger, world.screenArray[x,y].GreenInteger, world.screenArray[x,y].BlueInteger)); 
					} 
				}
				this.Refresh();
				renderImage=false;
			}
		}

		// *---------------------------------------*
		// Menu Buttons
		// *---------------------------------------*

		/// <summary>
		/// menuItem5_Click - Change picture box view mode
		/// </summary>
		private void menuItem5_Click(object sender, System.EventArgs e)
		{
			this.menuItem5.Checked = true;
			this.menuItem6.Checked = false;
			this.CUSTOM_Viewer.ImageSizeMode = RayTracer.SizeMode.RatioStretch;
		}

		/// <summary>
		/// menuItem6_Click - Change picture box view mode
		/// </summary>
		private void menuItem6_Click(object sender, System.EventArgs e)
		{
			this.menuItem5.Checked = false;
			this.menuItem6.Checked = true;
			this.CUSTOM_Viewer.ImageSizeMode = RayTracer.SizeMode.Scrollable;
		}

		/// <summary>
		/// menuItem9_Click - Copy temp buffer to screen
		/// </summary>
		private void menuItem9_Click(object sender, System.EventArgs e)
		{
			if (rendering)
			{
				renderImage=true;
			}
		}

		/// <summary>
		/// menuItem4_Click - Save the Image
		/// </summary>
		private void menuItem4_Click(object sender, System.EventArgs e)
		{
			// Displays a SaveFileDialog so the user can save the Image
			// assigned to Button2.
			SaveFileDialog saveFileDialog1 = new SaveFileDialog();
			saveFileDialog1.Filter = "JPeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif|TIFF Image|*.tiff";
			saveFileDialog1.Title = "Save an Image File";
			saveFileDialog1.ShowDialog();

			// If the file name is not an empty string open it for saving.
			if(saveFileDialog1.FileName != "")
			{
				// Saves the Image via a FileStream created by the OpenFile method.
				System.IO.FileStream fs = 
					(System.IO.FileStream)saveFileDialog1.OpenFile();
				// Saves the Image in the appropriate ImageFormat based upon the
				// File type selected in the dialog box.
				// NOTE that the FilterIndex property is one-based.
				switch(saveFileDialog1.FilterIndex)
				{
					case 1 : 
						this.CUSTOM_Viewer.Image.Save(fs, 
							System.Drawing.Imaging.ImageFormat.Jpeg);
						break;

					case 2 : 
						this.CUSTOM_Viewer.Image.Save(fs, 
							System.Drawing.Imaging.ImageFormat.Bmp);
						break;

					case 3 : 
						this.CUSTOM_Viewer.Image.Save(fs, 
							System.Drawing.Imaging.ImageFormat.Gif);
						break;
					case 4 : 
						this.CUSTOM_Viewer.Image.Save(fs, 
							System.Drawing.Imaging.ImageFormat.Tiff);
						break;
				}
				fs.Close();
			}
		}

		/// <summary>
		/// menuItem7_Click - BUILD
		/// </summary>
		private void menuItem7_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Sphere";
			StartRender();
		}

		/// <summary>
		/// menuItem10_Click - BUILD
		/// </summary>
		private void menuItem10_Click(object sender, System.EventArgs e)
		{
			buildFunction = "4 Spheres - Side View";
			StartRender();
		}

		/// <summary>
		/// menuItem11_Click - BUILD
		/// </summary>
		private void menuItem11_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TOS - Helix";
			StartRender();
		}

		/// <summary>
		/// menuItem12_Click - BUILD
		/// </summary>
		private void menuItem12_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TOS - Spiral";
			StartRender();
		}

		/// <summary>
		/// menuItem13_Click - BUILD
		/// </summary>
		private void menuItem13_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Plane";
			StartRender();
		}

		/// <summary>
		/// menuItem16_Click - BUILD
		/// </summary>
		private void menuItem16_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TOS - Ellipsoid";
			StartRender();
		}

		/// <summary>
		/// menuItem17_Click - BUILD
		/// </summary>
		private void menuItem17_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TOS - Torus";
			StartRender();
		}

		/// <summary>
		/// menuItem19_Click - BUILD
		/// </summary>
		private void menuItem19_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Reflective - 4 Spheres - Side View";
			StartRender();
		}

		/// <summary>
		/// menuItem19_Click - BUILD (Scratch Build)
		/// </summary>
		private void menuItem20_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Scratch Build";
			StartRender();		
		}

		/// <summary>
		/// menuItem23_Click - BUILD (Transform Boxes)
		/// </summary>
		private void menuItem23_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Transform Boxes";
			StartRender();
		}

		/// <summary>
		/// menuItem24_Click - BUILD
		/// </summary>
		private void menuItem24_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TOS - Sphere";
			StartRender();
		}

		/// <summary>
		/// menuItem14_Click - BUILD
		/// </summary>
		private void menuItem14_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Boxes";
			StartRender();		
		}

		/// <summary>
		/// menuItem18_Click - BUILD
		/// </summary>
		private void menuItem18_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TransTransBox";
			StartRender();				
		}

		/// <summary>
		/// menuItem28_Click - BUILD
		/// </summary>
		private void menuItem28_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TransparentBox";
			StartRender();		
		}

		/// <summary>
		/// menuItem29_Click - BUILD
		/// </summary>
		private void menuItem29_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TransparentEllipsoid";
			StartRender();				
		}

		/// <summary>
		/// menuItem30_Click - BUILD
		/// </summary>
		private void menuItem30_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TransparentSphere";
			StartRender();			
		}

		/// <summary>
		/// menuItem32_Click - BUILD
		/// </summary>
		private void menuItem32_Click(object sender, System.EventArgs e)
		{
			buildFunction = "SpheresInBoxes";
			StartRender();	
		}

		/// <summary>
		/// menuItem33_Click - BUILD
		/// </summary>
		private void menuItem33_Click(object sender, System.EventArgs e)
		{
			buildFunction = "SpheresInTorus";
			StartRender();	
		}

		/// <summary>
		/// menuItem34_Click - BUILD
		/// </summary>
		private void menuItem34_Click(object sender, System.EventArgs e)
		{
			buildFunction = "ShinySpiral";
			StartRender();	
		}

		/// <summary>
		/// menuItem35_Click - BUILD
		/// </summary>
		private void menuItem35_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TorusSquared";
			StartRender();		
		}

		/// <summary>
		/// menuItem36_Click - BUILD
		/// </summary>
		private void menuItem36_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Slices";
			StartRender();
		}

		/// <summary>
		/// menuItem37_Click - BUILD
		/// </summary>
		private void menuItem37_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Hanoi";
			StartRender();
		}

		/// <summary>
		/// menuItem38_Click - BUILD
		/// </summary>
		private void menuItem38_Click(object sender, System.EventArgs e)
		{
			buildFunction = "BitsAndBobs";
			StartRender();
		}

		/// <summary>
		/// menuItem21_Click - BUILD
		/// </summary>
		private void menuItem21_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Planets";
			StartRender();		
		}

		/// <summary>
		/// menuItem22_Click - BUILD
		/// </summary>
		private void menuItem22_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Moons";
			StartRender();		
		}

		/// <summary>
		/// menuItem27_Click - BUILD
		/// </summary>
		private void menuItem27_Click(object sender, System.EventArgs e)
		{
			buildFunction = "SpaceScape";
			StartRender();		
		}

		/// <summary>
		/// menuItem40_Click - BUILD
		/// </summary>
		private void menuItem40_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Noise";
			StartRender();		
		}

		/// <summary>
		/// menuItem41_Click - BUILD
		/// </summary>
		private void menuItem41_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Marble";
			StartRender();		
		}

		/// <summary>
		/// menuItem42_Click - BUILD
		/// </summary>
		private void menuItem42_Click(object sender, System.EventArgs e)
		{
			buildFunction = "FractalCylinders";
			StartRender();		
		}

		/// <summary>
		/// menuItem43_Click - BUILD
		/// </summary>
		private void menuItem43_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TexturedSpiral";
			StartRender();		
		}

		/// <summary>
		/// menuItem44_Click - BUILD
		/// </summary>
		private void menuItem44_Click(object sender, System.EventArgs e)
		{
			buildFunction = "Entropy";
			StartRender();		
		}

		/// <summary>
		/// menuItem46_Click - BUILD
		/// </summary>
		private void menuItem46_Click(object sender, System.EventArgs e)
		{
			buildFunction = "SimpleVenusDemilo";
			StartRender();			
		}

		/// <summary>
		/// menuItem47_Click - BUILD
		/// </summary>
		private void menuItem47_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TexturedVenusDemilo";
			StartRender();			
		}

		/// <summary>
		/// menuItem48_Click - BUILD
		/// </summary>
		private void menuItem48_Click(object sender, System.EventArgs e)
		{
			buildFunction = "TorusOfTriangles";
			StartRender();		
		}
		// *---------------------------------------*
		// Form Specific Functions
		// *---------------------------------------*

		private void MainForm_Load(object sender, System.EventArgs e)
		{
		}
	}
}
		