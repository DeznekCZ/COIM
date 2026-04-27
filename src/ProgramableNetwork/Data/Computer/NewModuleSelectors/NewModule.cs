using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Linq;
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

			PanelWithHeader panel = new PanelWithHeader()
				.Height(Sizes.BLOCK_SIZE * 4)
				.Width(320);

			if (item.ResearchDependency.IsNotEmpty) {
				ImmutableArray<ResearchNode> nodes = item.ResearchDependency.Map(r =>
					m_controllerView.Inspector.ResearchManager.GetResearchNode(r));
				panel.Header.Add(new ResearchSummaryIcon(nodes));
			}
			panel.Header.Add(new Label(item.Strings.Name).Class(Cls.bold));

			panel.BodyAdd(
				new ScrollColumn {
					new Label(item.Strings.DescShort)
						.TextOverflow(TextOverflow.Wrap)
						.TextAlign(TextAlignment.LeftTop)
						.AlignSelf(Align.Stretch)
				}.FlexGrow(1).AlignSelf(Align.Stretch));

			buttonRow.Add(
				panel,
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

		private class ResearchSummaryIcon : Row {
			private bool m_allResearched;

			public ResearchSummaryIcon(ImmutableArray<ResearchNode> nodes) {
				this.Class(Cls.group);
				this.Margin(5.px());
				Icon summary = AddAndReturn(new Icon());
				this.Observe(() => AggregateState(nodes))
					.Do(state => summary.Value(
						state == ResearchNodeState.Researched
							? Mafi.Unity.Assets.Unity.UserInterface.Research.ResearchUnlocked_svg
							: Mafi.Unity.Assets.Unity.UserInterface.Research.ResearchLocked_svg,
						state switch {
							ResearchNodeState.NotResearched => Theme.DangerColor,
							ResearchNodeState.InProgress => Theme.ImportantColor,
							ResearchNodeState.Researched => Theme.PositiveColor,
							_ => Theme.DangerColor
						}));

				Column floaterContent = new Column();
				floaterContent.Gap(5.px());
				foreach (ResearchNode node in nodes) {
					floaterContent.Add(new ResearchNodeReferenceUi(node));
				}
				this.Floater(floaterContent);
			}

			private ResearchNodeState AggregateState(ImmutableArray<ResearchNode> nodes) {
				if (m_allResearched) return ResearchNodeState.Researched;

				bool anyMissing = false;
				bool anyInProgress = false;
				foreach (ResearchNode node in nodes) {
					switch (node.State) {
						case ResearchNodeState.NotResearched: anyMissing = true; break;
						case ResearchNodeState.InProgress: anyInProgress = true; break;
					}
				}
				if (anyMissing) return ResearchNodeState.NotResearched;
				if (anyInProgress) return ResearchNodeState.InProgress;

				m_allResearched = true;
				return ResearchNodeState.Researched;
			}
		}
	}
}
