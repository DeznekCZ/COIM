using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{

    public class ControllerProto : LayoutEntityProto, ILayoutEntityProto, IProtoWithPropertiesUpdate, IProtoWithTiers, ILayoutEntityProtoWithElevation
    {
        public override Type EntityType { get; } = typeof(Controller);
        public int UsableTime { get; }
        public Electricity WorkingPower { get; }
        public Electricity IddlePower { get; }
        public int Rows { get; }
        public int Columns { get; }
        public Func<ModuleProto, bool> AllowedModule { get; }
        public Func<Controller, Action> InitModules { get; }
        public ITierData TierData { get; }
        public ControllerProto BasedOn { get; }
        public bool CanBeElevated { get; }
        public bool CanPillarsPassThrough { get; }

        public ControllerProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics,
            int rows = 4,
            int columns = 16,
            bool canPillarsPassTrough = true,
            int tierNumber = 1,
            Upoints? boostCost = null,
            Electricity? workingPower = default,
            Electricity? iddlePower = default,
            Func<ModuleProto, bool> allowedModules = null,
            IEnumerable<Tag> tags = null)
            : base(id, strings, layout, costs, graphics, constructionDurationPerProduct: Duration.FromSec(5), boostCost ?? 0.25.Upoints(), cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = workingPower ?? Electricity.FromKw(1);
            this.IddlePower = iddlePower ?? Electricity.FromKw(1);
            this.Rows =rows;
            this.Columns = columns;
            this.AllowedModule = allowedModules ?? ((module) => true);
            this.InitModules = (controller) => () => { };
            this.TierData =  new TierData(this, tierNumber);
            this.BasedOn = null;
            this.CanBeElevated = false;
            this.CanPillarsPassThrough = canPillarsPassTrough;
        }

        public ControllerProto(ID id, Str strings, ControllerProto basedOn, string iconPath, Func<Controller, Action> initModules,
            IEnumerable<Tag> tags = null)
            : base(id, strings, basedOn.Layout, basedOn.Costs, basedOn.Graphics.WithNewIcon(iconPath), constructionDurationPerProduct: Duration.FromSec(5), basedOn.BoostCost, cannotBeBuiltByPlayer: false, isUnique: false, cannotBeReflected: false, autoBuildMiniZippers: false, doNotStartConstructionAutomatically: false, tags: tags)
        {
            this.WorkingPower = basedOn.WorkingPower;
            this.IddlePower = basedOn.IddlePower;
            this.Rows = basedOn.Rows;
            this.Columns = basedOn.Columns;
            this.AllowedModule = basedOn.AllowedModule;
            this.InitModules = initModules ?? ((controller) => () => { });
            this.TierData = new TierData(this, -1);
            this.BasedOn = basedOn;
            this.CanBeElevated = false;
            this.CanPillarsPassThrough = BasedOn.CanPillarsPassThrough;
        }

        public static ControllerProto Phantom;
        public static ID PHANTOM_PRODUCT_ID = new ID("__PHANTOM_CONTROLLER");

        public static void RegisterPhantom(Mafi.Core.Mods.ProtoRegistrator registrator)
        {
            if (Proto.AllPhantoms.FirstOrDefault(p => p.Id == PHANTOM_PRODUCT_ID) != null)
                return;

            Proto.RegisterPhantom(Phantom = new ControllerProto(
                id: PHANTOM_PRODUCT_ID,
                strings: Proto.CreateStr(PHANTOM_PRODUCT_ID, "Controller", "Handles basic operations and automatization"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[1]"),
                costs: ((EntityCostsTpl)Mafi.Base.Costs.Build.CP2(4)).MapToEntityCosts(registrator),
                allowedModules: (module) => module.AllowedDevices.Contains(NewIds.Controllers.Controller),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Controller,
                    customIconPath: NewAssets.Computers.Icons.Controller,
                    categories: ImmutableArray<ToolbarCategoryProto>.Empty
                )
            ));
        }
    }
}
