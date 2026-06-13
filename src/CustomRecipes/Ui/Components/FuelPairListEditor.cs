using System;
using System.Collections.Generic;
using CustomAssets.Editor.Model;
using Mafi;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Components {

    /// Editable list of <see cref="FuelPairRef"/> rows — embedded by
    /// <see cref="Editors.NuclearReactorDefEditor"/> to surface the
    /// reactor's optional fuel-cycle list. Each row carries:
    ///   - fuelIn product picker (any ProductProto)
    ///   - spentFuelOut product picker
    ///   - durationSeconds (signed-int text field)
    ///   - remove button
    /// The footer "+ add fuel pair" seeds a new entry with empty product
    /// ids + 60 seconds. Same shape as PortListEditor — class-based with
    /// a public Refresh() so the parent editor can rebuild after layout
    /// changes that need a typed-rebind.
    public sealed class FuelPairListEditor : Column {

        private readonly List<FuelPairRef> m_list;
        private readonly Action m_onChanged;
        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        public FuelPairListEditor(List<FuelPairRef> list, Action onChanged,
                PackModel packModel, ProtosDb protosDb) {
            m_list = list ?? new List<FuelPairRef>();
            m_onChanged = onChanged;
            m_protosDb = protosDb;
            m_packModel = packModel;
            this.AlignItemsStretch();
            this.Gap(2.pt());
            Refresh();
        }

        public void Refresh() {
            Clear();
            for (int i = 0; i < m_list.Count; i++) {
                FuelPairRef p = m_list[i];
                int captured = i;
                Add(buildRow(p, onRemove: () => {
                    m_list.RemoveAt(captured);
                    m_onChanged?.Invoke();
                    Refresh();
                }, onFieldEdit: m_onChanged));
            }
            Add(new ButtonText(
                new LocStrFormatted("+ add fuel pair"),
                () => {
                    m_list.Add(new FuelPairRef {
                        FuelIn = "", SpentFuelOut = "", DurationSeconds = 60,
                    });
                    m_onChanged?.Invoke();
                    Refresh();
                }));
        }

        private UiComponent buildRow(FuelPairRef p, Action onRemove, Action onFieldEdit) {
            Column row = new Column();
            row.Gap(2.pt()).Padding(4.px()).AlignItemsStretch()
               .Border(1.px(), ColorRgba.DarkGray, 2);

            Row topLine = new Row();
            topLine.Gap(4.pt()).AlignItemsCenter();
            topLine.Add(new Label(new LocStrFormatted("fuelIn")).TinyFontSize());
            // Typed product picker — restricts the choice to live
            // ProductProtos in the prototypes DB and renders the product
            // icon next to the id.
            var fuelInPicker = new ProtoPicker<ProductProto>(
                m_protosDb,
                getId: () => p?.FuelIn,
                setId: id => { p.FuelIn = id; onFieldEdit?.Invoke(); },
                title: new LocStrFormatted("Pick fresh fuel product"));
            fuelInPicker.FlexGrow(1f);
            topLine.Add(fuelInPicker);
            topLine.Add(new ButtonText(new LocStrFormatted("✕"), onRemove)
                .Tooltip(new LocStrFormatted("Remove this fuel pair")));
            row.Add(topLine);

            Row midLine = new Row();
            midLine.Gap(4.pt()).AlignItemsCenter();
            midLine.Add(new Label(new LocStrFormatted("spentFuelOut")).TinyFontSize());
            var spentFuelOutPicker = new ProtoPicker<ProductProto>(
                m_protosDb,
                getId: () => p?.SpentFuelOut,
                setId: id => { p.SpentFuelOut = id; onFieldEdit?.Invoke(); },
                title: new LocStrFormatted("Pick spent fuel product"));
            spentFuelOutPicker.FlexGrow(1f);
            midLine.Add(spentFuelOutPicker);
            row.Add(midLine);

            Row bottomLine = new Row();
            bottomLine.Gap(4.pt()).AlignItemsCenter();
            bottomLine.Add(new Label(new LocStrFormatted("durationSeconds (fuel lifetime at power level 1)")).TinyFontSize());
            TextField durField = new TextField().AllIntegersOnly().Width(80.px());
            durField.Text(p.DurationSeconds.ToString(System.Globalization.CultureInfo.InvariantCulture));
            durField.OnValueChanged(v => {
                if (int.TryParse(v,
                        System.Globalization.NumberStyles.Integer,
                        System.Globalization.CultureInfo.InvariantCulture, out int n)) {
                    p.DurationSeconds = n;
                }
                onFieldEdit?.Invoke();
            });
            bottomLine.Add(durField);
            row.Add(bottomLine);

            return row;
        }
    }
}
