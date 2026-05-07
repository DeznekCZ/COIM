using Mafi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    /// <summary>
    /// Module-id replacement and pin-rename table consulted by <see cref="Module.initContexts"/>
    /// (and the cross-module fixup in <see cref="Controller.initContexts"/>) when a save
    /// references a prototype that is no longer registered, or whose pin ids changed in a
    /// later mod version.
    ///
    /// Two flavours of entry:
    /// <list type="bullet">
    /// <item><description><b>Proto swap</b> — old <c>ModuleProto.ID</c> mapped to a still-registered
    ///   <see cref="Migration.Replacement"/>, optionally with extension counts that reproduce
    ///   the removed module's pin layout (e.g. <c>Sum_4</c> → <c>Sum</c> with
    ///   <c>InputExtensionCount = 2</c>).</description></item>
    /// <item><description><b>Pin rename</b> — same proto id on both sides, but pin ids on the
    ///   prototype were renamed in a later version.  Use <see cref="RegisterPinRename"/> with
    ///   an <see cref="Migration.UntilVersion"/> gate so the rename applies only to saves
    ///   written before the rename shipped.</description></item>
    /// </list>
    ///
    /// Connection preservation: <see cref="Migration.InputIdMap"/> renames keys of the
    /// migrated module's own <c>InputModules</c> dict (incoming cables); <see cref="Migration.OutputIdMap"/>
    /// is applied during the controller's post-load pass to remap <c>OutputId</c> on every
    /// other module's <c>ModuleConnector</c> that points back to this one (outgoing cables).
    /// Anything that doesn't have an entry here falls through to the Phantom path.
    /// </summary>
    public class Deprecation
    {
        public readonly struct Migration
        {
            /// <summary>
            /// Replacement prototype id, or <c>null</c> for a pure pin-rename entry that
            /// keeps the same proto.  When non-null, <see cref="Module.initContexts"/>
            /// swaps the prototype during deserialization.
            /// </summary>
            public readonly ModuleProto.ID? Replacement;
            public readonly int? InputExtensionCount;
            public readonly int? OutputExtensionCount;
            public readonly int? DisplayExtensionCount;

            /// <summary>
            /// If set, the migration applies only when <c>module.LoadedVersion &lt; UntilVersion</c>.
            /// Use this for pin renames bound to a specific save-version bump (the rename shipped
            /// at version N, so saves at N-1 still carry the old ids and need remapping; saves
            /// at N+ already wrote the new ids and skip this entry).  <c>null</c> = always apply.
            /// </summary>
            public readonly int? UntilVersion;

            /// <summary>
            /// Old-to-new pin id map for the migrated module's <b>inputs</b>.  Keys of
            /// <c>InputModules</c> matching an entry are renamed to the new id (preserving
            /// the connection); entries with no map hit are left as-is and validated later.
            /// </summary>
            public readonly IReadOnlyDictionary<string, string> InputIdMap;

            /// <summary>
            /// Old-to-new pin id map for the migrated module's <b>outputs</b>.  Applied to
            /// every consumer's <c>ModuleConnector.OutputId</c> that points back at this
            /// module so cables drawn from renamed outputs reattach to the right new pin.
            /// </summary>
            public readonly IReadOnlyDictionary<string, string> OutputIdMap;

            public Migration(ModuleProto.ID? replacement,
                int? inputExt = null, int? outputExt = null, int? displayExt = null,
                int? untilVersion = null,
                IReadOnlyDictionary<string, string> inputIdMap = null,
                IReadOnlyDictionary<string, string> outputIdMap = null)
            {
                Replacement = replacement;
                InputExtensionCount = inputExt;
                OutputExtensionCount = outputExt;
                DisplayExtensionCount = displayExt;
                UntilVersion = untilVersion;
                InputIdMap = inputIdMap;
                OutputIdMap = outputIdMap;
            }

            public bool AppliesAt(int loadedVersion)
                => !UntilVersion.HasValue || loadedVersion < UntilVersion.Value;
        }

        public static Dictionary<ModuleProto.ID, Migration> Deprecations { get; private set; }
            = new Dictionary<ModuleProto.ID, Migration>();

        /// <summary>
        /// Register a proto migration: id rename, optional extension counts, optional
        /// version gate, and optional pin-id remaps.  Plain rename = pass only
        /// <paramref name="deprecated"/> and <paramref name="replacement"/>.  Pin-id maps
        /// preserve cables that would otherwise be silently dropped by the controller
        /// post-load pass when the new prototype's pin ids differ from the old one's.
        /// </summary>
        public static void RegisterDeprecation(ModuleProto.ID deprecated, ModuleProto.ID replacement,
            int? inputExt = null, int? outputExt = null, int? displayExt = null,
            int? untilVersion = null,
            IReadOnlyDictionary<string, string> inputIdMap = null,
            IReadOnlyDictionary<string, string> outputIdMap = null)
        {
            Deprecations[deprecated] = new Migration(replacement, inputExt, outputExt, displayExt,
                untilVersion, inputIdMap, outputIdMap);
        }

        /// <summary>
        /// Pin-rename-only entry for a still-registered prototype: no proto swap, no
        /// extension counts, just rename input/output pin ids on saves older than
        /// <paramref name="untilVersion"/>.  Use when a prototype kept its id but its
        /// <c>AddInput</c> / <c>AddOutput</c> ids changed in a later mod version.
        /// </summary>
        public static void RegisterPinRename(ModuleProto.ID protoId, int untilVersion,
            IReadOnlyDictionary<string, string> inputIdMap = null,
            IReadOnlyDictionary<string, string> outputIdMap = null)
        {
            Deprecations[protoId] = new Migration(replacement: null,
                untilVersion: untilVersion,
                inputIdMap: inputIdMap, outputIdMap: outputIdMap);
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
