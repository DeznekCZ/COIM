using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Utils;
using ProgramableNetwork.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.Ui;
using Display = Mafi.Unity.Ui.Library.Display;
using EntityId = Mafi.Core.EntityId;

namespace ProgramableNetwork.Data.Variables;

[GlobalDependency(RegistrationMode.AsSelf)]
public class VariableWindow : Window {

	public VariableWindow(ControllerContext context, VariableManager variableManager,
		UiContext uiContext, IUnityInputMgr inputMgr)
		: base("Network variables".ToDoLoc(), addFullscreenButton: true) {

		this.LaterText(() => NewTr.Inspector.NetworkVariablesTitle, this, (vv, t) => vv.Title(t));
		WindowMaxHeight(Percent.Hundred);
		MakeMovable();
		EnablePinning();

		AudioSource invalidOp = uiContext.AudioDb.InvalidOp();
		ScrollColumn scrollColumn = AddPanel().AddAndReturn(new ScrollColumn());

		this.DoOnSyncPeriodically(() => {
			Lyst<KeyValuePair<string, Fix32>> entries = variableManager.AllVariables.ToLyst();

			// Sync the number of entries with the number of variable entries in the UI
			if (scrollColumn.ChildrenCount != entries.Count) {
				if (scrollColumn.ChildrenCount > entries.Count) {
					for (int i = entries.Count; i < scrollColumn.ChildrenCount; i++) {
						VariableEntry entry = (VariableEntry)scrollColumn[i];
						entry.RemoveFromHierarchy();
					}
				} else if (scrollColumn.ChildrenCount < entries.Count) {
					for (int i = scrollColumn.ChildrenCount; i < entries.Count; i++) {
						// Factory closure injects the goto/inspector dependencies — the cache
						// reuses VariableEntry instances across refreshes so the buttons keep
						// their event subscriptions and don't churn allocations every tick.
						scrollColumn.AddCached(() => new VariableEntry(uiContext, inputMgr, invalidOp, variableManager));
					}
				}
			}

			// Update the variable entries with the current variable values
			for (int i = 0; i < entries.Count; i++) {
				VariableEntry entry = (VariableEntry)scrollColumn[i];
				KeyValuePair<string, Fix32> kvp = entries[i];
				entry.Name(kvp.Key).Value(kvp.Value);
			}
		}, Duration.OneTick);
	}
}

// TODO handle variable types other than Fix32 (e.g. bool, ProductProto, CropProto, EntityProto)
public class VariableEntry : Row {

	private readonly Label m_text;
	private readonly Display m_display;
	private readonly ButtonIcon m_gotoBtn;
	private readonly ButtonIcon m_inspectBtn;
	private readonly ButtonIcon m_removeBtn;
	private readonly UiContext m_uiContext;
	private readonly IUnityInputMgr m_inputMgr;
	private readonly AudioSource m_invalidOp;
	private readonly VariableManager m_variableManager;
	private string m_variableName;

	public VariableEntry(UiContext uiContext, IUnityInputMgr inputMgr, AudioSource invalidOp, VariableManager variableManager) {
		m_uiContext = uiContext;
		m_inputMgr = inputMgr;
		m_invalidOp = invalidOp;
		m_variableManager = variableManager;

		m_text = AddAndReturn(new Label()).FlexGrow(1);
		m_display = AddAndReturn(new Display()).Width(80.px());

		// Goto = camera pan to the controller that last wrote this variable
		// (tracked per-variable by VariableManager). Open inspector = double-click
		// pattern from ConnectionInfo: resolve the inspector for the writer and
		// activate it.  Buttons grey out when the writer entity is missing — the
		// remove button is the inverse, only available once the writer is gone
		// so saved variables can be cleaned up but a live producer can't be
		// silently removed from under itself.
		m_gotoBtn = AddAndReturn(new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.GoTo_svg));
		m_gotoBtn.LaterText<ButtonIcon>(() => NewTr.Inspector.VariablePanToWriter, this, (b, v) => b.Tooltip(v));
		m_gotoBtn.OnClick(panToWriter);
		m_gotoBtn.OnMouseEnterLeave(highlightWriter, removeWriterHighlight);
		m_gotoBtn.ObserveEnabled(() => findWriter() != null);

		m_inspectBtn = AddAndReturn(new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Edit_svg));
		m_inspectBtn.LaterText<ButtonIcon>(() => NewTr.Inspector.VariableOpenInspector, this, (b, v) => b.Tooltip(v));
		m_inspectBtn.OnClick(openWriterInspector);
		m_inspectBtn.OnMouseEnterLeave(highlightWriter, removeWriterHighlight);
		m_inspectBtn.ObserveEnabled(() => findWriter() != null);

		m_removeBtn = AddAndReturn(new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png));
		m_removeBtn.LaterText<ButtonIcon>(() => NewTr.Inspector.VariableRemove, this, (b, v) => b.Tooltip(v));
		m_removeBtn.OnClick(removeVariable);

		this.Margin(5);
	}

	private Controller findWriter() {
		if (string.IsNullOrEmpty(m_variableName)) {
			return null;
		}
		EntityId writerId = m_variableManager.GetVariableWriter(m_variableName);
		if (!writerId.IsValid) {
			return null;
		}
		return m_uiContext.EntitiesManager.TryGetEntity<Controller>(writerId, out Controller writer)
			? writer
			: null;
	}

	private void highlightWriter() {
		Controller writer = findWriter();
		if (writer != null) {
			m_uiContext.Highlighter.Highlight((IRenderedEntity)writer, ColorRgba.LightBlue);
		}
	}

	private void removeWriterHighlight() {
		Controller writer = findWriter();
		if (writer != null) {
			m_uiContext.Highlighter.RemoveHighlight((IRenderedEntity)writer);
		}
	}

	private void panToWriter() {
		Controller writer = findWriter();
		if (writer != null) {
			m_uiContext.CameraController.PanTo(writer.Position2f);
		} else {
			m_invalidOp.Play();
		}
	}

	private void openWriterInspector() {
		Controller writer = findWriter();
		if (writer == null) {
			m_invalidOp.Play();
			return;
		}
		if (m_uiContext.InspectorsManager.TryActivateFor(writer, out var inspector)) {
			m_inputMgr.ActivateNewController(inspector);
		} else {
			m_invalidOp.Play();
		}
	}

	private void removeVariable() {
		if (string.IsNullOrEmpty(m_variableName)) {
			m_invalidOp.Play();
			return;
		}
		m_uiContext.InputScheduler.ScheduleInputCmd(new VariableRemoveCmd(m_variableName));
	}

	public VariableEntry Name(string name) {
		m_variableName = name;
		m_text.Value(name.AsLoc());
		return this;
	}

	public VariableEntry Value(Fix32 value) {
		m_display.Value(value);
		return this;
	}
}
