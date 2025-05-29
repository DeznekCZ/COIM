using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.SpaceProgram;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using ProgramableNetwork.Data.DisplayEntity;
using ProgramableNetwork.Data.DisplayEntity.Displays;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Mafi.Base.Assets.Core;
using static Mafi.Core.Prototypes.Proto;
using static Mafi.Unity.Assets.Unity;
using TextAlignment = Mafi.Unity.UiToolkit.Component.TextAlignment;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Controllers
        {
            public static readonly StaticEntityProto.ID Light = new StaticEntityProto.ID("ProgramableNetwork_Display_Light");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Light = "Assets/ProgramableNetwork/Display/Light.Icon.png";
            }

            public static readonly string Light = "Assets/ProgramableNetwork/Display/Light.prefab";
        }
    }

    public class Displays : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            ToolbarCategoryProto networkCategoryProto = registrator.PrototypesDb.Get<ToolbarCategoryProto>(NewIds.Controllers.Category).ValueOrThrow("Missing game category");

            var pillars = new EntityLayoutParams(
                customPlacementRange: new ThicknessIRange(0, TransportPillarProto.MAX_PILLAR_HEIGHT.Value - 1),
                customTokens: new CustomLayoutToken[]
                {
                    new CustomLayoutToken("|0|", (param, height) =>
                    {
                        return new LayoutTokenSpec(
                            constraint: LayoutTileConstraint.UsingPillar,
                            heightFrom: 0,
                            heightToExcl: height,
                            maxTerrainHeight: 0,
                            minTerrainHeight: 0
                        );
                    })
                }
            );

            registrator.PrototypesDb.Add(new DisplayEntityProto(
                    id: NewIds.Controllers.Light,
                    strings: CreateStr(NewIds.Controllers.Light, "Light indicator", "Basic colorable light"),
                    layout: new EntityLayoutParser(registrator.PrototypesDb)
                        .ParseLayoutOrThrow(pillars, "|1|"),
                    costs: ((EntityCostsTpl) EntityCostsTpl.Build.Electronics(2).MaintenanceT1(0.01f)).MapToEntityCosts(registrator),
                    graphics: new LayoutEntityProto.Gfx(
                        prefabPath: NewAssets.Computers.Light,
                        customIconPath: NewAssets.Computers.Icons.Light,
                        categories: networkCategoryProto.SomeOption().ToImmutableArray()
                    ),
                    manager: (disp) => new BasicLightManager(disp)
                ));
        }
    }
}
