using System;
using UnityEngine;

namespace CustomAssets.Ui.Primitives {

    /// Wireframe / UV-template PNG renderer. Takes the layout polygons that
    /// each shape generator returns, scales them into a target pixel size,
    /// and rasterises each polygon's outline onto a Texture2D. The PNG is
    /// the modder's "texture template" — they paint on top of it.
    ///
    /// No font is used (Unity's TextureRendering would need a managed font
    /// asset). Outlines are enough; the modder learns which face is which
    /// from the dice-cross convention plus the OBJ's group names (Blender
    /// shows them as separate selectable groups).
    public static class PrimitiveWireframe {

        private static readonly Color32 s_lineColor = new Color32(0, 0, 0, 255);
        private static readonly Color32 s_bgColor   = new Color32(255, 255, 255, 255);
        private static readonly Color32 s_fillColor = new Color32(232, 232, 232, 255);

        // Iso-ish view direction used by RenderMesh3D*: 45° yaw + 30° pitch.
        // Captures a 3-quarter view of an axis-aligned shape (so a box shows
        // top + two sides, a cylinder shows the curve on its rim). Same
        // angle as classic CAD axonometric drawings.
        private const double IsoYaw   = Math.PI / 4.0;   // 45°
        private const double IsoPitch = Math.PI / 6.0;   // 30°

        private static readonly Color32 s_faceFillColor = new Color32(220, 224, 232, 255);
        private static readonly Color32 s_edgeColor     = new Color32(60, 60, 80, 255);

        /// <summary>Render the polygons into a square PNG of the given pixel
        /// size. The layout is scaled to fit while preserving aspect; the
        /// remaining area is filled with the background colour.</summary>
        public static byte[] RenderPng(ShapeResult shape, int pixelSize = 1024) {
            if (shape == null || shape.LayoutW <= 0 || shape.LayoutH <= 0) {
                return null;
            }

            Texture2D tex = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, mipChain: false);
            try {
                Color32[] pixels = new Color32[pixelSize * pixelSize];
                for (int i = 0; i < pixels.Length; i++) {
                    pixels[i] = s_bgColor;
                }

                double scale = Math.Min(pixelSize / shape.LayoutW, pixelSize / shape.LayoutH);
                double offX = (pixelSize - shape.LayoutW * scale) * 0.5;
                double offY = (pixelSize - shape.LayoutH * scale) * 0.5;

                // Light fill behind each polygon — helps the modder see where
                // each face lives at a glance even before they paint anything.
                foreach (LayoutPolygon poly in shape.Layout) {
                    fillPolygon(pixels, pixelSize, poly, scale, offX, offY, s_fillColor);
                }
                // Outlines on top.
                foreach (LayoutPolygon poly in shape.Layout) {
                    drawPolygonOutline(pixels, pixelSize, poly, scale, offX, offY);
                }

                tex.SetPixels32(pixels);
                tex.Apply(updateMipmaps: false);
                return tex.EncodeToPNG();
            } finally {
                UnityEngine.Object.Destroy(tex);
            }
        }

        /// <summary>Same as RenderPng but returns the Texture2D for in-window
        /// preview. Caller owns the texture and must destroy it when done.</summary>
        public static Texture2D RenderTexture(ShapeResult shape, int pixelSize = 512) {
            if (shape == null || shape.LayoutW <= 0 || shape.LayoutH <= 0) {
                return null;
            }
            Texture2D tex = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, mipChain: false);
            Color32[] pixels = new Color32[pixelSize * pixelSize];
            for (int i = 0; i < pixels.Length; i++) {
                pixels[i] = s_bgColor;
            }

            double scale = Math.Min(pixelSize / shape.LayoutW, pixelSize / shape.LayoutH);
            double offX = (pixelSize - shape.LayoutW * scale) * 0.5;
            double offY = (pixelSize - shape.LayoutH * scale) * 0.5;

