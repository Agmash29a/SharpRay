namespace SharpRay.WinForms;

/// <summary>
/// Reconstructed editable versions of selected scenes from World.cs (2004).
/// Geometry, camera positions and colours are derived from the historical Build_* methods.
/// Rendering is intentionally modernized; old material coefficient and camera behaviour
/// are not pixel-for-pixel identical.
/// </summary>
public static class LegacySceneCatalog
{
    public static readonly string[] Names =
    [
        "Modern demo",
        "2004 / Single sphere",
        "2004 / Four spheres",
        "2004 / Boxes",
        "2004 / Hanoi",
        "2004 / Marble materials",
        "2004 / Fractal cylinders",
        "2004 / Spheres on a torus"
    ];

    public static SceneDocument Create(string name) => name switch
    {
        "Modern demo" => SceneDocument.CreateDemo(),
        "2004 / Single sphere" => SingleSphere(),
        "2004 / Four spheres" => FourSpheres(),
        "2004 / Boxes" => Boxes(),
        "2004 / Hanoi" => Hanoi(),
        "2004 / Marble materials" => Marble(),
        "2004 / Fractal cylinders" => FractalCylinders(),
        "2004 / Spheres on a torus" => SpheresOnTorus(),
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown preset.")
    };

    private static CameraSettings Cam(float x, float y, float z,
        float targetX, float targetY, float targetZ, float fov) =>
        new() { X = x, Y = y, Z = z, TargetX = targetX,
            TargetY = targetY, TargetZ = targetZ, FieldOfView = fov };

    private static EditableLight Light(string name, float x, float y, float z, float power = 1) =>
        new() { Name = name, X = x, Y = y, Z = z, Power = power };

    private static EditableShape Sphere(string name, float x, float y, float z, float r, string colour,
        float reflectivity = 0, float transparency = 0) =>
        new() { Name = name, Shape = ShapeKind.Sphere, X = x, Y = y, Z = z, Radius = r,
            BaseColour = colour, Reflectivity = reflectivity, Transparency = transparency };

    private static EditableShape Ground(float y = 0) =>
        new() { Name = "Ground", Shape = ShapeKind.Plane, Y = y,
            BaseColour = "#BFBFBF", Reflectivity = 0.18f };

    private static SceneDocument SingleSphere() => new()
    {
        Camera = Cam(0, 0, 40, 0, 0, 0, 24),
        Lights = [Light("Left", -5, 20, 0), Light("Right", 10, 30, 10)],
        Objects = [Sphere("Original central sphere", 0, 0, 0, 5, "#FFCC99")],
        SkyColour = "#000000"
    };

    private static SceneDocument FourSpheres() => new()
    {
        Camera = Cam(10, 5.5f, 40, 5.25f, 5, 0, 24),
        Lights = [Light("Left", -5, 20, 0), Light("Right", 10, 30, 10)],
        Objects =
        [
            Sphere("Green", 0, 5, -0.5f, 3, "#00CC99"),
            Sphere("Orange", 4, 4, -6, 3, "#FF8000"),
            Sphere("Mauve", 4, 8.5f, -2, 2, "#E133FF"),
            Sphere("Blue", 10, 6, -2, 3, "#0000FF"),
            Ground(1)
        ],
        SkyColour = "#00264D"
    };

    private static SceneDocument Boxes() => new()
    {
        Camera = Cam(2.75f, 2, 10, 0.5f, 2, 0, 28),
        Lights = [Light("Main", -10, 20, 10, 1.5f)],
        Objects =
        [
            new EditableShape { Name = "Green foreground box", Shape = ShapeKind.Box,
                X = 0, Y = 2, Z = 0, SizeX = 2, SizeY = 3, SizeZ = 2, BaseColour = "#00CC4D" },
            new EditableShape { Name = "Red back box", Shape = ShapeKind.Box,
                X = 0.25f, Y = 2.75f, Z = -8, SizeX = 2.5f, SizeY = 4.5f,
                SizeZ = 8, BaseColour = "#FF0000" },
            Ground(0)
        ],
        SkyColour = "#003366"
    };

    private static SceneDocument Hanoi()
    {
        var doc = new SceneDocument
        {
            Camera = Cam(0, 10, 20, 0, 0.1f, 0, 41),
            SkyColour = "#4D667F",
            Lights = [Light("Overhead", 0, 20, 0, 1.5f),
                new EditableLight { Name = "Directional", Directional = true,
                    X = -1, Y = -1, Z = 1, Power = 1 }]
        };
        doc.Objects.Add(new EditableShape { Name = "Central cylinder", Shape = ShapeKind.Cylinder,
            SizeY = 30, BaseColour = "#1A1A99", Reflectivity = 0.7f });
        string[] colours = ["#993333", "#339933", "#333399", "#993399", "#999933"];
        float[] widths = [4, 3, 2, 1.5f, 1];
        for (int i = 0; i < colours.Length; i++)
            doc.Objects.Add(new EditableShape { Name = $"Ring {i + 1}", Shape = ShapeKind.Torus,
                Radius = 2, MinorRadius = 0.5f, Y = i, ScaleX = widths[i],
                ScaleY = 1.5f, ScaleZ = widths[i], BaseColour = colours[i], Reflectivity = 0.8f });
        EditableShape ground = Ground();
        ground.Reflectivity = 0.8f;
        doc.Objects.Add(ground);
        return doc;
    }

