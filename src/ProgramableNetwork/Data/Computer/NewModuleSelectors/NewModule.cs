using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Research;
using Mafi.Core.Syncers;
using ProgramableNetwork.Python;
using UnityEngine.UIElements;
using static ProgramableNetwork.Ui.ControllerView;
using Align = Mafi.Unity.UiToolkit.Component.Align;
using Button = Mafi.Unity.UiToolkit.Library.Button;
using Column = Mafi.Unity.UiToolkit.Library.Column;
using Label = Mafi.Unity.UiToolkit.Library.Label;
using TextOverflow = Mafi.Unity.UiToolkit.Component.TextOverflow;

namespace ProgramableNetwork.Ui {
	public class NewModule : AModuleProtoSelector {
		private ModuleProto item;
		private Button m_button;
		private PanelWithHeader m_panel;

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

			if (m_button != null) {
				return m_button;
			}

			m_button = new ButtonRow(new ButtonVariant().Gap(5));
			m_panel = m_button.AddAndReturn(new PanelWithHeader())
				.Height(Sizes.BLOCK_SIZE * 4).Width(320);

			ButtonIcon settingsBtn = new ButtonIcon(Button.IconOnly, Mafi.Unity.Assets.Unity.UserInterface.General.Configure_svg)
				.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
			settingsBtn.Icon.AbsolutePositionCenterMiddle();
			settingsBtn.MarginRight(5.px());
			settingsBtn.OnClick(() => openSettingsDialog(settingsBtn));
			// Show the cog whenever the dialog would have ANYTHING to render — fields
			// OR per-side extension counters.  Previously hidden for field-less but
			// extensible modules (e.g. FlipFlop), leaving no way to pre-set extension
			// counts before placement.
			settingsBtn.ObserveVisible(m_button, () => item.Fields.Count > 0
				|| item.MaxInputExtensions > 0
				|| item.MaxOutputExtensions > 0
				|| item.MaxDisplayExtensions > 0);
			m_panel.Header.Add(settingsBtn);

			ButtonIcon templateIcon = new ButtonIcon(Button.IconOnly, Mafi.Unity.Assets.Unity.UserInterface.General.Text_svg)
				.Size(Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE);
			templateIcon.Icon.AbsolutePositionCenterMiddle();
			templateIcon.OnClick(() => openTemplateDialog(settingsBtn));
			templateIcon.Visible(false);
			m_panel.Header.Add(templateIcon);

			m_panel.Header.AddAndReturn(new Label(item.Strings.Name))
				.Class(Cls.bold)
				.Fill()
				.TextCenterMiddle();
			m_panel.BodyAdd(
				new ScrollColumn() {
					c => c.FlexGrow(1).AlignSelf(Align.Stretch),
					new Label(item.Strings.DescShort)
						.TextOverflow(TextOverflow.Wrap)
						.TextAlign(TextAlignment.LeftTop)
						.AlignSelf(Align.Stretch)
				});
			// Base preview — natural-width ModuleView of the as-placed module
			// (no extensions).  For extensible modules a translucent fully-
			// expanded ghost is layered behind so players see the maximum
			// footprint at a glance.  Non-extensible modules skip the wrapper
			// entirely to keep picker cells tight when no ghost is needed.
			Module basePreviewModule = new Module(item, m_controllerView.Entity.Context, m_controllerView.Entity, 0);
			basePreviewModule.Prototype.ExecuteInit(basePreviewModule, log: false);
			basePreviewModule.Prototype.DisplayUpdate(basePreviewModule);
			ModuleView basePreview = new ModuleView(basePreviewModule, m_controllerView, m_controllerView.Inspector.Context, true, () => { });

			bool isExtensible = item.MaxInputExtensions > 0
				|| item.MaxOutputExtensions > 0
				|| item.MaxDisplayExtensions > 0;
			if (!isExtensible) {
				m_button.Add(basePreview);
			} else {
				Module ghostModule = new Module(item, m_controllerView.Entity.Context, m_controllerView.Entity, 0);
				ghostModule.SetInputExtensionCount(item.MaxInputExtensions);
				ghostModule.SetOutputExtensionCount(item.MaxOutputExtensions);
				ghostModule.SetDisplayExtensionCount(item.MaxDisplayExtensions);
				ghostModule.Prototype.ExecuteInit(ghostModule, log: false);
				ghostModule.Prototype.DisplayUpdate(ghostModule);

				ModuleView ghostPreview = new ModuleView(ghostModule, m_controllerView, m_controllerView.Inspector.Context, true, () => { });
				ghostPreview.IgnoreInputPicking();
				ghostPreview.Opacity(0.25f);
				ghostPreview.AbsolutePosition(top: Px.Zero, left: Px.Zero);

				int maxW = ghostModule.Layout.GetWidth(ghostModule);
				UiComponent previewLayer = new UiComponent();
				previewLayer.Size(maxW * Sizes.BLOCK_SIZE, Sizes.BLOCK_SIZE * 4);
				previewLayer.Add(ghostPreview);
				basePreview.AbsolutePosition(top: Px.Zero, left: Px.Zero);
				previewLayer.Add(basePreview);
				m_button.Add(previewLayer);
			}

