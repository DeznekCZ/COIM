namespace CustomAssets.IconStudio.Services;

// Append-only crash log at %TEMP%/IconStudio-crash.log. Used by the global handlers
// in Program.cs and any try/catch inside the app that survives but wants a record.
// Never throws — logging failures must not cascade into the failure they're logging.
public static class CrashLog
{
    public static readonly string LogPath =
        Path.Combine(Path.GetTempPath(), "IconStudio-crash.log");

    public static void Write(string source, object? payload)
    {
        try
        {
            File.AppendAllText(LogPath,
                $"\n[{DateTime.Now:O}] {source}\n{payload}\n");
        }
        catch
        {
            // Logging must not throw.
        }
    }
}
