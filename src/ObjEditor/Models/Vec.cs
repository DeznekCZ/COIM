using System.Globalization;

namespace CustomAssets.ObjEditor.Models;

// Stored in Unity convention: +X right, +Y up, +Z forward, left-handed.
// I/O writes these coordinates unchanged; renderer mirrors Z when handing
// data to Three.js so the world feels Unity-shaped on screen.
public record struct Vec3(double X, double Y, double Z)
{
    public static Vec3 Zero => new(0, 0, 0);
    public Vec3 With(double? x = null, double? y = null, double? z = null)
        => new(x ?? X, y ?? Y, z ?? Z);
    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###}, {2:0.###})", X, Y, Z);
}

public record struct Vec2(double U, double V)
{
    public static Vec2 Zero => new(0, 0);
    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture, "({0:0.###}, {1:0.###})", U, V);
}
