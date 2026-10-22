using System.Numerics;
using System.IO;

namespace SharpRay.Core;

public readonly record struct TextureContext(Vector3 Position, Vector3 LocalPoint, Vector2 UV);

public interface ITexture
{
    Vector3 Sample(in TextureContext context);
}

public sealed record SolidTexture(Vector3 Colour) : ITexture
{
    public Vector3 Sample(in TextureContext context) => Colour;
}

public sealed record CheckerTexture(Vector3 A, Vector3 B, float Frequency = 1) : ITexture
{
    public Vector3 Sample(in TextureContext context)
    {
        Vector3 p = context.Position * MathF.Max(0.0001f, Frequency);
        int x = (int)MathF.Floor(p.X);
        int y = (int)MathF.Floor(p.Y);
        int z = (int)MathF.Floor(p.Z);
        return ((x ^ y ^ z) & 1) == 0 ? A : B;
    }
}

public enum TextureProjection { UV, Planar, Cylindrical, Spherical, RectangularLegacy, CylindricalLegacy, SphericalLegacy }

/// <summary>
/// Immutable nearest-neighbour bitmap texture. BMP reader supports uncompressed 24/32-bit
/// Windows BMPs used by the original SharpRay's texture collection. No System.Drawing dependency.
/// </summary>
public sealed class ImageTexture : ITexture
{
    private readonly Vector3[] pixels;
    public int Width { get; }
    public int Height { get; }
    public TextureProjection Projection { get; }

    public ImageTexture(int width, int height, Vector3[] pixels, TextureProjection projection = TextureProjection.UV)
    {
        if (width < 1 || height < 1 || pixels.Length != checked(width * height))
            throw new ArgumentException("Pixel array dimensions are invalid.");
        Width = width;
        Height = height;
        this.pixels = (Vector3[])pixels.Clone();
        Projection = projection;
    }

    public Vector3 Sample(in TextureContext context)
    {
        Vector2 uv = Projection switch
        {
            TextureProjection.UV => context.UV,
            TextureProjection.Planar => new(context.LocalPoint.X, context.LocalPoint.Z),
            TextureProjection.Cylindrical => new(
                0.5f + MathF.Atan2(context.LocalPoint.Z, context.LocalPoint.X) / (2 * MathF.PI),
                context.LocalPoint.Y),
            TextureProjection.Spherical => Spherical(context.LocalPoint),
            TextureProjection.RectangularLegacy => new((context.LocalPoint.Z + 1) * 0.5f,
                (context.LocalPoint.X + 1) * 0.5f),
            TextureProjection.CylindricalLegacy => new(
                (MathF.Atan2(context.LocalPoint.X, context.LocalPoint.Z) / (2 * MathF.PI) + 1) % 1,
                (context.LocalPoint.Y + 1) * 0.5f),
            TextureProjection.SphericalLegacy => LegacySpherical(context.LocalPoint),
            _ => context.UV
        };
        static float Wrap(float f) => f - MathF.Floor(f);
        int x = Math.Clamp((int)(Wrap(uv.X) * Width), 0, Width - 1);
        int y = Math.Clamp((int)(Wrap(uv.Y) * Height), 0, Height - 1);
        return pixels[y * Width + x];
    }

    private static Vector2 Spherical(Vector3 point)
    {
        if (point.LengthSquared() < 1e-12f) return Vector2.Zero;
        Vector3 n = Vector3.Normalize(point);
        return new Vector2(0.5f + MathF.Atan2(n.Z, n.X) / (2 * MathF.PI),
            0.5f - MathF.Asin(Math.Clamp(n.Y, -1, 1)) / MathF.PI);
    }

    private static Vector2 LegacySpherical(Vector3 point)
    {
        if (point.LengthSquared() < 1e-12f) return Vector2.Zero;
        Vector3 n = Vector3.Normalize(point);
        float phi = MathF.Atan2(n.Y, n.X);
        float u = phi >= 0 ? phi / (2 * MathF.PI) : 1 + phi / (2 * MathF.PI);
        float v = 1 - MathF.Acos(Math.Clamp(n.Z, -1, 1)) / MathF.PI;
        return new Vector2(u, v);
    }

