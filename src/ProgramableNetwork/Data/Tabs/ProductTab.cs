using Mafi;
using Mafi.Core;
using Mafi.Core.Products;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public class ProductTab : Row/*, IRefreshable*/
    {
        private readonly string m_fieldId;
        private readonly Module m_module;
        private readonly Func<ProductProto, bool> m_filter;
        private readonly Action m_refresh;
        private ButtonIcon m_btnPreview;
        private ButtonIcon m_btnClear;
        private ProtoPickerPopup<ProductProto> m_protoPicker;

        public ProductTab(UiContext uiContext, Module module, string fieldId, Func<Module, ProductProto, bool> filter,
            Action refresh, Window parentWindow)
            : base()
        {
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (proto) => filter.Invoke(m_module, proto);
            m_refresh = refresh;

            this.Size(80, 40);

            m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
                .Size(40, 40)
                .OnClick(FindProduct)
                .Tooltip(Tr.Empty);
            Add(m_btnPreview);

            m_btnClear = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png)
                .Size(40, 40)
                .OnClick(() =>
                {
                    m_module.Field[m_fieldId] = Fix32.Zero;
                    m_btnClear.Visible(false);
                    m_refresh();
                });
            Add(m_btnClear);

            m_protoPicker = new ProtoPickerPopup<ProductProto>(
                optionsProvider: () => uiContext.ProtosDb
                        .All<ProductProto>()
                        .Where(p => p.IsAvailable)
                        .Where(m_filter)
                        .ToList(),
                optionViewFactory: (product) =>
                {
                    return new ButtonIconText(product.IconPath, product.Strings.Name)
                        .Tooltip(product.Strings.DescShort)
                        .Size(height: 60.px());
                },
                onOptionSelected: (product) =>
                {
                    m_module.Field[m_fieldId] = Fix32.FromRaw(product.SlimId.Value);
                    m_refresh();
                },
                button: m_btnPreview,
                title: Tr.Products,
                config: new ProtoPickerConfig
                {
                    ItemSize = new UnityEngine.Vector2(60, 60),
                    ItemsPerRow = 5
                },
                orderAlphabetically: true,
                searchable: true
            );
            parentWindow.OnCloseStart += ParentWindow_OnCloseStart;

            this.Observe(() => m_module.Field[m_fieldId])
                .Do((v) => Refresh());
        }

        private void ParentWindow_OnCloseStart(Window obj)
        {
            m_protoPicker.Close();
        }

        private void FindProduct()
        {
            m_protoPicker.Show();
        }

        public void Refresh()
        {
            int slimId = m_module.Field[m_fieldId, Fix32.Zero].RawValue;

            if (slimId == 0)
            {
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                return;
            }

            ProductProto foundProduct = m_module.Context.ProtosDb
                .Filter<ProductProto>(product => product.SlimId.Value == slimId)
                .FirstOrDefault();

            if (foundProduct == null)
            {
                m_module.Field[m_fieldId] = Fix32.Zero;
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                return;
            }

            m_btnPreview.Icon.Value(foundProduct.IconPath);
            m_btnClear.Visible(true);
        }
    }
}