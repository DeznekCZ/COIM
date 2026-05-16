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
			Action<ModuleProto, Action<bool, Module>> tryCreate,
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
					preview = new Module(m_proto, m_controllerView.Entity.Context, m_controllerView.Entity, 0);
					preview.Prototype.ExecuteInit(preview, log: false);
				}
			}
			catch (Exception e)
			{
				Log.Exception(e);
				preview = new Module(m_proto, m_controllerView.Entity.Context, m_controllerView.Entity, 0);
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
			// All the place + configure work — including the blueprint snapshot
			// copy that used to run here UI-side — is now baked into
			// ModulePlaceFromBlueprintCmd (see the lambda in
			// ControllerView.EnumerateBlueprintSelectorsCore where this selector
			// is constructed).  The cmd carries the deserialised Module snapshot
			// inline, so every MP peer reconstructs the same configured state
			// independently of whether they have the blueprint in their library.
			m_tryCreate(m_proto, (placed, placedModule) =>
			{
				if (placed && placedModule != null)
				{
					m_onSuccess?.Invoke(placedModule);
				}
			});
		}
	}
}
