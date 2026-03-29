using Mafi;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Mods;
using ProgramableNetwork.Ui.DisplayEntity;

namespace ProgramableNetwork.Data.DisplayEntity
{
    public class DisplayEntityProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate, ILayoutEntityProtoWithElevation, IProtoWithTiers, IProtoWithUpgrade
    {
		public static DisplayEntityProto Phantom;
		public static ID PHANTOM_PRODUCT_ID = new ID("__PHANTOM_DISPLAY");

        public override Type EntityType { get; } = typeof(DisplayEntity);
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public Fix32 WorkingDistance { get; set; }
        public Func<DisplayEntity, IDisplayEntityManager> DisplayManagerFactory { get; }
        public bool CanBeElevated { get; }
        public bool CanPillarsPassThrough { get; }
        public ITierData TierData => Upgrade.TierData;
        public UpgradeData Upgrade { get; }

        public DisplayEntityProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, Func<DisplayEntity, IDisplayEntityManager> manager,
            int tierNumber,
            bool canBeElevated = true,
            bool canPillarsPassTrough = true,
            Fix32? distanceBoost = null,
            Upoints? boostCost = null,
            Electricity? workingPower = null,
            Electricity? iddlePower = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(0.5), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(5);
            this.IddlePower = iddlePower ?? Electricity.FromKw(2);
            this.WorkingDistance = distanceBoost ?? Fix32.One;
            this.DisplayManagerFactory = manager;
            this.CanBeElevated = canBeElevated;
            this.CanPillarsPassThrough = canPillarsPassTrough;
            this.Upgrade = new UpgradeData(this);
            this.Upgrade.TierData.TierNumberForUi = tierNumber;
        }


		public static void RegisterPhantom(Mafi.Core.Mods.ProtoRegistrator registrator)
		{
			Phantom = new DisplayEntityProto(
					id: PHANTOM_PRODUCT_ID,
					strings: Proto.CreateStr(PHANTOM_PRODUCT_ID, "Display", "Phantom object for removement"),
					layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
					costs: ((EntityCostsTpl)Mafi.Base.Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                    manager: (display) => new NoManager(),
					tierNumber: 0,
					graphics: new LayoutEntityProto.Gfx(
							prefabPath: NewAssets.Computers.Controller,
							customIconPath: NewAssets.Computers.Icons.Controller,
							categories: ImmutableArray<ToolbarEntryData>.Empty
						)
				);
			registrator.PrototypesDb.RegisterPhantom(Phantom);
		}
	}

	public class NoManager : IDisplayEntityManager {
		public DisplayEntity Entity { get; }
		public DisplayEntityProto Proto { get; }
		public DisplayEntityMb Mb { get; }
		public IDisplayEntityInspector Inspector { get; }
		public void Init(DisplayEntityMb mb) {
			
		}
		public void RenderUpdate(GameTime time) {
			
		}
		public void SyncUpdate(GameTime time) {
			
		}
	}
}
