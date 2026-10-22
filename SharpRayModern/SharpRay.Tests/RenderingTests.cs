using System.Numerics;
using SharpRay.Core;
using Xunit;

namespace SharpRay.Tests;

public class GeometryAndBvhTests
{
    [Fact]
    public void SphereIntersectionUsesNearestPositiveRoot()
    {
        var sphere = new Sphere(Vector3.Zero, 1, new Material(Vector3.One));
        var ray = new Ray(new Vector3(0, 0, 5), -Vector3.UnitZ);
        Assert.True(sphere.TryHit(ray, 0.001f, 100, out Hit hit));
        Assert.InRange(hit.Distance, 3.999f, 4.001f);
        Assert.True(hit.FrontFace);
        Assert.InRange(hit.UV.X, 0, 1);
        Assert.InRange(hit.UV.Y, 0, 1);
    }

    [Fact]
    public void InteriorSphereHitHasReversedFaceNormal()
    {
        var sphere = new Sphere(Vector3.Zero, 1, new Material(Vector3.One));
        Assert.True(sphere.TryHit(new Ray(Vector3.Zero, Vector3.UnitX), 0.001f, 100, out Hit hit));
        Assert.False(hit.FrontFace);
        Assert.True(Vector3.Dot(hit.Normal, Vector3.UnitX) < 0);
    }

    [Fact]
    public void BoxParallelRaysAreCorrectlyRejectedOrAccepted()
    {
        var b = new Bounds(new Vector3(-1), new Vector3(1));
        Assert.False(b.Intersects(new Ray(new Vector3(2, 0, 0), Vector3.UnitY), 0, 100));
        Assert.True(b.Intersects(new Ray(new Vector3(0, 0, 5), -Vector3.UnitZ), 0, 100));
        var box = new AxisBox(new Vector3(-1), new Vector3(1), new Material(Vector3.One));
        Assert.True(box.TryHit(new Ray(new Vector3(0, 0, 5), -Vector3.UnitZ), 0.001f, 100, out Hit hit));
        Assert.InRange(hit.Distance, 3.999f, 4.001f);
    }

    [Fact]
    public void BvhProducesSameNearestHitAsLinearTraversalIncludingPlane()
    {
        Scene scene = new();
        var material = new Material(Vector3.One);
        scene.Objects.Add(new Plane(new Vector3(0, -2, 0), Vector3.UnitY, material));
        for (int i = 0; i < 35; i++)
            scene.Objects.Add(new Sphere(new Vector3((i % 7) - 3, (i / 7) * 0.7f, -4 - i * 0.8f),
                0.32f, material));
        scene.Objects.Add(new AxisBox(new Vector3(-0.7f, -0.2f, -3.8f),
            new Vector3(0.5f, 0.6f, -2.7f), material));
        CompiledScene accelerated = scene.Compile();
        for (int y = -4; y <= 4; y++)
        for (int x = -6; x <= 6; x++)
        {
            Ray ray = new(new Vector3(0, 1, 3),
                Vector3.Normalize(new Vector3(x * 0.16f, y * 0.14f, -1)));
            bool expected = scene.TryHit(ray, 0.001f, 1000, out Hit slow);
            bool actual = accelerated.TryHit(ray, 0.001f, 1000, out Hit fast);
            Assert.Equal(expected, actual);
            if (expected)
                Assert.InRange(MathF.Abs(slow.Distance - fast.Distance), 0, 0.0001f);
        }
    }

    [Fact]
    public void TriangleHitsAndHasFiniteBounds()
    {
        var triangle = new Triangle(Vector3.Zero, Vector3.UnitX, Vector3.UnitY,
            new Material(Vector3.One));
        Assert.True(triangle.TryHit(new Ray(new Vector3(0.2f, 0.2f, 1), -Vector3.UnitZ),
            0.001f, 100, out Hit hit));
        Assert.InRange(hit.Distance, 0.999f, 1.001f);
        Assert.True(triangle.BoundingBox.Intersects(
            new Ray(new Vector3(0.2f, 0.2f, 1), -Vector3.UnitZ), 0.001f, 100));
    }
}

