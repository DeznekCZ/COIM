using System.Globalization;

namespace CustomAssets.ObjEditor.Services;

// Append-only diagnostic log next to the exe. Used to catch silent interop
// failures (where exceptions are swallowed by a .catch on the JS side or by
// the try/catch in JSInvokable methods) so we can actually see what's
// happening across the WebView2 ↔ .NET boundary without a debugger attached.
//
// File: <exe-dir>/objeditor.log. Truncated to ~256 KB at startup so it
// doesn't grow unbounded across runs.
internal static class Diag
{
    private static readonly object _gate = new();
    private static readonly string _path =
        System.IO.Path.Combine(AppContext.BaseDirectory, "objeditor.log");
    private static bool _truncated;

    public static void Log(string message)
    {
        try
        {
            lock (_gate)
            {
                if (!_truncated)
                {
                    _truncated = true;
                    try
                    {
                        if (System.IO.File.Exists(_path))
                        {
                            var fi = new System.IO.FileInfo(_path);
                            if (fi.Length > 256 * 1024) System.IO.File.Delete(_path);
                        }
                    }
                    catch { }
                }
                var line = string.Format(CultureInfo.InvariantCulture,
                    "{0:HH:mm:ss.fff} {1}\n", DateTime.Now, message);
                System.IO.File.AppendAllText(_path, line);
            }
        }
        catch { /* never fail callers because of logging */ }
    }
}
