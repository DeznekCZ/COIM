using Mafi.Core.Syncers;
using System.Linq;
using Mafi;
using Mafi.Core;
using System;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.Ui.Library;
using Mafi.Localization;
using System.Collections.Generic;
using static Mafi.Unity.Assets.Unity;
using System.Globalization;
using Mafi.Core.Research;
using Mafi.Unity.Ui.Hud;

namespace ProgramableNetwork.Ui;

public partial class ControllerView
{
	public class ModuleView : Panel
	{
		private readonly Module m_module;
		private readonly ControllerView m_controller;

		public Module Module => m_module;

		public ModuleView(Module module, ControllerView controllerView, UiContext uiContext, bool preview, Action refresh)
			: base()
		{
			this.Margin(Px.Zero);
			this.Body.Padding(Px.Zero);
			this.Body.Margin(Px.Zero);
			this.Body.Gap(Px.Zero);

			this.m_module = module;
			this.m_controller = controllerView;
			string name = "moduleView_" + module.Id;
			var updater = UpdaterBuilder.Start();
			int baseWidth = module.Layout.GetBaseWidth(module);
			int width = module.Layout.GetWidth(module);
			bool displaysExists = module.Prototype.Displays.Count > 0;
			bool isExtensible = !preview && (module.Prototype.MaxInputExtensions > 0
				|| module.Prototype.MaxOutputExtensions > 0
				|| module.Prototype.MaxDisplayExtensions > 0);
			// Prototype-swap: a module whose proto belongs to a ModuleSwapGroup with more than
			// one member gets a swap button in its bottom-left corner (see swapGroupFor cache).
			ModuleSwapGroup swapGroup = preview ? null : swapGroupFor(module);
			bool isSwappable = swapGroup != null && swapGroup.Members.Count > 1;

			this.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4);
			this.Class(Cls.panel);
			// Outer panel resizes when any of the three extension dimensions change —
			// recomputes off the live layout so dynamic-width prototypes follow along.
			this.Observe(() => module.InputExtensionCount)
				.Observe(() => module.OutputExtensionCount)
				.Observe(() => module.DisplayExtensionCount)
				.Do((iE, oE, dE) => this.Size(module.Layout.GetWidth(module) * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4));

			// Add Input panel — full module width so extension pins land at the right
			// columns.  Children rebuilt in place when ext counts change so the static
			// pin objects stay alive (their Observe-driven cable-color subscriptions
			// keep working) until a redraw replaces them.
			Row inputsPanel = new Row()
				.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
				.Background(ColorRgba.DarkGreen)
				.AlignItemsEnd();
			AddInputs(uiContext, inputsPanel, module, preview, refresh);
			BodyAdd(inputsPanel);
			inputsPanel.Observe(() => module.InputExtensionCount)
					   .Observe(() => module.OutputExtensionCount)
					   .Observe(() => module.DisplayExtensionCount)
					   .Do((iE, oE, dE) => {
						   inputsPanel.Clear();
						   inputsPanel.Width(module.Layout.GetWidth(module) * Sizes.BLOCK_SIZE);
						   AddInputs(uiContext, inputsPanel, module, preview, refresh);
					   });

			// Full-width config button — spans the entire module including extension
			// cells; resizes when extensions are added/removed via the fieldsRow
			// observer below.  Text stays centred in the full-width button.
			ButtonText fieldsPanel = new ButtonText(module.Prototype.Symbol.AsLoc());
			fieldsPanel.TextOverflow(TextOverflow.Clip);
			fieldsPanel.TextAlign(TextAlignment.CenterMiddle);
			// Swap Mafi.Button's default Clickable manipulator for a plain ClickEvent
			// listener.  The manipulator captures the pointer on plain LMB and calls
			// StopImmediatePropagation on PointerUp before our modifier-aware handler
			// below could fire — that's why a plain LMB on a placed module wouldn't
			// open the Edit dialog while RMB / Alt+LMB / Shift+LMB worked correctly
			// (those don't activate the manipulator's LMB-only activator and reach our
			// handler unimpeded).  MoveClickToAnEvent leaves PointerUp untouched.
			fieldsPanel.MoveClickToAnEvent();
			if (!preview)
			{
				fieldsPanel.OnMouseEnterLeave(
						() => {
							m_controller.m_controller.HoveredModuleGraphic = module;
							m_controller.AddPreviewHighlight(module);
						},
						() => {
							if (m_controller.m_controller.HoveredModuleGraphic == module) {
								m_controller.m_controller.HoveredModuleGraphic = null;
							}
							m_controller.ClearPreviewHighlight();
						}
					);
				// Modifier-aware mouse dispatch (replaces the old Add/Move/Edit modes):
				//   LMB              → open the Edit dialog
				//   ALT + LMB        → toggle pick-up for move (drop happens on a slot via Alt+LMB)
				//   SHIFT + LMB      → copy this module to the "last created" clipboard
				//   RMB              → confirm-remove floater
				//   SHIFT + RMB      → remove without confirmation
				//
				// Single PointerUpEvent listener handles both buttons and reads
				// modifier flags from the event itself.  PointerUpEvent (UIElements'
				// modern pointer pipeline) is independent of MouseUpEvent — Mafi's
				// tooltip floater consumes MouseUpEvent on dismiss, but
				// PointerUpEvent bubbles through our listener untouched.  Inner
				// label/visual children of ButtonText forward pointer events, so
				// we walk up to fieldsPanel.RootElement to confirm the click
				// landed on this button rather than on the side pin/display
				// children that share the same outer panel.
				fieldsPanel.RegisterCallback<UnityEngine.UIElements.PointerUpEvent>(evt =>
				{
					var inspector = m_controller.m_controller;
					switch (evt.button)
					{
						case 0: // left
							if (evt.altKey)
							{
								// Toggle pick-up.  A second Alt+LMB on the same module
								// cancels the move; clicking a different module while
								// one is already picked just swaps the picked target.
								if (inspector.PickedUpModule == null)
								{
									inspector.PickedUpModule = module;
								}
								else if (inspector.PickedUpModule.Id == module.Id)
								{
									inspector.PickedUpModule = null;
								}
								else
								{
									inspector.PickedUpModule = module;
								}
							}
							else if (evt.shiftKey)
							{
								// Adopt this module's prototype + data as the "last
								// created" template so a Shift+LMB on a free slot
								// stamps a copy of it (no auto-placement here).
								ControllerView.m_lastCreated = module;
							}
							else
							{
								// Same orchestration shape as the empty-slot Add picker
								// (OpenAddPickerAt): a thin click dispatcher that hands
								// off to a method on ControllerView, so the dialog open
								// path matches the proven flow.  ControllerView guards
								// re-entry with m_settingsDialogInAction.
								m_controller.OpenSettingsAt(module, fieldsPanel);
							}
							evt.StopPropagation();
							break;
						case 1: // right
							if (evt.shiftKey)
							{
								// Power-user shortcut: bypass the confirmation floater.
								m_controller.RemoveModule(module);
							}
							else
							{
								openConfirmRemoveDialog(module, fieldsPanel);
							}
							evt.StopPropagation();
							break;
					}
				});

				// Cable-style highlight when this module is hovered from the connections panel,
				// when it has been picked up in Move mode, or — while ALL links are shown
				// (connection-button hover) — for modules that own an entity field, so the
				// tint points at the modules actually driving a world connection.
				fieldsPanel.Observe(() => m_controller.m_controller.HighlightedFromSidePanel)
					.Observe(() => m_controller.m_controller.PickedUpModule)
					.Observe(() => m_controller.m_controller.m_showsLinks)
					.Do((highlighted, picked, showsLinks) =>
					{
						bool isHovered = highlighted != null && highlighted.Id == module.Id;
						bool isPicked = picked != null && picked.Id == module.Id;
						bool isLinked = showsLinks && moduleHasEntityField(module);
						ColorRgba color;
						if (isPicked) {
							color = ColorRgba.Gold;
						} else if (isHovered || isLinked) {
							color = ColorRgba.CornflowerBlue;
						} else {
							color = ColorRgba.CornflowerBlue.SetA(0);
						}
						fieldsPanel.Border(all: 2.px(), radius: 4, color: color);
					});

				if (displaysExists)
				{
					this.Observe(() => DateTime.Now)
						.Do((time) => module.Prototype.DisplayUpdate(module));
				}
			}
			// fieldsPanel spans the full module width (including extension cells) and
			// resizes dynamically.  No separate filler needed — the button itself is
			// the full clickable surface.
			Px fieldsHeight = displaysExists ? Sizes.BLOCK_SIZE : (Sizes.BLOCK_SIZE * 2);
			fieldsPanel.Size(width * Sizes.BLOCK_SIZE, fieldsHeight);
			Row fieldsRow = new Row().Size(width * Sizes.BLOCK_SIZE, fieldsHeight);
			fieldsRow.Add(fieldsPanel);
			if (!preview)
			{
				// Status / error / warning shown via a tiny red-tinted badge in
				// fieldsPanel's bottom-left.  The diagnostic text is on the badge's
				// OWN Mafi tooltip (Mafi tooltip on a non-clickable Icon doesn't
				// conflict with anything), so the placed module's primary tooltip
				// stays free for the keybind hint floater attached below.
				Icon errorIcon = new Icon(UserInterface.General.Warning128_png)
					.Size(16.px(), 16.px());
				// Nudge the badge one cell right when the swap button occupies the corner so
				// the two 16-px widgets sit side by side instead of overlapping.
				errorIcon.AbsolutePosition(bottom: 0.px(), left: (isSwappable ? 16 : 0).px());
				errorIcon.SetVisible(false);
				this.Observe(() => module.Status)
					.Observe(() => module.Error)
					.Observe(() => module.Warning)
					.Do((status, text, warn) =>
					{
						bool isError = status == ModuleStatus.Error;
						// Aggregate the values of fields that opted into the tooltip via
						// AddXxxField(showInTooltip: true). Lines are appended after any
						// error/status text so the existing diagnostics stay first.
						string tooltipText = text ?? "";
						string aggregated = BuildFieldTooltip(module);
						if (!string.IsNullOrEmpty(aggregated))
						{
							tooltipText = string.IsNullOrEmpty(tooltipText)
								? aggregated
								: tooltipText + "\n\n" + aggregated;
						}
						bool hasText = !string.IsNullOrEmpty(tooltipText);
						errorIcon.SetVisible(hasText && (isError || warn));
						errorIcon.Tooltip(hasText ? tooltipText.AsLoc() : LocStrFormatted.Empty,
							enabled: hasText, isError: isError);
						errorIcon.Color(isError ? ColorRgba.Red : ColorRgba.Orange);

						if (status == ModuleStatus.Running) {
							fieldsPanel.Class(Cls.btn_general);
							fieldsPanel.ClassRemove(Cls.btn_primary);
						} else {
							fieldsPanel.Class(Cls.btn_primary);
							fieldsPanel.ClassRemove(Cls.btn_general);
						}
					});

				// Bottom-left status badge — same 16-px footprint as the +/- extension
				// buttons.  Visible only when there's diagnostic text to show.
				fieldsRow.Add(errorIcon);

				// Bottom-left swap button — offered only for modules whose prototype is in a
				// ModuleSwapGroup.  A "⇄" glyph (same 16-px footprint / styling as the +/-
				// extension buttons) opens a small picker of compatible prototypes; picking one
				// replaces this module's prototype in place, keeping cables + field values.
				// Added as a sibling of fieldsPanel (not a child), so a click on it never falls
				// through to fieldsPanel's Edit-dialog handler.
				if (isSwappable)
				{
					ButtonText swapBtn = new ButtonText("⇄".AsLoc())
						.Size(16.px(), 16.px())
						.MinWidth(16.px())
						.MaxWidth(16.px())
						.Padding(Px.Zero)
						.Margin(Px.Zero)
						.TextAlign(TextAlignment.CenterMiddle)
						.FontSize(10);
					swapBtn.AbsolutePosition(bottom: 0.px(), left: 0.px());
					swapBtn.Tooltip("Swap operation".ToDoLoc());
					swapBtn.OnClick(() => openSwapPicker(module, swapBtn, swapGroup, uiContext));
					fieldsRow.Add(swapBtn);
				}

				// Keybind hint floater — Mafi-style hover tooltip listing the LMB /
				// Alt+LMB / Shift+LMB / RMB / Shift+RMB shortcuts.  Now that
				// MoveClickToAnEvent() above stripped the click-eating Clickable
				// manipulator, attaching a Floater here no longer interferes with the
				// click path: Floater registers a tooltip-style listener that fires on
				// hover only.  Same KeyUi/icon row layout the base game uses for its
				// own keyboard hints (and the slot button's AddHelper).  Gated on the
				// inspector's m_showHints checkbox so the entire hint system can be
				// silenced from one place.
				fieldsPanel.Floater(() => m_controller.Inspector.m_showHints
					? buildModuleKeybindHelp(fieldsPanel).SomeOption<UiComponent>()
					: Option<UiComponent>.None);
			}
			fieldsRow.Observe(() => module.InputExtensionCount)
					 .Observe(() => module.OutputExtensionCount)
					 .Observe(() => module.DisplayExtensionCount)
					 .Do((iE, oE, dE) => {
						 int newW = module.Layout.GetWidth(module);
						 fieldsRow.Width(newW * Sizes.BLOCK_SIZE);
						 fieldsPanel.Width(newW * Sizes.BLOCK_SIZE);
					 });
			BodyAdd(fieldsRow);

