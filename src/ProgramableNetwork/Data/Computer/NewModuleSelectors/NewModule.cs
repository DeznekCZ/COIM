using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using static ProgramableNetwork.Ui.ControllerView;

namespace ProgramableNetwork.Ui
{
	public class NewModule : AModuleProtoSelector
	{
		private ModuleProto item;

		public NewModule(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, ModuleProto item)
			: base(controllerView, refresh, onSuccess, tryCreate)
		{
			this.item = item;
		}

		public override Proto.Str Strings => item.Strings;

		public override Proto.ID Id => item.Id;
		public override string SearchString => string.Join(" ",
			item.Symbol,
			item.Strings.Name.TranslatedString,
			item.Strings.DescShort.TranslatedString
		);

		public override ImmutableArray<Category> Categories => item.Categories;

		public override Button CreateUi()
		{
			return new ButtonRow(new ButtonVariant().Gap(5))
			{
				new PanelWithHeader(item.Strings.Name)
					.Height(Sizes.BLOCK_SIZE * 4)
					.Width(300)
					.BodyAdd(new Label(item.Strings.DescShort)
						.FlexGrow(1)
						.TextAlign(TextAlignment.LeftTop)
						.AlignSelf(Align.Stretch)),
				new ModuleView(new Module(item, m_controllerView.Entity.Context, m_controllerView.Entity), m_controllerView, m_controllerView.Inspector.Context, true, () => { })
					.With(mv => {
						mv.Module.Prototype.ExecuteInit(mv.Module, log: false);
						mv.Module.Prototype.DisplayUpdate(mv.Module);
					}),
			};
		}

		public override void Selected()
		{
			(bool create, Module module) = m_tryCreate(item);
			if (create)
			{
				module.Prototype.ExecuteInit(module);
			}
		}
	}
}
