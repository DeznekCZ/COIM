using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Input;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using UnityEngine.UIElements;
using static Mafi.Unity.Assets.Unity;
using Button = Mafi.Unity.UiToolkit.Library.Button;
using Column = Mafi.Unity.UiToolkit.Library.Column;
using Label = Mafi.Unity.UiToolkit.Library.Label;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Side panel listing every module's <see cref="EntityField"/> slots.
	/// Each row exposes a 36×36 entity selector icon with three actions:
	///   left-click: pick / change entity, shift+click: apply the last picked entity to
	///   this slot (if it passes the slot's filter and distance check), right-click: clear.
	/// Hovering a row highlights the bound world entity and the corresponding module box
	/// in the controller view.
	/// </summary>
	public class EntityConnectionsView : Row
	{
		// Most recently picked entity across any slot; survives panel rebuilds and inspector re-opens.
		private static IEntity s_lastPickedEntity;

		private readonly ControllerInspector m_inspector;
		private readonly Column m_childrenContainer;

		public EntityConnectionsView(ControllerInspector inspector)
			: base()
		{
			m_inspector = inspector;

			this.m_childrenContainer = this.AddAndReturn(new ScrollColumn())
				.Fill()
				.AddAndReturn(new Column())
				.Width(58.px())
				//.Class(Cls.group)
				.Padding(5.px())
				.Margin(2.px())
				.Gap(4.px());

			this.Observe(() => inspector.Entity)
				.ObserveIndexable(() => inspector.Entity?.Modules)
				.Do((entity, modules) => Rebuild());
		}

		private void Rebuild()
		{
			m_childrenContainer.Clear();

			Controller controller = m_inspector.Entity;
			if (controller == null) {
				return;
			}

			bool anyRow = false;
			// Flat list — every (module, field) entity slot becomes one icon stacked
			// vertically.  Identification of which module owns each icon happens through
			// the per-icon floater (which prepends the module symbol above the field name).
			foreach (Module module in controller.Modules)
			{
				foreach (EntityField field in module.Prototype.Fields.OfType<EntityField>())
				{
					m_childrenContainer.Add(BuildIcon(module, field));
					anyRow = true;
				}
			}

			if (!anyRow) {
				m_childrenContainer.AddAndReturn(new Label()
					.LaterText(() => Tr.None, this)
					.TextAlign(TextAlignment.CenterMiddle));
			}
		}

		private ButtonIcon BuildIcon(Module module, EntityField field)
		{
			ButtonIcon icon = new ButtonIcon(Button.IconOnly, UserInterface.General.Empty128_png);
			icon.IconSize(36.px(), 36.px());

			// Mirror the icon's hover state when the source module on the grid is hovered,
			// so the cross-direction highlight matches what direct icon hover already shows.
			Module rowModule = module;
			icon.Observe(() => m_inspector.HoveredModuleGraphic)
				.Do(hovered => icon.Border(
					all: 2.px(),
					radius: 4,
					color: hovered != null && hovered.Id == rowModule.Id
						? ColorRgba.CornflowerBlue
						: ColorRgba.CornflowerBlue.SetA(0)));

			// Live preview of the assigned entity icon.
			icon.Observe(() => module.Field[field.Id])
				.Do(_ => {
					IEntity entity = module.Field.Entity<Entity>(field.Id);
					icon.Icon.Value(entity?.GetIcon() ?? UserInterface.General.Empty128_png);
				});

			// Hover: highlight the world entity (cable color) + the module box on the grid.
			icon.OnMouseEnterLeave(
				() => {
					m_inspector.HighlightedFromSidePanel = module;
					m_inspector.AddPreviewHighlight(module);
				},
				() => {
					if (m_inspector.HighlightedFromSidePanel == module) {
						m_inspector.HighlightedFromSidePanel = null;
					}
					m_inspector.ClearPreviewHighlight();
				});

			// All three buttons handled via the raw MouseUpEvent so we can read evt.shiftKey
			// for the modifier and evt.button to discriminate left/middle/right uniformly,
			// without polling UnityEngine.Input or relying on Clickable activator filters.
			icon.RegisterCallback<UnityEngine.UIElements.MouseUpEvent>(evt => {
				switch (evt.button)
				{
					case 0: // left
						if (evt.shiftKey) {
							ApplyRememberedToSlot(module, field);
						} else {
							PickEntity(module, field);
						}
						evt.StopPropagation();
						break;
					case 1: // right — clear
						m_inspector.Context.InputScheduler.ScheduleInputCmd(
							new ModuleClearFieldCmd(module.Controller.Id, module.Id, field.Id));
						evt.StopPropagation();
						break;
					case 2: // middle — pan camera to bound entity
						IEntity bound = module.Field.Entity<Entity>(field.Id);
						if (bound != null && bound.HasPosition(out Tile2f position)) {
							m_inspector.Context.CameraController.PanTo(position);
						}
						evt.StopPropagation();
						break;
				}
			});

			// Hover floater — first lines identify the module + field, then the action hints.
			icon.Floater(() => BuildHintFloater(module, field));

			return icon;
		}

		private Option<UiComponent> BuildHintFloater(Module module, EntityField field)
		{
			Column column = new Column(gap: 2.pt());
			column.Add(new Label($"{module.Prototype.Symbol} : {field.Name}".AsLoc()).Class(Cls.window__title));
			column.Add(new HorizontalDivider());
			column.Add(new Row(gap: 2.pt())
			{
				new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
				new Label().LaterText(() => NewTr.Inspector.ConnectionsHintEdit, column).TinyFontSize().FlexGrow(0.8f)
			});
			column.Add(new Row(gap: 2.pt())
			{
				new Label().LaterText(() => NewTr.Inspector.Shift, column).Class(Cls.window__title).TinyFontSize().FlexGrow(0.2f),
				new Label("+".AsLoc()).TinyFontSize().TextAlign(TextAlignment.CenterMiddle),
				new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
				new Label().LaterText(() => NewTr.Inspector.ConnectionsHintCopyNext, column).TinyFontSize().FlexGrow(0.8f)
			});
			column.Add(new Row(gap: 2.pt())
			{
				new Icon(UserInterface.General.MouseScroll_svg).FlexGrow(0.2f),
				new Label().LaterText(() => NewTr.Inspector.ConnectionsHintPan, column).TinyFontSize().FlexGrow(0.8f)
			});
			column.Add(new Row(gap: 2.pt())
			{
				new Icon(UserInterface.General.RightClick128_png).FlexGrow(0.2f),
				new Label().LaterText(() => NewTr.Inspector.ConnectionsHintClear, column).TinyFontSize().FlexGrow(0.8f)
			});
			return column;
		}

		private void PickEntity(Module module, EntityField field)
		{
			m_inspector.EntitySelectionInput = new EntitySelector(
				module,
				field.Distance,
				() => { },
				field.EntitySelector,
				(entity) => {
					if (entity != null) {
						s_lastPickedEntity = entity;
					}
					m_inspector.Context.InputScheduler.ScheduleInputCmd(
						new ModuleSetEntityFieldCmd(module.Controller.Id, module.Id, field.Id, entity?.Id));
				});
		}

		/// <summary>
		/// Apply the most recently picked entity (across any slot, including in earlier
		/// inspector sessions during this game run) to the target slot, but only if it
		/// passes the slot's filter and distance check.  Otherwise plays an InvalidOp tone.
		/// </summary>
		private void ApplyRememberedToSlot(Module module, EntityField field)
		{
			IEntity remembered = s_lastPickedEntity;
			if (remembered == null
				|| !field.EntitySelector(module, remembered)
				|| !IsWithinDistance(module.Controller.Position3f, remembered, field.Distance))
			{
				m_inspector.Context.AudioDb.InvalidOp(true).Play();
				return;
			}

			m_inspector.Context.InputScheduler.ScheduleInputCmd(
				new ModuleSetEntityFieldCmd(module.Controller.Id, module.Id, field.Id, remembered.Id));
		}

		private static bool IsWithinDistance(Tile3f source, IEntity entity, Fix32 distance)
		{
			if (entity is IStaticEntity staticEntity)
			{
				return staticEntity.OccupiedTiles
					.Select(t => staticEntity.Position3f.AddX(t.RelativeX).AddY(t.RelativeY))
					.Any(t => (source - t).Length <= distance);
			}
			if (entity.HasPosition(out Tile3f tile))
			{
				return (source - tile).Length <= distance;
			}
			return false;
		}
	}
}
