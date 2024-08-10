

// COIExtended, Version=0.7.9.0, Culture=neutral, PublicKeyToken=null
// COIExtended.UI.Entities.EntityInspector<TEntity,TView>
using System.Collections.Generic;
using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static;
using Mafi.Core.Input;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UserInterface;

namespace Mafi.Unity
{

	public abstract class EntityInspector<TEntity, TView> : IUnityUi, IEntityInspector<TEntity>, IEntityInspector, IEntityInspectorFactory<TEntity>, IFactory<TEntity, IEntityInspector> where TEntity : class, IEntity where TView : ItemDetailWindowView
	{
		private Option<BuildingsAssigner> m_buildingsAssigner;

		private bool m_restoreHighlightAfterUpgrade;

		private Option<IStaticEntityWithReservedOcean> m_activatedOverlayEntity;

		private readonly Lyst<IRenderedEntity> m_secondaryHighlightedEntities;

		protected TView WindowView;

		private TEntity m_selectedEntity;

		public IInputScheduler InputScheduler => Context.InputScheduler;

		protected IUnityInputMgr InputMgr => Context.InputMgr;

		protected InspectorController Controller => Context.MainController;

		public TEntity SelectedEntity => m_selectedEntity;

		public InspectorContext Context { get; }

		public virtual bool DeactivateOnNonUiClick => true;

		protected EntityInspector(InspectorContext context)
		{
			m_secondaryHighlightedEntities = new Lyst<IRenderedEntity>();
			Context = context;
		}

		public virtual void RegisterUi(UiBuilder builder)
		{
			WindowView = GetView();
			WindowView.SetOnCloseButtonClickAction(delegate
			{
				InputMgr.DeactivateController(Controller);
			});
			WindowView.BuildUi(builder);
			if (m_buildingsAssigner.HasValue)
			{
				WindowView.AddUpdater(m_buildingsAssigner.Value.Updater);
			}
		}

		public IEntityInspector Create(TEntity entity)
		{
			m_selectedEntity = entity.CheckNotNull();
			return this;
		}

		public virtual void SyncUpdate(GameTime gameTime)
		{
			if (SelectedEntity.IsDestroyed)
			{
				//ForceDeactivate();
			}
			else
			{
				WindowView.SyncUpdate(gameTime);
			}
		}

		public virtual void RenderUpdate(GameTime gameTime)
		{
			WindowView.RenderUpdate(gameTime);
		}

		public virtual bool InputUpdate(IInputScheduler inputScheduler)
		{
			if (m_buildingsAssigner.IsNone)
			{
				return false;
			}
			return m_buildingsAssigner.Value.InputUpdate(inputScheduler);
		}

		public T ScheduleInputCmd<T>(T cmd) where T : IInputCommand
		{
			return InputScheduler.ScheduleInputCmd(cmd);
		}

		public void Clear()
		{
			if (SelectedEntity is StaticEntity)
			{
				WindowView.RenderUpdate(new GameTime());
			}
		}

		protected abstract TView GetView();

		protected virtual void OnActivated()
		{
			if (SelectedEntity is IStaticEntityWithReservedOcean entity)
			{
				m_activatedOverlayEntity = Context.OceanOverlayRenderer.ActivateForSingleEntity(entity);
			}
		}

		protected virtual void OnDeactivated()
		{
			if (m_activatedOverlayEntity.HasValue)
			{
				m_activatedOverlayEntity = Context.OceanOverlayRenderer.DeactivateForSingleEntity(m_activatedOverlayEntity);
			}
		}

		public void Activate()
		{
			if (m_buildingsAssigner.HasValue)
			{
				m_buildingsAssigner.Value.SetEntity(SelectedEntity);
			}
			//Context.Highlighter.Highlight(SelectedEntity, ColorRgba.Yellow);
			OnActivated();
			WindowView.Show();
		}

		public void Deactivate()
		{
			if (m_buildingsAssigner.HasValue)
			{
				m_buildingsAssigner.Value.DeactivateTool();
				Controller.SetHoverCursorSuppression(isSuppressed: false);
			}
			WindowView.Hide();
			OnDeactivated();
			//Context.Highlighter.RemoveHighlight(SelectedEntity);
			//RemoveSecondaryHighlight();
			m_selectedEntity = null;
		}
	}
}