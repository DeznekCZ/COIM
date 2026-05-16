using System.Globalization;
using System.Xml.Linq;
using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Round-trip Scene ↔ XML format. Reference image PNGs are inlined as base64 so
// a project file is self-contained (no companion folder). Format is versioned
// so we can evolve schema without breaking existing files.
public sealed class ProjectSerializer
{
    private const string FormatVersion = "1";
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public string Save(Scene scene)
    {
        var root = new XElement("IconStudioProject",
            new XAttribute("version", FormatVersion),
            new XElement("Scene",
                new XAttribute("width", scene.Width.ToString(Inv)),
                new XAttribute("height", scene.Height.ToString(Inv)),
                scene.ActiveLayerId is int aid ? new XAttribute("activeLayerId", aid) : null,
                new XElement("Layers", scene.Layers.Select(SerializeLayer))));

        var doc = new XDocument(new XDeclaration("1.0", "utf-8", null), root);
        return doc.ToString();
    }

    public Scene Load(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidDataException("Empty project file.");
        if (root.Name != "IconStudioProject")
            throw new InvalidDataException($"Not an IconStudio project (root <{root.Name}>).");

        var scene = new Scene();
        var sceneEl = root.Element("Scene") ?? throw new InvalidDataException("Missing <Scene>.");
        scene.Width = ParseD(sceneEl.Attribute("width")?.Value, 128);
        scene.Height = ParseD(sceneEl.Attribute("height")?.Value, 128);

        foreach (var layerEl in sceneEl.Element("Layers")?.Elements() ?? Enumerable.Empty<XElement>())
        {
            var layer = DeserializeLayer(layerEl);
            if (layer != null) scene.Layers.Add(layer);
        }

        var actStr = sceneEl.Attribute("activeLayerId")?.Value;
        if (int.TryParse(actStr, NumberStyles.Integer, Inv, out int active) &&
            scene.Layers.Any(l => l.Id == active))
        {
            scene.ActiveLayerId = active;
        }
        else
        {
            scene.ActiveLayerId = scene.Layers.FirstOrDefault()?.Id;
        }
        if (scene.Layers.Count == 0) scene.EnsureActiveShapeLayer();
        return scene;
    }

    private static XElement SerializeLayer(Layer layer) => layer switch
    {
        ShapeLayer sl => new XElement("ShapeLayer",
            new XAttribute("name", sl.Name),
            new XAttribute("visible", sl.Visible),
            new XAttribute("opacity", sl.Opacity.ToString(Inv)),
            sl.Shapes.Select(SerializeShape)),

        ReferenceImageLayer ri => new XElement("ReferenceImageLayer",
            new XAttribute("name", ri.Name),
            new XAttribute("visible", ri.Visible),
            new XAttribute("opacity", ri.Opacity.ToString(Inv)),
            new XAttribute("x", ri.X.ToString(Inv)),
            new XAttribute("y", ri.Y.ToString(Inv)),
            new XAttribute("w", ri.Width.ToString(Inv)),
            new XAttribute("h", ri.Height.ToString(Inv)),
            new XAttribute("pixelWidth", ri.PixelWidth),
            new XAttribute("pixelHeight", ri.PixelHeight),
            new XElement("Png", Convert.ToBase64String(ri.PngBytes))),

        _ => new XElement("UnknownLayer"),
    };

    private static XElement SerializeShape(Shape s)
    {
        // Common style attrs that every shape carries.
        XElement el = s.Kind switch
        {
            ShapeKind.Rect => new XElement("Rect",
                new XAttribute("x", s.X.ToString(Inv)),
                new XAttribute("y", s.Y.ToString(Inv)),
                new XAttribute("w", s.Width.ToString(Inv)),
                new XAttribute("h", s.Height.ToString(Inv)),
                s.CornerRadius > 0 ? new XAttribute("r", s.CornerRadius.ToString(Inv)) : null),
            ShapeKind.Circle => new XElement("Circle",
                new XAttribute("x", s.X.ToString(Inv)),
                new XAttribute("y", s.Y.ToString(Inv)),
                new XAttribute("w", s.Width.ToString(Inv)),
                new XAttribute("h", s.Height.ToString(Inv))),
            ShapeKind.Line => new XElement("Line",
                new XAttribute("x", s.X.ToString(Inv)),
                new XAttribute("y", s.Y.ToString(Inv)),
                new XAttribute("w", s.Width.ToString(Inv)),
                new XAttribute("h", s.Height.ToString(Inv))),
            ShapeKind.Polygon => new XElement("Polygon",
                s.Points.Select(p => new XElement("Point",
                    new XAttribute("x", p.X.ToString(Inv)),
                    new XAttribute("y", p.Y.ToString(Inv))))),
            ShapeKind.Polyline => new XElement("Polyline",
                s.Points.Select(p => new XElement("Point",
                    new XAttribute("x", p.X.ToString(Inv)),
                    new XAttribute("y", p.Y.ToString(Inv))))),
            _ => new XElement("Unknown"),
        };

        if (!string.IsNullOrEmpty(s.Fill)) el.Add(new XAttribute("fill", s.Fill));
        if (!string.IsNullOrEmpty(s.Stroke)) el.Add(new XAttribute("stroke", s.Stroke));
        if (s.StrokeWidth > 0) el.Add(new XAttribute("strokeWidth", s.StrokeWidth.ToString(Inv)));
        if (s.Opacity < 1.0) el.Add(new XAttribute("opacity", s.Opacity.ToString(Inv)));
        return el;
    }

