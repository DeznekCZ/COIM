using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.World.Entities;
using Mafi.Localization;

namespace ProgramableNetwork
{
    internal class MineInstanceProto : IProtoWithIcon
    {
        public MineInstanceProto(WorldMapMine mine)
        {
            Mine = mine;
            IconPath = mine.Prototype.IconPath;
            Strings = mine.CustomTitle.HasValue
                ? new Proto.Str(LocalizationManager.GetLocalizedString0Arg("name_" + mine.CustomTitle.Value, mine.CustomTitle.Value, "", true, true))
                : new Proto.Str(mine.Prototype.Strings.Name);
            Id = new Proto.ID(mine.Prototype.Id.Value + "_" + mine.Id.Value);
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