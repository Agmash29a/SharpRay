using System.Numerics;

namespace SharpRay.Core;

public readonly record struct Ray(Vector3 Origin, Vector3 Direction)
{
    public Vector3 At(float t) => Origin + t * Direction;
}

public readonly record struct Material(
    Vector3 Colour,
    float Reflectivity = 0,
    float Shininess = 48,
    ITexture? Texture = null,
    float Transparency = 0,
    float RefractiveIndex = 1.5f,
    float Ka = 1f,
    float Kd = 1f,
    float Ks = 0.4f);

public readonly record struct Hit(float Distance, Vector3 Position, Vector3 Normal, Material Material)
{
    public Vector3 LocalPoint { get; init; }
    public Vector2 UV { get; init; }
    public bool FrontFace { get; init; }
    public TextureContext TextureContext => new(Position, LocalPoint, UV);
}

public interface ISceneObject
{
    bool TryHit(in Ray ray, float minT, float maxT, out Hit hit);
}

public interface IBoundedSceneObject : ISceneObject
{
    Bounds BoundingBox { get; }
}

public readonly record struct Bounds(Vector3 Min, Vector3 Max)
{
    public Vector3 Centre => (Min + Max) * 0.5f;
    public static Bounds Union(Bounds a, Bounds b) =>
        new(Vector3.Min(a.Min, b.Min), Vector3.Max(a.Max, b.Max));

    public bool TryInterval(in Ray ray, float minT, float maxT, out float near, out float far)
    {
        near = minT;
        far = maxT;
        for (int axis = 0; axis < 3; axis++)
        {
            float origin = axis == 0 ? ray.Origin.X : axis == 1 ? ray.Origin.Y : ray.Origin.Z;
            float dir = axis == 0 ? ray.Direction.X : axis == 1 ? ray.Direction.Y : ray.Direction.Z;
            float low = axis == 0 ? Min.X : axis == 1 ? Min.Y : Min.Z;
            float high = axis == 0 ? Max.X : axis == 1 ? Max.Y : Max.Z;
            if (MathF.Abs(dir) < 1e-12f)
            {
                if (origin < low || origin > high) return false;
                continue;
            }
            float a = (low - origin) / dir;
            float b = (high - origin) / dir;
            if (a > b) (a, b) = (b, a);
            near = MathF.Max(near, a);
            far = MathF.Min(far, b);
            if (far < near) return false;
        }
        return true;
    }

    public bool Intersects(in Ray ray, float minT, float maxT) =>
        TryInterval(ray, minT, maxT, out _, out _);
}

public sealed record Sphere(Vector3 Centre, float Radius, Material Material) : IBoundedSceneObject
{
    public Bounds BoundingBox => new(Centre - new Vector3(Radius), Centre + new Vector3(Radius));

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        if (Radius <= 0) return false;
        Vector3 offset = ray.Origin - Centre;
        float a = Vector3.Dot(ray.Direction, ray.Direction);
        if (a < 1e-12f) return false;
        float b = Vector3.Dot(offset, ray.Direction);
        float c = offset.LengthSquared() - Radius * Radius;
        float d = b * b - a * c;
        if (d < 0) return false;
        float root = MathF.Sqrt(d);
        float t = (-b - root) / a;
        if (t < minT || t > maxT)
        {
            t = (-b + root) / a;
            if (t < minT || t > maxT) return false;
        }
        Vector3 point = ray.At(t);
        Vector3 outward = Vector3.Normalize((point - Centre) / Radius);
        bool frontFace = Vector3.Dot(ray.Direction, outward) < 0;
        float u = 0.5f + MathF.Atan2(outward.Z, outward.X) / (2 * MathF.PI);
        float v = 0.5f - MathF.Asin(Math.Clamp(outward.Y, -1, 1)) / MathF.PI;
        hit = new Hit(t, point, frontFace ? outward : -outward, Material)
        {
            LocalPoint = outward,
            FrontFace = frontFace,
            UV = new Vector2(u, v)
        };
        return true;
    }
}

public sealed record Plane(Vector3 Point, Vector3 Normal, Material Material) : ISceneObject
{
    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        if (Normal.LengthSquared() < 1e-12f) return false;
        Vector3 normal = Vector3.Normalize(Normal);
        float denom = Vector3.Dot(normal, ray.Direction);
        if (MathF.Abs(denom) < 1e-7f) return false;
        float t = Vector3.Dot(Point - ray.Origin, normal) / denom;
        if (t < minT || t > maxT) return false;
        Vector3 point = ray.At(t);
        bool frontFace = denom < 0;
        hit = new Hit(t, point, frontFace ? normal : -normal, Material)
        {
            FrontFace = frontFace,
            LocalPoint = point - Point,
            UV = new Vector2(point.X, point.Z)
        };
        return true;
    }
}

