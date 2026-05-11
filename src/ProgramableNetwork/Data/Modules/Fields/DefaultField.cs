using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Xml.Linq;
using Mafi.Localization;
using Mafi;
using Mafi.Core.Syncers;
using Mafi.Unity.Ui.Library;
using Mafi.Core;
using Mafi.Unity.Ui;

namespace ProgramableNetwork.Ui
{
	public static class IFieldExtensions
	{
		public static RowContainer Row(this UiComponent fieldContainer, IField entityField, Module module,
			UiContext uiContext, Px height, out bool draw, bool useFiller = true, bool directEdit = false)
		{
			// input field definition
			if (module.Prototype.Fields.Exists(i => i.Id == $"field_{entityField.Id}" && !(entityField is BooleanField)))
			{
				Panel col = new Panel();
				col.Body.Gap(5);
				fieldContainer.Add(col);

				Row rowBool = new Row();
				col.BodyAdd(rowBool);

				Label labelSettings = new Label();
				labelSettings.Value(entityField.Name);
				labelSettings.Tooltip(entityField.ShortDesc);
				labelSettings.Width(180);
				rowBool.Add(labelSettings);

				Toggle enabled = new Toggle();
				enabled.Value(module.Field.Bool[$"field_{entityField.Id}"]);
				enabled.OnValueChanged(v => {
					if (directEdit) {
						module.Field[$"field_{entityField.Id}"] = v ? Fix32.One : Fix32.Zero;
					} else {
						uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
							module.Controller.Id, module.Id, $"field_{entityField.Id}", v ? Fix32.One : Fix32.Zero));
					}
				});
				rowBool.Add(enabled);

				Display rowDisplay = new Display(NewTr.FieldStatus.None);
				rowDisplay.FlexGrow(1);
				rowDisplay.ObserveValue(() =>
				{
					if (module.Field.Bool[$"field_{entityField.Id}"]) {
						return NewTr.FieldStatus.On;
					} else if (module.InputModules.ContainsKey(entityField.Id)) {
						return NewTr.FieldStatus.Wired;
					} else {
						return NewTr.FieldStatus.None;
					}
				});
				rowBool.Add(rowDisplay);

				PanelRow row = new PanelRow(noBolts: true);
				row.Visible(module.Field.Bool[$"field_{entityField.Id}"]);
				row.ObserveVisible(enabled, () => module.Field.Bool[$"field_{entityField.Id}"]);
				col.Add(row);

				if (useFiller) {
					UiComponent filler = new UiComponent();
					filler.Width(180);
					row.BodyAdd(filler);
				}

				draw = true;
				return row;
			}
			else if (entityField is BooleanField)
			{
				Panel col = new Panel();
				col.Body.Gap(5);
				fieldContainer.Add(col);

				Row rowBool = new Row();
				col.BodyAdd(rowBool);

				Label labelSettings = new Label();
				labelSettings.Value(entityField.Name);
				labelSettings.Tooltip(entityField.ShortDesc);
				labelSettings.Width(180);
				rowBool.Add(labelSettings);

				Toggle enabled = new Toggle();
				enabled.Value(module.Field.Bool[$"field_{entityField.Id}"]);
				enabled.OnValueChanged(v => {
					if (directEdit) {
						module.Field[$"field_{entityField.Id}"] = v ? Fix32.One : Fix32.Zero;
					} else {
						uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
							module.Controller.Id, module.Id, $"field_{entityField.Id}", v ? Fix32.One : Fix32.Zero));
					}
				});
				rowBool.Add(enabled);

				Display rowDisplay = new Display(NewTr.FieldStatus.None);
				rowDisplay.FlexGrow(1);
				rowDisplay.ObserveValue(() =>
				{
					if (module.Field.Bool[$"field_{entityField.Id}"]) {
						return NewTr.FieldStatus.On;
					} else if (module.InputModules.ContainsKey(entityField.Id)) {
						return NewTr.FieldStatus.Wired;
					} else {
						return NewTr.FieldStatus.None;
					}
				});
				rowBool.Add(rowDisplay);

				Toggle active = new Toggle();
				active.Value(module.Field.Bool[entityField.Id]);
				active.ObserveEnabled(() => module.Field.Bool[$"field_{entityField.Id}"]);
				active.OnValueChanged(v => {
					if (directEdit) {
						module.Field[entityField.Id] = v ? Fix32.One : Fix32.Zero;
					} else {
						uiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
							module.Controller.Id, module.Id, entityField.Id, v ? Fix32.One : Fix32.Zero));
					}
				});
				rowBool.Add(active);

				Display activeDisplay = new Display(NewTr.FieldStatus.None);
				activeDisplay.FlexGrow(1);
				activeDisplay.ObserveState(() => !module.Field.Bool[$"field_{entityField.Id}"] ? DisplayState.Inactive : module.Field.Bool[entityField.Id] ? DisplayState.Positive : DisplayState.Danger);
				activeDisplay.ObserveValue(() =>
				{
					if (module.Field.Bool[entityField.Id]) {
						return "1".AsLoc();
					} else {
						return "0".AsLoc();
					}
				});
				rowBool.Add(activeDisplay);

				draw = false;
				return rowBool;
			}
			else
			{
				PanelRow row = new PanelRow();
				fieldContainer.Add(row);

				Label label = new Label();
				label.Value(entityField.Name);
				label.Tooltip(entityField.ShortDesc);
				label.Width(180);
				row.BodyAdd(label);

				// Hide the entire row when the field is paired with an input pin
				// that isn't currently active — keeps the settings panel free of
				// orphan threshold fields whose matching extension pin hasn't
				// been added yet.  fieldContainer is always-visible so it can act
				// as the event runner (Mafi updaters don't fire on hidden nodes).
				if (entityField.LinkedInputPinId != null)
				{
					string linkedId = entityField.LinkedInputPinId;
					row.ObserveVisible(fieldContainer, () => module.HasInput(linkedId));
				}

				draw = true;
				return row;
			}
		}

		/// <summary>
		/// With default height Sizes.BLOCK_SIZE
		/// </summary>
		/// <param name="fieldContainer"></param>
		/// <param name="entityField"></param>
		/// <returns></returns>
		public static RowContainer Row(this UiComponent fieldContainer, IField entityField, Module module, UiContext uiContext, out bool draw, bool useFiller = true, bool directEdit = false)
		{
			return fieldContainer.Row(entityField, module, uiContext, Sizes.BLOCK_SIZE, out draw, useFiller, directEdit);
		}
	}
}