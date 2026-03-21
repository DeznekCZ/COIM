using Mafi;
using Mafi.Core.Entities.Static;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mafi.Collections.ImmutableCollections;

namespace WindPower.Entity {
	public class WindTurbineProto : LayoutEntityProto, IProtoWithTiers {

		public override Type EntityType => typeof(WindTurbine);
		new public Gfx Graphics => (Gfx)base.Graphics;

		public ITierData TierData { get; }

		public Electricity GeneratedPower { get; }
		public MechPower BrakingPower { get; }
		public HeightTilesF GondolaHeight { get; }
		public HeightTilesF BladeWidth { get; }

		public WindTurbineProto(ID id, Str strings, EntityLayout layout, EntityCosts costs, Gfx graphics, Electricity generatedPower, MechPower brakingPower, HeightTilesF gondolaHeight, HeightTilesF bladeWidth, Duration? constructionDurationPerProduct = null, Upoints? boostCost = null, bool cannotBeBuiltByPlayer = false, bool isUnique = false, bool cannotBeReflected = false, bool autoBuildMiniZippers = false, bool doNotStartConstructionAutomatically = false, IEnumerable<Tag> tags = null)
			: base(id: id, strings: strings, layout: layout, costs: costs, graphics: graphics, constructionDurationPerProduct: constructionDurationPerProduct, boostCost: boostCost, cannotBeBuiltByPlayer: cannotBeBuiltByPlayer, isUnique: isUnique, cannotBeReflected: cannotBeReflected, autoBuildMiniZippers: autoBuildMiniZippers, doNotStartConstructionAutomatically: doNotStartConstructionAutomatically, tags: tags) {
			GeneratedPower = generatedPower;
			BrakingPower = brakingPower;
			GondolaHeight = gondolaHeight;
			BladeWidth = bladeWidth;
			TierData = new TierData(this);
		}

		new public class Gfx : LayoutEntityProto.Gfx {

			public readonly string GondolaGo;
			public readonly string RotorGo;
			public readonly string[] BladeGos;
			public readonly Fix32 SpeedMultiplier;

			public Gfx(
				string prefabPath, // inherited
				string gondolaGo,
				string rotorGo,
				string[] bladeGos,
				Fix32 speedMultiplier,
				// inherited
				RelTile3f prefabOrigin = new RelTile3f(),
				Option<string> customIconPath = new Option<string>(), ColorRgba color = new ColorRgba(),
				bool hideBlockedPortsIcon = false, VisualizedLayers? visualizedLayers = null,
				ImmutableArray<ToolbarEntryData>? categories = null, bool useInstancedRendering = false,
				bool useSemiInstancedRendering = false, string instancedRenderingAnimationProtoSwap = null,
				IReadOnlyDictionary<string, string> instancedRenderingAnimationMaterialSwap = null,
				ImmutableArray<string> instancedRenderingExcludedObjects = new ImmutableArray<string>(),
				string instancedRenderingExcludedObjectsPattern = null, int maxRenderedLod = 2147483647,
				bool disableEmptyChildrenStripping = false, bool removeUndergroundVertices = false,
				AngleDegrees1f? yawForGeneratedIcon = null, bool canBePickedUnderground = false
			) : base(prefabPath, prefabOrigin, customIconPath, color, hideBlockedPortsIcon, visualizedLayers,
				categories, useInstancedRendering, useSemiInstancedRendering, instancedRenderingAnimationProtoSwap,
				instancedRenderingAnimationMaterialSwap, instancedRenderingExcludedObjects,
				instancedRenderingExcludedObjectsPattern, maxRenderedLod, disableEmptyChildrenStripping,
				removeUndergroundVertices, yawForGeneratedIcon, canBePickedUnderground
			) {
				SpeedMultiplier = speedMultiplier;
				GondolaGo = gondolaGo;
				RotorGo = rotorGo;
				BladeGos = bladeGos;
			}
		}
	}
}