public sealed record AxisBox(Vector3 Min, Vector3 Max, Material Material) : IBoundedSceneObject
{
    public Bounds BoundingBox => new(Vector3.Min(Min, Max), Vector3.Max(Min, Max));

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        Bounds b = BoundingBox;
        // Keep the unclipped entry/exit distances to handle rays starting inside.
        if (!b.TryInterval(ray, float.NegativeInfinity, float.PositiveInfinity, out float enter, out float exit))
            return false;
        float t = enter >= minT ? enter : exit;
        if (t < minT || t > maxT || !float.IsFinite(t)) return false;
        Vector3 point = ray.At(t);
        float distance = float.PositiveInfinity;
        Vector3 normal = Vector3.UnitY;
        void Check(float d, Vector3 n) { if (d < distance) { distance = d; normal = n; } }
        Check(MathF.Abs(point.X - b.Min.X), -Vector3.UnitX);
        Check(MathF.Abs(point.X - b.Max.X), Vector3.UnitX);
        Check(MathF.Abs(point.Y - b.Min.Y), -Vector3.UnitY);
        Check(MathF.Abs(point.Y - b.Max.Y), Vector3.UnitY);
        Check(MathF.Abs(point.Z - b.Min.Z), -Vector3.UnitZ);
        Check(MathF.Abs(point.Z - b.Max.Z), Vector3.UnitZ);
        bool frontFace = Vector3.Dot(ray.Direction, normal) < 0;
        hit = new Hit(t, point, frontFace ? normal : -normal, Material)
        {
            FrontFace = frontFace, LocalPoint = point - b.Centre,
            UV = new Vector2(point.X, point.Z)
        };
        return true;
    }
}

public sealed record Triangle(Vector3 A, Vector3 B, Vector3 C, Material Material) : IBoundedSceneObject
{
    public Bounds BoundingBox
    {
        get
        {
            Vector3 min = Vector3.Min(A, Vector3.Min(B, C)) - new Vector3(0.0001f);
            Vector3 max = Vector3.Max(A, Vector3.Max(B, C)) + new Vector3(0.0001f);
            return new(min, max);
        }
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        Vector3 e1 = B - A, e2 = C - A;
        Vector3 p = Vector3.Cross(ray.Direction, e2);
        float det = Vector3.Dot(e1, p);
        if (MathF.Abs(det) < 1e-8f) return false;
        float invDet = 1f / det;
        Vector3 s = ray.Origin - A;
        float u = Vector3.Dot(s, p) * invDet;
        if (u < 0 || u > 1) return false;
        Vector3 q = Vector3.Cross(s, e1);
        float v = Vector3.Dot(ray.Direction, q) * invDet;
        if (v < 0 || u + v > 1) return false;
        float t = Vector3.Dot(e2, q) * invDet;
        if (t < minT || t > maxT) return false;
        Vector3 rawNormal = Vector3.Cross(e1, e2);
        if (rawNormal.LengthSquared() < 1e-12f) return false;
        rawNormal = Vector3.Normalize(rawNormal);
        bool frontFace = Vector3.Dot(ray.Direction, rawNormal) < 0;
        Vector3 point = ray.At(t);
        hit = new Hit(t, point, frontFace ? rawNormal : -rawNormal, Material)
        {
            FrontFace = frontFace, LocalPoint = point - A, UV = new Vector2(u, v)
        };
        return true;
    }
}

public sealed record PointLight(Vector3 Position, Vector3 Colour, float Power);
public sealed record DirectionalLight(Vector3 DirectionToLight, Vector3 Colour, float Power);

public sealed class Scene
{
    public List<ISceneObject> Objects { get; } = [];
    public List<PointLight> Lights { get; } = [];
    public List<DirectionalLight> DirectionalLights { get; } = [];
    public float AmbientIntensity { get; set; } = 0.055f;
    public Vector3 AmbientColour { get; set; } = Vector3.One;
    public Vector3 SkyColour { get; set; } = new(0.035f, 0.055f, 0.10f);

    // Reference path, useful for testing the BVH against the original linear traversal.
    public bool TryHit(in Ray ray, float minT, float maxT, out Hit nearest)
    {
        nearest = default;
        bool found = false;
        foreach (ISceneObject obj in Objects)
        {
            if (!obj.TryHit(ray, minT, maxT, out Hit hit)) continue;
            if (!found || hit.Distance < nearest.Distance)
            {
                maxT = hit.Distance;
                nearest = hit;
                found = true;
            }
        }
        return found;
    }

    public CompiledScene Compile() => new(this);
}
