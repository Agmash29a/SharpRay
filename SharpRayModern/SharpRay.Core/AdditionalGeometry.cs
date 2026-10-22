using System.Numerics;

namespace SharpRay.Core;

/// <summary>Finite horizontal disc, centered at Centre and oriented by Normal.</summary>
public sealed record Disc(Vector3 Centre, Vector3 Normal, float Radius, Material Material)
    : IBoundedSceneObject
{
    public Bounds BoundingBox
    {
        get
        {
            float radius = MathF.Max(0, Radius);
            Vector3 n = Normal.LengthSquared() > 1e-12f ? Vector3.Normalize(Normal) : Vector3.UnitY;
            Vector3 e = new(
                radius * MathF.Sqrt(MathF.Max(0, 1 - n.X * n.X)) + 0.0001f,
                radius * MathF.Sqrt(MathF.Max(0, 1 - n.Y * n.Y)) + 0.0001f,
                radius * MathF.Sqrt(MathF.Max(0, 1 - n.Z * n.Z)) + 0.0001f);
            return new(Centre - e, Centre + e);
        }
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        if (Radius <= 0 || Normal.LengthSquared() < 1e-12f) return false;
        Vector3 n = Vector3.Normalize(Normal);
        float denom = Vector3.Dot(ray.Direction, n);
        if (MathF.Abs(denom) < 1e-8f) return false;
        float t = Vector3.Dot(Centre - ray.Origin, n) / denom;
        if (t < minT || t > maxT) return false;
        Vector3 p = ray.At(t);
        if (Vector3.DistanceSquared(p, Centre) > Radius * Radius) return false;
        bool front = denom < 0;
        hit = new Hit(t, p, front ? n : -n, Material)
        {
            LocalPoint = p - Centre, UV = new Vector2(0.5f + (p.X - Centre.X) / (2 * Radius),
                0.5f + (p.Z - Centre.Z) / (2 * Radius)), FrontFace = front
        };
        return true;
    }
}

/// <summary>Open Y-axis cylinder, matching the historical OpenCylinder primitive.</summary>
public sealed record OpenCylinder(Vector3 Centre, float Radius, float Height, Material Material)
    : IBoundedSceneObject
{
    public Bounds BoundingBox => new(
        Centre - new Vector3(Radius, Height * 0.5f, Radius),
        Centre + new Vector3(Radius, Height * 0.5f, Radius));

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        if (Radius <= 0 || Height <= 0) return false;
        Vector3 o = ray.Origin - Centre;
        Vector3 d = ray.Direction;
        float a = d.X * d.X + d.Z * d.Z;
        if (a < 1e-12f) return false;
        float halfB = o.X * d.X + o.Z * d.Z;
        float c = o.X * o.X + o.Z * o.Z - Radius * Radius;
        float discriminant = halfB * halfB - a * c;
        if (discriminant < 0) return false;
        float root = MathF.Sqrt(discriminant);
        foreach (float t in new[] { (-halfB - root) / a, (-halfB + root) / a })
        {
            float y = o.Y + t * d.Y;
            if (t < minT || t > maxT || MathF.Abs(y) > Height * 0.5f) continue;
            Vector3 p = ray.At(t), local = p - Centre;
            Vector3 n = Vector3.Normalize(new Vector3(local.X, 0, local.Z));
            bool front = Vector3.Dot(ray.Direction, n) < 0;
            hit = new Hit(t, p, front ? n : -n, Material)
            {
                LocalPoint = local,
                UV = new Vector2(0.5f + MathF.Atan2(local.Z, local.X) / (2 * MathF.PI),
                    0.5f + local.Y / Height),
                FrontFace = front
            };
            return true;
        }
        return false;
    }
}

