using System;
using System.Collections.Generic;
using System.Reflection;
using Mafi;

namespace CustomAssets.Ui.Components {

    /// <summary>
    /// Resolves dotted typed-reference paths like
    /// <c>"Ids.Machines.AssemblyElectrified"</c> or
    /// <c>"Ids.Products.Electricity"</c> to the underlying proto id string
    /// (e.g. <c>"AssemblyElectrified"</c>, <c>"Product_Virtual_Electricity"</c>).
    ///
    /// Modders write recipe arguments either as string literals
    /// (<c>"Product_LeadAcidBatteryEmpty"</c>) or as typed references
    /// (<c>Ids.Products.IronOre</c>). The PackLoader captures the latter as
    /// a <see cref="PythonAPI.Expressions.PropertyExpression.Path"/> — a
    /// dotted string. The path differs from the proto's actual id when the
    /// id has a prefix (e.g. <c>"Product_Virtual_Electricity"</c> sits at
    /// <c>Ids.Products.Electricity</c>). To make the picker show the right
    /// proto when a recipe is loaded, we reflect through Mafi.Base.Ids once
    /// and build a path → real-id map.
    ///
    /// The map is lazily initialised on first lookup and cached for the
    /// lifetime of the process. Cost: a single recursive reflection walk
    /// over a few hundred static fields under <c>Mafi.Base.Ids</c>.
    /// </summary>
    public static class TypedRefResolver {

        private static Dictionary<string, string> s_pathToId;
        private static readonly object s_initLock = new object();

        /// <summary>Returns the underlying id string for a dotted typed-ref
        /// path, or null if the path isn't a known Ids entry. Returns null
        /// for non-dotted inputs without performing reflection.</summary>
        public static string ResolveOrNull(string dottedPath) {
            if (string.IsNullOrEmpty(dottedPath)) return null;
            if (dottedPath.IndexOf('.') < 0) return null;
            ensureInit();
            return s_pathToId.TryGetValue(dottedPath, out string id) ? id : null;
        }

        /// <summary>Reverse lookup: find a dotted typed-ref path that resolves
        /// to <paramref name="id"/>. Optional <paramref name="requiredPrefix"/>
        /// (e.g. <c>"Ids.Recipes"</c>) narrows the search so a recipe picker
        /// never accidentally hands back a machine path that happens to share
        /// the same underlying string. Returns null if nothing matches.</summary>
        public static string TypedRefFor(string id, string requiredPrefix = null) {
            if (string.IsNullOrEmpty(id)) return null;
            ensureInit();
            foreach (var kvp in s_pathToId) {
                if (kvp.Value != id) continue;
                if (requiredPrefix != null
                        && !kvp.Key.StartsWith(requiredPrefix + ".", StringComparison.Ordinal)) {
                    continue;
                }
                return kvp.Key;
            }
            return null;
        }

        private static void ensureInit() {
            if (s_pathToId != null) return;
            lock (s_initLock) {
                if (s_pathToId != null) return;
                var sink = new Dictionary<string, string>(StringComparer.Ordinal);
                try {
                    // Mafi.Base.Ids is a static container type with nested
                    // static classes (Machines, Products, Research, …). Each
                    // leaf is a public static readonly field of a Proto.ID
                    // subtype (MachineProto.ID, ProductProto.ID, …).
                    //
                    // Resolving by Type.GetType with an assembly-qualified
                    // name is brittle — the Mafi.Base assembly may have a
                    // version/public-key suffix that we don't know up front.
                    // Iterate loaded assemblies and ask each by simple name
                    // until one resolves "Mafi.Base.Ids".
                    Type rootType = findTypeAcrossAssemblies("Mafi.Base.Ids");
                    if (rootType != null) {
                        walkType(rootType, "Ids", sink);
                        Log.Info("TypedRefResolver: indexed "
                                 + sink.Count + " Mafi.Base.Ids entries");
                    } else {
                        Log.Warning("TypedRefResolver: Mafi.Base.Ids not found in loaded assemblies — typed-ref resolution disabled");
                    }
                } catch (Exception ex) {
                    // Reflection failure shouldn't bring down the picker —
                    // it just means typed-ref resolution won't work for the
                    // session. Log once and proceed with an empty cache.
                    Log.Warning("TypedRefResolver: reflection walk failed — " + ex.Message);
                }
                s_pathToId = sink;
            }
        }

        private static Type findTypeAcrossAssemblies(string fullName) {
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies()) {
                Type t;
                try { t = asm.GetType(fullName, throwOnError: false); }
                catch { continue; }
                if (t != null) return t;
            }
            return null;
        }

        // Recursive walk: read every public static field of `type` and try to
        // pull a `.Value` of type string from it (which is what Proto.ID
        // exposes). Recurse into nested public types. Skips anything that
        // doesn't look like a Proto.ID — non-id static fields (constants,
        // helpers) are silently ignored.
        private static void walkType(Type type, string prefix,
                                     Dictionary<string, string> sink) {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Static;
            foreach (FieldInfo f in type.GetFields(flags)) {
                object value;
                try { value = f.GetValue(null); }
                catch { continue; }
                if (value == null) continue;
                string idValue = tryReadIdValue(value);
                if (idValue != null) sink[prefix + "." + f.Name] = idValue;
            }
            foreach (PropertyInfo p in type.GetProperties(flags)) {
                if (p.GetIndexParameters().Length > 0) continue;
                object value;
                try { value = p.GetValue(null); }
                catch { continue; }
                if (value == null) continue;
                string idValue = tryReadIdValue(value);
                if (idValue != null) sink[prefix + "." + p.Name] = idValue;
            }
            // Recurse into nested types (Ids.Machines, Ids.Products, …).
            foreach (Type nested in type.GetNestedTypes(BindingFlags.Public)) {
                walkType(nested, prefix + "." + nested.Name, sink);
            }
        }

        // Proto.ID exposes a public string `Value` member. It's actually a
        // FIELD (not a property — `public readonly string Value;` on a
        // `readonly struct`), so we have to probe both forms or we miss
        // every entry. Each proto kind has its own nested ID struct
        // (MachineProto.ID, ProductProto.ID, …) but the Value member
        // follows the same shape across them.
        private static string tryReadIdValue(object candidate) {
            Type t = candidate.GetType();

            FieldInfo valueField = t.GetField("Value");
            if (valueField != null && valueField.FieldType == typeof(string)) {
                try { return valueField.GetValue(candidate) as string; }
                catch { return null; }
            }

            PropertyInfo valueProp = t.GetProperty("Value");
            if (valueProp != null && valueProp.PropertyType == typeof(string)) {
                try { return valueProp.GetValue(candidate) as string; }
                catch { return null; }
            }

            return null;
        }
    }
}