            foreach (LayoutPolygon poly in shape.Layout) {
                fillPolygon(pixels, pixelSize, poly, scale, offX, offY, s_fillColor);
            }
            foreach (LayoutPolygon poly in shape.Layout) {
                drawPolygonOutline(pixels, pixelSize, poly, scale, offX, offY);
            }

            tex.SetPixels32(pixels);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply(updateMipmaps: false);
            return tex;
        }

        // ---- Drawing primitives ---------------------------------------------

        private static void drawPolygonOutline(Color32[] pixels, int size, LayoutPolygon poly,
                double scale, double offX, double offY) {
            if (poly == null || poly.Points.Count < 2) return;
            for (int i = 0; i < poly.Points.Count; i++) {
                int j = (i + 1) % poly.Points.Count;
                int x0 = (int)Math.Round(poly.Points[i].X * scale + offX);
                int y0 = (int)Math.Round(poly.Points[i].Y * scale + offY);
                int x1 = (int)Math.Round(poly.Points[j].X * scale + offX);
                int y1 = (int)Math.Round(poly.Points[j].Y * scale + offY);
                drawLine(pixels, size, x0, y0, x1, y1, s_lineColor);
            }
        }

        private static void drawLine(Color32[] pixels, int size, int x0, int y0, int x1, int y1,
                Color32 col) {
            // Bresenham. Y is flipped (Texture2D's y=0 is bottom; layout y=0
            // is top) so we invert as we write.
            int dx = Math.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0);
            int sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true) {
                plot(pixels, size, x0, y0, col);
                // Thicken to 2px — a single-pixel line is hard to read in
                // most image editors at 100% zoom.
                plot(pixels, size, x0 + 1, y0, col);
                plot(pixels, size, x0, y0 + 1, col);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        private static void plot(Color32[] pixels, int size, int x, int y, Color32 col) {
            if (x < 0 || x >= size || y < 0 || y >= size) return;
            int py = size - 1 - y; // flip to texture's bottom-origin layout
            pixels[py * size + x] = col;
        }

        /// Scanline fill for the polygon's interior. Convex / simple polygons
        /// only (which is all our layouts emit — rectangles + triangles).
        private static void fillPolygon(Color32[] pixels, int size, LayoutPolygon poly,
                double scale, double offX, double offY, Color32 col) {
            if (poly == null || poly.Points.Count < 3) return;

            int n = poly.Points.Count;
            int[] xs = new int[n];
            int[] ys = new int[n];
            int minY = int.MaxValue, maxY = int.MinValue;
            for (int i = 0; i < n; i++) {
                xs[i] = (int)Math.Round(poly.Points[i].X * scale + offX);
                ys[i] = (int)Math.Round(poly.Points[i].Y * scale + offY);
                if (ys[i] < minY) minY = ys[i];
                if (ys[i] > maxY) maxY = ys[i];
            }
            if (minY < 0) minY = 0;
            if (maxY >= size) maxY = size - 1;

            int[] nodes = new int[n];
            for (int y = minY; y <= maxY; y++) {
                int count = 0;
                int j = n - 1;
                for (int i = 0; i < n; i++) {
                    if ((ys[i] < y && ys[j] >= y) || (ys[j] < y && ys[i] >= y)) {
                        double t = (double)(y - ys[i]) / (ys[j] - ys[i]);
                        nodes[count++] = (int)Math.Round(xs[i] + t * (xs[j] - xs[i]));
                    }
                    j = i;
                }
                Array.Sort(nodes, 0, count);
                for (int k = 0; k + 1 < count; k += 2) {
                    int a = nodes[k];
                    int b = nodes[k + 1];
                    if (a < 0) a = 0;
                    if (b >= size) b = size - 1;
                    for (int x = a; x <= b; x++) {
                        plot(pixels, size, x, y, col);
                    }
                }
            }
        }

        // ---- 3D mesh thumbnail ----------------------------------------------

        /// <summary>Render <paramref name="mesh"/> as a flat-shaded
        /// axonometric thumbnail. Painter's algorithm with face-centroid
        /// depth sort: paint back-to-front so closer faces overwrite
        /// farther ones. A simple lambert against a fixed light direction
        /// shades each face so the silhouette reads as a 3D object instead
        /// of a flat outline. UV data is ignored — only positions and
        /// face indices matter.</summary>
        public static Texture2D RenderMesh3DTexture(PrimitiveMesh mesh, int pixelSize) {
            byte[] _;
            return renderMesh3D(mesh, pixelSize, out _, encodePng: false);
        }

        /// <summary>Same as RenderMesh3DTexture but returns the encoded
        /// PNG bytes (and destroys the intermediate texture). Suitable
        /// for writing alongside the .obj on disk.</summary>
        public static byte[] RenderMesh3DPng(PrimitiveMesh mesh, int pixelSize) {
            Texture2D tex = renderMesh3D(mesh, pixelSize, out byte[] png, encodePng: true);
            if (tex != null) UnityEngine.Object.Destroy(tex);
            return png;
        }

        private static Texture2D renderMesh3D(PrimitiveMesh mesh, int pixelSize,
                out byte[] encodedPng, bool encodePng) {
            encodedPng = null;
            if (mesh == null || mesh.Positions.Count == 0 || mesh.Faces.Count == 0) {
                return null;
            }

            double cy = Math.Cos(IsoYaw);
            double sy = Math.Sin(IsoYaw);
            double cp = Math.Cos(IsoPitch);
            double sp = Math.Sin(IsoPitch);

            // Project every position to (sx, sy, depth). Depth grows toward
            // the camera so the painter's sort can ascend.
            int n = mesh.Positions.Count;
            double[] px = new double[n];
            double[] py = new double[n];
            double[] pz = new double[n];
            double minX = double.MaxValue, maxX = double.MinValue;
            double minY = double.MaxValue, maxY = double.MinValue;
            for (int i = 0; i < n; i++) {
                V3 p = mesh.Positions[i];
                // Yaw around Y, then pitch around X. Camera sits in +Z
                // direction looking toward the origin; the rotated z' is
                // also our depth value (higher = farther from camera).
                double x1 =  p.X * cy + p.Z * sy;
                double z1 = -p.X * sy + p.Z * cy;
                double y2 =  p.Y * cp - z1 * sp;
                double z2 =  p.Y * sp + z1 * cp;
                px[i] = x1;
                py[i] = y2;
                pz[i] = z2;
                if (x1 < minX) minX = x1;
                if (x1 > maxX) maxX = x1;
                if (y2 < minY) minY = y2;
                if (y2 > maxY) maxY = y2;
            }

            // Auto-fit into the texture with a small margin.
            double margin = pixelSize * 0.06;
            double extentX = Math.Max(maxX - minX, 1e-6);
            double extentY = Math.Max(maxY - minY, 1e-6);
            double scale = Math.Min(
                (pixelSize - 2 * margin) / extentX,
                (pixelSize - 2 * margin) / extentY);
            double cx = (pixelSize - (minX + maxX) * scale) * 0.5;
            double cyOff = (pixelSize - (minY + maxY) * scale) * 0.5;

            int[] sx = new int[n];
            int[] syPix = new int[n];
            for (int i = 0; i < n; i++) {
                sx[i]   = (int)Math.Round(px[i] * scale + cx);
                syPix[i]= (int)Math.Round(py[i] * scale + cyOff);
            }

            // Background pixels.
            Color32[] pixels = new Color32[pixelSize * pixelSize];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = s_bgColor;

            // Painter's algorithm: sort faces by max depth descending
            // (farthest first). Using max-depth handles long thin faces
            // (e.g. cylinder caps seen edge-on) better than centroid depth.
            int faceCount = mesh.Faces.Count;
            int[] order = new int[faceCount];
            double[] depths = new double[faceCount];
            for (int i = 0; i < faceCount; i++) {
                order[i] = i;
                PrimFace f = mesh.Faces[i];
                double maxDepth = double.MinValue;
                for (int k = 0; k < f.PositionIdx.Count; k++) {
                    int idx = f.PositionIdx[k];
                    if (idx < 0 || idx >= n) continue;
                    if (pz[idx] > maxDepth) maxDepth = pz[idx];
                }
                depths[i] = maxDepth;
            }
            Array.Sort(depths, order);
            Array.Reverse(order); // farthest faces first

            // Fixed light direction (normalised). Lambert dot against the
            // face normal in WORLD space, mapped to a tonal range that
            // keeps even back-lit faces visible.
            double lx = 0.4, ly = 0.8, lz = 0.45;
            double lLen = Math.Sqrt(lx * lx + ly * ly + lz * lz);
            lx /= lLen; ly /= lLen; lz /= lLen;

            foreach (int fIdx in order) {
                PrimFace f = mesh.Faces[fIdx];
                if (f.PositionIdx.Count < 3) continue;

                // World-space normal via Newell's method (robust for
                // n-gons; we don't assume planarity).
                double nxN = 0, nyN = 0, nzN = 0;
                int corners = f.PositionIdx.Count;
                for (int k = 0; k < corners; k++) {
                    int aIdx = f.PositionIdx[k];
                    int bIdx = f.PositionIdx[(k + 1) % corners];
                    if (aIdx < 0 || aIdx >= n || bIdx < 0 || bIdx >= n) continue;
                    V3 a = mesh.Positions[aIdx];
                    V3 b = mesh.Positions[bIdx];
                    nxN += (a.Y - b.Y) * (a.Z + b.Z);
                    nyN += (a.Z - b.Z) * (a.X + b.X);
                    nzN += (a.X - b.X) * (a.Y + b.Y);
                }
                double normLen = Math.Sqrt(nxN * nxN + nyN * nyN + nzN * nzN);
                if (normLen < 1e-9) continue;
                nxN /= normLen; nyN /= normLen; nzN /= normLen;

                double lambert = nxN * lx + nyN * ly + nzN * lz;
                // Remap [-1, 1] to a comfortable [0.45, 1.0] so even faces
                // facing away from the light keep enough contrast to read.
                double tone = 0.45 + 0.55 * (lambert * 0.5 + 0.5);
                if (tone < 0.0) tone = 0.0;
                if (tone > 1.0) tone = 1.0;
                byte r = (byte)(s_faceFillColor.r * tone);
                byte g = (byte)(s_faceFillColor.g * tone);
                byte bCh = (byte)(s_faceFillColor.b * tone);
                Color32 faceCol = new Color32(r, g, bCh, 255);

                // Fan-triangulate convex polygons into the scanline filler.
                // PrimitiveShapes never emits concave faces — boxes + cyl
                // caps + heap triangles are all convex — so a fan is enough.
                LayoutPolygon poly = new LayoutPolygon();
                for (int k = 0; k < corners; k++) {
                    int idx = f.PositionIdx[k];
                    if (idx < 0 || idx >= n) continue;
                    poly.Points.Add((sx[idx], syPix[idx]));
                }
                if (poly.Points.Count >= 3) {
                    fillPolygon(pixels, pixelSize, poly, 1.0, 0.0, 0.0, faceCol);
                }

                // Edge outlines on top of the fill, in a slightly darker
                // hue so they read cleanly against the shaded face.
                for (int k = 0; k < corners; k++) {
                    int aIdx = f.PositionIdx[k];
                    int bIdx = f.PositionIdx[(k + 1) % corners];
                    if (aIdx < 0 || aIdx >= n || bIdx < 0 || bIdx >= n) continue;
                    drawLine(pixels, pixelSize,
                        sx[aIdx], syPix[aIdx], sx[bIdx], syPix[bIdx], s_edgeColor);
                }
            }

            Texture2D tex = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, mipChain: false);
            tex.SetPixels32(pixels);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply(updateMipmaps: false);

            if (encodePng) {
                encodedPng = tex.EncodeToPNG();
            }
            return tex;
        }
    }
}
