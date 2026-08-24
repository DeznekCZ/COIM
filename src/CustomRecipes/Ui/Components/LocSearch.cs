using System;
using System.Text;
using Mafi;
using Mafi.Localization;

namespace CustomAssets.Ui.Components;

/// <summary>
/// Search-text helpers for picker popups that list GAME protos.
///
/// A proto's display name is localized — under a Czech game the steel plate
/// reads "Ocelový plát", so a search box matching only
/// <c>LocStr.TranslatedString</c> never finds it when the modder types the
/// English name they know from the wiki, the Python api or another mod's
/// source. Mafi keeps the en-US original next to every translation
/// (<see cref="LocalizationManager.GetUsEnStringFor"/>), so the pickers build
/// a BILINGUAL haystack: current language + English, both lower-cased.
///
/// Under an English game the two halves are identical and
/// <see cref="EnglishOrNull"/> returns null, so nothing is duplicated.
/// </summary>
public static class LocSearch {

    /// The en-US original behind <paramref name="str"/>, or null when the
    /// game already runs in English, when the string has no registered
    /// en-US entry (mod-defined LocStrs created at runtime), or when the
    /// lookup throws.
    ///
    /// Guarded by <see cref="LocalizationManager.HasTranslationId"/> because
    /// <c>GetUsEnStringFor</c> logs an ERROR for unknown ids — one per proto
    /// per popup open would drown the log.
    public static string EnglishOrNull(LocStr str) {
        string id = str.Id;
        if (string.IsNullOrEmpty(id)) {
            return null;
        }
        try {
            if (!LocalizationManager.HasTranslationId(id)) {
                return null;
            }
            string enUs = LocalizationManager.GetUsEnStringFor(str);
            if (string.IsNullOrEmpty(enUs)) {
                return null;
            }
            if (string.Equals(enUs, str.TranslatedString, StringComparison.Ordinal)) {
                return null;
            }
            return enUs;
        } catch (Exception ex) {
            Log.Warning("LocSearch: could not read the en-US original for '"
                + id + "' — " + ex.Message);
            return null;
        }
    }

    /// Translated name plus its English original, space separated. Returns
    /// just the translated name under an English game.
    public static string Bilingual(LocStr str) {
        string english = EnglishOrNull(str);
        if (english == null) {
            return str.TranslatedString ?? "";
        }
        return str.TranslatedString + " " + english;
    }

    /// Append <paramref name="str"/> in both languages to a haystack under
    /// construction, with a separating space. Null/empty parts are skipped.
    public static void AppendBilingual(StringBuilder sb, LocStr str) {
        Append(sb, str.TranslatedString);
        Append(sb, EnglishOrNull(str));
    }

    /// Append a plain (already-final) fragment to a haystack, with a
    /// separating space. Null/empty fragments are skipped.
    public static void Append(StringBuilder sb, string text) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }
        if (sb.Length > 0) {
            sb.Append(' ');
        }
        sb.Append(text);
    }
}
