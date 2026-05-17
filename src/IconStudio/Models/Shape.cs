using System.Globalization;
using System.Text;

namespace CustomAssets.IconStudio.Models;

public enum ShapeKind { Rect, Circle, Line, Polygon, Polyline }

// Per-edge segment data on polygon / polyline shapes. Segment i describes the
// edge FROM Points[i] TO Points[(i+1) % Points.Count]. For Polyline, the last
// segment (between the closing pair that doesn't exist) is unused.
//
// Kind=Line means a straight edge; Kind=Cubic means a cubic Bezier with two
// control points stored alongside.
public enum SegmentKind { Line, Cubic }

public sealed class SegmentData
{
    public SegmentKind Kind { get; set; } = SegmentKind.Line;
    public (double X, double Y) Control1 { get; set; }
    public (double X, double Y) Control2 { get; set; }

    public SegmentData Clone() => new()
    {
        Kind = Kind,
        Control1 = Control1,
        Control2 = Control2,
    };
}

public sealed class Shape
{
    private static int _next = 1;
    public int Id { get; } = System.Threading.Interlocked.Increment(ref _next);

    public ShapeKind Kind { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public double CornerRadius { get; set; }

    public List<(double X, double Y)> Points { get; } = new();

    // Optional per-edge data. null = all segments are straight lines (default).
    // When non-null, Segments[i] corresponds to the edge starting at Points[i].
    public List<SegmentData>? Segments { get; set; }

    public bool HasCurves
        => Segments != null && Segments.Any(s => s.Kind == SegmentKind.Cubic);

    public string Fill { get; set; } = "#222222";
    public string Stroke { get; set; } = "#000000";
    public double StrokeWidth { get; set; } = 0;
    public double Opacity { get; set; } = 1.0;

    public Shape Clone()
    {
        var s = new Shape
        {
            Kind = Kind,
            X = X, Y = Y, Width = Width, Height = Height,
            CornerRadius = CornerRadius,
            Fill = Fill, Stroke = Stroke,
            StrokeWidth = StrokeWidth, Opacity = Opacity,
        };
        s.Points.AddRange(Points);
        if (Segments != null)
            s.Segments = Segments.Select(seg => seg.Clone()).ToList();
        return s;
    }

    public string ToSvg()
    {
        var inv = CultureInfo.InvariantCulture;
        var attrs = new StringBuilder();
        attrs.Append($" fill=\"{(string.IsNullOrEmpty(Fill) ? "none" : Fill)}\"");
        if (StrokeWidth > 0 && !string.IsNullOrEmpty(Stroke))
        {
            attrs.Append($" stroke=\"{Stroke}\"");
            attrs.Append($" stroke-width=\"{StrokeWidth.ToString(inv)}\"");
        }
        if (Opacity < 1.0)
            attrs.Append($" opacity=\"{Opacity.ToString(inv)}\"");

        switch (Kind)
        {
            case ShapeKind.Rect:
                var rx = CornerRadius > 0
                    ? $" rx=\"{CornerRadius.ToString(inv)}\" ry=\"{CornerRadius.ToString(inv)}\""
                    : "";
                return $"<rect x=\"{X.ToString(inv)}\" y=\"{Y.ToString(inv)}\" width=\"{Width.ToString(inv)}\" height=\"{Height.ToString(inv)}\"{rx}{attrs}/>";

            case ShapeKind.Circle:
                var cx = X + Width / 2;
                var cy = Y + Height / 2;
                return $"<ellipse cx=\"{cx.ToString(inv)}\" cy=\"{cy.ToString(inv)}\" rx=\"{(Width/2).ToString(inv)}\" ry=\"{(Height/2).ToString(inv)}\"{attrs}/>";

            case ShapeKind.Line:
                var x2 = X + Width;
                var y2 = Y + Height;
                return $"<line x1=\"{X.ToString(inv)}\" y1=\"{Y.ToString(inv)}\" x2=\"{x2.ToString(inv)}\" y2=\"{y2.ToString(inv)}\"{attrs}/>";

            case ShapeKind.Polygon:
                if (Points.Count < 2) return "";
                if (HasCurves) return EmitPath(closed: true, attrs.ToString(), inv);
                var pts = string.Join(" ", Points.Select(p => $"{p.X.ToString(inv)},{p.Y.ToString(inv)}"));
                return $"<polygon points=\"{pts}\"{attrs}/>";

            case ShapeKind.Polyline:
                if (Points.Count < 2) return "";
                if (HasCurves) return EmitPath(closed: false, attrs.ToString(), inv);
                var ptsPl = string.Join(" ", Points.Select(p => $"{p.X.ToString(inv)},{p.Y.ToString(inv)}"));
                return $"<polyline points=\"{ptsPl}\"{attrs}/>";
        }
        return "";
    }

    // Emits an SVG <path> for a polygon / polyline that has at least one Cubic
    // segment. Straight edges use L; curved edges use C cp1 cp2 endpoint.
    // Closed (polygon) terminates with Z.
    private string EmitPath(bool closed, string attrs, CultureInfo inv)
    {
        var sb = new StringBuilder();
        sb.Append("<path d=\"M ");
        sb.Append(Points[0].X.ToString(inv));
        sb.Append(',');
        sb.Append(Points[0].Y.ToString(inv));

        int segCount = closed ? Points.Count : Points.Count - 1;
        for (int i = 0; i < segCount; i++)
        {
            var end = Points[(i + 1) % Points.Count];
            var seg = (Segments != null && i < Segments.Count) ? Segments[i] : null;
            if (seg != null && seg.Kind == SegmentKind.Cubic)
            {
                sb.Append(" C ");
                sb.Append(seg.Control1.X.ToString(inv)); sb.Append(','); sb.Append(seg.Control1.Y.ToString(inv));
                sb.Append(' ');
                sb.Append(seg.Control2.X.ToString(inv)); sb.Append(','); sb.Append(seg.Control2.Y.ToString(inv));
                sb.Append(' ');
                sb.Append(end.X.ToString(inv)); sb.Append(','); sb.Append(end.Y.ToString(inv));
            }
            else
            {
                sb.Append(" L ");
                sb.Append(end.X.ToString(inv)); sb.Append(','); sb.Append(end.Y.ToString(inv));
            }
        }
        if (closed) sb.Append(" Z");
        sb.Append("\"").Append(attrs).Append("/>");
        return sb.ToString();
    }
}
