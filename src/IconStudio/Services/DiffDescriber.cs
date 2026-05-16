using System.Text.RegularExpressions;

namespace CustomAssets.IconStudio.Services;

// Heuristic textual description of a DiffPatch — used to label entries in the
// History panel. Inspects the Removed / Added XML fragments and tries to spot
// common patterns:
//
//   • exactly one attribute changed → "fill: #f00 → #00f"
//   • several attributes changed → "fill, stroke updated"
//   • an element appeared / disappeared → "Added Rect" / "Removed Polygon"
//
// Best-effort: an unrecognised diff falls back to a byte-count summary so the
// row still shows something meaningful and the user can still click to jump.
public static class DiffDescriber
{
    private static readonly Regex AttrRx =
        new(@"(\w[\w-]*)=""([^""]*)""", RegexOptions.Compiled);

    private static readonly Regex ElementRx =
        new(@"<(/?)(\w+)(?=[\s/>])", RegexOptions.Compiled);

    // patch is in undo direction (Removed=new content, Added=old content). We
    // want to describe the FORWARD user action — what changed FROM old TO new —
    // so the parameters here are swapped: oldText = patch.Added, newText = patch.Removed.
    public static string DescribeForward(DiffPatch patch)
        => Describe(oldText: patch.Added, newText: patch.Removed);

    public static string Describe(string oldText, string newText)
    {
        if (string.IsNullOrEmpty(oldText) && string.IsNullOrEmpty(newText))
            return "(no change)";

        // Structural change first — added / removed elements dominate the meaning.
        var oldEls = OpeningElements(oldText);
        var newEls = OpeningElements(newText);
        var addedEls = newEls.Except(oldEls).Distinct().ToList();
        var removedEls = oldEls.Except(newEls).Distinct().ToList();
        if (addedEls.Count > 0 && removedEls.Count == 0)
            return addedEls.Count == 1 ? $"Added {Friendly(addedEls[0])}" : $"Added {addedEls.Count} elements";
        if (removedEls.Count > 0 && addedEls.Count == 0)
            return removedEls.Count == 1 ? $"Removed {Friendly(removedEls[0])}" : $"Removed {removedEls.Count} elements";

        // Attribute changes — pair common keys and look for known patterns.
        var oldAttrs = AttributeMap(oldText);
        var newAttrs = AttributeMap(newText);
        var changed = oldAttrs.Keys
            .Intersect(newAttrs.Keys)
            .Where(k => oldAttrs[k] != newAttrs[k])
            .ToList();

        // Known compound: x + y both changed → "Moved by (dx, dy)".
        if (changed.Contains("x") && changed.Contains("y") &&
            TryNum(oldAttrs["x"], out var ox) && TryNum(newAttrs["x"], out var nx) &&
            TryNum(oldAttrs["y"], out var oy) && TryNum(newAttrs["y"], out var ny))
        {
            var dx = Math.Round(nx - ox, 1);
            var dy = Math.Round(ny - oy, 1);
            // Strip x/y from the rest so we don't double-report.
            var others = changed.Except(new[] { "x", "y" }).ToList();
            var prefix = $"Moved by ({Pretty(dx)}, {Pretty(dy)})";
            return others.Count == 0
                ? prefix
                : others.Count == 1
                    ? $"{prefix} + {others[0]}"
                    : $"{prefix} + {others.Count} more";
        }

        // Known compound: w + h changed → "Resized".
        if (changed.Contains("w") && changed.Contains("h") &&
            TryNum(oldAttrs["w"], out var ow) && TryNum(newAttrs["w"], out var nw) &&
            TryNum(oldAttrs["h"], out var oh) && TryNum(newAttrs["h"], out var nh))
        {
            return $"Resized {Pretty(Math.Round(ow,1))}×{Pretty(Math.Round(oh,1))} → " +
                   $"{Pretty(Math.Round(nw,1))}×{Pretty(Math.Round(nh,1))}";
        }

        // Single attribute changed.
        if (changed.Count == 1)
        {
            var k = changed[0];
            return $"{Label(k)}: {ValueShort(oldAttrs[k])} → {ValueShort(newAttrs[k])}";
        }

        // 2–3 changes: list them compactly.
        if (changed.Count > 0 && changed.Count <= 3)
        {
            return string.Join(", ",
                changed.Select(k => $"{Label(k)}: {ValueShort(newAttrs[k])}"));
        }
        if (changed.Count > 3)
        {
            // Highlight one and summarise the rest.
            return $"{Label(changed[0])} + {changed.Count - 1} more";
        }

        // Element-level partial change (e.g. just the points list of a polygon
        // changed, with no attribute pairs to compare).
        if (oldText.Contains("<Point") || newText.Contains("<Point"))
        {
            var oldPoints = CountOccurrences(oldText, "<Point");
            var newPoints = CountOccurrences(newText, "<Point");
            if (oldPoints != newPoints)
                return $"Vertices {oldPoints} → {newPoints}";
            return "Edited vertices";
        }

        return $"Edited ({oldText.Length + newText.Length} bytes)";
    }

    // Map raw XML attribute names to friendlier labels in the history list.
    private static string Label(string key) => key switch
    {
        "x" => "x", "y" => "y", "w" => "width", "h" => "height",
        "r" => "corner",
        "fill" => "fill", "stroke" => "stroke",
        "strokeWidth" => "stroke-width",
        "opacity" => "opacity",
        "visible" => "visible", "name" => "name",
        _ => key,
    };

    private static string Friendly(string element) => element switch
    {
        "Rect" => "Rectangle",
        "Circle" => "Ellipse",
        "Line" => "Line",
        "Polygon" => "Polygon",
        "Polyline" => "Polyline",
        "ShapeLayer" => "Layer",
        "ReferenceImageLayer" => "Reference image",
        "Point" => "Vertex",
        _ => element,
    };

    private static string ValueShort(string s)
    {
        s = s.Trim();
        if (s.Length > 18) return s[..17] + "…";
        return s;
    }

    private static string Pretty(double v) =>
        v == Math.Truncate(v) ? ((int)v).ToString() :
        v.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture);

    private static bool TryNum(string s, out double v) =>
        double.TryParse(s, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out v);

    private static int CountOccurrences(string s, string sub)
    {
        if (string.IsNullOrEmpty(s) || string.IsNullOrEmpty(sub)) return 0;
        int n = 0, i = 0;
        while ((i = s.IndexOf(sub, i, StringComparison.Ordinal)) >= 0) { n++; i += sub.Length; }
        return n;
    }

    private static Dictionary<string, string> AttributeMap(string text)
    {
        // When the same attribute appears multiple times (e.g. several shapes'
        // x="…" in the same diff), keep the last value so the regex doesn't
        // throw on duplicate keys. Imperfect but acceptable for labels.
        var result = new Dictionary<string, string>();
        foreach (Match m in AttrRx.Matches(text))
            result[m.Groups[1].Value] = m.Groups[2].Value;
        return result;
    }

    private static List<string> OpeningElements(string text)
    {
        var list = new List<string>();
        foreach (Match m in ElementRx.Matches(text))
        {
            if (m.Groups[1].Value != "/") list.Add(m.Groups[2].Value);
        }
        return list;
    }

}
