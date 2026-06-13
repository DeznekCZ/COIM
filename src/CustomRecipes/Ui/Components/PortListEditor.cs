using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// Editable list of <see cref="PortRef"/> rows — shared by every editor
    /// that exposes a port-list argument (<c>edit_machine_ports</c>'s
    /// <c>add_ports</c> and <c>build_machine</c>'s <c>ports</c>). The
    /// captured <paramref name="list"/> is the def's actual
    /// <c>List&lt;PortRef&gt;</c>, so add/remove/edit mutations apply
    /// directly to the model — no separate sync step needed before save.
    ///
    /// Each row carries: name (1 char), type (segmented Input / Output / Any
    /// with selected-state highlight), shape (text + quick-pick chips),
    /// position (raw expression text), direction (segmented +X/-X/+Y/-Y
    /// with selected-state highlight), canOnlyConnectToTransports (Toggle),
    /// and a remove button.
    ///
    /// Exposed as a Column subclass with a public <see cref="Refresh"/> so
    /// callers can rebuild after an out-of-band mutation (e.g. the
    /// edit_machine_ports editor adds a PortRef via a layout-grid click and
    /// then needs the list to re-render the new row). Earlier revisions
    /// used a static factory with a captured rebuild closure; that turned
    /// out to be hard to invoke reliably from outside the closure, hence
    /// the class form.
    public sealed class PortListEditor : Column {

        // Common IoPortShape proto ids registered by the base game. Surfaced
        // as quick-pick buttons next to the shape field so the modder
        // doesn't have to remember the exact spelling.
        private static readonly (string Label, string Id)[] CommonShapes = new[] {
            ("Pipe",            "IoPortShape_Pipe"),
            ("Flat conveyor",   "IoPortShape_FlatConveyor"),
            ("Loose conveyor",  "IoPortShape_LooseMaterialConveyor"),
            ("Molten channel",  "IoPortShape_MoltenMetalChannel"),
        };

        private static readonly string[] Directions = new[] { "+X", "-X", "+Y", "-Y" };

        // The three IoPortType values Mafi exposes — Input, Output, Any.
        // Stored as the canonical lowercase strings the Python runtime
        // accepts (parsePortType in CustomAssetRegistrator.cs is case-
        // insensitive but we keep the form output stable). "any" maps to
        // IoPortType.Any — a bidirectional port that accepts both input
        // and output traffic.
        private static readonly (string Label, string Value)[] PortTypes = new[] {
            ("Input",  "input"),
            ("Output", "output"),
            ("Any",    "any"),
        };

        private readonly List<PortRef> m_list;
        private readonly Action m_onChanged;

        public PortListEditor(List<PortRef> list, Action onChanged) {
            m_list = list ?? new List<PortRef>();
            m_onChanged = onChanged;
            this.AlignItemsStretch();
            this.Gap(2.pt());
            Refresh();
        }

        /// Wipe the current row UI and rebuild from the bound list. Call
        /// after any external mutation (e.g. layout-grid click in the
        /// parent editor) to bring the row count back in sync.
        public void Refresh() {
            Clear();
            for (int i = 0; i < m_list.Count; i++) {
                PortRef p = m_list[i];
                int captured = i;
                Add(buildRow(p, onRemove: () => {
                    m_list.RemoveAt(captured);
                    m_onChanged?.Invoke();
                    Refresh();
                }, onFieldEdit: m_onChanged));
            }
            // Footer: + add port. Seeds a new PortRef with safe defaults
            // (input on a flat conveyor at origin, facing +X). The modder
            // tunes them in the row form. Direction defaults to "+X"
            // because most building layouts orient ports rightward;
            // shape defaults to FlatConveyor since unit/loose ports are
            // the most common modded surface.
            Add(new ButtonText(
                new LocStrFormatted("+ add port"),
                () => {
                    m_list.Add(new PortRef {
                        Name               = "p",
                        Type               = "input",
                        Shape              = "IoPortShape_FlatConveyor",
                        PositionExpression = "(0, 0, 0)",
                        Direction          = "+X",
                    });
                    m_onChanged?.Invoke();
                    Refresh();
                }));
        }

        // One port-row form: name / type / shape / position / direction /
        // canOnlyConnectToTransports / remove. Each field is bound straight
        // to the PortRef so edits propagate without a separate commit step.
        // <paramref name="onFieldEdit"/> fires after every value change so
        // the parent editor can re-render the layout-grid overlay — without
        // it, edits to name / position / direction / etc. would silently
        // diverge from the visual preview.
        private static UiComponent buildRow(PortRef p, Action onRemove, Action onFieldEdit) {
            Column row = new Column();
            row.Gap(2.pt()).Padding(4.px()).AlignItemsStretch()
               .Border(1.px(), ColorRgba.DarkGray, 2);

            // Top line: name + remove button. Type moved to its own line
            // so the three-option segmented control has room to render at
            // a comfortable width.
            Row topLine = new Row();
            topLine.Gap(4.pt()).AlignItemsCenter();

            topLine.Add(new Label(new LocStrFormatted("name")).TinyFontSize());
            TextField nameField = new TextField()
                .Class(Cls.fontMonospace);
            nameField.Text(p.Name ?? "");
            nameField.OnValueChanged(v => {
                // Port name is a single character — keep only the first.
                p.Name = string.IsNullOrEmpty(v) ? "" : v.Substring(0, 1);
                onFieldEdit?.Invoke();
            });
            nameField.Width(40.px());
            topLine.Add(nameField);

            topLine.Add(new UiComponent().FlexGrow(1f));

            topLine.Add(new ButtonText(new LocStrFormatted("✕"), onRemove)
                .Tooltip(new LocStrFormatted("Remove this port")));

            row.Add(topLine);

            // Type — segmented Input / Output / Any. The currently-active
            // option is marked with Cls.selected so the chosen value is
            // visually distinct without an extra readout label. Click any
            // button to switch.
            row.Add(buildSegmentedField(
                label: "type",
                options: PortTypes,
                getValue: () => p.Type ?? "input",
                setValue: v => { p.Type = v; onFieldEdit?.Invoke(); }));

            // Shape line: raw text field + quick-pick buttons.
            Row shapeLine = new Row();
            shapeLine.Gap(4.pt()).AlignItemsCenter();
            shapeLine.Add(new Label(new LocStrFormatted("shape")).TinyFontSize());
            TextField shapeField = new TextField().Class(Cls.fontMonospace);
            shapeField.Text(p.Shape ?? "");
            shapeField.OnValueChanged(v => { p.Shape = v; onFieldEdit?.Invoke(); });
            shapeField.FlexGrow(1f);
            shapeLine.Add(shapeField);
            row.Add(shapeLine);

            // Shape chips — also reflect the selected state so the modder
            // can see at a glance whether the current shape matches a
            // built-in. Unknown / custom shapes leave every chip unselected.
            Row quickPickRow = new Row();
            quickPickRow.Gap(2.pt()).AlignItemsCenter();
            List<ButtonText> shapeChips = new List<ButtonText>();
            foreach (var (label, id) in CommonShapes) {
                string capturedId = id;
                ButtonText chip = null;
                chip = new ButtonText(
                    new LocStrFormatted(label),
                    () => {
                        p.Shape = capturedId;
                        shapeField.Text(capturedId);
                        // Re-apply selected state across the chip group.
                        foreach (var c in shapeChips) {
                            c.ClassIff(Cls.selected, ReferenceEquals(c, chip));
                        }
                        onFieldEdit?.Invoke();
                    });
                chip.ClassIff(Cls.selected, p.Shape == id);
                shapeChips.Add(chip);
                quickPickRow.Add(chip);
            }
            row.Add(quickPickRow);

            // Position — three signed-int fields (X, Y, Z) bound directly
            // to PortRef.PositionX/Y/Z. Editing any one updates the model
            // immediately and fires the preview-refresh callback. The
            // adjacent raw text field is the escape hatch for non-tuple
            // shapes (typed-refs like Vector3i(...) or Ids.X) — when it's
            // non-empty the EMITTER writes that verbatim and ignores the
            // number fields, so a modder can paste in a custom expression
            // without losing it.
            //
            // The loader populates X/Y/Z directly from the AST when the
            // source is a plain tuple, so no string parsing happens here
            // for the common case; the raw field only carries content when
            // the source had something else.
            Row xyzLine = new Row();
            xyzLine.Gap(4.pt()).AlignItemsCenter();
            xyzLine.Add(new Label(new LocStrFormatted("position")).TinyFontSize());

            TextField xField = new TextField().AllIntegersOnly().Width(60.px());
            TextField yField = new TextField().AllIntegersOnly().Width(60.px());
            TextField zField = new TextField().AllIntegersOnly().Width(60.px());
            xField.Text(p.PositionX.ToString(System.Globalization.CultureInfo.InvariantCulture));
            yField.Text(p.PositionY.ToString(System.Globalization.CultureInfo.InvariantCulture));
            zField.Text(p.PositionZ.ToString(System.Globalization.CultureInfo.InvariantCulture));

            // Editing any of the three int fields clears PositionExpression
            // so the numeric values actually take effect on emit. Without
            // this, a port loaded from a non-tuple source (Vector3i(...) /
            // Ids.X.Y) would silently keep its original expression on save
            // and the modder's X/Y/Z edits would never reach the file.
            xField.OnValueChanged(v => {
                if (int.TryParse(v,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int n)) {
                    p.PositionX = n;
                    p.PositionExpression = null;
                }
                onFieldEdit?.Invoke();
            });
            yField.OnValueChanged(v => {
                if (int.TryParse(v,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int n)) {
                    p.PositionY = n;
                    p.PositionExpression = null;
                }
                onFieldEdit?.Invoke();
            });
            zField.OnValueChanged(v => {
                if (int.TryParse(v,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int n)) {
                    p.PositionZ = n;
                    p.PositionExpression = null;
                }
                onFieldEdit?.Invoke();
            });

            xyzLine.Add(new Label(new LocStrFormatted("x")).TinyFontSize());
            xyzLine.Add(xField);
            xyzLine.Add(new Label(new LocStrFormatted("y")).TinyFontSize());
            xyzLine.Add(yField);
            xyzLine.Add(new Label(new LocStrFormatted("z")).TinyFontSize());
            xyzLine.Add(zField);
            row.Add(xyzLine);

            // Direction — segmented +X / -X / +Y / -Y with selected state.
            // Same shape as the type buttons above.
            row.Add(buildSegmentedField(
                label: "direction",
                options: Directions.ConvertToPairs(),
                getValue: () => p.Direction ?? "+X",
                setValue: v => { p.Direction = v; onFieldEdit?.Invoke(); }));

            // canOnlyConnectToTransports: bool toggle. The vanilla Mafi
            // Toggle component renders as a small switch with a label.
            // Standalone:true draws its own outline so it reads as a
            // self-contained row rather than blending into the surrounding
            // form fields.
            Toggle canOnlyToggle = new Toggle(standalone: true);
            ((IComponentWithLabel)canOnlyToggle).SetLabel(
                new LocStrFormatted("canOnlyConnectToTransports"));
            canOnlyToggle.Value(p.CanOnlyConnectToTransports);
            canOnlyToggle.OnValueChanged(v => {
                p.CanOnlyConnectToTransports = v;
                onFieldEdit?.Invoke();
            });
            row.Add(canOnlyToggle);

            return row;
        }

        // Render a row of buttons that act as a single-select enum picker.
        // The button whose value matches getValue() is marked with
        // Cls.selected; clicking any button writes the new value and
        // re-applies the selected style across the group.
        private static UiComponent buildSegmentedField(
                string label,
                (string Label, string Value)[] options,
                Func<string> getValue,
                Action<string> setValue) {
            Row line = new Row();
            line.Gap(4.pt()).AlignItemsCenter();
            line.Add(new Label(new LocStrFormatted(label)).TinyFontSize());
            List<ButtonText> btns = new List<ButtonText>();
            foreach (var (lbl, val) in options) {
                string capturedVal = val;
                ButtonText btn = null;
                btn = new ButtonText(
                    new LocStrFormatted(lbl),
                    () => {
                        setValue(capturedVal);
                        // Re-apply selection across the whole group so the
                        // previously-active button drops Cls.selected.
                        foreach (var b in btns) {
                            b.ClassIff(Cls.selected, ReferenceEquals(b, btn));
                        }
                    });
                btn.ClassIff(Cls.selected, getValue() == val);
                btns.Add(btn);
                line.Add(btn);
            }
            return line;
        }
    }

    internal static class DirectionListExtensions {
        // Helper that lets buildSegmentedField consume the bare string
        // array of direction tokens uniformly with the (label, value)
        // tuple shape used for the type enum. Each direction's label and
        // value are the same string, so the conversion is trivial.
        public static (string Label, string Value)[] ConvertToPairs(this string[] values) {
            var result = new (string, string)[values.Length];
            for (int i = 0; i < values.Length; i++) result[i] = (values[i], values[i]);
            return result;
        }
    }
}
