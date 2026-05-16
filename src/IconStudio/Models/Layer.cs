namespace CustomAssets.IconStudio.Models;

public abstract class Layer
{
    private static int _next = 1;
    public int Id { get; } = System.Threading.Interlocked.Increment(ref _next);

    public string Name { get; set; } = "Layer";
    public bool Visible { get; set; } = true;
    public bool Locked { get; set; }
    public double Opacity { get; set; } = 1.0;
}

// Vector content the user has drawn. Each ShapeLayer is exported as an <g> wrapping
// its shapes; using layers (rather than one flat list) lets users redraw the same
// silhouette across stacked layers — a common workflow when tracing reference art.
public sealed class ShapeLayer : Layer
{
    public List<Shape> Shapes { get; } = new();

    public ShapeLayer() { Name = "Shape layer"; }
}

// Non-exported PNG layer used as a tracing template behind the canvas. The bytes
// are kept verbatim so we can ship them to the canvas as a data: URL and trace them
// via PngToSvgTracer on demand.
public sealed class ReferenceImageLayer : Layer
{
    public byte[] PngBytes { get; }
    public string DataUrl { get; }
    public int PixelWidth { get; }
    public int PixelHeight { get; }

    // Placement on the canvas (icon viewBox units).
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public ReferenceImageLayer(byte[] png, int pixelWidth, int pixelHeight,
                               double placeX, double placeY, double placeW, double placeH)
    {
        PngBytes = png;
        PixelWidth = pixelWidth;
        PixelHeight = pixelHeight;
        X = placeX; Y = placeY; Width = placeW; Height = placeH;
        DataUrl = "data:image/png;base64," + Convert.ToBase64String(png);
        Name = "Reference image";
        Opacity = 0.5;
    }
}