			m_button.ObserveEnumerable(() => TemplateRegistrator.GetModulePickerTemplates(item))
				.Do((items) => {
					templateIcon.SetVisible(items.Count > 0);
				});

			if (item.ResearchDependency.IsNotEmpty) {
				ImmutableArray<ResearchNode> nodes = item.ResearchDependency.Map(r =>
					m_controllerView.Inspector.ResearchManager.GetResearchNode(r));
				m_panel.Header.Add(new ResearchSummaryIcon(nodes));
			}
			return m_button;
		}

		private void openSettingsDialog(UiComponent anchor) {
			// id=0 keeps this off the ModuleIdManager's monotonic counter — no
			// risk of leaking ids when the picker is closed without committing.
			Module previewModule = new Module(item, m_controllerView.Entity.Context, m_controllerView.Entity, 0);
			previewModule.Prototype.ExecuteInit(previewModule, log: false);

			// keepOpenOnHover=false, openAfterDelay=false, closeOnClickOutside=true
			// — same flags as ControllerInspector.openDescriptionDialog and the base
			// game's OverlapSettingsPopup.  closeOnClickOutside means clicking
			// inside the dialog (including TextFields) is fine; only clicks landing
			// outside the dialog tear it down.
			FloatingColumn dialog = new FloatingColumn(
				new DropdownPositionPolicy(), false, false, true);
			dialog.Gap(5.px());

			PanelWithHeader panel = new PanelWithHeader(item.Strings.Name);
			panel.Width(320);
			panel.Height(Px.Auto);
			panel.Body.Gap(5.px());

			// Extension-count controls — one row per side the proto allows.  Each
			// row's +/- buttons write straight to previewModule via SetXExtensionCount,
			// and CopyFrom (on Add below) mirrors the picked counts onto the placed
			// module.  Sides whose Max is 0 are skipped so the dialog stays compact.
			if (item.MaxInputExtensions > 0) {
				panel.Body.Add(buildExtensionCountRow(
					() => NewTr.Inspector.ExtensionCountInputs,
					() => previewModule.InputExtensionCount,
					n => previewModule.SetInputExtensionCount(n),
					item.MaxInputExtensions,
					panel));
			}
			if (item.MaxOutputExtensions > 0) {
				panel.Body.Add(buildExtensionCountRow(
					() => NewTr.Inspector.ExtensionCountOutputs,
					() => previewModule.OutputExtensionCount,
					n => previewModule.SetOutputExtensionCount(n),
					item.MaxOutputExtensions,
					panel));
			}
			if (item.MaxDisplayExtensions > 0) {
				panel.Body.Add(buildExtensionCountRow(
					() => NewTr.Inspector.ExtensionCountDisplays,
					() => previewModule.DisplayExtensionCount,
					n => previewModule.SetDisplayExtensionCount(n),
					item.MaxDisplayExtensions,
					panel));
			}

			// Each IField renders its own row (label + control) into panel.Body
			// and wires its OnValueChanged back into the fake module's data dicts.
			// updateDialog runs DisplayUpdate on the proto so any computed display
			// values stay in sync with the freshly-edited inputs.
			// directEdit=true: skip the per-field save button and write straight
			// to module.Field[...].  The fake module isn't owned by a Controller,
			// so the equivalent ModuleSetXxxFieldCmd would no-op.
			foreach (IField field in item.Fields) {
				field.Init(
					m_controllerView.Inspector,
					m_controllerView.Inspector,
					panel.Body,
					m_controllerView.Inspector.Context,
					previewModule,
					() => previewModule.Prototype.DisplayUpdate(previewModule),
					directEdit: true);
			}

			panel.BodyAdd(new ButtonText(NewTr.Tools.Add)
				.LaterText(() => NewTr.Tools.Add, panel)
				.OnClick(() => {
					(bool create, Module module) = m_tryCreate(item);
					if (create) {
						module.Prototype.ExecuteInit(module);
						module.CopyFrom(previewModule);
						dialog.Close();
					}
				}));

			dialog.Add(panel);
			dialog.Open(anchor);
		}

