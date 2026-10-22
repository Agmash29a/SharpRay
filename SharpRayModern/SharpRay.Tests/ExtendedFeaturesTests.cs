using System.Numerics;
using SharpRay.Core;
using Xunit;

namespace SharpRay.Tests;

public sealed class LegacyPrimitiveTests
{
    private static readonly Material Matte = new(new Vector3(0.6f, 0.7f, 0.8f));

    [Fact]
    public void DiscAcceptsInsideAndRejectsOutsideRadius()
    {
        var disc = new Disc(Vector3.Zero, Vector3.UnitY, 2, Matte);
        Assert.True(disc.TryHit(new Ray(new Vector3(1, 3, 0), -Vector3.UnitY), 0.001f, 10, out Hit hit));
        Assert.InRange(hit.Distance, 2.999f, 3.001f);
        Assert.False(disc.TryHit(new Ray(new Vector3(3, 3, 0), -Vector3.UnitY), 0.001f, 10, out _));
        Assert.True(disc.BoundingBox.Intersects(new Ray(new Vector3(1, 3, 0), -Vector3.UnitY), 0.001f, 10));
    }

    [Fact]
    public void OpenCylinderTestsBothQuadraticRootsAndVerticalClipping()
    {
        var cylinder = new OpenCylinder(Vector3.Zero, 1, 2, Matte);
        Ray outside = new(new Vector3(0, 0, 5), -Vector3.UnitZ);
        Assert.True(cylinder.TryHit(outside, 0.001f, 10, out Hit hit));
        Assert.InRange(hit.Distance, 3.999f, 4.001f);
        Assert.False(cylinder.TryHit(new Ray(new Vector3(0, 2, 5), -Vector3.UnitZ),
            0.001f, 10, out _));
        Assert.True(cylinder.TryHit(new Ray(Vector3.Zero, Vector3.UnitX), 0.001f, 10, out Hit inside));
        Assert.False(inside.FrontFace);
    }

    [Fact]
    public void TorusQuarticReturnsNearestForwardIntersection()
    {
        var torus = new Torus(Vector3.Zero, 2, 0.5f, Matte);
        Ray ray = new(new Vector3(0, 0, 5), -Vector3.UnitZ);
        Assert.True(torus.TryHit(ray, 0.001f, 10, out Hit hit));
        Assert.InRange(hit.Distance, 2.499f, 2.501f);
        Assert.True(hit.FrontFace);
        Assert.False(torus.TryHit(new Ray(new Vector3(0, 2, 5), -Vector3.UnitZ),
            0.001f, 20, out _));
        Assert.True(torus.BoundingBox.Intersects(ray, 0.001f, 10));
    }

    [Fact]
    public void QuarticRootIsolationFindsFourRealRoots()
    {
        var roots = PolynomialRoots.RealRoots(4, 0, -5, 0, 1);
        Assert.Equal(4, roots.Count);
        Assert.InRange(roots[0], -2.00001, -1.99999);
        Assert.InRange(roots[1], -1.00001, -0.99999);
        Assert.InRange(roots[2], 0.99999, 1.00001);
        Assert.InRange(roots[3], 1.99999, 2.00001);
    }

    [Fact]
    public void QuarticFindsDoubleRootTangency()
    {
        // (x - 1)^2 (x + 2)^2
        var roots = PolynomialRoots.RealRoots(4, -4, -3, 2, 1);
        Assert.Equal(2, roots.Count);
        Assert.InRange(roots[0], -2.00001, -1.99999);
        Assert.InRange(roots[1], 0.99999, 1.00001);
    }

