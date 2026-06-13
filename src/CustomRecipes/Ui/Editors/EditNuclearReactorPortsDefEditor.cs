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

    /// <c>edit_nuclear_reactor_ports</c> editor — reactor-typed sibling
    /// of <see cref="EditMachinePortsDefEditor"/>. Typical use is adding
    /// the enrichment in/out ports (or any auxiliary connection) to an
    /// existing NuclearReactorProto. Click an empty cell adjacent to the
    /// reactor footprint to drop a draft port, or fine-tune via the
    /// textual PortListEditor; both surfaces write to the same
    /// <see cref="EditNuclearReactorPortsDef.AddPorts"/> list.
    public sealed class EditNuclearReactorPortsDefEditor : DefEditor<EditNuclearReactorPortsDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;
        private readonly Column m_portsHolder;
        private readonly Column m_previewHolder;
        private PortListEditor m_portList;

        public EditNuclearReactorPortsDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_packModel = packModel;
            m_protosDb  = protosDb;

            // Typed ProtoPicker — restricts the choice to live
            // NuclearReactorProtos in the prototypes DB.
            Column reactorHolder = new Column();
            reactorHolder.AlignItemsStretch();
            AddField("reactor (target to extend)", reactorHolder, onRefresh: () => {
                reactorHolder.Clear();
                if (value == null) return;
                reactorHolder.Add(new ProtoPicker<NuclearReactorProto>(
                    m_protosDb,
                    getId: () => value?.ReactorId,
                    setId: id => {
                        if (value != null) value.ReactorId = id;
                        refreshPreview();
                    },
                    title: new LocStrFormatted("Pick target reactor"),
                    variableResolver: EditorHelpers.VariableResolverFor(value)));
            });

            m_previewHolder = new Column();
            m_previewHolder.AlignItemsStretch();
            AddField(
                "layout (click an empty slot to add a port; click a draft port to remove)",
                m_previewHolder, onRefresh: refreshPreview);

            m_portsHolder = new Column();
            m_portsHolder.AlignItemsStretch();
            AddField("add_ports", m_portsHolder, onRefresh: () => {
                m_portsHolder.Clear();
                m_portList = null;
                if (value == null) return;
                if (value.AddPorts == null) {
                    value.AddPorts = new List<PortRef>();
                }
                m_portList = new PortListEditor(value.AddPorts, refreshPreview);
                m_portsHolder.Add(m_portList);
            });

            AddCommentField();
        }

        // Default shape for a click-placed port. Most enrichment / aux
        // reactor ports are conveyors; modder can change via the
        // PortListEditor row. (Pipe / FlatConveyor / LooseMaterialConveyor /
        // MoltenMetalChannel are the four typical reactor shapes.)
        private const string DefaultShapeId = "IoPortShape_FlatConveyor";

        private char freshPortName(NuclearReactorProto reactor) {
            HashSet<char> used = new HashSet<char>();
            if (reactor != null) {
                foreach (var p in reactor.Layout.Ports) used.Add(p.Name);
            }
            if (value?.AddPorts != null) {
                foreach (var pr in value.AddPorts) {
                    if (!string.IsNullOrEmpty(pr.Name)) used.Add(pr.Name[0]);
                }
            }
            for (char c = 'a'; c <= 'z'; c++) if (!used.Contains(c)) return c;
            for (char c = 'A'; c <= 'Z'; c++) if (!used.Contains(c)) return c;
            for (char c = '0'; c <= '9'; c++) if (!used.Contains(c)) return c;
            return '?';
        }

        private void refreshPreview() {
            m_previewHolder.Clear();
            if (value == null) return;

            NuclearReactorProto reactor = RecipeFormParts.ResolveProtoSafe<NuclearReactorProto>(
                m_protosDb, value.ReactorId);
            if (reactor == null) {
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.ReactorId)
                        ? "(pick a reactor to see its layout)"
                        : "(reactor '" + value.ReactorId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }
            if (value.AddPorts == null) value.AddPorts = new List<PortRef>();
            var pendings = RecipeFormParts.ResolvePendingPorts(value.AddPorts);
            m_previewHolder.Add(RecipeFormParts.BuildInteractivePortLayoutGrid(
                reactor.Layout, pendings,
                onPlace: (bx, by, dir) => {
                    char name = freshPortName(reactor);
                    value.AddPorts.Add(new PortRef {
                        Name      = name.ToString(),
                        Type      = "input",
                        Shape     = DefaultShapeId,
                        PositionX = bx,
                        PositionY = by,
                        PositionZ = 0,
                        Direction = dir,
                    });
                    value.Dirty = true;
                    m_portList?.Refresh();
                    refreshPreview();
                },
                onRemove: pr => {
                    value.AddPorts.Remove(pr);
                    value.Dirty = true;
                    m_portList?.Refresh();
                    refreshPreview();
                }));
        }
    }
}
