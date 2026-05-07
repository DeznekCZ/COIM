using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Data.Antene;
using UnityEngine;
using Display = Mafi.Unity.Ui.Library.Display;

namespace ProgramableNetwork.Ui
{
	[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
	public class AntenaInspector : BaseInspector<Antena>, ISelectionInspector<Antena, AntenaSelector, Antena>
	{
		private readonly AudioSource m_invalidOpSound;
		private PanelWithHeader m_bandPanel;
		private bool m_highlightSearched;
		private IRenderedEntity m_hoveredEntity;
		private DataBandProto[] m_databands;
		private bool m_onLoading;
		private PanelWithHeader m_signalPanel;
		private ScrollColumn m_signalList;
		private Antena m_oldEntity;
		private PanelWithHeader m_fmSignalPanel;
		private ScrollColumn m_fmSignalList;
		private readonly FMManager m_fmManager;

		public AntenaInspector(
			UiContext context,
			CursorManager cursorManager,
			CursorPickingManager cursorPickingManager,
			ShortcutsManager shortcutsManager,
			CameraController cameraController,
			FMManager fmManager,
			//TerrainCursor terrainCursor,
			NewInstanceOf<EntityHighlighter> entityHighlighter,
			NewInstanceOf<EntityHighlighter> entityHighlighterSelectable
			) : base(context)
		{
			CursorManager = cursorManager;
			CursorPickingManager = cursorPickingManager;
			//TerrainCursor = terrainCursor;
			EntityHighlighter = entityHighlighter.Instance;
			EntityHighlighterSelectable = entityHighlighterSelectable.Instance;
			ShortcutsManager = shortcutsManager;
			CameraController = cameraController;
			m_invalidOpSound = Context.AudioDb.InvalidOp();
			m_fmManager = fmManager;

			AddBandDisplay();
		}

		public CursorManager CursorManager { get; }
		public CursorPickingManager CursorPickingManager { get; }
		//public TerrainCursor TerrainCursor { get; }
		public EntityHighlighter EntityHighlighter { get; }
		public EntityHighlighter EntityHighlighterSelectable { get; }
		public ShortcutsManager ShortcutsManager { get; }
		public CameraController CameraController { get; }
		public AntenaSelector EntitySelectionInput { get; set; }

		public override bool InputUpdate()
		{
			if (EntitySelectionInput != null)
			{
				//var position = TerrainCursor.Tile3f.ToVector3();
				// set line view endpoint

				if (ShortcutsManager.IsPrimaryActionDown)
				{
					Tile3f source = Entity.Position3f;

					Option<Antena> pickedEntity = CursorPickingManager.PickEntity<Antena>(e => EntitySelectionInput.EntityFilter(e));
					if (pickedEntity.HasValue
						&& IsWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance))
					{
						EntitySelectionInput.Entity = pickedEntity.Value;
						EntitySelectionInput.Refresh();
						EntitySelectionInput = null;
						return true;
					}
					m_invalidOpSound.Play();
					return true;
				}
				if (ShortcutsManager.IsSecondaryActionDown)
				{
					EntitySelectionInput.Refresh();
					EntitySelectionInput = null;
					return true;
				}
			}
			return base.InputUpdate();
		}

		protected override void SyncUpdate(GameTime gameTime)
		{
			base.SyncUpdate(gameTime);

			if (EntitySelectionInput != null)
			{
				if (!m_highlightSearched)
				{
					m_highlightSearched = true;
			
					Tile3f source = Entity.Position3f;
					Fix32 innerDistance = EntitySelectionInput.EntitySearchDistance;
			
					Context.EntitiesManager.GetAllEntitiesOfType<Antena>()
						.Where(e => e is IEntityWithPosition && e is IRenderedEntity)
						.Where(e => EntitySelectionInput.EntityFilter(e))
						.Where(e => IsWithingDistance(source, e, innerDistance))
						.Call(e => EntityHighlighterSelectable.Highlight(e, ColorRgba.Green))
						.ToList();
				}
			
				Option<Antena> pickedEntity = CursorPickingManager.PickEntity<Antena>(e => EntitySelectionInput.EntityFilter(e));
			
				if (pickedEntity.IsNone || m_hoveredEntity != null)
				{
					EntityHighlighter.RemoveHighlight(m_hoveredEntity);
				}
			
				if (pickedEntity.HasValue)
				{
					m_hoveredEntity = pickedEntity.Value;
					Tile3f source = Entity.Position3f;
			
					if (IsWithingDistance(source, pickedEntity.Value, EntitySelectionInput.EntitySearchDistance))
					{
						EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.LightBlue);
					}
					else
					{
						EntityHighlighter.HighlightOnly(m_hoveredEntity, ColorRgba.Red);
					}
					return;
				}
			}
			if (EntitySelectionInput == null && m_hoveredEntity != null)
			{
				EntityHighlighter.RemoveHighlight(m_hoveredEntity);
				m_hoveredEntity = null;
			}
			if (EntitySelectionInput == null && m_highlightSearched)
			{
				EntityHighlighterSelectable.ClearAllHighlights();
				m_highlightSearched = false;
			}
		}

