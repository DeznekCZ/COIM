using Mafi;
using Mafi.Collections;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui.Hud;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using ProgramableNetwork.Data.Mod;
using ProgramableNetwork.Python;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Recipe-style picker for the Template-slot controller.  Lists every available
	/// <see cref="AControllerTemplateEntry"/> (Python-defined templates plus
	/// <c>[PN-Controller]-</c> blueprints from the local
	/// <see cref="Mafi.Core.Entities.Blueprints.BlueprintsLibrary"/>) and applies
	/// the chosen one to the live controller via the entry's Apply method.
	///
	/// Built fresh per open so newly-saved blueprints show up without a restart.
	/// </summary>
	public class PickControllerTemplate : FloatingColumn
	{
		private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();
		private string m_searchText = "";

		public PickControllerTemplate(Controller controller)
			: base(POLICY, false, false, true)
		{
			PanelWithHeader panel = AddAndReturn(new PanelWithHeader("Pick controller template".AsLoc()));
			panel.Height(Px.Auto);

			TextField search = new TextField()
				.Width(220.px())
				.Height(Sizes.BLOCK_SIZE);
			search.Placeholder(Tr.Search);
			search.FocusOnShow();
			search.OnValueChanged((text) => m_searchText = text ?? "");
			panel.Header.Add(search);

			ScrollColumn list = panel.Body.AddAndReturn(new ScrollColumn())
				.Width(420.px())
				.Height(500.px());

			foreach (AControllerTemplateEntry entry in CollectEntries(controller))
			{
				AControllerTemplateEntry captured = entry;
				PanelRow row = new PanelRow();
				row.PanelStyleHud();
				row.Width(400.px());
				row.Height(Px.Auto);

				Column textCol = new Column(2.pt());
				textCol.Add(new Label(captured.Name).Class(Cls.bold));
				if (!string.IsNullOrEmpty(captured.Description.Value))
				{
					textCol.Add(new Label(captured.Description)
						.TextOverflow(TextOverflow.Wrap));
				}

				ButtonText apply = new ButtonText("Use".AsLoc())
					.Width(60.px())
					.Height(Sizes.BLOCK_SIZE)
					.OnClick(() =>
					{
						try
						{
							captured.Apply(controller);
						}
						catch (Exception ex)
						{
							Log.Exception(ex);
						}
						Close();
					});

				row.BodyAdd(textCol, apply);
				list.Add(row);

				// Search filter — show/hide based on lowercased token match.
				this.Observe(() => m_searchText)
					.Do(text =>
					{
						bool match = string.IsNullOrEmpty(text) ||
							captured.SearchString.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) >= 0;
						row.Visible(match);
					});
			}

			this.Width(440.px());
			this.Height(Px.Auto);
		}

		private static IEnumerable<AControllerTemplateEntry> CollectEntries(Controller controller)
		{
			// Python-defined templates first — these are the curated/built-in
			// entries.  Read from the cache populated at RegisterDataInternal time
			// (the lambdas inside each template retain their registrator capture,
			// so they're safe to invoke at runtime even though the registrator
			// itself isn't DI-resolvable after load).
			foreach (ControllerTemplate t in ControllerTemplates.CachedTemplates)
			{
				yield return new PythonControllerTemplateEntry(t);
			}

			// Then user-saved [PN-Controller]- blueprints, sorted by their library
			// title so the alphabetical order in this picker matches the base
			// blueprint browser.
			Mafi.Core.Entities.Blueprints.BlueprintsLibrary library = null;
			try
			{
				library = GlobalDependencyResolver.Get<Mafi.Core.Entities.Blueprints.BlueprintsLibrary>();
			}
			catch (Exception e)
			{
				Log.Exception(e);
			}
			if (library != null)
			{
				Lyst<Mafi.Core.Entities.Blueprints.IBlueprint> all = new Lyst<Mafi.Core.Entities.Blueprints.IBlueprint>();
				foreach (var bp in EnumerateControllerBlueprints(library))
				{
					all.Add(bp);
				}
				all.Sort((a, b) => string.CompareOrdinal(a.Name ?? "", b.Name ?? ""));
				foreach (var bp in all)
				{
					yield return new BlueprintControllerTemplateEntry(bp);
				}
			}
		}

		private static IEnumerable<Mafi.Core.Entities.Blueprints.IBlueprint> EnumerateControllerBlueprints(
			Mafi.Core.Entities.Blueprints.BlueprintsLibrary library)
		{
			if (library?.Root == null) {
				yield break;
			}
			foreach (var bp in walk(library.Root)) {
				yield return bp;
			}
		}

		private static IEnumerable<Mafi.Core.Entities.Blueprints.IBlueprint> walk(
			Mafi.Core.Entities.Blueprints.IBlueprintsFolder folder)
		{
			if (folder == null) {
				yield break;
			}
			if (folder.Blueprints != null)
			{
				foreach (var bp in folder.Blueprints.AsEnumerable())
				{
					if (ControllerBlueprints.IsPnControllerBlueprint(bp)) {
						yield return bp;
					}
				}
			}
			if (folder.Folders != null)
			{
				foreach (var sub in folder.Folders.AsEnumerable())
				{
					foreach (var bp in walk(sub)) {
						yield return bp;
					}
				}
			}
		}
	}
}
