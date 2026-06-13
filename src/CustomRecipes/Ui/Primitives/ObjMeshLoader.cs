using System.Collections.Generic;
using System.Globalization;

namespace CustomAssets.Ui.Primitives {

    /// Minimal Wavefront OBJ parser used by the in-game thumbnail renderer.
    /// Reads only what the thumbnail needs — vertex positions and face
    /// vertex indices. UVs, normals, smoothing groups, materials, and mtllib
    /// references are tokenised away. Faces with non-integer / out-of-range
    /// indices are skipped; the rendered preview prefers a partial mesh
    /// over a hard parse error.
    public static class ObjMeshLoader {

        /// <summary>Parse the given OBJ source into a <see cref="PrimitiveMesh"/>
        /// containing only positions + face position-indices. UVs are left
        /// empty so the 3D renderer doesn't accidentally try to UV-shade
        /// a wireframe.</summary>
        public static PrimitiveMesh Parse(string objText) {
            PrimitiveMesh mesh = new PrimitiveMesh { Name = "loaded" };
            if (string.IsNullOrEmpty(objText)) return mesh;

            List<V3> positions = new List<V3>();
            foreach (string rawLine in objText.Split('\n')) {
                string line = rawLine.TrimEnd('\r', ' ', '\t');
                if (line.Length == 0 || line[0] == '#') continue;
                string[] parts = line.Split(
                    (char[])null,
                    System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0]) {
                    case "v":
                        if (parts.Length >= 4) {
                            positions.Add(new V3(
                                parseDouble(parts[1]),
                                parseDouble(parts[2]),
                                parseDouble(parts[3])));
                        }
                        break;

                    case "f":
                        if (parts.Length < 4) break;  // need a triangle minimum
                        List<int> corners = new List<int>(parts.Length - 1);
                        for (int i = 1; i < parts.Length; i++) {
                            // OBJ corner: "v", "v/vt", "v//vn", "v/vt/vn".
                            // Only the leading v slot is meaningful here.
                            string token = parts[i];
                            int slash = token.IndexOf('/');
                            string head = slash < 0 ? token : token.Substring(0, slash);
                            int idx = resolveIndex(head, positions.Count);
                            if (idx >= 0) corners.Add(idx);
                        }
                        if (corners.Count >= 3) {
                            mesh.AddFace(corners.ToArray(), null);
                        }
                        break;
                }
            }

            // Push the positions into the mesh in their declared order so
            // face indices line up with what we wrote on AddFace above.
            foreach (V3 p in positions) mesh.AddVertex(p);
            return mesh;
        }

        // OBJ allows negative indices (-1 = last vertex). Returns the
        // 0-based index into the position list, or -1 when the token is
        // unparseable / out of range.
        private static int resolveIndex(string token, int count) {
            if (!int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) {
                return -1;
            }
            if (n > 0 && n <= count) return n - 1;
            if (n < 0 && count + n >= 0) return count + n;
            return -1;
        }

        private static double parseDouble(string s) {
            return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)
                ? v : 0;
        }
    }
}
