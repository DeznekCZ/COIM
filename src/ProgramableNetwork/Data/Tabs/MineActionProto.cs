using Mafi.Base;
using Mafi.Core.Localization.Quantity;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Entities;
using Mafi.Localization;
using Mafi.Unity.UserInterface.Style;
using System;
using System.Reflection;
using static Mafi.Base.Assets.Base;
using static ProgramableNetwork.AMDataBandChannel;

namespace ProgramableNetwork
{
    internal class MineActionProto : IProtoWithIcon
    {
        private string v;

        public MineActionProto(AMOperation item, AMDataBandChannel channel)
        {
            Id = new Proto.ID("Progrmable_Network_AM_" + item.ToString());
            Strings = new Proto.Str(LocalizationManager.GetLocalizedString0Arg(Id.Value, GetName(item, channel), "", true, true));
            Value = item;
            IconPath = GetIconPath(item, channel);
        }

        public static string GetIconPath(AMOperation item, AMDataBandChannel channel)
        {
            switch (item)
            {
                case AMOperation.ReadQuantity:
                    return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Storage_svg;
                case AMOperation.ReadCapacity:
                    return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Storage_svg;
                case AMOperation.ReadUsage:
                    return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Storage_svg;
                case AMOperation.ReadProduct:
                    if (channel.WorldMapMine is null)
                        return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Storage_svg;
                    else
                        return channel.WorldMapMine.Product.IconPath;
                case AMOperation.WritePause:
                case AMOperation.ReadPause:
                    return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Pause_png;
                case AMOperation.WriteProduction:
                    return Mafi.Unity.Assets.Unity.UserInterface.EntityIcons.Worker_png;
                default:
                    return Mafi.Unity.Assets.Unity.UserInterface.General.Empty128_png;
            }
        }

        public static string GetName(AMOperation item, AMDataBandChannel channel)
        {
            switch (item)
            {
                case AMOperation.ReadProduct:
                    if (channel.WorldMapMine is null || !channel.WorldMapMine.CustomTitle.HasValue)
                        return typeof(AMOperation).GetField(item.ToString()).GetCustomAttribute<AMNameAttribute>().Name;
                    else
                        return channel.WorldMapMine.CustomTitle.HasValue
                            ? channel.WorldMapMine.CustomTitle.Value
                            : channel.WorldMapMine.Prototype.Strings.Name.TranslatedString;

                default:
                    return typeof(AMOperation).GetField(item.ToString()).GetCustomAttribute<AMNameAttribute>().Name;
            }
        }

        public string IconPath { get; }

        public Proto.Str Strings { get; }

        public Proto.ID Id { get; }

        public bool IsAvailable => true;

        public bool IsNotAvailable => false;

        public IMod Mod => null;

        public AMOperation Value { get; }

        public bool TryGetParam<T>(out T paramValue) where T : class
        {
            paramValue = null;
            return false;
        }
    }
}