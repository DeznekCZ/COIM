using Mafi.Collections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using Mafi.Collections.ImmutableCollections;
using static ProgramableNetwork.Ui.ControllerView;

namespace ProgramableNetwork.Ui
{
	public class TemplateModule : AModuleProtoSelector
	{
		private KeyValuePair<string, Template> item;

		public TemplateModule(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Action<ModuleProto, Action<bool, Module>> tryCreate, KeyValuePair<string, Template> item)
			: base(controllerView, refresh, onSuccess, tryCreate)
		{
			this.item = item;
		}

		public override Proto.Str Strings =>
			new Proto.Str(
				LocalizationManager.GetLocalizedString0Arg(
					Id.Value,
					string.Join(" ",
						item.Value.Name,
						item.Value.ModuleProto.Symbol,
						item.Value.ModuleProto.Strings.Name.TranslatedString,
						item.Value.ModuleProto.Strings.DescShort.TranslatedString
					),
					"No comment"
				));

		public override Proto.ID Id => new Proto.ID($"Template_{item.Value.ModuleProto.Id.Value}_{item.Key}");

		public override string SearchString => string.Join(" ",
			item.Value.Name,
			item.Value.ModuleProto.Symbol,
			item.Value.ModuleProto.Strings.Name.TranslatedString,
			item.Value.ModuleProto.Strings.DescShort.TranslatedString
		);

		public override ImmutableArray<Category> Categories => item.Value.ModuleProto.Categories;

		public override Button CreateUi()
		{
			return new ButtonRow(new ButtonVariant().Gap(5))
			{
				new PanelWithHeader($"Template: {item.Value.ModuleProto.Strings.Name.TranslatedString}".AsLoc())
					.Height(Sizes.BLOCK_SIZE * 4)
					.Width(320)
					.BodyAdd(new ScrollColumn {
						new Label(new LocStrFormatted(item.Value.Name))
							.TextOverflow(TextOverflow.Wrap)
							.TextAlign(TextAlignment.LeftTop)
							.AlignSelf(Align.Stretch)
					}.FlexGrow(1).AlignSelf(Align.Stretch)),
				new ModuleView(new Module(item.Value.ModuleProto, m_controllerView.Entity.Context, m_controllerView.Entity, 0), m_controllerView, m_controllerView.Inspector.Context, true, () => { })
					.With(mv => {
						mv.Module.Prototype.ExecuteInit(mv.Module, log: false);
						item.Value.Setting(mv.Module);
						mv.Module.Prototype.DisplayUpdate(mv.Module);
					}),
			};
		}

		public override void Selected()
		{
			// ExecuteInit is now part of Controller.TryPlaceModule (runs on the sim
			// thread for every MP peer).  The template's Setting lambda still runs
			// UI-side on the originating client only — MP-incorrect today.
			//
			// TODO MP correctness: route through a "place + configure" cmd that
			// CARRIES THE RESULTING MODULE STATE in its payload (not the template id),
			// because Python templates / blueprint entries can be loaded only on
			// the originating client and may not exist on other peers.  The originator
			// runs Setting on a throwaway Module locally to materialise the configured
			// state, serialises that into the cmd, and the executor copies it onto
			// the freshly-placed module on every peer.
			m_tryCreate(item.Value.ModuleProto, (created, module) =>
			{
				if (created && module != null) {
					item.Value.Setting(module);
				}
			});
		}
	}
}
