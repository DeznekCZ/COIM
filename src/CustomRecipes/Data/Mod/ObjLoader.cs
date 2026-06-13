using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Mafi;
using UnityEngine;

namespace CustomAssets.Data.Mod
{
    /// Minimal Wavefront OBJ loader. Reads positions (`v`), texcoords (`vt`), normals (`vn`)
    /// and faces (`f`) and produces a Unity Mesh suitable for unit-product prefabs (single
    /// mesh, single submesh — extracted as MeshFilter.sharedMesh by ProductsRenderer).
    /// Ignores material directives (`mtllib`, `usemtl`), grouping (`o`, `g`, `s`), and
    /// anything else: unit-product materials come from `add_unit_prefab`'s texture args, not
    /// from the .mtl file.
    /// Faces with more than 3 vertices are fan-triangulated. OBJ's per-channel index arrays
    /// (different positions/uvs/normals can share a face vertex) are de-duplicated into
    /// Unity's unified vertex format on the fly.
    internal static class ObjLoader
    {
        /// <param name="reverseWinding">
        /// When <c>false</c> (default), face corners are passed through unchanged —
        /// correct for OBJ files exported by Blender / Maya / 3ds Max (CCW-from-outside),
        /// which Unity treats as front-facing. Set to <c>true</c> only when the source
        /// .obj is authored CW (rare; some legacy tools or hand-written files) and the
        /// mesh would otherwise render inside-out.
        /// </param>
        public static Mesh LoadFromFile(string fullPath, bool reverseWinding = false)
        {
            if (!File.Exists(fullPath))
            {
                Log.Warning($"ObjLoader: file not found '{fullPath}'");
                return null;
            }
            try
            {
                using (var reader = new StreamReader(fullPath))
                    return Parse(reader, Path.GetFileNameWithoutExtension(fullPath), reverseWinding);
            }
            catch (Exception ex)
            {
                Log.Warning($"ObjLoader: failed to parse '{fullPath}': {ex.Message}");
                return null;
            }
        }

        private static Mesh Parse(TextReader reader, string name, bool reverseWinding = false)
        {
            var positions = new List<Vector3>();
            var uvs       = new List<Vector2>();
            var normals   = new List<Vector3>();

            // (posIdx, uvIdx, normIdx) → unified vertex index. -1 on a channel means absent.
            var unified  = new Dictionary<long, int>();
            var outVerts = new List<Vector3>();
            var outUVs   = new List<Vector2>();
            var outNorms = new List<Vector3>();
            var outTris  = new List<int>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                int hash = line.IndexOf('#');
                string trimmed = (hash >= 0 ? line.Substring(0, hash) : line).Trim();
                if (trimmed.Length == 0) continue;
                string[] tok = trimmed.Split(s_ws, StringSplitOptions.RemoveEmptyEntries);
                if (tok.Length == 0) continue;

                switch (tok[0])
                {
                    case "v":
                        if (tok.Length < 4) continue;
                        positions.Add(new Vector3(F(tok[1]), F(tok[2]), F(tok[3])));
                        break;
                    case "vn":
                        if (tok.Length < 4) continue;
                        normals.Add(new Vector3(F(tok[1]), F(tok[2]), F(tok[3])));
                        break;
                    case "vt":
                        if (tok.Length < 3) continue;
                        uvs.Add(new Vector2(F(tok[1]), F(tok[2])));
                        break;
                    case "f":
                        int n = tok.Length - 1;
                        if (n < 3) continue;
                        var faceIdx = new int[n];
                        for (int i = 0; i < n; i++)
                            faceIdx[i] = ResolveFaceVertex(tok[i + 1], positions, uvs, normals,
                                                          unified, outVerts, outUVs, outNorms);
                        // Fan-triangulation. Default (reverseWinding=false) passes the
                        // source corner order through (0, i, i+1) — correct for standard
                        // OBJ files (Blender / Maya / 3ds Max emit CCW-from-outside, which
                        // Unity treats as front-facing). Flipped path (0, i+1, i) is the
                        // escape hatch for CW-authored OBJs that would otherwise render
                        // inside-out.
                        if (reverseWinding)
                        {
                            for (int i = 1; i < n - 1; i++)
                            {
                                outTris.Add(faceIdx[0]);
                                outTris.Add(faceIdx[i + 1]);
                                outTris.Add(faceIdx[i]);
                            }
                        }
                        else
                        {
                            for (int i = 1; i < n - 1; i++)
                            {
                                outTris.Add(faceIdx[0]);
                                outTris.Add(faceIdx[i]);
                                outTris.Add(faceIdx[i + 1]);
                            }
                        }
                        break;
                }
            }

            var mesh = new Mesh { name = name };
            mesh.SetVertices(outVerts);
            mesh.SetUVs(0, outUVs);
            mesh.SetTriangles(outTris, 0);
            // Use authored normals only when at least one `vn` was present in the file.
            // Mixed (some face vertices with normal, some without) leaves zero normals in the
            // gaps, which would render black — RecalculateNormals is the safer fallback.
            if (normals.Count > 0)
                mesh.SetNormals(outNorms);
            else
                mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        // Resolve a face-vertex token like "12/4/3", "12//3", "12/4", or "12" into a unified
        // vertex index. Negative OBJ indices are relative to the end of the corresponding
        // channel list.
        private static int ResolveFaceVertex(string token,
            List<Vector3> positions, List<Vector2> uvs, List<Vector3> normals,
            Dictionary<long, int> unified,
            List<Vector3> outVerts, List<Vector2> outUVs, List<Vector3> outNorms)
        {
            string[] parts = token.Split('/');
            int posIdx  = ResolveIdx(parts[0], positions.Count);
            int uvIdx   = parts.Length > 1 && parts[1].Length > 0 ? ResolveIdx(parts[1], uvs.Count) : -1;
            int normIdx = parts.Length > 2 && parts[2].Length > 0 ? ResolveIdx(parts[2], normals.Count) : -1;

            // Pack (posIdx, uvIdx, normIdx) into a single 64-bit key. 21 bits per channel covers
            // ~2M entries — far more than any sane unit-product mesh.
            long key = ((long)(posIdx + 1) & 0x1FFFFF)
                     | (((long)(uvIdx + 1) & 0x1FFFFF) << 21)
                     | (((long)(normIdx + 1) & 0x1FFFFF) << 42);

            if (unified.TryGetValue(key, out int idx)) return idx;
            idx = outVerts.Count;
            unified[key] = idx;

            outVerts.Add(posIdx >= 0 && posIdx < positions.Count ? positions[posIdx] : Vector3.zero);
            outUVs.Add(uvIdx >= 0 && uvIdx < uvs.Count ? uvs[uvIdx] : Vector2.zero);
            outNorms.Add(normIdx >= 0 && normIdx < normals.Count ? normals[normIdx] : Vector3.zero);
            return idx;
        }

        private static int ResolveIdx(string s, int count)
        {
            int v = int.Parse(s, CultureInfo.InvariantCulture);
            return v > 0 ? v - 1 : count + v;
        }

        private static float F(string s) => float.Parse(s, CultureInfo.InvariantCulture);

        private static readonly char[] s_ws = { ' ', '\t' };
    }
}
