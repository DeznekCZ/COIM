using System.Text;
using System.Text.Json;
using CustomAssets.ObjEditor.Models;

namespace CustomAssets.ObjEditor.Services;

// Every 2 minutes, write the current scene to a recovery folder so a crash
// doesn't lose work. On startup the Editor checks for a manifest and offers
// to restore. A successful explicit Save clears the recovery copy because
// the user has committed to a real file.
//
// Storage location preference (first writable wins):
//   1. %LOCALAPPDATA%\ObjEditor\autosave   — persistent across reboots,
//                                            survives OS temp-folder cleanup
//   2. %TEMP%\ObjEditor-autosave           — fallback if AppData isn't writable
//
// Layout under that root:
//   scene.obj          OBJ text
//   scene.mtl          MTL text (if there are materials)
//   textures/<name>    one file per material texture
//   meta.json          manifest (timestamp, lights, last-known OBJ name)
public sealed class AutoSaveService : IDisposable
{
    public static readonly string Dir = ResolveDir();
    public static string MetaPath => Path.Combine(Dir, "meta.json");
    public static string ObjPath => Path.Combine(Dir, "scene.obj");
    public static string MtlPath => Path.Combine(Dir, "scene.mtl");
    public static string TextureDir => Path.Combine(Dir, "textures");

