using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Text;

namespace CustomAssets.ModBuilder.Svg
{
    // PNG → SVG vectorizer. Quantizes the input to a small palette, finds each connected
    // region of each color, traces its outer boundary, simplifies the polyline, and emits
    // an SVG `<path>` element per region.
    //
    // Output is polygonal — no curve fitting — which keeps the implementation simple and
    // works well for stylized / cartoony icons. Photographic input quantizes to muddy
    // blocks and traces poorly; that's not the intended use case.
    internal sealed class PngToSvgTracer
    {
        public int MaxColors { get; }
        public int AlphaThreshold { get; }
        public double SimplifyEpsilon { get; }
        public int MinRegionPixels { get; }
        public Color OutlineColor { get; }
        public double OutlineWidth { get; }

        public PngToSvgTracer(int maxColors = 4, int alphaThreshold = 128,
                              double simplifyEpsilon = 0.75, int minRegionPixels = 4,
                              Color? outlineColor = null, double outlineWidth = 1.5)
        {
            if (maxColors < 1) throw new ArgumentOutOfRangeException(nameof(maxColors));
            MaxColors = maxColors;
            AlphaThreshold = alphaThreshold;
            SimplifyEpsilon = simplifyEpsilon;
            MinRegionPixels = minRegionPixels;
            OutlineColor = outlineColor ?? Color.Black;
            OutlineWidth = outlineWidth;
        }

        public void TraceToFile(string pngPath, string svgPath)
        {
            using (var bmp = new Bitmap(pngPath))
            {
                string svg = Trace(bmp);
                string dir = Path.GetDirectoryName(svgPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(svgPath, svg);
            }
        }

        public string Trace(Bitmap bmp)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            // labels[i] = index into palette, or -1 for transparent
            int[] labels;
            Color[] palette;
            ExtractAndQuantize(bmp, out labels, out palette);

            var sb = new StringBuilder();
            sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            sb.AppendFormat(CultureInfo.InvariantCulture,
                "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {0} {1}\" width=\"{0}\" height=\"{1}\">\n",
                w, h);

            // Bucket sizes per color band (transparent excluded).
            var bandSizes = new int[palette.Length];
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] >= 0) bandSizes[labels[i]]++;

            // Layered emit:
            //   1) silhouette filled with the dominant (largest-area) color, no stroke;
            //   2) every other color band painted on top as a borderless fill;
            //   3) silhouette outline drawn LAST with stroke-only, no fill.
            // This produces a clean black outline around the whole shape and seamless
            // transitions between inner color regions — the inner fills sit on top of
            // the dominant-color base, so any tracer-introduced seam between two
            // adjacent inner regions just exposes the base color rather than a gap.

            // 1) silhouette = "any opaque pixel". Encode as labels-style int[] (0 inside,
            //    -1 outside) so the existing FindConnectedRegions / TraceBoundary work.
            var silhouette = new int[labels.Length];
            for (int i = 0; i < labels.Length; i++) silhouette[i] = labels[i] >= 0 ? 0 : -1;

            var silhouetteRegions = FindConnectedRegions(silhouette, w, h, 0);
            var silhouettePaths = new List<List<PointF>>();
            foreach (var region in silhouetteRegions)
            {
                if (region.PixelCount < MinRegionPixels) continue;
                var outline = TraceBoundary(silhouette, w, h, 0, region.SeedX, region.SeedY);
                if (outline == null || outline.Count < 3) continue;
                // The boundary tracer walks pixel CENTERS, so the resulting polygon lies
                // half a pixel inside the actual icon edge. Outset by +0.5px along each
                // edge's outward normal so the stroke lands on the real pixel boundary
                // instead of cutting into the silhouette.
                var simplified = DouglasPeucker(outline, SimplifyEpsilon);
                silhouettePaths.Add(OutsetPolygon(simplified, 0.5f));
            }

            if (silhouettePaths.Count == 0 || palette.Length == 0)
            {
                sb.Append("</svg>\n");
                return sb.ToString();
            }

            // Dominant color = largest band.
            int backgroundBand = 0;
            for (int i = 1; i < bandSizes.Length; i++)
                if (bandSizes[i] > bandSizes[backgroundBand]) backgroundBand = i;