		// Renders one "label  [-] N/MAX [+]" row.  labelGetter is used via
		// LaterText so the label re-resolves after the LocStr-rebind freeze; the
		// count label observes getter() and refreshes whenever the preview
		// module's extension property flips (Setter returns the clamped value
		// and Observe re-polls each tick, so the display stays in sync without
		// needing an explicit refresh callback).
		private static Row buildExtensionCountRow(Func<LocStr> labelGetter, Func<int> getter, Func<int, int> setter, int max, UiComponent host) {
			Row row = new Row(gap: 5.px());

			Label nameLabel = new Label(LocStrFormatted.Empty)
				.FlexGrow(1)
				.TextAlign(TextAlignment.LeftMiddle);
			nameLabel.LaterText(labelGetter, host);
			row.Add(nameLabel);

			ButtonText dec = new ButtonText("-".AsLoc())
				.Size(24.px(), 24.px())
				.Padding(Px.Zero)
				.TextAlign(TextAlignment.CenterMiddle);
			dec.OnClick(() => setter(getter() - 1));
			dec.ObserveEnabled(() => getter() > 0);
			row.Add(dec);

			Label countLabel = new Label(LocStrFormatted.Empty)
				.Width(50.px())
				.TextAlign(TextAlignment.CenterMiddle);
			countLabel.Observe(getter)
				.Do(n => countLabel.Value(new LocStrFormatted(n + "/" + max)));
			row.Add(countLabel);

			ButtonText inc = new ButtonText("+".AsLoc())
				.Size(24.px(), 24.px())
				.Padding(Px.Zero)
				.TextAlign(TextAlignment.CenterMiddle);
			inc.OnClick(() => setter(getter() + 1));
			inc.ObserveEnabled(() => getter() < max);
			row.Add(inc);

			return row;
		}

		// Repopulates `chipRow` with the current set of picker-flagged
		// templates targeting this module.  Called on every show of the
		// chip row (geometry-changed event from OnShow), so saving a
		// blueprint or reloading Python templates between picker opens
		// surfaces immediately on the next show — without the picker
		// having to be torn down and rebuilt.  Old chips are cleared
		// first so we don't double-render after a refresh.
		private void openTemplateDialog(Button templates) {
			FloatingColumn dialog = new FloatingColumn(
				new DropdownPositionPolicy(), false, false, true);
			dialog.Gap(5.px());

			Panel panel = new Panel();
			panel.Width(320);
			panel.Height(Px.Auto);
			panel.BodyGap(5.px());
			dialog.Add(panel);

			foreach (KeyValuePair<string, Template> templateItem
					in TemplateRegistrator.GetModulePickerTemplates(item)) {
				Template template = templateItem.Value;
				if (template.ModuleProto == null) {
					continue;
				}
				ButtonText chip = new ButtonText(new LocStrFormatted(template.Name ?? ""));
				chip.Tooltip(new LocStrFormatted(template.Name ?? ""));
				chip.OnClick(() => {
					(bool created, Module createdModule) = m_tryCreate(template.ModuleProto);
					if (created) {
						createdModule.Prototype.ExecuteInit(createdModule);
						template.Setting(createdModule);
						createdModule.Prototype.DisplayUpdate(createdModule);
						dialog.Close();
					}
				});
				chip.RegisterCallback<MouseUpEvent>(evt => evt.StopPropagation());
				chip.RegisterCallback<MouseDownEvent>(evt => evt.StopPropagation());
				panel.BodyAdd(chip);
			}

			dialog.Open(templates);
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
				if (m_allResearched) {
					return ResearchNodeState.Researched;
				}

				bool anyMissing = false;
				bool anyInProgress = false;
				foreach (ResearchNode node in nodes) {
					switch (node.State) {
						case ResearchNodeState.NotResearched: anyMissing = true; break;
						case ResearchNodeState.InProgress: anyInProgress = true; break;
					}
				}
				if (anyMissing) {
					return ResearchNodeState.NotResearched;
				}
				if (anyInProgress) {
					return ResearchNodeState.InProgress;
				}

				m_allResearched = true;
				return ResearchNodeState.Researched;
			}
		}
	}
}
