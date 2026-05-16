using Photino.NET;

namespace CustomAssets.IconStudio.Services;

// Thin wrapper around Photino's native file dialogs so Blazor components can ask
// for paths without taking a direct dependency on the window. The window reference
// is set once during startup (Program.cs) after PhotinoBlazorApp.Build() creates it.
//
// The async variants are used because the synchronous ones block the calling
// thread until the dialog dismisses — and when invoked from a Blazor event
// handler, that thread is NOT the UI/STA thread the native dialog needs.
public sealed class FileDialogService
{
    private PhotinoWindow? _window;

    public void SetWindow(PhotinoWindow window) => _window = window;

    public async Task<string?> OpenFileAsync(string title,
                                             (string Name, string[] Extensions)[] filters,
                                             string? defaultPath = null)
    {
        if (_window is null) return null;
        try
        {
            var paths = await _window.ShowOpenFileAsync(
                title, NormalizePath(defaultPath), multiSelect: false, filters);
            return paths is { Length: > 0 } ? paths[0] : null;
        }
        catch (Exception ex)
        {
            CrashLog.Write("OpenFileAsync", ex);
            ShowMessage("Open dialog failed", ex.Message);
            return null;
        }
    }

    public async Task<string?> SaveFileAsync(string title,
                                             (string Name, string[] Extensions)[] filters,
                                             string? defaultPath = null)
    {
        if (_window is null) return null;
        try
        {
            var path = await _window.ShowSaveFileAsync(
                title, NormalizePath(defaultPath), filters);
            return string.IsNullOrEmpty(path) ? null : path;
        }
        catch (Exception ex)
        {
            CrashLog.Write("SaveFileAsync", ex);
            ShowMessage("Save dialog failed", ex.Message);
            return null;
        }
    }

    public void ShowMessage(string title, string message)
    {
        if (_window is null) return;
        try { _window.ShowMessage(title, message); }
        catch (Exception ex) { CrashLog.Write("ShowMessage", ex); }
    }

    // Photino's native dialogs on Windows want an absolute path OR an empty string.
    // A bare filename (e.g. "icon.svg") would be resolved against the process's CWD
    // which on `dotnet run` is the project source dir, and against some shell variants
    // returns an invalid-path error that crashes the dialog. Fall back to MyDocuments
    // for the directory portion, keep the filename if provided.
    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return "";
        if (Path.IsPathRooted(path)) return path;
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docs, path);
    }
}
