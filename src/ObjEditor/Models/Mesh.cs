namespace CustomAssets.ObjEditor.Models;

public sealed class Vertex
{
    private static int _next = 1;
    public int Id { get; init; } = System.Threading.Interlocked.Increment(ref _next);
    public Vec3 Position { get; set; } = Vec3.Zero;
}

// Faces reference vertex IDs (stable across deletes) and optionally per-corner
// UV indices. A null UV index means "no UV for this corner".
public sealed class Face
{
    private static int _next = 1;
    public int Id { get; init; } = System.Threading.Interlocked.Increment(ref _next);
    public List<int> VertexIds { get; init; } = new();
    public List<int?> UvIds { get; init; } = new();
    public int? MaterialId { get; set; }
    // OBJ smoothing-group id. 0 = no smoothing (flat shaded). Non-zero =
    // average per-corner normals across all faces sharing the same group and
    // a vertex. A boundary between two different groups (or to group 0) is
    // a hard edge — neighbour does not contribute to this face's normal.
    public int SmoothingGroup { get; set; }
}

public sealed class UvCoord
{
    private static int _next = 1;
    public int Id { get; init; } = System.Threading.Interlocked.Increment(ref _next);
    public Vec2 Uv { get; set; } = Vec2.Zero;
}

public sealed class Material
{
    private static int _next = 1;
    public int Id { get; init; } = System.Threading.Interlocked.Increment(ref _next);
    public string Name { get; set; } = "material";
    public (double R, double G, double B) Diffuse { get; set; } = (0.8, 0.8, 0.8);
    public string TexturePath { get; set; } = "";
    public byte[]? TextureBytes { get; set; }
}

public sealed class Mesh
{
    public string Name { get; set; } = "mesh";
    public Dictionary<int, Vertex> Vertices { get; } = new();
    public Dictionary<int, Face> Faces { get; } = new();
    public Dictionary<int, UvCoord> Uvs { get; } = new();
    public Dictionary<int, Material> Materials { get; } = new();

    public Vertex AddVertex(Vec3 p)
    {
        var v = new Vertex { Position = p };
        Vertices[v.Id] = v;
        return v;
    }

    public UvCoord AddUv(Vec2 uv)
    {
        var u = new UvCoord { Uv = uv };
        Uvs[u.Id] = u;
        return u;
    }

    public Face AddFace(IEnumerable<int> vertexIds, IEnumerable<int?>? uvIds = null, int? materialId = null)
    {
        var f = new Face { MaterialId = materialId };
        f.VertexIds.AddRange(vertexIds);
        if (uvIds != null) f.UvIds.AddRange(uvIds);
        while (f.UvIds.Count < f.VertexIds.Count) f.UvIds.Add(null);
        Faces[f.Id] = f;
        return f;
    }

    public Material AddMaterial(string name)
    {
        var m = new Material { Name = name };
        Materials[m.Id] = m;
        return m;
    }

    public void DeleteVertex(int id)
    {
        Vertices.Remove(id);
        foreach (var faceId in Faces.Where(kv => kv.Value.VertexIds.Contains(id)).Select(kv => kv.Key).ToList())
            Faces.Remove(faceId);
    }

    public void DeleteFace(int id) => Faces.Remove(id);

    public void DeleteMaterial(int id)
    {
        Materials.Remove(id);
        foreach (var f in Faces.Values)
            if (f.MaterialId == id) f.MaterialId = null;
    }
}