			if (displaysExists) {
				// Displays panel grows by:
				//   - DisplayExtensionCount cells (rightmost display widget stretch), and
				//   - one cell per per-extension display widget linked to a pin side.
				// The filler covers any remaining module width claimed by input/output
				// extensions that aren't already absorbed by display growth.
				int dispExtNow() => System.Math.Min(module.DisplayExtensionCount, module.Prototype.MaxDisplayExtensions);
				int linkedActive() => module.Prototype.ExtensionDisplaysLinkedSide switch {
					ExtensionSide.Input => System.Math.Min(module.InputExtensionCount, module.Prototype.MaxInputExtensions),
					ExtensionSide.Output => System.Math.Min(module.OutputExtensionCount, module.Prototype.MaxOutputExtensions),
					_ => 0,
				};
				int extDispCount() => System.Math.Min(linkedActive(), module.Prototype.ExtensionDisplays.Count);
				// Mirror the absorption logic in AddDisplays: cells claimed by the linked
				// pin side but not covered by per-extension displays widen the last
				// static display, so the panel needs to grow to include them.  Only
				// kicks in when the proto declared no extension displays at all.
				int linkedLeftover() => module.Prototype.ExtensionDisplays.Count == 0
					? System.Math.Max(0, linkedActive())
					: 0;
				int displayPanelCells() => baseWidth + dispExtNow() + extDispCount() + linkedLeftover();

				Row displaysRow = new Row().Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
				Row displaysPanel = new Row()
					.Size(displayPanelCells() * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
					.Background(ColorRgba.DarkDarkGray);
				AddDisplays(uiContext, displaysPanel, module, preview, refresh);
				displaysRow.Add(displaysPanel);
				UiComponent displaysFiller = new UiComponent()
					.Width((width - displayPanelCells()) * Sizes.BLOCK_SIZE)
					.Height(Sizes.BLOCK_SIZE);
				displaysRow.Add(displaysFiller);
				displaysRow.Observe(() => module.InputExtensionCount)
						   .Observe(() => module.OutputExtensionCount)
						   .Observe(() => module.DisplayExtensionCount)
						   .Do((iE, oE, dE) => {
							   int newW = module.Layout.GetWidth(module);
							   int panelCells = displayPanelCells();
							   displaysRow.Width(newW * Sizes.BLOCK_SIZE);
							   displaysPanel.Clear();
							   displaysPanel.Width(panelCells * Sizes.BLOCK_SIZE);
							   AddDisplays(uiContext, displaysPanel, module, preview, refresh);
							   displaysFiller.Width((newW - panelCells) * Sizes.BLOCK_SIZE);
						   });
				BodyAdd(displaysRow);

				// Add display synchronization every 200 ms
				Element.schedule
					.Execute(() => module.Prototype.DisplayUpdate(module))
					.Every(100);
			}

			// Add Ouptut panel — same observable rebuild as inputs.
			Row outputsPanel = new Row()
				.Class(Cls.group)
				.Size(width * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
				.Background(ColorRgba.DarkRed)
				.AlignItemsEnd();
			AddOutputs(uiContext, outputsPanel, module, preview, refresh);
			BodyAdd(outputsPanel);
			outputsPanel.Observe(() => module.InputExtensionCount)
						.Observe(() => module.OutputExtensionCount)
						.Observe(() => module.DisplayExtensionCount)
						.Do((iE, oE, dE) => {
							outputsPanel.Clear();
							outputsPanel.Width(module.Layout.GetWidth(module) * Sizes.BLOCK_SIZE);
							AddOutputs(uiContext, outputsPanel, module, preview, refresh);
						});

			// Inline edge "+/-" — extensible modules get a tiny vertical pair of icon
			// buttons floating at the right edge of the panel so the player can grow /
			// shrink pin counts without opening the inspector.  Hidden in preview mode
			// (no controller bound for the cmd to target).  The buttons cover one side
			// (input or output) per pair; if both sides are extensible, two pairs are
			// shown stacked.
			if (isExtensible)
			{
				AddEdgeExtensionButtons(uiContext, module);
			}
		}

		// "+" sits flush against the right end of the affected row ("pin bar" for
		// input/output sides, "display row" for display side).  "-" parks directly
		// adjacent on the main-part side.
		private void AddEdgeExtensionButtons(UiContext uiContext, Module module)
		{
			if (module.Prototype.MaxInputExtensions > 0) {
				addEdgeButtonPair(uiContext, module, ExtensionSide.Input);
			}
			if (module.Prototype.MaxOutputExtensions > 0) {
				addEdgeButtonPair(uiContext, module, ExtensionSide.Output);
			}
			if (module.Prototype.MaxDisplayExtensions > 0) {
				addEdgeButtonPair(uiContext, module, ExtensionSide.Display);
			}
		}

		private void addEdgeButtonPair(UiContext uiContext, Module module, ExtensionSide side)
		{
			int max = side switch {
				ExtensionSide.Input => module.Prototype.MaxInputExtensions,
				ExtensionSide.Output => module.Prototype.MaxOutputExtensions,
				ExtensionSide.Display => module.Prototype.MaxDisplayExtensions,
				_ => 0,
			};
			int currentCount() => side switch {
				ExtensionSide.Input => module.InputExtensionCount,
				ExtensionSide.Output => module.OutputExtensionCount,
				ExtensionSide.Display => module.DisplayExtensionCount,
				_ => 0,
			};
			// A side only needs a free grid cell when it is currently THE limiter of
			// the module's width (i.e., its count is >= the other two sides').  When
			// another side is already wider, growing this side fits in the existing
			// footprint and never requires a new cell — so e.g. you can keep adding
			// outputs after inputs hit max, as long as outputs stay <= inputs.
			bool sideIsLimiter() => side switch {
				ExtensionSide.Input   => module.InputExtensionCount   >= System.Math.Max(module.OutputExtensionCount, module.DisplayExtensionCount),
				ExtensionSide.Output  => module.OutputExtensionCount  >= System.Math.Max(module.InputExtensionCount,  module.DisplayExtensionCount),
				ExtensionSide.Display => module.DisplayExtensionCount >= System.Math.Max(module.InputExtensionCount,  module.OutputExtensionCount),
				_ => true,
			};
			bool canGrow() => !sideIsLimiter() || m_controller.CanExtendModule(module);

			// Each button 16×16 so the click target is comfortable; the pair sits in
			// a 32×16 footprint enforced by the wrapper Row below.
			const int BTN_W = 16;
			const int BTN_H = 16;
			// Per-side Y choices:
			//   Input  → centered on y=BS   (boundary between input bar and main).
			//   Output → centered on y=3*BS (boundary between main and output bar).
			//   Display→ just above the display row so it never overlaps the displays.
			Px pairTop()
			{
				return side switch {
					ExtensionSide.Input => Sizes.BLOCK_SIZE - (BTN_H / 2).px(),
					ExtensionSide.Output => Sizes.BLOCK_SIZE * 3 - (BTN_H / 2).px(),
					ExtensionSide.Display => Sizes.BLOCK_SIZE * 2 - BTN_H.px(),
					_ => 0.px(),
				};
			}
			string addLabel = side switch {
				ExtensionSide.Input => "Add input pin",
				ExtensionSide.Output => "Add output pin",
				ExtensionSide.Display => "Widen display",
				_ => "Extend",
			};
			string removeLabel = side switch {
				ExtensionSide.Input => "Remove input pin (drops cable)",
				ExtensionSide.Output => "Remove output pin (drops cable)",
				ExtensionSide.Display => "Shrink display",
				_ => "Shrink",
			};

			// Wrap both buttons in a fixed-width Row so flex layout enforces the
			// 12-px-each footprint — using AbsolutePosition on each button alone let
			// ButtonText's intrinsic min-width win, and the two ended up overlapping.
			// IgnoreInputPicking on the wrapper itself: the Row sits ON TOP of
			// fieldsPanel (absolute positioning straddles the input/output bar
			// boundary so the pair visually peeks over fieldsPanel by 8 px on each
			// side) and would otherwise intercept clicks meant for the centre symbol
			// button.  The +/- child buttons keep their default pickingMode and
			// still receive their own clicks.
			Row pairRow = new Row().Size((BTN_W * 2).px(), BTN_H.px());
			pairRow.AbsolutePosition(top: pairTop(), right: 0.px());
			pairRow.IgnoreInputPicking();
			this.Add(pairRow);

			// "-" sits on the LEFT half of the pair, "+" on the RIGHT — mirrors the
			// previous "right=BTN_W / right=0" arrangement.
			ButtonText decBtn = new ButtonText("-".AsLoc())
				.Size(BTN_W.px(), BTN_H.px())
				.MinWidth(BTN_W.px())
				.MaxWidth(BTN_W.px())
				.Padding(Px.Zero)
				.Margin(Px.Zero)
				.TextAlign(TextAlignment.CenterMiddle)
				.FontSize(10);
			decBtn.Tooltip(removeLabel.ToDoLoc());
			decBtn.OnClick(() =>
			{
				int next = currentCount() - 1;
				if (next < 0) {
					uiContext.AudioDb.InvalidOp(true).Play();
					return;
				}
				uiContext.InputScheduler.ScheduleInputCmd(
					new ModuleSetExtensionCountCmd(module.Controller.Id, module.Id, side, next));
			});
			decBtn.ObserveEnabled(() => currentCount() > 0);
			decBtn.VisibleForRender(currentCount() > 0);
			decBtn.Observe(() => module.InputExtensionCount)
				  .Observe(() => module.OutputExtensionCount)
				  .Observe(() => module.DisplayExtensionCount)
				  .Do((iE, oE, dE) => decBtn.VisibleForRender(currentCount() > 0));
			pairRow.Add(decBtn);

			ButtonText incBtn = new ButtonText("+".AsLoc())
				.Size(BTN_W.px(), BTN_H.px())
				.MinWidth(BTN_W.px())
				.MaxWidth(BTN_W.px())
				.Padding(Px.Zero)
				.Margin(Px.Zero)
				.TextAlign(TextAlignment.CenterMiddle)
				.FontSize(10);
			incBtn.Tooltip(addLabel.ToDoLoc());
			incBtn.OnClick(() =>
			{
				int next = currentCount() + 1;
				if (next > max || !canGrow()) {
					uiContext.AudioDb.InvalidOp(true).Play();
					return;
				}
				uiContext.InputScheduler.ScheduleInputCmd(
					new ModuleSetExtensionCountCmd(module.Controller.Id, module.Id, side, next));
			});
			incBtn.ObserveEnabled(() => currentCount() < max && canGrow());
			pairRow.Add(incBtn);
		}

		// Renders the "name: value" line per ShowInTooltip-flagged field, joined by newlines.
		// Returns empty string when no fields opt in.  Fields with empty value strings are
		// skipped so we don't show "X:" on its own.
		private static string BuildFieldTooltip(Module module)
		{
			if (module?.Prototype?.Fields == null) {
				return "";
			}
			System.Text.StringBuilder sb = null;
			foreach (IField field in module.Prototype.Fields)
			{
				if (!field.ShowInTooltip) {
					continue;
				}
				string value = field.GetTooltipValue(module);
				if (string.IsNullOrEmpty(value)) {
					continue;
				}
				if (sb == null) {
					sb = new System.Text.StringBuilder();
				} else {
					sb.Append('\n');
				}
				sb.Append(field.Name.TranslatedString);
				sb.Append(": ");
				sb.Append(value);
			}
			return sb?.ToString() ?? "";
		}

		private void AddInputs(UiContext uiContext, Row inputsPanel, Module module, bool preview, Action refresh)
		{
			// Layout: [inner filler ── right-aligns statics in their original baseWidth]
			//         [static input pins from the prototype]
			//         [active extension input pins]
			//         [trailing input pins ── pinned to end of THIS row's active area]
			//         [outer filler ── present when the OTHER row's extensions widen us]
			// This keeps the static pin columns identical to the pre-extension layout.
			// Trailing pins follow the input row's own extension count only — display
			// or output extensions don't shift them; the gap is absorbed by outerFiller.
			var staticInputs = module.Prototype.Inputs;
			var trailingInputs = module.Prototype.InputsTrailing ?? (System.Collections.Generic.IList<ModuleConnectorProto>)System.Array.Empty<ModuleConnectorProto>();
			int totalWidth = module.Layout.GetWidth(module);
			int baseWidth = module.Layout.GetBaseWidth(module);
			int extCount = System.Math.Min(module.InputExtensionCount, module.Prototype.MaxInputExtensions);
			int innerFiller = System.Math.Max(0, baseWidth - staticInputs.Count - trailingInputs.Count);
			int outerFiller = System.Math.Max(0, totalWidth - baseWidth - extCount);

			if (innerFiller > 0)
			{
				inputsPanel.AddAndReturn(new UiComponent())
					.Width(innerFiller * Sizes.BLOCK_SIZE)
					.Height(Sizes.BLOCK_SIZE);
			}
			int staticAndExtPins = staticInputs.Count + extCount;
			int totalPins = staticAndExtPins + trailingInputs.Count;
			for (int i = 0; i < totalPins; i++)
			{
				ModuleConnectorProto input;
				if (i < staticInputs.Count) {
					input = staticInputs[i];
				} else if (i < staticAndExtPins) {
					input = module.Prototype.InputExtensions[i - staticInputs.Count];
				} else {
					input = trailingInputs[i - staticAndExtPins];
				}
				bool isConnected = module.InputModules.ContainsKey(input.Id);

				PortPinButton btn = new PortPinButton(PortPinButton.PortKind.Input, isConnected)
					.Tooltip(new LocStrFormatted((input.Name.Name + ": " + input.Name.DescShort).TrimEnd(':', ' ')));
				// Paint the dot with the matching cable's hue from the persistent
				// (source, output) colour pool so it stays consistent across redraws.
				if (isConnected && module.InputModules.TryGetValue(input.Id, out var initConn))
				{
					btn.DotColor(m_controller.GetOrCreateCableColor(initConn.ModuleId, initConn.OutputId));
				}
				inputsPanel.Add(btn);
				// Per-pin reactive: watch the connection's IDENTITY (source mod + output
				// id), not just its existence — that way a rewire (output replaced
				// without disconnect) also fires Do.  Colour comes from the persistent
				// (sourceId, outputId) pool so it stays consistent with the cable's
				// colour regardless of when the redraw runs.
				{
					string capturedId = input.Id;
					btn.Observe(() => module.InputModules.TryGetValue(capturedId, out var c)
							? c.ModuleId.ToString() + "." + (c.OutputId ?? "")
							: "")
					   .Do(connKey => {
						   bool connected = connKey.Length > 0;
						   btn.Connected(connected);
						   if (connected && module.InputModules.TryGetValue(capturedId, out var c)) {
							   btn.DotColor(m_controller.GetOrCreateCableColor(c.ModuleId, c.OutputId));
						   }
					   });
				}

				if (!preview)
				{
					btn .OnRightClick(() =>
						{
							if (module.InputModules.ContainsKey(input.Id))
							{
								// Wait for the disconnect to actually apply before refreshing
								// the UI — otherwise the redraw runs against stale state.
								uiContext.InputScheduler.ScheduleAndOnApplied(
									new ModuleSetInputConnectionCmd(
										module.Controller.Id, module.Id, input.Id, 0L, ""),
									btn, refresh);
							}
							else
							{
								uiContext.AudioDb.InvalidOp(true).Play();
							}
						})
						.OnClick(() =>
						{
							var inspector = m_controller.m_controller;
							if (inspector.OutputConnection == null)
							{
								uiContext.AudioDb.InvalidOp(true).Play();
								return;
							}
							ModuleConnector held = inspector.OutputConnection;
							// Left-click on an input that's ALREADY carrying the held
							// cable disconnects it — same shape as right-click but
							// without putting the held output down, so the user can
							// keep rerouting without an extra click.
							if (module.InputModules.TryGetValue(input.Id, out var existing)
								&& existing.Equals(held))
							{
								uiContext.InputScheduler.ScheduleAndOnApplied(
									new ModuleSetInputConnectionCmd(
										module.Controller.Id, module.Id, input.Id, 0L, ""),
									btn, refresh);
								return;
							}
							// A bus INPUT pin is itself fed by a module output — it must never
							// drive a module input.  Reject that wiring (only Plc / Controller
							// / NetworkRead pins, which hold a readable value, may feed inputs).
							if (held.IsBus
								&& held.TryGetPinIndex(out int heldPin)
								&& module.Controller?.GetBusById(held.ModuleId) is ControllerBus heldBus
								&& heldPin >= 0 && heldPin < ControllerBus.PinCount
								&& heldBus.PinTypes[heldPin] == ControllerBus.BusPinType.Output)
							{
								uiContext.AudioDb.InvalidOp(true).Play();
								return;
							}
							// Otherwise attach (overwrites any other prior source).  Carry the
							// connector kind so a held BUS pin is stored as a bus connection
							// (else the executor would build a module-kind connector and the
							// signal plan would prune it — no module has the bus's id).
							uiContext.InputScheduler.ScheduleAndOnApplied(
								new ModuleSetInputConnectionCmd(
									module.Controller.Id, module.Id, input.Id, held.ModuleId, held.OutputId, held.IsBus),
								btn, refresh);
						})
						// Inputs are "open" (enlarged circle) while an output is picked,
						// signalling that a click on this pin would complete the connection.
						// Outputs never enter the open state — they stay closed and look
						// identical to idle inputs.
						.Observe(() => m_controller.m_controller.OutputConnection != null)
						.Do(open =>
						{
							btn.Open(open);
						});

					btn.OnMouseEnterLeave(
						() => { m_controller.m_controller.m_higlightedInput = new ModuleConnector(module.Id, input.Id); },
						() => { m_controller.m_controller.m_higlightedInput = null; }
					);
				}
			}
			// outerFiller is always appended last — past the trailings — so when
			// another row's extensions widen the module past this row's own
			// extension count, the trailings stay anchored to the end of the
			// active input area and the empty cells appear to the right of them.
			if (outerFiller > 0)
			{
				inputsPanel.AddAndReturn(new UiComponent())
					.Width(outerFiller * Sizes.BLOCK_SIZE)
					.Height(Sizes.BLOCK_SIZE);
			}
		}

		private void AddOutputs(UiContext uiContext, Row inputsPanel, Module module, bool preview, Action refresh)
		{
			// Symmetric to AddInputs: inner filler keeps statics right-aligned in their
			// original baseWidth, then static pins, then active extension pins, then an
			// outer filler when the input row's extensions widen us beyond our own.
			var staticOutputs = module.Prototype.Outputs;
			int totalWidth = module.Layout.GetWidth(module);
			int baseWidth = module.Layout.GetBaseWidth(module);
			int extCount = System.Math.Min(module.OutputExtensionCount, module.Prototype.MaxOutputExtensions);
			int innerFiller = System.Math.Max(0, baseWidth - staticOutputs.Count);
			int outerFiller = System.Math.Max(0, totalWidth - baseWidth - extCount);

			if (innerFiller > 0)
			{
				inputsPanel.AddAndReturn(new UiComponent())
					.Width(innerFiller * Sizes.BLOCK_SIZE)
					.Height(Sizes.BLOCK_SIZE);
			}
			int totalPins = staticOutputs.Count + extCount;
			for (int i = 0; i < totalPins; i++)
			{
				var output = i < staticOutputs.Count
					? staticOutputs[i]
					: module.Prototype.OutputExtensions[i - staticOutputs.Count];
				// Live check — used both for the snapshot (initial state) and in event
				// handlers so the cached ModuleView never acts on a stale bool.
				string capturedOutId = output.Id;
				bool isConnected = outputIsConnected(module, capturedOutId);

				PortPinButton btn = new PortPinButton(PortPinButton.PortKind.Output, isConnected)
					.Tooltip(new LocStrFormatted((output.Name.Name + ": " + output.Name.DescShort).TrimEnd(':', ' ')));
				// Same hue as the cable(s) leaving this output — pulled from the
				// persistent (sourceId, outputId) colour pool.
				if (isConnected)
				{
					btn.DotColor(m_controller.GetOrCreateCableColor(module.Id, capturedOutId));
				}
				inputsPanel.Add(btn);
				// Per-pin reactive: refresh dot connected/colour when any module's
				// connection to this output changes.  Colour comes from the persistent
				// (sourceId, outputId) pool — stable across redraws.
				btn.Observe(() => outputIsConnected(module, capturedOutId))
				   .Do(connected => {
					   btn.Connected(connected);
					   if (connected) {
						   btn.DotColor(m_controller.GetOrCreateCableColor(module.Id, capturedOutId));
					   }
				   });

				if (!preview)
				{
					btn .OnRightClick(() =>
						{
							if (!outputIsConnected(module, capturedOutId))
							{
								uiContext.AudioDb.InvalidOp(true).Play();
								return;
							}

							// Disconnect the first input that consumes THIS specific output.
							// Previously only checked ModuleId, missing the OutputId filter —
							// modules with multiple outputs would disconnect the wrong cable.
							foreach (var target in m_controller.Entity.Modules)
							{
								foreach (var connection in target.InputModules)
								{
									if (connection.Value.ModuleId == module.Id
										&& connection.Value.OutputId == capturedOutId)
									{
										uiContext.InputScheduler.ScheduleAndOnApplied(
											new ModuleSetInputConnectionCmd(
												target.Controller.Id, target.Id, connection.Key, 0L, ""),
											btn, refresh);
										return;
									}
								}
							}
						})
						.OnClick(() =>
						{
							if (m_controller.m_controller.OutputConnection != null
								&& m_controller.m_controller.OutputConnection.ModuleId == module.Id
								&& m_controller.m_controller.OutputConnection.OutputId == output.Id)
							{
								m_controller.m_controller.OutputConnection = null;
							}
							else
							{
								m_controller.m_controller.OutputConnection = new ModuleConnector(module.Id, output.Id);
							}
						});
						btn.OnMouseEnterLeave(
							() => { m_controller.m_controller.m_higlightedOutput = new ModuleConnector(module.Id, output.Id); },
							() => { m_controller.m_controller.m_higlightedOutput = null; }
						);
						// Enlarge THIS output pin while it's the one picked for connection
						// (mirrors the inputs' "open" state when an output is held), so the
						// player can see which output the next input-click will wire to.
						btn.Observe(() =>
								m_controller.m_controller.OutputConnection != null
								&& m_controller.m_controller.OutputConnection.ModuleId == module.Id
								&& m_controller.m_controller.OutputConnection.OutputId == capturedOutId)
							.Do(open => btn.Open(open));
				}
			}
			if (outerFiller > 0)
			{
				inputsPanel.AddAndReturn(new UiComponent())
					.Width(outerFiller * Sizes.BLOCK_SIZE)
					.Height(Sizes.BLOCK_SIZE);
			}
		}

		// True when the module's prototype declares at least one entity field (i.e. it can
		// be wired to a world entity).  Used to limit the "links shown" settings-button
		// tint to modules that actually own a world connection.
		private static bool moduleHasEntityField(Module module)
		{
			if (module?.Prototype == null) {
				return false;
			}
			foreach (IField field in module.Prototype.Fields) {
				if (field is EntityField) {
					return true;
				}
			}
			return false;
		}

		private static bool outputIsConnected(Module module, string capturedOutId)
		{
			if (module.Controller is null) { return false; }
			foreach (var inputModule in module.Controller.Modules)
			{
				foreach (var connection in inputModule.InputModules)
				{
					if (connection.Value.ModuleId == module.Id && connection.Value.OutputId == capturedOutId)
					{
						return true;
					}
				}
			}
			// Also connected if this output feeds a variable-bus Input pin — that wiring
			// lives on the bus (PinSources), not in any module's InputModules, so the
			// loop above would otherwise miss it and the pin would look unconnected.
			if (module.Controller.Buses != null)
			{
				foreach (ControllerBus bus in module.Controller.Buses)
				{
					for (int i = 0; i < ControllerBus.PinCount; i++)
					{
						BusPinSource s = bus.PinSources[i];
						if (s != null && s.Kind == BusPinSource.SourceKind.LocalModule
							&& s.ModuleId == module.Id && s.OutputId == capturedOutId)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		private void AddDisplays(UiContext uiContext, Row displaysPanel, Module module, bool preview, Action refresh) {
			var displays = module.Prototype.Displays;
			int extCount = System.Math.Min(module.DisplayExtensionCount, module.Prototype.MaxDisplayExtensions);

			// Per-extension display widgets (flip-flop's per-channel LEDs etc.).
			// The active count comes from whichever pin side this proto is linked to,
			// clamped to whatever the proto registered.  Each one renders identically
			// to a static display.
			int linkedActive = module.Prototype.ExtensionDisplaysLinkedSide switch {
				ExtensionSide.Input => System.Math.Min(module.InputExtensionCount, module.Prototype.MaxInputExtensions),
				ExtensionSide.Output => System.Math.Min(module.OutputExtensionCount, module.Prototype.MaxOutputExtensions),
				_ => 0,
			};
			int extDispCount = System.Math.Min(linkedActive, module.Prototype.ExtensionDisplays.Count);
			// Cells claimed by the linked side's pin extensions but NOT covered by a
			// per-extension display widget — the LAST static display absorbs them so
			// the row stays gap-free without forcing the proto to declare a matching
			// filler extension display.  Gated on "no per-extension displays declared"
			// so modules like Stats_Unity that intentionally cap display extensions
			// below their pin extension count keep their existing gap behavior.
			int linkedLeftover = module.Prototype.ExtensionDisplays.Count == 0
				? System.Math.Max(0, linkedActive)
				: 0;
			int totalAbsorb = extCount + linkedLeftover;

			for (int i = 0; i < displays.Count; i++)
			{
				ModuleConnectorProto display = displays[i];
				// The LAST display absorbs DisplayExtensionCount cells (existing
				// AllowDisplayExtensions opt-in) plus any leftover cells from a linked
				// pin-side extension — see linkedLeftover above.  We materialise a thin
				// override of the proto so existing per-type renderers keep working
				// unchanged.
				if (totalAbsorb > 0 && i == displays.Count - 1)
				{
					display = new ModuleConnectorProto(
						display.Id, display.Name,
						display.Width + totalAbsorb.ToFix32(),
						display.DefaultText);
				}
				AddSingleDisplay(uiContext, displaysPanel, module, preview, display);
			}

			for (int i = 0; i < extDispCount; i++) {
				AddSingleDisplay(uiContext, displaysPanel, module, preview, module.Prototype.ExtensionDisplays[i]);
			}
		}

		// Per-display-type dispatch — extracted so both the static loop and the
		// per-extension loop go through the same renderer choice.
		private void AddSingleDisplay(UiContext uiContext, Row displaysPanel, Module module, bool preview, ModuleConnectorProto display)
		{
			if (display.DefaultText.StartsWith("[image]"))
			{
				displaysPanel.Add(ImageDisplay(uiContext, module, display));
			}
			else if (display.DefaultText.StartsWith("[button]"))
			{
				displaysPanel.Add(ToggleDisplay_Button(uiContext, module, display, preview));
			}
			else if (display.DefaultText.StartsWith("[pass]"))
			{
				displaysPanel.Add(PassDisplay_Button(uiContext, module, display, preview));
			}
			else if (display.DefaultText.StartsWith("[toggle]"))
			{
				displaysPanel.Add(ToggleDisplay(uiContext, module, display, preview));
			}
			else if (display.DefaultText.StartsWith("[led]"))
			{
				displaysPanel.Add(ToggleDisplay_LED(uiContext, module, display, preview, click : false));
			}
			else if (display.DefaultText.StartsWith("[fill]"))
			{
				if (display.Width > 0) {
					displaysPanel.Add(new Display().StateInactive().Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE));
				}
			}
			else if (display.DefaultText.StartsWith("[slider]"))
			{
				displaysPanel.Add(SliderDisplay(uiContext, module, display, preview));
			}
			else
			{
				displaysPanel.Add(TextDisplay(uiContext, module, display, preview));
			}
		}

		private UiComponent TextDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
		{
			var text = new StatusDisplay();
			text.Class(Cls.displayFont);
			text.PaddingLeftRight(6.px());
			text.Value(StatusText(module.Display[display.Id, display.DefaultText], out DisplayState? state, out ColorRgba? color).AsLoc());
			if (state.HasValue) {
				text.State(state ?? DisplayState.Neutral);
			}
			if (color.HasValue) {
				text.TextColor(color);
			}
			text.TextOverflow(TextOverflow.Clip);
			text.TextAlign(TextAlignment.RightMiddle);
			text.Color(ColorRgba.White);
			text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
			text.Observe(() => module.Display[display.Id, display.DefaultText])
				.Do((t) => {
					text.Value(StatusText(t, out DisplayState? stateN, out ColorRgba? colorN).AsLoc());
					if (stateN.HasValue) {
						text.State(stateN ?? DisplayState.Neutral);
					}
					if (colorN.HasValue) {
						text.TextColor(colorN);
					}
				});
			return text;
		}

		private string StatusText(string text, out DisplayState? state, out ColorRgba? color)
		{
			state = null;
			color = null;

			if (text.IsNullOrEmpty()) {
				return "";
			}

			while (text.StartsWith("#"))
			{
				switch (text[1])
				{
					case 'E':
						state = DisplayState.Danger;
						text = text.Substring(2);
						break;
					case 'W':
						state = DisplayState.Warning;
						text = text.Substring(2);
						break;
					case 'I':
						state = DisplayState.Inactive;
						text = text.Substring(2);
						break;
					case 'P':
						state = DisplayState.Positive;
						text = text.Substring(2);
						break;
					case 'C':
						int r = int.Parse(text.Substring(2, 2), NumberStyles.HexNumber);
						int g = int.Parse(text.Substring(4, 2), NumberStyles.HexNumber);
						int b = int.Parse(text.Substring(6, 2), NumberStyles.HexNumber);

						color = new ColorRgba(r, g, b);

						text = text.Substring(8);
						break;
					default:
						throw new NotImplementedException($"Missing type of format: {text[1]}!");
				}
			}
			return text;
		}

		private UiComponent ImageDisplay(UiContext uiContext, Module module, ModuleConnectorProto display)
		{
			string defaultIcon = display.DefaultText.Length > "[image]".Length
				? display.DefaultText.Substring("[image]".Length)
				: UserInterface.General.Empty128_png;
			var text = new DisplayWithIcon(StatusText(module.Display[display.Id, defaultIcon], out DisplayState? state, out ColorRgba? color));
			if (state.HasValue) {
				text.State(state ?? DisplayState.Neutral);
			}
			if (color.HasValue) {
				text.Icon.Color(color);
			}
			text.Icon.Margin(Px.Zero);
			text.Icon.Padding(Px.Zero);
			text.Icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
			text.Color(ColorRgba.White);
			text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
			text.Observe(() => module.Display[display.Id, defaultIcon])
				.Do((t) =>
				{
					text.Icon.Value(StatusText(t, out DisplayState? newState, out ColorRgba? newColor));
					if (newState.HasValue) {
						text.State(newState ?? DisplayState.Neutral);
					}
					if (newColor.HasValue) {
						text.Icon.Color(newColor);
					}
				});
			return text;
		}

		private UiComponent ToggleDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
		{
			char separator = display.DefaultText["[toggle]".Length];
			string[] options = display.DefaultText.Replace($"[toggle]{separator}", "").Split(separator);

			if (options.Length == 1 && options[0].Length == 0)
			{
				return ToggleDisplay_LED(uiContext, module, display, preview);
			}
			else if (options.Length == 1)
			{
				return ToggleDisplay_Symbol(uiContext, module, display, preview, options[0]);
			}
			else if (options.Length == 2)
			{
				return ToggleDisplay_DoubleText(uiContext, module, display, preview, options);
			}
			else
			{
				var text = new ButtonText(new Mafi.Localization.LocStrFormatted(module.Display[display.Id, display.DefaultText]));
				text.Color(ColorRgba.White);
				text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);

				m_controller.m_updaters.Add(new DataUpdater<
						string,
						(Module module, ButtonText text, ModuleConnectorProto display)
					>(
					getter: (c) => c.module.Display[c.display.Id, c.display.DefaultText],
					setter: (c, t) => c.text.Value(new Mafi.Localization.LocStrFormatted(t)),
					comparator: string.Equals,
					context: (module, text, display)
				));
				return text;
			}
		}

		private Button ToggleDisplay_DoubleText(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, string[] options)
		{
			var text = new ButtonText(new LocStrFormatted(module.Display[display.Id, options[0]]));
			text.TextOverflow(TextOverflow.Clip);
			text.TextAlign(TextAlignment.CenterMiddle);
			text.FontSize(10);
			text.Color(ColorRgba.White);
			text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);

			text.Observe(() => module.Display[display.Id, options[0]])
				.Do(t => text.Value(t.AsLoc()));

			if (!preview) {
				text.OnClick(() =>
				{
					if (module.Display[display.Id, options[0]] == options[0]) {
						module.Display[display.Id] = options[1];
					} else {
						module.Display[display.Id] = options[0];
					}
				});
			}
			return text;
		}

		private UiComponent ToggleDisplay_Symbol(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, string symbol, bool click = true)
		{
			ButtonText text = new ButtonText(new LocStrFormatted(symbol));
			text.TextOverflow(TextOverflow.Clip);
			text.TextAlign(TextAlignment.CenterMiddle);
			text.FontSize(10);
			text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
			text.Color(module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red);
			text.Observe(() => module.Display[display.Id, ""].Length > 0 ? ColorRgba.Green : ColorRgba.Red)
				.Do((color) => text.Color(color));

			if (click && !preview)
			{
				text.OnClick(() =>
				{
					if (module.Display[display.Id, ""].Length > 0) {
						module.Display[display.Id] = "";
					} else {
						module.Display[display.Id] = "1";
					}
				});
			}
			return text;
		}

		private UiComponent ToggleDisplay_LED(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview, bool click = true)
		{
			if (click) {
				return ToggleDisplay_Symbol(uiContext, module, display, preview, "●", click);
			}

			var text = new DisplayWithIcon(UserInterface.General.Circle_svg);
			string value = StatusText(module.Display[display.Id, ""], out DisplayState? state, out ColorRgba? color);
			text.Icon.Color(color.HasValue ? color : value.Length > 0 ? ColorRgba.Green : ColorRgba.Red);
			text.Icon.Margin(Px.Zero);
			text.Icon.Padding(Px.Zero);
			text.Icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
			if (state.HasValue) {
				text.State(state ?? DisplayState.Neutral);
			}
			text.Color(ColorRgba.White);
			text.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE);
			text.Observe(() => (color: StatusText(module.Display[display.Id, ""], out DisplayState? newState, out ColorRgba? userColor).Length > 0 ? ColorRgba.Green : ColorRgba.Red, state: newState, userColor))
				.Do((pair) =>
				{
					if (pair.userColor != null) {
						text.Icon.Color(pair.userColor);
					} else {
						text.Icon.Color(pair.color);
					}

					if (pair.state.HasValue) {
						text.State(pair.state ?? DisplayState.Neutral);
					}
				});
			return text;
		}

		// Interactive SVG rotary toggle knob (display type "[button]").  One of three
		// monochrome knob PNGs (rasterized from the mod's Controls/*.svg) is chosen by
		// state and tinted — the white face takes the full colour, the gray bezel a
		// darker shade, and the black ring / pointer stay dark under the multiply-tint:
		//   off    -> pointer up-left  (Toggle_Off), red
		//   on     -> pointer up-right (Toggle_On),  green
		//   held   -> pointer straight up (Toggle_Mid), the current state's colour
		//   paused -> gray (controller paused), rotation still shows the on/off value
		// On/off is the module display value (non-empty == on, matching "[toggle]").
		private UiComponent ToggleDisplay_Button(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
		{
			ColorRgba offColor = new ColorRgba(210, 70, 60);     // inactive — red
			ColorRgba onColor = new ColorRgba(70, 195, 110);     // active — green
			ColorRgba pausedColor = new ColorRgba(140, 146, 156); // controller paused — gray

			bool isOn() => module.Display[display.Id, ""].Length > 0;
			bool isPaused() => module.Controller != null && module.Controller.IsPaused;

			// Full-cell click target; the square knob icon is centred inside it so a
			// wider-than-1 display cell doesn't stretch the art horizontally.
			Row container = new Row()
				.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE)
				.AlignItemsCenterMiddle();

			Icon icon = new Icon(NewAssets.Controls.ToggleOff);
			// Fill the cell — the knob art carries its own margin inside the viewBox so
			// the black ring isn't clipped at full size.
			icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
			container.Add(icon);

			// Held state — while pressed the pointer rotates to the neutral mid (up)
			// position; the colour still reflects the current on/off value.
			bool pressed = false;

			void repaint()
			{
				bool on = isOn();
				string tex = pressed
					? NewAssets.Controls.ToggleMid
					: (on ? NewAssets.Controls.ToggleOn : NewAssets.Controls.ToggleOff);
				// Paused wins on colour (gray); otherwise the on/off hue.
				ColorRgba color = isPaused() ? pausedColor : (on ? onColor : offColor);
				icon.Value(tex, color);
			}
			repaint();

			// React to value flips (Python action / input pin) and to the controller
			// being paused / resumed.
			icon.Observe(() => isOn())
				.Observe(() => isPaused())
				.Do((on, paused) => repaint());

			if (!preview)
			{
				container.RegisterCallback<UnityEngine.UIElements.PointerDownEvent>(_ => { pressed = true; repaint(); });
				container.RegisterCallback<UnityEngine.UIElements.PointerLeaveEvent>(_ =>
				{
					// Pointer left while held — cancel the press (no toggle) and redraw.
					if (pressed) {
						pressed = false;
						repaint();
					}
				});
				container.RegisterCallback<UnityEngine.UIElements.PointerUpEvent>(evt =>
				{
					bool wasPressed = pressed;
					pressed = false;
					// Only left-button releases that started as a press on this widget flip
					// the toggle — mirrors a real button's click semantics.
					if (wasPressed && evt.button == 0)
					{
						module.Display[display.Id] = isOn() ? "" : "1";
						evt.StopPropagation();
					}
					repaint();
				});
			}
			return container;
		}

		// Interactive SVG pass-through gate knob (display type "[pass]").  Like the
		// "[button]" knob but the pointer is a flow ARROW and the light tracks the input:
		//   passing (on)  -> arrow down,  green while the input is non-zero else red
		//   blocked (off) -> arrow right, light off (gray)
		//   held          -> arrow bottom-right, keeps the current light colour
		// On/off (gate open) is the module display value (non-empty == passing), matching
		// the Button (pass value) module's Action.
		private UiComponent PassDisplay_Button(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
		{
			ColorRgba litColor = new ColorRgba(70, 195, 110);  // passing, input on — green
			ColorRgba noInputColor = new ColorRgba(210, 70, 60); // passing, input off — red
			ColorRgba offColor = new ColorRgba(140, 146, 156); // blocked — gray

			bool isPassing() => module.Display[display.Id, ""].Length > 0;
			bool inputActive() => module.Input["value"] != Fix32.Zero;

			Row container = new Row()
				.Size(Sizes.BLOCK_SIZE * display.Width.ToFloat(), Sizes.BLOCK_SIZE)
				.AlignItemsCenterMiddle();

			Icon icon = new Icon(NewAssets.Controls.PassRight);
			icon.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE);
			container.Add(icon);

			// Held state — while pressed the arrow rotates to the bottom-right neutral
			// position; the light still reflects the current passing/input state.
			bool pressed = false;

			void repaint()
			{
				bool passing = isPassing();
				string tex = pressed
					? NewAssets.Controls.PassDiag
					: (passing ? NewAssets.Controls.PassDown : NewAssets.Controls.PassRight);
				// While passing: green if a signal is present on the input, red if not.
				// While blocked: gray (light off).
				ColorRgba color = passing
					? (inputActive() ? litColor : noInputColor)
					: offColor;
				icon.Value(tex, color);
			}
			repaint();

			// React to the gate flipping (click / Python / input pin) and to the input
			// signal turning on/off so the light follows it live.
			icon.Observe(() => isPassing())
				.Observe(() => inputActive())
				.Do((passing, active) => repaint());

			if (!preview)
			{
				container.RegisterCallback<UnityEngine.UIElements.PointerDownEvent>(_ => { pressed = true; repaint(); });
				container.RegisterCallback<UnityEngine.UIElements.PointerLeaveEvent>(_ =>
				{
					if (pressed) {
						pressed = false;
						repaint();
					}
				});
				container.RegisterCallback<UnityEngine.UIElements.PointerUpEvent>(evt =>
				{
					bool wasPressed = pressed;
					pressed = false;
					if (wasPressed && evt.button == 0)
					{
						module.Display[display.Id] = isPassing() ? "" : "1";
						evt.StopPropagation();
					}
					repaint();
				});
			}
			return container;
		}

		// TODO(displays): revisit the slider/display abstraction — the display
		// definition needs richer fields (similar to inputs/outputs/cable info) so a
		// single proto entry carries enough to render and bind data without parsing
		// the DefaultText string.  Current implementation works for the slider but is
		// the minimum-viable shape; deferred for later.
		//
		// Slider display.  Value lives on module.Display[id] and the bounds on
		// module.Display[id + "_min"] / module.Display[id + "_max"], so a Python
		// action() can drive everything by writing strings back through the Display
		// dict.  Proto-time defaults are baked into DefaultText as
		// "[slider]:<min>:<max>" by AddDisplaySlider.  Width is display.Width clamped
		// to 1-4 cells.  IgnoreInputPicking makes the slider non-interactive — it's a
		// display, not an input.
		private UiComponent SliderDisplay(UiContext uiContext, Module module, ModuleConnectorProto display, bool preview)
		{
			// Parse the proto-time defaults out of DefaultText.
			float defaultMin = 0f;
			float defaultMax = 1f;
			if (display.DefaultText.Length > "[slider]".Length)
			{
				string body = display.DefaultText.Substring("[slider]".Length);
				if (body.Length > 0)
				{
					char sep = body[0];
					string[] parts = body.Substring(1).Split(sep);
					if (parts.Length >= 1 && float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float pmin)) {
						defaultMin = pmin;
					}
					if (parts.Length >= 2 && float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float pmax)) {
						defaultMax = pmax;
					}
				}
			}

