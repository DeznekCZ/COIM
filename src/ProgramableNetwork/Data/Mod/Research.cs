using Mafi;
using Mafi.Base;
using Mafi.Collections;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;
using Mafi.Core.Research;
using Mafi.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ResNodeID = Mafi.Core.Research.ResearchNodeProto.ID;

namespace ProgramableNetwork
{
	public partial class NewIds
	{
		public partial class Research
		{
			public static readonly ResNodeID ProgramableNetwork_Stage1 = Ids.Research.CreateId("ProgramableNetwork_Stage1");
			public static readonly ResNodeID ProgramableNetwork_AntenaStage2 = Ids.Research.CreateId("ProgramableNetwork_AntenaStage2");
		}
	}

	internal class Research : AValidatedData, IResearchNodesData
	{
		private static readonly Dictionary<ResNodeID, List<ModuleProto.ID>> m_modules = new Dictionary<ResNodeID, List<ModuleProto.ID>>();

		protected override void RegisterDataInternal(ProtoRegistrator registrator)
		{
			ResearchNodeProto nodeProto = registrator.ResearchNodeProtoBuilder
				.Start("Programable Network", NewIds.Research.ProgramableNetwork_Stage1, 4)
				.Description("Unlocks controlled input by condition")
				.AddLayoutEntityToUnlock(NewIds.Controllers.Controller)
				.AddLayoutEntityToUnlock(NewIds.Controllers.Antena)
				.AddLayoutEntityToUnlock(NewIds.Controllers.Speaker)
				.AddLayoutEntityToUnlock(NewIds.Controllers.Light)
				.AddLayoutEntityToUnlock(NewIds.Controllers.Display7)
				.AddLayoutEntityToUnlock(NewIds.Controllers.Display16)
				.AddProtosToUnlockNoIcon(registrator.PrototypesDb.All<ControllerProto>()
																 .Where(c => c.BasedOn?.Id == NewIds.Controllers.Controller)
																 .Select(c => (Proto.ID)(c.Id)))
				.AddProtosToUnlock<ModuleProto>(m_modules[NewIds.Research.ProgramableNetwork_Stage1].WithGenericId())
				.BuildAndAdd();

			ResearchNodeProto antenaT2Proto = registrator.ResearchNodeProtoBuilder
				.Start("Long range antena", NewIds.Research.ProgramableNetwork_AntenaStage2, 8)
				.Description("Unlocks higher antena for better signal spreading (100% bonus of distance)")
				.AddLayoutEntityToUnlock(NewIds.Controllers.AntenaT2)
				.BuildAndAdd();
			
			nodeProto.GridPositionWherePossible(registrator.PrototypesDb, new Vector2i(40, 2));
			nodeProto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.ResearchLab2));

			antenaT2Proto.GridPositionWherePossible(registrator.PrototypesDb, new Vector2i(60, -2));
			antenaT2Proto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(NewIds.Research.ProgramableNetwork_Stage1));
			antenaT2Proto.AddParent(registrator.PrototypesDb.GetOrThrow<ResearchNodeProto>(Ids.Research.Cp3Packing));

			List<ModuleProto> modules = registrator.PrototypesDb.All<ModuleProto>().ToList();
			Log.Info($"Total loaded modules ({modules.Count}):\n    {string.Join("\n    ", modules.Select(m => m.Id.Value))}");
		}

		internal static void AddModule(ModuleProto.ID id, ResNodeID stage)
		{
			if (!m_modules.TryGetValue(stage, out var moduleProtos))
			{
				m_modules[stage] = moduleProtos = new List<ModuleProto.ID>();
			}

			moduleProtos.Add(id);
		}
	}

	public static class ReseachExtensions
	{
		public static void GridPositionWherePossible(this ResearchNodeProto proto, ProtosDb protos, params Vector2i[] options) {
			Vector2i defaultOption = options[0];
			Dict<int, Lyst<int>> usedOptions = protos.All<ResearchNodeProto>()
				.SelectMany(p => yieldFrom(p.GridPosition)
					.GroupBy(v => v.X, v => v.Y))
				.GroupBy(g => g.Key, g => g)
				.ToDict(p => p.Key, p => p.SelectMany(sg => sg).ToLyst());
			foreach (Vector2i option in options) {
				if (usedOptions.TryGetValue(option.X, out Lyst<int> yS)
					&& (yS.Contains(option.Y)
						|| yS.Contains(option.Y + 1)
						|| yS.Contains(option.Y + 2))) {
					continue;
				}
				proto.GridPosition = option;
				break;
			}
			for (int yP = defaultOption.Y, yN = defaultOption.Y; ; yP++, yN--) {
				bool positiveUsed = usedOptions.TryGetValue(defaultOption.X, out Lyst<int> yS)
					&& yS.Contains(yP)
					&& yS.Contains(yP + 1)
					&& yS.Contains(yP + 2);
				bool negativeUsed = usedOptions.TryGetValue(defaultOption.X, out yS)
					&& yS.Contains(yN)
					&& yS.Contains(yN + 1)
					&& yS.Contains(yN + 2);
				if (positiveUsed && negativeUsed) {
					continue;
				}
				if (positiveUsed) {
					proto.GridPosition = new Vector2i(defaultOption.X, yN);
					break;
				}
				if (negativeUsed) {
					proto.GridPosition = new Vector2i(defaultOption.X, yP);
					break;
				}
			}
		}

		private static IEnumerable<Vector2i> yieldFrom(Vector2i gridPosition) {
			for (int x = gridPosition.X; x < gridPosition.X + 4; x++) {
				for (int y = gridPosition.Y; y < gridPosition.Y + 3; y++) {
					yield return new Vector2i(x, y);
				}
			}
		}

		public static ResearchNodeProtoBuilder.State AddProtosToUnlockNoIcon(this ResearchNodeProtoBuilder.State builder, IEnumerable<Proto.ID> protos) {
			foreach (var item in protos)
			{
				builder.AddProtoUnlockNoIcon(item);
			}
			return builder;
		}
		public static ResearchNodeProtoBuilder.State AddProtosToUnlock<T>(this ResearchNodeProtoBuilder.State builder, IEnumerable<Proto.ID> protos)
			where T : Proto, IProtoWithIcon
		{
			foreach (var item in protos)
			{
				builder.AddProtoToUnlock<T>(item);
			}
			return builder;
		}
		public static IEnumerable<Proto.ID> WithGenericId<T>(this IEnumerable<T> protos)
		{
			foreach (var item in protos)
			{
				yield return new Proto.ID((string)item.GetType().GetField("Value").GetValue(item));
			}
		}
	}
}