using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Terrain;
using Mafi.Localization;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface.Style;

namespace MultiplayerContracts
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static ProductQuantity Of(this int count, ProductProto.ID id, ProtosDb protosDb) {
            return QuantityExtensions.Of(count, protosDb.Get<ProductProto>(id).ValueOrThrow($"missing product: {id.Value}"));
        }

        public static Proto.ID TradeContract(this ProductProto.ID product)
        {
            return new Proto.ID($"MPC_{product.Value}_1000");
        }

        public static ProductQuantity TradeQuantity(this ProductProto.ID product, int amount, ProtosDb protosDb)
        {
            return amount.Of(product, protosDb);
        }

        public static T AppendTo<T>(this T element, StackContainer stackContainer, ContainerPosition? containerPosition) where T : IUiElement
        {
            stackContainer.Append(element, element.GetSize(), containerPosition, default, false);
            return element;
        }

        public static T AddToGrid<T>(this T element, GridContainer gridContainer) where T : IUiElement
        {
            gridContainer.Append(element);
            return element;
        }

        public static T AppendTo<T>(this T element, MyTabContainer tabContainer, string tabText, out Tab tab) where T : IUiElement
        {
            tabContainer.AddTab(new LocStrFormatted(tabText), element, out tab);
            return element;
        }

        public static T AppendTo<T>(this T element, MyTabContainer tabContainer, LocStrFormatted tabText, out Tab tab) where T : IUiElement
        {
            tabContainer.AddTab(tabText, element, out tab);
            return element;
        }

        public static T SetSize<T>(this T element, int x, int y) where T : IUiElement
        {
            return element.SetSize(new UnityEngine.Vector2(x, y));
        }

        public static string GetIcon(this IEntity entity)
        {
            if (entity is LayoutEntity positionedForGraphics)
                return positionedForGraphics.Prototype.Graphics.IconPath;

            else if (entity is DynamicGroundEntity dynamicForGraphics)
                return dynamicForGraphics.Prototype.Graphics.IconPath;

            else if (entity is Transport transportForGraphics)
                return transportForGraphics.Prototype.Graphics.IconPath;

            else
                return new UiStyle().Icons.Empty;
        }

        public static bool HasPosition(this IEntity entity, out Tile3f position)
        {
            if (entity is IEntityWithPosition positioned)
            {
                position = positioned.Position3f;
                return true;
            }

            if (entity is DynamicGroundEntity dynamic)
            {
                position = dynamic.Position3f;
                return true;
            }

            position = Tile3f.Zero;
            return false;
        }

        public static bool HasPosition(this IEntity entity, out Tile2f position)
        {
            if (entity is IEntityWithPosition positioned)
            {
                position = positioned.Position2f;
                return true;
            }

            if (entity is DynamicGroundEntity dynamic)
            {
                position = dynamic.Position2f;
                return true;
            }

            position = Tile2f.Zero;
            return false;
        }
    }
}
