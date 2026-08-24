using CustomAssets.ObjEditor.Models;

namespace CustomAssets.ObjEditor.Services;

public enum SelectionMode { Vertex, Face }

public sealed class SceneStore
{
    public Mesh Mesh { get; private set; } = SeedCube();
    public string? LoadedObjPath { get; private set; }
    public SelectionMode Mode { get; private set; } = SelectionMode.Vertex;
    public HashSet<int> SelectedVertices { get; } = new();
    public HashSet<int> SelectedFaces { get; } = new();
    public int? SelectedMaterialId { get; private set; }

    // -------- Face-placement tool state --------
    //
    // When IsPlacingFace is true, clicks on the 3D viewport append vertices
    // to PlacingVertexIds instead of changing the selection. The order of
    // clicks defines the winding (CCW = front face under our Unity-axis
    // convention). FinishPlacement triangulates the polygon for >4 corners
    // using a fan from PlacingFanRoot.
    public bool IsPlacingFace { get; private set; }
    public List<int> PlacingVertexIds { get; } = new();
    public int PlacingFanRoot { get; set; } = 0;

    // -------- Cut-face tool state --------
    //
    // After selecting one face (≥4 corners), the user enters cut mode and picks
    // two non-adjacent corners. The face is then replaced by two new faces
    // along that chord, both inheriting the original material.
    public bool IsCuttingFace { get; private set; }
    public int CuttingFaceId { get; private set; }
    public List<int> CuttingPickedCorners { get; } = new();

    public event Action? OnChange;
    public void Notify() => OnChange?.Invoke();

    public void Replace(Mesh mesh, string? path)
    {
        Mesh = mesh;
        LoadedObjPath = path;
        SelectedVertices.Clear();
        SelectedFaces.Clear();
        SelectedMaterialId = mesh.Materials.Values.OrderBy(m => m.Id).FirstOrDefault()?.Id;
        Notify();
    }

    public void SetMode(SelectionMode m)
    {
        Mode = m;
        SelectedVertices.Clear();
        SelectedFaces.Clear();
        Notify();
    }

    public void SelectVertex(int id, bool additive)
    {
        if (!additive) { SelectedVertices.Clear(); SelectedFaces.Clear(); }
        if (id != 0)
        {
            if (!SelectedVertices.Add(id) && additive) SelectedVertices.Remove(id);
        }
        Notify();
    }

    public void SelectFace(int id, bool additive)
    {
        if (!additive) { SelectedVertices.Clear(); SelectedFaces.Clear(); }
        if (id != 0)
        {
            if (!SelectedFaces.Add(id) && additive) SelectedFaces.Remove(id);
        }
        Notify();
    }

    public void SelectMaterial(int? id) { SelectedMaterialId = id; Notify(); }

    public void SetVertexPosition(int id, Vec3 p)
    {
        if (Mesh.Vertices.TryGetValue(id, out var v)) { v.Position = p; Notify(); }
    }

    // Apply a single coordinate to every selected vertex. axis: 0=X, 1=Y, 2=Z.
    // Used by the bottom-bar multi-vertex inputs when a shared value is typed.
    public void SetSelectedVerticesCoord(int axis, double value)
    {
        if (SelectedVertices.Count == 0) return;
        foreach (var id in SelectedVertices)
            if (Mesh.Vertices.TryGetValue(id, out var v))
                v.Position = axis switch
                {
                    0 => v.Position.With(x: value),
                    1 => v.Position.With(y: value),
                    _ => v.Position.With(z: value),
                };
        Notify();
    }

