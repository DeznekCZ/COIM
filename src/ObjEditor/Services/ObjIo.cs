using System.Globalization;
using System.Text;
using CustomAssets.ObjEditor.Models;

namespace CustomAssets.ObjEditor.Services;

// Parses and writes Wavefront OBJ. Coordinates pass through unchanged — the
// editor treats them as Unity-convention (+Y up, +Z forward). Normals are
// ignored on import (Three.js recomputes for shading) and omitted on export.
public sealed class ObjIo
{
    public sealed record LoadResult(Mesh Mesh, string? MtlLibRelative);

    public LoadResult Load(string objText)
    {
        var mesh = new Mesh();
        var positions = new List<Vec3>();
        var uvs = new List<Vec2>();
        var vertexIds = new List<int>();
        var uvIds = new List<int>();
        string? mtllib = null;
        int? currentMaterial = null;
        var materialByName = new Dictionary<string, int>(StringComparer.Ordinal);
        // OBJ smoothing groups: `s off` / `s 0` → flat (group 0); any non-zero
        // integer is a distinct group. Two faces in the same group share
        // averaged corner normals where they meet; different groups → hard edge.
        int currentSmoothing = 0;

        foreach (var rawLine in objText.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r', ' ', '\t');
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "mtllib":
                    if (parts.Length > 1)
                        mtllib = line.Substring(parts[0].Length).Trim();
                    break;

                case "o":
                case "g":
                    if (parts.Length > 1) mesh.Name = parts[1];
                    break;

                case "v":
                    if (parts.Length >= 4)
                        positions.Add(new Vec3(D(parts[1]), D(parts[2]), D(parts[3])));
                    break;

                case "vt":
                    if (parts.Length >= 3)
                        uvs.Add(new Vec2(D(parts[1]), D(parts[2])));
                    break;

                case "vn":
                    break;

                case "usemtl":
                    if (parts.Length > 1)
                    {
                        var name = parts[1];
                        if (!materialByName.TryGetValue(name, out var mid))
                        {
                            mid = mesh.AddMaterial(name).Id;
                            materialByName[name] = mid;
                        }
                        currentMaterial = mid;
                    }
                    break;

                case "s":
                    if (parts.Length > 1)
                    {
                        var arg = parts[1];
                        if (string.Equals(arg, "off", StringComparison.OrdinalIgnoreCase))
                            currentSmoothing = 0;
                        else if (!int.TryParse(arg, NumberStyles.Integer, CultureInfo.InvariantCulture, out currentSmoothing))
                            currentSmoothing = 0;
                        if (currentSmoothing < 0) currentSmoothing = 0;
                    }
                    break;

                case "f":
                    var vs = new List<int>();
                    var us = new List<int?>();
                    for (int i = 1; i < parts.Length; i++)
                    {
                        var corner = parts[i].Split('/');
                        var vIdx = ResolveIndex(corner[0], positions.Count);
                        if (vIdx < 0) continue;
                        EnsureVertex(mesh, positions, vertexIds, vIdx);
                        vs.Add(vertexIds[vIdx]);

                        int? uvId = null;
                        if (corner.Length >= 2 && corner[1].Length > 0)
                        {
                            var uIdx = ResolveIndex(corner[1], uvs.Count);
                            if (uIdx >= 0)
                            {
                                EnsureUv(mesh, uvs, uvIds, uIdx);
                                uvId = uvIds[uIdx];
                            }
                        }
                        us.Add(uvId);
                    }
                    if (vs.Count >= 3)
                    {
                        // Reverse the corner order so what was CCW-from-outside
                        // in the OBJ (standard winding) becomes CW-from-outside
                        // in memory (Unity convention used by the renderer).
                        // The UV list is reversed in lockstep so each corner
                        // keeps the same UV index it had in the OBJ.
                        vs.Reverse();
                        us.Reverse();
                        var face = mesh.AddFace(vs, us, currentMaterial);
                        face.SmoothingGroup = currentSmoothing;
                    }
                    break;
            }
        }

        for (int i = 0; i < positions.Count; i++) EnsureVertex(mesh, positions, vertexIds, i);
        return new LoadResult(mesh, mtllib);
    }

    private static void EnsureVertex(Mesh mesh, List<Vec3> positions, List<int> ids, int index)
    {
        while (ids.Count <= index) ids.Add(0);
        if (ids[index] == 0) ids[index] = mesh.AddVertex(positions[index]).Id;
    }

    private static void EnsureUv(Mesh mesh, List<Vec2> uvs, List<int> ids, int index)
    {
        while (ids.Count <= index) ids.Add(0);
        if (ids[index] == 0) ids[index] = mesh.AddUv(uvs[index]).Id;
    }

    private static int ResolveIndex(string token, int count)
    {
        if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return -1;
        if (n > 0) return n - 1;
        if (n < 0) return count + n;
        return -1;
    }

    private static double D(string s)
        => double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);

    public string Write(Mesh mesh, string? mtlLibName = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Written by ObjEditor");
        if (!string.IsNullOrEmpty(mtlLibName))
            sb.Append("mtllib ").AppendLine(mtlLibName);
        sb.Append("o ").AppendLine(string.IsNullOrEmpty(mesh.Name) ? "mesh" : mesh.Name);

        var vertexOrder = mesh.Vertices.Values.OrderBy(v => v.Id).ToList();
        var uvOrder = mesh.Uvs.Values.OrderBy(u => u.Id).ToList();
        var vertexIndex = vertexOrder.Select((v, i) => (v.Id, Idx: i + 1)).ToDictionary(t => t.Id, t => t.Idx);
        var uvIndex = uvOrder.Select((u, i) => (u.Id, Idx: i + 1)).ToDictionary(t => t.Id, t => t.Idx);

        foreach (var v in vertexOrder)
            sb.AppendFormat(CultureInfo.InvariantCulture, "v {0} {1} {2}\n",
                v.Position.X, v.Position.Y, v.Position.Z);
        foreach (var u in uvOrder)
            sb.AppendFormat(CultureInfo.InvariantCulture, "vt {0} {1}\n", u.Uv.U, u.Uv.V);

        var groupedFaces = mesh.Faces.Values
            .OrderBy(f => f.MaterialId ?? -1)
            .ThenBy(f => f.SmoothingGroup)  // group by smoothing too — fewer `s` toggles
            .ThenBy(f => f.Id)
            .ToList();
        int? lastMat = null;
        int? lastSmoothing = null;
        foreach (var f in groupedFaces)
        {
            if (f.MaterialId != lastMat)
            {
                if (f.MaterialId is int mid && mesh.Materials.TryGetValue(mid, out var mat))
                    sb.Append("usemtl ").AppendLine(mat.Name);
                lastMat = f.MaterialId;
            }
            if (f.SmoothingGroup != lastSmoothing)
            {
                sb.AppendLine(f.SmoothingGroup == 0 ? "s off" : "s " + f.SmoothingGroup.ToString(CultureInfo.InvariantCulture));
                lastSmoothing = f.SmoothingGroup;
            }
            sb.Append('f');
            // Emit standard OBJ CCW-from-outside winding. The model stores
            // Unity-CW order, so iterate corners in reverse for export.
            for (int i = f.VertexIds.Count - 1; i >= 0; i--)
            {
                sb.Append(' ');
                sb.Append(vertexIndex[f.VertexIds[i]]);
                var uvId = i < f.UvIds.Count ? f.UvIds[i] : null;
                if (uvId is int uid && uvIndex.TryGetValue(uid, out var u))
                    sb.Append('/').Append(u);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}
