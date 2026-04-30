using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.World;
using Mafi.Core.World.Entities;
using Mafi.Serialization;
using Mafi.Unity.UiToolkit.Component;
using System;
using System.Linq;

namespace ProgramableNetwork
{
    public class AMDataBandChannel : IDataBandChannel
    {
        public int Index { get; set; }
        public Fix32? Value { get; set; }
        public int ValidIterations { get; set; }

        /// <summary>
        /// World-map mine bound to this channel.  Mutually exclusive with
        /// <see cref="BattleShip"/>; setting one clears the other.
        /// </summary>
        public WorldMapMine WorldMapMine
        {
            get => m_mine;
            set
            {
                m_sourceId = value?.Id ?? new EntityId(0);
                m_mine = value;
                m_ship = null;
            }
        }

        /// <summary>
        /// Player's main ship bound to this channel as the data source for ship-targeted
        /// <see cref="AMOperation"/> values.  Mutually exclusive with <see cref="WorldMapMine"/>.
        /// </summary>
        public BattleShip BattleShip
        {
            get => m_ship;
            set
            {
                m_sourceId = value?.Id ?? new EntityId(0);
                m_ship = value;
                m_mine = null;
            }
        }

        public Vector2i HomeLocation => GlobalDependencyResolver.Get<WorldMapManager>().Map.HomeLocation.Position;

        public AMDataBand OriginalDataBand { get; set; }

        public AMOperation Operation { get => m_operation; set => m_operation = value; }

        private WorldMapMine m_mine;
        private BattleShip m_ship;
        // Single saved source-entity ID; resolved at load time to either a WorldMapMine
        // or a BattleShip depending on which kind the entity actually is.
        private EntityId m_sourceId;

        private AMOperation m_operation;

        private IRandom random;

        public static void Serialize(AMDataBandChannel channel, BlobWriter writer)
        {
            // Wire layout unchanged from v5; the field previously named m_mineId is now
            // m_sourceId and may resolve to either a WorldMapMine or a BattleShip at load.
            writer.WriteByte(/*version*/5);
            writer.WriteInt(channel.Index);
            writer.WriteBool(channel.Value.HasValue);
            writer.WriteInt(channel.Value?.RawValue ?? 0);
            writer.WriteInt(channel.ValidIterations);
            writer.WriteInt(channel.m_sourceId.Value);
            writer.WriteInt((int)channel.m_operation);
        }

        public static AMDataBandChannel Deserialize(BlobReader reader)
        {
            var version = reader.ReadByte();
            int index = reader.ReadInt();
            Fix32? value = null;
            if (version < 3)
            {
                var array = reader.ReadArray<int>().Select(Fix32.FromInt).ToArray();
                if (array.Length > 0) {
					value = array[0];
				}
			}
            else if (version > 3)
            {
                bool exists = reader.ReadBool();
                value = Fix32.FromRaw(reader.ReadInt());
                if (!exists) {
					value = null;
				}
			}
            else
            {
                var array = reader.ReadArray<Fix32>();
                if (array.Length > 0) {
					value = array[0];
				}
			}

            int validIterations = reader.ReadInt();
            new EntityId(version > 0 && version < 5 ? reader.ReadInt() : 0); // ignore antena
            var sourceId = new EntityId(version > 3 ? reader.ReadInt() : 0);
            var operation = (AMOperation)(version > 3 ? reader.ReadInt() : 0);

            return new AMDataBandChannel()
            {
                Index = index,
                Value = value,
                ValidIterations = validIterations,
                m_sourceId = sourceId,
                m_operation = operation
            };
        }

        public void UpdateAntenaReference(AMDataBand self, IEntitiesManager manager)
        {
            OriginalDataBand = self;
            m_mine = null;
            m_ship = null;
            // Single non-generic lookup, then dispatch via `is` so we never hit the
            // typed-TryGetEntity "type didn't match" failure mode.
            var maybeEntity = manager.GetEntity(m_sourceId);
            if (maybeEntity.HasValue)
            {
                if (maybeEntity.Value is WorldMapMine mine)
                {
                    m_mine = mine;
                }
                else if (maybeEntity.Value is BattleShip ship)
                {
                    m_ship = ship;
                }
            }
        }

        public void Move(int v)
        {
            int newIndex = Index + v;
            if (newIndex < 0) {
				Index = OriginalDataBand.Prototype.Channels + newIndex;
			} else if (newIndex >= OriginalDataBand.Prototype.Channels) {
				Index = newIndex - OriginalDataBand.Prototype.Channels;
			} else {
				Index = newIndex;
			}
		}

        public enum AMOperation
        {
            // ALTERNATE
            [AMName("No action")] None = 0,

            // READS — bound to a WorldMapMine
            [AMName("Read quantity")] ReadQuantity = 1,
            [AMName("Read capacity")] ReadCapacity = 2,
            [AMName("Read usage (0-100%)")] ReadUsage = 3,
            [AMName("Read product")] ReadProduct = 4,
            [AMName("Read pause")] ReadPause = 5,

            // WRITES — bound to a WorldMapMine
            [AMName("Set pause")] WritePause = 24,
            [AMName("Set production")] WriteProduction = 25,

