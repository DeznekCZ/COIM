using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.NuclearReactors;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_nuclear_reactor_fuels</c> editor — typed picker for the
    /// target NuclearReactorProto + a FuelPairListEditor for the fuels
    /// to append. Much smaller surface than the full reactor editor —
    /// the modder only configures what's added, not the whole proto.
    public sealed class EditNuclearReactorFuelsDefEditor : DefEditor<EditNuclearReactorFuelsDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        private readonly Column m_reactorHolder;
        private readonly Column m_fuelsHolder;
        private FuelPairListEditor m_fuelList;

        public EditNuclearReactorFuelsDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_packModel = packModel;
            m_protosDb  = protosDb;

            // Reactor picker — typed ProtoPicker<NuclearReactorProto> so
            // the modder can only pick a real reactor proto.
            m_reactorHolder = new Column();
            m_reactorHolder.AlignItemsStretch();
            AddField("reactor (target NuclearReactorProto to extend)",
                m_reactorHolder, onRefresh: () => {
                    m_reactorHolder.Clear();
                    if (value == null) return;
                    m_reactorHolder.Add(new ProtoPicker<NuclearReactorProto>(
                        m_protosDb,
                        getId: () => value?.ReactorId,
                        setId: id => { if (value != null) value.ReactorId = id; },
                        title: new LocStrFormatted("Pick target reactor"),
                        variableResolver: EditorHelpers.VariableResolverFor(value)));
                });

            // FuelPairListEditor — same component used inside the full
            // reactor editor. Each row picks fuel-in + spent-fuel-out
            // products and a duration. Edits propagate to value.AddFuels.
            m_fuelsHolder = new Column();
            m_fuelsHolder.AlignItemsStretch();
            AddField("add_fuels (FuelPair entries to append)",
                m_fuelsHolder, onRefresh: () => {
                    m_fuelsHolder.Clear();
                    m_fuelList = null;
                    if (value == null) return;
                    if (value.AddFuels == null) value.AddFuels = new List<FuelPairRef>();
                    m_fuelList = new FuelPairListEditor(
                        value.AddFuels,
                        onChanged: () => { value.Dirty = true; },
                        m_packModel, m_protosDb);
                    m_fuelsHolder.Add(m_fuelList);
                });

            AddCommentField();
        }
    }
}
