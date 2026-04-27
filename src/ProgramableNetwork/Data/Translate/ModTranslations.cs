using Mafi;
using Mafi.Collections;
using Mafi.Core.Mods;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace ProgramableNetwork
{
	/// <summary>
	/// Loads mod-specific translation files into <c>LocalizationManager.s_data</c> via reflection.
	///
	/// COI loads exactly one translation JSON (from <c>&lt;WorkDir&gt;/Translations/&lt;lang&gt;.json</c>)
	/// at startup, and <c>LocalizationManager.TryLoadTranslationsFrom</c> rejects further calls once
	/// any <c>Loc.Str</c> has run — which is true by the time our mod loads. To still ship per-mod
	/// translations we splice our entries into the already-loaded dictionary.
	///
	/// File layout (next to the mod DLL): <c>Translations/&lt;LangFileName&gt;.json</c>, same JSON
	/// schema as the base game (<c>[ ["key", "translation"], ... ]</c>).
	/// </summary>
	public static class ModTranslations
	{
		private const string TRANSLATIONS_DIR = "Translations";
		private const string ENGLISH_TEMPLATE_FILE = "en.json";

		/// <summary>
		/// Key prefixes recognized as belonging to this mod when exporting an English template.
		/// All Loc.Str / Proto.CreateStr ids registered by this mod start with one of these.
		/// </summary>
		private static readonly string[] MOD_KEY_PREFIXES =
		{
			"ProgramableNetwork_",
			"ResearchProgramableNetwork_",
			"__PHANTOM_CONTROLLER",
			"__PHANTOM_DISPLAY",
			"__PHANTOM__MODULE__"
		};

		private static bool s_loaded;

		/// <summary>
		/// Idempotent. Loads from <c>&lt;manifest.RootDirectoryPath&gt;/Translations/&lt;lang.FileName&gt;</c>.
		/// Preferred entry point — call from <c>RegisterPrototypes</c> with the manifest passed to the
		/// <see cref="ModDefinition"/> constructor.
		/// </summary>
		public static bool Load(ModManifest manifest)
		{
			if (s_loaded) {
				return true;
			}
			s_loaded = true;
			string modRoot = manifest?.RootDirectoryPath;
			try
			{
				LocalizationManager.LangInfo lang = LocalizationManager.CurrentLangInfo;
				if (lang.CultureInfoId == "en-US")
				{
					// English uses the second arg of Loc.Str(...) directly; nothing to inject.
					return true;
				}

				if (string.IsNullOrEmpty(modRoot))
				{
					return true;
				}

				string filePath = Path.Combine(modRoot, TRANSLATIONS_DIR, lang.FileName);
				if (!File.Exists(filePath))
				{
					Log.Info($"[ProgramableNetwork] No translation file for '{lang.CultureInfoId}' at '{filePath}'.");
					return true;
				}

				string json = File.ReadAllText(filePath);
				if (!LocalizationUtils.TryParseJsonFileData(json, out Dict<string, LocalizationManager.LocData> parsed, out string error))
				{
					Log.Error($"[ProgramableNetwork] Failed to parse '{filePath}': {error}");
					return true;
				}

				FieldInfo sDataField = typeof(LocalizationManager).GetField("s_data", BindingFlags.NonPublic | BindingFlags.Static);
				if (sDataField == null)
				{
					Log.Error("[ProgramableNetwork] LocalizationManager.s_data field not found via reflection.");
					return true;
				}

				Dict<string, LocalizationManager.LocData> sData = sDataField.GetValue(null) as Dict<string, LocalizationManager.LocData>;
				if (sData == null)
				{
					sData = new Dict<string, LocalizationManager.LocData>();
					sDataField.SetValue(null, sData);
				}

				int count = 0;
				foreach (KeyValuePair<string, LocalizationManager.LocData> kvp in parsed)
				{
					sData[kvp.Key] = kvp.Value;
					count++;
				}

				Log.Info($"[ProgramableNetwork] Loaded {count} translations for '{lang.CultureInfoId}' from '{filePath}'.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "[ProgramableNetwork] Failed to load mod translations.");
			}

			return true;
		}

		/// <summary>
		/// Writes an English translation template to <c>&lt;manifest.RootDirectoryPath&gt;/Translations/en.json</c>
		/// containing every key registered by this mod (filtered by <see cref="MOD_KEY_PREFIXES"/>) with
		/// its source en-US string. Translators copy this file, rename it to the target language
		/// (e.g. <c>cs.json</c>) and replace the right-hand strings.
		///
		/// Call from <c>RegisterPrototypes</c> AFTER all <c>RegisterData&lt;...&gt;</c> calls so every
		/// proto's name/desc has been registered into <c>s_enUsData</c>.
		/// </summary>
		public static void ExportEnglish(ModManifest manifest)
		{
			try
			{
				if (manifest == null || string.IsNullOrEmpty(manifest.RootDirectoryPath))
				{
					Log.Error("[ProgramableNetwork] ExportEnglish: manifest unavailable.");
					return;
				}

				// Defensive re-scan: ensures every static LocStr* field declared in this mod's assembly
				// (NewTr, NewTr.Tools, NewTr.Inspector, NewTr.FieldStatus, etc.) has had its cctor
				// triggered so its key is in s_enUsData. COI normally does this when loading the mod
				// DLL, but re-running is cheap (GetValue on an already-initialized field is a no-op)
				// and guards against any field that didn't get scanned for whatever reason. Phantom
				// strings registered via static methods (e.g. ControllerProto.RegisterPhantom,
				// DisplayEntityProto.RegisterPhantom) are picked up because RegisterPrototypes calls
				// those methods before the export console command can run.
				LocalizationManager.ScanForStaticLocStrFields(typeof(ModTranslations).Assembly);

				FieldInfo enUsField = typeof(LocalizationManager).GetField("s_enUsData", BindingFlags.NonPublic | BindingFlags.Static);
				if (enUsField == null)
				{
					Log.Error("[ProgramableNetwork] ExportEnglish: LocalizationManager.s_enUsData field not found via reflection.");
					return;
				}

				// Match COI's own ExportJsonTranslationsFile filtering: skip keys explicitly opted out
				// (s_skipForExport) and skip TODO/HIDE markers used by upstream code as placeholders.
				// Mafi.Collections.Set<string> doesn't implement non-generic ICollection, so we copy
				// the keys into a plain HashSet for lookup.
				FieldInfo skipExportField = typeof(LocalizationManager).GetField("s_skipForExport", BindingFlags.NonPublic | BindingFlags.Static);
				HashSet<string> skipExport = new HashSet<string>(StringComparer.Ordinal);
				if (skipExportField?.GetValue(null) is IEnumerable skipEnum)
				{
					foreach (object item in skipEnum)
					{
						if (item is string s) skipExport.Add(s);
					}
				}

				// s_enUsData is Dict<string, LocDataEnUs>; LocDataEnUs is a private nested struct, so we
				// iterate as a non-generic IEnumerable and reflect on KeyValuePair<,>.Key/Value plus the
				// struct's EnUs/Plural fields.
				IEnumerable enUsEnumerable = enUsField.GetValue(null) as IEnumerable;
				if (enUsEnumerable == null)
				{
					Log.Error("[ProgramableNetwork] ExportEnglish: s_enUsData is null or not enumerable.");
					return;
				}

				FieldInfo enUsValueField = null;
				FieldInfo pluralField = null;
				PropertyInfo kvpKeyProp = null;
				PropertyInfo kvpValueProp = null;
				PropertyInfo pluralHasValueProp = null;
				PropertyInfo pluralValueProp = null;

				List<KeyValuePair<string, string[]>> entries = new List<KeyValuePair<string, string[]>>();
				foreach (object kvp in enUsEnumerable)
				{
					if (kvpKeyProp == null)
					{
						Type kvpType = kvp.GetType();
						kvpKeyProp = kvpType.GetProperty("Key");
						kvpValueProp = kvpType.GetProperty("Value");
					}

					string key = kvpKeyProp.GetValue(kvp) as string;
					if (string.IsNullOrEmpty(key)) continue;
					if (!HasModPrefix(key)) continue;
					if (skipExport.Contains(key)) continue;

					object locData = kvpValueProp.GetValue(kvp);
					if (locData == null) continue;

					if (enUsValueField == null)
					{
						Type locDataType = locData.GetType();
						enUsValueField = locDataType.GetField("EnUs", BindingFlags.Public | BindingFlags.Instance);
						pluralField = locDataType.GetField("Plural", BindingFlags.Public | BindingFlags.Instance);
						if (enUsValueField == null)
						{
							Log.Error("[ProgramableNetwork] ExportEnglish: could not resolve LocDataEnUs.EnUs field.");
							return;
						}
					}

					string enUs = enUsValueField.GetValue(locData) as string ?? "";
					if (string.IsNullOrEmpty(enUs)) continue;
					if (enUs.IndexOf("TODO", StringComparison.Ordinal) >= 0) continue;
					if (enUs.IndexOf("HIDE", StringComparison.Ordinal) >= 0) continue;

					string plural = null;
					if (pluralField != null)
					{
						object pluralOpt = pluralField.GetValue(locData);
						if (pluralOpt != null)
						{
							if (pluralHasValueProp == null)
							{
								pluralHasValueProp = pluralOpt.GetType().GetProperty("HasValue");
								pluralValueProp = pluralOpt.GetType().GetProperty("Value");
							}
							if (pluralHasValueProp != null && (bool)pluralHasValueProp.GetValue(pluralOpt))
							{
								plural = pluralValueProp?.GetValue(pluralOpt) as string;
							}
						}
					}

					entries.Add(new KeyValuePair<string, string[]>(key,
						plural != null ? new[] { enUs, plural } : new[] { enUs }));
				}

				entries.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));

				StringBuilder sb = new StringBuilder(64 * entries.Count);
				sb.AppendLine("[");
				for (int i = 0; i < entries.Count; i++)
				{
					sb.Append("\t[\"").Append(JsonWriter.JsonEscapeString(entries[i].Key)).Append("\", ");
					string[] values = entries[i].Value;
					for (int j = 0; j < values.Length; j++)
					{
						if (j > 0) sb.Append(", ");
						sb.Append('"').Append(JsonWriter.JsonEscapeString(values[j])).Append('"');
					}
					sb.Append(']');
					if (i < entries.Count - 1) sb.Append(',');
					sb.AppendLine();
				}
				sb.AppendLine("]");

				string outDir = Path.Combine(manifest.RootDirectoryPath, TRANSLATIONS_DIR);
				Directory.CreateDirectory(outDir);
				string outPath = Path.Combine(outDir, ENGLISH_TEMPLATE_FILE);
				File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);

				Log.Info($"[ProgramableNetwork] Exported {entries.Count} English keys to '{outPath}'.");
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "[ProgramableNetwork] Failed to export English translations.");
			}
		}

		private static bool HasModPrefix(string key)
		{
			for (int i = 0; i < MOD_KEY_PREFIXES.Length; i++)
			{
				if (key.StartsWith(MOD_KEY_PREFIXES[i], StringComparison.Ordinal)) return true;
			}
			return false;
		}
	}
}
