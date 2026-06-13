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

    /// <c>build_machine</c> editor — sibling of <see cref="GeneratorDefEditor"/>
    /// for general MachineProtos. Creates a new machine by cloning a
    /// source MachineProto and applying overrides; the <c>copy_*</c>
    /// flags decide which source fields are inherited vs. blank.
    ///
    /// Layout follows GeneratorDefEditor's vertical-stack of TextFields
    /// + machine picker + shared <see cref="PortListEditor"/> for ports +
    /// interactive layout grid for click-to-place port placement.
    public sealed class BuildMachineDefEditor : NamedDefEditor<BuildMachineDef> {

        private readonly ProtosDb m_protosDb;
        private readonly PackModel m_packModel;

        private readonly TextField m_id;
        private readonly TextField m_name;
        private readonly TextField m_description;
        private readonly Column m_sourceHolder;
        private readonly Column m_previewHolder;
        private readonly Column m_portsHolder;
        private readonly TextField m_powerKw;
        private readonly ResearchIdPicker m_research;
        private readonly TextField m_copyRecipes;
        private readonly TextField m_copyLayout;
        private readonly TextField m_copyPorts;
        private readonly TextField m_copyGraphics;
        private readonly TextField m_lockedOnInit;
        // See [[EditMachinePortsDefEditor.m_portList]] — held for direct
        // Refresh() after a layout-grid click.
        private PortListEditor m_portList;

        public BuildMachineDefEditor(PackModel packModel, ProtosDb protosDb) {
            m_packModel = packModel;
            m_protosDb  = protosDb;

            m_id = AddField("machineId (new id for the machine)",
                new TextField().OnValueChanged(v => { if (value != null) value.MachineId = v; }),
                onRefresh: () => m_id.Text(value.MachineId ?? ""));

            m_name = AddField("name (defaults to source name)",
                new TextField().OnValueChanged(v => { if (value != null) value.Name = v; }),
                onRefresh: () => m_name.Text(value.Name ?? ""));

            m_description = AddField("description (defaults to source description)",
                new TextField()
                    .Multiline(true)
                    .SetTextAreaMinHeight(48.px())
                    .OnValueChanged(v => { if (value != null) value.Description = v; }),
                onRefresh: () => m_description.Text(value.Description ?? ""));

            m_sourceHolder = new Column();
            m_sourceHolder.AlignItemsStretch();
            AddField("source (machine to clone visuals + layout from)", m_sourceHolder,
                onRefresh: () => {
                    m_sourceHolder.Clear();
                    if (value == null) return;
                    m_sourceHolder.Add(new ProtoPicker<MachineProto>(
                        m_protosDb,
                        getId: () => value?.SourceId,
                        setId: id => {
                            if (value != null) value.SourceId = id;
                            refreshPreview();
                        },
                        title: new LocStrFormatted("Pick source machine"),
                        variableResolver: EditorHelpers.VariableResolverFor(value)));
                });

            m_previewHolder = new Column();
            m_previewHolder.AlignItemsStretch();
            AddField(
                "layout (click an empty slot to add a port; click a draft port to remove)",
                m_previewHolder, onRefresh: refreshPreview);

            // "Import source ports" affordance — copies the resolved source
            // machine's existing ports into AddPorts as PortRef rows the
            // modder can then edit/remove inline. Mirror of the Python-side
            // workflow when copy_ports=False: the modder doesn't inherit
            // the source ports at runtime, so the editor pre-populates the
            // list with the same shape (each port name/type/shape/position/
            // direction/conveyor-only flag carried over) and lets them
            // trim it.
            ButtonText importPortsBtn = new ButtonText(
                new LocStrFormatted("⤓ Import source ports"),
                onImportSourcePorts);
            importPortsBtn.Tooltip(new LocStrFormatted(
                "Copy each port from the resolved source machine into the "
                + "ports list. Skips port names already present. Combined "
                + "with copy_ports=False this lets you start from the "
                + "source's ports and modify the set without inheriting "
                + "them at runtime."));
            Add(importPortsBtn);

            m_portsHolder = new Column();
            m_portsHolder.AlignItemsStretch();
            AddField("ports (extras on top of source when copy_ports=true; otherwise the full set)", m_portsHolder,
                onRefresh: () => {
                    m_portsHolder.Clear();
                    m_portList = null;
                    if (value == null) return;
                    if (value.AddPorts == null) value.AddPorts = new List<PortRef>();
                    m_portList = new PortListEditor(value.AddPorts, refreshPreview);
                    m_portsHolder.Add(m_portList);
                });

            m_powerKw = AddField(
                "consumedPowerPerTick (kW; blank = inherit from source)",
                new TextField()
                    .PositiveIntegersOnly()
                    .OnValueChanged(v => {
                        if (value != null) value.ConsumedPowerPerTickKw = EditorHelpers.ParseNullableInt(v);
                    }),
                onRefresh: () => m_powerKw.Text(value.ConsumedPowerPerTickKw.HasValue
                    ? value.ConsumedPowerPerTickKw.Value.ToString() : ""));

            m_research = AddField("research (optional; unlocks this machine)",
                new ResearchIdPicker(
                    m_packModel, m_protosDb, ownerDef: null,
                    getId: () => value?.ResearchId,
                    setId: id => { if (value != null) value.ResearchId = string.IsNullOrEmpty(id) ? null : id; },
                    title: new LocStrFormatted("Pick research")),
                onRefresh: () => m_research.RefreshDisplay());

            m_copyRecipes = AddField(
                "copy_recipes (true = republish source recipes; default true)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true") value.CopyRecipes = true;
                    else if (s == "false") value.CopyRecipes = false;
                    else value.CopyRecipes = true;
                }),
                onRefresh: () => m_copyRecipes.Text(value.CopyRecipes ? "true" : "false"));

            m_copyLayout = AddField(
                "copy_layout (false = blank 1x1 layout; default true)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "false") value.CopyLayout = false;
                    else value.CopyLayout = true;
                }),
                onRefresh: () => m_copyLayout.Text(value.CopyLayout ? "true" : "false"));

            m_copyPorts = AddField(
                "copy_ports (false = drop source ports; default true). Pair with the 'Import source ports' button above to populate the ports list with the source's port set as an editable starting list.",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "false") value.CopyPorts = false;
                    else value.CopyPorts = true;
                    // Toggling this changes whether the source's ports
                    // show on the layout preview, so re-render right
                    // away — the modder sees the new machine's port
                    // surface match their choice the moment they flip
                    // the flag.
                    refreshPreview();
                }),
                onRefresh: () => m_copyPorts.Text(value.CopyPorts ? "true" : "false"));

            m_copyGraphics = AddField(
                "copy_graphics (false = empty Gfx; default true)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "false") value.CopyGraphics = false;
                    else value.CopyGraphics = true;
                }),
                onRefresh: () => m_copyGraphics.Text(value.CopyGraphics ? "true" : "false"));

            m_lockedOnInit = AddField(
                "lockedOnInit (true / false / blank to inherit research default)",
                new TextField().OnValueChanged(v => {
                    if (value == null) return;
                    string s = (v ?? "").Trim().ToLowerInvariant();
                    if (s == "true") value.LockedOnInit = true;
                    else if (s == "false") value.LockedOnInit = false;
                    else value.LockedOnInit = null;
                }),
                onRefresh: () => m_lockedOnInit.Text(value.LockedOnInit.HasValue
                    ? (value.LockedOnInit.Value ? "true" : "false")
                    : ""));

            // Visual footprint / box-type / mesh editor. Complements the
            // port preview grid above: this panel authors the tile shape
            // (layout_str, overriding copy_layout) and the mesh, while the
            // port grid keeps handling add_ports in COI coordinates.
            AddLayoutEditor(m_packModel, m_protosDb);
        }

        // Resolve the current source machine and append PortRefs for every
        // port it carries into AddPorts. Skips port names already present
        // in AddPorts so a second click is idempotent. Marks the def dirty
        // and refreshes both the port list editor and the layout preview
        // so the new rows show up immediately. When no source machine is
        // resolved the call no-ops — the modder needs to pick one first.
        private void onImportSourcePorts() {
            if (value == null) return;
            MachineProto src = RecipeFormParts.ResolveMachine(m_protosDb, value.SourceId);
            if (src == null) return;
            if (value.AddPorts == null) value.AddPorts = new List<PortRef>();
            HashSet<string> existingNames = new HashSet<string>(System.StringComparer.Ordinal);
            foreach (PortRef pr in value.AddPorts) {
                if (!string.IsNullOrEmpty(pr.Name)) existingNames.Add(pr.Name);
            }
            int added = 0;
            foreach (var port in src.Layout.Ports) {
                string name = port.Name.ToString();
                if (existingNames.Contains(name)) continue;
                value.AddPorts.Add(new PortRef {
                    Name      = name,
                    Type      = port.Type.ToString().ToLowerInvariant(),
                    Shape     = port.Shape != null ? port.Shape.Id.Value : null,
                    PositionX = port.RelativePosition.X,
                    PositionY = port.RelativePosition.Y,
                    PositionZ = port.RelativePosition.Z,
                    Direction = port.RelativeDirection.ToString(),
                    CanOnlyConnectToTransports = port.Spec.CanOnlyConnectToTransports,
                });
                existingNames.Add(name);
                added++;
            }
            if (added > 0) {
                value.Dirty = true;
                m_portList?.Refresh();
                refreshPreview();
            }
        }

        private const string DefaultShapeId = "IoPortShape_FlatConveyor";

        private char freshPortName(MachineProto source) {
            HashSet<char> used = new HashSet<char>();
            if (source != null) {
                foreach (var p in source.Layout.Ports) used.Add(p.Name);
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

            MachineProto src = RecipeFormParts.ResolveMachine(m_protosDb, value.SourceId);
            if (src == null) {
                m_previewHolder.Add(new Label(new LocStrFormatted(
                    string.IsNullOrEmpty(value.SourceId)
                        ? "(pick a source machine to see its layout)"
                        : "(machine '" + value.SourceId + "' could not be resolved)"))
                    .Color(ColorRgba.LightGray));
                return;
            }
            if (value.AddPorts == null) value.AddPorts = new List<PortRef>();
            var pendings = RecipeFormParts.ResolvePendingPorts(value.AddPorts);
            // copy_ports=false drops the source's ports at runtime, so the
            // preview hides them too — modders see exactly what the new
            // machine will carry instead of being misled by phantom source
            // ports that won't be there.
            m_previewHolder.Add(RecipeFormParts.BuildInteractivePortLayoutGrid(
                src.Layout, pendings,
                onPlace: (bx, by, dir) => {
                    char name = freshPortName(src);
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
                onRemove: (pr) => {
                    value.AddPorts.Remove(pr);
                    value.Dirty = true;
                    m_portList?.Refresh();
                    refreshPreview();
                },
                showSourcePorts: value.CopyPorts));
        }
    }
}