    // Resolve the most reliable writable spot for our recovery files. Tries
    // %LOCALAPPDATA%\ObjEditor\autosave first; if that path can't be created
    // (rare — corporate policy, read-only profile, etc) we fall back to
    // %TEMP% so autosave still works somewhere.
    private static string ResolveDir()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), "ObjEditor", "autosave"),
            Path.Combine(Path.GetTempPath(), "ObjEditor-autosave"),
        };
        foreach (var c in candidates)
        {
            try
            {
                Directory.CreateDirectory(c);
                // Smoke-test write so we don't pick a directory we can create
                // but not actually write into (some sandboxed environments
                // allow create + deny writes inside).
                var probe = Path.Combine(c, ".probe");
                File.WriteAllText(probe, "");
                File.Delete(probe);
                return c;
            }
            catch { /* try next candidate */ }
        }
        // Last-ditch: use the first candidate path even if probe failed; the
        // SaveSnapshot try/catch will log the real error per attempt.
        return candidates[0];
    }

    private readonly SceneStore _scene;
    private readonly LightingStore _lights;
    private readonly ObjIo _obj;
    private readonly MtlIo _mtl;
    private readonly System.Timers.Timer _timer;
    private readonly object _gate = new();
    private string? _lastObjName;

    public AutoSaveService(SceneStore scene, LightingStore lights, ObjIo obj, MtlIo mtl)
    {
        _scene = scene; _lights = lights; _obj = obj; _mtl = mtl;
        // 2-minute period per the spec. AutoReset keeps it ticking; we don't
        // start until the editor calls Start so the timer doesn't fire while
        // we're still booting up.
        _timer = new System.Timers.Timer(2 * 60 * 1000) { AutoReset = true };
        _timer.Elapsed += (_, _) => SaveSnapshot();
        Diag.Log("AutoSave dir: " + Dir);
    }

    public void SetLastObjName(string? name) { _lastObjName = name; }

    public void Start() { _timer.Start(); Diag.Log("AutoSave timer started (2 min)"); }

    // Caller invokes after a successful explicit Save — once the user has a
    // real file, the recovery copy is no longer needed and would just confuse
    // the next startup prompt.
    public void Clear()
    {
        try
        {
            lock (_gate)
            {
                if (!Directory.Exists(Dir)) return;
                Directory.Delete(Dir, recursive: true);
                Diag.Log("AutoSave cleared: " + Dir);
            }
        }
        catch (Exception ex) { Diag.Log("AutoSave.Clear: " + ex); }
    }

    public void SaveSnapshot()
    {
        try
        {
            lock (_gate)
            {
                Directory.CreateDirectory(Dir);
                Directory.CreateDirectory(TextureDir);

                var hasMaterials = _scene.Mesh.Materials.Count > 0;
                var mtlName = hasMaterials ? "scene.mtl" : null;
                File.WriteAllText(ObjPath, _obj.Write(_scene.Mesh, mtlName), Encoding.UTF8);
                if (hasMaterials)
                    File.WriteAllText(MtlPath, _mtl.Write(_scene.Mesh), Encoding.UTF8);

                // Wipe + rewrite textures so a deleted material doesn't leave
                // a stale file lying around to be picked up by recovery.
                foreach (var existing in Directory.EnumerateFiles(TextureDir))
                {
                    try { File.Delete(existing); } catch { }
                }
                foreach (var m in _scene.Mesh.Materials.Values)
                {
                    if (m.TextureBytes is not { Length: > 0 }) continue;
                    var name = MtlIo.TextureFileName(m);
                    if (name is null) continue;
                    var path = Path.Combine(TextureDir, name);
                    File.WriteAllBytes(path, m.TextureBytes);
                }

                var manifest = new AutoSaveManifest
                {
                    Timestamp = DateTimeOffset.UtcNow.ToString("o"),
                    LastObjName = _lastObjName,
                    VertexCount = _scene.Mesh.Vertices.Count,
                    FaceCount = _scene.Mesh.Faces.Count,
                    MaterialCount = _scene.Mesh.Materials.Count,
                    Ambient = _lights.Ambient,
                    Lights = _lights.Lights.Select(l => new LightDto
                    {
                        Name = l.Name, Azim = l.Azim, Elev = l.Elev,
                        R = l.R, G = l.G, B = l.B, Intensity = l.Intensity,
                    }).ToList(),
                };
                File.WriteAllText(MetaPath,
                    JsonSerializer.Serialize(manifest,
                        new JsonSerializerOptions { WriteIndented = true }),
                    Encoding.UTF8);
                Diag.Log($"AutoSave wrote {_scene.Mesh.Vertices.Count} v · " +
                         $"{_scene.Mesh.Faces.Count} f · {_scene.Mesh.Materials.Count} m " +
                         $"to {Dir}");
            }
        }
        catch (Exception ex) { Diag.Log("AutoSave.SaveSnapshot: " + ex); }
    }

    // Returns a manifest only if an autosave exists; null otherwise. The
    // caller (Editor.razor) uses this on startup to decide whether to ask
    // the user about recovery.
    public AutoSaveManifest? TryLoadManifest()
    {
        try
        {
            if (!File.Exists(MetaPath)) return null;
            var json = File.ReadAllText(MetaPath);
            return JsonSerializer.Deserialize<AutoSaveManifest>(json);
        }
        catch (Exception ex)
        {
            Diag.Log("Autosave.TryLoadManifest: " + ex);
            return null;
        }
    }

    // Read the saved OBJ + MTL + textures back into a mesh, returning the
    // mesh plus the light state. Doesn't touch any global state — the Editor
    // applies the result via SceneStore.Replace and LightingStore.
    public (Mesh Mesh, AutoSaveManifest Manifest)? TryLoad()
    {
        try
        {
            if (!File.Exists(ObjPath) || !File.Exists(MetaPath)) return null;
            var manifest = TryLoadManifest();
            if (manifest is null) return null;

            var load = _obj.Load(File.ReadAllText(ObjPath));
            if (File.Exists(MtlPath))
            {
                _mtl.ApplyToMesh(load.Mesh, File.ReadAllText(MtlPath), mtlDirectory: TextureDir);
                // ApplyToMesh only loads textures whose map_Kd files are
                // findable on disk; for our autosave layout that's textures/.
                // Anything missing simply has TextureBytes == null.
            }
            return (load.Mesh, manifest);
        }
        catch (Exception ex)
        {
            Diag.Log("Autosave.TryLoad: " + ex);
            return null;
        }
    }

    public void Dispose()
    {
        try { _timer.Stop(); _timer.Dispose(); } catch { }
    }
}

public sealed class AutoSaveManifest
{
    public string Timestamp { get; set; } = "";
    public string? LastObjName { get; set; }
    public int VertexCount { get; set; }
    public int FaceCount { get; set; }
    public int MaterialCount { get; set; }
    public double Ambient { get; set; } = 0.25;
    public List<LightDto> Lights { get; set; } = new();
}

public sealed class LightDto
{
    public string Name { get; set; } = "";
    public double Azim { get; set; }
    public double Elev { get; set; }
    public double R { get; set; }
    public double G { get; set; }
    public double B { get; set; }
    public double Intensity { get; set; }
}
