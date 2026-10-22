using System.Numerics;

namespace SharpRay.Core;

/// <summary>Applies inverse affine transforms to texture coordinates (historical TextureInstance).</summary>
public sealed class TextureInstance : ITexture
{
    private readonly ITexture source;
    private readonly Matrix4x4 inverse;

    public TextureInstance(ITexture source, Matrix4x4 textureTransform)
    {
        this.source = source ?? throw new ArgumentNullException(nameof(source));
        if (!Matrix4x4.Invert(textureTransform, out inverse))
            throw new ArgumentException("Texture transform must be invertible.", nameof(textureTransform));
    }

    public Vector3 Sample(in TextureContext context) => source.Sample(
        new TextureContext(Vector3.Transform(context.Position, inverse),
            Vector3.Transform(context.LocalPoint, inverse), context.UV));
}

/// <summary>Wraps the result of multioctave fBm into repeated colour bands.</summary>
public sealed record FbmWrapTexture(Vector3 First, Vector3 Second, float Frequency = 2,
    int Octaves = 5, float Repetitions = 2) : ITexture
{
    public Vector3 Sample(in TextureContext context)
    {
        float n = ProceduralNoise.Fractal(context.LocalPoint * Frequency, Octaves);
        float t = n * Repetitions;
        t -= MathF.Floor(t);
        return Vector3.Lerp(First, Second, t);
    }
}

/// <summary>Historical fractal sum material family with configurable band contrast.</summary>
public sealed record FractalSumTexture(Vector3 First, Vector3 Second, float Frequency = 2,
    int Octaves = 5, float Contrast = 1) : ITexture
{
    public Vector3 Sample(in TextureContext context)
    {
        float n = ProceduralNoise.Fractal(context.LocalPoint * Frequency, Octaves);
        float t = Math.Clamp(0.5f + n * Contrast * 0.5f, 0, 1);
        return Vector3.Lerp(First, Second, t);
    }
}

/// <summary>
/// Marble colour ramp sampled along its horizontal axis, perturbed by multi-octave
/// turbulence. Corresponds to the original Marble bitmap ramp and procedural noise.
/// </summary>
public sealed record MarbleRampTexture(ImageTexture Ramp, float Frequency = 2,
    float Distortion = 3f, int Octaves = 5) : ITexture
{
    public Vector3 Sample(in TextureContext context)
    {
        Vector3 p = context.LocalPoint * Frequency;
        float n = ProceduralNoise.Fractal(p, Octaves, absolute: true);
        float t = 0.5f + 0.5f * MathF.Sin(p.Y * MathF.PI + Distortion * n);
        // Clamp rather than wrapping the rightmost ramp sample back to column zero.
        t = Math.Clamp(t, 0, 0.999999f);
        return Ramp.Sample(new TextureContext(context.Position, p, new Vector2(t, 0.5f)));
    }
}
