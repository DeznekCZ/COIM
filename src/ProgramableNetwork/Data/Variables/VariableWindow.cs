using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.GameLoop;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl;
using Mafi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using UnityEngine;
using Mafi.Unity.Ui.Library;
using Display = Mafi.Unity.Ui.Library.Display;

namespace ProgramableNetwork.Data.Variables;

[GlobalDependency(RegistrationMode.AsSelf)]
public class VariableWindow : Window {

    public VariableWindow(ControllerContext context, VariableManager variableManager)
		: base("Network variables".ToDoLoc(), addFullscreenButton: true) {

		this.LaterText(() => NewTr.Inspector.NetworkVariablesTitle, this, (vv, t) => vv.Title(t));
		WindowMaxHeight(Percent.Hundred);
		MakeMovable();

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
						scrollColumn.AddCached<VariableEntry>();
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

	public VariableEntry() {
		m_text = AddAndReturn(new Label()).Width(50.Percent());
		m_display = AddAndReturn(new Display()).Width(50.Percent());
		this.Margin(5);
	}

	public VariableEntry Name(string name) {
		m_text.Value(name.AsLoc());
		return this;
	}

	public VariableEntry Value(Fix32 value) {
		m_display.Value(value);
		return this;
	}
}