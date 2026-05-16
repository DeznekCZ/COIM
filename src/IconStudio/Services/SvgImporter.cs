using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Parses a standard SVG file into a new Scene. Supports rect / circle / ellipse /
// line / polygon / polyline directly. <path> elements are converted to polygons
// only if their `d` attribute uses just M / L / H / V / Z (no curves) — curved
// paths are skipped with a SkippedItems entry the caller can show to the user.
public sealed class SvgImporter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    private static readonly XNamespace SvgNs = "http://www.w3.org/2000/svg";

    public sealed record ImportResult(Scene Scene, List<string> SkippedItems);

    public ImportResult Import(string svgText)
    {
        var doc = XDocument.Parse(svgText);
        var root = doc.Root ?? throw new InvalidDataException("Empty SVG.");
        if (root.Name.LocalName != "svg")
            throw new InvalidDataException("Root element is not <svg>.");

        var scene = new Scene();
        var (vbX, vbY, vbW, vbH) = ParseViewBox(root);
        scene.Width = vbW > 0 ? vbW : 128;
        scene.Height = vbH > 0 ? vbH : 128;

        var skipped = new List<string>();

        // Walk top-level children. Each <g> becomes its own ShapeLayer; loose shapes
        // outside any <g> all go into a single "Imported" layer.
        var orphans = new ShapeLayer { Name = "Imported" };
        foreach (var child in root.Elements())
        {
            if (child.Name.LocalName == "g")
            {
                var groupLayer = new ShapeLayer { Name = child.Attribute("id")?.Value ?? "Group" };
                ImportInto(groupLayer, child, vbX, vbY, skipped);
                if (groupLayer.Shapes.Count > 0) scene.Layers.Add(groupLayer);
            }
            else
            {
                var shape = ParseShape(child, vbX, vbY, skipped);
                if (shape != null) orphans.Shapes.Add(shape);
            }
        }
        if (orphans.Shapes.Count > 0) scene.Layers.Add(orphans);
        if (scene.Layers.Count == 0) scene.EnsureActiveShapeLayer();
        else scene.ActiveLayerId = scene.Layers.Last().Id;

        return new ImportResult(scene, skipped);
    }

    private static void ImportInto(ShapeLayer layer, XElement parent, double vbX, double vbY,
                                   List<string> skipped)
    {
        foreach (var el in parent.Elements())
        {
            if (el.Name.LocalName == "g")
            {
                // Flatten nested groups into the same layer to keep the model simple.
                ImportInto(layer, el, vbX, vbY, skipped);
                continue;
            }
            var shape = ParseShape(el, vbX, vbY, skipped);
            if (shape != null) layer.Shapes.Add(shape);
        }
    }

    private static Shape? ParseShape(XElement el, double vbX, double vbY, List<string> skipped)
    {
        Shape? s = el.Name.LocalName switch
        {
            "rect" => new Shape
            {
                Kind = ShapeKind.Rect,
                X = ParseAttr(el, "x") - vbX,
                Y = ParseAttr(el, "y") - vbY,
                Width = ParseAttr(el, "width"),
                Height = ParseAttr(el, "height"),
                CornerRadius = ParseAttr(el, "rx", 0),
            },
            "circle" => CircleFromAttrs(el, vbX, vbY),
            "ellipse" => EllipseFromAttrs(el, vbX, vbY),
            "line" => new Shape
            {
                Kind = ShapeKind.Line,
                X = ParseAttr(el, "x1") - vbX,
                Y = ParseAttr(el, "y1") - vbY,
                Width = ParseAttr(el, "x2") - ParseAttr(el, "x1"),
                Height = ParseAttr(el, "y2") - ParseAttr(el, "y1"),
                Fill = "none",
            },
            "polygon" => PolygonFromAttrs(el, vbX, vbY, ShapeKind.Polygon),
            "polyline" => PolygonFromAttrs(el, vbX, vbY, ShapeKind.Polyline),
            "path" => PathToPolygon(el, vbX, vbY, skipped),
            _ => null,
        };

        if (s == null)
        {
            if (!IgnorableElement(el.Name.LocalName))
                skipped.Add($"<{el.Name.LocalName}> (unsupported)");
            return null;
        }
        ApplyStyle(s, el);
        return s;
    }

    private static bool IgnorableElement(string name)
        => name is "title" or "desc" or "defs" or "style" or "metadata" or "linearGradient"
           or "radialGradient" or "stop" or "filter" or "mask" or "clipPath" or "pattern";

    private static Shape CircleFromAttrs(XElement el, double vbX, double vbY)
    {
        var cx = ParseAttr(el, "cx") - vbX;
        var cy = ParseAttr(el, "cy") - vbY;
        var r = ParseAttr(el, "r");
        return new Shape
        {
            Kind = ShapeKind.Circle,
            X = cx - r, Y = cy - r,
            Width = r * 2, Height = r * 2,
        };
    }

    private static Shape EllipseFromAttrs(XElement el, double vbX, double vbY)
    {
        var cx = ParseAttr(el, "cx") - vbX;
        var cy = ParseAttr(el, "cy") - vbY;
        var rx = ParseAttr(el, "rx");
        var ry = ParseAttr(el, "ry");
        return new Shape
        {
            Kind = ShapeKind.Circle,
            X = cx - rx, Y = cy - ry,
            Width = rx * 2, Height = ry * 2,
        };
    }

    private static Shape? PolygonFromAttrs(XElement el, double vbX, double vbY, ShapeKind kind)
    {
        var pts = el.Attribute("points")?.Value;
        if (string.IsNullOrWhiteSpace(pts)) return null;
        var nums = NumberToken.Matches(pts)
            .Select(m => double.Parse(m.Value, NumberStyles.Float, Inv))
            .ToArray();
        // Polygon needs at least 3 vertices (6 doubles); polyline just 2 (4 doubles).
        var minDoubles = kind == ShapeKind.Polygon ? 6 : 4;
        if (nums.Length < minDoubles) return null;
        var s = new Shape { Kind = kind };
        if (kind == ShapeKind.Polyline) s.Fill = "none";
        for (int i = 0; i + 1 < nums.Length; i += 2)
            s.Points.Add((nums[i] - vbX, nums[i + 1] - vbY));
        return s;
    }

    // Best-effort path → polygon conversion. Only handles M/L/H/V/Z (uppercase or
    // lowercase). If any curve command is encountered we record it as skipped and
    // bail — we don't have a Path shape kind yet (bezier support is a follow-up).
    private static Shape? PathToPolygon(XElement el, double vbX, double vbY, List<string> skipped)
    {
        var d = el.Attribute("d")?.Value;
        if (string.IsNullOrWhiteSpace(d)) return null;

        if (Regex.IsMatch(d, @"[CcSsQqTtAa]"))
        {
            skipped.Add("<path> with curves (bezier import not yet supported — open as polygon by tracing PNG export instead)");
            return null;
        }

        var pts = new List<(double X, double Y)>();
        double cx = 0, cy = 0, subX = 0, subY = 0;
        char last = '\0';
        int i = 0;
        while (i < d.Length)
        {
            while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
            if (i >= d.Length) break;
            char cmd;
            if ("MmLlHhVvZz".IndexOf(d[i]) >= 0) { cmd = d[i]; i++; }
            else { cmd = last == 'M' ? 'L' : last == 'm' ? 'l' : last; if (cmd == '\0') break; }

            switch (cmd)
            {
                case 'M': case 'm':
                    cx = ReadNum(d, ref i) + (cmd == 'm' ? cx : 0);
                    cy = ReadNum(d, ref i) + (cmd == 'm' ? cy : 0);
                    subX = cx; subY = cy;
                    pts.Add((cx - vbX, cy - vbY));
                    break;
                case 'L': case 'l':
                    cx = ReadNum(d, ref i) + (cmd == 'l' ? cx : 0);
                    cy = ReadNum(d, ref i) + (cmd == 'l' ? cy : 0);
                    pts.Add((cx - vbX, cy - vbY));
                    break;
                case 'H': case 'h':
                    cx = ReadNum(d, ref i) + (cmd == 'h' ? cx : 0);
                    pts.Add((cx - vbX, cy - vbY));
                    break;
                case 'V': case 'v':
                    cy = ReadNum(d, ref i) + (cmd == 'v' ? cy : 0);
                    pts.Add((cx - vbX, cy - vbY));
                    break;
                case 'Z': case 'z':
                    cx = subX; cy = subY;
                    break;
            }
            last = cmd;
        }
        if (pts.Count < 3) return null;
        var s = new Shape { Kind = ShapeKind.Polygon };
        s.Points.AddRange(pts);
        return s;
    }

    private static void ApplyStyle(Shape s, XElement el)
    {
        var styleAttrs = ParseInlineStyle(el.Attribute("style")?.Value);

        string? Get(string key) =>
            styleAttrs.TryGetValue(key, out var v) ? v : el.Attribute(key)?.Value;

        var fill = Get("fill");
        if (!string.IsNullOrEmpty(fill)) s.Fill = NormalizeNone(fill);

        var stroke = Get("stroke");
        if (!string.IsNullOrEmpty(stroke)) s.Stroke = NormalizeNone(stroke);

        var sw = Get("stroke-width");
        if (sw != null && double.TryParse(sw, NumberStyles.Float, Inv, out var swv))
            s.StrokeWidth = swv;

        var op = Get("opacity");
        if (op != null && double.TryParse(op, NumberStyles.Float, Inv, out var opv))
            s.Opacity = opv;
    }

    private static string NormalizeNone(string v)
        => string.Equals(v.Trim(), "none", StringComparison.OrdinalIgnoreCase) ? "none" : v.Trim();

    private static Dictionary<string, string> ParseInlineStyle(string? style)
    {
        var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(style)) return d;
        foreach (var part in style.Split(';'))
        {
            var i = part.IndexOf(':');
            if (i <= 0) continue;
            d[part[..i].Trim()] = part[(i + 1)..].Trim();
        }
        return d;
    }

    private static (double X, double Y, double W, double H) ParseViewBox(XElement root)
    {
        var vb = root.Attribute("viewBox")?.Value;
        if (!string.IsNullOrWhiteSpace(vb))
        {
            var parts = vb.Split(new[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 4 &&
                double.TryParse(parts[0], NumberStyles.Float, Inv, out var x) &&
                double.TryParse(parts[1], NumberStyles.Float, Inv, out var y) &&
                double.TryParse(parts[2], NumberStyles.Float, Inv, out var w) &&
                double.TryParse(parts[3], NumberStyles.Float, Inv, out var h) &&
                w > 0 && h > 0)
            {
                return (x, y, w, h);
            }
        }
        var ww = ParseAttr(root, "width", 0);
        var hh = ParseAttr(root, "height", 0);
        return (0, 0, ww, hh);
    }

    private static double ParseAttr(XElement el, string name, double dflt = 0)
    {
        var raw = el.Attribute(name)?.Value;
        if (string.IsNullOrWhiteSpace(raw)) return dflt;
        var m = NumberToken.Match(raw);
        if (!m.Success) return dflt;
        return double.Parse(m.Value, NumberStyles.Float, Inv);
    }

    private static double ReadNum(string d, ref int i)
    {
        while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
        int start = i;
        if (i < d.Length && (d[i] == '+' || d[i] == '-')) i++;
        while (i < d.Length && (char.IsDigit(d[i]) || d[i] == '.')) i++;
        if (i < d.Length && (d[i] == 'e' || d[i] == 'E'))
        {
            i++;
            if (i < d.Length && (d[i] == '+' || d[i] == '-')) i++;
            while (i < d.Length && char.IsDigit(d[i])) i++;
        }
        if (i == start) return 0;
        return double.Parse(d.AsSpan(start, i - start), NumberStyles.Float, Inv);
    }

    private static readonly Regex NumberToken = new(
        @"[-+]?(\d+\.\d*|\.\d+|\d+)([eE][-+]?\d+)?",
        RegexOptions.Compiled);
}