    // Force every selected vertex's axis to a shared value (the current mean
    // by default). Lets the user resolve a "Mixed" indicator with one click.
    public void AlignSelectedVerticesOnAxis(int axis, double? target = null)
    {
        if (SelectedVertices.Count < 2) return;
        double t;
        if (target.HasValue) t = target.Value;
        else
        {
            double sum = 0; int n = 0;
            foreach (var id in SelectedVertices)
                if (Mesh.Vertices.TryGetValue(id, out var v))
                {
                    sum += axis switch { 0 => v.Position.X, 1 => v.Position.Y, _ => v.Position.Z };
                    n++;
                }
            if (n == 0) return;
            t = sum / n;
        }
        SetSelectedVerticesCoord(axis, t);
    }

    // Collapse every selected vertex into a single survivor at the centroid.
    // All face references to the disappeared vertices are repointed to the
    // survivor, so adjacent faces remain intact but now share a corner.
    public void WeldSelectedVertices()
    {
        if (SelectedVertices.Count < 2) return;
        var ids = SelectedVertices.ToList();
        double sx = 0, sy = 0, sz = 0; int n = 0;
        foreach (var id in ids)
            if (Mesh.Vertices.TryGetValue(id, out var v))
            { sx += v.Position.X; sy += v.Position.Y; sz += v.Position.Z; n++; }
        if (n == 0) return;
        var avg = new Vec3(sx / n, sy / n, sz / n);
        var survivor = ids[0];
        if (!Mesh.Vertices.TryGetValue(survivor, out var first)) return;
        first.Position = avg;
        for (int i = 1; i < ids.Count; i++)
        {
            var dead = ids[i];
            if (dead == survivor) continue;
            foreach (var face in Mesh.Faces.Values)
                for (int j = 0; j < face.VertexIds.Count; j++)
                    if (face.VertexIds[j] == dead) face.VertexIds[j] = survivor;
            Mesh.Vertices.Remove(dead);
        }
        // A face that had two corners pointing at the merged vertices would
        // now have duplicate corners side-by-side or in a row; collapse them
        // so we don't leave degenerate edges behind.
        foreach (var face in Mesh.Faces.Values)
            CollapseRepeatedCorners(face);
        SelectedVertices.Clear();
        SelectedVertices.Add(survivor);
        Notify();
    }

    // Inverse of weld for a single shared vertex: each face using it gets
    // its own copy at the same position. The originally selected vertex is
    // kept by the first face; subsequent faces are repointed to fresh copies.
    public void SplitSelectedVertex()
    {
        if (SelectedVertices.Count != 1) return;
        var vid = SelectedVertices.First();
        if (!Mesh.Vertices.TryGetValue(vid, out var src)) return;
        var faces = Mesh.Faces.Values.Where(f => f.VertexIds.Contains(vid)).ToList();
        if (faces.Count <= 1) return;
        // Skip the first face so the original vertex still has one face
        // using it (otherwise the un-replaced vertex would become orphaned).
        for (int i = 1; i < faces.Count; i++)
        {
            var face = faces[i];
            var copy = Mesh.AddVertex(src.Position);
            for (int j = 0; j < face.VertexIds.Count; j++)
                if (face.VertexIds[j] == vid) face.VertexIds[j] = copy.Id;
        }
        Notify();
    }

    public int FacesUsingVertex(int vertexId)
        => Mesh.Faces.Values.Count(f => f.VertexIds.Contains(vertexId));

    private static void CollapseRepeatedCorners(Face face)
    {
        // Walk the corner list; drop adjacent duplicates (including wrap-around
        // last↔first). Two-corner residue means the face has degenerated and
        // we leave it alone — the caller can spot it via vertex count.
        if (face.VertexIds.Count < 2) return;
        for (int i = face.VertexIds.Count - 1; i >= 0; i--)
        {
            int next = (i + 1) % face.VertexIds.Count;
            if (face.VertexIds[i] == face.VertexIds[next])
            {
                face.VertexIds.RemoveAt(next > i ? next : i);
                if (next < face.UvIds.Count) face.UvIds.RemoveAt(next > i ? next : i);
                if (face.VertexIds.Count < 2) break;
            }
        }
    }

