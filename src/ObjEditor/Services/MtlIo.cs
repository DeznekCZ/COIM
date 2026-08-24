using System.Globalization;
using System.Text;
using CustomAssets.ObjEditor.Models;

namespace CustomAssets.ObjEditor.Services;

public sealed class MtlIo
{
    public void ApplyToMesh(Mesh mesh, string mtlText, string mtlDirectory)
    {
        Material? current = null;
        foreach (var rawLine in mtlText.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r', ' ', '\t');
            if (line.Length == 0 || line[0] == '#') continue;
            var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            switch (parts[0])
            {
                case "newmtl":
                    if (parts.Length > 1)
                    {
                        var name = parts[1];
                        current = mesh.Materials.Values.FirstOrDefault(m => m.Name == name)
                                  ?? mesh.AddMaterial(name);
                    }
                    break;
                case "Kd":
                    if (current != null && parts.Length >= 4)
                        current.Diffuse = (D(parts[1]), D(parts[2]), D(parts[3]));
                    break;
                case "map_Kd":
                    if (current != null && parts.Length > 1)
                    {
                        var rel = line.Substring(parts[0].Length).Trim();
                        var path = Path.IsPathRooted(rel) ? rel : Path.Combine(mtlDirectory, rel);
                        current.TexturePath = path;
                        if (File.Exists(path))
                        {
                            try { current.TextureBytes = File.ReadAllBytes(path); }
                            catch { /* keep path, drop bytes */ }
                        }
                    }
                    break;
            }
        }
    }

    public string Write(Mesh mesh)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# Written by ObjEditor");
        foreach (var m in mesh.Materials.Values.OrderBy(m => m.Id))
        {
            sb.Append("newmtl ").AppendLine(m.Name);
            sb.AppendFormat(CultureInfo.InvariantCulture, "Kd {0} {1} {2}\n",
                m.Diffuse.R, m.Diffuse.G, m.Diffuse.B);
            // Emit map_Kd whenever we actually have bytes — even if TexturePath
            // was lost along the way (e.g. user loaded then re-saved a session
            // before opening a real file). The basename below has to match what
            // Editor.razor writes to disk for the round-trip to find the image.
            var name = TextureFileName(m);
            if (name is not null) sb.Append("map_Kd ").AppendLine(name);
        }
        return sb.ToString();
    }

    // Single source of truth for "what filename does this material's texture
    // get?" — used by both the MTL writer and Editor.razor's save-bytes loop
    // so the map_Kd line and the saved PNG always agree.
    public static string? TextureFileName(Material m)
    {
        if (m.TextureBytes is not { Length: > 0 }) return null;
        if (!string.IsNullOrEmpty(m.TexturePath))
            return Path.GetFileName(m.TexturePath);
        // No remembered name — synthesize one from the material name. Strip
        // characters that aren't filesystem-safe to avoid surprises.
        var safe = new string((m.Name ?? "material").Select(c =>
            char.IsLetterOrDigit(c) || c == '_' || c == '-' ? c : '_').ToArray());
        return safe + ".png";
    }

    private static double D(string s)
        => double.Parse(s, NumberStyles.Float, CultureInfo.InvariantCulture);
}
