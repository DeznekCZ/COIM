using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Console;
using Mafi.Core.Research;
using Mafi.Unity.UiToolkit;
using ProgramableNetwork.Data.Variables;
using UnityEngine;
using static Mafi.Unity.Assets.Unity;
using Display = Mafi.Unity.Ui.Library.Display;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork.Ui;

[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
public partial class ControllerInspector : BaseInspector<Controller>, ISelectionInspector<IEntity, EntitySelector, Controller> {
	private readonly AudioSource m_invalidOpSound;
	private bool m_highlightSearched;
	private IRenderedEntity m_hoveredEntity;
	private readonly PanelWithHeader m_modulesPanel;
	private readonly LinesFactory m_linesFactory;
	private readonly List<LineMb> m_lines = new List<LineMb>();
	private readonly Material m_movingArrowsLineMaterialShared;
	private readonly ControllerView m_view;
	private readonly ButtonIcon m_colorButton;
	private readonly VariableWindowController m_variableWindowController;
	private readonly PlcPyCodeEditorWindowController m_plcPyCodeEditorWindowController;

	// TODO
	public ModuleConnector m_higlightedOutput;
	public ModuleConnector m_higlightedInput;
	public bool m_showsLinks;
	// Per-inspector toggle for the click-action hint floaters: the "+ click to add"
	// helper on empty slots and the LMB/Alt+LMB/Shift+LMB/RMB/Shift+RMB tooltip on
	// placed modules.  Default on so first-time users still discover the keybinds;
	// flips to off via the checkbox in the modules-panel header so power users can
	// quiet the inspector once they know the shortcuts.
	public bool m_showHints = true;
	public IGameConsole m_console;

	/// <summary>
	/// When non-null, the corresponding ModuleView will paint a cable-style highlight
	/// on its central button.  Set by <see cref="EntityConnectionsView"/> on row hover.
	/// </summary>
	public Module HighlightedFromSidePanel;

	/// <summary>
	/// When non-null, the matching section in <see cref="EntityConnectionsView"/> is
	/// highlighted.  Set by <see cref="ControllerView.ModuleView"/> on the module's
	/// fieldsPanel hover so the user can see which side-panel entry owns the configuration
	/// for the module they're pointing at on the grid.
	/// </summary>
	public Module HoveredModuleGraphic;

	/// <summary>
	/// When non-null, the user has ALT+LMB-clicked this module to "pick it up"; the
	/// next ALT+LMB on a free slot drops it there.  No edit mode gate any more —
	/// pickup is purely modifier-driven and the visual cue (gold tint) is the only
	/// indicator that a move is in flight.
	/// </summary>
	public Module PickedUpModule;

	public ResearchManager ResearchManager { get; }

	public ControllerInspector(
		UiContext context,
		CursorManager cursorManager,
		CursorPickingManager cursorPickingManager,
		ShortcutsManager shortcutsManager,
		//TerrainCursor terrainCursor,
		CameraController cameraController,
		VariableWindowController variableWindowController,
		PlcPyCodeEditorWindowController plcPyCodeEditorWindowController,
		NewInstanceOf<EntityHighlighter> entityHighlighter,
		NewInstanceOf<EntityHighlighter> entityHighlighterSelectable,
		LinesFactory linesFactory,
		AssetsDb assetsDb,
		ResearchManager researchManager,
		IGameConsole console
		) : base(context) {
		m_linesFactory = linesFactory;
		m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
		CursorManager = cursorManager;
		CameraController = cameraController;
		CursorPickingManager = cursorPickingManager;
		//TerrainCursor = terrainCursor;
		EntityHighlighter = entityHighlighter.Instance;
		EntityHighlighterSelectable = entityHighlighterSelectable.Instance;
		ShortcutsManager = shortcutsManager;
		ResearchManager = researchManager;
		m_invalidOpSound = context.AudioDb.InvalidOp();
		m_console = console;
		m_variableWindowController = variableWindowController;
		m_plcPyCodeEditorWindowController = plcPyCodeEditorWindowController;

		// Wider than the default 650px inspector so the module grid + cable corridors
		// + side connections panel all have room without crowding.
		WindowSize(750.px(), Px.Auto);

		ProgressBar bar;
		StatusRow.Clear();
		StatusRow.Gap(5.px());

		// Combined speed widget — Label showing the live tick rate plus compact
		// -/+ ButtonIcons, all inside a single DisplayRow.  DisplayRow already
		// provides the display font + glass background, so a plain Label inside
		// renders in the right style without needing a nested Display.
		DisplayRow speedControl = new DisplayRow();
		speedControl.LaterText<DisplayRow>(() => NewTr.Inspector.ComputingSpeedTooltip, this, (d, v) => d.Tooltip(v));
		Label speedLabel = new Label(LocStrFormatted.Empty)
			.Width(70.px())
			.TextAlign(TextAlignment.RightMiddle);
		speedLabel.Observe(() => Entity?.DelayBetweenTicks ?? 0)
				  .Do(d => speedLabel.Value(new LocStrFormatted(
					  $"{(600 / (1f + d)).ToFix32().ToStringRounded(0)} t/m")));
		ButtonIcon decBtn = new ButtonIcon(Button.IconOnly, UserInterface.General.Minus128_png)
			.Size(20.px(), 20.px())
			.IconSize(16.px(), 16.px())
			.OnClick(() => Entity.DelayBetweenTicks++);
		decBtn.Icon.AbsolutePositionCenterMiddle();
		decBtn.ObserveEnabled(() => Entity.DelayBetweenTicks < 29);
		ButtonIcon incBtn = new ButtonIcon(Button.IconOnly, UserInterface.General.Plus128_png)
			.Size(20.px(), 20.px())
			.IconSize(16.px(), 16.px())
			.OnClick(() => Entity.DelayBetweenTicks--);
        incBtn.Icon.AbsolutePositionCenterMiddle();
		incBtn.ObserveEnabled(() => Entity.DelayBetweenTicks > 0);
		speedControl.Row.Add(speedLabel, decBtn, incBtn);

		StatusRow.Add(
			Status,
			new UiComponent().Fill(),
			new Label()
				.LaterText(() => NewTr.Inspector.ComputingSpeed, this)
				.TextAlign(TextAlignment.RightMiddle),
			// Fixed height — StatusRow constrains its children to a slim strip and
			// HeightAuto would collapse the bar to ~0 px.  Default invisible because
			// the observer below only flips it on once the player picks a delay
			// >= 10 ticks (otherwise the bar would just blink full each tick).
			(bar = new ProgressBar()
				.Height(Sizes.BLOCK_SIZE)
				.Width(150)
				.Visible(false)),
			speedControl
		);

        this.Observe(() => Entity.DelayBetweenTicks)
            .Observe(() => Entity.Clock)
            .Observe(() => Entity.IsEnabled)
            .Do((speed, clock, enabled) => {
				if (speed < 10)
				{
					bar.Visible(false);
					return;
                }
				bar.Visible(true);
                bar.Color(enabled ? ColorRgba.GreenYellow : ColorRgba.DarkYellow);
                if (speed == clock)
                {
                    bar.Value(Percent.Hundred);
                }
                else
                {
                    bar.ValueFromRatio(clock, speed);
                }
            });

        // Description button + module-count chip live in the inspector's
        // TopLeftDisplays row (BaseInspector's actual left-side header — Window's
        // LeftHeaderButtons sit in the title bar and don't render here).
        // Click on the button opens a fresh FloatingColumn dialog because Mafi's
        // .Floater()/.FloaterInteractive() are tooltip-based (hover, not click).
        ButtonIcon descBtn = new ButtonIcon(Button.Header, UserInterface.General.Edit_svg);
		descBtn.Tooltip("Controller description (also lists modules)".ToDoLoc());
		descBtn.OnClick(() =>
		{
			if (Entity == null) {
				return;
			}
			openDescriptionDialog(descBtn);
		});
		TopLeftDisplays.Add(descBtn);

		// At-a-glance module count next to the description button — same pattern as
		// the VariableHudDisplay (button + Display chip).  Saves a click for the
		// common "how many modules does this controller have" question without
		// having to open the dialog.
		Display moduleCountDisplay = new Display("0".AsLoc()).Width(40.px());
		moduleCountDisplay.TextCenterMiddle();
		moduleCountDisplay.Tooltip("Number of modules in this controller".ToDoLoc());
		moduleCountDisplay.ObserveValue(() => Entity?.Modules?.Count ?? 0);
		TopLeftDisplays.Add(moduleCountDisplay);

        // Show main body, there is no AddPanel method used, must be set manually
		this.MainBody.Show();

        Row panels = this.MainBody.AddAndReturn(new Row())
			.HeightAuto()
			.Gap(5.px());
		// align to top so the connections panel doesn't end up in the middle when there are few modules
		panels.JustifyItemsStart();
		panels.AlignItemsStart();

		// UI
		m_modulesPanel = panels.AddAndReturn(new PanelWithHeader().Fill().HeightAuto());
		m_modulesPanel.Header.Add(
			new Label()
				.LaterText(() => NewTr.Inspector.Modules, this)
				.FlexGrow(1)
				.TextAlign(TextAlignment.CenterMiddle)
			);
		// "Show hints" checkbox sits flush at the right end of the header.  Both
		// floaters (slot "+ click to add" helper and per-module keybind tooltip)
		// observe m_showHints and return Option<UiComponent>.None when off, so a
		// single bool flip silences every hint at once.
		Toggle hintsToggle = new Toggle();
		hintsToggle.Value(m_showHints);
		hintsToggle.LaterText<Toggle>(() => NewTr.Inspector.ShowHints, this, (t, v) => t.Tooltip(v));
		hintsToggle.OnValueChanged(v => m_showHints = v);
		m_modulesPanel.Header.Add(
			new Label().LaterText(() => NewTr.Inspector.ShowHints, this).TinyFontSize().TextAlign(TextAlignment.RightMiddle),
			hintsToggle
		);
		m_modulesPanel.BodyAdd(m_view = new ControllerView(this, refresh));

		PanelWithHeader connectionsPanel = panels.AddAndReturn(new PanelWithHeader().Fill().HeightAuto());
		connectionsPanel.Header.Add(
			new Label()
				.LaterText(() => NewTr.Inspector.Connections, this)
				.FlexGrow(1)
				.TextAlign(TextAlignment.CenterMiddle)
			);
		EntityConnectionsView connectionsView = new EntityConnectionsView(this);
		connectionsPanel.BodyAdd(connectionsView);

		HeaderButtons.AddAndReturn(new ButtonIcon(Button.Header, UserInterface.General.Connect128_png))
			.OnClick(() => Entity.Resolver.Resolve<ConnectionInfo>().Open(context.UiRoot))
			.OnMouseEnterLeave(addPreviewHighlightAll, ClearPreviewHighlight);

		// "Save controller as blueprint" — dual to the per-module save button in
		// ModuleEditDialog.  Snapshots the entire controller (modules, layout,
		// internal cables, color, speed) into BlueprintsLibrary as
		// [PN-Controller]-<name> so the player can paste it elsewhere via the
		// base-game blueprint browser.
		ButtonIcon saveCtrlBp = new ButtonIcon(Button.Header, UserInterface.General.Save_svg);
		saveCtrlBp.Tooltip("Save this controller (with modules) as a reusable blueprint".ToDoLoc());
		saveCtrlBp.OnClick(() => SaveBlueprintDialog.ForController(Entity, saveCtrlBp, context, Entity.Resolver));
		HeaderButtons.Add(saveCtrlBp);

		m_colorButton = new ButtonIcon(UserInterface.Cursors.Paint32_png);
		m_colorButton.Observe(() => Entity.Color)
				   .Do(color => m_colorButton.Icon.Color(color));

		RgbColorPicker colorPicker = new RgbColorPicker()
			.LaterText<RgbColorPicker>(() => NewTr.Inspector.ControllerColor, this, (cp, v) => cp.Title(v));
		colorPicker.Observe(() => Entity.Color)
				   .Do(c => colorPicker.Value(c));
		colorPicker.OnColorChanged(c => context.InputScheduler.ScheduleInputCmd(
			new ControllerSetColorCmd(Entity.Id, c)));
		m_colorButton.FloaterInteractive(colorPicker);

		TopRightButtons.Add(m_colorButton);

		this.Observe(() => Entity)
			.Observe(() => Entity?.Modules)
			.Do((entity, modules) => refresh());

		this.Observe(() => Entity?.State)
			.Do((state) => {
				Status.As(state ?? Tr.EntityStatus__Working, DisplayState.Positive);
			});
	}

	// Opens a fresh FloatingColumn each click anchored to the description button.
	// Re-creating per-click keeps observer wiring simple — the dialog binds to the
	// inspector's current Entity at the moment it opens.
	//
	// Layout: two stacked PanelRows with PanelStyleHud backgrounds and a small
	// gap between them — no horizontal divider, the panel borders do the visual
	// separation.  Each panel has a fixed ~10-line height; overflow scrolls
	// internally so the dialog stays the same size regardless of how much text
	// the player wrote or how many modules the controller has.
	private void openDescriptionDialog(Button anchor)
	{
		FloatingColumn dialog = new FloatingColumn(
			new DropdownPositionPolicy(), false, false, true);
		dialog.Gap(5.px());

		// Approx 10 lines of text — TextField/Label line-height runs ~18px so we
		// pick a round 200px (handier than fiddling per-renderer line metrics).
		const float TEN_LINES_PX = 200f;

		// --- Description editor panel -------------------------------------------------
		PanelRow descPanel = dialog.AddAndReturn(new PanelRow(noBolts: true).PanelStyleHud());
		descPanel.Width(420.px());
		descPanel.Height(Px.Auto);

		TextField descEditor = new TextField()
			.Width(400.px())
			.Height(TEN_LINES_PX.px());
		descEditor.Multiline(true);
		descEditor.OnValueChanged(text =>
		{
			if (Entity == null) {
				return;
			}
			Entity.CustomDescription = string.IsNullOrEmpty(text) ? Option<string>.None : text.SomeOption();
		});
		descEditor.Observe(() => Entity)
				  .Observe(() => Entity?.CustomDescription)
				  .Do((entity, desc) =>
				  {
					  string current = entity?.CustomDescription.HasValue == true
						  ? entity.CustomDescription.Value
						  : "";
					  if (descEditor.GetText() != current)
					  {
						  descEditor.Value(new LocStrFormatted(current ?? ""));
					  }
				  });
		descPanel.BodyAdd(c => c.Padding(8), descEditor);

		// --- Module list panel --------------------------------------------------------
		PanelRow listPanel = dialog.AddAndReturn(new PanelRow(noBolts: true).PanelStyleHud());
		listPanel.Width(420.px());
		listPanel.Height(Px.Auto);

		// ScrollColumn holds the module-list Label; overflow scrolls inside the
		// panel rather than pushing the whole dialog taller.  Width matches the
		// editor and accounts for the standard 17 px scroll-bar gutter.
		ScrollColumn listScroll = new ScrollColumn();
		listScroll.Width(400.px());
		listScroll.Height(TEN_LINES_PX.px());

		Label moduleListLabel = new Label(LocStrFormatted.Empty)
			.TextOverflow(TextOverflow.Wrap);
		moduleListLabel.Observe(() => Entity)
					   .Observe(() => Entity?.Modules?.Count)
					   .Do((entity, _count) =>
					   {
						   string list = entity == null ? "" : buildModuleListLabel(entity);
						   moduleListLabel.Value(new LocStrFormatted(list));
					   });
		listScroll.Add(moduleListLabel);
		listPanel.BodyAdd(c => c.Padding(8), listScroll);

		dialog.Width(440.px());
		dialog.Height(Px.Auto);
		dialog.Open(anchor);
	}

	// Renders only the auto-appended "Modules: …" tail used by the description
	// panel.  Kept symmetric with Controller.GetFullDescription's tail half so
	// the two stay in lock-step if module display formatting ever changes.
	private static string buildModuleListLabel(Controller entity)
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.Append("Modules:");
		if (entity.Modules == null || entity.Modules.Count == 0)
		{
			sb.Append(" (none)");
		}
		else
		{
			foreach (Module m in entity.Modules)
			{
				if (m?.Prototype == null) {
					continue;
				}
				sb.Append("\n  ");
				sb.Append(m.Prototype.Symbol);
				sb.Append("  ");
				sb.Append(m.Prototype.Strings.Name.TranslatedString);
			}
		}
		return sb.ToString();
	}

	private void refresh() {
		if (Entity is null) {
			m_view.RedrawComponents();
		}
	}

	public CursorManager CursorManager { get; }
	public CursorPickingManager CursorPickingManager { get; }
	//public TerrainCursor TerrainCursor { get; }
	public EntityHighlighter EntityHighlighter { get; }
	public EntityHighlighter EntityHighlighterSelectable { get; }
	public ShortcutsManager ShortcutsManager { get; }
	public EntitySelector EntitySelectionInput { get; set; }
	public ModuleConnector OutputConnection { get; internal set; }
	public CameraController CameraController { get; }
	public VariableWindowController VariableWindowController => m_variableWindowController;
	public PlcPyCodeEditorWindowController PlcPyCodeEditorWindowController => m_plcPyCodeEditorWindowController;

	public override bool InputUpdate() {
		if (EntitySelectionInput != null) {
			//var position = TerrainCursor.Tile3f.ToVector3();
			// set line view endpoint

			if (ShortcutsManager.IsPrimaryActionDown) {
				Tile3f source = Entity.Position3f;

				Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));
				if (pickedEntity.HasValue
					&& isWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance)) {
					EntitySelectionInput.Entity = pickedEntity.Value;
					EntitySelectionInput.Refresh();
					EntitySelectionInput = null;
					return true;
				}
				m_invalidOpSound.Play();
				return true;
			}
			if (ShortcutsManager.IsSecondaryActionDown) {
				EntitySelectionInput.Refresh();
				EntitySelectionInput = null;
				return true;
			}
		}
		if (OutputConnection != null) {
			if (ShortcutsManager.IsSecondaryActionDown) {
				OutputConnection = null;
				return true;
			}
		}

		updateHighlight();
		return base.InputUpdate();
	}
	private void updateHighlight() {
		if (EntitySelectionInput != null) {
			if (!m_highlightSearched) {
				m_highlightSearched = true;

				Tile3f source = Entity.Position3f;
				Fix32 innerDistance = EntitySelectionInput.EntitySearchDistance;

				Context.EntitiesManager.GetAllEntitiesOfType<Entity>()
					.Where(e => e is IEntityWithPosition && e is IRenderedEntity)
					.Where(e => EntitySelectionInput.EntityFilter(e))
					.Where(e => isWithingDistance(source, e as IRenderedEntity, innerDistance))
					.Call(e => EntityHighlighterSelectable.Highlight(e as IRenderedEntity, ColorRgba.Green))
					.ToList();
			}

			Option<IRenderedEntity> pickedEntity = CursorPickingManager.PickEntity<IRenderedEntity>(e => EntitySelectionInput.EntityFilter((Entity)e));

			if (pickedEntity.IsNone || m_hoveredEntity != null) {
				EntityHighlighter.RemoveHighlight(m_hoveredEntity);
			}

			if (pickedEntity.HasValue) {
				m_hoveredEntity = pickedEntity.Value;
				Tile3f source = Entity.Position3f;

				if (isWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance)) {
					EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.LightBlue);
				} else {
					EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.Red);
				}
				return;
			}
		}
		if (EntitySelectionInput == null && m_hoveredEntity != null) {
			EntityHighlighter.RemoveHighlight(m_hoveredEntity);
			m_hoveredEntity = null;
		}
		if (EntitySelectionInput == null && m_highlightSearched) {
			EntityHighlighterSelectable.ClearAllHighlights();
			m_highlightSearched = false;
		}
	}

	private bool isWithingDistance(Tile3f source, IRenderedEntity target, Fix32 searchDistance) {
		if (target is IStaticEntity entity && entity.OccupiedTiles
			.Select(t => entity.Position3f.AddX(t.RelativeX).AddY(t.RelativeY))
			.FirstOrDefault(t => (source - t).Length <= searchDistance) != Tile3f.Zero) {
			return true;
		}

		return false;
	}

	private void addPreviewHighlightAll() {
		Dictionary<EntityId, IEntity> entities = new Dictionary<EntityId, IEntity>();
		foreach (var module in Entity.Modules) {
			getEntitiesOfModule(entities, module);
		}
		addPreviewHighlightOfEntities(entities.Values);
		m_showsLinks = true;
	}

	internal void AddPreviewHighlight(Module module) {
		Dictionary<EntityId, IEntity> entities = new Dictionary<EntityId, IEntity>();
		getEntitiesOfModule(entities, module);
		addPreviewHighlightOfEntities(entities.Values);
	}

	internal void ClearPreviewHighlight() {
		EntityHighlighter.ClearAllHighlights();
		clearAllLines();
		m_showsLinks = false;
	}

	private static void getEntitiesOfModule(Dictionary<EntityId, IEntity> entities, Module module) {
		foreach (var item in module.Prototype.Fields.Where(f => f is EntityField)) {
			if (module.Field.Entity<IEntity>(item.Id) is IRenderedEntity entity) {
				entities[entity.Id] = entity;
			}
		}
	}

	private void addPreviewHighlightOfEntities(IEnumerable<IEntity> entities) {
		foreach (var entity in entities) {
			EntityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);

			if (entity is Transport transport) {
				var line = m_linesFactory.CreateLine(transport.StartPosition.ToCenterVector3(), Entity.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
				line.SetTextureMode(LineTextureMode.Tile);
				m_lines.Add(line);
			} else {
				entity.HasPosition(out Tile3f position);
				var line = m_linesFactory.CreateLine(position.ToVector3(), Entity.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
				line.SetTextureMode(LineTextureMode.Tile);
				m_lines.Add(line);
			}
		}
	}

	private void clearAllLines() {
		foreach (var line in m_lines) {
			UnityEngine.Object.Destroy(line.gameObject);
		}
		m_lines.Clear();
	}
}
