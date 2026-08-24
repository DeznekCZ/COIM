using CustomAssets.ObjEditor.Services;
using Microsoft.Extensions.DependencyInjection;
using Photino.Blazor;

namespace CustomAssets.ObjEditor;

internal static class Program
{
    // WebView2 (Photino's underlying renderer on Windows) requires the UI thread to
    // be STA. Top-level statements compile to a Main without [STAThread] which
    // causes the WebView to load but never paint — symptom is a permanent blank
    // window. Keep this explicit.
    [STAThread]
    private static void Main(string[] args)
    {
        Services.Diag.Log("==== ObjEditor starting ====");
        var builder = PhotinoBlazorAppBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.Services.AddLogging();
        builder.Services.AddSingleton<SceneStore>();
        builder.Services.AddSingleton<LightingStore>();
        builder.Services.AddSingleton<ObjIo>();
        builder.Services.AddSingleton<MtlIo>();
        builder.Services.AddSingleton<AutoSaveService>();

        var app = builder.Build();

        // Start the 2-minute autosave timer once the host is built so it
        // doesn't fire while we're still initializing services.
        app.Services.GetRequiredService<AutoSaveService>().Start();

        app.MainWindow
            .SetTitle("ObjEditor — COI Custom Assets")
            .SetUseOsDefaultSize(false)
            .SetSize(1400, 900)
            .SetResizable(true)
            .SetDevToolsEnabled(true)
            // Native menu stays enabled so the user can right-click anywhere
            // else and reach "Inspect" / dev tools. The 3D viewport's JS
            // contextmenu handler calls preventDefault + stopPropagation to
            // hide the native menu only over the canvas (see onContextMenu
            // in objeditor.js).
            .SetContextMenuEnabled(true);

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try { app.MainWindow.ShowMessage("Fatal", e.ExceptionObject?.ToString() ?? "Unknown"); }
            catch { /* modal can fail on the wrong thread; swallow */ }
        };

        app.Run();
    }
}
