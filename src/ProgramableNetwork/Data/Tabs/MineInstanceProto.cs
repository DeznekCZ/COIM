using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.World;
using Mafi.Core.World.Entities;
using Mafi.Localization;

namespace ProgramableNetwork
{
    /// <summary>
    /// Pickable AM-channel source.  Wraps either a <see cref="WorldMapMine"/> (the original
    /// purpose) or the player's <see cref="BattleShip"/> so both can appear in the same
    /// source-picker list and bind to the channel through the existing UI flow.
    /// </summary>
    internal class MineInstanceProto : IProtoWithIcon
    {
        public MineInstanceProto(WorldMapMine mine, AMDataBandChannel dataBandChannel)
        {
            Mine = mine;
            Ship = null;
            IconPath = mine.Prototype.IconPath;
            Strings = GetStrings(mine, dataBandChannel);
            Id = new Proto.ID(mine.Prototype.Id.Value + "_" + mine.Id.Value);
        }

        public MineInstanceProto(BattleShip ship, AMDataBandChannel dataBandChannel)
        {
            Mine = null;
            Ship = ship;
            // The ship's prototype lives on the entity; reuse its icon path.
            IconPath = ship.Prototype.Graphics.IconPath;
            Strings = GetShipStrings(ship);
            Id = new Proto.ID("ship_" + ship.Id.Value);
        }

        public static Proto.Str GetStrings(WorldMapMine mine, AMDataBandChannel dataBandChannel)
        {
            if (mine is null) {
				return new Proto.Str(LocalizationManager.GetLocalizedString0Arg("name_empty", "No selection", "", true, true));
			}
			return new Proto.Str(LocalizationManager.GetLocalizedString0Arg(
                "name_" + mine.CustomTitle.Value,
                (mine.CustomTitle.HasValue ? mine.CustomTitle.Value : mine.Prototype.Strings.Name.TranslatedString) +
                "\n(distance: " + dataBandChannel.Distance(mine).IntegerPart + " km," +
                " error: " + dataBandChannel.ErrorPossibility(mine) + ")" ,
                "", true, true));
        }

        public static Proto.Str GetShipStrings(BattleShip ship)
        {
            if (ship is null) {
                return new Proto.Str(LocalizationManager.GetLocalizedString0Arg("name_empty", "No selection", "", true, true));
            }
            string baseName = ship.CustomTitle.HasValue
                ? ship.CustomTitle.Value
                : ship.Prototype.Strings.Name.TranslatedString;
            return new Proto.Str(LocalizationManager.GetLocalizedString0Arg(
                "name_ship_" + ship.Id.Value,
                baseName + "\n(main ship — crew, HP, fuel reads)",
                "", true, true));
        }

        public WorldMapMine Mine { get; internal set; }
        public BattleShip Ship { get; internal set; }
        public bool IsShip => Ship != null;
		public bool IsObsolete => false;

        public string IconPath { get; }

        public Proto.Str Strings { get; }

        public Proto.ID Id { get; }

        public bool IsAvailable => true;

        public bool IsNotAvailable => !IsAvailable;

        public IMod Mod { get; }

        public bool IsInitialized => true;

        public bool IsLocked => false;

        public bool IsUnlocked => true;

        public bool IsUnlockedAndAvailable => false;

        public bool IsLockedOrUnavailable => true;

        public bool TryGetParam<T>(out T paramValue) where T : class
        {
            paramValue = default;
            return false;
        }
    }
}