    [Fact]
    public void TransformedInstancePreservesDistancesAndNormalOrientation()
    {
        var sphere = new Sphere(Vector3.Zero, 1, Matte);
        Matrix4x4 matrix = Matrix4x4.CreateScale(2f) *
                           Matrix4x4.CreateTranslation(3f, 0, 0);
        var instance = new TransformedObject(sphere, matrix);
        Ray ray = new(new Vector3(3, 0, 10), -Vector3.UnitZ);
        Assert.True(instance.TryHit(ray, 0.001f, 20, out Hit hit));
        Assert.InRange(hit.Distance, 7.999f, 8.001f);
        Assert.InRange(hit.Normal.Z, 0.999f, 1f);
        Assert.True(instance.BoundingBox.Intersects(ray, 0.001f, 20));
    }

    [Fact]
    public void RotatedInstanceTransformsNormalWithInverseTranspose()
    {
        var box = new AxisBox(new Vector3(-1), new Vector3(1), Matte);
        var rotated = new TransformedObject(box, Matrix4x4.CreateRotationY(MathF.PI / 2));
        Ray ray = new(new Vector3(0, 0, 5), -Vector3.UnitZ);
        Assert.True(rotated.TryHit(ray, 0.001f, 10, out Hit hit));
        Assert.True(hit.Normal.Z > 0.9f);
    }

    [Fact]
    public void CompoundUsesBvhForManyObjects()
    {
        var shapes = Enumerable.Range(0, 100)
            .Select(i => (IBoundedSceneObject)new Sphere(new Vector3(i * 4, 0, 0), 1, Matte))
            .ToList();
        var compound = new Compound(shapes);
        Assert.True(compound.TryHit(new Ray(new Vector3(0, 0, 5), -Vector3.UnitZ),
            0.001f, 100, out Hit hit));
        Assert.InRange(hit.Distance, 3.999f, 4.001f);
        Assert.False(compound.TryHit(new Ray(new Vector3(-3, 0, 5), -Vector3.UnitZ),
            0.001f, 100, out _));
    }

    [Fact]
    public void BvhMatchesLinearWithTransformedObjectsAndTorus()
    {
        Scene scene = new();
        scene.Objects.Add(new Torus(Vector3.Zero, 1.7f, 0.3f, Matte));
        scene.Objects.Add(new TransformedObject(new Sphere(Vector3.Zero, 1, Matte),
            Matrix4x4.CreateScale(0.6f) * Matrix4x4.CreateTranslation(0, 0, -4)));
        scene.Objects.Add(new OpenCylinder(new Vector3(3, 0, -2), 0.5f, 2, Matte));
        CompiledScene bvh = scene.Compile();
        for (int iy = -6; iy <= 6; iy++)
        for (int ix = -6; ix <= 6; ix++)
        {
            Ray ray = new(new Vector3(0, 0, 5),
                Vector3.Normalize(new Vector3(ix * 0.14f, iy * 0.14f, -1)));
            bool a = scene.TryHit(ray, 0.0001f, 100, out Hit slow);
            bool b = bvh.TryHit(ray, 0.0001f, 100, out Hit fast);
            Assert.Equal(a, b);
            if (a) Assert.InRange(MathF.Abs(slow.Distance - fast.Distance), 0, 0.001f);
        }
    }
}

public sealed class MeshImportTests
{
    private static readonly Material Material = new(Vector3.One);