    public Vertex AddVertex(Vec3 p) { var v = Mesh.AddVertex(p); Notify(); return v; }

    public void DeleteSelection()
    {
        if (Mode == SelectionMode.Vertex)
            foreach (var id in SelectedVertices.ToList()) Mesh.DeleteVertex(id);
        else if (Mode == SelectionMode.Face)
            foreach (var id in SelectedFaces.ToList()) Mesh.DeleteFace(id);
        SelectedVertices.Clear();
        SelectedFaces.Clear();
        Notify();
    }

    public Face? CreateFaceFromSelection()
    {
        if (SelectedVertices.Count < 3) return null;
        var face = Mesh.AddFace(SelectedVertices, materialId: SelectedMaterialId);
        Notify();
        return face;
    }

    public void BeginPlaceFace()
    {
        IsPlacingFace = true;
        PlacingVertexIds.Clear();
        PlacingFanRoot = 0;
        SelectedVertices.Clear();
        SelectedFaces.Clear();
        // The tool only makes sense in vertex mode (we pick vertices).
        Mode = SelectionMode.Vertex;
        Notify();
    }

    public void AddVertexToPlacement(int vertexId)
    {
        if (!IsPlacingFace) return;
        if (vertexId == 0 || !Mesh.Vertices.ContainsKey(vertexId)) return;
        // Don't allow the same vertex twice in a row — almost always a misclick.
        if (PlacingVertexIds.Count > 0 && PlacingVertexIds[^1] == vertexId) return;
        // Don't allow duplicates anywhere; a polygon edge through itself is degenerate.
        if (PlacingVertexIds.Contains(vertexId)) return;
        PlacingVertexIds.Add(vertexId);
        Notify();
    }

    public void RemoveLastFromPlacement()
    {
        if (!IsPlacingFace || PlacingVertexIds.Count == 0) return;
        PlacingVertexIds.RemoveAt(PlacingVertexIds.Count - 1);
        if (PlacingFanRoot >= PlacingVertexIds.Count) PlacingFanRoot = 0;
        Notify();
    }

    // Flip the picked order. Cheap way to change which side the face will
    // face without re-picking — the arrow flips because the first three
    // vertices are now in opposite winding.
    public void ReversePlacement()
    {
        if (!IsPlacingFace || PlacingVertexIds.Count < 2) return;
        PlacingVertexIds.Reverse();
        PlacingFanRoot = 0;
        Notify();
    }

    public void CancelPlacement()
    {
        IsPlacingFace = false;
        PlacingVertexIds.Clear();
        PlacingFanRoot = 0;
        Notify();
    }

    // Triangulates the placement polygon by fanning from PlacingFanRoot, then
    // creates one Face per resulting triangle (or a single quad for 4 verts,
    // single tri for 3) and exits the tool.
    public List<Face> FinishPlacement()
    {
        var created = new List<Face>();
        if (!IsPlacingFace) return created;
        var verts = PlacingVertexIds.ToList();
        if (verts.Count < 3) return created;

        if (verts.Count == 3 || verts.Count == 4)
        {
            // Keep the polygon as a single face — preserves the user's intent
            // and the OBJ writer is happy to emit n-gons of up to 4 corners.
            created.Add(Mesh.AddFace(verts, materialId: SelectedMaterialId));
        }
        else
        {
            // Fan triangulation. Rotate so the chosen root is at index 0,
            // then emit (root, i, i+1) for i = 1 .. n-2.
            var root = Math.Clamp(PlacingFanRoot, 0, verts.Count - 1);
            var rotated = verts.Skip(root).Concat(verts.Take(root)).ToList();
            for (int i = 1; i < rotated.Count - 1; i++)
            {
                created.Add(Mesh.AddFace(
                    new[] { rotated[0], rotated[i], rotated[i + 1] },
                    materialId: SelectedMaterialId));
            }
        }

        IsPlacingFace = false;
        PlacingVertexIds.Clear();
        PlacingFanRoot = 0;
        Notify();
        return created;
    }

