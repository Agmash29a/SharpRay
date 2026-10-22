using System.Numerics;

namespace SharpRay.Core;

/// <summary>
/// Immutable median-split bounding volume hierarchy. Built once per render and safe for
/// concurrent ray queries. Infinite planes stay in a separate linear list.
/// </summary>
internal sealed class BvhNode
{
    private const int LeafSize = 4;
    private readonly BvhNode? left;
    private readonly BvhNode? right;
    private readonly IBoundedSceneObject[]? leaf;
    public Bounds Box { get; }

    private BvhNode(Bounds box, IBoundedSceneObject[]? leaf, BvhNode? left, BvhNode? right)
    {
        Box = box;
        this.leaf = leaf;
        this.left = left;
        this.right = right;
    }

    public static BvhNode? Build(IBoundedSceneObject[] objects)
    {
        if (objects.Length == 0) return null;
        return BuildRange(objects, 0, objects.Length);
    }

    private static BvhNode BuildRange(IBoundedSceneObject[] objects, int start, int count)
    {
        Bounds box = objects[start].BoundingBox;
        Vector3 minCentroid = box.Centre, maxCentroid = box.Centre;
        for (int i = start + 1; i < start + count; i++)
        {
            Bounds b = objects[i].BoundingBox;
            box = Bounds.Union(box, b);
            minCentroid = Vector3.Min(minCentroid, b.Centre);
            maxCentroid = Vector3.Max(maxCentroid, b.Centre);
        }

        if (count <= LeafSize)
            return new BvhNode(box, objects.AsSpan(start, count).ToArray(), null, null);

        Vector3 extent = maxCentroid - minCentroid;
        int axis = extent.X >= extent.Y && extent.X >= extent.Z ? 0 :
            extent.Y >= extent.Z ? 1 : 2;

        static float Component(Vector3 v, int axis) => axis switch
        {
            0 => v.X, 1 => v.Y, _ => v.Z
        };
        Array.Sort(objects, start, count, Comparer<IBoundedSceneObject>.Create(
            (a, b) => Component(a.BoundingBox.Centre, axis).CompareTo(Component(b.BoundingBox.Centre, axis))));

        int half = count / 2;
        return new BvhNode(box, null, BuildRange(objects, start, half),
            BuildRange(objects, start + half, count - half));
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit closest)
    {
        closest = default;
        if (!Box.Intersects(ray, minT, maxT)) return false;

        if (leaf is not null)
        {
            bool found = false;
            foreach (var obj in leaf)
            {
                if (!obj.TryHit(ray, minT, maxT, out Hit hit)) continue;
                maxT = hit.Distance;
                closest = hit;
                found = true;
            }
            return found;
        }

        // Visit nearer child first, allowing the other side to be pruned with a tighter limit.
        BvhNode? first = left, second = right;
        float leftNear = 0, rightNear = 0;
        bool leftValid = first is not null && first.Box.TryInterval(ray, minT, maxT, out leftNear, out _);
        bool rightValid = second is not null && second.Box.TryInterval(ray, minT, maxT, out rightNear, out _);
        if (leftValid && rightValid && rightNear < leftNear)
            (first, second) = (second, first);
        bool foundFirst = first is not null && first.TryHit(ray, minT, maxT, out closest);
        if (foundFirst) maxT = closest.Distance;
        if (second is not null && second.TryHit(ray, minT, maxT, out Hit secondHit))
        {
            closest = secondHit;
            return true;
        }
        return foundFirst;
    }
}

public sealed class CompiledScene
{
    private readonly BvhNode? bounded;
    private readonly ISceneObject[] unbounded;
    public IReadOnlyList<PointLight> Lights { get; }
    public IReadOnlyList<DirectionalLight> DirectionalLights { get; }
    public float AmbientIntensity { get; }
    public Vector3 AmbientColour { get; }
    public Vector3 SkyColour { get; }

    public CompiledScene(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);
        ISceneObject[] objects = scene.Objects.ToArray();
        bounded = BvhNode.Build(objects.OfType<IBoundedSceneObject>().ToArray());
        unbounded = objects.Where(o => o is not IBoundedSceneObject).ToArray();
        Lights = Array.AsReadOnly(scene.Lights.ToArray());
        DirectionalLights = Array.AsReadOnly(scene.DirectionalLights.ToArray());
        AmbientIntensity = Math.Clamp(scene.AmbientIntensity, 0f, 1f);
        AmbientColour = Vector3.Clamp(scene.AmbientColour, Vector3.Zero, Vector3.One);
        SkyColour = scene.SkyColour;
    }

    public bool TryHit(in Ray ray, float minT, float maxT, out Hit closest)
    {
        closest = default;
        bool found = bounded is not null && bounded.TryHit(ray, minT, maxT, out closest);
        if (found) maxT = closest.Distance;
        foreach (ISceneObject obj in unbounded)
        {
            if (!obj.TryHit(ray, minT, maxT, out Hit hit)) continue;
            maxT = hit.Distance;
            closest = hit;
            found = true;
        }
        return found;
    }
}
