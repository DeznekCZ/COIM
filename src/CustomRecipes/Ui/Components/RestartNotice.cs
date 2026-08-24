using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components;

/// <summary>
/// Small inline note telling the modder that a structural change they just
/// made won't show up in the running game until it is restarted.
///
/// Applies to anything the game reads once during proto registration —
/// ports added to a machine, cloned entities, new toolbar categories, and
/// similar. Those defs are emitted to the pack source immediately, but the
/// live session already built its proto DB and its build-menu lists from the
/// pre-edit state, so the new entry is simply absent from every in-game list
/// until the protos are registered again on the next launch.
///
/// Deliberately a plain tiny-font line rather than a <see cref="LegacyBanner"/>-
/// style colored bar: this is a persistent, expected condition shown next to
/// several controls, not a one-off problem the modder has to act on. A full
/// banner repeated in five editors would drown out the actual warnings.
/// </summary>
public sealed class RestartNotice : Row {

    /// Wording shared by the UI notice and the modding docs, so the two
    /// can't drift apart.
    public const string DefaultText =
        "Takes effect after a game restart — newly added ports, cloned " +
        "entities and other structural changes are registered at load time, " +
        "so they won't appear in the in-game lists this session.";

    public RestartNotice(string text = DefaultText) {
        this.AlignItemsCenter();
        this.Gap(4.px());

        Add(new Label(new LocStrFormatted("ⓘ"))
            .TinyFontSize()
            .Color(ColorRgba.Orange));
        Add(new Label(new LocStrFormatted(text))
            .TinyFontSize()
            .Color(ColorRgba.Orange)
            .FlexGrow(1f));
    }
}
