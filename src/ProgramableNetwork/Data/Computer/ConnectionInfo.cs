using Mafi;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Mafi.Unity.Entities;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Component;
using static ProgramableNetwork.NewIds;

namespace ProgramableNetwork
{
    [GlobalDependency(RegistrationMode.AsSelf)]
    public class ConnectionInfo : Window
    {
        private ScrollColumn m_scrollableStackContainer;
        private UiComponent m_stackContainer;
        private readonly Material m_movingArrowsLineMaterialShared;
        private readonly List<IDataUpdater> m_updaters;
        private readonly UiContext m_UiContext;
        private readonly EntityHighlighter m_entityHighlighter;
        private readonly LinesFactory m_linesFactory;
        private readonly List<LineMb> m_lines = new List<LineMb>();
        private readonly AudioSource m_invalidOpSound;

        public ConnectionInfo(IUnityInputMgr unityInput, UiContext UiContext, NewInstanceOf<EntityHighlighter> entityHighlighter,
            LinesFactory linesFactory, AssetsDb assetsDb)
            : base(new Mafi.Localization.LocStrFormatted("Controller menu"), addFullscreenButton: false)
        {
            m_UiContext = UiContext;
            m_movingArrowsLineMaterialShared = assetsDb.GetSharedMaterial("Assets/Core/Materials/MovingArrowsLine.mat");
            m_invalidOpSound = UiContext.AudioDb.InvalidOp();
            m_entityHighlighter = entityHighlighter.Instance;
            m_linesFactory = linesFactory;
            m_updaters = new List<IDataUpdater>();
            unityInput.RegisterGlobalShortcut(
                m => KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.W),
                () =>
                {
                    this.Open(UiContext.UiRoot);
                    return true;
                });
            OnOpenStart += VariableWindow_OnOpenStart;
            OnCloseStart += VariableWindow_OnCloseStart;

            ShortcutToShow(KeyBindings.FromPrimaryKeys(KbCategory.General, ShortcutMode.Game, KeyCode.LeftControl, KeyCode.W));

            //this.AbsolutePositionCenter();
            MakeMovable();

            UpdaterBuilder updaterBuilder = UpdaterBuilder.Start();

            Body.AddAndReturn(new Panel())
                .AddAndReturn(m_scrollableStackContainer = new ScrollColumn());

            m_stackContainer = new UiComponent();
            m_stackContainer.Size(width: 100.Percent());
            m_scrollableStackContainer.Add(m_stackContainer);
            m_scrollableStackContainer.Size(width: 100.Percent());

            WindowSize(800.px(), 600.px());

            // TODO search bar

            Refresh();

            this.Observe(() => DateTime.Now.Ticks)
                .Do((l) => m_updaters.ForEach(u => u.Update()));
        }

        private void VariableWindow_OnCloseStart(Window w)
        {
            m_UiContext.EntitiesManager.StaticEntityAdded.RemoveNonSaveable(this, EntitiesChanged);
            m_UiContext.EntitiesManager.StaticEntityRemoved.RemoveNonSaveable(this, EntitiesChanged);
            ClearAllLines();
        }

        private void ClearAllLines()
        {
            foreach (var line in m_lines)
                UnityEngine.Object.Destroy(line.gameObject);
            m_lines.Clear();
        }

        private void VariableWindow_OnOpenStart()
        {
            m_UiContext.EntitiesManager.StaticEntityAdded.AddNonSaveable(this, EntitiesChanged);
            m_UiContext.EntitiesManager.StaticEntityRemoved.AddNonSaveable(this, EntitiesChanged);
            Refresh();
        }

        private void EntitiesChanged(IStaticEntity entity)
        {
            Refresh();
        }