    [Fact]
    public void LoadsHistoricalSharpRayTriangleText()
    {
        string text = "triangle { vertex 0 0 0 normal 0 0 1 vertex 1 0 0 normal 0 0 1 vertex 0 1 0 normal 0 0 1 }\n";
        string path = WriteTemp(".txt", text);
        try
        {
            var mesh = MeshImporter.Load(path, Material);
            Assert.True(mesh.TryHit(new Ray(new Vector3(0.25f, 0.25f, 2), -Vector3.UnitZ),
                0.001f, 10, out Hit hit));
            Assert.InRange(hit.Distance, 1.999f, 2.001f);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void LoadsAsciiPlyQuadAndTriangulatesFaces()
    {
        string ply = "ply\nformat ascii 1.0\nelement vertex 4\nproperty float x\n" +
            "property float y\nproperty float z\nelement face 1\n" +
            "property list uchar int vertex_indices\nend_header\n" +
            "0 0 0\n1 0 0\n1 1 0\n0 1 0\n4 0 1 2 3\n";
        string path = WriteTemp(".ply", ply);
        try
        {
            var mesh = MeshImporter.Load(path, Material);
            Assert.True(mesh.TryHit(new Ray(new Vector3(0.25f, 0.75f, 2), -Vector3.UnitZ),
                0.001f, 10, out _));
            Assert.True(mesh.TryHit(new Ray(new Vector3(0.75f, 0.25f, 2), -Vector3.UnitZ),
                0.001f, 10, out _));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void RejectsPlyWithInvalidFaceIndex()
    {
        string ply = "ply\nformat ascii 1.0\nelement vertex 3\nproperty float x\n" +
            "property float y\nproperty float z\nelement face 1\n" +
            "property list uchar int vertex_indices\nend_header\n" +
            "0 0 0\n1 0 0\n0 1 0\n3 0 1 100\n";
        string path = WriteTemp(".ply", ply);
        try { Assert.Throws<InvalidDataException>(() => MeshImporter.Load(path, Material)); }
        finally { File.Delete(path); }
    }

    private static string WriteTemp(string extension, string text)
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + extension);
        File.WriteAllText(path, text);
        return path;
    }
}

public sealed class ExtendedTextureTests
{
    private static readonly TextureContext Sample =
        new(Vector3.Zero, new Vector3(0.7f, 1.3f, -0.4f), Vector2.Zero);

    [Fact]
    public void TransformTextureChangesCheckerSampling()
    {
        ITexture checker = new CheckerTexture(Vector3.One, Vector3.Zero);
        var moved = new TextureInstance(checker, Matrix4x4.CreateTranslation(1, 0, 0));
        TextureContext sample = new(new Vector3(0.1f, 0.1f, 0.1f), Vector3.Zero, Vector2.Zero);
        Assert.NotEqual(checker.Sample(sample), moved.Sample(sample));
    }

    [Fact]
    public void WrappedFbmFractalSumAndMarbleRampAreFinite()
    {
        var pixels = new[] { Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, Vector3.One };
        var ramp = new ImageTexture(4, 1, pixels);
        ITexture[] textures =
        [
            new FbmWrapTexture(Vector3.Zero, Vector3.One),
            new FractalSumTexture(Vector3.Zero, Vector3.One),
            new MarbleRampTexture(ramp)
        ];
        foreach (ITexture tex in textures)
        {
            Vector3 a = tex.Sample(Sample);
            Vector3 b = tex.Sample(Sample);
            Assert.Equal(a, b);
            Assert.True(float.IsFinite(a.X) && float.IsFinite(a.Y) && float.IsFinite(a.Z));
            Assert.InRange(a.X, 0, 1);
            Assert.InRange(a.Y, 0, 1);
            Assert.InRange(a.Z, 0, 1);
        }
    }
}

public sealed class InteractiveEditorMathTests
{
    [Fact]
    public void GizmoDragFollowsOnlyTheSelectedProjectedAxis()
    {
        Vector2 start = new(100, 100), end = new(200, 100);
        Vector2 press = new(150, 100);
        Assert.InRange(GizmoMath.AxisDisplacement(start, end, press,
            new Vector2(200, 140)), 0.499f, 0.501f);
        Assert.InRange(GizmoMath.AxisDisplacement(start, end, press,
            new Vector2(100, 70)), -0.501f, -0.499f);
    }

    [Fact]
    public void GizmoHitTestMeasuresDistanceToLineSegment()
    {
        Assert.InRange(GizmoMath.DistanceToAxis(new Vector2(140, 105),
            new Vector2(100, 100), new Vector2(200, 100)), 4.999f, 5.001f);
        Assert.InRange(GizmoMath.DistanceToAxis(new Vector2(50, 100),
            new Vector2(100, 100), new Vector2(200, 100)), 49.999f, 50.001f);
    }

    [Fact]
    public void CameraProjectionRoundTripsThroughScreenRay()
    {
        Camera camera = new(new Vector3(0, 0, 5), Vector3.Zero);
        Vector3 position = new(1, 0.7f, 0);
        Assert.True(camera.TryProject(position, 16f / 9f, out Vector2 projected));
        Ray ray = camera.GetRay(projected.X, projected.Y, 16f / 9f);
        Vector3 expected = Vector3.Normalize(position - new Vector3(0, 0, 5));
        Assert.InRange(Vector3.Distance(expected, ray.Direction), 0, 0.00001f);
    }

    [Fact]
    public void CameraRejectsBehindCameraProjection()
    {
        Camera camera = new(Vector3.Zero, -Vector3.UnitZ);
        Assert.False(camera.TryProject(new Vector3(0, 0, 2), 1, out _));
    }
}

public sealed class DirectionalLightTests
{
    [Fact]
    public async Task DirectionalLightIlluminatesVisibleSphere()
    {
        Scene dark = new() { AmbientIntensity = 0, SkyColour = Vector3.Zero };
        dark.Objects.Add(new Sphere(Vector3.Zero, 1, new Material(Vector3.One)));
        Camera camera = new(new Vector3(0, 0, 4), Vector3.Zero);
        RenderOptions options = new(9, 9, 1, 0);
        int[] unlit = await new Renderer().RenderAsync(dark, camera, options);
        dark.DirectionalLights.Add(new DirectionalLight(Vector3.UnitZ, Vector3.One, 1));
        int[] lit = await new Renderer().RenderAsync(dark, camera, options);
        Assert.True((lit[4 * 9 + 4] & 0xFFFFFF) > (unlit[4 * 9 + 4] & 0xFFFFFF));
    }

    [Fact]
    public void CompiledSceneSnapshotsLightsAtBuildTime()
    {
        Scene scene = new();
        scene.DirectionalLights.Add(new DirectionalLight(Vector3.UnitZ, Vector3.One, 1));
        CompiledScene snapshot = scene.Compile();
        scene.DirectionalLights.Clear();
        Assert.Single(snapshot.DirectionalLights);
    }
}


public sealed class HistoricalAssetIntegrationTests
{
    private static string Asset(params string[] parts)
    {
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "..", "..", "..", "..", ".."));
        return Path.Combine(new[] { root }.Concat(parts).ToArray());
    }