/// <summary>Quartic Y-axis torus, matching the original GENERIC_Torus equation.</summary>
public sealed record Torus(Vector3 Centre, float MajorRadius, float MinorRadius, Material Material)
    : IBoundedSceneObject
{
    public Bounds BoundingBox
    {
        get
        {
            float r = MajorRadius + MinorRadius;
            Vector3 e = new(r, MinorRadius, r);
            return new(Centre - e, Centre + e);
        }
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        if (MajorRadius <= 0 || MinorRadius <= 0) return false;
        Vector3 o = ray.Origin - Centre, d = ray.Direction;
        double major2 = (double)MajorRadius * MajorRadius;
        double minor2 = (double)MinorRadius * MinorRadius;
        double d2 = Vector3.Dot(d, d);
        double od = Vector3.Dot(o, d);
        double k = Vector3.Dot(o, o) - major2 - minor2;
        // (|p|^2 - a^2 - b^2)^2 + 4*a^2*y^2 - 4*a^2*b^2 == 0
        double[] c =
        [
            k * k + 4 * major2 * ((double)o.Y * o.Y - minor2),
            4 * k * od + 8 * major2 * o.Y * d.Y,
            2 * k * d2 + 4 * od * od + 4 * major2 * d.Y * d.Y,
            4 * d2 * od,
            d2 * d2
        ];
        if (!BoundingBox.TryInterval(ray, minT, maxT, out float near, out float far)) return false;
        double closest = double.PositiveInfinity;
        foreach (double t in PolynomialRoots.RealRoots(c))
            if (t >= Math.Max(minT, near) && t <= Math.Min(maxT, far) && t < closest)
                closest = t;
        if (!double.IsFinite(closest)) return false;
        float distance = (float)closest;
        Vector3 p = ray.At(distance), local = p - Centre;
        float k2 = local.LengthSquared() - MajorRadius * MajorRadius - MinorRadius * MinorRadius;
        Vector3 n = new(local.X * k2,
            local.Y * (k2 + 2 * MajorRadius * MajorRadius), local.Z * k2);
        if (n.LengthSquared() < 1e-12f) return false;
        n = Vector3.Normalize(n);
        bool front = Vector3.Dot(ray.Direction, n) < 0;
        hit = new Hit(distance, p, front ? n : -n, Material)
        {
            LocalPoint = local,
            UV = new Vector2(0.5f + MathF.Atan2(local.Z, local.X) / (2 * MathF.PI),
                0.5f + MathF.Atan2(local.Y,
                    MathF.Sqrt(local.X * local.X + local.Z * local.Z) - MajorRadius) / (2 * MathF.PI)),
            FrontFace = front
        };
        return true;
    }
}

/// <summary>
/// Real polynomial roots through recursive derivative isolation and bisection (degree up to four).
/// Handles tangencies via stationary-point checking, unlike fixed-step sampling.
/// </summary>
public static class PolynomialRoots
{
    public static IReadOnlyList<double> RealRoots(params double[] coeff)
    {
        if (coeff is null || coeff.Length == 0) return [];
        int degree = coeff.Length - 1;
        while (degree > 0 && Math.Abs(coeff[degree]) < 1e-18) degree--;
        if (degree == 0) return [];
        if (degree == 1) return [-coeff[0] / coeff[1]];
        double leading = Math.Abs(coeff[degree]);
        double bound = 1d;
        for (int i = 0; i < degree; i++) bound = Math.Max(bound, 1 + Math.Abs(coeff[i]) / leading);
        if (!double.IsFinite(bound)) return [];
        double[] derivative = new double[degree];
        for (int i = 1; i <= degree; i++) derivative[i - 1] = coeff[i] * i;
        List<double> points = [-bound];
        foreach (double value in RealRoots(derivative))
            if (value > -bound && value < bound) points.Add(value);
        points.Add(bound);
        points.Sort();
        List<double> roots = [];
        double Evaluate(double x)
        {
            double value = coeff[degree];
            for (int k = degree - 1; k >= 0; k--) value = value * x + coeff[k];
            return value;
        }
        bool Zero(double x, double value)
        {
            double scale = 0, power = 1;
            for (int i = 0; i <= degree; i++)
            {
                scale += Math.Abs(coeff[i]) * power;
                power *= Math.Abs(x);
            }
            return Math.Abs(value) < 1e-12 * Math.Max(1d, scale);
        }
        void Add(double root)
        {
            if (double.IsFinite(root) && roots.All(x => Math.Abs(x - root) > 1e-6 * Math.Max(1d, Math.Abs(root))))
                roots.Add(root);
        }
        foreach (double point in points) if (Zero(point, Evaluate(point))) Add(point);
        for (int i = 0; i + 1 < points.Count; i++)
        {
            double left = points[i], right = points[i + 1];
            double fl = Evaluate(left), fr = Evaluate(right);
            if (!double.IsFinite(fl) || !double.IsFinite(fr) || Math.Sign(fl) == Math.Sign(fr)) continue;
            for (int j = 0; j < 100; j++)
            {
                double mid = (left + right) * 0.5;
                double fm = Evaluate(mid);
                if (Math.Sign(fm) == Math.Sign(fl)) { left = mid; fl = fm; }
                else { right = mid; fr = fm; }
                if (right - left < 1e-12 * Math.Max(1d, Math.Abs(mid))) break;
            }
            Add((left + right) * 0.5);
        }
        roots.Sort();
        return roots;
    }
}