        private void Refresh()
        {
            //m_updaters.Clear();
            //m_controllers.ClearAndDestroyAll();

            ClearAllLines();

            var controllers = m_UiContext.EntitiesManager.GetAllEntitiesOfType<Controller>();
            foreach (var controller in controllers)
            {
                Row controllerLine = new Row();
                controllerLine.Size(width: 100.Percent());

                var controllerButton = new ButtonIcon(controller.Prototype.IconPath)
                    //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                    .OnClick(() => m_UiContext.CameraController.PanTo(controller.Position2f))
                    .OnDoubleClick(() =>
                    {
                        UiContext UiContext = GlobalDependencyResolver.Get<UiContext>();
                        if (UiContext.InspectorsManager.TryActivateFor(controller, out var inspectorController))
                            UiContext.InputMgr.ActivateNewController(inspectorController);
                        else
                            m_invalidOpSound.Play();
                    })
                    .Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2);
                controllerButton.IconSize(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE * 1.5f);

                controllerLine.Add(controllerButton);

                Grid linkcontainer = new Grid(8);
                linkcontainer.Component.FlexGrow(1);
                controllerLine.Add(linkcontainer.Component);

                List<(Module module, List<EntityField> fields)> list = controller.Modules
                    .Select(module => (
                        module,
                        module.Prototype.Fields
                            .Where(field => field is EntityField)
                            .Cast<EntityField>()
                            .ToList()
                    ))
                    .ToList();

                Dictionary<EntityId, IEntity> allEntities = new Dictionary<EntityId, IEntity>();
                foreach (var (module, fields) in list)
                {
                    foreach (EntityField field in fields)
                    {
                        IEntity entity = module.Field.Entity<IEntity>(field.Id);
                        if (entity?.HasPosition(out Tile3f _) ?? false)
                            allEntities[entity.Id] = entity;

                        m_updaters.Add(new DataUpdaterChecked<EntityId?, (Module module, EntityField field)>(
                            (c) => c.module.Field.Entity<IEntity>(c.field.Id)?.Id,
                            (c, s) => Refresh(),
                            (a, b) => a == b,
                            (module, field),
                            entity?.Id
                        ));
                    }
                }

                foreach (var entity in allEntities.Values)
                {
                    var entityButton = new ButtonIcon(entity?.Prototype is IProtoWithIcon withIcon
                                    ? withIcon.IconPath : Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
                        //.ToolTip(this, item.CustomTitle.ValueOrNull ?? item.DefaultTitle.Value)
                        .OnClick(() => m_UiContext.CameraController.PanTo(entity.HasPosition(out Tile2f position)
                                            ? position : controller.Position2f))
                        .OnDoubleClick(() =>
                        {
                            UiContext UiContext = GlobalDependencyResolver.Get<UiContext>();
                            if (UiContext.InspectorsManager.TryActivateFor(entity, out var inspector))
                                UiContext.InputMgr.ActivateNewController(inspector);
                            else
                                m_invalidOpSound.Play();
                        })
                        .Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2);
                    entityButton.IconSize(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE * 1.5f);
                    entityButton.OnMouseEnter(
                            (e) =>
                            {
                                ClearAllLines();
                                m_entityHighlighter.ClearAllHighlights();
                                m_entityHighlighter.Highlight(controller, ColorRgba.Yellow);
                                m_entityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);
                                entity.HasPosition(out Tile3f position);
                                var line = m_linesFactory.CreateLine(position.ToVector3(), controller.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                                line.SetTextureMode(LineTextureMode.Tile);
                                m_lines.Add(line);
                            }
                        );
                    entityButton.OnMouseLeave(
                            (e) =>
                            {
                                ClearAllLines();
                                m_entityHighlighter.ClearAllHighlights();
                            }
                        );
                    linkcontainer.Add( entityButton );
                }

                controllerButton
                    .OnMouseEnterLeave(
                        () =>
                        {
                            m_entityHighlighter.ClearAllHighlights();
                            m_entityHighlighter.Highlight(controller, ColorRgba.Yellow);
                            foreach (var entity in allEntities.Values)
                            {
                                entity.HasPosition(out Tile3f position);
                                var line = m_linesFactory.CreateLine(position.ToVector3(), controller.Position3f.ToVector3(), 1.5f, Color.red, m_movingArrowsLineMaterialShared);
                                line.SetTextureMode(LineTextureMode.Tile);
                                m_lines.Add(line);
                                m_entityHighlighter.Highlight(entity as IRenderedEntity, ColorRgba.CornflowerBlue);
                            }
                        },
                        () =>
                        {
                            ClearAllLines();
                            m_entityHighlighter.ClearAllHighlights();
                        }
                    );

                m_scrollableStackContainer.Add(controllerLine);
            }
        }
    }
}