    [Fact]
    public void OriginalVenusDeMiloTriangleDatasetImportsAndIntersects()
    {
        string source = Asset("RayTracer", "Gfx", "PLY", "VenusDeMilo.txt");
        Assert.True(File.Exists(source), "Expected the original SharpRay historical assets in checkout.");
        Compound mesh = MeshImporter.Load(source, new Material(Vector3.One));
        Assert.True(mesh.BoundingBox.Max.Y > mesh.BoundingBox.Min.Y);
        Assert.True(mesh.BoundingBox.Max.X > mesh.BoundingBox.Min.X);
        Assert.True(mesh.BoundingBox.Max.Z > mesh.BoundingBox.Min.Z);
    }

    [Theory]
    [InlineData("Mercator", "earthmap.bmp")]
    [InlineData("Ramps", "MarbleRamp.bmp")]
    public void OriginalSharpRayBitmapFilesImport(string directory, string filename)
    {
        string source = Asset("RayTracer", "Gfx", directory, filename);
        Assert.True(File.Exists(source));
        ImageTexture bitmap = ImageTexture.FromBmp(source);
        Assert.True(bitmap.Width > 0 && bitmap.Height > 0);
        Vector3 pixel = bitmap.Sample(new TextureContext(Vector3.Zero, Vector3.Zero, new Vector2(0.25f)));
        Assert.True(float.IsFinite(pixel.X) && float.IsFinite(pixel.Y) && float.IsFinite(pixel.Z));
    }
}
