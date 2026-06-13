using System;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Data.Translate {

    /// Deferred text application for UI components.
    ///
    /// Background: <see cref="LocStr"/> snapshots its translated string at
    /// construction. A <c>static readonly LocStr</c> field declared in a UI
    /// class is therefore frozen English until something forces its cctor
    /// to run AFTER translations have been spliced. If a UI element is
    /// built (e.g. <c>new Label(NewTr.MyText)</c>) before the splice, the
    /// element captures the wrong text and will never refresh on its own.
    ///
    /// <see cref="ModTranslations.LoadForPack"/> handles dynamically-
    /// registered prototype names (recipe/research/product), which are
    /// fine because the splice happens before .py execution. But if the
    /// mod's own UI ever declares static LocStr fields and uses them in
    /// components built during static init, those components need the
    /// safety net of re-evaluating their text after the splice has run.
    ///
    /// Usage:
    /// <code>
    ///   new Label()
    ///       .LaterText(() => MyDialog.Title, this);   // host = the window
    /// </code>
    /// The lambda fires once at registration (initial paint, may be wrong
    /// language) AND once on the host's first <c>OnShow</c> (by which
    /// time translations have been spliced). After that the registration
    /// is dropped — there's no ongoing refresh, since LocStr values are
    /// immutable once correct.
    ///
    /// This file is infrastructure-only. Nothing in the editor currently
    /// uses it; the editor today builds Labels with English
    /// <see cref="LocStrFormatted"/> literals. When the editor's text is
    /// migrated to per-class LocStrs, wrap each component construction with
    /// <c>.LaterText(() => MyClass.MyField, this)</c>.
    public static class LaterTextExtensions {

        /// Chainable LaterText for any <see cref="IComponentWithText"/>
        /// (Label, ButtonText, ButtonIconText, …) backed by a UiComponent
        /// host. The text getter is invoked immediately, then re-invoked
        /// once when <paramref name="host"/> first becomes visible.
        public static T LaterText<T>(
            this T component, Func<LocStrFormatted> textGetter, UiComponent host
        ) where T : IComponentWithText {
            if (textGetter == null || host == null) return component;
            component.SetValue(textGetter());
            host.OnShow(() => {
                try { component.SetValue(textGetter()); }
                catch (Exception ex) {
                    Mafi.Log.Warning("LaterText: setter threw: " + ex.Message);
                }
            });
            return component;
        }

        /// LocStr-returning overload — convenient when the source is a
        /// raw LocStr rather than a LocStrFormatted. The conversion is
        /// implicit and allocation-free.
        public static T LaterText<T>(
            this T component, Func<LocStr> textGetter, UiComponent host
        ) where T : IComponentWithText {
            if (textGetter == null) return component;
            return component.LaterText(() => (LocStrFormatted)textGetter(), host);
        }

        /// General-purpose hook for non-IComponentWithText targets — e.g.
        /// applying the translated text via <c>Tooltip(...)</c>, or any
        /// other setter that takes a LocStrFormatted. Pass an explicit
        /// setter <c>(target, value) => target.Tooltip(value)</c>.
        public static T LaterText<T>(
            this T target, Func<LocStrFormatted> textGetter, UiComponent host,
            Action<T, LocStrFormatted> setter
        ) {
            if (textGetter == null || setter == null || host == null) return target;
            setter(target, textGetter());
            host.OnShow(() => {
                try { setter(target, textGetter()); }
                catch (Exception ex) {
                    Mafi.Log.Warning("LaterText: setter threw: " + ex.Message);
                }
            });
            return target;
        }
    }
}