    public Material AddMaterial(string name)
    {
        var m = Mesh.AddMaterial(name);
        SelectedMaterialId ??= m.Id;
        Notify();
        return m;
    }

    // ---- Cut-face tool ----

    public bool BeginCutFace()
    {
        if (SelectedFaces.Count != 1) return false;
        var faceId = SelectedFaces.First();
        if (!Mesh.Faces.TryGetValue(faceId, out var face) || face.VertexIds.Count < 4) return false;
        IsCuttingFace = true;
        CuttingFaceId = faceId;
        CuttingPickedCorners.Clear();
        Notify();
        return true;
    }

    public void CancelCut()
    {
        IsCuttingFace = false;
        CuttingFaceId = 0;
        CuttingPickedCorners.Clear();
        Notify();
    }

    // The viewport hands us a vertex id from picking; map to a corner index on
    // the face being cut. Two non-adjacent corner picks commit the split.
    public void AddCutCornerByVertex(int vertexId)
    {
        if (!IsCuttingFace) return;
        if (!Mesh.Faces.TryGetValue(CuttingFaceId, out var face)) { CancelCut(); return; }
        var cornerIdx = face.VertexIds.IndexOf(vertexId);
        if (cornerIdx < 0) return; // not a corner of the face being cut
        if (CuttingPickedCorners.Contains(cornerIdx)) return;

        CuttingPickedCorners.Add(cornerIdx);

        if (CuttingPickedCorners.Count == 2)
        {
            var a = CuttingPickedCorners[0];
            var b = CuttingPickedCorners[1];
            var n = face.VertexIds.Count;
            // Reject adjacent picks — they would produce a 2-vertex degenerate
            // polygon. Drop the second pick and let the user try another corner.
            if (Math.Abs(a - b) == 1 || Math.Abs(a - b) == n - 1)
            {
                CuttingPickedCorners.RemoveAt(1);
                Notify();
                return;
            }
            CommitCut(face, a, b);
            return;
        }

        Notify();
    }

    private void CommitCut(Face face, int a, int b)
    {
        var lo = Math.Min(a, b);
        var hi = Math.Max(a, b);
        var n = face.VertexIds.Count;

        // Polygon A: corners lo..hi inclusive.
        var vertsA = new List<int>();
        var uvsA = new List<int?>();
        for (int i = lo; i <= hi; i++)
        {
            vertsA.Add(face.VertexIds[i]);
            uvsA.Add(i < face.UvIds.Count ? face.UvIds[i] : null);
        }
        // Polygon B: corners hi..n-1 then 0..lo inclusive. Shares the chord
        // with polygon A so both new faces are properly closed.
        var vertsB = new List<int>();
        var uvsB = new List<int?>();
        for (int i = hi; i < n; i++)
        {
            vertsB.Add(face.VertexIds[i]);
            uvsB.Add(i < face.UvIds.Count ? face.UvIds[i] : null);
        }
        for (int i = 0; i <= lo; i++)
        {
            vertsB.Add(face.VertexIds[i]);
            uvsB.Add(i < face.UvIds.Count ? face.UvIds[i] : null);
        }

        var mat = face.MaterialId;
        Mesh.DeleteFace(face.Id);
        var f1 = Mesh.AddFace(vertsA, uvsA, mat);
        var f2 = Mesh.AddFace(vertsB, uvsB, mat);

        IsCuttingFace = false;
        CuttingFaceId = 0;
        CuttingPickedCorners.Clear();
        SelectedFaces.Clear();
        SelectedFaces.Add(f1.Id);
        SelectedFaces.Add(f2.Id);
        Notify();
    }

    public void AssignMaterialToSelectedFaces(int? materialId)
    {
        foreach (var id in SelectedFaces)
            if (Mesh.Faces.TryGetValue(id, out var f)) f.MaterialId = materialId;
        Notify();
    }

