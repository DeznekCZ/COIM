using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Input;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.Audio;
using Mafi.Unity.Camera;
using Mafi.Unity.Entities;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library.Inspectors;
using Mafi.Unity.UiStatic.Cursors;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace ProgramableNetwork
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

        public AntenaInspector(
            UiContext context,
            CursorManager cursorManager,
            CursorPickingManager cursorPickingManager,
            ShortcutsManager shortcutsManager,
            CameraController cameraController,
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
                .FirstOrDefault(t => (source - t).Length <= searchDistance) != Tile3f.Zero)
                return true;

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

            m_signalPanel = AddPanelWithHeader();
            m_signalPanel.Collapsed(true);
            m_signalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
            m_signalPanel.Header.Add(new Label(new Mafi.Localization.LocStrFormatted("Redirected signals")));
            m_signalPanel.Header.Add(new UiComponent().FlexGrow(1)); // filler
            m_signalPanel.Header.AddAndReturn(new ButtonText(new Mafi.Localization.LocStrFormatted("+")))
                .OnClick(() => {
                    Entity.DataBand.CreateChannel();
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

            this.Observe(() => Entity)
                .Observe(() => Entity?.Prototype)
                .DoOnSync((antena, proto) =>
                {
                    if (antena == null) return;

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
                    if (antena.DataBand == null) return;
                    RefreshRedirections(antena.DataBand);
                });

            tabContainer.OnTabActivate(() =>
            {
                if (tabContainer.ActiveTabIndex is null) return;
                if (m_onLoading) return;

                Entity.DataBand = m_databands[tabContainer.ActiveTabIndex ?? 0]
                                        .Constructor(Entity, Entity.Context, m_databands[tabContainer.ActiveTabIndex ?? 0]);

                RefreshRedirections(Entity.DataBand);
            });
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
}
