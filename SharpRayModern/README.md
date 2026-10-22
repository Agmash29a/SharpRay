# SharpRay Modern — feature branch

This directory contains a new .NET 8 ray tracer and Windows Forms scene editor inspired by the algorithms in Riley T. Perry's original 2004 SharpRay. It is separate from the original .NET Framework 2.0 project and does not change the master branch.

## Features

- CPU ray tracer: spheres, planes, boxes, triangles; shadows, direct diffuse/specular light, reflections and Snell-law refraction with Schlick Fresnel
- Immutable median-split bounding-volume hierarchy (BVH) for finite primitives, unbounded fallback for planes
- Linear and smooth-cubic value noise, fBm, turbulence and procedural marble, plus solid and checker textures
- Uncompressed 24/32-bit BMP image mapping with UV, spherical, cylindrical and planar projections
- Property-inspector-based Windows Forms scene editor, with shape/light/camera controls, render/cancel, BMP selection, PNG export and JSON scene save/load
- Parallel CPU rendering, gamma correction, deterministic supersampling
- xUnit tests for geometry, BVH correctness against linear traversal, noise, images and rendering
- GitHub Actions workflow compiling and testing this feature branch on Windows

## Run

Windows 10/11 and .NET 8 SDK (or Visual Studio 2022 + .NET desktop development):

    dotnet run --project SharpRayModern/SharpRay.WinForms/SharpRay.WinForms.csproj -c Release

Tests:

    dotnet test SharpRayModern/SharpRay.Tests/SharpRay.Tests.csproj -c Release

## Editing

Use the left-hand Objects and Lights tabs to add and select entities. Select Camera to edit view position and target. Change selected properties in the inspector below the list, then press Render. Choose BMP selects an image for the selected shape. Use File to save/load JSON scenes or export PNG. For textures, choose Solid, Checker, Noise, FBm, Turbulence, Marble, or Bitmap and adjust frequency/octaves/colours. Colours are HTML RGB strings such as #AA6633.

BMP image paths in saved scenes are retained as entered. When sharing a scene JSON, also share its BMP images or update paths.

## Mapping from historical SharpRay

The original CubicNoise, fBm, Turbulence and Marble_Procedural concepts are reimplemented using deterministic immutable texture samplers. The original ImageTexture and Spherical/Cylindrical/Rectangular mapping concepts are reimplemented as platform-independent BMP texture sampling. Reflective and Transparent materials are represented by new material parameters. The historical uniform-grid accelerator is replaced with a BVH, with a linear reference path for testing.

This is not yet a full faithful port of all old algorithms, scene presets or shapes. Torus, PLY import, object instancing, transformable textures, full old scene catalog, area lights and GPU rendering remain future work. The editor is a property grid, not an in-viewport drag-gizmo editor. Textures will not always reproduce original pixel-for-pixel because the noise implementation and colour management differ.

The original BMP assets are still in RayTracer/Gfx on this branch; choose them through the scene editor when running from the cloned repository.
