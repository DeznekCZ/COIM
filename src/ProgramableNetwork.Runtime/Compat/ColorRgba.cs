namespace Mafi
{
    /// <summary>
    /// Minimal stand-in for Mafi's <c>ColorRgba</c>. The interpreter only needs to (a) construct it
    /// (exposed to Python via <c>Expressions.Initializers</c>) and (b) carry it on a controller
    /// template. RGBA byte channels are enough for the web app to render the same color.
    /// </summary>
    public readonly struct ColorRgba
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;
        public readonly byte A;

        public ColorRgba(byte r, byte g, byte b, byte a = 255)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public override string ToString() => $"#{R:X2}{G:X2}{B:X2}{A:X2}";
    }
}