/// <summary>Repeat a bounded mesh/primitive using inverse ray transforms and inverse-transpose normals.</summary>
public sealed class TransformedObject : IBoundedSceneObject
{
    public IBoundedSceneObject Source { get; }
    public Matrix4x4 World { get; }
    private readonly Matrix4x4 inverse;
    private readonly Matrix4x4 inverseTranspose;
    public Bounds BoundingBox { get; }

    public TransformedObject(IBoundedSceneObject source, Matrix4x4 world)
    {
        Source = source;
        World = world;
        if (!Matrix4x4.Invert(world, out inverse))
            throw new ArgumentException("Transform matrix must be invertible.", nameof(world));
        inverseTranspose = Matrix4x4.Transpose(inverse);
        Bounds original = source.BoundingBox;
        Vector3 min = new(float.PositiveInfinity), max = new(float.NegativeInfinity);
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new(
                (i & 1) == 0 ? original.Min.X : original.Max.X,
                (i & 2) == 0 ? original.Min.Y : original.Max.Y,
                (i & 4) == 0 ? original.Min.Z : original.Max.Z);
            Vector3 p = Vector3.Transform(corner, world);
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        BoundingBox = new Bounds(min - new Vector3(0.0001f), max + new Vector3(0.0001f));
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit)
    {
        hit = default;
        Ray local = new(Vector3.Transform(ray.Origin, inverse),
            Vector3.TransformNormal(ray.Direction, inverse));
        // Direction is deliberately NOT normalized, preserving t under affine transforms.
        if (!Source.TryHit(local, minT, maxT, out Hit sourceHit)) return false;
        Vector3 point = ray.At(sourceHit.Distance);
        Vector3 outward = sourceHit.FrontFace ? sourceHit.Normal : -sourceHit.Normal;
        Vector3 transformedNormal = Vector3.TransformNormal(outward, inverseTranspose);
        if (transformedNormal.LengthSquared() < 1e-12f) return false;
        transformedNormal = Vector3.Normalize(transformedNormal);
        bool front = Vector3.Dot(ray.Direction, transformedNormal) < 0;
        hit = sourceHit with
        {
            Position = point,
            Normal = front ? transformedNormal : -transformedNormal,
            FrontFace = front,
            LocalPoint = sourceHit.LocalPoint
        };
        return true;
    }
}

/// <summary>Compound with an internal BVH; can include many triangles or object instances.</summary>
public sealed class Compound : IBoundedSceneObject
{
    private readonly CompiledScene scene;
    public Bounds BoundingBox { get; }
    public Compound(IEnumerable<IBoundedSceneObject> objects)
    {
        var list = objects.ToList();
        if (list.Count == 0) throw new ArgumentException("Compound needs at least one object.");
        Bounds box = list[0].BoundingBox;
        foreach (var obj in list.Skip(1)) box = Bounds.Union(box, obj.BoundingBox);
        BoundingBox = box;
        Scene nested = new();
        foreach (var obj in list) nested.Objects.Add(obj);
        scene = nested.Compile();
    }
    public bool TryHit(in Ray ray, float minT, float maxT, out Hit hit) =>
        scene.TryHit(ray, minT, maxT, out hit);
}
