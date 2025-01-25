using Mafi;
using Mafi.Collections;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Economy;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Population;
using Mafi.Core.Prototypes;
using System.Collections.Generic;

namespace ProgramableNetwork.ModuleTester
{
    internal class FakeEntity : StaticEntity,
        IElectricityConsumingEntity,
        IUnityConsumingEntity,
        IEntityWithWorkers
    {
        public FakeEntity(EntityId id, StaticEntityProto prototype, EntityContext context) : base(id, prototype, context, Tile3i.Zero)
        {
        }

        public override bool CanBePaused => true;

        public Electricity PowerRequired => throw new System.NotImplementedException();

        public Option<IElectricityConsumerReadonly> ElectricityConsumer => Option<IElectricityConsumerReadonly>.Create(new FakeElecticityConsumer(this));

        public int GeneralPriority => throw new System.NotImplementedException();

        public bool IsGeneralPriorityVisible => throw new System.NotImplementedException();

        public bool IsCargoAffectedByGeneralPriority => throw new System.NotImplementedException();

        public Upoints MonthlyUnityConsumed => throw new System.NotImplementedException();

        public Upoints MaxMonthlyUnityConsumed => throw new System.NotImplementedException();

        public Proto.ID UpointsCategoryId => throw new System.NotImplementedException();

        public Option<UnityConsumer> UnityConsumer => throw new System.NotImplementedException();

        public int WorkersNeeded => 5;

        public bool HasWorkersCached { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public override AssetValue Value => throw new System.NotImplementedException();

        public override AssetValue ConstructionCost => throw new System.NotImplementedException();

        public override ImmutableArray<OccupiedTileRelative> OccupiedTiles => throw new System.NotImplementedException();

        public override ImmutableArray<OccupiedVertexRelative> OccupiedVertices => throw new System.NotImplementedException();

        public override LayoutTileConstraint OccupiedVerticesCombinedConstraint => throw new System.NotImplementedException();

        public override ImmutableArray<KeyValuePair<Tile2i, HeightTilesF>> VehicleSurfaceHeights => throw new System.NotImplementedException();

        public override StaticEntityPfTargetTiles PfTargetTiles => throw new System.NotImplementedException();

        public override void NotifyUnevenTerrain(IReadOnlySet<int> groundVerticesViolatingConstraints, int newIndex, bool wasAdded, out bool canCollapse)
        {
            throw new System.NotImplementedException();
        }

        public override bool TryCollapseOnUnevenTerrain(IReadOnlySet<int> groundVerticesViolatingConstraints, EntityCollapseHelper collapseHelper)
        {
            throw new System.NotImplementedException();
        }
    }
}