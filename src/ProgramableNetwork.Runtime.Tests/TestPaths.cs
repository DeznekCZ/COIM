using System;
using System.IO;

namespace ProgramableNetwork.Runtime.Tests
{
    /// <summary>Locates the repo's Python module files by walking up from the test output directory.</summary>
    internal static class TestPaths
    {
        public static string Module(string fileName) =>
            Path.Combine(ModulesRoot(), "Custom", fileName);

        public static string ModulesRoot()
        {
            DirectoryInfo dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                string candidate = Path.Combine(dir.FullName, "src", "ProgramableNetwork.Modules");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }
            throw new DirectoryNotFoundException("Could not locate src/ProgramableNetwork.Modules from " + AppContext.BaseDirectory);
        }
    }
}
