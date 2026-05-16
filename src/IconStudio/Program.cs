using CustomAssets.IconStudio;
using CustomAssets.IconStudio.Services;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace CustomAssets.IconStudio;

internal static class Program
{
    // WebView2 (Photino's underlying renderer on Windows) requires the UI thread to
    // be STA. Top-level statements compile to a Main without [STAThread] which
    // causes the WebView to load but never paint — symptom is a permanent blank
    // window. Keep this explicit.
    [STAThread]
    private static void Main(string[] args)
    {
        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.Services.AddLogging();
        builder.Services.AddSingleton<SceneStore>();
        builder.Services.AddSingleton<SvgExporter>();
        builder.Services.AddSingleton<PngPreviewService>();
        builder.Services.AddSingleton<PngImportService>();
        builder.Services.AddSingleton<FileDialogService>();
        builder.Services.AddSingleton<ProjectSerializer>();
        builder.Services.AddSingleton<SvgImporter>();
        builder.Services.AddSingleton<AutoSaveService>();
        builder.Services.AddSingleton<UndoService>();

        var app = builder.Build();
        app.Services.GetRequiredService<FileDialogService>().SetWindow(app.MainWindow);

        // Touch the autosave + undo services so their timers start and they
        // subscribe to scene changes. Without this, neither fires until
        // something else resolves them.
        _ = app.Services.GetRequiredService<AutoSaveService>();
        _ = app.Services.GetRequiredService<UndoService>();

        app.MainWindow
            .SetTitle("IconStudio — COI Custom Assets")
            .SetUseOsDefaultSize(false)
            .SetSize(1400, 900)
            .SetResizable(true)
            .SetDevToolsEnabled(true)
            .SetContextMenuEnabled(true);

        // Log + (try to) surface unhandled exceptions. ShowMessage on the wrong
        // thread can itself crash, so we log first — the log survives even if the
        // modal dialog call below fails.
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            CrashLog.Write("UnhandledException", e.ExceptionObject);
            try { app.MainWindow.ShowMessage("Fatal", e.ExceptionObject?.ToString() ?? "Unknown"); }
            catch { /* dialog might fail; we still wrote to the log */ }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            CrashLog.Write("UnobservedTaskException", e.Exception);
            e.SetObserved();
        };

        CrashLog.Write("Startup", $"IconStudio launched, log path: {CrashLog.LogPath}");

        app.Run();
    }
}
