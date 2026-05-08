using System;

namespace CustomAssets.StubBuilder.Commands
{
    internal static class GameStubsCommand
    {
        private const string Usage =
@"StubBuilder game-stubs - regenerate Mafi/Mafi.Core/Mafi.Base type stubs.

Status: SCAFFOLDED, NOT YET IMPLEMENTED.

The existing src/GenerateIds project walks Mafi assemblies via strongly-typed
references (typeof(Fix32), typeof(CoreMod), typeof(BaseMod)) and emits stubs to
src/Recipes/Mafi/**. Porting that logic here requires either:
  (a) project-referencing the same Mafi DLLs (re-introduces the COI_ROOT compile
      dependency that we want to avoid for a portable tool), or
  (b) a full reflection-only port using Assembly.LoadFrom + late binding.

Until (b) is done, run src/GenerateIds in Visual Studio to refresh the Mafi
stubs. This stub is here so future revisions can land it without changing the
StubBuilder CLI surface.

USAGE
    StubBuilder.exe game-stubs --coi-root <path> --output-root <dir>

OPTIONS
    --coi-root <path>     COI install (containing Captain of Industry_Data\Managed).
    --output-root <dir>   Where to emit Mafi/**/__init__.py (e.g. src\Recipes).
    -h, --help            Show this help.
";

        public static int Run(string[] args)
        {
            // Accept and ignore the documented args for forward compatibility.
            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine(Usage);
                        return 0;
                }
            }

            Console.Error.WriteLine("[StubBuilder] game-stubs is not yet implemented.");
            Console.Error.WriteLine("[StubBuilder] Use src/GenerateIds (run from Visual Studio) to refresh Mafi.* stubs.");
            Console.Error.WriteLine("[StubBuilder] See `StubBuilder game-stubs --help` for the planned interface.");
            return 2;
        }
    }
}