    // Reverse a face's corner order — same vertices, opposite winding, normal
    // flipped. UvIds are reversed in lockstep so each corner keeps its UV.
    public void ReverseFaceWinding(int faceId)
    {
        if (!Mesh.Faces.TryGetValue(faceId, out var f)) return;
        f.VertexIds.Reverse();
        f.UvIds.Reverse();
        Notify();
    }

    public void ReverseSelectedFaces()
    {
        if (SelectedFaces.Count == 0) return;
        foreach (var id in SelectedFaces)
            if (Mesh.Faces.TryGetValue(id, out var f))
            {
                f.VertexIds.Reverse();
                f.UvIds.Reverse();
            }
        Notify();
    }

    public void SetFaceSmoothingGroup(int faceId, int group)
    {
        if (!Mesh.Faces.TryGetValue(faceId, out var f)) return;
        f.SmoothingGroup = Math.Max(0, group);
        Notify();
    }

    public void SetSelectedFacesSmoothingGroup(int group)
    {
        if (SelectedFaces.Count == 0) return;
        var g = Math.Max(0, group);
        foreach (var id in SelectedFaces)
            if (Mesh.Faces.TryGetValue(id, out var f))
                f.SmoothingGroup = g;
        Notify();
    }

    // +1 / -1 nudgers used by the Properties panel arrows. Group never drops
    // below 0 (which is "flat"). Each call clamps and notifies.
    public void AdjustSelectedFacesSmoothingGroup(int delta)
    {
        if (SelectedFaces.Count == 0) return;
        foreach (var id in SelectedFaces)
            if (Mesh.Faces.TryGetValue(id, out var f))
                f.SmoothingGroup = Math.Max(0, f.SmoothingGroup + delta);
        Notify();
    }

    public void SetMaterialName(int id, string name)
    {
        if (!Mesh.Materials.TryGetValue(id, out var m)) return;
        m.Name = string.IsNullOrWhiteSpace(name) ? m.Name : name.Trim();
        Notify();
    }

    public void SetMaterialDiffuse(int id, double r, double g, double b)
    {
        if (!Mesh.Materials.TryGetValue(id, out var m)) return;
        m.Diffuse = (Math.Clamp(r, 0, 1), Math.Clamp(g, 0, 1), Math.Clamp(b, 0, 1));
        Notify();
    }

    public void DeleteMaterial(int id)
    {
        Mesh.DeleteMaterial(id);
        if (SelectedMaterialId == id) SelectedMaterialId = Mesh.Materials.Values.OrderBy(m => m.Id).FirstOrDefault()?.Id;
        Notify();
    }

    public void SetUvForFaceCorner(int faceId, int cornerIndex, Vec2 uv)
    {
        if (!Mesh.Faces.TryGetValue(faceId, out var face)) return;
        if (cornerIndex < 0 || cornerIndex >= face.VertexIds.Count) return;
        while (face.UvIds.Count <= cornerIndex) face.UvIds.Add(null);
        if (face.UvIds[cornerIndex] is int existing && Mesh.Uvs.TryGetValue(existing, out var coord))
            coord.Uv = uv;
        else
            face.UvIds[cornerIndex] = Mesh.AddUv(uv).Id;
        Notify();
    }

    // Replace each (faceId, cornerIndex) corner's UV id with a brand new id
    // at the same position, so subsequent moves don't drag any other corner
    // that previously shared the same id. No-op when the corner has no UV.
    public void DetachUvs(IEnumerable<(int faceId, int corner)> points)
    {
        var changed = false;
        foreach (var (fid, ci) in points)
        {
            if (!Mesh.Faces.TryGetValue(fid, out var face)) continue;
            if (ci < 0 || ci >= face.VertexIds.Count) continue;
            while (face.UvIds.Count <= ci) face.UvIds.Add(null);
            Vec2 current = Vec2.Zero;
            if (face.UvIds[ci] is int existing && Mesh.Uvs.TryGetValue(existing, out var coord))
                current = coord.Uv;
            face.UvIds[ci] = Mesh.AddUv(current).Id;
            changed = true;
        }
        if (changed) Notify();
    }

