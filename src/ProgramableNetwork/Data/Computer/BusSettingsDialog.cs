using Mafi;
using Mafi.Core.Entities;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;

namespace ProgramableNetwork.Ui
{
    // Settings popup for a single variable bus, opened by clicking the bus name in
    // the gutter.  Rename the bus, name + type its 4 pins, pick a Controller-type
    // pin's remote source, and delete the bus.
    //
    // Multi-view: the body is rebuilt in place (m_body.Clear + re-add) for the
    // controller / output sub-pickers — this is dialog-internal, so it never calls
    // view.RedrawComponents() (which would destroy the anchor and close the dialog).
    // The gutter is refreshed once on close via OnCloseDone.
    //
    // NOTE: the source controller is chosen here from an in-dialog list of in-range
    // controllers; selecting it on the MAP (cursor pick) is a planned follow-up.
    public class BusSettingsDialog : FloatingColumn
    {
        private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();

        // The only module whose pins a Controller-type bus pin may bind to — the remote
        // controller's named publishing endpoint.
        private static readonly string CONNECTION_CONTROLLER_OUTPUT = "Connection_Controller_Output".ModuleId();
        private const string CONNECTION_CONTROLLER_LABEL = "Connection: Controller";

        private readonly ControllerView m_view;
        private readonly Controller m_controller;
        private readonly ControllerBus m_bus;
        private readonly Column m_body;
        // The controller currently world-highlighted by a hovered picker row, so it can be
        // cleared on navigation/close (a removed button may never fire its mouse-leave).
        private Controller m_hoverHighlight;

        public BusSettingsDialog(ControllerView view, ControllerBus bus)
            : base(POLICY, false, false, true)
        {
            m_view = view;
            m_controller = view.Entity;
            m_bus = bus;

            // Refresh the gutter once, on close — never mid-edit (a live redraw would
            // destroy the anchor button and close this dialog).  Also drop any lingering
            // world highlight from a hovered controller row.
            this.OnCloseDone += _ =>
            {
                clearHoverHighlight();
                view.RedrawComponents();
            };

            PanelWithHeader panel = new PanelWithHeader(("Bus: " + bus.Name).AsLoc());
            panel.Height(Px.Auto);
            Add(panel);

            m_body = panel.Body;
            m_body.Gap(5.px());

            this.Width(260.px());
            this.Height(Px.Auto);

            showMain();
        }

        // ---- Views -----------------------------------------------------------------

        private void showMain()
        {
            clearHoverHighlight();
            m_body.Clear();
            m_body.Add(labeledRow("Name", nameEditor()));

            // Side is set by the slot the bus was added to, so no side control here.
            for (int i = 0; i < ControllerBus.PinCount; i++)
            {
                int idx = i;

                Row pinRow = new Row();
                pinRow.Gap(4.px());

                TextField nameTf = new TextField();
                nameTf.Value(new LocStrFormatted(m_bus.PinNames[idx] ?? ""));
                nameTf.Width(96.px());
                nameTf.Height(Sizes.BLOCK_SIZE);
                nameTf.OnValueChanged((e) => m_controller.SetBusPinName(m_bus.Id, idx, nameTf.GetText()));
                pinRow.Add(nameTf);

                Label typeLabel = new Label(pinTypeLabel(m_bus.PinTypes[idx]));
				typeLabel.Tooltip(pinTypeTooltip(m_bus.PinTypes[idx]));
				typeLabel.Width(64.px());
                pinRow.Add(typeLabel);

                ButtonText cycleBtn = new ButtonText("▸".AsLoc());
                cycleBtn.Width(Sizes.BLOCK_SIZE);
                cycleBtn.Height(Sizes.BLOCK_SIZE);
                cycleBtn.OnClick(() =>
                {
                    m_controller.SetBusPinType(m_bus.Id, idx, nextPinType(m_bus.PinTypes[idx]));
                    showMain(); // rebuild so the source row appears/disappears with the type
                });
                pinRow.Add(cycleBtn);

                m_body.Add(labeledRow("Pin " + (idx + 1), pinRow));

                // Controller-type pins read a specific output on another controller.
                if (m_bus.PinTypes[idx] == ControllerBus.BusPinType.Controller)
                {
                    Row srcRow = new Row();
                    srcRow.Gap(4.px());
                    Label srcLabel = new Label(describeSource(m_bus.PinSources[idx]).AsLoc());
                    srcLabel.Width(120.px());
                    srcRow.Add(srcLabel);
                    ButtonText pickBtn = new ButtonText("pick".AsLoc());
                    pickBtn.OnClick(() => showControllerList(idx));
                    srcRow.Add(pickBtn);
                    m_body.Add(labeledRow("  src", srcRow));
                }
            }

            ButtonText delete = new ButtonText("Delete bus".AsLoc());
            delete.OnClick(() =>
            {
                m_controller.RemoveBus(m_bus.Id);
                Close(); // OnCloseDone redraws the gutter, dropping the removed bus.
            });
            m_body.Add(delete);

            m_body.AddAndReturn(new UiComponent()).FlexGrow(1);
        }

