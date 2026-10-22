using System.Numerics;

namespace SharpRay.Core;

public sealed record RenderOptions(int Width = 800, int Height = 500, int Samples = 4, int MaxBounces = 4)
{
    public void Validate()
    {
        if (Width is < 1 or > 8000 || Height is < 1 or > 8000)
            throw new ArgumentOutOfRangeException(nameof(Width));
        if (Samples is < 1 or > 128) throw new ArgumentOutOfRangeException(nameof(Samples));
        if (MaxBounces is < 0 or > 24) throw new ArgumentOutOfRangeException(nameof(MaxBounces));
        checked { _ = Width * Height; }
    }
}

public sealed class Camera
{
    private readonly Vector3 position;
    private readonly Vector3 forward, right, up;
    private readonly float scale;
    public Camera(Vector3 position, Vector3 target, float verticalFieldOfView = 55f)
    {
        if (verticalFieldOfView is <= 1 or >= 170) throw new ArgumentOutOfRangeException(nameof(verticalFieldOfView));
        this.position = position;
        Vector3 look = target - position;
        if (look.LengthSquared() < 1e-8f) look = -Vector3.UnitZ;
        forward = Vector3.Normalize(look);
        Vector3 worldUp = MathF.Abs(Vector3.Dot(forward, Vector3.UnitY)) > 0.999f ? Vector3.UnitZ : Vector3.UnitY;
        right = Vector3.Normalize(Vector3.Cross(forward, worldUp));
        up = Vector3.Normalize(Vector3.Cross(right, forward));
        scale = MathF.Tan(verticalFieldOfView * MathF.PI / 360f);
    }
    /// <summary>World position to normalized viewport coordinates, matching GetRay.</summary>
    public bool TryProject(Vector3 world, float aspect, out Vector2 uv)
    {
        Vector3 offset = world - position;
        float depth = Vector3.Dot(offset, forward);
        uv = default;
        if (depth <= 0.0001f || aspect <= 0) return false;
        float x = Vector3.Dot(offset, right) / (depth * aspect * scale);
        float y = Vector3.Dot(offset, up) / (depth * scale);
        uv = new Vector2((1 + x) * 0.5f, (1 - y) * 0.5f);
        return float.IsFinite(uv.X) && float.IsFinite(uv.Y);
    }

    public Ray GetRay(float u, float v, float aspect)
    {
        Vector3 direction = forward + right * ((2 * u - 1) * aspect * scale) + up * ((1 - 2 * v) * scale);
        return new Ray(position, Vector3.Normalize(direction));
    }
}

public sealed class Renderer
{
    private const float Epsilon = 0.0008f;

    public Task<int[]> RenderAsync(Scene scene, Camera camera, RenderOptions options,
        IProgress<int>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ArgumentNullException.ThrowIfNull(camera);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        // BVH construction is done before the worker starts. The scene is then immutable to all threads.
        CompiledScene compiled = scene.Compile();
        return Task.Run(() =>
        {
            int[] pixels = new int[checked(options.Width * options.Height)];
            int completedRows = 0;
            Parallel.For(0, options.Height, new ParallelOptions { CancellationToken = cancellationToken }, y =>
            {
                for (int x = 0; x < options.Width; x++)
                {
                    Vector3 sum = Vector3.Zero;
                    for (int sample = 0; sample < options.Samples; sample++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        // Deterministic, distributed sample offsets, independent of worker scheduling.
                        float dx = (sample + 0.5f) / options.Samples;
                        float dy = ((sample * 0.61803398875f) % 1f + 0.5f / options.Samples) % 1f;
                        Ray ray = camera.GetRay((x + dx) / options.Width,
                            (y + dy) / options.Height, (float)options.Width / options.Height);
                        sum += Trace(compiled, ray, options.MaxBounces);
                    }
                    pixels[y * options.Width + x] = Pack(sum / options.Samples);
                }
                progress?.Report(Interlocked.Increment(ref completedRows) * 100 / options.Height);
            });
            return pixels;
        }, cancellationToken);
    }

