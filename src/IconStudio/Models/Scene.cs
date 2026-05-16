namespace CustomAssets.IconStudio.Models;

public sealed class Scene
{
    // Logical icon canvas size. SVG viewBox is "0 0 Width Height"; the editor maps
    // this to the on-screen <canvas> via a uniform scale factor.
    public double Width { get; set; } = 128;
    public double Height { get; set; } = 128;

    public List<Layer> Layers { get; } = new();

    public int? ActiveLayerId { get; set; }

    public Layer? ActiveLayer =>
        ActiveLayerId is int id ? Layers.FirstOrDefault(l => l.Id == id) : null;

    public ShapeLayer EnsureActiveShapeLayer()
    {
        if (ActiveLayer is ShapeLayer sl) return sl;
        var found = Layers.OfType<ShapeLayer>().LastOrDefault();
        if (found != null) { ActiveLayerId = found.Id; return found; }
        var created = new ShapeLayer { Name = $"Layer {Layers.Count + 1}" };
        Layers.Add(created);
        ActiveLayerId = created.Id;
        return created;
    }

    public void Move(Layer layer, int delta)
    {
        int i = Layers.IndexOf(layer);
        if (i < 0) return;
        int j = Math.Clamp(i + delta, 0, Layers.Count - 1);
        if (i == j) return;
        Layers.RemoveAt(i);
        Layers.Insert(j, layer);
    }

    public void Remove(Layer layer)
    {
        Layers.Remove(layer);
        if (ActiveLayerId == layer.Id) ActiveLayerId = Layers.LastOrDefault()?.Id;
    }

    // Reassigns this Scene's contents to mirror `other`. Callers (SceneStore.LoadScene)
    // use this so they don't have to replace the Scene reference held by components.
    public void ReplaceWith(Scene other)
    {
        Width = other.Width;
        Height = other.Height;
        Layers.Clear();
        foreach (var l in other.Layers) Layers.Add(l);
        ActiveLayerId = other.ActiveLayerId;
    }
}
