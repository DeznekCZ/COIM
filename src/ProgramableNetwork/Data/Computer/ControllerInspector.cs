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
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System.Collections.Generic;
using System.Linq;
using Mafi.Core.Console;
using Mafi.Core.Research;
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

	// TODO
	public ModuleConnector m_higlightedOutput;
	public ModuleConnector m_higlightedInput;
	public bool m_showsLinks;
	public IGameConsole m_console;

	public ResearchManager ResearchManager { get; }

	public ControllerInspector(
		UiContext context,
		CursorManager cursorManager,
		CursorPickingManager cursorPickingManager,
		ShortcutsManager shortcutsManager,
		//TerrainCursor terrainCursor,
		CameraController cameraController,
		VariableWindowController variableWindowController,
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

		ProgressBar bar;
		AddPanelRow(
				new Label()
					.LaterText(() => NewTr.Inspector.ComputingSpeed, this)
					.TextAlign(TextAlignment.LeftMiddle),
				new UiComponent().Fill(),
				bar = new ProgressBar()
					.HeightAuto()
					.Width(150),
				new Display()
					.Value(0)
					.Width(150)
					.LaterText<Display>(() => NewTr.Inspector.ComputingSpeedTooltip, this, (d, v) => d.Tooltip(v))
					.ObserveValue(() => $"{(600 / (1f + Entity.Speed)).ToFix32().ToStringRounded(0)} t/m"),
				new ButtonText("-".AsLoc())
					.TextAlign(TextAlignment.CenterMiddle)
					.Width(50)
					.OnClick(() => Entity.Speed++)
					.ObserveEnabled(() => Entity.Speed < 29),
				new ButtonText("+".AsLoc())
					.TextAlign(TextAlignment.CenterMiddle)
					.Width(50)
					.OnClick(() => Entity.Speed--)
					.ObserveEnabled(() => Entity.Speed > 0)
			)
			.BodyGap(5.px());

		bar.ObserveVisibleForRender(() => Entity.Speed >= 10)
			.Observe(() => Entity.Speed)
			.Observe(() => Entity.Clock)
			.Observe(() => Entity.IsEnabled)
			.Do((speed, clock, enabled) => {
				bar.Color(enabled ? ColorRgba.GreenYellow : ColorRgba.DarkYellow);
				if (speed == clock) {
					bar.Value(Percent.Hundred);
				}
				else {
					bar.ValueFromRatio(clock, speed);
				}
			});

		// UI
		m_modulesPanel = AddPanelWithHeader();
		m_modulesPanel.Header.Add(
			new Label()
				.LaterText(() => NewTr.Inspector.Modules, this)
				.FlexGrow(1)
				.TextAlign(TextAlignment.CenterMiddle)
			);
		m_modulesPanel.Add(m_view = new ControllerView(this, refresh).AlignSelfCenter());

		HeaderButtons.AddAndReturn(new ButtonIcon(Button.Header, UserInterface.General.Connect128_png))
			.OnClick(() => GlobalDependencyResolver.Get<ConnectionInfo>().Open(context.UiRoot))
			.OnMouseEnterLeave(addPreviewHighlightAll, ClearPreviewHighlight);

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

		EmbedStatusToTheTop();

		this.Observe(() => Entity)
			.Observe(() => Entity?.Modules)
			.Observe(() => Entity?.Rows)
			.Do((entity, module, rows) => refresh());

		this.Observe(() => Entity?.State)
			.Do((state) => {
				Status.As(state ?? Tr.EntityStatus__Working, DisplayState.Positive);
			});
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
