using System.ComponentModel;
using System.Drawing;
using System.Numerics;
using System.Text.Json;
using SharpRay.Core;

namespace SharpRay.WinForms;

public enum ShapeKind { Sphere, Plane, Box, Triangle, Disc, Cylinder, Torus, Mesh }
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
    [Category("Geometry")] public float MinorRadius { get; set; } = 0.3f;
    [Category("Geometry")] public string MeshPath { get; set; } = "";
    [Category("Transform")] public float RotateX { get; set; }
    [Category("Transform")] public float RotateY { get; set; }
    [Category("Transform")] public float RotateZ { get; set; }
    [Category("Transform")] public float ScaleX { get; set; } = 1;
    [Category("Transform")] public float ScaleY { get; set; } = 1;
    [Category("Transform")] public float ScaleZ { get; set; } = 1;

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
        Matrix4x4 rotation = Matrix4x4.CreateFromYawPitchRoll(
            MathF.PI * RotateY / 180f, MathF.PI * RotateX / 180f, MathF.PI * RotateZ / 180f);
        if (Shape == ShapeKind.Plane)
            return new SharpRay.Core.Plane(centre, Vector3.TransformNormal(Vector3.UnitY, rotation), material);
        IBoundedSceneObject primitive = Shape switch
        {
            ShapeKind.Sphere => new Sphere(Vector3.Zero, MathF.Max(0.001f, Radius), material),
            ShapeKind.Box => new AxisBox(-size * 0.5f, size * 0.5f, material),
            ShapeKind.Triangle => new Triangle(Vector3.Zero, new Vector3(size.X, 0, 0),
                new Vector3(0, size.Y, 0), material),
            ShapeKind.Disc => new Disc(Vector3.Zero, Vector3.UnitY, MathF.Max(0.001f, Radius), material),
            ShapeKind.Cylinder => new OpenCylinder(Vector3.Zero, MathF.Max(0.001f, Radius),
                MathF.Max(0.001f, SizeY), material),
            ShapeKind.Torus => new Torus(Vector3.Zero, MathF.Max(0.001f, Radius),
                Math.Clamp(MinorRadius, 0.001f, MathF.Max(0.001f, Radius)), material),
            ShapeKind.Mesh when !string.IsNullOrWhiteSpace(MeshPath) =>
                MeshImporter.Load(MeshPath, material),
            ShapeKind.Mesh => throw new InvalidOperationException($"Choose a mesh file for '{Name}'."),
            _ => throw new NotSupportedException($"Unsupported shape: {Shape}")
        };
        Matrix4x4 transform = Matrix4x4.CreateScale(
            MathF.Max(0.001f, MathF.Abs(ScaleX)),
            MathF.Max(0.001f, MathF.Abs(ScaleY)),
            MathF.Max(0.001f, MathF.Abs(ScaleZ))) *
            rotation * Matrix4x4.CreateTranslation(centre);
        return new TransformedObject(primitive, transform);
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
