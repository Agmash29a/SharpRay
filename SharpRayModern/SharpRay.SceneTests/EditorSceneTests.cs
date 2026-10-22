using System.Numerics;
using SharpRay.Core;
using SharpRay.WinForms;
using Xunit;

namespace SharpRay.SceneTests;

public sealed class SceneDocumentTests
{
    [Fact]
    public void DemoSceneBuildsAndHasWorkingBvh()
    {
        SceneDocument doc = SceneDocument.CreateDemo();
        var (scene, camera) = doc.Build();
        Assert.True(scene.Objects.Count >= 5);
        Assert.Equal(2, scene.Lights.Count);
        CompiledScene bvh = scene.Compile();
        Ray ray = camera.GetRay(0.5f, 0.5f, 800f / 500f);
        Assert.True(bvh.TryHit(ray, 0.0001f, 100, out _));
    }

    [Fact]
    public void JsonRoundTripPreservesNewGeometryAndTextureSettings()
    {
        var doc = new SceneDocument();
        doc.Objects.Add(new EditableShape
        {
            Name = "Glass torus", Shape = ShapeKind.Torus, Radius = 2.4f,
            MinorRadius = 0.6f, RotateY = 24, RotateX = 45,
            ScaleX = 2, ScaleY = 0.8f, ScaleZ = 2.3f,
            Texture = TextureKind.FbmWrap, TextureScale = 3,
            TransformTexture = true, TextureRotationY = 40,
            Reflectivity = 0.3f, Transparency = 0.4f
        });
        doc.Lights.Add(new EditableLight { Name = "Sun", Directional = true,
            X = 1, Y = 2, Z = 3 });
        string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");
        try
        {
            doc.Save(path);
            SceneDocument reloaded = SceneDocument.Load(path);
            EditableShape shape = Assert.Single(reloaded.Objects);
            Assert.Equal(ShapeKind.Torus, shape.Shape);
            Assert.Equal(TextureKind.FbmWrap, shape.Texture);
            Assert.Equal(24, shape.RotateY);
            Assert.Equal(0.6f, shape.MinorRadius);
            Assert.Equal(3, shape.TextureScale);
            Assert.True(shape.TransformTexture);
            var (scene, _) = reloaded.Build();
            Assert.Single(scene.Objects);
            Assert.Single(scene.DirectionalLights);
            Assert.Equal(0, scene.Lights.Count);
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void AllSupportedGeometryShapesBuild()
    {
        var doc = new SceneDocument();
        foreach (ShapeKind kind in Enum.GetValues<ShapeKind>())
        {
            if (kind == ShapeKind.Mesh) continue;
            doc.Objects.Add(new EditableShape { Shape = kind, Radius = 1.5f,
                MinorRadius = 0.3f, SizeY = 2, RotateX = 20 });
        }
        var (scene, _) = doc.Build();
        Assert.Equal(Enum.GetValues<ShapeKind>().Length - 1, scene.Objects.Count);
        Assert.All(scene.Objects, obj => Assert.NotNull(obj));
    }

    [Fact]
    public void EditingPositionMovesGeometryWithoutRewritingRayTracingCode()
    {
        var shape = new EditableShape { Name = "Sphere", Shape = ShapeKind.Sphere, Radius = 1, X = 3 };
        var doc = new SceneDocument();
        doc.Objects.Add(shape);
        (Scene scene, _) = doc.Build();
        Assert.True(scene.TryHit(new Ray(new Vector3(3, 0, 5), -Vector3.UnitZ),
            0.001f, 20, out _));
        shape.X = -3;
        (scene, _) = doc.Build();
        Assert.False(scene.TryHit(new Ray(new Vector3(3, 0, 5), -Vector3.UnitZ),
            0.001f, 20, out _));
        Assert.True(scene.TryHit(new Ray(new Vector3(-3, 0, 5), -Vector3.UnitZ),
            0.001f, 20, out _));
    }

    [Fact]
    public void InvalidBitmapReferenceIsRejectedClearly()
    {
        var doc = new SceneDocument();
        doc.Objects.Add(new EditableShape { Texture = TextureKind.Bitmap });
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() => doc.Build());
        Assert.Contains("BMP", exception.Message);
    }
}

public sealed class LegacyCatalogTests
{
    public static IEnumerable<object[]> Presets => LegacySceneCatalog.Names.Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(Presets))]
    public void EveryHistoricalPresetBuildsEditableGeometry(string name)
    {
        SceneDocument doc = LegacySceneCatalog.Create(name);
        Assert.NotEmpty(doc.Objects);
        Assert.NotEmpty(doc.Lights);
        var (scene, camera) = doc.Build();
        Assert.Equal(doc.Objects.Count, scene.Objects.Count);
        Assert.NotNull(camera);
        Assert.NotNull(scene.Compile());
    }

    [Fact]
    public void HanoiMatchesFiveHistoricalRingsAndCentralCylinder()
    {
        SceneDocument doc = LegacySceneCatalog.Create("2004 / Hanoi");
        Assert.Equal(5, doc.Objects.Count(o => o.Shape == ShapeKind.Torus));
        Assert.Single(doc.Objects.Where(o => o.Shape == ShapeKind.Cylinder));
        Assert.All(doc.Objects.Where(o => o.Shape == ShapeKind.Torus),
            ring => Assert.True(ring.Reflectivity > 0));
    }

    [Fact]
    public void OriginalParametricTorusSamplingIsPortedWithTheSameCount()
    {
        SceneDocument doc = LegacySceneCatalog.Create("2004 / Spheres on a torus");
        Assert.Equal(33 * 33, doc.Objects.Count(o => o.Shape == ShapeKind.Sphere));
    }

    [Fact]
    public async Task OriginalSingleSpherePresetProducesAnImage()
    {
        SceneDocument doc = LegacySceneCatalog.Create("2004 / Single sphere");
        var (scene, camera) = doc.Build();
        int[] pixels = await new Renderer().RenderAsync(scene, camera,
            new RenderOptions(21, 15, 1, 1));
        Assert.Equal(21 * 15, pixels.Length);
        Assert.True(pixels.Distinct().Count() > 2);
    }
}
