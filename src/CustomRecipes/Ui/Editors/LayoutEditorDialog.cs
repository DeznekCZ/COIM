using System;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// Standalone movable <see cref="Window"/> hosting the visual layout
    /// editor — the footprint / box-type / mesh panel PLUS the existing port
    /// editor for kinds that carry ports. A real window (not a floating popup)
    /// so the modder can drag it beside the main editor while wiring up a
    /// layout, matching <see cref="Components.NewPackDialog"/> /
    /// <see cref="Components.TranslationsPanel"/>.
    ///
    /// Port editing reuses the same <see cref="PortListEditor"/> the inline
    /// machine / reactor forms use — the dialog doesn't introduce a second
    /// port-editing surface, it just brings the existing one into the same
    /// place as the footprint.
    public sealed class LayoutEditorDialog : Window {

        public LayoutEditorDialog(ILayoutHostDef host, PackModel model,
                ProtosDb protosDb, Action onChanged)
            : base(new LocStrFormatted("Layout editor")) {
            MakeMovable();
            WindowSize(560.px(), 680.px());

            ScrollColumn body = new ScrollColumn();
            body.AlignItemsStretch().Gap(6.px());

            body.Add(new LayoutEditorPanel(model, protosDb, host, onChanged));

            // Reuse the existing port editor for kinds that have ports
            // (machines, labs, reactors). Settlement modules return null and
            // simply get no port section.
            if (host.Ports != null) {
                body.Add(new Label(new LocStrFormatted("ports")).Class(Cls.groupHeader));
                body.Add(new PortListEditor(host.Ports, onChanged));
            }

            Body.Add(body);
        }
    }
}