            // READS — main ship (no mine binding required)
            [AMName("Ship: crew current")] ReadShipCrew = 40,
            [AMName("Ship: crew required")] ReadShipCrewRequired = 41,
            [AMName("Ship: HP current")] ReadShipHp = 42,
            [AMName("Ship: HP max")] ReadShipMaxHp = 43,
            [AMName("Ship: HP percent (0-100%)")] ReadShipHpPercent = 44,
            [AMName("Ship: refugees count")] ReadShipRefugees = 45,
            [AMName("Ship: fuel-remaining distance")] ReadShipFuelDistance = 46,
            [AMName("Ship: at home (0/1)")] ReadShipIsAtHome = 47,
        }

        public void Update()
        {
            // Ship-targeted operations require the channel to be bound to the BattleShip
            // (selected through the source picker), so they share the same binding model
            // as mine ops — no global-resolver fallback.
            if (m_operation >= AMOperation.ReadShipCrew && m_operation <= AMOperation.ReadShipIsAtHome)
            {
                BattleShip ship = m_ship;
                if (ship == null || ship.IsDestroyed) {
					return;
				}

				switch (m_operation)
                {
                    case AMOperation.ReadShipCrew:
                        OriginalDataBand.Update(Index, ship.CurrentCrew.ToFix32());
                        break;
                    case AMOperation.ReadShipCrewRequired:
                        OriginalDataBand.Update(Index, ship.CrewRequired.ToFix32());
                        break;
                    case AMOperation.ReadShipHp:
                        OriginalDataBand.Update(Index, ship.CurrentHp.ToFix32());
                        break;
                    case AMOperation.ReadShipMaxHp:
                        OriginalDataBand.Update(Index, ship.MaxHp.ToFix32());
                        break;
                    case AMOperation.ReadShipHpPercent:
                        OriginalDataBand.Update(Index, (100 * ship.CurrentHp).ToFix32() / ship.MaxHp.Max(1).ToFix32());
                        break;
                    case AMOperation.ReadShipRefugees:
                        OriginalDataBand.Update(Index, ship.RefugeesCount.ToFix32());
                        break;
                    case AMOperation.ReadShipFuelDistance:
                        OriginalDataBand.Update(Index, ship.GetFuelRemainingDistance().ToFix32());
                        break;
                    case AMOperation.ReadShipIsAtHome:
                        OriginalDataBand.Update(Index, Fix32.FromRaw(ship.IsAtHomeCell ? 1 : 0));
                        break;
                }
                return;
            }

            if (!(WorldMapMine is null))
            {
                if (random == null) {
					random = GlobalDependencyResolver.Get<RandomProvider>().GetSimRandomFor(this);
				}

				if (random.NextPercent() < ErrorPossibility(WorldMapMine))
                {
                    // action is not done, the transmit failed
                    return;
                }

                switch (m_operation)
                {
                    // READS
                    case AMOperation.ReadQuantity:
                        OriginalDataBand.Update(Index, WorldMapMine.Buffer.Quantity.Value.ToFix32());
                        break;
                    case AMOperation.ReadCapacity:
                        OriginalDataBand.Update(Index, WorldMapMine.Buffer.Capacity.Value.ToFix32());
                        break;
                    case AMOperation.ReadUsage:
                        OriginalDataBand.Update(Index, 100 * WorldMapMine.Buffer.Quantity.Value.ToFix32() / WorldMapMine.Buffer.Capacity.Value.ToFix32());
                        break;
                    case AMOperation.ReadProduct:
                        OriginalDataBand.Update(Index, Fix32.FromRaw(WorldMapMine.Buffer.Product.SlimId.Value));
                        break;
                    case AMOperation.ReadPause:
                        OriginalDataBand.Update(Index, Fix32.FromRaw(WorldMapMine.IsPaused ? 1 : 0));
                        break;

                    // WRITES
                    case AMOperation.WritePause:
                        WorldMapMine.SetPaused(OriginalDataBand.Read(Index, Fix32.Zero) > Fix32.Zero);
                        break;
                    case AMOperation.WriteProduction:
                        WorldMapMine.SetProductionStep(OriginalDataBand.Read(Index, Fix32.Zero).IntegerPart);
                        break;

                    // ALTERNATE
                    default:
                        m_operation = AMOperation.None; // reset unknown value
                        break;
                }
            }
        }

        public Percent ErrorPossibility(WorldMapMine mine)
        {
            var entityDistance = OriginalDataBand.Antena.Prototype.DistanceBoost * OriginalDataBand.Prototype.Distance;
            var measuredDistance = Distance(mine);
            return (0.15.ToFix32() * (measuredDistance / entityDistance)).ToPercent();
        }

        public Fix32 Distance(WorldMapMine mine)
        {
            return mine?.Location.Position.DistanceTo(HomeLocation) ?? Fix32.MaxValue;
        }

        public UiComponent CreateUI(Ui.AntenaInspector antenaInspector, IDataBandChannel channel)
        {
            throw new NotImplementedException("Display of channel is finalized");
        }
    }

    public class AMNameAttribute : Attribute
    {
        public AMNameAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}