    private static SceneDocument Marble()
    {
        var doc = new SceneDocument
        {
            Camera = Cam(0, 5.5f, 160, 0, 2, 0, 12),
            Lights = [Light("Top left", -5, 20, 0), Light("Bottom right", 10, -30, 10)],
            SkyColour = "#1A1A4D"
        };
        string? rampPath = FindOriginalAsset("Ramps", "MarbleRamp.bmp");
        var names = new[] { "Marble ramp", "Linear noise", "fBm", "Procedural marble" };
        for (int i = 0; i < 4; i++)
            doc.Objects.Add(new EditableShape
            {
                Name = names[i], Shape = ShapeKind.Sphere, X = -10.5f + i * 7,
                Y = 5, Radius = 3, BaseColour = "#8BB6EC", AccentColour = "#23374A",
                Texture = i switch
                {
                    0 => rampPath is null ? TextureKind.Marble : TextureKind.MarbleRamp,
                    1 => TextureKind.Noise,
                    2 => TextureKind.FBm,
                    _ => TextureKind.Marble
                },
                BitmapPath = rampPath ?? "",
                Frequency = i == 1 ? 20 : 5,
                Octaves = 6,
                Interpolation = i == 1 ? SharpRay.Core.NoiseInterpolation.Linear :
                    SharpRay.Core.NoiseInterpolation.Cubic
            });
        EditableShape ground = Ground(1);
        ground.Reflectivity = 0.8f;
        doc.Objects.Add(ground);
        return doc;
    }

    private static SceneDocument FractalCylinders()
    {
        var doc = new SceneDocument
        {
            Camera = Cam(0, 20, -10, 0, 0.1f, 0, 33),
            Lights =
            [
                Light("Forward", 0, 0, 6, 2.5f),
                new EditableLight { Name = "Directional", Directional = true,
                    X = -1, Y = -1, Z = 1, Power = 1 }
            ]
        };
        for (int y = -7; y <= 7; y += 2)
            doc.Objects.Add(new EditableShape { Name = $"Glass slice {y}", Shape = ShapeKind.Box,
                Y = y, SizeX = 8, SizeY = 0.2f, SizeZ = 4,
                BaseColour = "#7D4C88", Transparency = 0.8f,
                Reflectivity = 0.2f });
        string[] filenames = ["JSCool.bmp", "Set_Zoom2_150its.bmp", "JS_Zoom1.bmp"];
        for (int i = 0; i < 3; i++)
        {
            string? path = FindOriginalAsset("Fractals", filenames[i]);
            doc.Objects.Add(new EditableShape
            {
                Name = $"Fractal cylinder {i + 1}", Shape = ShapeKind.Cylinder,
                X = (i - 1) * 2.5f, Radius = 1, SizeY = 8,
                BitmapPath = path ?? "",
                Texture = path is null ? TextureKind.FBm : TextureKind.Bitmap,
                Projection = SharpRay.Core.TextureProjection.CylindricalLegacy,
                BaseColour = "#6286CE", AccentColour = "#111B42"
            });
        }
        return doc;
    }

    private static SceneDocument SpheresOnTorus()
    {
        var doc = new SceneDocument
        {
            Camera = Cam(40, 0, 40, 0, -3, 0, 24),
            SkyColour = "#3380CC",
            Lights =
            [
                Light("Left", -5, 20, 0),
                Light("Right", 10, 30, 10),
                new EditableLight { Name = "Directional", Directional = true, X = -1,
                    Y = -1, Z = 1, Power = 1 }
            ]
        };
        doc.Objects.Add(Ground(1));
        // Same major/minor parametrization and 11.25 degree spacing as Build_TOSTorus.
        for (int ti = 0; ti <= 32; ti++)
        for (int pi = 0; pi <= 32; pi++)
        {
            float theta = ti * 11.25f * MathF.PI / 180f;
            float phi = pi * 11.25f * MathF.PI / 180f;
            float ring = 3 + MathF.Cos(phi);
            doc.Objects.Add(Sphere($"Torus sample {ti},{pi}",
                4 * ring * MathF.Cos(theta), 2 * ring * MathF.Sin(theta),
                2 * MathF.Sin(phi), 1.5f,
                $"#{128:X2}{Math.Clamp((int)(ti * 255f / 43f), 0, 255):X2}40"));
        }
        return doc;
    }

    private static string? FindOriginalAsset(string folder, string file)
    {
        foreach (string origin in new[] { AppContext.BaseDirectory, Directory.GetCurrentDirectory() })
        {
            DirectoryInfo? dir = new(origin);
            for (int depth = 0; depth < 9 && dir is not null; depth++, dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, "RayTracer", "Gfx", folder, file);
                if (File.Exists(candidate)) return candidate;
            }
        }
        return null;
    }
}
