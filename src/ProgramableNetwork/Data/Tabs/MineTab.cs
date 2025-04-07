using Mafi;
using Mafi.Core.World.Entities;
using Mafi.Unity.Ui;
using Mafi.Unity.Ui.Library;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public class MineTab : Row/*, IRefreshable*/
    {
        private readonly AMDataBandChannel m_fieldId;
        private readonly Antena m_module;
        private readonly AntenaInspector m_inspector;
        private readonly Action m_refresh;
        private readonly Fix32 m_distanceBoost;
        private readonly Row m_btnPreviewHolder;
        private ButtonIcon m_btnPreview;
        private ButtonIcon m_btnClear;
        private ProtoPickerPopup<MineInstanceProto> m_protoPicker;

        public MineTab(UiContext uiContext, Antena module, AMDataBandChannel fieldId, Fix32 distanceBoost,
            Window parentWindow, AntenaInspector inspector, Action refresh)
            : base()
        {
            m_fieldId = fieldId;
            m_module = module;
            m_inspector = inspector;
            m_refresh = refresh;
            m_distanceBoost = distanceBoost;

            this.Size(80, 40);

            m_btnPreview = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png)
                .Size(40, 40)
                .OnClick(FindProduct)
                .Tooltip(MineInstanceProto.GetStrings(m_fieldId.WorldMapMine, fieldId).Name);
            Add(m_btnPreview);

            m_btnClear = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png)
                .Size(40, 40)
                .OnClick(() =>
                {
                    m_fieldId.WorldMapMine = null;
                    m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                    m_btnClear.Visible(false);
                    m_refresh();
                });
            Add(m_btnClear);

            m_protoPicker = new ProtoPickerPopup<MineInstanceProto>(
                optionsProvider: () => m_module.Context.EntitiesManager
                    .GetAllEntitiesOfType<WorldMapMine>()
                    .Where(p => p.IsOwnedByPlayer)
                    .Select(p => new MineInstanceProto(p, m_fieldId))
                    .ToList(),
                optionViewFactory: (product) =>
                {
                    return new ButtonIconText(product.IconPath, product.Strings.Name)
                        .Tooltip(MineInstanceProto.GetStrings(m_fieldId.WorldMapMine, fieldId).DescShort)
                        .Size(height: 60.px());
                },
                onOptionSelected: (product) =>
                {
                    m_fieldId.WorldMapMine = product.Mine;
                    m_refresh();
                },
                button: m_btnPreview,
                title: new Mafi.Localization.LocStrFormatted("Select mine"),
                config: new ProtoPickerConfig
                {
                    ItemSize = new UnityEngine.Vector2(60, 60),
                    ItemsPerRow = 5
                },
                orderAlphabetically: true,
                searchable: true
            );

            Refresh();
        }

        private void FindProduct()
        {
            m_protoPicker.Show();
        }

        public void Refresh()
        {
            if (m_fieldId.WorldMapMine is null)
            {
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                m_refresh();
                return;
            }

            if (!m_fieldId.WorldMapMine.IsOwnedByPlayer)
            {
                m_fieldId.WorldMapMine = null;
                m_btnPreview.Icon.Value(Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png);
                m_btnClear.Visible(false);
                m_refresh();
                return;
            }

            m_btnPreview.Icon.Value(m_fieldId.WorldMapMine.Prototype.IconPath);
            m_btnClear.Visible(true);
            m_refresh();
        }
    }
}