using System;
using System.Collections.Generic;
using System.Globalization;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// Editable list of <see cref="EnrichmentStepRef"/> rows — embedded by
    /// <see cref="EnrichmentRefEditor"/> to surface the breeding curve a
    /// reactor advances through. Each row carries the three integer
    /// parameters of the runtime EnrichmentStepData struct:
    ///   - fuelMultiplier (percent — 100 = 1× efficiency)
    ///   - breedingRatio (output units per input unit)
    ///   - steamReductionDiv (divisor on per-step steam output)
    /// Mirrors the FuelPairListEditor pattern (class-based Column with a
    /// public Refresh() so callers can rebuild after a typed swap).
    public sealed class EnrichmentStepListEditor : Column {

        private readonly List<EnrichmentStepRef> m_list;
        private readonly Action m_onChanged;

        public EnrichmentStepListEditor(List<EnrichmentStepRef> list, Action onChanged) {
            m_list = list ?? new List<EnrichmentStepRef>();
            m_onChanged = onChanged;
            this.AlignItemsStretch();
            this.Gap(2.pt());
            Refresh();
        }

        public void Refresh() {
            Clear();
            for (int i = 0; i < m_list.Count; i++) {
                EnrichmentStepRef s = m_list[i];
                int captured = i;
                Add(buildRow(captured, s, onRemove: () => {
                    m_list.RemoveAt(captured);
                    m_onChanged?.Invoke();
                    Refresh();
                }, onFieldEdit: m_onChanged));
            }
            Add(new ButtonText(
                new LocStrFormatted("+ add enrichment step"),
                () => {
                    // Sensible defaults — 100% fuel multiplier means
                    // 1× efficiency; 1 breeding ratio means 1 output
                    // per input; 1 steam divisor means no dampening.
                    m_list.Add(new EnrichmentStepRef {
                        FuelMultiplierPercent = 100,
                        BreedingRatio = 1,
                        SteamReductionDiv = 1,
                    });
                    m_onChanged?.Invoke();
                    Refresh();
                }));
        }

        private UiComponent buildRow(int index, EnrichmentStepRef s, Action onRemove, Action onFieldEdit) {
            Column row = new Column();
            row.Gap(2.pt()).Padding(4.px()).AlignItemsStretch()
               .Border(1.px(), ColorRgba.DarkGray, 2);

            Row header = new Row();
            header.Gap(4.pt()).AlignItemsCenter();
            header.Add(new Label(new LocStrFormatted("step " + index)).TinyFontSize());
            header.Add(new ButtonText(new LocStrFormatted("✕"), onRemove)
                .Tooltip(new LocStrFormatted("Remove this enrichment step")));
            row.Add(header);

            row.Add(buildIntRow("fuelMultiplier (percent, 100 = 1×)",
                () => s.FuelMultiplierPercent,
                v => { s.FuelMultiplierPercent = v; onFieldEdit?.Invoke(); }));
            row.Add(buildIntRow("breedingRatio (output units per input unit)",
                () => s.BreedingRatio,
                v => { s.BreedingRatio = v; onFieldEdit?.Invoke(); }));
            row.Add(buildIntRow("steamReductionDiv (divides per-step steam output)",
                () => s.SteamReductionDiv,
                v => { s.SteamReductionDiv = v; onFieldEdit?.Invoke(); }));

            return row;
        }

        private static UiComponent buildIntRow(string label, Func<int> get, Action<int> set) {
            Row r = new Row();
            r.Gap(4.pt()).AlignItemsCenter();
            r.Add(new Label(new LocStrFormatted(label)).TinyFontSize());
            TextField f = new TextField().AllIntegersOnly().Width(80.px());
            f.Text(get().ToString(CultureInfo.InvariantCulture));
            f.OnValueChanged(v => {
                if (int.TryParse(v, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) {
                    set(n);
                }
            });
            r.Add(f);
            return r;
        }
    }

    /// Full <see cref="EnrichmentRef"/> editor — input/output products and
    /// ports + the rational ProcessedPerLevel + buffer capacity + meltdown
    /// behaviour + default-step index + a nested
    /// <see cref="EnrichmentStepListEditor"/>. Every field stays nullable
    /// so the modder can leave individual entries empty to inherit from
    /// the source reactor's Enrichment.
    public sealed class EnrichmentRefEditor : Column {

        /// Build an <see cref="EnrichmentRef"/> populated from the live
        /// <c>Enrichment</c> data carried by the given reactor proto. Used
        /// by edit_nuclear_reactor_enrichment (and the build-reactor
        /// fill-from-source path) to seed the editor with the target's
        /// current breeding chemistry so the modder edits deltas instead
        /// of typing every field from scratch. Returns <c>null</c> when
        /// the reactor has no Enrichment (most non-breeder reactors).
        ///
        /// Conversion notes:
        ///   * ProcessedPerLevel is a PartialQuantity (Fix32-backed,
        ///     denominator 1024). We expose it back as a reduced num/den
        ///     pair so simple fractions like 1/2 / 1/4 read sensibly
        ///     instead of as 512/1024 / 256/1024.
        ///   * Percent.RawValue divides by 1000 to recover the integer
        ///     percent value (100% → 100).
        ///   * char ports become single-char strings; default(char) → null.
        public static EnrichmentRef CreateFromProto(NuclearReactorProto reactor) {
            if (reactor == null || !reactor.Enrichment.HasValue) return null;
            NuclearReactorProto.EnrichmentData ed = reactor.Enrichment.Value;

            EnrichmentRef result = new EnrichmentRef {
                InputProductId           = ed.InputProduct?.Id.Value,
                OutputProductId          = ed.OutputProduct?.Id.Value,
                InPort                   = ed.InPort  != default(char) ? ed.InPort.ToString()  : null,
                OutPort                  = ed.OutPort != default(char) ? ed.OutPort.ToString() : null,
                BuffersCapacity          = ed.BuffersCapacity.Value,
                DestroyContentOnMeltdown = ed.DestroyContentOnMeltdown,
                DefaultEnrichmentStep    = ed.DefaultEnrichmentStep,
            };

            int rawNum = ed.ProcessedPerLevel.Value.RawValue;
            const int rawDen = 1024;
            int gcd = computeGcd(Math.Abs(rawNum), rawDen);
            if (gcd > 0) {
                result.ProcessedPerLevelNumerator   = rawNum / gcd;
                result.ProcessedPerLevelDenominator = rawDen / gcd;
            } else {
                result.ProcessedPerLevelNumerator   = 0;
                result.ProcessedPerLevelDenominator = 1;
            }

            result.Steps = new List<EnrichmentStepRef>();
            foreach (NuclearReactorProto.EnrichmentStepData step in ed.EnrichmentSteps) {
                result.Steps.Add(new EnrichmentStepRef {
                    FuelMultiplierPercent = step.FuelMultiplier.RawValue / 1000,
                    BreedingRatio         = step.BreedingRatio,
                    SteamReductionDiv     = step.SteamReductionDiv,
                });
            }
            return result;
        }

        private static int computeGcd(int a, int b) {
            while (b != 0) {
                int t = b;
                b = a % b;
                a = t;
            }
            return a;
        }


        private readonly EnrichmentRef m_ref;
        private readonly Action m_onChanged;
        private readonly ProtosDb m_protosDb;

        public EnrichmentRefEditor(EnrichmentRef value, Action onChanged, ProtosDb protosDb) {
            m_ref = value;
            m_onChanged = onChanged;
            m_protosDb = protosDb;
            this.AlignItemsStretch();
            this.Gap(4.pt()).Padding(6.px())
                .Border(1.px(), ColorRgba.DarkGray, 2);
            build();
        }

        private void build() {
            Clear();

            // ---- Breeding products ----
            // Port letters (InPort / OutPort) are intentionally not
            // surfaced — the runtime infers them from the selected
            // products against the source reactor's layout. The fields
            // stay on EnrichmentRef so hand-edited .py files round-trip.
            Add(label("inputProduct (breeding feedstock; blank = inherit)"));
            Add(new ProtoPicker<ProductProto>(
                m_protosDb,
                getId: () => m_ref.InputProductId,
                setId: id => {
                    m_ref.InputProductId = string.IsNullOrEmpty(id) ? null : id;
                    m_onChanged?.Invoke();
                },
                title: new LocStrFormatted("Pick breeding INPUT product"),
                allowNone: true));

            Add(label("outputProduct (breeding result; blank = inherit)"));
            Add(new ProtoPicker<ProductProto>(
                m_protosDb,
                getId: () => m_ref.OutputProductId,
                setId: id => {
                    m_ref.OutputProductId = string.IsNullOrEmpty(id) ? null : id;
                    m_onChanged?.Invoke();
                },
                title: new LocStrFormatted("Pick breeding OUTPUT product"),
                allowNone: true));

            // ---- Rational ProcessedPerLevel (num / denom) ----
            // PartialQuantity at runtime — modder enters numerator and
            // denominator so fractional rates (1.2 = 6/5) round-trip
            // exactly. Both halves null = inherit; either half alone
            // logs as a parse error at registration time.
            Row procRow = new Row();
            procRow.Gap(4.pt()).AlignItemsCenter();
            procRow.Add(new Label(new LocStrFormatted("processedPerLevel  num /")).TinyFontSize());
            procRow.Add(buildNullableIntField(
                () => m_ref.ProcessedPerLevelNumerator,
                v => m_ref.ProcessedPerLevelNumerator = v, 60));
            procRow.Add(new Label(new LocStrFormatted("denom")).TinyFontSize());
            procRow.Add(buildNullableIntField(
                () => m_ref.ProcessedPerLevelDenominator,
                v => m_ref.ProcessedPerLevelDenominator = v, 60));
            Add(procRow);

            // ---- Buffers + meltdown + default step ----
            Add(label("buffersCapacity (raw quantity; blank = inherit)"));
            Add(buildNullableIntField(
                () => m_ref.BuffersCapacity,
                v => m_ref.BuffersCapacity = v, 100));

            Add(buildTriStateBool("destroyContentOnMeltdown",
                () => m_ref.DestroyContentOnMeltdown,
                v => m_ref.DestroyContentOnMeltdown = v));

            Add(label("defaultEnrichmentStep (0-based step index at startup; blank = inherit)"));
            Add(buildNullableIntField(
                () => m_ref.DefaultEnrichmentStep,
                v => m_ref.DefaultEnrichmentStep = v, 60));

            // ---- Step list ----
            Add(new Label(new LocStrFormatted(
                "steps (breeding curve — empty list = inherit source steps)")).TinyFontSize());
            if (m_ref.Steps == null) m_ref.Steps = new List<EnrichmentStepRef>();
            Add(new EnrichmentStepListEditor(m_ref.Steps, m_onChanged));
        }

        private static Label label(string text) {
            return new Label(new LocStrFormatted(text)).TinyFontSize();
        }

        private TextField buildNullableIntField(Func<int?> get, Action<int?> set, int width) {
            TextField f = new TextField().PositiveIntegersOnly().Width(width.px());
            int? cur = get();
            f.Text(cur.HasValue ? cur.Value.ToString(CultureInfo.InvariantCulture) : "");
            f.OnValueChanged(v => {
                if (string.IsNullOrWhiteSpace(v)) {
                    set(null);
                } else if (int.TryParse(v.Trim(),
                        NumberStyles.Integer, CultureInfo.InvariantCulture, out int n)) {
                    set(n);
                }
                m_onChanged?.Invoke();
            });
            return f;
        }

        /// Tri-state selector — null (inherit) / true / false. Mirrors the
        /// segmented-button pattern in PortListEditor; ButtonText carries
        /// Cls.selected for the currently-active value and clicking any
        /// option re-applies the selection across the row.
        private UiComponent buildTriStateBool(string lbl, Func<bool?> get, Action<bool?> set) {
            Row r = new Row();
            r.Gap(4.pt()).AlignItemsCenter();
            r.Add(new Label(new LocStrFormatted(lbl)).TinyFontSize());
            (string Label, bool? Value)[] options = {
                ("inherit", (bool?)null),
                ("yes",     true),
                ("no",      false),
            };
            List<ButtonText> btns = new List<ButtonText>();
            foreach (var (txt, val) in options) {
                bool? capturedVal = val;
                ButtonText btn = null;
                btn = new ButtonText(new LocStrFormatted(txt), () => {
                    set(capturedVal);
                    m_onChanged?.Invoke();
                    foreach (ButtonText b in btns) {
                        b.ClassIff(Cls.selected, ReferenceEquals(b, btn));
                    }
                });
                btn.ClassIff(Cls.selected, get().Equals(capturedVal));
                btns.Add(btn);
                r.Add(btn);
            }
            return r;
        }
    }
}
