using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Syncers;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ProgramableNetwork
{
    public class ProtoTab<T> : Row/*, IRefreshable*/
        where T : EntityProto, IProtoWithIcon
    {
        private readonly string m_fieldId;
        private readonly Module m_module;
        private readonly Func<T, bool> m_filter;
        private readonly Action m_refresh;
        private readonly Window m_window;
        private readonly UiContext m_UiContext;
        private ButtonIcon m_btnPreview;
        private ButtonIcon m_btnClear;
        private ProtoPickerPopup<T> m_protoPicker;

        public ProtoTab(UiContext uiContext, Module module, string fieldId, Func<Module, T, bool> filter,
            Action refresh, Window parentWindow, ControllerInspector inspector)
            : base()
        {
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (e) => filter.Invoke(m_module, e);
            m_refresh = refresh;
            m_window = parentWindow;
            m_UiContext = uiContext;
            parentWindow.OnCloseStart += ParentWindow_OnCloseStart;

            m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
            m_btnPreview.Size(40, 40);
            m_btnPreview.OnClick(FindProduct);
            Add(m_btnPreview);

            m_btnClear = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png);
            m_btnClear.Size(20, 40);
            m_btnClear.OnClick(() => {
                m_module.Field[m_fieldId] = Fix32.Zero;
                m_module.Field[m_fieldId, false] = "";
                m_refresh();
            });
            m_btnClear.Visible(false);
            Add(m_btnClear);

            m_protoPicker = new ProtoPickerPopup<T>(
                optionsProvider: GetItems,
                optionViewFactory: (item) => new ButtonIcon(item.IconPath).Tooltip(item.Strings.Name),
                onOptionSelected: (product) =>
                {
                    m_module.Field[m_fieldId] = FixSavedGames.GetPrototypeString(product.Id.Value);
                    m_module.Field[m_fieldId, false] = product.Id.Value;
                    m_refresh();
                },
                button: m_btnPreview,
                title: Tr.ProductSelectorTitle,
                config: new ProtoPickerConfig
                {
                    ItemSize = new Vector2(60, 60),
                    ItemsPerRow = 6,
                }
            );

            this.Observe(() => m_module.Field[m_fieldId])
                .Do((item) => Refresh());
        }

        private void ParentWindow_OnCloseStart(Window obj)
        {
            m_protoPicker.Close();
        }

        protected override void OnDetached()
        {
            m_window.OnCloseStart -= ParentWindow_OnCloseStart;
            base.OnDetached();
        }

        private IEnumerable<T> GetItems()
        {
            return m_UiContext.ProtosDb
                .All<T>()
                .Where(p => p.IsAvailable)
                .Where(m_filter);
        }

        private void FindProduct()
        {
            m_protoPicker.Show();
        }

        public void Refresh()
        {
            string slimId = m_module.Field[m_fieldId, ""];

            if (string.IsNullOrEmpty(slimId))
            {
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                return;
            }

            T foundProduct = m_module.Context.ProtosDb
                .Get<T>(new Proto.ID(slimId)).ValueOrNull;

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