			string keyMin = display.Id + "_min";
			string keyMax = display.Id + "_max";

			float resolveMin()
			{
				string s = module.Display[keyMin, ""];
				return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : defaultMin;
			}
			float resolveMax()
			{
				string s = module.Display[keyMax, ""];
				return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : defaultMax;
			}

			int blockWidth = Math.Max(1, Math.Min(4, display.Width.ToFloat().RoundToInt()));
			var slider = new Mafi.Unity.UiToolkit.Library.Slider();
			slider.Size(Sizes.BLOCK_SIZE * blockWidth, Sizes.BLOCK_SIZE);
			slider.IgnoreInputPicking(); // read-only display, not an input

			slider.Observe(() => module.Display[display.Id, "0"])
				  .Observe(resolveMin)
				  .Observe(resolveMax)
				  .Do((valStr, min, max) =>
				  {
					  if (max <= min) {
						max = min + 1f; // safety: avoid divide-by-zero in Slider.Value
					}
					slider.Range(min, max);
					slider.Value(float.TryParse(valStr, NumberStyles.Float, CultureInfo.InvariantCulture,
						out float v)
						? v
						: min);
				});
			return slider;
		}

		// Builds the keybind reference floater body shown by Floater() on hover —
		// mirrors Mafi's SimpleTooltip shell (Panel.StyleFloater + BrightText +
		// ReducedPadding) and uses KeyUi for modifier chips so the visual matches
		// the rest of the in-game hover hints.  Each row is `[KeyUi(MOD)] +
		// [MouseLeft/Right icon]   description`; modifier-less rows skip the chip.
		private UiComponent buildModuleKeybindHelp(UiComponent host)
		{
			Column rows = new Column(gap: 4.px());
			rows.Add(makeModuleKeybindRow(null,    /*left:*/ true,  () => NewTr.Inspector.ModuleHintOpen,         host));
			rows.Add(makeModuleKeybindRow("ALT",   /*left:*/ true,  () => NewTr.Inspector.ModuleHintMove,         host));
			rows.Add(makeModuleKeybindRow("SHIFT", /*left:*/ true,  () => NewTr.Inspector.ModuleHintCopy,         host));
			rows.Add(makeModuleKeybindRow(null,    /*left:*/ false, () => NewTr.Inspector.ModuleHintRemove,       host));
			rows.Add(makeModuleKeybindRow("SHIFT", /*left:*/ false, () => NewTr.Inspector.ModuleHintRemoveDirect, host));
			return rows;
		}