        // Highlights (on) / clears (off) a controller in the world for a hovered picker
        // row, tracking the active one so navigation/close can drop it deterministically.
        private void hoverHighlightController(Controller c, bool on)
        {
            if (on)
            {
                if (m_hoverHighlight != null && m_hoverHighlight != c)
                {
                    m_view.Inspector.Context.Highlighter.RemoveHighlight(m_hoverHighlight);
                }
                m_hoverHighlight = c;
                m_view.Inspector.Context.Highlighter.Highlight(c, ColorRgba.Green);
            }
            else if (m_hoverHighlight == c)
            {
                m_view.Inspector.Context.Highlighter.RemoveHighlight(c);
                m_hoverHighlight = null;
            }
        }

        private void clearHoverHighlight()
        {
            if (m_hoverHighlight != null)
            {
                m_view.Inspector.Context.Highlighter.RemoveHighlight(m_hoverHighlight);
                m_hoverHighlight = null;
            }
            // Also drop any source-arrow + highlight from a hovered output group.
            m_view.Inspector.ClearBusSourceArrow();
        }

        private void showControllerList(int pinIdx)
        {
            clearHoverHighlight();
            m_body.Clear();
            m_body.Add(backButton(showMain));
            m_body.Add(new Label(("Controller for pin " + (pinIdx + 1)).AsLoc()));

            // Only OTHER in-range controllers are listable — reading this controller's
            // own pins makes no sense here (local pins are connected directly on the
            // grid / via PLC), so self is intentionally excluded.
            bool any = false;
            foreach (Controller c in m_view.Inspector.Context.EntitiesManager.GetAllEntitiesOfType<Controller>())
            {
                if (!m_controller.IsInBusLinkRange(c))
                {
                    continue;
                }
                Controller remote = c;
                ButtonText b = new ButtonText(c.GetTitle().AsLoc());
                b.OnClick(() => showOutputList(pinIdx, remote));
                // Highlight the candidate controller in the world while hovering its row,
                // so the player can see which physical controller they're about to pick.
                b.OnMouseEnterLeave(
                    () => hoverHighlightController(remote, true),
                    () => hoverHighlightController(remote, false));
                m_body.Add(b);
                any = true;
            }

            if (!any)
            {
                m_body.Add(new Label("(no controllers in range)".AsLoc()));
            }
        }

