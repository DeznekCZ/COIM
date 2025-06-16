using Mafi;
using Mafi.Core;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Collections.Generic;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork.Ui
{
    public class PickNewModule : FloatingColumn
    {
        private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();

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

            ScrollColumn dataColumn = panel.Body.AddAndReturn(new ScrollColumn());
            dataColumn.Width(Px.Auto);
            dataColumn.Height(600.px());

            Log.Info($"[PickNewModule] Generating hashset");
            Dictionary<string, Button> searchDict = new Dictionary<string, Button>();
            List<UiComponent> searchList = new List<UiComponent>();
            foreach (var item in protos)
            {
                try
                {
                    Button child = item.CreateUi();
                    searchDict.Add(item.SearchString, child);
                    searchList.Add(child);
                    child.OnClick(item.Selected);
                }
                catch (Exception e)
                {
                    Log.Error($"[PickNewModule] Failed to create ui for module: {item.Id}");
                    Log.Exception(e);

                    searchList.Add(new PanelRow() { new Label(item.Strings.Name).Class(Cls.error) });
                }
            }
            Log.Info($"[PickNewModule] Total {searchDict.Count} modules");

            Log.Info($"[PickNewModule] Constructing UI");
            dataColumn.Add(searchList);

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

            Log.Info($"[PickNewModule] Opening");
            Open(button);
        }
    }
}