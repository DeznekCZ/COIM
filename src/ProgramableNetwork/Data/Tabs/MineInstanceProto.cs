using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Entities;
using Mafi.Localization;

namespace ProgramableNetwork
{
    internal class MineInstanceProto : IProtoWithIcon
    {
        public MineInstanceProto(WorldMapMine mine, AMDataBandChannel dataBandChannel)
        {
            Mine = mine;
            IconPath = mine.Prototype.IconPath;
            Strings = GetStrings(mine, dataBandChannel);
            Id = new Proto.ID(mine.Prototype.Id.Value + "_" + mine.Id.Value);
        }

        public static Proto.Str GetStrings(WorldMapMine mine, AMDataBandChannel dataBandChannel)
        {
            if (mine is null) return new Proto.Str(LocalizationManager.GetLocalizedString0Arg("name_empty", "No selection", "", true, true));
            return new Proto.Str(LocalizationManager.GetLocalizedString0Arg(
                "name_" + mine.CustomTitle.Value,
                (mine.CustomTitle.HasValue ? mine.CustomTitle.Value : mine.Prototype.Strings.Name.TranslatedString) +
                "\n(distance: " + dataBandChannel.Distance(mine).IntegerPart + " km," +
                " error: " + dataBandChannel.ErrorPossibility(mine) + ")" ,
                "", true, true));
        }

        public WorldMapMine Mine { get; internal set; }

        public string IconPath { get; }

        public Proto.Str Strings { get; }

        public Proto.ID Id { get; }

        public bool IsAvailable => true;

        public bool IsNotAvailable => !IsAvailable;

        public IMod Mod { get; }

        public bool TryGetParam<T>(out T paramValue) where T : class
        {
            paramValue = default;
            return false;
        }
    }
}