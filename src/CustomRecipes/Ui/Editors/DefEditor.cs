using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <summary>
    /// Base class for per-kind definition editors. Mirrors the Mafi
    /// "self-contained Column UI built in the constructor" pattern used by
    /// game inspectors: subclass <see cref="DefEditor{T}"/>, instantiate
    /// fields in the constructor via <see cref="AddField"/>, and bind each
    /// field's get/set to the current <see cref="Value"/> reference via the
    /// observer list. When the editor's <see cref="Value"/> instance is
    /// swapped (e.g. selecting a different def in the tree), every observer
    /// re-binds in one pass so the existing UI components are reused rather
    /// than torn down.
    ///
    /// No virtual <c>Init()</c> method is exposed because the constructor
    /// runs that work directly — subclasses call <see cref="AddField"/>
    /// inline. Observer registration is also inline: each binding is added
    /// to <see cref="m_observers"/> in the constructor and replayed when
    /// <see cref="Value"/> changes.
    /// </summary>
    public abstract class DefEditor<TDefBase> : Column where TDefBase : DefBase {

        /// Currently-bound model instance. Subclasses read through this in
        /// every field accessor (`() => value.Name` etc.) so observer
        /// replays after a <see cref="Value"/> swap pick up the new model
        /// automatically. Subclasses must tolerate a null value at
        /// construction time — observers won't fire until the first
        /// <see cref="Value"/> call.
        protected TDefBase value;

        /// Per-field rebinders. Each entry is a closure that re-reads its
        /// field's current display value from the editor's <c>value</c> and
        /// pushes it back into the UI component. Populated implicitly by
        /// <see cref="AddField"/> via <see cref="OnRefresh"/>.
        private readonly List<Action> m_observers = new List<Action>();

        protected DefEditor() {
            this.AlignItemsStretch();
            this.Gap(4.px()).Padding(4.px());
		}

        /// <summary>Swap the currently-edited model instance. Every observer
        /// registered via <see cref="OnRefresh"/> fires once so each field
        /// reflects the new value. Subclasses can override to add extra
        /// behaviour (validation refresh, header label updates, etc.) but
        /// should call <c>base.Value(newValue)</c> first.</summary>
        public virtual DefEditor<TDefBase> Value(TDefBase newValue) {
            this.value = newValue;
            if (newValue == null) return this;
            for (int i = 0; i < m_observers.Count; i++) {
                m_observers[i]();
            }
            return this;
        }

        /// <summary>Register a callback that fires whenever
        /// <see cref="Value"/> swaps. Subclasses typically don't call this
        /// directly — <see cref="AddField"/> registers the rebind for the
        /// field it just added — but it's exposed for cases where multiple
        /// fields need coordinated updates (validation summaries, computed
        /// totals).</summary>
        protected void OnRefresh(Action onRefresh) {
            if (onRefresh != null) m_observers.Add(onRefresh);
        }

        /// <summary>Add a labeled field to the editor body. The label sits
        /// above the field as a TinyFontSize Label; the field stretches to
        /// the editor's full width. Returns the field component so callers
        /// can chain configuration (`.Class(...)`, `.OnValueChanged(...)`,
        /// etc.) at the call site.</summary>
        /// <param name="label">Static label text shown above the field.
        /// LocStrFormatted is passed by value — for dynamic labels, store
        /// the returned Label reference yourself and update it directly.</param>
        /// <param name="fieldComponent">The input component being labeled.
        /// Stretches horizontally inside the row.</param>
        /// <param name="onRefresh">Optional callback that re-reads the
        /// field's current display value from <see cref="value"/> and
        /// pushes it into the component. Fires immediately on the next
        /// <see cref="Value"/> call. Pass null when the field is purely
        /// write-only (e.g. unbound action buttons) or when the subclass
        /// handles rebinding itself.</param>
        protected TComponent AddField<TComponent>(LocStrFormatted label, TComponent fieldComponent,
                Action onRefresh = null) where TComponent : UiComponent {
            Column row = new Column {
                new Label(label).Class(Cls.groupHeader),
                fieldComponent
            }.Class(Cls.group).Gap(4.px()).Padding(4.px());
            row.AlignItemsStretch();
            Add(row);
            if (onRefresh != null) m_observers.Add(onRefresh);
            return fieldComponent;
        }

        /// <summary>Same as <see cref="AddField{TComponent}(LocStrFormatted, TComponent,Action)"/>
        /// but with a plain string label — sugar for the common case where the label text is built
        /// inline from a literal.</summary>
        protected TComponent AddField<TComponent>(string label, TComponent fieldComponent,
                Action onRefresh = null) where TComponent : UiComponent {
            return AddField(new LocStrFormatted(label), fieldComponent, onRefresh);
        }

        // ---- Bound field helpers ---------------------------------------------
        //
        // Concrete editors used to repeat the same get-it / OnValueChanged /
        // onRefresh dance for every text field, just with different property
        // names. The helpers below absorb that boilerplate: each takes a
        // property accessor (getter/setter that read from the editor's
        // current `value`) and registers the wiring + observer in one call.
        // Null-`value` guards live in the helpers so the field doesn't have
        // to worry about it.

        /// <summary>Plain single-line string field. Reads/writes
        /// <paramref name="getter"/> / <paramref name="setter"/> on the
        /// editor's current value. Set <paramref name="multiline"/> to
        /// produce a multi-line text area with the standard 48px minimum
        /// height — match the loose / fluid / unit `description` field
        /// shape.</summary>
        protected TextField AddStringField(string label,
                Func<TDefBase, string> getter, Action<TDefBase, string> setter,
                bool multiline = false, bool monospace = false) {
            TextField field = new TextField();
            if (multiline) field.Multiline(true).SetTextAreaMinHeight(48.px());
            if (monospace) field.Class(Cls.fontMonospace);
            field.OnValueChanged(v => { if (value != null) setter(value, v); });
            return AddField(label, field, onRefresh: () => field.Text(getter(value) ?? ""));
        }

        /// <summary>Plain single-line string field where blank input maps
        /// back to null. Saves the repeated `setter = string.IsNullOrEmpty
        /// (v) ? null : v` pattern.</summary>
        protected TextField AddOptionalStringField(string label,
                Func<TDefBase, string> getter, Action<TDefBase, string> setter,
                bool multiline = false, bool monospace = false) {
            return AddStringField(label, getter,
                (v, s) => setter(v, string.IsNullOrEmpty(s) ? null : s),
                multiline, monospace);
        }

        /// <summary>Positive-integer field bound to a nullable-int property.
        /// Blank input clears the value; non-integer input is ignored.</summary>
        protected TextField AddNullableIntField(string label,
                Func<TDefBase, int?> getter, Action<TDefBase, int?> setter) {
            TextField field = new TextField().PositiveIntegersOnly();
            field.OnValueChanged(v => {
                if (value == null) return;
                if (string.IsNullOrWhiteSpace(v)) setter(value, null);
                else if (int.TryParse(v.Trim(), out int parsed)) setter(value, parsed);
            });
            return AddField(label, field, onRefresh: () => {
                int? cur = getter(value);
                field.Text(cur.HasValue ? cur.Value.ToString() : "");
            });
        }

        /// <summary>Double field with invariant-culture parsing. Same
        /// null-on-blank semantics as <see cref="AddNullableIntField"/>.</summary>
        protected TextField AddNullableDoubleField(string label,
                Func<TDefBase, double?> getter, Action<TDefBase, double?> setter) {
            TextField field = new TextField();
            field.OnValueChanged(v => {
                if (value == null) return;
                setter(value, EditorHelpers.ParseNullableDouble(v));
            });
            return AddField(label, field, onRefresh: () => {
                double? cur = getter(value);
                field.Text(cur.HasValue ? EditorHelpers.FormatDouble(cur.Value) : "");
            });
        }

        /// <summary>Raw Python expression field — monospace text, blank
        /// maps to null. Use for fields like `inputProduct`, `color`,
        /// `tiling` etc. that the emitter writes verbatim.</summary>
        protected TextField AddRawExpressionField(string label,
                Func<TDefBase, string> getter, Action<TDefBase, string> setter) {
            return AddOptionalStringField(label, getter, setter,
                multiline: false, monospace: true);
        }

        /// <summary>Multi-line text field bound to <see cref="DefBase.Comment"/>
        /// — the `#`-prefixed comment block preserved above each def. Blank
        /// input maps to null so the emitter omits the comment lines
        /// entirely instead of writing a stray blank `# `.</summary>
        protected TextField AddCommentField(
                string label = "comment (notes shown above the statement)") {
            return AddOptionalStringField(label,
                getter: d => d.Comment,
                setter: (d, v) => d.Comment = v,
                multiline: true);
        }

        /// <summary>Add the layout editor entry for any def kind that
        /// implements <see cref="ILayoutHostDef"/> (machines, settlement
        /// modules, mines, labs, reactors). Renders a compact row — a status
        /// line plus an "Edit layout…" button that opens the visual editor in
        /// a DIALOG — rather than embedding the (large) grid inline. The
        /// layout is only overridden once the modder actually edits inside the
        /// dialog; merely opening it changes nothing. A "Reset to source"
        /// button appears when a custom layout is active so the override can
        /// be cleared. No-op for kinds whose <see cref="DefBase.SupportsLayout"/>
        /// is false.</summary>
        protected void AddLayoutEditor(PackModel packModel, ProtosDb protosDb) {
            Column holder = new Column();
            holder.AlignItemsStretch().Gap(4.px());
            AddField("layout (footprint, ports, mesh)", holder, onRefresh: () => {
                holder.Clear();
                if (value == null || !value.SupportsLayout || !(value is ILayoutHostDef)) return;

                bool custom = value.Layout != null && value.Layout.IsStructured;
                holder.Add(new Label(new LocStrFormatted(custom
                        ? "custom layout (overrides source) — click Edit to adjust"
                        : "inherits source layout — click Edit to customise"))
                    .Color(custom ? new ColorRgba((byte)200, (byte)180, (byte)90, (byte)255) : ColorRgba.LightGray)
                    .TinyFontSize());

                Row buttons = new Row().Gap(6.px());
                buttons.Add(new ButtonText(new LocStrFormatted("Edit layout…"),
                    () => openLayoutDialog(packModel, protosDb)));
                if (custom) {
                    buttons.Add(new ButtonText(new LocStrFormatted("Reset to source"), () => {
                        if (value == null) return;
                        // Drop the structured override: blank the layout string
                        // and the cached model so emit reverts to inheriting the
                        // source (copy_layout) and the dialog re-parses fresh.
                        if (value is ILayoutHostDef h) h.LayoutSourceStr = null;
                        value.Layout = null;
                        value.Dirty = true;
                        Value(value);
                    }));
                }
                holder.Add(buttons);
            });
        }

        // Open the visual layout editor as a standalone movable Window (not a
        // floating popup). The window hosts the footprint / mesh panel AND the
        // existing port editor, so all layout work lives in one place while
        // the inline form stays compact. UiRoot is obtained from this
        // component's attachment rather than threading UiContext through every
        // editor ctor.
        private void openLayoutDialog(PackModel packModel, ProtosDb protosDb) {
            if (!(value is ILayoutHostDef host)) return;
            DefBase owner = value;
            this.RunWhenAttached(root => {
                LayoutEditorDialog dialog = new LayoutEditorDialog(
                    host, packModel, protosDb,
                    onChanged: () => { if (owner != null) owner.Dirty = true; });
                // Refresh the inline status line / Reset button once the dialog
                // closes so it reflects any override made inside it.
                dialog.OnCloseStart += _ => { if (value == owner) Value(value); };
                dialog.Open(root);
            });
        }
    }

    /// <summary>
    /// Intermediate editor base for kinds that inherit <see cref="NamedDef"/>
    /// — i.e. they carry both a primary id and a human-facing display name.
    /// Adds <see cref="AddIdField"/> and <see cref="AddNameField"/> shortcuts
    /// that bind directly to <c>NamedDef.Id</c> / <c>NamedDef.Name</c>.
    ///
    /// Asset-style kinds (textures, materials, prefabs) inherit
    /// <see cref="DefEditor{T}"/> directly because they don't have a Name —
    /// their identifier IS the asset path. Unlock kinds inherit DefEditor
    /// directly too (composite identifier, no display name).
    /// </summary>
    public abstract class NamedDefEditor<TNamedDef> : DefEditor<TNamedDef>
            where TNamedDef : NamedDef {

        /// <summary>Single-line text field bound to <see cref="NamedDef.Id"/>.
        /// Label defaults to "id"; each kind passes its API-canonical name
        /// (researchId / productId / categoryId / …).</summary>
        protected TextField AddIdField(string label = "id") {
            return AddStringField(label, d => d.Id, (d, v) => d.Id = v);
        }

        /// <summary>Single-line text field bound to <see cref="NamedDef.Name"/>.
        /// Most kinds use the default "name" label.</summary>
        protected TextField AddNameField(string label = "name") {
            return AddStringField(label, d => d.Name, (d, v) => d.Name = v);
        }
    }
}
