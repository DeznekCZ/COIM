using Mafi;
using Mafi.Collections;
using Mafi.Core.Mods;
using Mafi.Localization;
using Mafi.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

				// LocStr captures its TranslatedString at construction. Any static LocStr field that
				// was initialized by the mod loader before our splice ran is permanently English.
				// Force-init any not-yet-touched static LocStr fields so they get the (now-current)
				// Czech, then reach into the already-frozen ones and overwrite their TranslatedString
				// from s_data. Both passes together cover the whole assembly.
				LocalizationManager.ScanForStaticLocStrFields(typeof(ModTranslations).Assembly);
				RebindStaticLocStrs(typeof(ModTranslations).Assembly, sData);
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "[ProgramableNetwork] Failed to load mod translations.");
			}

			return true;
		}

		/// <summary>
		/// Walks every static field in <paramref name="asm"/> whose declaring type lives in
		/// <c>Mafi.Localization</c> and looks like a localized-string carrier (has an <c>Id</c>
		/// string field plus at least one other string field). For each such static field reads
		/// its <c>Id</c>, looks up <paramref name="sData"/>[Id], and copies the translation strings
		/// from <c>LocData</c>'s <c>ImmutableArray&lt;string&gt;</c> into the matching slots on the
		/// LocStr instance — index 0 to the first non-<c>Id</c> string field, index 1 to the second
		/// (covers plural variants), etc.
		///
		/// Boxed-write-back pattern works for both struct- and class-based LocStr types.
		/// </summary>
		private static void RebindStaticLocStrs(Assembly asm, Dict<string, LocalizationManager.LocData> sData)
		{
			// Find the ImmutableArray<string> field on LocData — that's where the parsed translations
			// live (one entry per JSON value: index 0 = singular translation, index 1 = plural, etc.).
			Type locDataType = typeof(LocalizationManager.LocData);
			FieldInfo locDataArrayField = null;
			foreach (FieldInfo f in locDataType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
			{
				Type ft = f.FieldType;
				if (!ft.IsGenericType) continue;
				if (!ft.Name.StartsWith("ImmutableArray", StringComparison.Ordinal)) continue;
				Type[] args = ft.GetGenericArguments();
				if (args.Length != 1 || args[0] != typeof(string)) continue;
				locDataArrayField = f;
				break;
			}
			if (locDataArrayField == null)
			{
				Log.Warning("[ProgramableNetwork] RebindStaticLocStrs: could not find ImmutableArray<string> field on LocData.");
				return;
			}

			// Reflect Length + indexer once (ImmutableArray<T> exposes both publicly).
			Type immutableArrayType = locDataArrayField.FieldType;
			PropertyInfo lengthProp = immutableArrayType.GetProperty("Length", BindingFlags.Public | BindingFlags.Instance);
			PropertyInfo itemProp = null;
			foreach (PropertyInfo p in immutableArrayType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
			{
				ParameterInfo[] indexers = p.GetIndexParameters();
				if (indexers.Length == 1 && indexers[0].ParameterType == typeof(int) && p.PropertyType == typeof(string))
				{
					itemProp = p;
					break;
				}
			}
			if (lengthProp == null || itemProp == null)
			{
				Log.Warning("[ProgramableNetwork] RebindStaticLocStrs: ImmutableArray<string> shape lacks expected Length/indexer.");
				return;
			}

			// Discover LocStr-shaped types: any Mafi.Localization type with an Id string field plus
			// one or more other string fields (those are the translation slots we'll overwrite).
			Assembly mafiAsm = typeof(LocStr).Assembly;
			Dictionary<Type, LocStrShape> shapesByType = new Dictionary<Type, LocStrShape>();
			foreach (Type t in mafiAsm.GetTypes())
			{
				if (t.Namespace != "Mafi.Localization") continue;
				FieldInfo[] stringFields = t
					.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)
					.Where(f => f.FieldType == typeof(string))
					.ToArray();
				FieldInfo idField = stringFields.FirstOrDefault(f => f.Name == "Id");
				if (idField == null) continue;
				FieldInfo[] translationSlots = stringFields.Where(f => f != idField).ToArray();
				if (translationSlots.Length == 0) continue;
				shapesByType[t] = new LocStrShape(idField, translationSlots);
			}

			int rebound = 0;
			int missing = 0;
			object[] indexBuf = new object[1];
			foreach (Type type in asm.GetTypes())
			{
				FieldInfo[] fields;
				try
				{
					fields = type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
				}
				catch
				{
					continue;
				}

				foreach (FieldInfo field in fields)
				{
					if (!shapesByType.TryGetValue(field.FieldType, out LocStrShape shape)) continue;

					object boxed;
					try { boxed = field.GetValue(null); }
					catch { continue; }
					if (boxed == null) continue;

					string id = shape.IdField.GetValue(boxed) as string;
					if (string.IsNullOrEmpty(id)) continue;

					if (!sData.TryGetValue(id, out LocalizationManager.LocData data))
					{
						missing++;
						continue;
					}

					object array = locDataArrayField.GetValue(data);
					if (array == null) continue;
					int length = (int)lengthProp.GetValue(array);
					if (length == 0) continue;

					try
					{
						bool wrote = false;
						int slotsToFill = Math.Min(length, shape.Slots.Length);
						for (int i = 0; i < slotsToFill; i++)
						{
							indexBuf[0] = i;
							string translation = itemProp.GetValue(array, indexBuf) as string;
							if (string.IsNullOrEmpty(translation)) continue;
							shape.Slots[i].SetValue(boxed, translation);
							wrote = true;
						}
						if (wrote)
						{
							field.SetValue(null, boxed); // write back — required for struct LocStr, harmless for class
							rebound++;
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex, $"[ProgramableNetwork] Rebind failed for {type.FullName}.{field.Name} (id={id}).");
					}
				}
			}
			Log.Info($"[ProgramableNetwork] Rebound {rebound} static LocStr fields ({missing} keys not in s_data).");
		}

		private readonly struct LocStrShape
		{
			public readonly FieldInfo IdField;
			public readonly FieldInfo[] Slots;
			public LocStrShape(FieldInfo id, FieldInfo[] slots)
			{
				IdField = id;
				Slots = slots;
			}
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
