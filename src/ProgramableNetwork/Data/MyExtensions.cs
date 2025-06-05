using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Dynamic;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Prototypes;
using Mafi.Unity.Audio;
using System;
using System.Reflection;
using UnityEngine;

namespace ProgramableNetwork
{
    public static class MyExtensions
    {
        public static ImmutableArray<T> ToImmutableArray<T>(this Option<T> option) where T : class
        {
            return new Lyst<T>() { option.ValueOrNull }.ToImmutableArray();
        }

        public static T With<T>(this T self, Action<T> action)
        {
            action(self);
            return self;
        }

        public static T As<T>(this object self)
        {
            return (T)self;
        }

        public static Option<T> AsOption<I, T>(this IEquatable<I> option, Func<I,T> converter)
            where I : class
            where T : class
        {
            return (option is Option<I> opt && opt.HasValue) ? converter(opt.Value).CreateOption() : Option.None;
        }

        public static string GetIcon(this IEntity entity)
        {
            if (entity is LayoutEntityBase positionedForGraphics)
                return positionedForGraphics.Prototype.Graphics.IconPath;

            else if (entity is DynamicGroundEntity dynamicForGraphics)
                return dynamicForGraphics.Prototype.Graphics.IconPath;

            else if (entity is Transport transportForGraphics)
                return transportForGraphics.Prototype.Graphics.IconPath;

            else if (entity is IProtoWithIcon iconified)
                return iconified.IconPath;

            else
                return Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png;
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

        public static string SerializationInfo(this IEntity entity, Controller computer)
        {
            if (!entity.HasPosition(out Tile3f position))
                return "";

            RelTile3f offset = position - computer.Position3f;
            return $"{entity.Prototype.Id.Value}:{offset}";
        }

        public static string ModuleId(this string id)
        {
            return "ProgramableNetwork_Module_" + id;
        }

        public static void SetCategories(this LayoutEntityProto.Gfx gfx, ImmutableArray<ToolbarEntryData> toolbarCategories)
        {
            if (gfx == null) return;
            var field = typeof(LayoutEntityProto.Gfx)
                .GetField("<Categories>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(gfx, toolbarCategories);
        }

        public static AudioSource InvalidOp(this AudioDb audios, bool shared = false)
        {
            if (shared)
                return audios.GetSharedAudioUi(Mafi.Unity.Assets.Unity.UserInterface.Audio.InvalidOp_prefab);
            else
                return audios.GetClonedAudio(Mafi.Unity.Assets.Unity.UserInterface.Audio.InvalidOp_prefab, AudioChannel.UserInterface);
        }
    }
}
