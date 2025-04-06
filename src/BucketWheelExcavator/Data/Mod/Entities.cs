using Mafi;
using Mafi.Base;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using BucketWheelExcavator.Entity;
using Mafi.Core.Entities.Dynamic;
using System.Collections.Generic;
using Mafi.Collections.ImmutableCollections;
using System;
using System.Linq;
using Mafi.Core.Research;

namespace BucketWheelExcavator
{
    public partial class NewIds
    {
        public partial class BucketWheelExcavator
        {
            public static StaticEntityProto.ID BucketExcavator_T1 => new StaticEntityProto.ID($"BucketExcavator_T1");
            public static Proto.ID BucketExcavator_T1_FuelDiesel => new Proto.ID($"BucketExcavator_T1_FuelDiesel");
        }
    }

    internal class Entities : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            var fuelTanks = new List<FuelTankProto>{
                registrator.FuelTankProtoBuilder.Start(NewIds.BucketWheelExcavator.BucketExcavator_T1)
                    .SetProduct(Mafi.Base.Ids.Products.Diesel, 10.Quantity(), Duration.FromMin(0.5))
                    .SetIdleFuelConsumption(10.Percent())
                    .BuildTank()
            }.ToImmutableArray();

            var entityProto = registrator.PrototypesDb.Add(new BucketWheelExcavatorProto(
                id: NewIds.BucketWheelExcavator.BucketExcavator_T1,
                strings: Proto.CreateStr(NewIds.BucketWheelExcavator.BucketExcavator_T1, "Bucket excavator", "Basic bucket excavator"),
                layout: new EntityLayoutParser(registrator.PrototypesDb).ParseLayoutOrThrow(
                    new EntityLayoutParams(customTokens: new CustomLayoutToken[]
                    {
                        new CustomLayoutToken("!0]", (param, height) => {
                            return new LayoutTokenSpec(
                                constraint: LayoutTileConstraint.DisableTerrainPhysics | LayoutTileConstraint.NoRubbleAfterCollapse,
                                heightFrom: 0,
                                heightToExcl: 9,
                                vehicleHeight: height,
                                portHeight: height
                            );
                        })
                    }, customPortHeights: new KeyValuePair<char, int>[]
                    {
                        new KeyValuePair<char,int>('A', 2)
                    }),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "++++++++".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                    "...e....".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v")
                ),
                costs: ((EntityCostsTpl)new EntityCostsTpl.Builder()
                    .CP2(5)
                    .MaintenanceT1(2)
                    .Product(9, Ids.Products.Steel)
                    .Product(5, Ids.Products.VehicleParts3)
                ).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: "Assets/BucketWheelExcavator/BucketWheelExcavator_T1.prefab",
                    customIconPath: "Assets/BucketWheelExcavator/BucketWheelExcavator_T1_Icon.png",
                    categories: registrator.PrototypesDb.Get<ToolbarCategoryProto>(Mafi.Base.Ids.ToolbarCategories.BuildingsForVehicles).ToImmutableArray()
                ),
                fuelTanks: fuelTanks,
                workersNeeded: 20,
                cannotBeReflected: true,
                constructionDurationPerProduct: Duration.FromSec(1)
            ));

            Log.Info("Layouts parsed");
        }

                //    "..............****..............".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...........**********...........".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".........**************.........".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".......******************.......".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "......********************......".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".....**********************.....".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "....************************....".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...**************************...".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...**************************...".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "..****************************..".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "..****************************..".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".******************************.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".***********++++++++***********.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".***********++++++++***********.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "************++++++++************".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "************++++++++************".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "************++++++++************".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "************++++++++************".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".***********++++++++***********.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".***********++++++++***********.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".**************e***************.".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "..****************************..".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "..****************************..".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...**************************...".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...**************************...".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "....************************....".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".....**********************.....".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "......********************......".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".......******************.......".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    ".........**************.........".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "...........**********...........".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v"),
                //    "..............****..............".Replace("*", "!3]").Replace(".", "   ").Replace("+", "[9]").Replace("e", "A~v")
                //),
    }
}
