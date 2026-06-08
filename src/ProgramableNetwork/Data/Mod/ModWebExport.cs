using Mafi;
using Mafi.Core;
using Mafi.Core.Console;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ProgramableNetwork
{
    /// <summary>
    /// Console command <c>pn_exportWebData</c> — writes a self-contained bundle for the WASM web app
    /// (preview + simulator) under <c>&lt;modRoot&gt;/WebExport</c>:
    /// <list type="bullet">
    ///   <item>modules/*.py, core/*.py — the Python sources (the bundle's metadata IS this structure)</item>
    ///   <item>core/ids.py — generated proto descriptors (<see cref="ModuleIdsGenerator.Generate"/>)</item>
    ///   <item>icons/&lt;id&gt;.png — module sprites (best-effort; the app falls back to the symbol text)</item>
    ///   <item>index.json — a file index + icon map (NOT module metadata)</item>
    /// </list>
    /// Discovered by COI's DI via <see cref="GlobalDependencyAttribute"/>; the proto list and sprite db
    /// are injected. Runs on the main thread because reading Unity textures requires it.
    /// </summary>
    [GlobalDependency(RegistrationMode.AsSelf, false, false)]
    public class ModWebExport
    {
        private const string MOD_ID = "ProgramableNetwork";

        private readonly ProtosDb m_protosDb;
        private readonly AssetsDb m_assetsDb;

        public ModWebExport(ProtosDb protosDb, AssetsDb assetsDb)
        {
            m_protosDb = protosDb;
            m_assetsDb = assetsDb;
        }

        [ConsoleCommand(
            invokeOnMainThread: true,
            invokeDuringSync: false,
            documentation: "Exports module sources, generated descriptors and icons for the WASM web " +
                           "preview/simulator to <modRoot>/WebExport.",
            customCommandName: "pn_exportWebData")]
        private string ExportWebData()
        {
            ModManifest manifest = ModsLoader.LoadedAndFailedMods
                .AsEnumerable()
                .FirstOrDefault(x => x.Manifest.Id == MOD_ID)
                ?.Manifest;

            if (manifest == null)
            {
                return $"Mod '{MOD_ID}' not found in loaded mods.";
            }

            string root = manifest.RootDirectoryPath;
            string outDir = Path.Combine(root, "WebExport");
            string modulesOut = Path.Combine(outDir, "modules");
            string coreOut = Path.Combine(outDir, "core");
            string iconsOut = Path.Combine(outDir, "icons");
            Directory.CreateDirectory(modulesOut);
            Directory.CreateDirectory(coreOut);
            Directory.CreateDirectory(iconsOut);

            // The deployed layout puts Custom modules at <root>/Modules/*.py and Core stubs at
            // <root>/Modules/Core/*.py (see DeployToModsFolder in the .csproj).
            string modulesSrc = Path.Combine(root, "Modules");
            string coreSrc = Path.Combine(modulesSrc, "Core");

            List<string> moduleFiles = CopyPython(modulesSrc, modulesOut, topOnly: true);
            List<string> coreFiles = CopyPython(coreSrc, coreOut, topOnly: true);

            // Generated descriptor (runtime-only proto facts) — lives alongside the Core stubs so the
            // web loader resolves `from Core.ids import ...` the same way the game does.
            ModuleProto[] modules = m_protosDb.All<ModuleProto>().OrderBy(m => m.Id.Value).ToArray();
            File.WriteAllText(Path.Combine(coreOut, "ids.py"), ModuleIdsGenerator.Generate(modules), Encoding.UTF8);
            if (!coreFiles.Contains("ids.py"))
            {
                coreFiles.Add("ids.py");
            }

            // Icons (best-effort).
            List<(string id, string file)> iconMap = new List<(string, string)>();
            int iconOk = 0;
            int iconFail = 0;
            foreach (ModuleProto proto in modules)
            {
                string id = proto.Id.Value;
                string file = "icons/" + Sanitize(id) + ".png";
                if (TryExportIcon(SafeIconPath(proto), Path.Combine(iconsOut, Sanitize(id) + ".png")))
                {
                    iconMap.Add((id, file));
                    iconOk++;
                }
                else
                {
                    iconFail++;
                }
            }

            File.WriteAllText(Path.Combine(outDir, "index.json"),
                BuildIndexJson(manifest, moduleFiles, coreFiles, iconMap), Encoding.UTF8);

            return $"Exported {moduleFiles.Count} modules, {coreFiles.Count} core files, " +
                   $"{iconOk} icons ({iconFail} skipped) to '{outDir}'.";
        }

        private static List<string> CopyPython(string srcDir, string destDir, bool topOnly)
        {
            List<string> copied = new List<string>();
            if (!Directory.Exists(srcDir))
            {
                return copied;
            }
            SearchOption option = topOnly ? SearchOption.TopDirectoryOnly : SearchOption.AllDirectories;
            foreach (string path in Directory.GetFiles(srcDir, "*.py", option))
            {
                string name = Path.GetFileName(path);
                File.Copy(path, Path.Combine(destDir, name), overwrite: true);
                copied.Add(name);
            }
            return copied;
        }

        // ModuleProto.IconPath is `Graphics.IconPath`; some protos have no Graphics, so reading it
        // can throw. Guard it here rather than at the (argument-evaluation) call site.
        private static string SafeIconPath(ModuleProto proto)
        {
            try
            {
                return proto.IconPath;
            }
            catch
            {
                return null;
            }
        }

        private bool TryExportIcon(string assetPath, string outFile)
        {
            if (string.IsNullOrEmpty(assetPath))
            {
                return false;
            }
            try
            {
                UnityEngine.Texture2D tex = m_assetsDb.GetSharedTexture(assetPath);
                if (tex == null)
                {
                    return false;
                }
                byte[] png = EncodeReadable(tex);
                if (png == null || png.Length == 0)
                {
                    return false;
                }
                File.WriteAllBytes(outFile, png);
                return true;
            }
            catch
            {
                // Missing asset / unreadable texture — the web app falls back to the module symbol.
                return false;
            }
        }

        /// <summary>
        /// EncodeToPNG fails on textures imported as non-readable (the usual case for bundled sprites),
        /// so blit through a RenderTexture into a CPU-readable copy first.
        /// </summary>
        private static byte[] EncodeReadable(UnityEngine.Texture2D tex)
        {
            UnityEngine.RenderTexture rt = UnityEngine.RenderTexture.GetTemporary(
                tex.width, tex.height, 0,
                UnityEngine.RenderTextureFormat.ARGB32,
                UnityEngine.RenderTextureReadWrite.sRGB);
            UnityEngine.RenderTexture previous = UnityEngine.RenderTexture.active;
            try
            {
                UnityEngine.Graphics.Blit(tex, rt);
                UnityEngine.RenderTexture.active = rt;
                UnityEngine.Texture2D readable = new UnityEngine.Texture2D(
                    tex.width, tex.height, UnityEngine.TextureFormat.RGBA32, false);
                readable.ReadPixels(new UnityEngine.Rect(0, 0, tex.width, tex.height), 0, 0);
                readable.Apply();
                return UnityEngine.ImageConversion.EncodeToPNG(readable);
            }
            finally
            {
                UnityEngine.RenderTexture.active = previous;
                UnityEngine.RenderTexture.ReleaseTemporary(rt);
            }
        }

        private static string BuildIndexJson(
            ModManifest manifest,
            List<string> moduleFiles,
            List<string> coreFiles,
            List<(string id, string file)> iconMap)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("{");
            sb.AppendLine($"  \"mod\": \"{JsonEscape(manifest.Id)}\",");
            sb.AppendLine($"  \"version\": \"{JsonEscape(manifest.Version.ToString())}\",");
            AppendArray(sb, "modules", moduleFiles.Select(f => "modules/" + f), indent: "  ", trailingComma: true);
            AppendArray(sb, "core", coreFiles.Select(f => "core/" + f), indent: "  ", trailingComma: true);

            sb.AppendLine("  \"icons\": {");
            for (int i = 0; i < iconMap.Count; i++)
            {
                string comma = i < iconMap.Count - 1 ? "," : "";
                sb.AppendLine($"    \"{JsonEscape(iconMap[i].id)}\": \"{JsonEscape(iconMap[i].file)}\"{comma}");
            }
            sb.AppendLine("  }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void AppendArray(StringBuilder sb, string name, IEnumerable<string> items, string indent, bool trailingComma)
        {
            string[] arr = items.ToArray();
            sb.AppendLine($"{indent}\"{name}\": [");
            for (int i = 0; i < arr.Length; i++)
            {
                string comma = i < arr.Length - 1 ? "," : "";
                sb.AppendLine($"{indent}  \"{JsonEscape(arr[i])}\"{comma}");
            }
            sb.AppendLine($"{indent}]{(trailingComma ? "," : "")}");
        }

        private static string Sanitize(string id) => id.Replace('/', '_').Replace('\\', '_');

        private static string JsonEscape(string s) =>
            s == null ? "" : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}
