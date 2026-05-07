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

		public TemplateModule(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, KeyValuePair<string, Template> item)
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
			(bool created, Module module) = m_tryCreate(item.Value.ModuleProto);
			if (created) {
				module.Prototype.ExecuteInit(module);
				item.Value.Setting(module);
			}
		}
	}
}
