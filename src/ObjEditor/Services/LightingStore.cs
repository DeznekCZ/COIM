using CustomAssets.ObjEditor.Models;

namespace CustomAssets.ObjEditor.Services;

// Holds the directional lights affecting all viewports. Index 0 is the sun
// (cannot be removed). Up to MaxLights total — shader has a fixed array
// bound; extras beyond that are silently ignored at push time.
public sealed class LightingStore
{
    public const int MaxLights = 4;

    public List<Light> Lights { get; } = new()
    {
        new Light { Name = "Sun", Azim = 45, Elev = 55, Intensity = 1.0 },
    };

    // Ambient term sampled even when every dot(N, light) is zero. Keeps the
    // back of the model visible. Mirrors the old hardcoded 0.25 baseline.
    public double Ambient { get; set; } = 0.25;

    public event Action? OnChange;
    public void Notify() => OnChange?.Invoke();

    public Light AddLight()
    {
        if (Lights.Count >= MaxLights) return Lights[^1];
        var l = new Light
        {
            Name = "Light " + (Lights.Count + 1),
            Azim = 200,
            Elev = 25,
            R = 0.6, G = 0.7, B = 1.0,  // a cool fill so additions are visible
            Intensity = 0.4,
        };
        Lights.Add(l);
        Notify();
        return l;
    }

    public void RemoveLight(int index)
    {
        // Index 0 is the sun — keep it. Removing it would orphan the existing
        // azim/elev toolbar inputs that bind to Lights[0].
        if (index <= 0 || index >= Lights.Count) return;
        Lights.RemoveAt(index);
        Notify();
    }

    public void Update(int index, Action<Light> mutate)
    {
        if (index < 0 || index >= Lights.Count) return;
        mutate(Lights[index]);
        Notify();
    }

    public void SetAmbient(double value)
    {
        Ambient = Math.Clamp(value, 0, 1);
        Notify();
    }
}
