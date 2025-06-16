using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Microsoft.CSharp;
using UnityEngine;
using Mafi.Unity.Ui;

namespace Mafi.Unity;

#if DEV_ONLY
/// <summary>
/// To run demos add them to <see cref="FILE_GROUPS_TO_WATCH"/>. Content will automatically
/// update on save.
///
/// Class has to implement IHotReloadInit to run.
/// </summary>
[GlobalDependency(RegistrationMode.AsSelf)]
public class HotReloadRunner {

	private readonly struct WatchedFileGroup {

		public readonly string RootNamespace;
		public readonly string ClassToRun;
		public readonly string WatchedDirectory;
		public readonly bool WatchRecursively;

		public WatchedFileGroup(
			string rootNamespace,
			string classToRun,
			string watchedDirectory,
			bool watchRecursively
		) {
			RootNamespace = rootNamespace;
			ClassToRun = classToRun;
			WatchedDirectory = watchedDirectory;
			WatchRecursively = watchRecursively;
		}

	}

	private static readonly WatchedFileGroup[] FILE_GROUPS_TO_WATCH = new WatchedFileGroup[] {
		new WatchedFileGroup(
			rootNamespace: "ProgramableNetwork.Ui",
			classToRun: nameof(UiReloadInit),
			watchedDirectory: "Captain of Industry\\Captain of Industry_Data\\Managed\\ProgramableNetwork",
			watchRecursively: true
		)
	};


	private readonly DependencyResolver m_resolver;
	private readonly ImmutableArray<ReloadItemRunner> m_reloadables;
	private int m_reloadsDone = 0;


	public HotReloadRunner(
		IGameLoopEvents gameLoopEvents,
		DependencyResolver resolver,
		IHotReloadConfig config
	) {
		m_resolver = resolver;
		Lyst<ReloadItemRunner> reloadables = new();

		if (config.EnableHotReload || true) {
			gameLoopEvents.SyncUpdate.AddNonSaveable(this, checkFiles);
			foreach (WatchedFileGroup group in FILE_GROUPS_TO_WATCH) {
				if (ReloadItemRunner.TryCreate(this, group, out ReloadItemRunner runner, out string error)) {
					reloadables.Add(runner);
				} else {
					Log.Warning("Failed to create hot-reload runner: " + error);
				}
			}
		}

		m_reloadables = reloadables.ToImmutableArray();
	}

	private void checkFiles(GameTime time) {
		foreach (ReloadItemRunner runner in m_reloadables) {
			runner.CheckFileChange();
		}
	}

	private bool run(Lyst<string> contentToCompile, string rootNamespace, string classToRun, ReloadItemRunner runner) {
		++m_reloadsDone;

		string[] codeToCompile = new string[contentToCompile.Count];
		for (int i = 0; i < contentToCompile.Count; i++) {
			string code = contentToCompile[i];

			Option<string> result = processCodeToCompile(code, rootNamespace);
			if (result.IsNone) {
				return false;
			}
			codeToCompile[i] = result.Value;
		}

		string[] assembliesToAdd = {
			"ProgramableNetwork.dll"
		};

		CSharpCodeProvider provider = new CSharpCodeProvider();
		CompilerParameters parameters = new CompilerParameters { GenerateInMemory = true };
		foreach (var assem in AppDomain.CurrentDomain.GetAssemblies()) {
			// Skip dynamic assemblies
			if (!assem.IsDynamic) {
				foreach (string dllName in assembliesToAdd) {
					if (assem.Location.EndsWith(dllName)) {
						parameters.ReferencedAssemblies.Add(assem.Location);
						// Log.Error($"{assembly.FullName} {assembly.Location}");
					}
				}
			}
		}
		// Pass our flags to the compiler
		Lyst<string> options = new Lyst<string>();
#if DEV_ONLY
		options.Add("DEV_ONLY");
#endif
#if CHEATS_ENABLED
		options.Add("CHEATS_ENABLED");
#endif
#if RELEASE_CHEATS
		options.Add("RELEASE_CHEATS");
#endif
		if (options.IsNotEmpty) {
			parameters.CompilerOptions = $"/define:{options.JoinStrings(";")}";
		}

		// Keep temp files so we can extract error class names
		parameters.TempFiles.KeepFiles = true;
		CompilerResults results = provider.CompileAssemblyFromSource(parameters, codeToCompile);
		Dict<string, string> resolvedClasses = new Dict<string, string>();

		if (results.Errors.HasErrors) {
			StringBuilder sb = new StringBuilder($"Compilation error for reload set under '{classToRun}':");
			sb.AppendLine();
			foreach (CompilerError error in results.Errors) {
				if (resolvedClasses.TryGetValue(error.FileName, out string className) == false) {
					className = getClassNameFromFile(error.FileName);
					resolvedClasses.Add(error.FileName, className);
					// Log.Error(File.ReadAllText(error.FileName));
				}
				sb.AppendLine($"[{error.Line}]: {(error.IsWarning ? "W" : "E")}: {error.ErrorText} at '{className}'");
			}
			Log.Error(sb.ToString());
		}
		foreach (string file in parameters.TempFiles) {
			File.Delete(file);
		}
		if (results.Errors.HasErrors) {
			return false;
		}

		Assembly assembly = results.CompiledAssembly;
		string newNamespace = renameNamespace(rootNamespace);
		Type programType = assembly.GetType($"{newNamespace}.{classToRun}");
		if (programType == null) {
			Log.Error($"Couldn't find {newNamespace}.{classToRun} to run!");
			return false;
		}

		runner.ClearCurrentDemo();
		object demo = m_resolver.Instantiate(programType);
		if (demo is not IHotReloadInit gd) {
			Log.Error("Demo needs to implement IHotReloadable!");
			return false;
		}
		runner.SetNewDemo(gd);
		return true;
	}

