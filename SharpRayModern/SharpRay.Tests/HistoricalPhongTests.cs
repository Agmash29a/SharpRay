using System.Numerics;
using SharpRay.Core;
using Xunit;

namespace SharpRay.Tests;

public sealed class HistoricalPhongTests
{
    [Fact]
    public async Task KaControlsColouredAmbientLight()
    {
        Camera camera = new(new Vector3(0, 0, 5), Vector3.Zero);
        RenderOptions options = new(9, 9, 1, 0);

        static Scene MakeScene(float ka)
        {
            Scene scene = new()
            {
                AmbientIntensity = 1,
                AmbientColour = Vector3.UnitX,
                SkyColour = Vector3.Zero
            };
            scene.Objects.Add(new Sphere(Vector3.Zero, 1,
                new Material(Vector3.One, Ka: ka)));
            return scene;
        }

        int centre = 4 * 9 + 4;
        int[] dark = await new Renderer().RenderAsync(MakeScene(0), camera, options);
        int[] bright = await new Renderer().RenderAsync(MakeScene(1), camera, options);
        Assert.Equal(0, dark[centre] & 0xFFFFFF);
        Assert.NotEqual(0, bright[centre] & 0xFF0000);
        Assert.Equal(0, bright[centre] & 0x00FFFF);
    }

    [Fact]
    public async Task KdControlsDiffuseIllumination()
    {
        Camera camera = new(new Vector3(0, 0, 5), Vector3.Zero);
        RenderOptions options = new(9, 9, 1, 0);

        static Scene MakeScene(float kd)
        {
            Scene scene = new() { AmbientIntensity = 0, SkyColour = Vector3.Zero };
            scene.Objects.Add(new Sphere(Vector3.Zero, 1,
                new Material(Vector3.One, Kd: kd, Ks: 0)));
            scene.DirectionalLights.Add(new DirectionalLight(Vector3.UnitZ, Vector3.One, 1));
            return scene;
        }

        int centre = 4 * 9 + 4;
        int[] dark = await new Renderer().RenderAsync(MakeScene(0), camera, options);
        int[] bright = await new Renderer().RenderAsync(MakeScene(1), camera, options);
        Assert.Equal(0, dark[centre] & 0xFFFFFF);
        Assert.True((bright[centre] & 0xFFFFFF) > 0);
    }

    [Fact]
    public void CompiledSceneSnapshotsAmbientParameters()
    {
        Scene scene = new() { AmbientIntensity = 0.8f, AmbientColour = Vector3.UnitY };
        CompiledScene compiled = scene.Compile();
        scene.AmbientIntensity = 0;
        scene.AmbientColour = Vector3.Zero;
        Assert.Equal(0.8f, compiled.AmbientIntensity);
        Assert.Equal(Vector3.UnitY, compiled.AmbientColour);
    }
}
