using Mafi.Core.Products;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
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
            this.ShortDesc = shortDesc;
            this.filter = filter;
        }

        public string Id { get; }

        public string Name { get; }
        public string ShortDesc { get; }

        public int Size => 40;

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
        {
            ProductTab protoTab = new ProductTab(uiContext, module, Id, filter, updateDialog, parentWindow);
            fieldContainer.Row(this).Add(protoTab);
        }

        public void InitData(Module module)
        {
            // nothing to validate
        }

        public void Validate(Module module)
        {
            // nothing to validate
        }
    }
}