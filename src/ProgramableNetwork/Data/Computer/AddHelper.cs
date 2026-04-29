using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork.Ui
{
    public class AddHelper
    {
        private Func<ControllerView> m_controllerView;

        public AddHelper(Func<ControllerView> controllerView)
        {
            this.m_controllerView = controllerView;
        }

        public Option<UiComponent> Display()
        {
            Column column = new Column(gap: 2.pt());

            Row visible = new Row(gap: 2.pt())
            {
                new Label().LaterText(() => NewTr.Inspector.Shift, column).Class(Cls.window__title).TinyFontSize().FlexGrow(0.2f),
                new Label("+".AsLoc()).TinyFontSize().TextAlign(TextAlignment.CenterMiddle),
                new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
                new Label().LaterText(() => NewTr.Inspector.AddLastCreated, column).TinyFontSize().FlexGrow(0.6f)
            };

            column.Add(new Row(gap: 2.pt())
            {
                new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
                new Label().LaterText(() => NewTr.Inspector.AddNewModule, column).TinyFontSize().FlexGrow(0.8f)
            });
            column.Add(visible);
            column.Add(new Row(gap: 2.pt())
            {
                new Icon(UserInterface.General.RightClick128_png).FlexGrow(0.2f),
                new Label().LaterText(() => NewTr.Inspector.AddFromTemplate, column).TinyFontSize().FlexGrow(0.8f)
            });

            column.Observe(() => m_controllerView().LastCreated)
                  .Do((module) => visible.SetVisible(module != null));

            return column;
        }
    }
}