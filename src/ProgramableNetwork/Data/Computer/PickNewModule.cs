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

namespace ProgramableNetwork.Ui
{
	public class PickNewModule : FloatingColumn
	{
		private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();
		public Controller Controller { get; set; }

		public PickNewModule(LocStrFormatted title, IEnumerable<AModuleProtoSelector> protos, UiComponent button)
			: base(POLICY, false, false, true)
		{
			Log.Info($"[PickNewModule] Generating layout");

			PanelWithHeader panel = AddAndReturn(new PanelWithHeader().Height(Px.Auto));

			TextField search = new TextField();
			search.Placeholder(Tr.Search);
			search.MinWidth(150.px());
			search.FlexGrow(0.5f);
			search.FocusOnShow();

			panel.Header.Add(
				new Icon(UserInterface.General.Search_svg),
				search,
				new Label(title).TextAlign(TextAlignment.CenterMiddle).FlexGrow(1)
			);

			Log.Info($"[PickNewModule] Generating hashset");
			Dict<string, Button> searchDict = [];
			Dict<string, Lyst<UiComponent>> categoryDict = [];

			Lyst<Category> categories = [];
			Lyst<UiComponent> searchList = [];
			foreach (AModuleProtoSelector item in protos)
			{
				try
				{
					Button searchItem = item.CreateUi();
					searchDict.Add(item.SearchString, searchItem);
					searchList.Add(searchItem);
					searchItem.OnClick(item.Selected);

					foreach (Category category in item.Categories)
					{
						if (!categoryDict.TryGetValue(category.Id, out Lyst<UiComponent> list))
						{
							list = [];
							categoryDict.Add(category.Id, list);
							categories.Add(category);
						}

						Button child = item.CreateUi();
						child.ObserveVisible(panel.Body, () => searchItem.IsVisible());
						child.OnClick(item.Selected);
						list.Add(child);
					}
				}
				catch (Exception e)
				{
					Log.Error($"[PickNewModule] Failed to create ui for module: {item.Id}");
					Log.Exception(e);

					searchList.Add(new PanelRow() { new Label(item.Strings.Name).Class(Cls.error) });
				}
			}
			//Log.Info($"[PickNewModule] Total {searchDict.Count} modules");

			SideTabContainer tabs = panel.Body.AddAndReturn(new SideTabContainer())
				.Height(600.px())
				.Width(600.px());

			Log.Info($"[PickNewModule] All");
			Column dataColumn = new Column();
			dataColumn.Width(Percent.Hundred);
			dataColumn.Height(Px.Auto);
			dataColumn.Add(searchList);
			tabs.AddTab("All".ToDoLoc(), dataColumn);

			Log.Info($"[PickNewModule] Categories");
			foreach (Category cat in categories.OrderBy(c => c.Name)) {
				Column catDataColumn = new Column();
				catDataColumn.Width(Percent.Hundred);
				catDataColumn.Height(Px.Auto);
				catDataColumn.MinHeight(Px.Auto);
				catDataColumn.Add(categoryDict[cat.Id]);
				tabs.AddTab(cat.Name.ToDoLoc(), catDataColumn);
			}

			search.OnValueChanged(s =>
			{
				if (s.IsNullOrEmpty())
				{
					foreach (var item in searchDict.Values)
					{
						item.SetVisible(true);
					}
				}
				else
				{
					foreach (var item in searchDict)
					{
						item.Value.SetVisible(item.Key.Contains(s, StringComparison.InvariantCultureIgnoreCase));
					}
				}
			});

			panel.Body.AddAndReturn(new UiComponent()).FlexGrow(1);

			this.Height(Px.Auto);
			this.Width(600.px());
		}
	}

}