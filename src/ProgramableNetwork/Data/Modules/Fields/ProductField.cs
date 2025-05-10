using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

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
            SingleProductPickerUi productPicker = new SingleProductPickerUi(
                allAvailableProducts: () => uiContext.ProtosDb
                        .All<ProductProto>()
                        .Where(p => p.IsAvailable)
                        .Where(p => filter.Invoke(module, p)),
                onProductSelected: (product) =>
                {
                    module.Field[Id] = Fix32.FromRaw(product.SlimId.Value);
                    updateDialog();
                },
                selectedProduct: () => module.Field.Product(Id).CreateOption()
            );

            fieldContainer.Row(this).Add(productPicker);
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