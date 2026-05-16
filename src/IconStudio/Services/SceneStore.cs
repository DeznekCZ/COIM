using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Single-instance scene + change notification. Components subscribe to OnChange to
// re-render when the scene mutates. Kept deliberately simple — no undo stack yet.
public sealed class SceneStore
{
    public Scene Scene { get; } = new();

    public event Action? OnChange;

    public Shape? SelectedShape { get; private set; }

    // ── Brush ──────────────────────────────────────────────────────────
    // Editor-local "remember last colors" state. New shapes inherit these
    // values at commit time so the user doesn't reset to gray each time.
    // Updated by: PropertyPanel fill/stroke edits, eyedropper pick.
    // Deliberately NOT part of the Scene, so brush changes don't pollute
    // undo history with cross-shape color noise.
    public string CurrentFill { get; set; } = "#cccccc";
    public string CurrentStroke { get; set; } = "#000000";
    public double CurrentStrokeWidth { get; set; } = 0;

    public SceneStore()
    {
        Scene.EnsureActiveShapeLayer();
    }

    public void Notify() => OnChange?.Invoke();

    public void Select(Shape? shape) => SelectMulti(shape, false);

    public void DeleteSelected()
    {
        var toDelete = GetSelectedShapes().ToList();
        if (toDelete.Count == 0) return;
        foreach (var layer in Scene.Layers.OfType<ShapeLayer>())
            foreach (var s in toDelete)
                layer.Shapes.Remove(s);
        SelectedShape = null;
        SelectedShapeIds.Clear();
        Notify();
    }

    public void AddShape(Shape s)
    {
        var layer = Scene.EnsureActiveShapeLayer();
        layer.Shapes.Add(s);
        SelectedShape = s;
        Notify();
    }

    // Swap in a fresh Scene (project load / SVG import). Selection is cleared so
    // the property panel doesn't dangle on a shape that no longer exists.
    public void LoadScene(Scene newScene)
    {
        SelectedShape = null;
        Scene.ReplaceWith(newScene);
        Notify();
    }

    // Relocates a shape from whichever ShapeLayer currently owns it to `target`.
    // The shape's identity (Id, geometry, style) is preserved; only its parent
    // changes. No-op if the shape is already in target.
    public void MoveShapeToLayer(Shape shape, ShapeLayer target)
    {
        if (target.Shapes.Contains(shape)) return;
        foreach (var layer in Scene.Layers.OfType<ShapeLayer>())
        {
            if (layer.Shapes.Remove(shape)) break;
        }
        target.Shapes.Add(shape);
        Scene.ActiveLayerId = target.Id;
        SelectedShape = shape;
        Notify();
    }

    // In-memory clipboard for Ctrl+C / Ctrl+V. Stores cloned shapes (so subsequent
    // mutations of the original don't affect the clipboard, and paste re-clones so
    // multiple pastes produce independent shapes).
    private readonly List<Shape> _clipboard = new();
    public bool HasClipboard => _clipboard.Count > 0;

    public void CopySelected()
    {
        _clipboard.Clear();
        foreach (var s in GetSelectedShapes())
            _clipboard.Add(s.Clone());
    }

    public void Paste(double offsetX = 8, double offsetY = 8)
    {
        if (_clipboard.Count == 0) return;
        var layer = Scene.EnsureActiveShapeLayer();
        SelectedShapeIds.Clear();
        Shape? last = null;
        foreach (var src in _clipboard)
        {
            var copy = src.Clone();
            if (copy.Kind == ShapeKind.Polygon || copy.Kind == ShapeKind.Polyline)
            {
                for (int i = 0; i < copy.Points.Count; i++)
                    copy.Points[i] = (copy.Points[i].X + offsetX, copy.Points[i].Y + offsetY);
            }
            else
            {
                copy.X += offsetX;
                copy.Y += offsetY;
            }
            layer.Shapes.Add(copy);
            SelectedShapeIds.Add(copy.Id);
            last = copy;
        }
        SelectedShape = last;
        Notify();
    }

    // ── Selection (single + multi) ─────────────────────────────────────
    // SelectedShape remains the "primary" selection that single-selection-aware
    // code (PropertyPanel single-shape rendering, vertex/handle drag in JS) uses.
    // SelectedShapeIds holds the full set when multi-select is active.
    public HashSet<int> SelectedShapeIds { get; } = new();

    public IEnumerable<Shape> GetSelectedShapes()
    {
        if (SelectedShapeIds.Count == 0)
        {
            if (SelectedShape != null) yield return SelectedShape;
            yield break;
        }
        foreach (var l in Scene.Layers.OfType<ShapeLayer>())
            foreach (var s in l.Shapes)
                if (SelectedShapeIds.Contains(s.Id)) yield return s;
    }

    public void SelectMulti(Shape? shape, bool addMode)
    {
        if (shape is null)
        {
            if (addMode) return; // clicking empty space with Ctrl held: keep selection
            SelectedShape = null;
            SelectedShapeIds.Clear();
            Notify();
            return;
        }
        if (addMode)
        {
            if (SelectedShapeIds.Contains(shape.Id))
            {
                // Toggle: deselect
                SelectedShapeIds.Remove(shape.Id);
                if (SelectedShape?.Id == shape.Id)
                    SelectedShape = GetSelectedShapes().FirstOrDefault();
            }
            else
            {
                SelectedShapeIds.Add(shape.Id);
                SelectedShape = shape;
            }
        }
        else
        {
            SelectedShape = shape;
            SelectedShapeIds.Clear();
            SelectedShapeIds.Add(shape.Id);
        }
        Notify();
    }
}
