using Mafi;
using Mafi.Core;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
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
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Mafi.Unity.Assets.Unity.Generated.Icons;

namespace ProgramableNetwork.Ui
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
        private readonly DisplayWithIcon m_btnFuelPreview;
        private DisplayWithIcon m_btnPreview;
        private ButtonIcon m_btnClear;
        private FloatingColumn m_protoPicker;

        private readonly bool m_directEdit;

        public ProtoTab(UiContext uiContext, Module module, string fieldId, Func<Module, T, bool> filter,
            Action refresh, Window parentWindow, ControllerInspector inspector, bool directEdit = false)
            : base()
        {
            m_fieldId = fieldId;
            m_module = module;
            m_filter = (e) => filter.Invoke(m_module, e);
            m_refresh = refresh;
            m_window = parentWindow;
            m_UiContext = uiContext;
            m_directEdit = directEdit;
            parentWindow.OnCloseStart += ParentWindow_OnCloseStart;

            m_btnPreview = new DisplayWithIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
            m_btnPreview.Icon.Margin(0);
            m_btnPreview.Icon.Size(Sizes.IMAGE_SIZE * 1.5f, Sizes.IMAGE_SIZE * 1.5f);
            m_btnPreview.Icon.Padding(0);
            m_btnPreview.Margin(0);
            m_btnPreview.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE * 1.5f);
            Add(m_btnPreview);

            ButtonIcon selectionButton = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Edit_svg);
            selectionButton.Height(Sizes.BLOCK_SIZE * 1.5f);
            selectionButton.OnClick(FindProduct);
            Add(selectionButton);

            m_btnClear = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png);
            m_btnClear.Height(Sizes.BLOCK_SIZE * 1.5f);
            m_btnClear.OnClick(() => {
                if (m_directEdit) {
                    m_module.FieldNumberData.TryRemove(m_fieldId, out _);
                    m_module.StringData.TryRemove("field__" + m_fieldId, out _);
                } else {
                    m_UiContext.InputScheduler.ScheduleInputCmd(new ModuleClearFieldCmd(
                        m_module.Controller.Id, m_module.Id, m_fieldId));
                }
                m_refresh();
            });
            m_btnClear.Visible(false);
            Add(m_btnClear);

            if (typeof(T).IsAssignableTo(typeof(DrivingEntityProto)))
            {
                m_btnFuelPreview = new DisplayWithIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnFuelPreview.Icon.Margin(0);
                m_btnFuelPreview.Icon.Size(Sizes.IMAGE_SIZE * 1.5f, Sizes.IMAGE_SIZE * 1.5f);
                m_btnFuelPreview.Icon.Padding(0);
                m_btnFuelPreview.Margin(0);
                m_btnFuelPreview.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE * 1.5f);
                InsertAt(1, m_btnFuelPreview);

                var veh = m_UiContext.ProtosDb.Get<DrivingEntityProto>(new Proto.ID(m_module.Field[m_fieldId, false])).ValueOrNull;
                m_btnPreview.Icon.Value(veh);
                m_btnFuelPreview.Icon.Value(veh?.FuelTankProto.Value?.Product);
                m_protoPicker = new ProtoPickerPopup<DrivingEntityProto>(
                    optionsProvider: (() => GetItems().Select(d => d is DrivingEntityProto p ? p : null)),
                    optionViewFactory: ProtoPickerFactories.VehicleFactory,
                    onOptionSelected: (DrivingEntityProto product) =>
                    {
                        applyProtoSelection(product.Id.Value);
                        m_refresh();
                    },
                    button: selectionButton,
                    title: Tr.SelectVehicle_Title,
                    config: ProtoPickerConfig.Vehicles
                );
            }
            else if (typeof(T).IsAssignableTo(typeof(ProductProto)))
            {
                m_btnPreview.Icon.Value(m_UiContext.ProtosDb.Get<ProductProto>(new Proto.ID(m_module.Field[m_fieldId, false])).ValueOrNull);
                m_protoPicker = new ProtoPickerPopup<ProductProto>(
                    optionsProvider: (() => GetItems().Select(d => d is ProductProto p ? p : null)),
                    optionViewFactory: ProtoPickerFactories.ProductFactory,
                    onOptionSelected: (ProductProto product) =>
                    {
                        applyProtoSelection(product.Id.Value);
                        m_refresh();
                    },
                    button:  selectionButton,
                    title: Tr.ProductSelectorTitle,
                    config:  ProtoPickerConfig.Products
                );
            }
            else
            {
                m_btnPreview.Icon.Value(m_UiContext.ProtosDb.Get<T>(new Proto.ID(m_module.Field[m_fieldId, false])).ValueOrNull);
                m_protoPicker = new ProtoPickerPopup<T>(
                    optionsProvider: GetItems,
                    optionViewFactory: (item) => new ButtonIcon(item.IconPath)
                        .Tooltip(item.Strings.Name)
                        .AsProtoPickerOptionButton(),
                    onOptionSelected: (product) =>
                    {
                        applyProtoSelection(product.Id.Value);
                        m_refresh();
                    },
                    button: selectionButton,
                    title: Tr.ProductSelectorTitle,
                    config: new ProtoPickerConfig
                    {
                        ItemSize = new Vector2(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE * 2),
                        ItemsPerRow = 6,
                    }
                );
            }

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

        // Mirror of the ControllerCommandExecutor's dual-write path for
        // ModuleSetFix32FieldCmd + ModuleSetStringFieldCmd applied to the same
        // field — used by every onOptionSelected branch.  When directEdit, write
        // straight to the data dicts (the module isn't owned by a Controller yet
        // so the cmd targeting Controller.Id would no-op).
        private void applyProtoSelection(string protoIdValue)
        {
            if (m_directEdit) {
                m_module.Field[m_fieldId] = FixSavedGames.GetPrototypeString(protoIdValue);
                m_module.Field[m_fieldId, false] = protoIdValue;
            } else {
                m_UiContext.InputScheduler.ScheduleInputCmd(new ModuleSetFix32FieldCmd(
                    m_module.Controller.Id, m_module.Id, m_fieldId, FixSavedGames.GetPrototypeString(protoIdValue)));
                m_UiContext.InputScheduler.ScheduleInputCmd(new ModuleSetStringFieldCmd(
                    m_module.Controller.Id, m_module.Id, m_fieldId, protoIdValue));
            }
        }

        private void FindProduct()
        {
            m_protoPicker.Show();
        }

        public void Refresh()
        {
            string fullId = m_module.Field[m_fieldId, ""];

            if (string.IsNullOrEmpty(fullId))
            {
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnFuelPreview?.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                return;
            }

            T foundProduct = m_module.Context.ProtosDb
                .Get<T>(new Proto.ID(fullId)).ValueOrNull;

            if (foundProduct == null)
            {
                m_module.Field[m_fieldId] = Fix32.Zero;
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnFuelPreview?.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                return;
            }

            m_btnPreview.Icon.Value(foundProduct);
            if (foundProduct is DrivingEntityProto veh) {
				m_btnFuelPreview.Icon.Value(veh.FuelTankProto.ValueOrNull?.Product);
			}
			m_btnClear.Visible(true);
        }
    }
}