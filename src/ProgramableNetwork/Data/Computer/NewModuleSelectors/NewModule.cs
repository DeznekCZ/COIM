using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using Mafi;
using Mafi.Core.Research;
using Mafi.Core.Syncers;
using static ProgramableNetwork.Ui.ControllerView;

namespace ProgramableNetwork.Ui {
	public class NewModule : AModuleProtoSelector {
		private ModuleProto item;

		public NewModule(ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, ModuleProto item)
			: base(controllerView, refresh, onSuccess, tryCreate) {
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

		public override Button CreateUi() {

			ButtonRow buttonRow = new ButtonRow(new ButtonVariant().Gap(5));
			buttonRow.Add(
				new PanelWithHeader(item.Strings.Name)
					.Height(Sizes.BLOCK_SIZE * 4)
					.Width(300)
					.BodyAdd(
						(new Row() {
							c => c.Add(item.ResearchDependency.Map(r =>
								new ResearchNodeReferenceUi(m_controllerView.Inspector.ResearchManager
									.GetResearchNode(r))).AsEnumerable())
						}).ObserveVisible(buttonRow, () => item.ResearchDependency.IsNotEmpty),
						new Label(item.Strings.DescShort)
							.FlexGrow(1)
							.TextAlign(TextAlignment.LeftTop)
							.AlignSelf(Align.Stretch)),
				new ModuleView(new Module(item, m_controllerView.Entity.Context, m_controllerView.Entity), m_controllerView, m_controllerView.Inspector.Context, true, () => { })
					.With(mv => {
						mv.Module.Prototype.ExecuteInit(mv.Module, log: false);
						mv.Module.Prototype.DisplayUpdate(mv.Module);
					}));
			return buttonRow;
		}

		public override void Selected() {
			(bool create, Module module) = m_tryCreate(item);
			if (create) {
				module.Prototype.ExecuteInit(module);
			}
		}

		private class ResearchNodeReferenceUi : Row {
			public ResearchNodeReferenceUi(ResearchNode research) {
				this.Class(Cls.group);
				this.Margin(5.px());
				this.Gap(5.px());
				this.Tooltip(research.Proto.Strings.DescShort);
				Icon unlockIcon = AddAndReturn(new Icon());
				this.Observe(() => research.State)
					.Do(available => unlockIcon.Value(
						available switch {
							ResearchNodeState.Researched
								=> Mafi.Unity.Assets.Unity.UserInterface.Research.ResearchUnlocked_svg,
							_ => Mafi.Unity.Assets.Unity.UserInterface.Research.ResearchLocked_svg
						},
						available switch {
							ResearchNodeState.NotResearched => Theme.DangerColor,
							ResearchNodeState.InProgress => Theme.ImportantColor,
							ResearchNodeState.Researched => Theme.PositiveColor,
							_ => Theme.DangerColor
						}));
				Add(new Icon(Mafi.Unity.Assets.Unity.UserInterface.General.ResearchPoint_svg));
				if (research.Proto.Graphics.Icons.IsNotEmpty) {
					Add(new Icon().Value(research.Proto.Graphics.Icons.First));
				}
				Add(new Label().Value(research.Proto.Strings.Name));
			}
		}
	}
}