	private string getClassNameFromFile(string filePath) {
		if (File.Exists(filePath) == false) {
			return "unknown";
		}

		string fileContent = File.ReadAllText(filePath);
		string firstClassRegex = @"^\s*(public|private|internal)?\s*class\s+(\w+)";
		Match matchFirstClass = Regex.Match(fileContent, firstClassRegex, RegexOptions.Multiline);
		if (matchFirstClass.Success == false) {
			return "unknown";
		}
		return matchFirstClass.Groups[2].Value;
	}

	private Option<string> processCodeToCompile(string codeToCompile, string rootNamespace) {
		// Unity's Mono compiler can't handle namespaces without curly braces
		var regex = new Regex(@"namespace\s+([A-Za-z0-9_.]+);");
		var match = regex.Match(codeToCompile);
		if (match.Success == false) {
			Log.Error("Failed to parse the namespace");
			return Option<string>.None;
		}
		string oldNamespaceName = match.Groups[1].Value;
		string newNamespaceName = renameNamespace(rootNamespace, oldNamespaceName);

		// Replace the old namespace declaration with the new format, also rename the namespace
		string formattedNamespace = $"namespace {newNamespaceName} {{\n";
		codeToCompile = regex.Replace(codeToCompile, formattedNamespace);

		// Replace all affected usings
		codeToCompile = replaceUsings(codeToCompile, rootNamespace, renameNamespace(rootNamespace));

		// Unit's compiler does not support typedefs
		codeToCompile = codeToCompile.Replace("UiAssets", "Assets.Unity.UserInterface");

		codeToCompile += "\n}";

		return codeToCompile;
	}

	private string renameNamespace(string oldPrefix, string oldFullNamespace = null) {
		oldFullNamespace ??= oldPrefix;
		return oldFullNamespace.Replace(oldPrefix, $"{oldPrefix}{m_reloadsDone}");
	}

	private static string replaceUsings(string fileContent, string namespacePrefix, string newPrefix) {
		// Escape dots in namespacePrefix for Regex matching
		string escapedPrefix = Regex.Escape(namespacePrefix);
		string pattern = $@"\busing\s+({escapedPrefix}(\.[\w.]+)?);";
		// Replace matched using statements with the new prefix
		string result = Regex.Replace(fileContent, pattern, match =>
		{
			string matchedNamespace = match.Groups[1].Value;
			return $"using {matchedNamespace.Replace(namespacePrefix, newPrefix)};";
		});
		return result;
	}


	private class ReloadItemRunner {

		private readonly HotReloadRunner m_playground;
		private readonly WatchedFileGroup m_watchedGroup;
		private readonly string m_pathToMonitor;

		private Option<IHotReloadInit> m_runningDemo;

		private bool m_fileChanged;

		private readonly Lyst<string> m_filesContentsTmp = new();


		private ReloadItemRunner(
			HotReloadRunner playground,
			WatchedFileGroup watchedGroup,
			string pathToMonitor
		) {
			m_playground = playground;
			m_watchedGroup = watchedGroup;
			m_pathToMonitor = pathToMonitor;

			FileSystemWatcher fsWatcher = new(pathToMonitor, "*.cs");
			fsWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
			fsWatcher.IncludeSubdirectories = watchedGroup.WatchRecursively;
			fsWatcher.EnableRaisingEvents = true;
			fsWatcher.Changed += onFileChanged;
			m_fileChanged = true;
		}

		public static bool TryCreate(
			HotReloadRunner playground,
			WatchedFileGroup watchedGroup,
			out ReloadItemRunner runner,
			out string error
		) {

			string rootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", ".."));
			string pathToMonitor = Path.Combine(rootPath, watchedGroup.WatchedDirectory);

			if (Directory.Exists(pathToMonitor)) {
				runner = new ReloadItemRunner(playground, watchedGroup, pathToMonitor);
				error = "";
				return true;
			} else {
				runner = null;
				error = $"Root directory not found, hot reload won't be available:\n{pathToMonitor}";
				return false;
			}
		}


		private void onFileChanged(object sender, FileSystemEventArgs e) {
			// Note: This could be invoked on non-main thread, maybe.
			if (e.FullPath.EndsWith(".cs")) {
				Log.Info($"File change detected: {e.FullPath}");
				m_fileChanged = true;
			}
		}

		internal void CheckFileChange() {
			if (m_fileChanged) {
				m_fileChanged = false;
				ReloadFiles();
			}
		}

		internal void ReloadFiles() {
			m_filesContentsTmp.Clear();
			if (File.Exists(m_pathToMonitor) && m_pathToMonitor.EndsWith(".cs")) {
				m_filesContentsTmp.Add(File.ReadAllText(m_pathToMonitor));
			} else if (Directory.Exists(m_pathToMonitor)) {
				string[] files = Directory.GetFiles(m_pathToMonitor, "*.cs", SearchOption.AllDirectories);
				foreach (string filePath in files) {
					m_filesContentsTmp.Add(File.ReadAllText(filePath));
				}
			}
			m_playground.run(m_filesContentsTmp, m_watchedGroup.RootNamespace, m_watchedGroup.ClassToRun, this);
		}

		internal void ClearCurrentDemo() {
			m_runningDemo.ValueOrNull?.Destroy();
			m_runningDemo = Option<IHotReloadInit>.None;
		}

		internal void SetNewDemo(IHotReloadInit demo) {
			m_runningDemo = demo.SomeOption();
		}
	}

}
#endif
