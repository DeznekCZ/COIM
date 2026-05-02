using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.InputControl.Inspectors;
using System;
using Mafi.Core.Entities.Static;
using System.Linq;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Core.Syncers;
using Mafi.Unity.Ui.Library;
using ProgramableNetwork.Utils;

namespace ProgramableNetwork.Ui {
	public class EntityField : IField {
		private string id;
		private LocStr name;
		private LocStr shortDesc;
		private Func<Module, IEntity, bool> entitySelector;
		private Fix32 distance;
		private Fix64 sqrDistance;

		public EntityField(string id, Proto.Str strs, Func<Module, IEntity, bool> entitySelector, Fix32 distance) {
			this.id = id;
			this.name = strs.Name;
			this.shortDesc = strs.DescShort;
			this.entitySelector = entitySelector;
			this.distance = distance;
			this.sqrDistance = distance.ToFix64() * distance.ToFix64();
		}

		public string Id => id;

		public LocStr Name => name;
		public LocStr ShortDesc => shortDesc;
		public bool ShowInTooltip => false; // shown in connections
		public Fix32 Distance => distance;
		public Func<Module, IEntity, bool> EntitySelector => entitySelector;

		public int Size => 40;

		public string GetTooltipValue(Module module) {
			// Stored entity ref, formatted as "<TypeName> #<id>".  Same shape ModuleConnector
			// uses elsewhere when describing a target.
			Fix32 raw = module.Field[id, Fix32.Zero];
			if (raw == Fix32.Zero) {
				return "";
			}
			return raw.RawValue.ToString();
		}

		public void Validate(Module module) {
			EntityInfo entityData = null;
			// FOR ONLY NEWLY CONSTRUCTED
			module.FieldNumberData.TryRemove(Id, out _);
			if (module.StringData.TryGetValue("field__" + Id, out var value)) {
				//Log.Info("Searching for entity in module by config: " + module.Id + " with key: " + Id);
				entityData = JsonConvert.DeserializeObject<EntityInfo>(value);

				foreach (IEntity entity in module.Controller.Context.EntitiesManager.Entities) {
					if (entitySelector.Invoke(module, entity)) {
						entity.HasPosition(out Tile3f entityBlockTile);
						entity.HasPosition(out Tile2f entityTile);

						if (entityData.Z == int.MinValue) {
							if (module.Controller.Position2f - entityTile == entityData.Relative2) {
								//Log.Info("Found by definition");
								module.Field.Entity(Id, entity); // set by position
								return; // BUT position changed
							}
						} else {
							if (module.Controller.Position3f - entityBlockTile == entityData.Relative) {
								//Log.Info("Found by definition");
								module.Field.Entity(Id, entity); // set by position
								return; // BUT position changed
							}
						}
					}
				}
				//Log.Info("Not found");
			} else {
				//Log.Info("Entity is not set: " + Id + " with key: " + Id);
			}
		}

		public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog) {
			Picker picker = new Picker(module, id, entitySelector, distance, updateDialog, parentWindow, inspector);
			fieldContainer.Row(this, module, uiContext, out _).Add(picker);
		}

		public void InitData(Module module) {
			// nothing to do
		}

		public class Picker : Row {
			private readonly Func<Module, IEntity, bool> m_filter;
			private readonly Action m_refresh;
			private readonly Window m_window;
			private readonly ControllerInspector m_inspector;
			private readonly UiContext m_UiContext;
			private readonly Module m_module;
			private readonly string m_dataName;
			private readonly Fix32 m_distance;
			private readonly ButtonIcon m_selectionButton;
			private readonly DisplayWithIcon m_btnPreview;

			public Picker(Module module, string dataName, Func<Module, IEntity, bool> filter, Fix32 distance, Action refresh, Window parentWindow, ControllerInspector inspector)
				: base() {
				m_filter = filter;
				m_refresh = refresh;
				m_window = parentWindow;
				m_inspector = inspector;
				m_UiContext = inspector.Context;
				m_module = module;
				m_dataName = dataName;
				m_distance = distance;
				parentWindow.OnCloseStart += ParentWindow_OnCloseStart;

				m_btnPreview = new DisplayWithIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
				m_btnPreview.Icon.Margin(0);
				m_btnPreview.Icon.Size(Sizes.IMAGE_SIZE * 1.5f, Sizes.IMAGE_SIZE * 1.5f);
				m_btnPreview.Icon.Padding(0);
				m_btnPreview.Margin(0);
				m_btnPreview.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE * 1.5f);
				m_btnPreview.OnClick(() => {
					Entity entity = m_module.Field.Entity<Entity>(m_dataName);
					if (entity.HasPosition(out Tile2f position)) {
						m_UiContext.CameraController.PanTo(position);
					}
				});
				Add(m_btnPreview);

				m_selectionButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Edit_svg);
				m_selectionButton.Height(Sizes.BLOCK_SIZE * 1.5f);
				m_selectionButton.OnClick(PickEntity);
				Add(m_selectionButton);

				m_btnPreview.OnMouseEnterLeave(
					() => {
						Entity entity = m_module.Field.Entity<Entity>(m_dataName);
						if (entity is null) {
							return;
						}
						m_UiContext.Highlighter.Highlight(
							(IRenderedEntity)entity, ColorRgba.LightBlue);
					},
					() => {
						Entity entity = m_module.Field.Entity<Entity>(m_dataName);
						if (entity is null) {
							return;
						}
						m_UiContext.Highlighter.RemoveHighlight(
							(IRenderedEntity)entity);
					});
				m_selectionButton.OnMouseEnterLeave(
					() => {
						Entity entity = m_module.Field.Entity<Entity>(m_dataName);
						if (entity is null) {
							return;
						}
						m_UiContext.Highlighter.Highlight(
							(IRenderedEntity)entity, ColorRgba.LightBlue);
					},
					() => {
						Entity entity = m_module.Field.Entity<Entity>(m_dataName);
						if (entity is null) {
							return;
						}
						m_UiContext.Highlighter.RemoveHighlight(
							(IRenderedEntity)entity);
					});

				m_inspector.EntitySelectionInput = null;

				this.Observe(() => m_module.Field[m_dataName])
					.Do((item) => Refresh());
			}

			private void ParentWindow_OnCloseStart(Window obj) {
				m_inspector.EntitySelectionInput = null;
			}

			protected override void OnDetached() {
				m_window.OnCloseStart -= ParentWindow_OnCloseStart;
				base.OnDetached();
			}

			private void PickEntity() {
				m_selectionButton.Class(Cls.selected);
				m_inspector.EntitySelectionInput = new EntitySelector(m_module, m_distance, m_refresh, m_filter,
					(entity) => {
						m_selectionButton.ClassRemove(Cls.selected);
						m_UiContext.InputScheduler.ScheduleInputCmd(new ModuleSetEntityFieldCmd(
							m_module.Controller.Id, m_module.Id, m_dataName, entity?.Id));
						m_refresh();
					});
			}

			private void Refresh() {
				Entity entity = m_module.Field.Entity<Entity>(m_dataName);
				if (entity is null) {
					m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
				} else {
					m_btnPreview.Icon.Value(entity.GetIcon());
				}
			}
		}
	}
}