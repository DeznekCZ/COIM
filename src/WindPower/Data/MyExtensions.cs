using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Localization;
using Mafi.Unity;
using System;
using static Mafi.Unity.Assets.Unity.UserInterface;

namespace WindPower
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
                return General.Empty128_png;
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
