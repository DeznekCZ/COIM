using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Core.Syncers;
using Mafi.Core.UnlockingTree;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Linq;
using System.Reflection;

namespace BucketWheelExcavator
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
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

        public static T SetSize<T>(this T element, int x, int y) where T : IUiElement
        {
            return element.SetSize(new UnityEngine.Vector2(x, y));
        }

        public static StackContainer AllignRight<T>(this T element, UiBuilder builder) where T : IUiElement
        {
            return builder.NewStackContainer(element.GameObject.name + "_invert")
                .SetStackingDirection(StackContainer.Direction.RightToLeft)
                .SetSizeMode(StackContainer.SizeMode.Dynamic);
        }

        public static string GetIcon(this IEntity entity)
        {
            if (entity is LayoutEntityBase positionedForGraphics)
                return positionedForGraphics.Prototype.Graphics.IconPath;

            else if (entity is DynamicGroundEntity dynamicForGraphics)
                return dynamicForGraphics.Prototype.Graphics.IconPath;

            else if (entity is Transport transportForGraphics)
                return transportForGraphics.Prototype.Graphics.IconPath;

            else
                return new IconsPaths().Empty;
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

        public static string ModuleId(this string id)
        {
            return "ProgramableNetwork_Module_" + id;
        }

        public static TElement ToolTip<TElement>(this TElement element, ITooltipInspector inspector, string toolTip, Offset? offset = null, bool attached = false)
            where TElement : IUiElementWithHover<TElement>
        {
            return element.SetOnMouseEnterLeaveActions(() => inspector.OnMouseIn(toolTip, offset, attached ? element : (IUiElement)null), inspector.OnMouseOut);
        }

        public static TElement ToolTip<TElement>(this TElement element, ITooltipInspector inspector, Func<string> toolTip, Offset? offset = null, bool attached = false)
            where TElement : IUiElementWithHover<TElement>
        {
            return element.SetOnMouseEnterLeaveActions(() => inspector.OnMouseIn(toolTip(), offset, attached ? element : (IUiElement)null), inspector.OnMouseOut);
        }

        public static TElement ToolTip<TElement>(this TElement element, ITooltipInspector inspector, LocStr toolTip, Offset? offset = null, bool attached = false)
            where TElement : IUiElementWithHover<TElement>
        {
            return element.SetOnMouseEnterLeaveActions(() => inspector.OnMouseIn(toolTip.TranslatedString, offset, attached ? element : (IUiElement)null), inspector.OnMouseOut);
        }

        public static TElement ToolTip<TElement>(this TElement element, ITooltipInspector inspector, Func<LocStr> toolTip, Offset? offset = null, bool attached = false)
            where TElement : IUiElementWithHover<TElement>
        {
            return element.SetOnMouseEnterLeaveActions(() => inspector.OnMouseIn(toolTip().TranslatedString, offset, attached ? element : (IUiElement)null), inspector.OnMouseOut);
        }

        public static Btn SettingFieldNameStyle(this Btn element, UiBuilder builder)
        {
            element.SetButtonStyle(
                builder.Style.Global.GeneralBtn
                .Extend(
                    normalMaskClr: builder.Style.Panel.Background,
                    backgroundClr: builder.Style.Panel.Background,
                    hoveredClr: builder.Style.Panel.Background,
                    disabledMaskClr: builder.Style.Panel.Background,
                    pressedClr: builder.Style.Panel.Background,
                    foregroundClrWhenDisabled: ColorRgba.White,
                    border: new Mafi.Unity.UiFramework.Styles.BorderStyle(
                        ColorRgba.Black, 0
                    )
                )
                .ExtendText(
                    color: ColorRgba.White
                ));
            return element;
        }

        public static void SetCategories(this LayoutEntityProto.Gfx gfx, ImmutableArray<ToolbarCategoryProto> toolbarCategories)
        {
            if (gfx == null) return;
            var field = typeof(LayoutEntityProto.Gfx)
                .GetField("<Categories>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(gfx, toolbarCategories);
        }

        public static void AddUnlockable(this ResearchNodeProto node, params IProtoWithIcon[] entityProtos)
        {
            if (node == null) return;
            var field = typeof(ResearchNodeProto)
                .GetField("Units", BindingFlags.Public | BindingFlags.Instance);
            field.SetValue(node, node.Units.AsEnumerable().Concat(entityProtos.Select(e => new ProtoWithIconUnlock(e))).ToImmutableArray());
        }
        public static UpdaterBuilder DoWhen<T>(this TriggerBuilder<T> triggerBuilder, Func<T, bool> filter, Action<T> action)
        {
            return triggerBuilder.Do((t) =>
            {
                if (filter(t))
                    action(t);
            });
        }
        public static UpdaterBuilder DoWhen<T1, T2>(this TriggerBuilder<T1, T2> triggerBuilder, Func<T1, T2, bool> filter, Action<T1, T2> action)
        {
            return triggerBuilder.Do((t1, t2) =>
            {
                if (filter(t1, t2))
                    action(t1, t2);
            });
        }
        public static UpdaterBuilder DoWhen<T1, T2, T3>(this TriggerBuilder<T1, T2, T3> triggerBuilder, Func<T1, T2, T3, bool> filter, Action<T1, T2, T3> action)
        {
            return triggerBuilder.Do((t1, t2, t3) =>
            {
                if (filter(t1, t2, t3))
                    action(t1, t2, t3);
            });
        }
    }
}
