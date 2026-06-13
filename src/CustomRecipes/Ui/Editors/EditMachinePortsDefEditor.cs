using System.Collections.Generic;
using CustomAssets.Editor.Model;
using CustomAssets.Ui.Components;
using Mafi;
using Mafi.Core.Factory.Machines;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace CustomAssets.Ui.Editors {

    /// <c>edit_machine_ports</c> editor — pick the target machine, then
    /// place additional ports either by clicking the layout grid (an empty
    /// cell adjacent to the building edge becomes a port) or by editing
    /// the textual <see cref="PortRef"/> rows. Both surfaces write to the
    /// same <see cref="EditMachinePortsDef.AddPorts"/> list so they stay
    /// in sync.
    public sealed class EditMachinePortsDefEditor : DefEditor<EditMachinePortsDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;
        private readonly Column m_portsHolder;
        private readonly Column m_previewHolder;
        // Live PortListEditor instance for the currently-bound def. Held so
        // out-of-band mutations (layout-grid clicks) can call Refresh()
        // directly instead of routing through the observer rebuild.
        private PortListEditor m_portList;

        public EditMachinePortsDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_packModel = packModel;
            m_protosDb  = protosDb;

            // MachineIdPicker surfaces this pack's BuildMachineDefs
            // alongside game machines, so edit_machine_ports can target a
            // machine the modder is defining in the same pack via its
            // variable binding.
            MachineIdPicker machinePicker = new MachineIdPicker(
                m_packModel, m_protosDb,
                getId: () => value?.MachineId,
                setId: id => {
                    if (value != null) value.MachineId = id;
                    refreshPreview();
                },
                getOwnerDef: () => value,
                title: new LocStrFormatted("Pick machine to extend"));
            AddField("machine (target to extend)", machinePicker,
                onRefresh: () => machinePicker.RefreshDisplay());

            // Layout-grid placement surface. Sits above the textual port
            // list so the modder reads "click here to add" before scrolling
            // down to fine-tune the resulting PortRef.
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
                    value.AddPorts = new System.Collections.Generic.List<PortRef>();
                }
                m_portList = new PortListEditor(value.AddPorts, refreshPreview);
                m_portsHolder.Add(m_portList);
            });
        }

        // Map "+X" / "-X" / "+Y" / "-Y" to the IoPortShape id we'll seed
        // a new draft port with. We don't know the modder's intent yet
        // (fluid? loose? unit?) so we default to FlatConveyor — the most
        // common shape for unit-product machines — and let the modder
        // adjust via the quick-pick chips in PortListEditor.
        private const string DefaultShapeId = "IoPortShape_FlatConveyor";

        // Compute a fresh port label letter that doesn't already collide
        // with anything in the source layout or the current draft list.
        // Walks 'a'..'z' then 'A'..'Z' then '0'..'9' before falling back
        // to '?'. Keeps generated names readable and stable.
        private char freshPortName(MachineProto machine) {
            HashSet<char> used = new HashSet<char>();
            if (machine != null) {
                foreach (var p in machine.Layout.Ports) used.Add(p.Name);
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

            MachineProto machine = RecipeFormParts.ResolveMachine(m_protosDb, value.MachineId);
            if (machine == null) {
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.MachineId)
                        ? "(pick a machine to see its layout)"
                        : "(machine '" + value.MachineId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }
            if (value.AddPorts == null) value.AddPorts = new List<PortRef>();
            var pendings = RecipeFormParts.ResolvePendingPorts(value.AddPorts);
            m_previewHolder.Add(RecipeFormParts.BuildInteractivePortLayoutGrid(
                machine.Layout, pendings,
                onPlace: (bx, by, dir) => {
                    char name = freshPortName(machine);
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
                    // Refresh the port list directly (each row carries
                    // captured per-PortRef closures so the safest update
                    // is a full rebuild) AND re-render the layout overlay.
                    m_portList?.Refresh();
                    refreshPreview();
                },
                onRemove: (pr) => {
                    value.AddPorts.Remove(pr);
                    value.Dirty = true;
                    m_portList?.Refresh();
                    refreshPreview();
                }));
        }
    }
}