    private static Vector3 Trace(CompiledScene scene, Ray ray, int remaining)
    {
        if (!scene.TryHit(ray, Epsilon, float.MaxValue, out Hit hit)) return scene.SkyColour;
        Material mat = hit.Material;
        Vector3 surface = mat.Texture?.Sample(hit.TextureContext) ?? mat.Colour;
        surface = Vector3.Max(surface, Vector3.Zero);
        Vector3 direct = surface * 0.055f;
        Vector3 view = -ray.Direction;

        foreach (PointLight light in scene.Lights)
        {
            Vector3 toLight = light.Position - hit.Position;
            float d2 = toLight.LengthSquared();
            if (d2 < Epsilon * Epsilon) continue;
            float distance = MathF.Sqrt(d2);
            Vector3 l = toLight / distance;
            float ndotl = Vector3.Dot(hit.Normal, l);
            if (ndotl <= 0) continue;
            if (scene.TryHit(new Ray(hit.Position + hit.Normal * Epsilon, l), Epsilon,
                distance - Epsilon, out _)) continue;
            Vector3 reflectedLight = Vector3.Reflect(-l, hit.Normal);
            float specular = MathF.Pow(MathF.Max(0, Vector3.Dot(view, reflectedLight)),
                Math.Clamp(mat.Shininess, 1, 500));
            float attenuated = MathF.Max(0, light.Power) / (1 + d2 * 0.04f);
            direct += (surface * ndotl + new Vector3(specular * 0.4f)) *
                light.Colour * attenuated;
        }

        float reflectivity = Math.Clamp(mat.Reflectivity, 0f, 1f);
        float transparency = Math.Clamp(mat.Transparency, 0f, 1f);
        if (remaining <= 0 || (reflectivity <= 0 && transparency <= 0))
            return direct * (1 - transparency);

        float ior = Math.Clamp(mat.RefractiveIndex, 1.001f, 4f);
        float eta = hit.FrontFace ? 1f / ior : ior;
        float cosine = Math.Clamp(Vector3.Dot(-ray.Direction, hit.Normal), 0, 1);
        float r0 = MathF.Pow((1 - ior) / (1 + ior), 2);
        float fresnel = r0 + (1 - r0) * MathF.Pow(1 - cosine, 5);
        float sin2 = MathF.Max(0f, 1f - cosine * cosine);
        bool totalInternalReflection = eta * eta * sin2 > 1f;

        // Separate glass and metallic-mirror contributions while keeping weights <= 1.
        float mirrorWeight = reflectivity + (1 - reflectivity) *
            transparency * (totalInternalReflection ? 1 : fresnel);
        float refractionWeight = totalInternalReflection ? 0 :
            (1 - reflectivity) * transparency * (1 - fresnel);
        float diffuseWeight = (1 - reflectivity) * (1 - transparency);

        Vector3 colour = direct * diffuseWeight;
        if (mirrorWeight > 0)
        {
            Vector3 reflected = Vector3.Reflect(ray.Direction, hit.Normal);
            Ray bounce = new(hit.Position + hit.Normal * Epsilon, Vector3.Normalize(reflected));
            colour += Trace(scene, bounce, remaining - 1) * mirrorWeight;
        }
        if (refractionWeight > 0)
        {
            Vector3 refracted = eta * ray.Direction +
                (eta * cosine - MathF.Sqrt(MathF.Max(0, 1 - eta * eta * sin2))) * hit.Normal;
            Ray transmitted = new(hit.Position - hit.Normal * Epsilon, Vector3.Normalize(refracted));
            colour += Trace(scene, transmitted, remaining - 1) * refractionWeight;
        }
        return colour;
    }

    private static int Pack(Vector3 linear)
    {
        static int Channel(float v)
        {
            if (!float.IsFinite(v)) return 0;
            return (int)(255 * MathF.Pow(Math.Clamp(v, 0, 1), 1f / 2.2f) + 0.5f);
        }
        return unchecked((int)0xFF000000 | (Channel(linear.X) << 16) |
            (Channel(linear.Y) << 8) | Channel(linear.Z));
    }
}

public static class DemoScene
{
    public static (Scene Scene, Camera Camera) Create()
    {
        var scene = new Scene();
        scene.Objects.Add(new Plane(new Vector3(0, -1, 0), Vector3.UnitY,
            new Material(Vector3.One, 0.12f, Texture: new CheckerTexture(
                new Vector3(0.82f, 0.85f, 0.9f), new Vector3(0.12f, 0.14f, 0.18f)))));
        scene.Objects.Add(new Sphere(new Vector3(-1.45f, 0, 0), 1,
            new Material(Vector3.One, 0.23f, Texture: new ProceduralTexture(
                ProceduralKind.Marble, new Vector3(0.15f, 0.3f, 0.58f), Vector3.One, 3.2f))));
        scene.Objects.Add(new Sphere(new Vector3(0.55f, -0.1f, -1.4f), 0.9f,
            new Material(Vector3.One, 0.1f, Texture: new ProceduralTexture(
                ProceduralKind.FBm, new Vector3(0.14f, 0.12f, 0.38f),
                new Vector3(0.4f, 0.8f, 0.95f), 4))));
        scene.Objects.Add(new Sphere(new Vector3(1.45f, -0.45f, 1.0f), 0.55f,
            new Material(new Vector3(0.9f, 0.84f, 0.18f), Reflectivity: 0.8f)));
        scene.Objects.Add(new Sphere(new Vector3(0.15f, -0.62f, 1.2f), 0.37f,
            new Material(Vector3.One, Transparency: 0.92f, RefractiveIndex: 1.5f)));
        scene.Objects.Add(new AxisBox(new Vector3(-3, -1, -4), new Vector3(-2.3f, 0.1f, -3),
            new Material(new Vector3(0.86f, 0.37f, 0.22f))));
        scene.Lights.Add(new PointLight(new Vector3(-4, 7, 5), Vector3.One, 1.55f));
        scene.Lights.Add(new PointLight(new Vector3(5, 4, -3), new Vector3(0.5f, 0.65f, 1), 0.65f));
        return (scene, new Camera(new Vector3(0, 2.4f, 7.5f), new Vector3(0, -0.1f, 0)));
    }
}
