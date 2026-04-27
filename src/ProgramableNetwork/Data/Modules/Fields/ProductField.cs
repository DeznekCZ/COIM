using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork.Ui
{
    internal class ProductField : IField
    {
        private Func<Module, ProductProto, bool> filter;

        public ProductField(string id, Proto.Str strs, Func<Module, ProductProto, bool> filter)
        {
            this.Id = id;
            this.Name = strs.Name;
            this.ShortDesc = strs.DescShort;
            this.filter = filter;
        }

        public string Id { get; }

        public LocStr Name { get; }
        public LocStr ShortDesc { get; }

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

            fieldContainer.Row(this, module, out _).Add(productPicker);
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