		private bool IsWithingDistance(Tile3f source, IRenderedEntity target, Fix32 searchDistance)
		{
			if (target is IStaticEntity entity && entity.OccupiedTiles
				.Select(t => entity.Position3f.AddX(t.RelativeX).AddY(t.RelativeY))
				.FirstOrDefault(t => (source - t).Length <= searchDistance) != Tile3f.Zero) {
				return true;
			}

			return false;
		}

		protected override void OnDeactivated()
		{
			base.OnDeactivated();
			//EntitySelectionInput = null;
		}

		private void AddBandDisplay()
		{
			m_bandPanel = AddPanelWithHeader();
			m_bandPanel.Header.Add(new Label(new Mafi.Localization.LocStrFormatted("Bands")));

			TabContainer tabContainer = new TabContainer();
			m_bandPanel.Add(tabContainer);
			m_databands = Context.ProtosDb.All<DataBandProto>().ToArray();

			// FM signal panel
			m_fmSignalPanel = AddPanelWithHeader();
			m_fmSignalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
			m_fmSignalPanel.Header.Add(new Label("Received signals".ToDoLoc()));
			m_fmSignalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
			m_fmSignalPanel.ObserveVisible(this, () => Entity.DataBand is FMDataBand);
			m_fmSignalList = m_fmSignalPanel.Body.AddAndReturn(new ScrollColumn());
			m_fmSignalList.Gap(5.px());
			m_fmSignalList.MaxHeight(400.px());
			m_fmSignalList.MinHeight(100.px());

			// Not very efficient to observe the whole list every time,
			// but FMDataBand doesn't have that many channels and this
			// is simpler to implement than observing each channel separately
			m_fmSignalPanel.ObserveEnumerable(() => m_fmManager.Signals(Entity.CenterTile))
				.Do(entries => {
					// Create a list of all channels currently displayed
					if (m_fmSignalList.ChildrenCount != entries.Count) {
						if (m_fmSignalList.ChildrenCount > entries.Count) {
							for (int i = m_fmSignalList.ChildrenCount - 1; i >= entries.Count; i--) {
								FMDataBandChannelEntry entry = (FMDataBandChannelEntry)m_fmSignalList[i];
								entry.RemoveFromHierarchy();
							}
						} else if (m_fmSignalList.ChildrenCount < entries.Count) {
							for (int i = m_fmSignalList.ChildrenCount; i < entries.Count; i++) {
								m_fmSignalList.AddCached<FMDataBandChannelEntry>()
									.SetCameraController(Context.CameraController)
									.SetHightligter(EntityHighlighter);
							}
						}
					}

					// Update all entries with current data
					for (int i = 0; i < entries.Count; i++) {
						FMDataBandChannelEntry entry = (FMDataBandChannelEntry)m_fmSignalList[i];
						KeyValuePair<int, (Fix32, FMDataBandChannel)> signal = entries[i];
						entry.DataBand(signal.Value.Item2)
							.Channel(signal.Key)
							.Strength(signal.Value.Item1);
					}
				});

			// Redirections and AM signals panel
			m_signalPanel = AddPanelWithHeader();
			m_signalPanel.Collapsed(true);
			m_signalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
			m_signalPanel.Header.Add(new Label(new Mafi.Localization.LocStrFormatted("Redirected signals")));
			m_signalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
			m_signalPanel.Header.AddAndReturn(new ButtonText(new Mafi.Localization.LocStrFormatted("+")))
				.OnClick(() => {
					Context.InputScheduler.ScheduleInputCmd(new AntenaCreateRedirectedChannelCmd(Entity.Id));
					m_signalPanel.Collapsed(false);
					RefreshRedirections(Entity.DataBand);
				})
				.Height(Sizes.BLOCK_SIZE);
			m_signalList = m_signalPanel.Body.AddAndReturn(new ScrollColumn());
			m_signalList.Gap(5.px());
			m_signalList.MaxHeight(400.px());
			m_signalList.MinHeight(100.px());

			foreach (DataBandProto item in m_databands)
			{
				tabContainer.AddTab(item.Strings.Name, GetTabContent(item), iconAssetPath: null);
			}

			m_oldEntity = Entity;
			this.Observe(() => Entity)
				.Observe(() => Entity?.Prototype)
				.DoOnSync((antena, proto) =>
				{
					if (m_oldEntity != null) {
						m_oldEntity.Selected = false;
					}

					if (antena == null)
					{
						m_oldEntity = null;
						return;
					}

					m_oldEntity = antena;
					antena.Selected = true;

					m_signalPanel.Collapsed(antena.DataBand?.Channels?.Count() == 0);

					for (int i = 0; i < m_databands.Length; i++)
					{
						if (antena.DataBand?.Prototype == m_databands[i])
						{
							m_onLoading = true;
							tabContainer.SwitchToTab(i);
							m_onLoading = false;
							return;
						}
					}

					m_signalList.Clear();
					if (antena.DataBand == null) {
						return;
					}
					RefreshRedirections(antena.DataBand);
				});

			m_onLoading = true;
			tabContainer.OnTabActivate(() =>
			{
				if (tabContainer.ActiveTabIndex is null) {
					return;
				}

				if (!m_onLoading)
				{
					Context.InputScheduler.ScheduleInputCmd(new AntenaSetDataBandCmd(
						Entity.Id, m_databands[tabContainer.ActiveTabIndex ?? 0].Id));
				}

				RefreshRedirections(Entity.DataBand);
			});
			m_onLoading = false;
		}