public class TextureTests
{
    [Fact]
    public void CheckerTextureAlternatesAcrossGridCells()
    {
        var checker = new CheckerTexture(Vector3.One, Vector3.Zero);
        Vector3 a = checker.Sample(new TextureContext(new Vector3(0.2f), Vector3.Zero, Vector2.Zero));
        Vector3 b = checker.Sample(new TextureContext(new Vector3(1.2f, 0.2f, 0.2f), Vector3.Zero, Vector2.Zero));
        Assert.Equal(Vector3.One, a);
        Assert.Equal(Vector3.Zero, b);
    }

    [Fact]
    public void ImageTextureSamplesChosenTexels()
    {
        var image = new ImageTexture(2, 2,
            [Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, Vector3.One]);
        Assert.Equal(Vector3.UnitX, image.Sample(new TextureContext(Vector3.Zero, Vector3.Zero, new Vector2(0.2f, 0.2f))));
        Assert.Equal(Vector3.One, image.Sample(new TextureContext(Vector3.Zero, Vector3.Zero, new Vector2(0.7f, 0.7f))));
    }

    [Theory]
    [InlineData(ProceduralKind.Noise)]
    [InlineData(ProceduralKind.FBm)]
    [InlineData(ProceduralKind.Turbulence)]
    [InlineData(ProceduralKind.Marble)]
    public void ProceduralTextureIsDeterministicAndBounded(ProceduralKind kind)
    {
        var texture = new ProceduralTexture(kind, Vector3.Zero, Vector3.One);
        TextureContext point = new(Vector3.Zero, new Vector3(1.2f, -0.4f, 3.3f), Vector2.Zero);
        Vector3 a = texture.Sample(point);
        Vector3 b = texture.Sample(point);
        Assert.Equal(a, b);
        Assert.InRange(a.X, 0, 1);
        Assert.InRange(a.Y, 0, 1);
        Assert.InRange(a.Z, 0, 1);
    }

    [Fact]
    public void BitmapReaderLoadsTwentyFourBitImage()
    {
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".bmp");
        try
        {
            using (var stream = File.Create(path))
            using (var writer = new BinaryWriter(stream))
            {
                // 1 x 1, 24-bit, bottom-up, 4 bytes per scanline.
                writer.Write((ushort)0x4D42);
                writer.Write((uint)58);
                writer.Write((uint)0);
                writer.Write((uint)54);
                writer.Write((uint)40);
                writer.Write(1); writer.Write(1);
                writer.Write((ushort)1);
                writer.Write((ushort)24);
                writer.Write((uint)0);
                writer.Write((uint)4);
                writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
                writer.Write((byte)0); writer.Write((byte)0); writer.Write((byte)255); writer.Write((byte)0);
            }
            ImageTexture image = ImageTexture.FromBmp(path);
            Vector3 colour = image.Sample(new TextureContext(Vector3.Zero, Vector3.Zero, Vector2.Zero));
            Assert.InRange(colour.X, 0.99f, 1);
            Assert.InRange(colour.Y, 0, 0.01f);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}

public class RendererTests
{
    [Fact]
    public async Task RenderProducesOpaquePixels()
    {
        var (scene, camera) = DemoScene.Create();
        int[] pixels = await new Renderer().RenderAsync(scene, camera,
            new RenderOptions(20, 14, 1, 2));
        Assert.Equal(280, pixels.Length);
        Assert.All(pixels, pixel => Assert.Equal(0xFFu, ((uint)pixel) >> 24));
        Assert.True(pixels.Distinct().Count() > 2);
    }

    [Fact]
    public async Task PreCancelledRenderDoesNotComplete()
    {
        var (scene, camera) = DemoScene.Create();
        using CancellationTokenSource source = new();
        source.Cancel();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            new Renderer().RenderAsync(scene, camera,
                new RenderOptions(20, 14, 1), cancellationToken: source.Token));
    }

    [Fact]
    public void InvalidRenderDimensionsAreRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderOptions(0, 30).Validate());
        Assert.Throws<ArgumentOutOfRangeException>(() => new RenderOptions(30, 0).Validate());
    }
}
