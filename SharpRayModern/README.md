# SharpRay Modern — .NET 8 ray tracer and scene editor

This is a substantial new C# implementation of Riley T. Perry's 2004 **SharpRay**, developed as a feature branch alongside the original .NET Framework 2.0 project. The historical source and images under RayTracer/ remain untouched.

## Getting started

Requires Windows 10/11 and the .NET 8 SDK or Visual Studio 2022 with the .NET desktop workload.

From the repository root:

    dotnet run --project SharpRayModern/SharpRay.WinForms/SharpRay.WinForms.csproj -c Release

The editor opens with a default render. Use the left panel to add shapes, lights and edit the camera or world; press **Render** to update. Use **Cancel** for long renders. Save images from **File > Export PNG**, or save/reopen a scene with JSON.

## Interactive viewport controls

- Click a rendered sphere, box, cylinder, triangle, disc or torus to select it in the Objects list.
- Drag the **red X**, **green Y** or **blue Z** arrow of the selected object.
- Choose **Move**, **Rotate** or **Scale** in the top toolbar; dragging a projected handle edits the corresponding axis.
- The inspector updates as you drag; the expensive CPU ray trace is refreshed **on mouse release**, not continuously.
- Axis handles always use world X/Y/Z. Camera-parallel handles are suppressed when their projection is too short.
- For precision editing, directly enter numeric values in the Properties panel.
- In the **World** tab, adjust sky colour, coloured ambient light and recursion depth.

## Engine and materials

| Historical area | Modern implementation |
|---|---|
| Sphere/generic sphere, axis box/generic cube, plane, triangle | Analytic primitives; configurable transforms |
| Open cylinder, disc, quartic torus | Analytic intersections, face normals and UV |
| Compound and Instance | BVH-backed compounds and full affine instancing with inverse-transpose normals |
| Accelerated Triangle / uniform Grid | Immutable median-split BVH (bounded) and separate infinite planes |
| Textured/reflective/transparent Phong | Material with Ka/Kd/Ks, shininess, reflectivity, Snell refraction and Schlick Fresnel |
| Ambient, directional, point lights | Ambient intensity/colour, direct illumination, hard shadows |
| Cubic and linear value noise, fBm, fBmWrap, FractalSum, turbulence | Thread-safe deterministic procedural samplers |
| Procedural marble, bitmap marble ramps | Analytic and texture-ramp marble |
| Checkerboard, image textures, TextureInstance | Textures, inverse affine texture transforms, BMP loader |
| Rectangular, spherical and cylindrical mapping | Modern modes plus matching historical UV coordinate conventions |
| PinHole camera, ViewPlane | Perspective camera, configurable dimensions, sampling and recursion |
| PLY reader | ASCII PLY and SharpRay's historical triangle/normal text format |
| Old Windows Forms viewer | Editor, ray-based scene selection, viewport drag/rotate/scale handles, JSON and PNG |
| Hand-managed thread and arrays | Task/Parallel CPU renderer, immutable compiled scene, CancellationToken, typed collections |

The 2004 sources contain numerous named World.Build_* scenes. The **Legacy scenes** menu includes reconstructed examples for SingleSphere, 4SpheresSideView, Boxes, Hanoi, Marble, FractalCylinders and TOSTorus, as well as the modern demonstration. Historical coordinates, materials and layouts provide a basis for these editable scenes, but the shaders, noise hashes, antialiasing and pinhole camera parameterization are modern implementations, so images will differ.

## Tests

Core regression tests, covering rendering, geometry, BVH versus exhaustive traversal, quartic torus roots, historical mesh import, bitmap assets, materials, texture sampling, shadows, camera projection and gizmo math:

    dotnet test SharpRayModern/SharpRay.Tests/SharpRay.Tests.csproj -c Release

Windows editor/model tests, covering JSON round-tripping, scene editing, geometry options, historical presets and render smoke tests:

    dotnet test SharpRayModern/SharpRay.SceneTests/SharpRay.SceneTests.csproj -c Release

GitHub Actions automatically builds the Windows Forms project and runs **both test projects** when this feature branch is pushed or relevant files change.

## Source structure

    SharpRayModern/
      SharpRay.Core/
        Geometry.cs              Primitive contracts, hit info, scene
        AdditionalGeometry.cs     Disc, cylinder, torus, instancing, compounds
        MeshImporter.cs          ASCII PLY and historical triangle file support
        Bvh.cs                   Acceleration and compiled scene
        Textures.cs              Basic procedural and bitmap textures
        LegacyTextures.cs        Advanced old procedural families and texture instances
        Renderer.cs              Cameras, parallel CPU renderer, Phong-style tracing
        GizmoMath.cs             Tested screen-space manipulator maths
      SharpRay.WinForms/
        SceneDocument.cs         Editable, JSON-serializable scene model
        SceneViewport.cs         Interactive selection and XYZ manipulators
        LegacySceneCatalog.cs    Reconstructed original scenes
        MainForm.cs              Editor and renderer UI
      SharpRay.Tests/
      SharpRay.SceneTests/

## Known differences and remaining parity work

This is **not yet a pixel-for-pixel complete port of the entire historical project**. Seven of the many original named scenes are reconstructed, rather than every World.Build_* method. In particular, not every original per-scene constant, pixel sampling rule or noise function is bit-identical. Mesh import currently uses flat triangle normals; smooth vertex normals and some older specialized transforms are not preserved. The editor has move/rotate/scale gizmos but not arbitrary mesh-vertex editing or realtime GPU preview. Soft shadows, a GPU path, a node-based material editor and a full historical-scene-by-scene visual regression suite are future work.

The original BMP and text assets remain in RayTracer/Gfx and can be selected in the editor or opened from the Legacy scenes presets. If you distribute a JSON scene separately, include any referenced files and correct their saved absolute paths.

The untouched original sources remain the archival reference and enable a careful step-by-step comparison.