		protected override void OnDetached()
		{
			base.OnDetached();
			if (m_oldEntity != null)
			{
				m_oldEntity.Selected = false;
				m_oldEntity = null;
			}
		}

		private void RefreshRedirections(IDataBand databand)
		{
			m_signalList.Clear();

			foreach (var channel in databand.Channels)
			{
				m_signalList.Add(databand.Prototype.Buttons(this, channel));
			}
		}

		private UiComponent GetTabContent(DataBandProto item)
		{
			return new Label(item.Strings.DescShort);
		}
	}

	public class FMDataBandChannelEntry : Row {

		private FMDataBandChannel m_dataBandChannel;
		private readonly Display m_channel;
		private readonly Display m_id3;
		private readonly Display m_strength;
		private readonly Display m_dataCount;
		private readonly ButtonIcon m_gotoButton;

		private CameraController m_cameraController;
		private EntityHighlighter m_highlighter;

		public FMDataBandChannelEntry() {

			this.Height(24.px());
			m_strength = AddAndReturn(new Display(".....".AsLoc())).Width(48.px());
			m_strength.TextCenterMiddle();
			m_channel = AddAndReturn(new Display("85.5 MHz".AsLoc())).Width(96.px());
			m_id3 = AddAndReturn(new Display("ID".AsLoc())).Fill();
			m_id3.TextLeftMiddle();
			m_id3.ObserveValue(() => 
				(m_dataBandChannel?.Id3?.IsNullOrEmpty() ?? true ? "N/A" : m_dataBandChannel.Id3)
				.AsLoc());
			m_dataCount = AddAndReturn(new Display("0".AsLoc())).Width(24.px());
			m_dataCount.ObserveValue(() => m_dataBandChannel?.Count ?? 0);
			m_gotoButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Search_svg)
				.Height(24.px())
				.OnClick(panToSourceAntenna);
			m_gotoButton.OnMouseEnterLeave(highlight, clearHighlight);
		}

		public FMDataBandChannelEntry SetCameraController(CameraController cameraController) {
			m_cameraController = cameraController;
			return this;
		}

		private void panToSourceAntenna() {
			m_cameraController.PanTo(m_dataBandChannel.Antena.Position2f);
		}

		public FMDataBandChannelEntry SetHightligter(EntityHighlighter highlighter) {
			m_highlighter = highlighter;
			return this;
		}

		private void highlight() {
			m_highlighter.HighlightOnly(m_dataBandChannel.Antena, ColorRgba.Cyan);
		}

		private void clearHighlight() {
			m_highlighter.ClearAllHighlights();
		}

		public FMDataBandChannelEntry DataBand(FMDataBandChannel channel) {
			m_dataBandChannel = channel;
			return this;
		}

		public FMDataBandChannelEntry Channel(int signalKey) {
			m_channel.Value((((171 + signalKey).ToFix32() * 0.5f.ToFix32()).ToStringRounded(1) + " MHz").AsLoc());
			return this;
		}

		public FMDataBandChannelEntry Strength(Fix32 signalStrength) {
			int signalLevel = (signalStrength * 5).IntegerPart;
			m_strength.Value((new string('|', signalLevel)
				+ new string('.', 5 - signalLevel)).AsLoc());
			
			ColorRgba color = signalLevel switch {
				0 => ColorRgba.Red,
				1 => ColorRgba.FromHex("FF4500"), // OrangeRed
				2 => ColorRgba.Orange,
				3 => ColorRgba.Yellow,
				4 => ColorRgba.GreenYellow,
				5 => ColorRgba.Green,
				_ => ColorRgba.White
			};
			m_strength.TextColor(color);
			return this;
		}

		protected override void OnDetached() {
			base.OnDetached();
			m_highlighter.ClearAllHighlights();
		}
	}
}
