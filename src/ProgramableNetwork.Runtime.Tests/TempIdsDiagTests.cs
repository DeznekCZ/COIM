using System;
using System.IO;
using ProgramableNetwork.Runtime.Sim;
using Xunit;
using Xunit.Abstractions;

namespace ProgramableNetwork.Runtime.Tests
{
    public class TempIdsDiagTests
    {
        private readonly ITestOutputHelper output;
        public TempIdsDiagTests(ITestOutputHelper output) { this.output = output; }

        [Fact]
        public void Load_real_export_ids()
        {
            string path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Captain of Industry", "Mods", "ProgramableNetwork", "WebExport", "core", "ids.py");
            if (!File.Exists(path)) { output.WriteLine("NOT FOUND: " + path); return; }

            string raw = File.ReadAllText(path);
            output.WriteLine($"first char U+{((int)raw[0]):X4}, length {raw.Length}");
            try
            {
                var defs = ModuleLoader.LoadFile("ids.py", raw);
                output.WriteLine($"Loaded {defs.Count} module definitions (raw).");
            }
            catch (Exception ex)
            {
                output.WriteLine("RAW FAILED: " + ex.GetType().Name + ": " + ex.Message);
            }

            string noBom = raw.TrimStart('﻿');
            try
            {
                var defs = ModuleLoader.LoadFile("ids.py", noBom);
                output.WriteLine($"Loaded {defs.Count} module definitions (BOM stripped).");
            }
            catch (Exception ex)
            {
                output.WriteLine("NOBOM FAILED: " + ex.GetType().Name + ": " + ex.Message);
            }
        }
    }
}