		private Row makeModuleKeybindRow(string modifier, bool left, Func<LocStr> labelGetter, UiComponent host)
		{
			Row row = new Row(gap: 4.px());
			if (modifier != null)
			{
				row.Add(new KeyUi(modifier));
				row.Add(new Icon("Assets/Unity/UserInterface/General/PlusThin.svg").Size(12.px()));
			}
			row.Add(new Icon(left
				? "Assets/Unity/UserInterface/General/MouseLeft.svg"
				: "Assets/Unity/UserInterface/General/MouseRight.svg")
				.Size(Px.Auto, 24.px()));
			row.Add(new Label().LaterText(labelGetter, host).TinyFontSize().FlexGrow(1));
			return row;
		}

		// Small confirmation popup anchored under the right-clicked module body.
		// Keeps the destructive Remove action one click away from any accidental
		// right-click on the inspector grid (Shift+RMB bypasses this dialog).
		// Mirrors the dropdown-floater pattern used elsewhere in this codebase
		// (ModuleEditDialog, openDescriptionDialog) so the visual matches the
		// rest of the inspector.
		private void openConfirmRemoveDialog(Module module, UiComponent anchor)
		{
			FloatingColumn dialog = new FloatingColumn(
				new DropdownPositionPolicy(), false, false, true);
			dialog.Gap(5.px());

			PanelRow body = dialog.AddAndReturn(new PanelRow(noBolts: true).PanelStyleHud());
			body.Width(Px.Auto);
			body.Height(Px.Auto);

			Label prompt = new Label()
				.LaterText(() => NewTr.Inspector.ConfirmRemoveModule, dialog)
				.TextAlign(TextAlignment.LeftMiddle);

			// Icon-only buttons keep the confirm dropdown compact and language-neutral:
			// trash for the destructive action, "circle with cross" cancel for dismiss.
			// Tooltip text still localises through NewTr so screen readers and hover
			// hints stay translated.
			ButtonIcon removeBtn = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
			removeBtn.LaterText<ButtonIcon>(() => NewTr.Inspector.ConfirmRemoveYes, dialog, (b, v) => b.Tooltip(v));
			removeBtn.OnClick(() =>
			{
				m_controller.RemoveModule(module);
				dialog.Close();
			});

			ButtonIcon cancelBtn = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Cancel_svg)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
			cancelBtn.LaterText<ButtonIcon>(() => NewTr.Inspector.ConfirmRemoveCancel, dialog, (b, v) => b.Tooltip(v));
			cancelBtn.OnClick(() => dialog.Close());

