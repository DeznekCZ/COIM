using Mafi;
using Mafi.Base;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Mods;
using Mafi.Core.Ports;
using Mafi.Core.Ports.Io;
using Mafi.Core.Prototypes;
using Mafi.Unity;
using Mafi.Unity.InputControl.Inspectors;
using Mafi.Unity.UiFramework.Components;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Mafi.Unity.UiFramework;
using Mafi.Serialization;
using Mafi.Core.Buildings.Storages;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Products;

namespace ControlledTransport
{
    public partial class NewIds
    {
        public partial class ControlledTransports
        {
            public static readonly StaticEntityProto.ID PressureValve = new StaticEntityProto.ID("TransportControl_PressureValve");
            public static readonly StaticEntityProto.ID WeightOpener = new StaticEntityProto.ID("TransportControl_WeightOpener");
            public static readonly StaticEntityProto.ID Counter = new StaticEntityProto.ID("TransportControl_Counter");
        }
    }

    public partial class NewAssets
    {
        public partial class ControlledTransports
        {
            public partial class Icons
            {
                public static readonly string PressureValve = Mafi.Base.Assets.Base.Products.Icons.Water_svg;
                public static readonly string WeightOpener = Mafi.Base.Assets.Base.Products.Icons.Gravel_svg;
                public static readonly string Counter = Mafi.Base.Assets.Base.Products.Icons.Iron_svg;
            }

            public static readonly string PressureValve = "Assets/ControlledTransports/PressureValve.prefab";
            public static readonly string WeightOpener = "Assets/ControlledTransports/PressureValve.prefab";
            public static readonly string Counter = "Assets/ControlledTransports/PressureValve.prefab";
        }
    }

    internal class Controls : AValidatedData
    {
        private static readonly ProductType FLUID = new ProductType(typeof(FluidProductProto));
        private static readonly ProductType LOOSE = new ProductType(typeof(LooseProductProto));
        private static readonly ProductType COUNTABLE = new ProductType(typeof(CountableProductProto));

        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            registrator.PrototypesDb.Add(new TransportControlProto(
                id: NewIds.ControlledTransports.PressureValve,
                strings: Proto.CreateStr(NewIds.ControlledTransports.PressureValve, "Pressure value", "Valve pass trough liquid, when pipe is in specified range"),
                filter: FLUID,
                layout: registrator.LayoutParser.ParseLayoutOrThrow(">@A[1]>@Z"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(1)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.ControlledTransports.PressureValve,
                    customIconPath: NewAssets.ControlledTransports.Icons.PressureValve,
                    categories: registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Transports).ToImmutableArray()
                ),
                cannotBeReflected: true
            ));

            registrator.PrototypesDb.Add(new TransportControlProto(
                id: NewIds.ControlledTransports.WeightOpener,
                strings: Proto.CreateStr(NewIds.ControlledTransports.WeightOpener, "Weight opener", "This gate is open when belt or storage is connected and full"),
                filter: LOOSE,
                layout: registrator.LayoutParser.ParseLayoutOrThrow(">~A[1]>~Z"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(1).Product(1, Ids.Products.Rubber)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.ControlledTransports.WeightOpener,
                    customIconPath: NewAssets.ControlledTransports.Icons.WeightOpener,
                    categories: registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Transports).ToImmutableArray()
                ),
                cannotBeReflected: true
            ));

            registrator.PrototypesDb.Add(new TransportControlProto(
                id: NewIds.ControlledTransports.Counter,
                strings: Proto.CreateStr(NewIds.ControlledTransports.Counter, "Counter", "Mechanically counting device that allows certain number of items to flow"),
                filter: COUNTABLE,
                layout: registrator.LayoutParser.ParseLayoutOrThrow(">#A[1]>#Z"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(1).Product(1, Ids.Products.Rubber)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.ControlledTransports.Counter,
                    customIconPath: NewAssets.ControlledTransports.Icons.Counter,
                    categories: registrator.PrototypesDb.Get<ToolbarCategoryProto>(Ids.ToolbarCategories.Transports).ToImmutableArray()
                ),
                cannotBeReflected: true
            ));
        }
    }
}
