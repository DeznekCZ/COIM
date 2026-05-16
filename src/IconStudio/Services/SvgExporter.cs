using System.Globalization;
using System.Text;
using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

public sealed class SvgExporter
{
    public string Export(Scene scene)
    {
        var inv = CultureInfo.InvariantCulture;
        var sb = new StringBuilder();
        sb.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
        sb.AppendFormat(inv,
            "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 {0} {1}\" width=\"{0}\" height=\"{1}\">\n",
            scene.Width, scene.Height);

        // Reference images are tracing-only and are intentionally NOT serialized.
        foreach (var layer in scene.Layers)
        {
            if (!layer.Visible) continue;
            if (layer is not ShapeLayer sl) continue;
            if (sl.Shapes.Count == 0) continue;

            var opacityAttr = layer.Opacity < 1.0
                ? $" opacity=\"{layer.Opacity.ToString(inv)}\""
                : "";
            sb.AppendFormat(inv, "  <g id=\"layer-{0}\"{1}>\n", layer.Id, opacityAttr);
            foreach (var s in sl.Shapes)
            {
                var line = s.ToSvg();
                if (!string.IsNullOrEmpty(line))
                    sb.Append("    ").Append(line).Append('\n');
            }
            sb.Append("  </g>\n");
        }

        sb.Append("</svg>\n");
        return sb.ToString();
    }
}
