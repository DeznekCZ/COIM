using System.Net.Http.Json;
using ProgramableNetwork.Runtime.Sim;

namespace WebPreview.Services;

/// <summary>
/// Loads the exported bundle (index.json + Python sources) and parses every module through the shared
/// <see cref="ModuleLoader"/> (the real in-game interpreter). The resulting <see cref="ModuleDefinition"/>s
/// drive BOTH the preview gallery and the simulator — there is no separate metadata format.
/// </summary>
public sealed class ModuleCatalog
{
    private readonly HttpClient http;

    public List<ModuleDefinition> Modules { get; } = new();
    public List<string> LoadErrors { get; } = new();
    public bool Loaded { get; private set; }

    public ModuleCatalog(HttpClient http)
    {
        this.http = http;
    }

    /// <summary>Module definitions grouped by their first category id (modules with none go under "other").</summary>
    public IEnumerable<IGrouping<string, ModuleDefinition>> ByCategory() =>
        Modules.GroupBy(m => m.Categories.Count > 0 ? m.Categories[0] : "other")
               .OrderBy(g => g.Key);

    public ModuleDefinition? Find(string id) => Modules.FirstOrDefault(m => m.Id == id);

    public async Task EnsureLoadedAsync(string dataRoot = "data")
    {
        if (Loaded)
        {
            return;
        }

        HashSet<string> seenIds = new();

        // 1) Python-authored Custom modules — these carry executable behavior (an action).
        BundleIndex? index = await http.GetFromJsonAsync<BundleIndex>($"{dataRoot}/index.json");
        if (index?.Modules != null)
        {
            foreach (string relative in index.Modules)
            {
                try
                {
                    string content = await http.GetStringAsync($"{dataRoot}/{relative}");
                    string fileName = relative.Contains('/') ? relative[(relative.LastIndexOf('/') + 1)..] : relative;
                    foreach (ModuleDefinition def in ModuleLoader.LoadFile(fileName, content))
                    {
                        if (seenIds.Add(def.Id))
                        {
                            Modules.Add(def);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Template/controller files and any unsupported construct fail here; skip them so
                    // the gallery still shows every real module.
                    LoadErrors.Add($"{relative}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        }

        // 2) Generated ids.py descriptor — covers ALL modules (incl. the C#-defined ones). Metadata
        //    only (no action). Added for any module id not already provided by a Custom file above, so
        //    Python modules keep their behavior and C# modules show up as preview-only.
        try
        {
            string ids = await http.GetStringAsync($"{dataRoot}/core/ids.py");
            foreach (ModuleDefinition def in ModuleLoader.LoadFile("ids.py", ids))
            {
                if (seenIds.Add(def.Id))
                {
                    Modules.Add(def);
                }
            }
        }
        catch (Exception ex)
        {
            // ids.py is produced by the in-game `pn_exportWebData` command; absent in a bare dev tree.
            LoadErrors.Add($"core/ids.py: {ex.GetType().Name}: {ex.Message}");
        }

        Modules.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        Loaded = true;
    }

    private sealed class BundleIndex
    {
        public List<string>? Modules { get; set; }
        public List<string>? Core { get; set; }
    }
}
