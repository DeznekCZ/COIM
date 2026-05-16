using CustomAssets.IconStudio.Models;

namespace CustomAssets.IconStudio.Services;

// Continuously snapshots the scene to a recovery file so the user can never lose
// work even if the manual save dialog misbehaves. Subscribes to SceneStore changes,
// debounces, and writes XML to %TEMP%/IconStudio-autosave.xml.
//
// The startup recovery flow:
//   - On app launch, Editor checks AutoSavePath for a stale file.
//   - If present, user is offered to load it.
//   - File is deleted only on graceful Save or explicit "discard".
public sealed class AutoSaveService : IDisposable
{
    public static readonly string AutoSavePath =
        Path.Combine(Path.GetTempPath(), "IconStudio-autosave.xml");

    public static readonly string AutoSaveMarkerPath =
        Path.Combine(Path.GetTempPath(), "IconStudio-autosave.marker.txt");

    private readonly SceneStore _store;
    private readonly ProjectSerializer _serializer;
    private readonly Timer _timer;
    private volatile bool _dirty;
    private DateTime _lastWriteUtc = DateTime.MinValue;

    // Minimum gap between writes. Most edits trigger many Notify() calls in quick
    // succession (vertex drag, color change); we coalesce them.
    private static readonly TimeSpan DebounceInterval = TimeSpan.FromSeconds(1);

    public AutoSaveService(SceneStore store, ProjectSerializer serializer)
    {
        _store = store;
        _serializer = serializer;
        _store.OnChange += OnSceneChanged;

        // Background tick checks dirty flag and flushes; keeps the write off the
        // UI thread so even a slow disk can't stall the editor.
        _timer = new Timer(_ => FlushIfDirty(), null, DebounceInterval, DebounceInterval);
    }

    private void OnSceneChanged() => _dirty = true;

    private void FlushIfDirty()
    {
        if (!_dirty) return;
        if (DateTime.UtcNow - _lastWriteUtc < DebounceInterval) return;
        try
        {
            // Snapshot Scene synchronously on whatever thread the timer fires on.
            // Scene is mutated only from the UI thread, so reading here is a race
            // window — worst case we capture a slightly-stale state, which is fine
            // for recovery purposes.
            var xml = _serializer.Save(_store.Scene);
            var tmp = AutoSavePath + ".tmp";
            File.WriteAllText(tmp, xml);
            File.Move(tmp, AutoSavePath, overwrite: true);
            File.WriteAllText(AutoSaveMarkerPath, DateTime.Now.ToString("O"));
            _lastWriteUtc = DateTime.UtcNow;
            _dirty = false;
        }
        catch (Exception ex)
        {
            CrashLog.Write("AutoSave failed", ex);
            // Leave _dirty true so we retry next tick.
        }
    }

    // Removes the autosave file. Called after a successful manual Save so the next
    // session doesn't offer recovery from a state the user already preserved.
    public static void Clear()
    {
        try { if (File.Exists(AutoSavePath)) File.Delete(AutoSavePath); } catch { /* ignore */ }
        try { if (File.Exists(AutoSaveMarkerPath)) File.Delete(AutoSaveMarkerPath); } catch { /* ignore */ }
    }

    // Returns the timestamp of the last autosave, or null if no autosave is present.
    public static DateTime? GetAutosaveTimestamp()
    {
        try
        {
            if (!File.Exists(AutoSavePath)) return null;
            return File.GetLastWriteTime(AutoSavePath);
        }
        catch { return null; }
    }

    public void Dispose()
    {
        _store.OnChange -= OnSceneChanged;
        // Final flush before going away.
        FlushIfDirty();
        _timer.Dispose();
    }
}