    public static ImageTexture FromBmp(string path, TextureProjection projection = TextureProjection.UV)
    {
        using FileStream stream = File.OpenRead(path);
        using BinaryReader reader = new(stream);
        if (reader.ReadUInt16() != 0x4D42) throw new InvalidDataException("Not a BMP file.");
        reader.ReadUInt32();
        reader.ReadUInt32();
        uint pixelOffset = reader.ReadUInt32();
        uint dibSize = reader.ReadUInt32();
        if (dibSize < 40) throw new NotSupportedException("Expected Windows BITMAPINFOHEADER BMP.");
        int width = reader.ReadInt32();
        int signedHeight = reader.ReadInt32();
        ushort planes = reader.ReadUInt16();
        ushort depth = reader.ReadUInt16();
        uint compression = reader.ReadUInt32();
        if (width is < 1 or > 16000 || signedHeight == 0 || (long)Math.Abs((long)signedHeight) > 16000 ||
            planes != 1 || (depth != 24 && depth != 32) || compression != 0)
            throw new NotSupportedException("Only uncompressed 24/32-bit BMPs up to 16000 pixels are supported.");
        int height = Math.Abs(signedHeight);
        int stride = checked(((width * depth + 31) / 32) * 4);
        Vector3[] result = new Vector3[checked(width * height)];
        stream.Position = pixelOffset;
        for (int srcY = 0; srcY < height; srcY++)
        {
            byte[] row = reader.ReadBytes(stride);
            if (row.Length != stride) throw new EndOfStreamException("Truncated BMP.");
            int dstY = signedHeight > 0 ? height - 1 - srcY : srcY;
            for (int x = 0; x < width; x++)
            {
                int pos = x * (depth / 8);
                result[dstY * width + x] = new Vector3(ToLinear(row[pos + 2]),
                    ToLinear(row[pos + 1]), ToLinear(row[pos]));
            }
        }
        return new ImageTexture(width, height, result, projection);
    }

    private static float ToLinear(byte channel) =>
        MathF.Pow(channel / 255f, 2.2f);
}

public enum NoiseInterpolation { Linear, Cubic }
public enum ProceduralKind { Noise, FBm, Turbulence, Marble }

/// <summary>
/// Stable integer-hash value noise; no mutable random state shared between rendering threads.
/// Linear and smooth-cubic interpolation are analogous to the source noise families.
/// </summary>
public static class ProceduralNoise
{
    private static float Hash(int x, int y, int z)
    {
        unchecked
        {
            uint n = (uint)x * 0x8da6b343u ^ (uint)y * 0xd8163841u ^ (uint)z * 0xcb1ab31fu;
            n ^= n >> 16; n *= 0x7feb352du; n ^= n >> 15;
            n *= 0x846ca68bu; n ^= n >> 16;
            return (n / (float)uint.MaxValue) * 2 - 1;
        }
    }

    public static float Value(Vector3 p, NoiseInterpolation interpolation = NoiseInterpolation.Cubic)
    {
        int ix = (int)MathF.Floor(p.X), iy = (int)MathF.Floor(p.Y), iz = (int)MathF.Floor(p.Z);
        float x = p.X - ix, y = p.Y - iy, z = p.Z - iz;
        if (interpolation == NoiseInterpolation.Cubic)
        {
            x = x * x * (3 - 2 * x);
            y = y * y * (3 - 2 * y);
            z = z * z * (3 - 2 * z);
        }
        float Mix(float a, float b, float t) => a + (b - a) * t;
        return Mix(
            Mix(Mix(Hash(ix, iy, iz), Hash(ix + 1, iy, iz), x),
                Mix(Hash(ix, iy + 1, iz), Hash(ix + 1, iy + 1, iz), x), y),
            Mix(Mix(Hash(ix, iy, iz + 1), Hash(ix + 1, iy, iz + 1), x),
                Mix(Hash(ix, iy + 1, iz + 1), Hash(ix + 1, iy + 1, iz + 1), x), y), z);
    }

    public static float Fractal(Vector3 p, int octaves = 5, float lacunarity = 2f,
        float gain = 0.5f, bool absolute = false, NoiseInterpolation interpolation = NoiseInterpolation.Cubic)
    {
        float sum = 0, totalAmplitude = 0, amplitude = 1;
        for (int i = 0; i < Math.Clamp(octaves, 1, 12); i++)
        {
            float n = Value(p, interpolation);
            sum += (absolute ? MathF.Abs(n) : n) * amplitude;
            totalAmplitude += amplitude;
            p *= lacunarity;
            amplitude *= gain;
        }
        return sum / MathF.Max(totalAmplitude, 1e-6f);
    }
}

/// <summary>Procedural noise, fBm, turbulence and marble from a single configurable texture.</summary>
public sealed record ProceduralTexture(
    ProceduralKind Kind, Vector3 A, Vector3 B,
    float Frequency = 2, int Octaves = 5,
    float Strength = 2f, NoiseInterpolation Interpolation = NoiseInterpolation.Cubic) : ITexture
{
    public Vector3 Sample(in TextureContext context)
    {
        Vector3 p = context.LocalPoint * MathF.Max(Frequency, 0.001f);
        float n = Kind switch
        {
            ProceduralKind.Noise => ProceduralNoise.Value(p, Interpolation) * 0.5f + 0.5f,
            ProceduralKind.FBm => ProceduralNoise.Fractal(p, Octaves,
                interpolation: Interpolation) * 0.5f + 0.5f,
            ProceduralKind.Turbulence => ProceduralNoise.Fractal(p, Octaves,
                absolute: true, interpolation: Interpolation),
            ProceduralKind.Marble => 0.5f + 0.5f * MathF.Sin(
                p.Y * 4f + Strength * ProceduralNoise.Fractal(p, Octaves,
                absolute: true, interpolation: Interpolation) * 8f),
            _ => 0
        };
        return Vector3.Lerp(A, B, Math.Clamp(n, 0f, 1f));
    }
}
