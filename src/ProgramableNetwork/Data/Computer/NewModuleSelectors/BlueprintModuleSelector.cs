using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Blueprints;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using static ProgramableNetwork.Ui.ControllerView;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Picker entry that wraps a base-game <see cref="IBlueprint"/> whose payload is a
	/// single <see cref="Module"/>.  Behavior parallels <see cref="TemplateModule"/>
	/// (preview via real ModuleView, click to place into the target slot) — the only
	/// difference is the data source: we extract the module from the saved blueprint
	/// instead of running a Python settings lambda.
	///
	/// The saved-blueprints scanner in <see cref="ControllerView"/> creates one of
	/// these per <c>[PN-Module]-</c> prefixed entry in <see cref="BlueprintsLibrary"/>.
	/// </summary>
	public class BlueprintModuleSelector : AModuleProtoSelector
	{
		private readonly IBlueprint m_blueprint;
		private readonly ModuleProto m_proto;

		public BlueprintModuleSelector(
			ControllerView controllerView,
			Action refresh,
			Action<Module> onSuccess,
			Func<ModuleProto, (bool, Module)> tryCreate,
			IBlueprint blueprint,
			ModuleProto proto)
			: base(controllerView, refresh, onSuccess, tryCreate)
		{
			m_blueprint = blueprint;
			m_proto = proto;
		}

		// Strip the [PN-Module]- prefix for display; users named these themselves and
		// don't need the marker shouted back at them.
		private string DisplayName
		{
			get
			{
				string n = m_blueprint.Name ?? "";
				return n.StartsWith(ModuleBlueprints.TitlePrefix)
					? n.Substring(ModuleBlueprints.TitlePrefix.Length)
					: n;
			}
		}

		public override Proto.Str Strings =>
			new Proto.Str(LocalizationManager.GetLocalizedString0Arg(
				Id.Value,
				string.Join(" ",
					DisplayName,
					m_proto.Symbol,
					m_proto.Strings.Name.TranslatedString,
					m_proto.Strings.DescShort.TranslatedString),
				"Player-saved module blueprint"));

		// Each library entry gets a stable picker id derived from its title.  Avoids
		// collisions with the proto-derived ids the other selectors use.
		public override Proto.ID Id => new Proto.ID($"Blueprint_{m_proto.Id.Value}_{m_blueprint.Name}");

		public override string SearchString => string.Join(" ",
			DisplayName,
			m_proto.Symbol,
			m_proto.Strings.Name.TranslatedString,
			m_proto.Strings.DescShort.TranslatedString);

		// Always shows up under the Saved category; ALSO inherits the underlying
		// module's categories so the entry appears when filtering by e.g. "Control"
		// or the original module's category, in addition to the dedicated "Saved" tab.
		public override ImmutableArray<Category> Categories
		{
			get
			{
				var list = new List<Category> { Category.Saved };
				foreach (Category c in m_proto.Categories.AsEnumerable()) {
					list.Add(c);
				}
				return list.ToImmutableArray();
			}
		}

		public override Button CreateUi()
		{
			// Preview construction mirrors TemplateModule.  We instantiate a throwaway
			// Module on the live host controller so initContexts/ExecuteInit run with
			// real protos, then re-apply the blueprint payload onto it.  The Module
			// returned by TryExtractInto is also a fresh instance, so the preview
			// reflects exactly what will be placed.
			Module preview;
			try
			{
				Option<Module> extracted = ModuleBlueprints.TryExtractInto(m_blueprint, m_controllerView.Entity);
				if (extracted.HasValue)
				{
					preview = extracted.Value;
				}
				else
				{
					preview = new Module(m_proto, m_controllerView.Entity.Context, m_controllerView.Entity);
					preview.Prototype.ExecuteInit(preview, log: false);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				preview = new Module(m_proto, m_controllerView.Entity.Context, m_controllerView.Entity);
				preview.Prototype.ExecuteInit(preview, log: false);
			}

			return new ButtonRow(new ButtonVariant().Gap(5))
			{
				new PanelWithHeader($"Saved: {DisplayName}".AsLoc())
					.Height(Sizes.BLOCK_SIZE * 4)
					.Width(320)
					.BodyAdd(new ScrollColumn {
						new Label(new LocStrFormatted(m_proto.Strings.Name.TranslatedString))
							.Class(Cls.bold),
						new Label(new LocStrFormatted(m_proto.Strings.DescShort.TranslatedString))
							.TextOverflow(TextOverflow.Wrap)
							.TextAlign(TextAlignment.LeftTop)
							.AlignSelf(Align.Stretch)
					}.FlexGrow(1).AlignSelf(Align.Stretch)),
				new ModuleView(preview, m_controllerView, m_controllerView.Inspector.Context, true, () => { })
					.With(mv => {
						try { mv.Module.Prototype.DisplayUpdate(mv.Module); }
						catch (Exception e) { Log.Exception(e); }
					}),
			};
		}

		public override void Selected()
		{
			(bool placed, Module placedModule) = m_tryCreate(m_proto);
			if (!placed || placedModule == null)
			{
				return;
			}
			// Overwrite the freshly-placed default with the saved configuration.
			// Re-extracting from the blueprint (rather than copying from the preview)
			// keeps each placement independent of any other selector's UI state.
			Option<Module> extracted = ModuleBlueprints.TryExtractInto(m_blueprint, m_controllerView.Entity);
			if (!extracted.HasValue)
			{
				return;
			}
			Module from = extracted.Value;
			// Mirrors the in-controller "Copy / Paste" handler in ModuleEditDialog —
			// only configured/persistent state is copied.  Input/Output numeric
			// snapshots are per-tick volatile values that get overwritten next sim
			// tick from real cable inputs, so copying them just adds noise.  Likewise
			// InputModules (cable connections) are intentionally not restored: the
			// blueprint's source-side module IDs don't exist in the target controller.
			placedModule.NumberData.Clear();
			placedModule.FieldNumberData.Clear();
			placedModule.StringData.Clear();
			foreach (var kv in from.NumberData)       { placedModule.NumberData[kv.Key] = kv.Value; }
			foreach (var kv in from.FieldNumberData)  { placedModule.FieldNumberData[kv.Key] = kv.Value; }
			foreach (var kv in from.StringData)       { placedModule.StringData[kv.Key] = kv.Value; }
			// Pin extension counts roundtrip through the blueprint payload — restore
			// them so a saved A+B with extra inputs places back at the same width.
			placedModule.SetInputExtensionCount(from.InputExtensionCount);
			placedModule.SetOutputExtensionCount(from.OutputExtensionCount);
			// ArrayData is replaced wholesale rather than per-element copied — the
			// saved buffer is the canonical state for any module that uses Array.
			if (from.ArrayData != null && from.ArrayData.Length > 0)
			{
				Fix32[] copy = new Fix32[from.ArrayData.Length];
				Array.Copy(from.ArrayData, copy, copy.Length);
				typeof(Module)
					.GetProperty(nameof(Module.ArrayData))
					.SetValue(placedModule, copy);
			}
			placedModule.Prototype.ExecuteInit(placedModule, log: false);
			m_onSuccess?.Invoke(placedModule);
		}
	}
}