        private void showOutputList(int pinIdx, Controller remote)
        {
            clearHoverHighlight();
            m_body.Clear();
            m_body.Add(backButton(() => showControllerList(pinIdx)));
            m_body.Add(new Label(("Output on " + remote.GetTitle()).AsLoc()));

            // All selectable sources live in one group panel; hovering anywhere in it
            // highlights the remote controller and draws a moving-arrows line from it to
            // THIS controller, so the player sees where the value is read from.
            Column sources = new Column();
            sources.Class(Mafi.Unity.UiToolkit.Cls.group);
            sources.Gap(2.px());
            sources.OnMouseEnterLeave(
                () => m_view.Inspector.ShowBusSourceArrow(remote),
                () => m_view.Inspector.ClearBusSourceArrow());

            bool any = false;

            // A Controller-type bus pin reads a remote controller's published values.
            // The publishing endpoint is the "Connection: Controller (output)" module,
            // which exposes its named pins (a/b/c/d + extensions) under a name.  Those
            // are modelled as the module's INPUTS (cabled locally on the remote), so we
            // iterate EffectiveInputs and list them — every other module type is
            // intentionally skipped (its raw outputs aren't a stable cross-controller
            // contract).
            foreach (Module m in remote.Modules)
            {
                if (m?.Prototype == null || m.Prototype.Id.Value != CONNECTION_CONTROLLER_OUTPUT)
                {
                    continue;
                }
                Module mod = m;
                foreach (ModuleConnectorProto pin in m.EffectiveInputs)
                {
                    if (pin == null || string.IsNullOrEmpty(pin.Id))
                    {
                        continue;
                    }
                    string outId = pin.Id;
                    ButtonText b = new ButtonText(moduleOutputName(mod, pin).AsLoc());
                    b.OnClick(() =>
                    {
                        m_controller.SetBusPinSource(m_bus.Id, pinIdx,
                            BusPinSource.ExternalModuleOutput(remote.Id, mod.Id, outId));
                        showMain();
                    });
                    sources.Add(b);
                    any = true;
                }
            }

            // Bus pins — only OUTPUT-type pins are exposed across controllers: an Output
            // pin is the controller's published value ("read by other controllers", per
            // its tooltip).  Controller pins are local relays and PLC pins are local
            // PLC↔module scratch — neither is a cross-controller source, so both are
            // skipped here.  Unnamed pins are addressed by their integer index (bus[i]).
            foreach (ControllerBus rb in remote.Buses)
            {
                for (int i = 0; i < ControllerBus.PinCount; i++)
                {
                    if (rb.PinTypes[i] != ControllerBus.BusPinType.Output)
                    {
                        continue;
                    }
                    ControllerBus remoteBus = rb;
                    int pi = i;
                    ButtonText b = new ButtonText(busPinName(rb, i).AsLoc());
                    b.OnClick(() =>
                    {
                        m_controller.SetBusPinSource(m_bus.Id, pinIdx,
                            BusPinSource.ExternalBusPin(remote.Id, remoteBus.Id, pi, remoteBus.Name));
                        showMain();
                    });
                    sources.Add(b);
                    any = true;
                }
            }

            if (any)
            {
                m_body.Add(sources);
            }
            else
            {
                // Nothing on the target controller exposes an output — tell the player
                // instead of a blank list.
                m_body.Add(new Label("(nothing to connect)".AsLoc()));
            }
        }

        // ---- Helpers ---------------------------------------------------------------

        private ButtonText backButton(System.Action onBack)
        {
            ButtonText b = new ButtonText("← back".AsLoc());
            b.OnClick(onBack);
            return b;
        }

        private TextField nameEditor()
        {
            TextField tf = new TextField();
            tf.Value(new LocStrFormatted(m_bus.Name ?? ""));
            tf.Width(150.px());
            tf.Height(Sizes.BLOCK_SIZE);
            // The gutter shows the name vertically in a narrow display strip, so cap it
            // at 7 characters (≈ the strip's height of 7 monospace lines).
            tf.CharLimit(7);
            tf.OnValueChanged((e) => m_controller.RenameBus(m_bus.Id, tf.GetText()));
            return tf;
        }

        private static UiComponent labeledRow(string label, UiComponent editor)
        {
            Row row = new Row();
            row.Gap(4.px());
            Label l = new Label(label.AsLoc());
            l.Width(56.px());
            row.Add(l);
            row.Add(editor);
            return row;
        }

        // Friendly label for a stored source, resolving the remote controller/module/
        // output to their display names (falling back to ids when an entity can no
        // longer be resolved).  Instance method so it can reach the entities manager.
        private string describeSource(BusPinSource src)
        {
            if (src == null)
            {
                return "(none)";
            }

            Controller remote = null;
            m_view.Inspector.Context.EntitiesManager.TryGetEntity(src.ControllerId, out remote);
            string where = remote != null && remote != m_controller
                ? remote.GetTitle() + ": "
                : "";

            switch (src.Kind)
            {
                case BusPinSource.SourceKind.ExternalModule:
                    Module mod = remote?.Modules?.Find(x => x.Id == src.ModuleId);
                    if (mod?.Prototype != null)
                    {
                        // The bound pin is the endpoint's published INPUT (see showOutputList),
                        // so resolve its display name via GetInputProto.
                        return where + moduleOutputName(mod, mod.GetInputProto(src.OutputId));
                    }
                    return where + "#" + src.ModuleId + " " + src.OutputId;
                case BusPinSource.SourceKind.ExternalControllerBus:
                    // Resolve by id first, then by the remembered name (the id can change
                    // on the remote across copy/paste / reorder; the name is stable).
                    ControllerBus rb = remote?.GetBusById(src.BusId) ?? findBusByName(remote, src.BusName);
                    if (rb != null)
                    {
                        return where + busPinName(rb, src.PinIndex);
                    }
                    // Unresolved — show the remembered name so the link is still legible.
                    string busLabel = string.IsNullOrEmpty(src.BusName) ? "bus " + src.BusId : src.BusName;
                    return where + busLabel + "[" + src.PinIndex + "]";
                default:
                    return src.Kind.ToString();
            }
        }

