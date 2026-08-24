using System;
using System.Collections.Generic;
using CustomAssets.Data.Mod;
using CustomAssets.Editor.Io;
using Mafi;

namespace CustomAssets.Ui.Components;

/// Ambient "which pack is being edited" state for the expression controls.
///
/// The form builders that host an expression field (RecipeFormParts and friends) are
/// static helpers reached through several call sites, so threading a pack reference
/// down to every numeric field would touch a lot of signatures for one popup. The
/// editor sets this when it switches packs instead — the same shape
/// CustomAssetRegistrator uses for its current-mod state, and safe for the same
/// reason: exactly one pack is being edited at a time.
public static class ExpressionContext
{
    private static string s_packRootPath;
    private static Dictionary<string, object> s_values;

    /// Config field names of the current pack, for the composer's chips.
    public static IEnumerable<string> ConfigFieldNames => Values().Keys;

    /// Called by the editor whenever the selected pack changes (and after a save, so
    /// a freshly edited config.json is picked up).
    public static void SetPack(string packRootPath)
    {
        s_packRootPath = packRootPath;
        s_values = null;
    }

    /// Current pack's config values, as the runtime would see them — expressions in
    /// config.json resolved, numbers coerced to int/float. Empty when no pack is set
    /// or it has no config.json. Cached until the next SetPack.
    public static Dictionary<string, object> Values()
    {
        if (s_values != null)
        {
            return s_values;
        }
        s_values = new Dictionary<string, object>(StringComparer.Ordinal);
        if (string.IsNullOrEmpty(s_packRootPath))
        {
            return s_values;
        }
        try
        {
            ConfigSchema schema = ConfigSchema.Load(s_packRootPath);
            s_values = schema.EvaluateValues(out Dictionary<string, string> _);
        }
        catch (Exception ex)
        {
            Log.Warning("ExpressionContext: cannot read config.json — " + ex.Message);
        }
        return s_values;
    }

    /// Evaluate an expression the modder typed, against this pack's config values.
    /// Returns false with a modder-facing message; a reference to something that is
    /// not a config field (a Python variable defined in the file) is reported as
    /// "cannot preview" rather than as an error, since it is legal at load time.
    public static bool TryPreview(string expression, out object value, out string message)
    {
        value = null;
        message = null;
        if (string.IsNullOrWhiteSpace(expression))
        {
            return false;
        }
        if (ConfigExpressions.TryEvaluate(expression, Values(), out value, out string error))
        {
            return true;
        }
        message = error;
        return false;
    }
}
