using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using SharpRay.Core;

namespace SharpRay.WinForms;

public enum ShapeKind { Sphere, Plane, Box, Triangle }
public enum TextureKind { Solid, Checker, Noise, FBm, Turbulence, Marble, Bitmap }

public sealed class EditableShape
{
    [Category("Identity")] public string Name { get; set; } = "Sphere";
    [Category("Geometry")] public ShapeKind Shape { get; set; } = ShapeKind.Sphere;
    [Category("Geometry")] public float X { get; set; }
    [Category("Geometry")] public float Y { get; set; }
    [Category("Geometry")] public float Z { get; set; }
    [Category("Geometry")] public float Radius { get; set; } = 1;
    [Category("Geometry")] public float SizeX { get; set; } = 1;
    [Category("Geometry")] public float SizeY { get; set; } = 1;
    [Category("Geometry")] public float SizeZ { get; set; } = 1;

    [Category("Material")] public string BaseColour { get; set; } = "#D46C3C";
    [Category("Material")] public string AccentColour { get; set; } = "#27364F";
    [Category("Material")] public float Reflectivity { get; set; } = 0.1f;
    [Category("Material")] public float Transparency { get; set; }
    [Category("Material")] public float RefractiveIndex { get; set; } = 1.5f;
    [Category("Material")] public float Shininess { get; set; } = 48f;

    [Category("Texture")] public TextureKind Texture { get; set; } = TextureKind.Solid;
    [Category("Texture")] public float Frequency { get; set; } = 2f;
    [Category("Texture")] public int Octaves { get; set; } = 5;
    [Category("Texture")] public NoiseInterpolation Interpolation { get; set; } = NoiseInterpolation.Cubic;
    [Category("Texture")] public TextureProjection Projection { get; set; } = TextureProjection.UV;
    [Category("Texture")] public string BitmapPath { get; set; } = "";

    public override string ToString() => $"{Name} ({Shape})";

    internal ISceneObject ToGeometry()
    {
        Vector3 centre = new(X, Y, Z);
        Vector3 size = new(MathF.Max(0.001f, MathF.Abs(SizeX)),
            MathF.Max(0.001f, MathF.Abs(SizeY)), MathF.Max(0.001f, MathF.Abs(SizeZ)));
        Vector3 primary = SceneDocument.ParseColour(BaseColour);
        Vector3 accent = SceneDocument.ParseColour(AccentColour);
        ITexture? texture = Texture switch
        {
            TextureKind.Checker => new CheckerTexture(primary, accent, Frequency),
            TextureKind.Noise => new ProceduralTexture(ProceduralKind.Noise, primary, accent,
                Frequency, Octaves, Interpolation: Interpolation),
            TextureKind.FBm => new ProceduralTexture(ProceduralKind.FBm, primary, accent,
                Frequency, Octaves, Interpolation: Interpolation),
            TextureKind.Turbulence => new ProceduralTexture(ProceduralKind.Turbulence, primary, accent,
                Frequency, Octaves, Interpolation: Interpolation),
            TextureKind.Marble => new ProceduralTexture(ProceduralKind.Marble, primary, accent,
                Frequency, Octaves, Interpolation: Interpolation),
            TextureKind.Bitmap when !string.IsNullOrWhiteSpace(BitmapPath) =>
                ImageTexture.FromBmp(BitmapPath, Projection),
            TextureKind.Bitmap => throw new InvalidOperationException($"Select a BMP file for '{Name}'."),
            _ => null
        };
        Material material = new(primary, Math.Clamp(Reflectivity, 0, 1),
            Math.Clamp(Shininess, 1, 500), texture, Math.Clamp(Transparency, 0, 1),
            Math.Clamp(RefractiveIndex, 1.001f, 4));
        return Shape switch
        {
            ShapeKind.Sphere => new Sphere(centre, MathF.Max(0.001f, Radius), material),
            ShapeKind.Plane => new Plane(centre, Vector3.UnitY, material),
            ShapeKind.Box => new AxisBox(centre - size * 0.5f, centre + size * 0.5f, material),
            ShapeKind.Triangle => new Triangle(centre,
                centre + new Vector3(size.X, 0, 0),
                centre + new Vector3(0, size.Y, 0), material),
            _ => throw new NotSupportedException($"Unsupported shape: {Shape}")
        };
    }
}

