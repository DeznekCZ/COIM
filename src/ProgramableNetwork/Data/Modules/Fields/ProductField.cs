using Mafi.Core.Products;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

namespace ProgramableNetwork
{
    internal class ProductField : IField
    {
        private Func<Module, ProductProto, bool> filter;

        public ProductField(string id, string name, string shortDesc, Func<Module, ProductProto, bool> filter)
        {
            this.Id = id;
            this.Name = name;
            this.shortDesc = shortDesc;
            this.filter = filter;
        }

        public string Id { get; }

        public string Name { get; }
        public string shortDesc { get; }

        public int Size => 40;

        public void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(40);

            var txt = uiBuilder
                .NewBtnGeneral("name")
                .SettingFieldNameStyle(uiBuilder)
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(40)
                .SetText(Name)
                .ToolTip(inspector, shortDesc, attached: true)
                .AppendTo(fieldContainer);

            ProductTab productTab = new ProductTab(uiBuilder, module, Id, filter, updateDialog, parentWindow, inspector.Context);
            productTab.AppendTo(fieldContainer);
        }

        public void Validate(Module module)
        {
            // nothing to validate
        }
    }
}