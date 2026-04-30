using Mafi;
using Mafi.Base;
using Mafi.Base.Prototypes.Machines.ComputingEntities;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.Datacenters;
using Mafi.Core.Factory.Transports;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Core.Roads;
using ProgramableNetwork.Data.Speaker;
using System;
using System.Threading;

namespace ProgramableNetwork
{
    public partial class NewIds
    {
        public partial class Controllers
        {
            public static readonly Proto.ID Category = new Proto.ID("ProgramableNetwork_Category");
            public static readonly StaticEntityProto.ID Controller = new StaticEntityProto.ID("ProgramableNetwork_Computer");
            public static readonly StaticEntityProto.ID Antena = new StaticEntityProto.ID("ProgramableNetwork_Antena");
            public static readonly StaticEntityProto.ID AntenaT2 = new StaticEntityProto.ID("ProgramableNetwork_AntenaT2");
            public static readonly StaticEntityProto.ID Database = new StaticEntityProto.ID("ProgramableNetwork_Database");
            public static readonly StaticEntityProto.ID Speaker = new StaticEntityProto.ID("ProgramableNetwork_Speaker");
            public static StaticEntityProto.ID ControllerTemplate(string id) => new StaticEntityProto.ID($"ProgramableNetwork_Computer_{id}");
        }
    }

    public partial class NewAssets
    {
        public partial class Computers
        {
            public partial class Icons
            {
                public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Icon.png";
                public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Icon.png";
                public static readonly string Speaker = "Assets/ProgramableNetwork/Speaker/Icon.png";
                public static string ControllerTemplate(string id) => $"Assets/ProgramableNetwork/Computer/Icon_{id}.png";
            }

            public static readonly string Controller = "Assets/ProgramableNetwork/Computer/Computer.prefab";
            public static readonly string Antena = "Assets/ProgramableNetwork/Antena/Antena.prefab";
            public static readonly string AntenaT2 = "Assets/ProgramableNetwork/Antena/AntenaT2.prefab";
            public static readonly string Speaker = "Assets/ProgramableNetwork/Speaker/Speaker.prefab";
        }
    }

    internal class Entities : AValidatedData
    {
        private readonly ModJsonConfig m_config;

        public Entities(ModJsonConfig config)
        {
            m_config = config;
        }

        protected override void RegisterDataInternal(ProtoRegistrator registrator) {

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

            var antenaT1 = registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.Antena,
                strings: Proto.CreateStr(NewIds.Controllers.Antena, "Antena", "Handles signal transfer for longer distance"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[4]"),
                tier: 1,
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Antena,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                )
            ));

            var antenaT2 = registrator.PrototypesDb.Add(new AntenaProto(
                id: NewIds.Controllers.AntenaT2,
                strings: Proto.CreateStr(NewIds.Controllers.AntenaT2, "Antena II", "Handles signal transfer for longer distance (100% bonus to range)"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[9][9]", "[9][9]"),
                tier: 2,
                costs: ((EntityCostsTpl)Costs.Build.CP3(8).MaintenanceT2(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.AntenaT2,
                    customIconPath: NewAssets.Computers.Icons.Antena,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                ),
                distanceBoost: ((float)m_config.GetDouble("antena_t2_boost", 2.0)).ToFix32()
            ));

            antenaT1.SetNextTierIndirect(antenaT2);

            registrator.PrototypesDb.Add(new SpeakerProto(
                id: NewIds.Controllers.Speaker,
                strings: Proto.CreateStr(NewIds.Controllers.Speaker, "Speaker", "Play a sound when on, the sound may be triggered Controller, or may work manually"),
                layout: registrator.LayoutParser.ParseLayoutOrThrow("[3]"),
                costs: ((EntityCostsTpl)Costs.Build.CP2(4).MaintenanceT1(2)).MapToEntityCosts(registrator),
                graphics: new LayoutEntityProto.Gfx(
                    prefabPath: NewAssets.Computers.Speaker,
                    customIconPath: NewAssets.Computers.Icons.Speaker,
                    categories: registrator.GetCategoriesProtos(NewIds.Controllers.Category)
                )
            ));
        }
    }
}