			Row buttons = new Row(gap: 5.px()) { removeBtn, cancelBtn };
			body.BodyAdd(c => c.Padding(8).Gap(5.px()), prompt, buttons);

			dialog.Open(anchor);
		}

		// Static reverse-lookup cache: prototype id → its ModuleSwapGroup.  Built once lazily
		// from the (immutable-after-registration) ModuleSwapGroup protos so each ModuleView
		// build is a single dictionary hit rather than a scan over every group.  Never
		// invalidated — the group protos are the same instances for the whole process.
		private static Dictionary<ModuleProto.ID, ModuleSwapGroup> s_swapGroupByProto;

		private static ModuleSwapGroup swapGroupFor(Module module)
		{
			if (module?.Prototype == null || module.Context?.ProtosDb == null)
			{
				return null;
			}
			if (s_swapGroupByProto == null)
			{
				Dictionary<ModuleProto.ID, ModuleSwapGroup> map = new Dictionary<ModuleProto.ID, ModuleSwapGroup>();
				foreach (ModuleSwapGroup group in module.Context.ProtosDb.All<ModuleSwapGroup>())
				{
					foreach (ModuleProto member in group.Members)
					{
						map[member.Id] = group;
					}
				}
				s_swapGroupByProto = map;
			}
			return s_swapGroupByProto.TryGetValue(module.Prototype.Id, out ModuleSwapGroup found) ? found : null;
		}

		// Standard dropdown-style picker of the swap group's other members — modelled on the
		// new-module picker (PickNewModule) but without the category column, since a swap group
		// IS a single category.  Header carries a search icon, a search field, and the group
		// name; the body is a scrollable list where each row renders the candidate as a live
		// module preview (same as the new-module selector) plus its name.  Allowed rows schedule
		// a ModuleSwapPrototypeCmd (and redraw the grid on apply so the new pins/width render);
		// blocked rows are disabled with the failing check's reason as tooltip so the player
		// sees WHY it can't swap.
		private void openSwapPicker(Module module, UiComponent anchor, ModuleSwapGroup group, UiContext uiContext)
		{
			// Card width used both for the row layout and for the dropdown's left shift below.
			const int cardWidth = 320;
			// How far left to pull the dropdown so the module-preview column (which sits after the
			// title/description card in each row) lines up under the module being swapped: the card
			// width + the ButtonVariant gap (5) + the panel/scroll/button-row inner padding (~15).
			// Bump the trailing term if the preview lands a hair right/left of the module.
			const int previewLeftInset = cardWidth + 5 + 8;

			FloatingColumn dialog = new FloatingColumn(
				new SwapPickerPositionPolicy(previewLeftInset), false, false, true);

			PanelWithHeader panel = dialog.AddAndReturn(new PanelWithHeader().Height(Px.Auto));

			// Header: search icon + text field + group name.  Live-filter query is held in a
			// captured local read by the per-row Observe below.
			string searchText = "";
			TextField search = new TextField();
			search.Placeholder(Tr.Search);
			search.MinWidth(150.px());
			search.FlexGrow(1);
			search.FocusOnShow();
			search.OnValueChanged(text => searchText = text);
			dialog.OnShow(() => search.SetValue("".AsLoc()));

			panel.Header.Gap(5.px());
			panel.Header.Add(
				new Icon(UserInterface.General.Search_svg),
				search,
				new Label(group.DisplayName).TextAlign(TextAlignment.CenterMiddle).FlexGrow(1));

			ScrollColumn list = panel.Body.AddAndReturn(new ScrollColumn())
				.Width(Px.Auto)
				.Height(Px.Auto);
			// Render the candidate buttons as one connected group (segmented look) rather than
			// separate floating rows.
			list.Class(Cls.group);

			foreach (ModuleProto candidate in group.Members)
			{
				if (candidate.Id == module.Prototype.Id)
				{
					continue;
				}
				LocStrFormatted blockedReason = group.Check(module, candidate);
				bool allowed = blockedReason.IsEmptyOrNull;
				string name = candidate.Strings.Name.TranslatedString;
				string searchString = name + " " + candidate.Symbol;

				// Ghost preview of the candidate carrying THIS module's current extension counts
				// (clamped to the candidate's own maxes, no topology side effect) so the preview
				// shows the actual post-swap footprint rather than the candidate's bare default.
				Module ghost = new Module(candidate, m_controller.Entity.Context, m_controller.Entity, 0);
				ghost.InputExtensionCount = System.Math.Min(module.InputExtensionCount, candidate.MaxInputExtensions);
				ghost.OutputExtensionCount = System.Math.Min(module.OutputExtensionCount, candidate.MaxOutputExtensions);
				ghost.DisplayExtensionCount = System.Math.Min(module.DisplayExtensionCount, candidate.MaxDisplayExtensions);
				ghost.Prototype.ExecuteInit(ghost, log: false);
				ghost.Prototype.DisplayUpdate(ghost);
				ModuleView preview = new ModuleView(ghost, m_controller, uiContext, true, () => { });
				preview.IgnoreInputPicking();   // clicks fall through to the row button

				// Titled card + description, same layout as the new-module picker
				// (NewModule.CreateUi): [ name header + description ][ module preview ].
				PanelWithHeader card = new PanelWithHeader().Height(Sizes.BLOCK_SIZE * 4).Width(cardWidth);
				card.Header.AddAndReturn(new Label(candidate.Strings.Name))
					.Class(Cls.bold).Fill().TextCenterMiddle();
				card.BodyAdd(
					new ScrollColumn() {
						c => c.FlexGrow(1).AlignSelf(Mafi.Unity.UiToolkit.Component.Align.Stretch),
						new Label(candidate.Strings.DescShort)
							.TextOverflow(TextOverflow.Wrap)
							.TextAlign(TextAlignment.LeftTop)
							.AlignSelf(Mafi.Unity.UiToolkit.Component.Align.Stretch)
					});

				ButtonRow optionBtn = new ButtonRow(new ButtonVariant().Gap(5));
				optionBtn.Add(card);
				optionBtn.Add(preview);
				optionBtn.Tooltip((allowed ? name : name + " — " + blockedReason.Value).AsLoc());
				if (allowed)
				{
					ModuleProto target = candidate;
					optionBtn.OnClick(() =>
					{
						uiContext.InputScheduler.ScheduleAndOnApplied(
							new ModuleSwapPrototypeCmd(module.Controller.Id, module.Id, target.Id.Value),
							optionBtn,
							() => { dialog.Close(); m_controller.RedrawComponents(); });
					});
				}
				else
				{
					optionBtn.ObserveEnabled(() => false);
				}
				list.Add(optionBtn);

				// Live search filter — hide rows whose name/symbol don't contain the query.
				dialog.Observe(() => searchText)
					.Do(query => optionBtn.Visible(
						searchString.Contains(query, StringComparison.InvariantCultureIgnoreCase)));
			}

			dialog.Height(Px.Auto);
			dialog.Open(anchor);
		}
	}

	private void AddPreviewHighlight(Module module)
	{
		m_controller.AddPreviewHighlight(module);
	}

	private void ClearPreviewHighlight()
	{
		m_controller.ClearPreviewHighlight();
	}

	// Routes module deletion through a command so the mutation (drop from
	// Modules + scrub every dangling cable) runs on the sim thread.  RedrawComponents
	// runs in the OnApplied callback so the panel doesn't repaint against stale state.
	// onApplied receives the real outcome (cmd.Result == true ⇔ removed) for callers
	// that need to know whether the mutation actually happened.
	public void RemoveModule(Module module, Action<bool> onApplied = null)
	{
		if (module == null || module.Controller == null || Entity == null) {
			onApplied?.Invoke(false);
			return;
		}
		if (module.Controller.Id != Entity.Id) {
			onApplied?.Invoke(false);
			return;
		}
		ModuleRemoveCmd cmd = new ModuleRemoveCmd(Entity.Id, module.Id);
		m_controller.Context.InputScheduler.ScheduleAndOnApplied(cmd, this, () =>
		{
			bool ok = cmd.ResultSet && !cmd.HasError;
			if (ok) {
				RedrawComponents();
			}
			onApplied?.Invoke(ok);
		});
	}

	// New move logic operating directly on Module.Row / Module.Column.
	// Mirrors <see cref="Controller.TryMoveModule"/>'s clamp-then-check rule:
	// the target is snapped to the controller's bounds first, so a wide module
	// already at the right edge reports CanMove(x:+1) == true (it would land
	// on its current position — a successful no-op).  Callers can still rely
	// on this for "should the keyboard nudge play a fail sound?" — the answer
	// is now only false when the clamped slot is occupied by something else.
	public bool CanMove(Module module, int x = 0, int y = 0)
	{
		if (module == null || module.Controller == null || Entity == null) {
			return false;
		}
		if (module.Controller.Id != Entity.Id || Entity.Prototype == null) {
			return false;
		}
		int width = module.Layout.GetWidth(module);
		if (width <= 0 || width > Entity.Prototype.Columns) {
			return false;
		}
		int targetRow = module.Row + y;
		int targetCol = module.Column + x;
		int clampedRow = Math.Max(0, Math.Min(Entity.Prototype.Rows - 1, targetRow));
		int clampedCol = Math.Max(0, Math.Min(Entity.Prototype.Columns - width, targetCol));
		if (clampedRow == module.Row && clampedCol == module.Column) {
			return true;
		}
		return Entity.IsRangeFree(clampedRow, clampedCol, width, ignore: module);
	}

	// Relative move (arrow-style nudge).  The target is dispatched as-is — the
	// executor clamps past-end-of-line targets to the rightmost valid slot, so
	// nudging a wide module against the edge is a successful no-op instead of
	// a refusal.  No pre-validation here: the cmd's HasError reflects whether
	// another module actually blocked the (clamped) slot.
	public void Move(Module module, int x = 0, int y = 0, Action<bool> onApplied = null)
	{
		if (module == null || module.Controller == null || Entity == null) {
			onApplied?.Invoke(false);
			return;
		}
		if (module.Controller.Id != Entity.Id) {
			onApplied?.Invoke(false);
			return;
		}
		int targetRow = module.Row + y;
		int targetCol = module.Column + x;
		ModuleMoveToCmd cmd = new ModuleMoveToCmd(Entity.Id, module.Id, targetRow, targetCol);
		m_controller.Context.InputScheduler.ScheduleAndOnApplied(cmd, this, () =>
		{
			bool ok = cmd.ResultSet && !cmd.HasError;
			if (ok) {
				RedrawComponents();
			} else {
				m_controller.Context.AudioDb.InvalidOp(true).Play();
			}
			onApplied?.Invoke(ok);
		});
	}

	/// Drop a module at an explicit (row, col).  The executor clamps past-end-of-
	/// line targets, so an in-bounds-after-clamp request applies even when the
	/// raw target is off-grid.  The returned bool only confirms the cmd was
	/// dispatched (entity refs valid) — the actual outcome is surfaced via the
	/// optional <paramref name="onApplied"/> callback (true ⇔ cmd.Result == true).
	public bool TryMoveTo(Module module, int targetRow, int targetCol, Action<bool> onApplied = null)
	{
		if (module == null || module.Controller == null || Entity == null) {
			onApplied?.Invoke(false);
			return false;
		}
		if (module.Controller.Id != Entity.Id) {
			onApplied?.Invoke(false);
			return false;
		}
		ModuleMoveToCmd cmd = new ModuleMoveToCmd(Entity.Id, module.Id, targetRow, targetCol);
		m_controller.Context.InputScheduler.ScheduleAndOnApplied(cmd, this, () =>
		{
			bool ok = cmd.ResultSet && !cmd.HasError;
			if (ok) {
				RedrawComponents();
			} else {
				m_controller.Context.AudioDb.InvalidOp(true).Play();
			}
			onApplied?.Invoke(ok);
		});
		return true;
	}
}
