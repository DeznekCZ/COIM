using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ProgramableNetwork.Runtime.Sim;
using Xunit;
using Xunit.Abstractions;

namespace ProgramableNetwork.Runtime.Tests
{
    /// <summary>
    /// Smoke test: every Custom/*.py must tokenize, parse and have its class body executed without
    /// throwing, and modules deriving from Module must surface as definitions. Catches loader/parser
    /// gaps across the whole module set, not just hysteresis.
    /// </summary>
    public class LoadAllModulesTests
    {
        private readonly ITestOutputHelper output;

        public LoadAllModulesTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void All_custom_modules_load()
        {
            string customDir = Path.Combine(TestPaths.ModulesRoot(), "Custom");
            string[] all = Directory.GetFiles(customDir, "*.py", SearchOption.AllDirectories);
            // Controller-template files are a separate concept (predefined controller layouts), not
            // simulatable modules — out of scope for the runtime/simulator. Skip them here.
            List<string> files = new List<string>();
            foreach (string f in all)
            {
                if (!Path.GetFileName(f).Contains("template"))
                {
                    files.Add(f);
                }
            }
            Assert.NotEmpty(files);

            int totalModules = 0;
            List<string> failures = new List<string>();
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                try
                {
                    List<ModuleDefinition> defs = ModuleLoader.LoadFile(name, File.ReadAllText(file));
                    totalModules += defs.Count;
                }
                catch (Exception ex)
                {
                    failures.Add($"{name}: {ex.GetType().Name}: {ex.Message}");
                }
            }

            output.WriteLine($"Loaded {files.Count} files, {totalModules} module definitions.");
            if (failures.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"{failures.Count}/{files.Count} module files failed to load:");
                foreach (string f in failures)
                {
                    sb.AppendLine("  - " + f);
                }
                Assert.Fail(sb.ToString());
            }

            Assert.True(totalModules > 0, "No module definitions were produced.");
        }
    }
}
