using System.Globalization;
using System.Text;

namespace CustomAssets.IconStudio.Models;

public enum ShapeKind { Rect, Circle, Line, Polygon, Polyline }

public sealed class Shape
{
    private static int _next = 1;
    public int Id { get; } = System.Threading.Interlocked.Increment(ref _next);

    // Settable rather than init-only so PropertyPanel's "Convert to…" buttons
    // can mutate a shape in place — keeping its Id (and therefore its place in
    // the active layer, selection, etc.) across conversion.
    public ShapeKind Kind { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    public double Width { get; set; }
    public double Height { get; set; }

    public double CornerRadius { get; set; }

    public List<(double X, double Y)> Points { get; } = new();

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
                var r = Math.Min(Width, Height) / 2;
                return $"<ellipse cx=\"{cx.ToString(inv)}\" cy=\"{cy.ToString(inv)}\" rx=\"{(Width/2).ToString(inv)}\" ry=\"{(Height/2).ToString(inv)}\"{attrs}/>";

            case ShapeKind.Line:
                var x2 = X + Width;
                var y2 = Y + Height;
                return $"<line x1=\"{X.ToString(inv)}\" y1=\"{Y.ToString(inv)}\" x2=\"{x2.ToString(inv)}\" y2=\"{y2.ToString(inv)}\"{attrs}/>";

            case ShapeKind.Polygon:
                if (Points.Count < 2) return "";
                var pts = string.Join(" ", Points.Select(p => $"{p.X.ToString(inv)},{p.Y.ToString(inv)}"));
                return $"<polygon points=\"{pts}\"{attrs}/>";

            case ShapeKind.Polyline:
                if (Points.Count < 2) return "";
                var ptsPl = string.Join(" ", Points.Select(p => $"{p.X.ToString(inv)},{p.Y.ToString(inv)}"));
                return $"<polyline points=\"{ptsPl}\"{attrs}/>";
        }
        return "";
    }
}