    private static Layer? DeserializeLayer(XElement el)
    {
        switch (el.Name.LocalName)
        {
            case "ShapeLayer":
            {
                var sl = new ShapeLayer
                {
                    Name = el.Attribute("name")?.Value ?? "Layer",
                    Visible = ParseB(el.Attribute("visible")?.Value, true),
                    Opacity = ParseD(el.Attribute("opacity")?.Value, 1.0),
                };
                foreach (var shapeEl in el.Elements())
                {
                    var shape = DeserializeShape(shapeEl);
                    if (shape != null) sl.Shapes.Add(shape);
                }
                return sl;
            }
            case "ReferenceImageLayer":
            {
                var b64 = el.Element("Png")?.Value ?? "";
                if (string.IsNullOrWhiteSpace(b64)) return null;
                var bytes = Convert.FromBase64String(b64.Trim());
                var pw = (int)ParseD(el.Attribute("pixelWidth")?.Value, 0);
                var ph = (int)ParseD(el.Attribute("pixelHeight")?.Value, 0);
                var ri = new ReferenceImageLayer(bytes, pw, ph,
                    ParseD(el.Attribute("x")?.Value, 0),
                    ParseD(el.Attribute("y")?.Value, 0),
                    ParseD(el.Attribute("w")?.Value, 128),
                    ParseD(el.Attribute("h")?.Value, 128))
                {
                    Name = el.Attribute("name")?.Value ?? "Reference image",
                    Visible = ParseB(el.Attribute("visible")?.Value, true),
                    Opacity = ParseD(el.Attribute("opacity")?.Value, 0.5),
                };
                return ri;
            }
            default:
                return null;
        }
    }

    private static Shape? DeserializeShape(XElement el)
    {
        Shape s;
        switch (el.Name.LocalName)
        {
            case "Rect":
                s = new Shape
                {
                    Kind = ShapeKind.Rect,
                    X = ParseD(el.Attribute("x")?.Value, 0),
                    Y = ParseD(el.Attribute("y")?.Value, 0),
                    Width = ParseD(el.Attribute("w")?.Value, 0),
                    Height = ParseD(el.Attribute("h")?.Value, 0),
                    CornerRadius = ParseD(el.Attribute("r")?.Value, 0),
                };
                break;
            case "Circle":
                s = new Shape
                {
                    Kind = ShapeKind.Circle,
                    X = ParseD(el.Attribute("x")?.Value, 0),
                    Y = ParseD(el.Attribute("y")?.Value, 0),
                    Width = ParseD(el.Attribute("w")?.Value, 0),
                    Height = ParseD(el.Attribute("h")?.Value, 0),
                };
                break;
            case "Line":
                s = new Shape
                {
                    Kind = ShapeKind.Line,
                    X = ParseD(el.Attribute("x")?.Value, 0),
                    Y = ParseD(el.Attribute("y")?.Value, 0),
                    Width = ParseD(el.Attribute("w")?.Value, 0),
                    Height = ParseD(el.Attribute("h")?.Value, 0),
                };
                break;
            case "Polygon":
                s = new Shape { Kind = ShapeKind.Polygon };
                foreach (var p in el.Elements("Point"))
                {
                    s.Points.Add((
                        ParseD(p.Attribute("x")?.Value, 0),
                        ParseD(p.Attribute("y")?.Value, 0)));
                }
                if (s.Points.Count < 3) return null;
                break;
            case "Polyline":
                s = new Shape { Kind = ShapeKind.Polyline, Fill = "none" };
                foreach (var p in el.Elements("Point"))
                {
                    s.Points.Add((
                        ParseD(p.Attribute("x")?.Value, 0),
                        ParseD(p.Attribute("y")?.Value, 0)));
                }
                if (s.Points.Count < 2) return null;
                break;
            default:
                return null;
        }
        s.Fill = el.Attribute("fill")?.Value ?? s.Fill;
        s.Stroke = el.Attribute("stroke")?.Value ?? s.Stroke;
        s.StrokeWidth = ParseD(el.Attribute("strokeWidth")?.Value, 0);
        s.Opacity = ParseD(el.Attribute("opacity")?.Value, 1.0);
        return s;
    }

    private static double ParseD(string? v, double dflt)
        => double.TryParse(v, NumberStyles.Float, Inv, out var d) ? d : dflt;

    private static bool ParseB(string? v, bool dflt)
        => bool.TryParse(v, out var b) ? b : dflt;
}