        // Display name for a Controller-pin source: a Controller-type bus pin only ever
        // binds to a "Connection: Controller (output)" module's named pin.  The label
        // includes the endpoint's user-set NAME field (so two such modules are told
        // apart) plus the pin's own display name (every published pin has one, "A"/...).
        private static string moduleOutputName(Module mod, ModuleConnectorProto pin)
        {
            string outName = pin?.Name.Name.TranslatedString;
            if (string.IsNullOrEmpty(outName))
            {
                outName = pin?.Id ?? "?";
            }
            string connName = mod != null ? mod.Field["name", ""] : "";
            string label = string.IsNullOrEmpty(connName)
                ? CONNECTION_CONTROLLER_LABEL
                : CONNECTION_CONTROLLER_LABEL + " \"" + connName + "\"";
            return label + " — " + outName;
        }

        // Finds a bus on a controller by its (case-sensitive) name — the fallback when a
        // stored bus id no longer resolves.  Null when the controller or name is missing.
        private static ControllerBus findBusByName(Controller controller, string busName)
        {
            if (controller?.Buses == null || string.IsNullOrEmpty(busName))
            {
                return null;
            }
            foreach (ControllerBus b in controller.Buses)
            {
                if (b.Name == busName)
                {
                    return b;
                }
            }
            return null;
        }

        // Display name for a bus pin: "<bus>.<pin name>" when named, else "<bus>[index]".
        private static string busPinName(ControllerBus bus, int pinIndex)
        {
            if (pinIndex < 0 || pinIndex >= ControllerBus.PinCount)
            {
                return bus.Name + "[?]";
            }
            return string.IsNullOrEmpty(bus.PinNames[pinIndex])
                ? bus.Name + "[" + pinIndex + "]"
                : bus.Name + "." + bus.PinNames[pinIndex];
        }

        private static LocStrFormatted pinTypeLabel(ControllerBus.BusPinType type)
        {
            switch (type)
            {
                case ControllerBus.BusPinType.Output: return "output".ToDoLoc();
                case ControllerBus.BusPinType.Controller: return "controller".ToDoLoc();
                case ControllerBus.BusPinType.Plc: return "PLC".ToDoLoc();
                default: return "?".ToDoLoc();
            }
        }

        private static LocStrFormatted pinTypeTooltip(ControllerBus.BusPinType type)
        {
            switch (type)
            {
                case ControllerBus.BusPinType.Output: return ("When connected from a module output, is allowed to be"
					+ " read by other controllers in close proximity; and PLC module can access it too, see help of"
					+ " the PLC module").ToDoLoc();
                case ControllerBus.BusPinType.Controller: return ("When connected to a module input, is allowed to"
					+ " read value from another selected controller and its bus or output module").ToDoLoc();
                case ControllerBus.BusPinType.Plc: return ("When connected to a module input, is allowed to read value"
					+ " from a PLC module, value must be set in PLC, see help of the PLC module").ToDoLoc();
                default: return "?".ToDoLoc();
            }
        }

        private static ControllerBus.BusPinType nextPinType(ControllerBus.BusPinType type)
        {
            switch (type)
            {
                case ControllerBus.BusPinType.Output: return ControllerBus.BusPinType.Controller;
                case ControllerBus.BusPinType.Controller: return ControllerBus.BusPinType.Plc;
                case ControllerBus.BusPinType.Plc: return ControllerBus.BusPinType.Output;
                default: return ControllerBus.BusPinType.Output;
            }
        }
    }
}
