using System.Drawing;
using System.Globalization;
using System.Xml.Linq;
using CustomAssets.ModBuilder.Svg;
using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Two modes:
//   AddReferenceLayer  — non-destructive tracing template behind the canvas.
//   TraceToShapes      — run PngToSvgTracer and convert its <path> output into the
//                        scene as polygon shapes the user can edit.
public sealed class PngImportService
{
    public ReferenceImageLayer AddReferenceLayer(Scene scene, byte[] png)
    {
        using var ms = new MemoryStream(png);
        using var bmp = new Bitmap(ms);
        var layer = new ReferenceImageLayer(
            png, bmp.Width, bmp.Height,
            placeX: 0, placeY: 0,
            placeW: scene.Width, placeH: scene.Height);
        scene.Layers.Add(layer);
        return layer;
    }

    public ShapeLayer TraceToShapes(Scene scene, byte[] png,
                                    int maxColors = 4, double simplify = 0.75,
                                    int minRegion = 4, double outlineWidth = 0)
    {
        using var ms = new MemoryStream(png);
        using var bmp = new Bitmap(ms);

        var tracer = new PngToSvgTracer(
            maxColors: maxColors,
            alphaThreshold: 128,
            simplifyEpsilon: simplify,
            minRegionPixels: minRegion,
            outlineColor: Color.Black,
            outlineWidth: outlineWidth);

        string svg = tracer.Trace(bmp);
        var doc = XDocument.Parse(svg);

        // Map tracer's pixel-space viewBox into the scene's coordinate space.
        double srcW = bmp.Width;
        double srcH = bmp.Height;
        double sx = scene.Width / srcW;
        double sy = scene.Height / srcH;

        var layer = new ShapeLayer { Name = $"Traced ({bmp.Width}×{bmp.Height})" };
        scene.Layers.Add(layer);
        scene.ActiveLayerId = layer.Id;

        XNamespace svgNs = "http://www.w3.org/2000/svg";
        foreach (var path in doc.Descendants(svgNs + "path"))
        {
            var d = (string?)path.Attribute("d");
            if (string.IsNullOrWhiteSpace(d)) continue;
            string fill = (string?)path.Attribute("fill") ?? "#000000";
            string stroke = (string?)path.Attribute("stroke") ?? "";
            double strokeW = ParseLen((string?)path.Attribute("stroke-width"));

            foreach (var poly in ExtractPolygons(d!, sx, sy))
            {
                var shape = new Shape
                {
                    Kind = ShapeKind.Polygon,
                    Fill = fill,
                    Stroke = stroke,
                    StrokeWidth = strokeW,
                };
                shape.Points.AddRange(poly);
                layer.Shapes.Add(shape);
            }
        }
        return layer;
    }

    // The tracer's output uses M / L / Z only (no curves), so a simple state machine
    // is enough. Each subpath ("M x y L x y ... Z") becomes one polygon.
    private static IEnumerable<List<(double X, double Y)>> ExtractPolygons(string d, double sx, double sy)
    {
        var current = new List<(double X, double Y)>();
        int i = 0;
        double cx = 0, cy = 0;
        char lastCmd = '\0';
        while (i < d.Length)
        {
            while (i < d.Length && (char.IsWhiteSpace(d[i]) || d[i] == ',')) i++;
            if (i >= d.Length) break;
            char cmd;
            if ("MmLlZzHhVv".IndexOf(d[i]) >= 0) { cmd = d[i]; i++; }
            else { cmd = lastCmd == 'M' ? 'L' : lastCmd == 'm' ? 'l' : lastCmd; }
            switch (cmd)
            {
                case 'M':
                case 'm':
                    if (current.Count > 0) { yield return current; current = new(); }
                    double mx = ReadNum(d, ref i);
                    double my = ReadNum(d, ref i);
                    if (cmd == 'm') { mx += cx; my += cy; }
                    cx = mx; cy = my;
                    current.Add((cx * sx, cy * sy));
                    break;
                case 'L':
                case 'l':
                    double lx = ReadNum(d, ref i);
                    double ly = ReadNum(d, ref i);
                    if (cmd == 'l') { lx += cx; ly += cy; }
                    cx = lx; cy = ly;
                    current.Add((cx * sx, cy * sy));
                    break;
                case 'H': case 'h':
                    double hx = ReadNum(d, ref i);
                    if (cmd == 'h') hx += cx;
                    cx = hx;
                    current.Add((cx * sx, cy * sy));
                    break;
                case 'V': case 'v':
                    double vy = ReadNum(d, ref i);
                    if (cmd == 'v') vy += cy;
                    cy = vy;
                    current.Add((cx * sx, cy * sy));
                    break;
                case 'Z':
                case 'z':
                    if (current.Count > 0) { yield return current; current = new(); }
                    break;
                default:
                    i++;
                    break;
            }
            lastCmd = cmd;
        }
        if (current.Count > 0) yield return current;
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
        return double.Parse(d.Substring(start, i - start), CultureInfo.InvariantCulture);
    }

    private static double ParseLen(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return 0;
        return double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : 0;
    }
}
