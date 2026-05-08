using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CustomAssets.ModBuilder
{
    internal sealed class BuildOptions
    {
        public string PackDir { get; set; }
        public string GeneratedRoot { get; set; }
        public string CoiRoot { get; set; }
        public string CoiMods { get; set; }
        public string LibCsproj { get; set; }
        public string LibDll { get; set; }
        public PackMode Mode { get; set; } = PackMode.Auto;
        public string Configuration { get; set; } = "Release";
        public bool NoBuild { get; set; }
        public bool Supporter { get; set; }
        public bool TrainsDlc { get; set; }
        public string IdOut { get; set; }
        public string CsprojOut { get; set; }
    }

    internal sealed class PackBuilder
    {
        private readonly BuildOptions _options;

        public PackBuilder(BuildOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public int Run()
        {
            var manifestPath = Path.Combine(_options.PackDir, "manifest.json");
            var manifest = Manifest.Load(manifestPath);
            Console.WriteLine($"[ModBuilder] Pack {_options.PackDir} -> mod id '{manifest.Id}' v{manifest.RawVersion}");

            var mode = ResolveMode(_options.PackDir, _options.Mode);
            Console.WriteLine($"[ModBuilder] Mode: {mode}");

            var generatedDir = Path.Combine(_options.GeneratedRoot, manifest.Id);
            Directory.CreateDirectory(generatedDir);

            var csPath = Path.Combine(generatedDir, manifest.Id + ".cs");
            var csprojPath = Path.Combine(generatedDir, manifest.Id + ".csproj");

            File.WriteAllText(csPath, RenderCs(manifest));
            File.WriteAllText(csprojPath, RenderCsproj(manifest, mode, generatedDir));

            Console.WriteLine($"[ModBuilder] Wrote {csPath}");
            Console.WriteLine($"[ModBuilder] Wrote {csprojPath}");

            if (!string.IsNullOrEmpty(_options.IdOut))
                File.WriteAllText(_options.IdOut, manifest.Id);
            if (!string.IsNullOrEmpty(_options.CsprojOut))
                File.WriteAllText(_options.CsprojOut, csprojPath);

            if (_options.NoBuild)
            {
                Console.WriteLine("[ModBuilder] --no-build requested, skipping dotnet build.");
                return 0;
            }

            return InvokeDotnetBuild(csprojPath);
        }

        private static PackMode ResolveMode(string packDir, PackMode requested)
        {
            if (requested != PackMode.Auto) return requested;
            var bundlesDir = Path.Combine(packDir, "AssetBundles");
            if (Directory.Exists(bundlesDir) &&
                Directory.EnumerateFileSystemEntries(bundlesDir).Any())
                return PackMode.Bundle;
            return PackMode.Generic;
        }

        private static string RenderCs(Manifest manifest)
        {
            return TemplateLoader.Load("ModClass.tmpl")
                .Replace("{{ID}}", manifest.Id);
        }

        private string RenderCsproj(Manifest manifest, PackMode mode, string generatedDir)
        {
            var template = TemplateLoader.Load("Mod.csproj.tmpl");

            var packDirAbs = Path.GetFullPath(_options.PackDir);
            var libRef = BuildLibReference(generatedDir);
            var (assetBundlesItem, assetBundlesCopy) = BuildAssetBundleFragments(mode, packDirAbs);

            var values = new Dictionary<string, string>
            {
                ["{{ID}}"] = manifest.Id,
                ["{{NORMALIZED_VERSION}}"] = manifest.NormalizedVersion,
                ["{{RAW_VERSION}}"] = manifest.RawVersion,
                ["{{DISPLAY_TITLE}}"] = EscapeForXml(manifest.DisplayTitle),
                ["{{AUTHOR}}"] = EscapeForXml(manifest.Author),
                ["{{COI_ROOT}}"] = _options.CoiRoot ?? string.Empty,
                ["{{COI_MODS}}"] = _options.CoiMods ?? string.Empty,
                ["{{PACK_DIR}}"] = packDirAbs,
                ["{{LIB_REFERENCE}}"] = libRef,
                ["{{ASSETBUNDLES_ITEMGROUP}}"] = assetBundlesItem,
                ["{{ASSETBUNDLES_COPY}}"] = assetBundlesCopy,
                ["{{SUPPORTER}}"] = _options.Supporter ? "true" : "false",
                ["{{TRAINS_DLC}}"] = _options.TrainsDlc ? "true" : "false",
            };

            foreach (var kv in values)
                template = template.Replace(kv.Key, kv.Value);
            return template;
        }

        private string BuildLibReference(string generatedDir)
        {
            if (!string.IsNullOrEmpty(_options.LibCsproj))
            {
                var rel = MakeRelative(generatedDir, _options.LibCsproj);
                return $"<ProjectReference Include=\"{EscapeForXml(rel)}\" />";
            }
            if (!string.IsNullOrEmpty(_options.LibDll))
            {
                var dllPath = Path.GetFullPath(_options.LibDll);
                return
                    "<Reference Include=\"CustomAssets\">\n" +
                    $"      <HintPath>{EscapeForXml(dllPath)}</HintPath>\n" +
                    "      <Private>false</Private>\n" +
                    "    </Reference>";
            }
            throw new InvalidOperationException(
                "No library reference. Pass --lib-csproj <path-to-CustomAssets.csproj> or --lib-dll <path-to-CustomAssets.dll>.");
        }

        private static (string itemGroup, string copyStep) BuildAssetBundleFragments(PackMode mode, string packDirAbs)
        {
            if (mode != PackMode.Bundle) return (string.Empty, string.Empty);
            var itemGroup =
                $"    <AssetBundles Include=\"{EscapeForXml(packDirAbs)}\\AssetBundles\\**\\*\" Exclude=\"**\\AssetBundles;**\\AssetBundles.manifest\" />\n";
            var copyStep =
                "    <Copy SourceFiles=\"@(AssetBundles)\" DestinationFolder=\"$(COI_MODS)\\{{ID}}\\AssetBundles\" />\n";
            return (itemGroup, copyStep);
        }

        private static string MakeRelative(string fromDir, string toPath)
        {
            var fromUri = new Uri(EnsureTrailingSeparator(Path.GetFullPath(fromDir)));
            var toUri = new Uri(Path.GetFullPath(toPath));
            var rel = Uri.UnescapeDataString(fromUri.MakeRelativeUri(toUri).ToString());
            return rel.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string EnsureTrailingSeparator(string path) =>
            path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? path : path + Path.DirectorySeparatorChar;

        private static string EscapeForXml(string value)
        {
            if (value == null) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");
        }

        private int InvokeDotnetBuild(string csprojPath)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"build \"{csprojPath}\" -c {_options.Configuration} -nologo",
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = false,
                CreateNoWindow = false,
            };
            Console.WriteLine($"[ModBuilder] Running: dotnet {psi.Arguments}");
            try
            {
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                        Console.Error.WriteLine($"[ModBuilder] dotnet build failed with exit code {process.ExitCode}");
                    return process.ExitCode;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Console.Error.WriteLine($"[ModBuilder] Failed to invoke 'dotnet' ({ex.Message}). Install the .NET SDK or build the generated csproj manually: {csprojPath}");
                return 127;
            }
        }
    }
}
