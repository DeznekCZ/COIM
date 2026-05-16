using System.Drawing.Imaging;
using System.Xml.Linq;
using CustomAssets.ModBuilder.Svg;
using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Renders the current Scene to a PNG byte buffer using the same SvgRenderer the
// build pipeline uses. The Blazor UI converts the bytes to a data: URL for the
// preview <img> in the export dialog.
public sealed class PngPreviewService
{
    private readonly SvgExporter _exporter;

    public PngPreviewService(SvgExporter exporter)
    {
        _exporter = exporter;
    }

    public byte[] RenderPng(Scene scene, int size, int supersample = 2)
        => RenderPng(scene, size, size, supersample);

    public byte[] RenderPng(Scene scene, int width, int height, int supersample = 2)
    {
        string svg = _exporter.Export(scene);
        var doc = XDocument.Parse(svg);
        var renderer = new SvgRenderer(width, height, supersample);
        using var bmp = renderer.Render(doc);
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public string RenderPngDataUrl(Scene scene, int size, int supersample = 2)
        => "data:image/png;base64," + Convert.ToBase64String(RenderPng(scene, size, supersample));

    public string RenderPngDataUrl(Scene scene, int width, int height, int supersample = 2)
        => "data:image/png;base64," + Convert.ToBase64String(RenderPng(scene, width, height, supersample));
}
