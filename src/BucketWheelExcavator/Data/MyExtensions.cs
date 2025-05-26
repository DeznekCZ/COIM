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
using System;
using System.Linq;
using System.Reflection;
using static Mafi.Unity.Assets.Unity;

namespace BucketWheelExcavator
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
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
                return UserInterface.General.Empty128_png;
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
        public static void DoWhen<T>(this TriggerBuilder<T> triggerBuilder, Func<T, bool> filter, Action<T> action)
        {
            triggerBuilder.Do((t) =>
            {
                if (filter(t))
                    action(t);
            });
        }
        public static void DoWhen<T1, T2>(this TriggerBuilder<T1, T2> triggerBuilder, Func<T1, T2, bool> filter, Action<T1, T2> action)
        {
            triggerBuilder.Do((t1, t2) =>
            {
                if (filter(t1, t2))
                    action(t1, t2);
            });
        }
        public static void DoWhen<T1, T2, T3>(this TriggerBuilder<T1, T2, T3> triggerBuilder, Func<T1, T2, T3, bool> filter, Action<T1, T2, T3> action)
        {
            triggerBuilder.Do((t1, t2, t3) =>
            {
                if (filter(t1, t2, t3))
                    action(t1, t2, t3);
            });
        }
    }
}