            // 1) Silhouette fill in dominant color.
            foreach (var path in silhouettePaths)
            {
                sb.Append("    <path fill=\"");
                sb.Append(ColorToHex(palette[backgroundBand]));
                if (palette[backgroundBand].A < 255)
                {
                    sb.Append("\" fill-opacity=\"");
                    sb.Append((palette[backgroundBand].A / 255.0).ToString("0.###", CultureInfo.InvariantCulture));
                }
                sb.Append("\" d=\"");
                AppendPolygonPath(sb, path);
                sb.Append("\"/>\n");
            }

            // 2) Other color bands, descending by area so smaller details paint on top.
            var order = new int[palette.Length];
            for (int i = 0; i < order.Length; i++) order[i] = i;
            Array.Sort(order, (a, b) => bandSizes[b].CompareTo(bandSizes[a]));

            foreach (int bandIdx in order)
            {
                if (bandIdx == backgroundBand) continue;
                if (bandSizes[bandIdx] < MinRegionPixels) continue;
                var regions = FindConnectedRegions(labels, w, h, bandIdx);
                foreach (var region in regions)
                {
                    if (region.PixelCount < MinRegionPixels) continue;
                    var outer = TraceBoundary(labels, w, h, bandIdx, region.SeedX, region.SeedY);
                    if (outer == null || outer.Count < 3) continue;
                    var simplified = DouglasPeucker(outer, SimplifyEpsilon);
                    if (PolygonAreaAbs(simplified) < 0.5) continue;
                    sb.Append("    <path fill=\"");
                    sb.Append(ColorToHex(palette[bandIdx]));
                    if (palette[bandIdx].A < 255)
                    {
                        sb.Append("\" fill-opacity=\"");
                        sb.Append((palette[bandIdx].A / 255.0).ToString("0.###", CultureInfo.InvariantCulture));
                    }
                    sb.Append("\" d=\"");
                    AppendPolygonPath(sb, simplified);
                    sb.Append("\"/>\n");
                }
            }

            // 3) Silhouette outline last so it sits on top of the inner fills.
            if (OutlineWidth > 0)
            {
                foreach (var path in silhouettePaths)
                {
                    sb.Append("    <path fill=\"none\" stroke=\"");
                    sb.Append(ColorToHex(OutlineColor));
                    sb.Append("\" stroke-width=\"");
                    sb.Append(OutlineWidth.ToString("0.##", CultureInfo.InvariantCulture));
                    sb.Append("\" stroke-linejoin=\"round\" stroke-linecap=\"round\" d=\"");
                    AppendPolygonPath(sb, path);
                    sb.Append("\"/>\n");
                }
            }

