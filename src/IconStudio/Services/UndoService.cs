namespace CustomAssets.IconStudio.Services;

public sealed record HistoryEntry(DiffPatch Patch, string Description, DateTime At);

// Diff-based undo / redo with a human-readable history.
//
// Per scene change (debounced ~400 ms): compute a DiffPatch between the previous
// baseline and the new serialized state, attach a description via DiffDescriber,
// push as a HistoryEntry. The description is computed once at push time and
// stays attached as the entry travels between undo / redo stacks — that way
// "Move shape" still reads as "Move shape" after the user redoes it.
//
// Memory: each entry stores only the changed XML interval (Removed + Added) +
// a small description string + a DateTime. For typical edits that's a few dozen
// bytes per entry, not the full ~10 KB scene XML.
public sealed class UndoService : IDisposable
{
    private readonly SceneStore _store;
    private readonly ProjectSerializer _serializer;
    private readonly Stack<HistoryEntry> _undo = new();
    private readonly Stack<HistoryEntry> _redo = new();
    private readonly Timer _flushTimer;
    private string _currentXml;
    private DateTime _lastChangeUtc = DateTime.MinValue;
    private bool _restoring;

    private static readonly TimeSpan DebounceWindow = TimeSpan.FromMilliseconds(400);
    private const int MaxStackDepth = 200;

    public event Action? OnChange;

    public UndoService(SceneStore store, ProjectSerializer serializer)
    {
        _store = store;
        _serializer = serializer;
        _currentXml = _serializer.Save(_store.Scene);
        _store.OnChange += OnSceneChanged;
        _flushTimer = new Timer(_ => MaybeFlush(), null,
            TimeSpan.FromMilliseconds(250), TimeSpan.FromMilliseconds(250));
    }

    // Exposed snapshots of the stacks for the History panel. Top-first: index 0
    // is the most recent past edit (one Undo away).
    public IReadOnlyList<HistoryEntry> UndoEntries => _undo.ToArray();
    public IReadOnlyList<HistoryEntry> RedoEntries => _redo.ToArray();

    private void OnSceneChanged()
    {
        if (_restoring) return;
        _lastChangeUtc = DateTime.UtcNow;
    }

    private void MaybeFlush()
    {
        if (_restoring) return;
        if (_lastChangeUtc == DateTime.MinValue) return;
        if (DateTime.UtcNow - _lastChangeUtc < DebounceWindow) return;
        ForceFlush();
    }

    private void ForceFlush()
    {
        if (_lastChangeUtc == DateTime.MinValue) return;
        string newXml;
        try { newXml = _serializer.Save(_store.Scene); }
        catch (Exception ex) { CrashLog.Write("Undo.Snapshot", ex); return; }

        if (newXml == _currentXml) { _lastChangeUtc = DateTime.MinValue; return; }

        var undoPatch = DiffPatch.Between(newXml, _currentXml);
        var description = DiffDescriber.DescribeForward(undoPatch);
        _undo.Push(new HistoryEntry(undoPatch, description, DateTime.Now));
        TrimStack(_undo);
        _redo.Clear();
        _currentXml = newXml;
        _lastChangeUtc = DateTime.MinValue;
        OnChange?.Invoke();
    }

    private static void TrimStack(Stack<HistoryEntry> s)
    {
        if (s.Count <= MaxStackDepth) return;
        var arr = s.ToArray(); // top-first
        s.Clear();
        for (int i = MaxStackDepth - 1; i >= 0; i--) s.Push(arr[i]);
    }

    public bool CanUndo => _undo.Count > 0 || HasPendingChange();
    public bool CanRedo => _redo.Count > 0;

    private bool HasPendingChange()
    {
        if (_lastChangeUtc == DateTime.MinValue) return false;
        try { return _serializer.Save(_store.Scene) != _currentXml; }
        catch { return false; }
    }

    public void Undo()
    {
        ForceFlush();
        if (_undo.Count == 0) return;
        var entry = _undo.Pop();
        string prev;
        try { prev = entry.Patch.ApplyTo(_currentXml); }
        catch (Exception ex) { CrashLog.Write("Undo.Apply", ex); return; }
        // Description stays with the action; only the patch is inverted so it can
        // be applied in the reverse direction on Redo.
        _redo.Push(entry with { Patch = entry.Patch.Inverse() });
        Restore(prev);
        OnChange?.Invoke();
    }

    public void Redo()
    {
        if (_redo.Count == 0) return;
        var entry = _redo.Pop();
        string next;
        try { next = entry.Patch.ApplyTo(_currentXml); }
        catch (Exception ex) { CrashLog.Write("Redo.Apply", ex); return; }
        _undo.Push(entry with { Patch = entry.Patch.Inverse() });
        Restore(next);
        OnChange?.Invoke();
    }

    // Convenience for the History panel — jump N steps back (positive) or
    // forward (negative). Loops Undo/Redo internally so each intermediate
    // step still creates a clean OnChange / Notify cycle.
    public void Jump(int delta)
    {
        if (delta == 0) return;
        if (delta > 0)
        {
            for (int i = 0; i < delta && _undo.Count > 0; i++) Undo();
        }
        else
        {
            for (int i = 0; i < -delta && _redo.Count > 0; i++) Redo();
        }
    }

    private void Restore(string xml)
    {
        try
        {
            _restoring = true;
            var scene = _serializer.Load(xml);
            _store.LoadScene(scene);
            _currentXml = xml;
            _lastChangeUtc = DateTime.MinValue;
        }
        catch (Exception ex) { CrashLog.Write("Undo.Restore", ex); }
        finally { _restoring = false; }
    }

    public void Dispose()
    {
        _store.OnChange -= OnSceneChanged;
        _flushTimer.Dispose();
    }
}
