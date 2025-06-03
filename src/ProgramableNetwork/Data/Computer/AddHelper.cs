using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using static Mafi.Unity.Assets.Unity;

namespace ProgramableNetwork
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
            Row visible = new Row(gap: 2.pt())
            {
                new Label("Shift".AsLoc()).Class(Cls.window__title).FlexGrow(0.2f),
                new Label("+".AsLoc()).TextAlign(TextAlignment.CenterMiddle),
                new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
                new Label("Add last created / coppied".AsLoc()).FlexGrow(0.6f)
            };

            Column column = new Column(gap: 2.pt())
            {
                new Row(gap: 2.pt())
                {
                    new Icon(UserInterface.General.LeftClick128_png).FlexGrow(0.2f),
                    new Label("Add new module".AsLoc()).FlexGrow(0.8f)
                },
                visible,
                new Row(gap: 2.pt())
                {
                    new Icon(UserInterface.General.RightClick128_png).FlexGrow(0.2f),
                    new Label("Add from template".AsLoc()).FlexGrow(0.8f)
                },
            };
            column.Observe(() => m_controllerView().LastCreated)
                  .Do((module) => visible.SetVisible(module != null));

            return column;
        }
    }
}