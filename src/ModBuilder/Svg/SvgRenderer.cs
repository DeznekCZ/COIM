using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace CustomAssets.ModBuilder.Svg
{
    // Minimal SVG → PNG rasterizer built on System.Drawing (GDI+).
    //
    // Supports the shape set used by typical game icons: rect (with rx/ry), circle, ellipse,
    // line, polyline, polygon, path; <g> grouping with `transform=`; inherited fill/stroke
    // styling via attributes or CSS-style `style=""`; linear and radial gradients defined in
    // <defs> and referenced via `url(#id)`.
    //
    // NOT supported: <text>, <image>, masks, clip-paths, filters, patterns, external CSS,
    // class-based selectors. Anything unknown is silently skipped — the goal is icon
    // rendering, not faithful SVG playback.
    internal sealed class SvgRenderer
    {
        public int OutputWidth { get; }
        public int OutputHeight { get; }
        public int Supersample { get; }

        // Back-compat — square output. Build pipeline (svg2png) uses this overload.
        public int OutputSize => OutputWidth;

        public SvgRenderer(int outputSize = 128, int supersample = 2)
            : this(outputSize, outputSize, supersample) { }

        // Rectangular overload. The SVG viewBox is letterboxed into the requested
        // pixel rectangle preserving the source aspect ratio.
        public SvgRenderer(int outputWidth, int outputHeight, int supersample = 2)
        {
            if (outputWidth <= 0) throw new ArgumentOutOfRangeException(nameof(outputWidth));
            if (outputHeight <= 0) throw new ArgumentOutOfRangeException(nameof(outputHeight));
            if (supersample <= 0) throw new ArgumentOutOfRangeException(nameof(supersample));
            OutputWidth = outputWidth;
            OutputHeight = outputHeight;
            Supersample = supersample;
        }

        public void RenderToFile(string svgPath, string pngPath)
        {
            using (var bmp = Render(svgPath))
            {
                string dir = Path.GetDirectoryName(pngPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                bmp.Save(pngPath, ImageFormat.Png);
            }
        }

        public Bitmap Render(string svgPath)
        {
            var doc = XDocument.Load(svgPath);
            return Render(doc);
        }

        public Bitmap Render(XDocument doc)
        {
            int internalW = OutputWidth * Supersample;
            int internalH = OutputHeight * Supersample;
            var bmp = new Bitmap(OutputWidth, OutputHeight, PixelFormat.Format32bppArgb);
            using (var hi = new Bitmap(internalW, internalH, PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(hi))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.Transparent);

                var ctx = new RenderContext(g, doc, internalW, internalH);
                ctx.RenderRoot();

                using (var g2 = Graphics.FromImage(bmp))
                {
                    if (Supersample > 1)
                    {
                        g2.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g2.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g2.CompositingQuality = CompositingQuality.HighQuality;
                    }
                    g2.DrawImage(hi, new Rectangle(0, 0, OutputWidth, OutputHeight));
                }
            }
            return bmp;
        }

        // Walks the SVG tree, maintaining inherited style + accumulated transform.
        private sealed class RenderContext
        {
            private readonly Graphics _g;
            private readonly XDocument _doc;
            private readonly int _renderW;
            private readonly int _renderH;
            private readonly Dictionary<string, XElement> _idMap = new Dictionary<string, XElement>();
            private static readonly XNamespace Svg = "http://www.w3.org/2000/svg";

            public RenderContext(Graphics g, XDocument doc, int renderW, int renderH)
            {
                _g = g;
                _doc = doc;
                _renderW = renderW;
                _renderH = renderH;
                IndexIds(doc.Root);
            }

            private void IndexIds(XElement e)
            {
                if (e == null) return;
                var id = e.Attribute("id");
                if (id != null) _idMap[id.Value] = e;
                foreach (var child in e.Elements()) IndexIds(child);
            }

            public void RenderRoot()
            {
                XElement root = _doc.Root;
                if (root == null) return;

                // Map viewBox (or fallback width/height) to the render bitmap, preserving aspect
                // ratio by centering with letterbox space.
                var vb = ParseViewBox(root);
                float scale = Math.Min(_renderW / vb.Width, _renderH / vb.Height);
                float offX = (_renderW - vb.Width * scale) / 2f - vb.X * scale;
                float offY = (_renderH - vb.Height * scale) / 2f - vb.Y * scale;
                var baseTransform = new Matrix(scale, 0, 0, scale, offX, offY);

                var style = new SvgStyle();
                style.ApplyFromElement(root);
                RenderChildren(root, baseTransform, style);
            }

            private void RenderChildren(XElement parent, Matrix parentTransform, SvgStyle parentStyle)
            {
                foreach (var e in parent.Elements())
                {
                    // Skip non-paintable elements that are never directly rendered.
                    if (e.Name.LocalName == "defs" || e.Name.LocalName == "title" ||
                        e.Name.LocalName == "desc" || e.Name.LocalName == "metadata" ||
                        e.Name.LocalName == "style" || e.Name.LocalName == "linearGradient" ||
                        e.Name.LocalName == "radialGradient" || e.Name.LocalName == "stop" ||
                        e.Name.LocalName == "clipPath" || e.Name.LocalName == "mask" ||
                        e.Name.LocalName == "pattern" || e.Name.LocalName == "filter") continue;

                    var localTransform = parentTransform.Clone();
                    var transformAttr = (string)e.Attribute("transform");
                    if (!string.IsNullOrEmpty(transformAttr))
                    {
                        var m = SvgTransform.Parse(transformAttr);
                        localTransform.Multiply(m, MatrixOrder.Prepend);
                    }

                    var style = parentStyle.Clone();
                    style.ApplyFromElement(e);

                    switch (e.Name.LocalName)
                    {
                        case "g":
                            RenderChildren(e, localTransform, style);
                            break;
                        case "rect":
                            DrawRect(e, localTransform, style);
                            break;
                        case "circle":
                            DrawCircle(e, localTransform, style);
                            break;
                        case "ellipse":
                            DrawEllipse(e, localTransform, style);
                            break;
                        case "line":
                            DrawLine(e, localTransform, style);
                            break;
                        case "polyline":
                            DrawPolyline(e, localTransform, style, close: false);
                            break;
                        case "polygon":
                            DrawPolyline(e, localTransform, style, close: true);
                            break;
                        case "path":
                            DrawPath(e, localTransform, style);
                            break;
                        case "svg":
                            // Nested <svg> — treat as a group; viewBox of nested <svg>s is ignored
                            // for simplicity (rare in icons).
                            RenderChildren(e, localTransform, style);
                            break;
                    }
                }
            }

            private RectangleF ParseViewBox(XElement root)
            {
                var vbAttr = (string)root.Attribute("viewBox");
                if (!string.IsNullOrWhiteSpace(vbAttr))
                {
                    var parts = vbAttr.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 4 &&
                        float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                        float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                        float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float w) &&
                        float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out float h) &&
                        w > 0 && h > 0)
                    {
                        return new RectangleF(x, y, w, h);
                    }
                }
                float ww = ParseLength((string)root.Attribute("width"), 100f);
                float hh = ParseLength((string)root.Attribute("height"), 100f);
                if (ww <= 0) ww = 100;
                if (hh <= 0) hh = 100;
                return new RectangleF(0, 0, ww, hh);
            }

            private static float ParseLength(string s, float defaultValue)
            {
                if (string.IsNullOrWhiteSpace(s)) return defaultValue;
                var m = Regex.Match(s, @"^\s*([-+]?\d*\.?\d+(?:[eE][-+]?\d+)?)");
                if (!m.Success) return defaultValue;
                return float.Parse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private void DrawRect(XElement e, Matrix tx, SvgStyle s)
            {
                float x = ParseLength((string)e.Attribute("x"), 0);
                float y = ParseLength((string)e.Attribute("y"), 0);
                float w = ParseLength((string)e.Attribute("width"), 0);
                float h = ParseLength((string)e.Attribute("height"), 0);
                if (w <= 0 || h <= 0) return;
                float rx = ParseLength((string)e.Attribute("rx"), 0);
                float ry = ParseLength((string)e.Attribute("ry"), rx);
                if (rx <= 0 && ry <= 0)
                {
                    var path = new GraphicsPath();
                    path.AddRectangle(new RectangleF(x, y, w, h));
                    PaintPath(path, tx, s, new RectangleF(x, y, w, h));
                    path.Dispose();
                }
                else
                {
                    if (rx > w / 2f) rx = w / 2f;
                    if (ry > h / 2f) ry = h / 2f;
                    var path = MakeRoundedRect(x, y, w, h, rx, ry);
                    PaintPath(path, tx, s, new RectangleF(x, y, w, h));
                    path.Dispose();
                }
            }

            private static GraphicsPath MakeRoundedRect(float x, float y, float w, float h, float rx, float ry)
            {
                var p = new GraphicsPath();
                float dx = rx * 2f;
                float dy = ry * 2f;
                p.StartFigure();
                p.AddArc(x, y, dx, dy, 180, 90);
                p.AddArc(x + w - dx, y, dx, dy, 270, 90);
                p.AddArc(x + w - dx, y + h - dy, dx, dy, 0, 90);
                p.AddArc(x, y + h - dy, dx, dy, 90, 90);
                p.CloseFigure();
                return p;
            }

            private void DrawCircle(XElement e, Matrix tx, SvgStyle s)
            {
                float cx = ParseLength((string)e.Attribute("cx"), 0);
                float cy = ParseLength((string)e.Attribute("cy"), 0);
                float r = ParseLength((string)e.Attribute("r"), 0);
                if (r <= 0) return;
                var path = new GraphicsPath();
                path.AddEllipse(cx - r, cy - r, r * 2, r * 2);
                PaintPath(path, tx, s, new RectangleF(cx - r, cy - r, r * 2, r * 2));
                path.Dispose();
            }

            private void DrawEllipse(XElement e, Matrix tx, SvgStyle s)
            {
                float cx = ParseLength((string)e.Attribute("cx"), 0);
                float cy = ParseLength((string)e.Attribute("cy"), 0);
                float rx = ParseLength((string)e.Attribute("rx"), 0);
                float ry = ParseLength((string)e.Attribute("ry"), 0);
                if (rx <= 0 || ry <= 0) return;
                var path = new GraphicsPath();
                path.AddEllipse(cx - rx, cy - ry, rx * 2, ry * 2);
                PaintPath(path, tx, s, new RectangleF(cx - rx, cy - ry, rx * 2, ry * 2));
                path.Dispose();
            }

            private void DrawLine(XElement e, Matrix tx, SvgStyle s)
            {
                float x1 = ParseLength((string)e.Attribute("x1"), 0);
                float y1 = ParseLength((string)e.Attribute("y1"), 0);
                float x2 = ParseLength((string)e.Attribute("x2"), 0);
                float y2 = ParseLength((string)e.Attribute("y2"), 0);
                var path = new GraphicsPath();
                path.AddLine(x1, y1, x2, y2);
                PaintPath(path, tx, s, RectangleF.FromLTRB(
                    Math.Min(x1, x2), Math.Min(y1, y2), Math.Max(x1, x2), Math.Max(y1, y2)));
                path.Dispose();
            }

            private void DrawPolyline(XElement e, Matrix tx, SvgStyle s, bool close)
            {
                var pointsAttr = (string)e.Attribute("points");
                if (string.IsNullOrWhiteSpace(pointsAttr)) return;
                var nums = new List<float>();
                foreach (Match m in SvgTransform.NumberToken.Matches(pointsAttr))
                {
                    if (float.TryParse(m.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float v))
                        nums.Add(v);
                }
                if (nums.Count < 4) return;
                var pts = new PointF[nums.Count / 2];
                for (int i = 0; i < pts.Length; i++)
                    pts[i] = new PointF(nums[i * 2], nums[i * 2 + 1]);
                var path = new GraphicsPath();
                path.AddLines(pts);
                if (close) path.CloseFigure();
                var bbox = BoundsOf(pts);
                PaintPath(path, tx, s, bbox);
                path.Dispose();
            }

            private static RectangleF BoundsOf(PointF[] pts)
            {
                float minX = pts[0].X, maxX = pts[0].X;
                float minY = pts[0].Y, maxY = pts[0].Y;
                for (int i = 1; i < pts.Length; i++)
                {
                    if (pts[i].X < minX) minX = pts[i].X;
                    if (pts[i].X > maxX) maxX = pts[i].X;
                    if (pts[i].Y < minY) minY = pts[i].Y;
                    if (pts[i].Y > maxY) maxY = pts[i].Y;
                }
                return RectangleF.FromLTRB(minX, minY, maxX, maxY);
            }

            private void DrawPath(XElement e, Matrix tx, SvgStyle s)
            {
                var d = (string)e.Attribute("d");
                if (string.IsNullOrWhiteSpace(d)) return;
                var path = SvgPath.Parse(d);
                if (s.FillRule == "evenodd") path.FillMode = FillMode.Alternate;
                else path.FillMode = FillMode.Winding;
                var bbox = path.GetBounds();
                PaintPath(path, tx, s, bbox);
                path.Dispose();
            }

            private void PaintPath(GraphicsPath path, Matrix tx, SvgStyle s, RectangleF userBBox)
            {
                var saved = _g.Save();
                _g.Transform = tx;
                Brush fillBrush = ResolveFillBrush(s, userBBox);
                if (fillBrush != null)
                {
                    _g.FillPath(fillBrush, path);
                    fillBrush.Dispose();
                }
                Pen strokePen = ResolveStrokePen(s, userBBox);
                if (strokePen != null)
                {
                    _g.DrawPath(strokePen, path);
                    strokePen.Dispose();
                }
                _g.Restore(saved);
            }

            private Brush ResolveFillBrush(SvgStyle s, RectangleF bbox)
            {
                if (string.IsNullOrEmpty(s.Fill)) // default in SVG is "black"
                    return new SolidBrush(SvgColor.ApplyOpacity(Color.Black, s.FillOpacity * s.Opacity));

                if (s.Fill.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
                {
                    string id = ExtractUrlId(s.Fill);
                    if (id != null && _idMap.TryGetValue(id, out var grad))
                    {
                        var brush = BuildGradientBrush(grad, bbox, s.FillOpacity * s.Opacity);
                        if (brush != null) return brush;
                    }
                    return null;
                }

                Color? c = SvgColor.Parse(s.Fill);
                if (c == null) return null;
                return new SolidBrush(SvgColor.ApplyOpacity(c.Value, s.FillOpacity * s.Opacity));
            }

            private Pen ResolveStrokePen(SvgStyle s, RectangleF bbox)
            {
                if (string.IsNullOrEmpty(s.Stroke)) return null;
                if (s.Stroke.StartsWith("url(", StringComparison.OrdinalIgnoreCase))
                {
                    // Gradient-stroked icons are rare; fall back to no stroke if encountered.
                    return null;
                }
                Color? c = SvgColor.Parse(s.Stroke);
                if (c == null) return null;
                var pen = new Pen(SvgColor.ApplyOpacity(c.Value, s.StrokeOpacity * s.Opacity), Math.Max(0.001f, s.StrokeWidth));
                pen.LineJoin = ParseLineJoin(s.StrokeLineJoin);
                pen.StartCap = pen.EndCap = ParseLineCap(s.StrokeLineCap);
                pen.MiterLimit = s.StrokeMiterLimit;
                return pen;
            }

            private static LineJoin ParseLineJoin(string j)
            {
                switch (j)
                {
                    case "round": return LineJoin.Round;
                    case "bevel": return LineJoin.Bevel;
                    default: return LineJoin.Miter;
                }
            }

            private static LineCap ParseLineCap(string c)
            {
                switch (c)
                {
                    case "round": return LineCap.Round;
                    case "square": return LineCap.Square;
                    default: return LineCap.Flat;
                }
            }

            private static string ExtractUrlId(string url)
            {
                // url(#foo) — strip url( ) and a leading #.
                int hash = url.IndexOf('#');
                if (hash < 0) return null;
                int end = url.IndexOf(')', hash);
                if (end < 0) end = url.Length;
                return url.Substring(hash + 1, end - hash - 1).Trim();
            }

            private Brush BuildGradientBrush(XElement grad, RectangleF bbox, double opacityMult)
            {
                // Resolve xlink:href chain — gradients commonly inherit stops from another gradient.
                var stops = CollectStops(grad);
                if (stops.Count < 2) return null;

                // Determine if the gradient is objectBoundingBox (default) or userSpaceOnUse.
                bool userSpace = string.Equals((string)grad.Attribute("gradientUnits"), "userSpaceOnUse", StringComparison.OrdinalIgnoreCase);

                if (grad.Name.LocalName == "linearGradient")
                {
                    float x1 = ParseLength((string)grad.Attribute("x1"), userSpace ? 0 : 0);
                    float y1 = ParseLength((string)grad.Attribute("y1"), 0);
                    float x2 = ParseLength((string)grad.Attribute("x2"), userSpace ? bbox.Right : 1);
                    float y2 = ParseLength((string)grad.Attribute("y2"), 0);
                    if (!userSpace)
                    {
                        x1 = bbox.X + x1 * bbox.Width;
                        y1 = bbox.Y + y1 * bbox.Height;
                        x2 = bbox.X + x2 * bbox.Width;
                        y2 = bbox.Y + y2 * bbox.Height;
                    }
                    // Edge case: zero-length gradient line → flat color.
                    if (Math.Abs(x1 - x2) < 1e-3 && Math.Abs(y1 - y2) < 1e-3)
                    {
                        return new SolidBrush(SvgColor.ApplyOpacity(stops[stops.Count - 1].Color, opacityMult));
                    }
                    var lg = new LinearGradientBrush(new PointF(x1, y1), new PointF(x2, y2), Color.Black, Color.White);
                    lg.InterpolationColors = BuildColorBlend(stops, opacityMult);
                    return lg;
                }
                else if (grad.Name.LocalName == "radialGradient")
                {
                    float cx = ParseLength((string)grad.Attribute("cx"), 0.5f);
                    float cy = ParseLength((string)grad.Attribute("cy"), 0.5f);
                    float r = ParseLength((string)grad.Attribute("r"), 0.5f);
                    if (!userSpace)
                    {
                        cx = bbox.X + cx * bbox.Width;
                        cy = bbox.Y + cy * bbox.Height;
                        r = r * Math.Max(bbox.Width, bbox.Height);
                    }
                    if (r <= 0) return null;
                    var path = new GraphicsPath();
                    path.AddEllipse(cx - r, cy - r, r * 2, r * 2);
                    var pg = new PathGradientBrush(path);
                    pg.CenterPoint = new PointF(cx, cy);
                    // PathGradientBrush colors are CenterColor → SurroundColors. Use first stop
                    // as center and last as edge, with stops in between via InterpolationColors.
                    pg.CenterColor = SvgColor.ApplyOpacity(stops[0].Color, opacityMult);
                    pg.SurroundColors = new[] { SvgColor.ApplyOpacity(stops[stops.Count - 1].Color, opacityMult) };
                    pg.InterpolationColors = BuildColorBlend(stops, opacityMult);
                    path.Dispose();
                    return pg;
                }
                return null;
            }

            private static ColorBlend BuildColorBlend(List<GradientStop> stops, double opacityMult)
            {
                var blend = new ColorBlend(stops.Count);
                for (int i = 0; i < stops.Count; i++)
                {
                    blend.Positions[i] = stops[i].Offset;
                    blend.Colors[i] = SvgColor.ApplyOpacity(stops[i].Color, opacityMult * stops[i].Opacity);
                }
                // Positions must be strictly 0..1 and monotonically non-decreasing for GDI+.
                if (blend.Positions[0] > 0f) blend.Positions[0] = 0f;
                if (blend.Positions[blend.Positions.Length - 1] < 1f) blend.Positions[blend.Positions.Length - 1] = 1f;
                return blend;
            }

            private List<GradientStop> CollectStops(XElement grad)
            {
                var stops = new List<GradientStop>();
                // Follow xlink:href chain if needed (max depth 4 — guards against cycles).
                var current = grad;
                int hops = 0;
                while (current != null && hops < 4)
                {
                    foreach (var stop in current.Elements().Where(x => x.Name.LocalName == "stop"))
                    {
                        var styleAttr = (string)stop.Attribute("style") ?? "";
                        var styleProps = StyleParser.ParseStyleString(styleAttr);
                        string scolor = styleProps.TryGetValue("stop-color", out string sc) ? sc : ((string)stop.Attribute("stop-color") ?? "black");
                        string sopacity = styleProps.TryGetValue("stop-opacity", out string so) ? so : ((string)stop.Attribute("stop-opacity") ?? "1");
                        string soffset = (string)stop.Attribute("offset") ?? "0";

                        Color? c = SvgColor.Parse(scolor) ?? Color.Black;
                        double opacity = ParsePercentOrNumber(sopacity, 1.0);
                        float offset = (float)ParsePercentOrNumber(soffset, 0.0);
                        if (offset < 0) offset = 0;
                        if (offset > 1) offset = 1;
                        stops.Add(new GradientStop { Offset = offset, Color = c.Value, Opacity = opacity });
                    }
                    if (stops.Count > 0) break;
                    // Inherit from xlink:href.
                    XNamespace xlink = "http://www.w3.org/1999/xlink";
                    string href = (string)current.Attribute(xlink + "href") ?? (string)current.Attribute("href");
                    if (string.IsNullOrEmpty(href) || !href.StartsWith("#")) break;
                    string refId = href.Substring(1);
                    if (!_idMap.TryGetValue(refId, out current)) break;
                    hops++;
                }
                stops.Sort((a, b) => a.Offset.CompareTo(b.Offset));
                return stops;
            }

            private static double ParsePercentOrNumber(string s, double defaultValue)
            {
                if (string.IsNullOrWhiteSpace(s)) return defaultValue;
                s = s.Trim();
                if (s.EndsWith("%", StringComparison.Ordinal))
                {
                    return double.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture) / 100.0;
                }
                return double.Parse(s, CultureInfo.InvariantCulture);
            }

        }

        private struct GradientStop
        {
            public float Offset;
            public Color Color;
            public double Opacity;
        }
    }

    // Holds the inheritable subset of SVG presentation attributes. Children clone the parent
    // SvgStyle and apply their own overrides — matching SVG's CSS-like inheritance rules.
    internal sealed class SvgStyle
    {
        // null = inherited / not set; string preserved (incl. "url(#…)" gradient refs).
        public string Fill;
        public string Stroke;
        public float StrokeWidth = 1f;
        public string StrokeLineCap = "butt";
        public string StrokeLineJoin = "miter";
        public float StrokeMiterLimit = 4f;
        public double FillOpacity = 1.0;
        public double StrokeOpacity = 1.0;
        public double Opacity = 1.0;
        public string FillRule = "nonzero";

        public SvgStyle Clone() => (SvgStyle)MemberwiseClone();

        public void ApplyFromElement(System.Xml.Linq.XElement e)
        {
            // Inline style="" wins over discrete attributes, per SVG.
            var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var a in e.Attributes())
            {
                if (string.Equals(a.Name.LocalName, "style", StringComparison.OrdinalIgnoreCase)) continue;
                attrs[a.Name.LocalName] = a.Value;
            }
            foreach (var kv in StyleParser.ParseStyleString((string)e.Attribute("style") ?? ""))
            {
                attrs[kv.Key] = kv.Value;
            }
            if (attrs.TryGetValue("fill", out string v)) Fill = v;
            if (attrs.TryGetValue("stroke", out v)) Stroke = v;
            if (attrs.TryGetValue("stroke-width", out v) && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float sw)) StrokeWidth = sw;
            if (attrs.TryGetValue("stroke-linecap", out v)) StrokeLineCap = v;
            if (attrs.TryGetValue("stroke-linejoin", out v)) StrokeLineJoin = v;
            if (attrs.TryGetValue("stroke-miterlimit", out v) && float.TryParse(v, NumberStyles.Float, CultureInfo.InvariantCulture, out float sml)) StrokeMiterLimit = sml;
            if (attrs.TryGetValue("fill-opacity", out v)) FillOpacity = ParseUnit(v, 1.0);
            if (attrs.TryGetValue("stroke-opacity", out v)) StrokeOpacity = ParseUnit(v, 1.0);
            if (attrs.TryGetValue("opacity", out v)) Opacity = ParseUnit(v, 1.0);
            if (attrs.TryGetValue("fill-rule", out v)) FillRule = v;
        }

        private static double ParseUnit(string s, double defaultValue)
        {
            if (string.IsNullOrWhiteSpace(s)) return defaultValue;
            s = s.Trim();
            if (s.EndsWith("%", StringComparison.Ordinal))
                return double.Parse(s.Substring(0, s.Length - 1), CultureInfo.InvariantCulture) / 100.0;
            return double.Parse(s, CultureInfo.InvariantCulture);
        }
    }

    // Shared `style="key:val;key:val"` parser used both by SvgStyle (per-element style attr)
    // and by SvgRenderer.RenderContext (gradient stops, which also accept inline style).
    internal static class StyleParser
    {
        public static Dictionary<string, string> ParseStyleString(string style)
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (string.IsNullOrWhiteSpace(style)) return d;
            foreach (var part in style.Split(';'))
            {
                int colon = part.IndexOf(':');
                if (colon <= 0) continue;
                string key = part.Substring(0, colon).Trim();
                string val = part.Substring(colon + 1).Trim();
                if (key.Length > 0) d[key] = val;
            }
            return d;
        }
    }
}