            sb.Append("</svg>\n");
            return sb.ToString();
        }

        // ---- Quantization ---------------------------------------------------------------

        // Pull pixel data into managed arrays and quantize. Transparent pixels (alpha
        // below AlphaThreshold) get label -1 and are excluded from the palette.
        private void ExtractAndQuantize(Bitmap bmp, out int[] labels, out Color[] palette)
        {
            int w = bmp.Width;
            int h = bmp.Height;
            int total = w * h;
            var pixels = new int[total]; // packed ARGB
            using (var rgba = bmp.Clone(new Rectangle(0, 0, w, h), PixelFormat.Format32bppArgb))
            {
                var data = rgba.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                try
                {
                    System.Runtime.InteropServices.Marshal.Copy(data.Scan0, pixels, 0, total);
                }
                finally { rgba.UnlockBits(data); }
            }

            // Build a histogram of opaque colors. To keep the palette stable we snap to a
            // 6-bit-per-channel grid (64^3 = 262144 buckets) before counting. Most icons
            // have far fewer than 64 distinct color "intentions" so this is plenty.
            var hist = new Dictionary<int, int>();
            for (int i = 0; i < total; i++)
            {
                int argb = pixels[i];
                int a = (argb >> 24) & 0xFF;
                if (a < AlphaThreshold) continue;
                int r = (argb >> 16) & 0xFF;
                int g = (argb >> 8) & 0xFF;
                int b = argb & 0xFF;
                int snapped = (Snap(r) << 16) | (Snap(g) << 8) | Snap(b);
                hist.TryGetValue(snapped, out int c);
                hist[snapped] = c + 1;
            }

            // Seed the palette with the top-MaxColors most-frequent histogram buckets.
            var sorted = new List<KeyValuePair<int, int>>(hist);
            sorted.Sort((x, y) => y.Value.CompareTo(x.Value));
            int paletteSize = Math.Min(MaxColors, sorted.Count);
            palette = new Color[paletteSize];
            for (int i = 0; i < paletteSize; i++)
            {
                int rgb = sorted[i].Key;
                palette[i] = Color.FromArgb(255, (rgb >> 16) & 0xFF, (rgb >> 8) & 0xFF, rgb & 0xFF);
            }

            labels = new int[total];

            // Refine the seed palette with k-means. The naive "histogram top-N + nearest"
            // approach has a failure mode: anti-aliased fringe pixels between two true
            // colors form their own histogram entries, which either steal a palette slot
            // for a meaningless mid-tone or bias the nearest-match toward whichever side
            // they happen to be closer to in RGB. K-means iteratively pulls the palette
            // centers to the mean of their assigned pixels, so the centers settle on the
            // actual visual colors and fringe pixels are split correctly between them.
            //
            // 8 iterations is overkill for typical icons (usually converges in 3-4) but
            // is cheap at this resolution and guarantees stability.
            const int KMeansIterations = 8;
            var sumR = new long[palette.Length];
            var sumG = new long[palette.Length];
            var sumB = new long[palette.Length];
            var counts = new long[palette.Length];

            for (int iter = 0; iter < KMeansIterations; iter++)
            {
                Array.Clear(sumR, 0, sumR.Length);
                Array.Clear(sumG, 0, sumG.Length);
                Array.Clear(sumB, 0, sumB.Length);
                Array.Clear(counts, 0, counts.Length);

                for (int i = 0; i < total; i++)
                {
                    int argb = pixels[i];
                    int a = (argb >> 24) & 0xFF;
                    if (a < AlphaThreshold) { labels[i] = -1; continue; }
                    int r = (argb >> 16) & 0xFF;
                    int g = (argb >> 8) & 0xFF;
                    int b = argb & 0xFF;
                    int best = 0;
                    int bestD = int.MaxValue;
                    for (int p = 0; p < palette.Length; p++)
                    {
                        int dr = palette[p].R - r;
                        int dg = palette[p].G - g;
                        int db = palette[p].B - b;
                        int d = dr * dr + dg * dg + db * db;
                        if (d < bestD) { bestD = d; best = p; }
                    }
                    labels[i] = best;
                    counts[best]++;
                    sumR[best] += r; sumG[best] += g; sumB[best] += b;
                }

                bool changed = false;
                for (int p = 0; p < palette.Length; p++)
                {
                    if (counts[p] == 0) continue;
                    int newR = (int)(sumR[p] / counts[p]);
                    int newG = (int)(sumG[p] / counts[p]);
                    int newB = (int)(sumB[p] / counts[p]);
                    if (palette[p].R != newR || palette[p].G != newG || palette[p].B != newB)
                    {
                        palette[p] = Color.FromArgb(255, newR, newG, newB);
                        changed = true;
                    }
                }
                if (!changed) break;
            }
        }

        // Snap an 8-bit channel to 6 bits (multiples of 4). Quantization grid that ignores
        // tiny anti-alias variation while still distinguishing intended colors.
        private static int Snap(int channel) => channel & 0xFC;

        // ---- Connected components -------------------------------------------------------

        private struct RegionSeed
        {
            public int SeedX;
            public int SeedY;
            public int PixelCount;
        }

        // Flood-fill scan to find one seed per connected region of `band` in `labels`.
        // The seed is the topmost-leftmost pixel of each component, which is convenient
        // for the boundary tracer (it can walk clockwise from a known-leftmost start).
        //
        // 8-CONNECTED on purpose: the boundary tracer (TraceBoundary) uses Moore neighbor-
        // hoods, so it can step diagonally between pixels. If we flood-filled with only
        // 4-connectivity, two diagonally-touching pixels would be separate "regions" here
        // but the tracer would walk across the diagonal — emitting two overlapping paths
        // for what's visually one shape. Keeping connectivity consistent fixes that.
        private static List<RegionSeed> FindConnectedRegions(int[] labels, int w, int h, int band)
        {
            var seeds = new List<RegionSeed>();
            var seen = new bool[labels.Length];
            var stack = new Stack<int>();
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (seen[idx] || labels[idx] != band) continue;
                    int sx = x, sy = y; // topmost-leftmost by traversal order
                    int count = 0;
                    stack.Clear();
                    stack.Push(idx);
                    seen[idx] = true;
                    while (stack.Count > 0)
                    {
                        int p = stack.Pop();
                        count++;
                        int px = p % w;
                        int py = p / w;
                        // 8-connected neighbors (orthogonal + diagonal).
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            int ny = py + dy;
                            if (ny < 0 || ny >= h) continue;
                            for (int dx = -1; dx <= 1; dx++)
                            {
                                if (dx == 0 && dy == 0) continue;
                                int nx = px + dx;
                                if (nx < 0 || nx >= w) continue;
                                Push(stack, seen, labels, band, ny * w + nx);
                            }
                        }
                    }
                    seeds.Add(new RegionSeed { SeedX = sx, SeedY = sy, PixelCount = count });
                }
            }
            return seeds;
        }

        private static void Push(Stack<int> stack, bool[] seen, int[] labels, int band, int idx)
        {
            if (!seen[idx] && labels[idx] == band)
            {
                seen[idx] = true;
                stack.Push(idx);
            }
        }

        // ---- Boundary tracing -----------------------------------------------------------

        // Walk the outer boundary of a region starting from a known-leftmost seed pixel.
        // The output is a sequence of integer pixel-corner points (PointF with .0 / .5
        // precision since we emit half-pixel corner positions for smoother SVG fills).
        //
        // Algorithm: Moore-neighbor tracing. At each boundary pixel, scan its 8 neighbors
        // clockwise starting just after the previous neighbor; the first one that's also
        // in the region becomes the next boundary pixel. Termination: return to start
        // having approached from the same direction (Jacob's stopping criterion).
        private static List<PointF> TraceBoundary(int[] labels, int w, int h, int band, int seedX, int seedY)
        {
            // 8-connected neighbor offsets clockwise starting from "west" (left of pixel).
            // Index N+1 of the neighbor we came from is where we resume scanning.
            int[] nx = { -1, -1, 0, 1, 1, 1, 0, -1 };
            int[] ny = { 0, -1, -1, -1, 0, 1, 1, 1 };

            int startIdx = seedY * w + seedX;
            if (labels[startIdx] != band) return null;

            // Approach direction: since seed is the topmost-leftmost pixel of the region,
            // we approached it from the west (neighbor index 0). Start scan at index 0+1=1.
            int curX = seedX, curY = seedY;
            int backtrack = 0; // came from west

            var points = new List<PointF>();
            EmitCorners(points, curX, curY);

            int safety = 8 * (w * h) + 4;
            while (safety-- > 0)
            {
                int foundDir = -1;
                for (int k = 1; k <= 8; k++)
                {
                    int dir = (backtrack + k) & 7;
                    int tx = curX + nx[dir];
                    int ty = curY + ny[dir];
                    if (tx < 0 || ty < 0 || tx >= w || ty >= h) continue;
                    if (labels[ty * w + tx] == band)
                    {
                        foundDir = dir;
                        // We will leave (curX, curY) and enter (tx, ty). The new "came from"
                        // direction is opposite of `dir`, +4 mod 8.
                        backtrack = (dir + 4) & 7;
                        curX = tx;
                        curY = ty;
                        break;
                    }
                }
                if (foundDir == -1)
                {
                    // Isolated pixel — emit its 4 corners as a 1x1 square and stop.
                    break;
                }
                EmitCorners(points, curX, curY);
                if (curX == seedX && curY == seedY)
                {
                    // Visited start again. Done.
                    break;
                }
            }
            return points;
        }

        // Emit a polygon-corner approximation: the pixel's center +/- (0.5, 0.5). For a
        // tracer this coarse, just placing one point per pixel center is enough — Douglas-
        // Peucker collapses straight runs anyway.
        private static void EmitCorners(List<PointF> outList, int x, int y)
        {
            outList.Add(new PointF(x + 0.5f, y + 0.5f));
        }

        // ---- Douglas-Peucker simplifier -------------------------------------------------

        private static List<PointF> DouglasPeucker(List<PointF> points, double epsilon)
        {
            if (points.Count < 3) return new List<PointF>(points);
            var keep = new bool[points.Count];
            keep[0] = true;
            keep[points.Count - 1] = true;
            DPRecurse(points, 0, points.Count - 1, epsilon, keep);
            var output = new List<PointF>();
            for (int i = 0; i < points.Count; i++)
                if (keep[i]) output.Add(points[i]);
            return output;
        }

        private static void DPRecurse(List<PointF> pts, int i0, int i1, double epsilon, bool[] keep)
        {
            if (i1 <= i0 + 1) return;
            double maxDist = 0;
            int maxIdx = i0;
            PointF a = pts[i0];
            PointF b = pts[i1];
            double dx = b.X - a.X, dy = b.Y - a.Y;
            double len = Math.Sqrt(dx * dx + dy * dy);
            for (int i = i0 + 1; i < i1; i++)
            {
                double d;
                if (len < 1e-9)
                {
                    double ex = pts[i].X - a.X;
                    double ey = pts[i].Y - a.Y;
                    d = Math.Sqrt(ex * ex + ey * ey);
                }
                else
                {
                    d = Math.Abs((pts[i].X - a.X) * dy - (pts[i].Y - a.Y) * dx) / len;
                }
                if (d > maxDist) { maxDist = d; maxIdx = i; }
            }
            if (maxDist > epsilon)
            {
                keep[maxIdx] = true;
                DPRecurse(pts, i0, maxIdx, epsilon, keep);
                DPRecurse(pts, maxIdx, i1, epsilon, keep);
            }
        }

        // ---- SVG emit helpers -----------------------------------------------------------

        private static void AppendPolygonPath(StringBuilder sb, List<PointF> points)
        {
            if (points.Count == 0) return;
            sb.Append('M');
            sb.Append(points[0].X.ToString("0.##", CultureInfo.InvariantCulture));
            sb.Append(' ');
            sb.Append(points[0].Y.ToString("0.##", CultureInfo.InvariantCulture));
            for (int i = 1; i < points.Count; i++)
            {
                sb.Append(" L");
                sb.Append(points[i].X.ToString("0.##", CultureInfo.InvariantCulture));
                sb.Append(' ');
                sb.Append(points[i].Y.ToString("0.##", CultureInfo.InvariantCulture));
            }
            sb.Append(" Z");
        }

        private static string ColorToHex(Color c) =>
            string.Format(CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}", c.R, c.G, c.B);

        // Absolute signed-area of a closed polygon via the shoelace formula. Used to
        // reject degenerate "polygons" that are really thin slivers from single-pixel-wide
        // regions — they trace out a line back-and-forth and have ~0 enclosed area, so
        // they'd render as a stroke-less invisible path anyway.
        private static double PolygonAreaAbs(List<PointF> pts)
        {
            int n = pts.Count;
            if (n < 3) return 0;
            double area = 0;
            for (int i = 0; i < n; i++)
            {
                var p = pts[i];
                var q = pts[(i + 1) % n];
                area += (double)p.X * q.Y - (double)q.X * p.Y;
            }
            return Math.Abs(area) * 0.5;
        }

        // Shift each vertex of a closed polygon outward along the bisector of its two
        // adjacent edges. Used to push the pixel-center-traced boundary out to the real
        // pixel-boundary edge (offset = 0.5 px).
        //
        // Assumes clockwise winding in image coordinates (y goes down) — which is what
        // TraceBoundary produces when seeded at the topmost-leftmost pixel. Outward
        // normal of edge (a→b) is therefore the right-perpendicular: (dy, -dx)/|edge|.
        //
        // The miter offset along the bisector is `2 / (1 + dot(n_in, n_out))` times the
        // edge offset, which becomes huge for sharp convex corners — clamped to a 5x
        // miter limit to avoid stray spikes on a single-pixel protrusion.
        private static List<PointF> OutsetPolygon(List<PointF> poly, float offset)
        {
            int n = poly.Count;
            if (n < 3) return new List<PointF>(poly);

            var normals = new PointF[n];
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                float dx = poly[j].X - poly[i].X;
                float dy = poly[j].Y - poly[i].Y;
                float len = (float)Math.Sqrt(dx * dx + dy * dy);
                if (len < 1e-6f) { normals[i] = new PointF(0, 0); continue; }
                normals[i] = new PointF(dy / len, -dx / len);
            }

            var result = new List<PointF>(n);
            const float MiterFloor = 0.4f; // ~5x miter limit
            for (int i = 0; i < n; i++)
            {
                int prev = (i + n - 1) % n;
                var nIn = normals[prev];
                var nOut = normals[i];
                float sumX = nIn.X + nOut.X;
                float sumY = nIn.Y + nOut.Y;
                float lenSum = (float)Math.Sqrt(sumX * sumX + sumY * sumY);
                if (lenSum < 1e-6f)
                {
                    // 180° reversal — no meaningful bisector; keep vertex put.
                    result.Add(poly[i]);
                    continue;
                }
                float dot = nIn.X * nOut.X + nIn.Y * nOut.Y;
                float miter = 1f + dot;
                if (miter < MiterFloor) miter = MiterFloor;
                float ox = poly[i].X + sumX * offset / miter;
                float oy = poly[i].Y + sumY * offset / miter;
                result.Add(new PointF(ox, oy));
            }
            return result;
        }
    }
}
