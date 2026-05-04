using Mafi.Core;
using Mafi.Core.Console;
using Mafi.Core.Mods;
using System.IO;
using System.Linq;
using Mafi;

namespace ProgramableNetwork;

/// <summary>
/// Console commands for this mod. Discovered by COI's DI via <see cref="GlobalDependencyAttribute"/>;
/// every method tagged with <see cref="ConsoleCommandAttribute"/> becomes invokable from the in-game console.
/// </summary>
[GlobalDependency(RegistrationMode.AsSelf, false, false)]
public class ModConsoleCommands
{
    private const string MOD_ID = "ProgramableNetwork";

    [ConsoleCommand(
        invokeOnMainThread: false,
        invokeDuringSync: false,
        documentation: "Exports every en-US translation key registered by the ProgramableNetwork mod " +
                       "to <modRoot>/Translations/en.json. Translators copy that file, rename it to the " +
                       "target language (e.g. cs.json, de.json) and replace the right-hand strings.",
        customCommandName: "pn_exportTranslations")]
    private string ExportTranslations()
    {
        ModManifest manifest = ModsLoader.LoadedAndFailedMods
            .AsEnumerable()
            .FirstOrDefault(x => x.Manifest.Id == MOD_ID)
            ?.Manifest;

        if (manifest == null)
        {
            return $"Mod '{MOD_ID}' not found in loaded mods.";
        }

        ModTranslations.ExportEnglish(manifest);
        return $"Exported English translation template to '{Path.Combine(manifest.RootDirectoryPath, "Translations", "en.json")}'.";
    }
}