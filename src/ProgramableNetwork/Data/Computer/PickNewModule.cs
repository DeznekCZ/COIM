using Mafi;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using Mafi.Collections;
using static Mafi.Unity.Assets.Unity;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.Ui.Library;

namespace ProgramableNetwork.Ui;

public class PickNewModule : FloatingColumn {
	private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();
	private string m_searchText = "";

	public PickNewModule(LocStrFormatted title, IEnumerable<AModuleProtoSelector> protos)
		: base(POLICY, false, false, true) {
		Log.Info($"[PickNewModule] Generating layout");

		PanelWithHeader panel = AddAndReturn(new PanelWithHeader().Height(Px.Auto));

		TextField search = new TextField();
		search.Placeholder(Tr.Search);
		search.MinWidth(150.px());
		search.FlexGrow(0.5f);
		search.FocusOnShow();
		search.OnValueChanged((text) => m_searchText = text);

		this.OnShow(() => { search.SetValue("".AsLoc()); });

		ButtonIcon clear = new ButtonIcon(Assets.Unity.UserInterface.General.Trash128_png);
		clear.OnClick(() => {
			search.ClearValue();
			m_searchText = "";
		});

		panel.Header.Add(
				new Icon(UserInterface.General.Search_svg),
				search,
				clear,
				new Label(title).TextAlign(TextAlignment.CenterMiddle).FlexGrow(1)
			);
		panel.Header.Gap(5.px());

		Log.Info($"[PickNewModule] Generating hashset");
		List<UiComponent> buttonList = [];
		Dict<string, ButtonText> categoryDict = [];
		Dict<string, Category> categoryOrdering = [];
		foreach (AModuleProtoSelector item in protos) {
			try {
				Button searchItem = item.CreateUi();
				searchItem.OnClick(item.Selected);
				buttonList.Add(searchItem);

				// Prepare all listened categories list
				Lyst<Button> observedButtons = [];

				foreach (Category category in item.Categories) {
					if (!categoryDict.TryGetValue(category.Id, out ButtonText categoryButton)) {
						categoryButton = new ButtonText(Button.ToggleGroup, category.Name)
							.Toggleable()
							.Selected();
						categoryButton.OnDoubleClick((b) => {
							foreach (ButtonText categoryButtonForUnselection in categoryDict.Values) {
								categoryButtonForUnselection.Selected(false);
							}
							b.Selected();
						});
						categoryButton.OnClick((b) => b.Selected(b.IsSelected() == false));
						categoryDict.Add(category.Id, categoryButton);
						categoryOrdering.Add(category.Id, category);
					}
					observedButtons.Add(categoryButton);
				}

				this
					.Observe(() => m_searchText)
					.Observe(() => IIndexableExtensions.Any(observedButtons, c => c.IsSelected()))
					.Do((searchString, categorySelected) => {
						searchItem.Visible(categorySelected && item.SearchString
							.Contains(searchString, StringComparison.InvariantCultureIgnoreCase));
					});
			} catch (Exception e) {
				Log.Error($"[PickNewModule] Failed to create ui for module: {item.Id}");
				Log.Exception(e);

				buttonList.Add(new PanelRow() { new Label(item.Strings.Name).Class(Cls.error) });
			}
		}

		Log.Info($"[PickNewModule] Total {buttonList.Count} modules");

		Row row = panel.Body.AddAndReturn(new Row())
			.Width(Px.Auto);

		ScrollColumn categoriesSelection = row.AddAndReturn(new ScrollColumn())
			.Width(150.px())
			.Height(600);
		Button allButton = categoriesSelection.AddAndReturn(new ButtonText(LocStrFormatted.Empty)
			.LaterText<ButtonText>(() => NewTr.Inspector.All, this, (b, v) => b.Value(v)));
		categoriesSelection.Add(new HorizontalDivider().Height(10.px()));
		allButton.OnClick(() => {
			foreach (ButtonText b in categoryDict.Values) {
				b.Selected();
			}
		});
		categoriesSelection.Add(categoryDict
			.OrderBy(i => categoryOrdering[i.Key].Name.TranslatedString)
			.Select(c => c.Value));

		row.Add(new VerticalDivider().Width(10.px()));

		// Vertical scrollbar takes 17 px (see ScrollBase.PreventResizeForScroller); add it so the
		// rightmost module entries aren't clipped when the scroller is visible.
		Px scrollerWidth = 17.px();
		ScrollBoth modulesSelection = row.AddAndReturn(new ScrollBoth())
			.Width(Sizes.BLOCK_SIZE * 4 + 340.px() + scrollerWidth)
			.Height(600);
		modulesSelection.Add(buttonList);

		this.Height(Px.Auto);
		this.Width(Sizes.BLOCK_SIZE * 4 + 500.px() + scrollerWidth);
		
		Log.Info($"[PickNewModule] Created");
	}
}
