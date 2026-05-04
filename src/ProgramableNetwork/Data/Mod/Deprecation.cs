using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    /// <summary>
    /// Module-id replacement table consulted by <see cref="Module.initContexts"/> when a
    /// save references a prototype that is no longer registered.  Each entry maps the
    /// removed id to a still-registered prototype, optionally with extension counts that
    /// reproduce the removed module's pin layout (e.g. <c>Sum_4</c> → <c>Sum</c> with
    /// <c>InputExtensionCount = 2</c> so the migrated module retains four inputs).
    ///
    /// Entries with no ext counts behave like a plain id-rename; entries with counts also
    /// run a post-proto-swap fixup that pins the module's <c>InputExtensionCount</c> /
    /// <c>OutputExtensionCount</c> to the recorded values (clamped to the new prototype's
    /// max extensions).  Anything that doesn't have an entry here falls through to the
    /// Phantom path.
    /// </summary>
    public class Deprecation
    {
        public readonly struct Migration
        {
            public readonly ModuleProto.ID Replacement;
            public readonly int? InputExtensionCount;
            public readonly int? OutputExtensionCount;
            public readonly int? DisplayExtensionCount;

            public Migration(ModuleProto.ID replacement, int? inputExt = null, int? outputExt = null, int? displayExt = null)
            {
                Replacement = replacement;
                InputExtensionCount = inputExt;
                OutputExtensionCount = outputExt;
                DisplayExtensionCount = displayExt;
            }
        }

        public static Dictionary<ModuleProto.ID, Migration> Deprecations { get; private set; }
            = new Dictionary<ModuleProto.ID, Migration>();

        /// <summary>Plain id rename — no extension adjustment.</summary>
        public static void RegisterDeprecation(ModuleProto.ID deprecated, ModuleProto.ID replacement)
        {
            Deprecations[deprecated] = new Migration(replacement);
        }

        /// <summary>
        /// Id rename plus optional extension counts — useful when the removed module was
        /// the wider sibling of an extensible one and we need to grow the migrated
        /// module's pin / display extent to match what the player saved.
        /// </summary>
        public static void RegisterDeprecation(ModuleProto.ID deprecated, ModuleProto.ID replacement,
            int? inputExt = null, int? outputExt = null, int? displayExt = null)
        {
            Deprecations[deprecated] = new Migration(replacement, inputExt, outputExt, displayExt);
        }

        public static ModuleProto.ID? GetAlternative(ModuleProto.ID original)
        {
            if (Deprecations?.TryGetValue(original, out var migration) ?? false) {
                return migration.Replacement;
            } else {
                return null;
            }
        }

        public static Migration? GetMigration(ModuleProto.ID original)
        {
            if (Deprecations?.TryGetValue(original, out var migration) ?? false) {
                return migration;
            } else {
                return null;
            }
        }
    }
}
