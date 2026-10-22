using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace SharpRay.Core;

/// <summary>
/// Loads ASCII PLY and the historical SharpRay "triangle { vertex ... normal ... }"
/// text format (e.g. RayTracer/Gfx/PLY/VenusDeMilo.txt).
/// Returns a BVH-backed mesh with triangulated polygon faces.
/// </summary>
public static partial class MeshImporter
{
    private const int MaxVertices = 2_000_000;
    private const int MaxTriangles = 3_000_000;

    [GeneratedRegex(@"vertex\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)\s+([+-]?(?:\d+\.?\d*|\.\d+)(?:[eE][+-]?\d+)?)",
        RegexOptions.IgnoreCase)]
    private static partial Regex VertexPattern();

    public static Compound Load(string path, Material material)
    {
        using StreamReader reader = File.OpenText(path);
        string? first = reader.ReadLine();
        if (first is null) throw new InvalidDataException("Empty mesh file.");
        var tris = first.Trim() == "ply"
            ? ReadAsciiPly(reader, material)
            : ReadHistorical(reader, first, material);
        return new Compound(tris);
    }

    private static List<IBoundedSceneObject> ReadHistorical(StreamReader reader,
        string firstLine, Material material)
    {
        List<IBoundedSceneObject> triangles = [];
        string? line = firstLine;
        while (line is not null)
        {
            MatchCollection vertices = VertexPattern().Matches(line);
            if (vertices.Count >= 3)
            {
                Vector3 a = ParsePoint(vertices[0]), b = ParsePoint(vertices[1]), c = ParsePoint(vertices[2]);
                triangles.Add(new Triangle(a, b, c, material));
                if (triangles.Count > MaxTriangles) throw new InvalidDataException("Mesh too large.");
            }
            line = reader.ReadLine();
        }
        if (triangles.Count == 0) throw new InvalidDataException("No triangles in legacy mesh.");
        return triangles;
    }

    private static Vector3 ParsePoint(Match match) =>
        new(float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture),
            float.Parse(match.Groups[3].Value, CultureInfo.InvariantCulture));

    private static List<IBoundedSceneObject> ReadAsciiPly(StreamReader reader, Material material)
    {
        int vertexCount = -1, faceCount = -1, vertexProperties = 0;
        bool inVertex = false;
        while (true)
        {
            string line = reader.ReadLine() ?? throw new InvalidDataException("Truncated PLY header.");
            if (line == "end_header") break;
            string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3 && parts[0] == "format" &&
                (parts[1] != "ascii" || parts[2] != "1.0"))
                throw new NotSupportedException("Only ASCII PLY format 1.0 is supported.");
            if (parts.Length >= 3 && parts[0] == "element")
            {
                inVertex = parts[1] == "vertex";
                if (inVertex) vertexCount = int.Parse(parts[2], CultureInfo.InvariantCulture);
                else if (parts[1] == "face") faceCount = int.Parse(parts[2], CultureInfo.InvariantCulture);
            }
            if (inVertex && parts.Length >= 2 && parts[0] == "property")
                vertexProperties++;
        }
        if (vertexCount < 1 || faceCount < 1 || vertexCount > MaxVertices || faceCount > MaxTriangles ||
            vertexProperties < 3) throw new InvalidDataException("Invalid PLY vertex/face counts.");
        Vector3[] vertices = new Vector3[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            string[] parts = (reader.ReadLine() ?? throw new EndOfStreamException())
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) throw new InvalidDataException("Malformed PLY vertex.");
            vertices[i] = new Vector3(float.Parse(parts[0], CultureInfo.InvariantCulture),
                float.Parse(parts[1], CultureInfo.InvariantCulture),
                float.Parse(parts[2], CultureInfo.InvariantCulture));
        }
        List<IBoundedSceneObject> triangles = [];
        for (int i = 0; i < faceCount; i++)
        {
            string[] parts = (reader.ReadLine() ?? throw new EndOfStreamException())
                .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 4) throw new InvalidDataException("Malformed PLY face.");
            int count = int.Parse(parts[0], CultureInfo.InvariantCulture);
            if (count < 3 || count > 4096 || parts.Length < count + 1)
                throw new InvalidDataException("Malformed PLY polygon.");
            int[] ids = new int[count];
            for (int j = 0; j < count; j++)
            {
                ids[j] = int.Parse(parts[j + 1], CultureInfo.InvariantCulture);
                if (ids[j] < 0 || ids[j] >= vertices.Length)
                    throw new InvalidDataException("PLY polygon index out of range.");
            }
            for (int j = 1; j < count - 1; j++)
            {
                triangles.Add(new Triangle(vertices[ids[0]], vertices[ids[j]],
                    vertices[ids[j + 1]], material));
                if (triangles.Count > MaxTriangles) throw new InvalidDataException("Mesh too large.");
            }
        }
        return new Compound(triangles);
    }
}