    // Merge several corner UVs into a single shared id at the average of
    // their current positions. Other corners that previously shared the old
    // ids keep their original ids (they're effectively split off).
    public void ConnectUvs(IEnumerable<(int faceId, int corner)> points)
    {
        var list = points.ToList();
        if (list.Count < 2) return;
        double su = 0, sv = 0;
        int n = 0;
        foreach (var (fid, ci) in list)
        {
            if (!Mesh.Faces.TryGetValue(fid, out var face)) continue;
            if (ci < 0 || ci >= face.UvIds.Count) continue;
            if (face.UvIds[ci] is int existing && Mesh.Uvs.TryGetValue(existing, out var coord))
            { su += coord.Uv.U; sv += coord.Uv.V; n++; }
        }
        var avg = n > 0 ? new Vec2(su / n, sv / n) : Vec2.Zero;
        var shared = Mesh.AddUv(avg);
        foreach (var (fid, ci) in list)
        {
            if (!Mesh.Faces.TryGetValue(fid, out var face)) continue;
            if (ci < 0 || ci >= face.VertexIds.Count) continue;
            while (face.UvIds.Count <= ci) face.UvIds.Add(null);
            face.UvIds[ci] = shared.Id;
        }
        Notify();
    }

    public void SetMaterialTexture(int materialId, string path, byte[] bytes)
    {
        if (!Mesh.Materials.TryGetValue(materialId, out var m)) return;
        m.TexturePath = path;
        m.TextureBytes = bytes;
        Notify();
    }

    // Default scene: unit cube centered at origin so a fresh window shows
    // something. Each face is wound Unity-CW-from-outside and assigned a
    // standard [0,1]² UV quad so a loaded texture is immediately visible
    // (otherwise every corner samples (0,0) and looks flat-coloured).
    private static Mesh SeedCube()
    {
        var m = new Mesh { Name = "cube" };
        var mat = m.AddMaterial("default");
        mat.Diffuse = (0.7, 0.75, 0.85);
        var p = new (double X, double Y, double Z)[]
        {
            (-0.5, -0.5, -0.5), ( 0.5, -0.5, -0.5),
            ( 0.5,  0.5, -0.5), (-0.5,  0.5, -0.5),
            (-0.5, -0.5,  0.5), ( 0.5, -0.5,  0.5),
            ( 0.5,  0.5,  0.5), (-0.5,  0.5,  0.5),
        };
        var verts = p.Select(c => m.AddVertex(new Vec3(c.X, c.Y, c.Z)).Id).ToArray();
        int[][] quads =
        {
            new[] { 3, 2, 1, 0 }, // -Z
            new[] { 6, 7, 4, 5 }, // +Z
            new[] { 7, 3, 0, 4 }, // -X
            new[] { 2, 6, 5, 1 }, // +X
            new[] { 0, 1, 5, 4 }, // -Y
            new[] { 7, 6, 2, 3 }, // +Y
        };
        // Shared UV quad — one corner per face index in the order each quad
        // lists its vertices. (0,0) → (1,0) → (1,1) → (0,1) walks CCW in
        // UV space which matches each quad's vertex order.
        var uvCorners = new (double U, double V)[]
        {
            (0, 0), (1, 0), (1, 1), (0, 1),
        };
        var uvIds = uvCorners.Select(c => m.AddUv(new Vec2(c.U, c.V)).Id).ToArray();
        foreach (var q in quads)
        {
            var face = m.AddFace(q.Select(i => verts[i]), materialId: mat.Id);
            face.UvIds.Clear();
            for (int i = 0; i < 4; i++) face.UvIds.Add(uvIds[i]);
        }
        return m;
    }
}