public sealed class EditableLight
{
    public string Name { get; set; } = "Light";
    public float X { get; set; } = -4;
    public float Y { get; set; } = 7;
    public float Z { get; set; } = 5;
    public string Colour { get; set; } = "#FFFFFF";
    public float Power { get; set; } = 1.5f;
    public override string ToString() => Name;
    internal PointLight ToLight() => new(new Vector3(X, Y, Z),
        SceneDocument.ParseColour(Colour), MathF.Max(0, Power));
}

public sealed class CameraSettings
{
    public float X { get; set; }
    public float Y { get; set; } = 2.4f;
    public float Z { get; set; } = 7.5f;
    public float TargetX { get; set; }
    public float TargetY { get; set; } = -0.1f;
    public float TargetZ { get; set; }
    public float FieldOfView { get; set; } = 55;
    internal Camera ToCamera() => new(new(X, Y, Z),
        new(TargetX, TargetY, TargetZ), FieldOfView);
}

public sealed class SceneDocument
{
    public List<EditableShape> Objects { get; set; } = [];
    public List<EditableLight> Lights { get; set; } = [];
    public CameraSettings Camera { get; set; } = new();
    public string SkyColour { get; set; } = "#33435A";

    public static SceneDocument CreateDemo() => new()
    {
        Objects =
        [
            new EditableShape { Name = "Checker floor", Shape = ShapeKind.Plane, Y = -1,
                BaseColour = "#C9D4E5", AccentColour = "#303947", Texture = TextureKind.Checker,
                Frequency = 1, Reflectivity = 0.14f },
            new EditableShape { Name = "Marble", X = -1.45f,
                BaseColour = "#194777", AccentColour = "#FFFFFF", Texture = TextureKind.Marble,
                Frequency = 3.2f, Reflectivity = 0.2f },
            new EditableShape { Name = "fBm", X = 0.55f, Y = -0.1f, Z = -1.4f, Radius = 0.9f,
                Texture = TextureKind.FBm, BaseColour = "#17205C",
                AccentColour = "#69BCF7", Frequency = 4 },
            new EditableShape { Name = "Gold mirror", X = 1.45f, Y = -0.45f, Z = 1,
                Radius = 0.55f, BaseColour = "#D9BF3A", Reflectivity = 0.8f },
            new EditableShape { Name = "Glass", X = 0.15f, Y = -0.62f, Z = 1.2f,
                Radius = 0.37f, BaseColour = "#FFFFFF", Transparency = 0.92f },
            new EditableShape { Name = "Box", Shape = ShapeKind.Box, X = -2.6f,
                Y = -0.45f, Z = -3.4f, SizeX = 0.8f, SizeY = 1.1f, SizeZ = 1.0f }
        ],
        Lights =
        [
            new EditableLight { Name = "Key light" },
            new EditableLight { Name = "Blue fill", X = 5, Y = 4, Z = -3,
                Colour = "#9FC5FF", Power = 0.65f }
        ]
    };

    public (Scene Scene, Camera Camera) Build()
    {
        Scene scene = new() { SkyColour = ParseColour(SkyColour) * 0.18f };
        foreach (EditableShape obj in Objects)
            scene.Objects.Add(obj.ToGeometry());
        foreach (EditableLight light in Lights)
            scene.Lights.Add(light.ToLight());
        return (scene, Camera.ToCamera());
    }

    internal static Vector3 ParseColour(string html)
    {
        Color c = ColorTranslator.FromHtml(html);
        static float Linear(byte s) => MathF.Pow(s / 255f, 2.2f);
        return new Vector3(Linear(c.R), Linear(c.G), Linear(c.B));
    }

    public static SceneDocument Load(string path)
    {
        SceneDocument doc = JsonSerializer.Deserialize<SceneDocument>(File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Scene file is empty.");
        doc.Objects ??= [];
        doc.Lights ??= [];
        doc.Camera ??= new();
        return doc;
    }

    public void Save(string path) => File.WriteAllText(path,
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
}
