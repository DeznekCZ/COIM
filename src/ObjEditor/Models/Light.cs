namespace CustomAssets.ObjEditor.Models;

// Directional light. Azimuth is degrees from +X toward +Z in Unity space;
// elevation is degrees above the ground plane (+90 = straight up). Color is
// in [0,1] linear-ish, multiplied by Intensity at shader-feed time so the
// values map cleanly to UI sliders.
public sealed class Light
{
    public double Azim { get; set; }
    public double Elev { get; set; } = 55;
    public double R { get; set; } = 1.0;
    public double G { get; set; } = 1.0;
    public double B { get; set; } = 1.0;
    public double Intensity { get; set; } = 1.0;
    // Display name shown in the UI list. The first light is the sun and
    // always keeps Name = "Sun"; extras default to "Light N".
    public string Name { get; set; } = "Light";
}
