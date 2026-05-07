using Mafi;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using Mafi.Localization;
using System;
using System.Linq;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Wraps a player-saved <c>[PN-Controller]-</c> blueprint into a picker entry.
	/// Apply pulls the modules array out of the blueprint's <see cref="EntityConfigData"/>
	/// and reattaches each module to the live controller with a fresh id, then
	/// transfers color and speed so the result looks identical to the original
	/// snapshot — minus the on-world position, which is intentionally not touched
	/// (the player is configuring an already-placed controller).
	/// </summary>
	public class BlueprintControllerTemplateEntry : AControllerTemplateEntry
	{
		private readonly IBlueprint m_blueprint;

		public BlueprintControllerTemplateEntry(IBlueprint blueprint)
		{
			m_blueprint = blueprint;
		}

		// Strip the [PN-Controller]- prefix from the displayed name; the prefix is
		// only useful for distinguishing entries inside the base library browser.
		private string DisplayName
		{
			get
			{
				string n = m_blueprint.Name ?? "";
				return n.StartsWith(ControllerBlueprints.TitlePrefix)
					? n.Substring(ControllerBlueprints.TitlePrefix.Length)
					: n;
			}
		}

		public override string Id => "bp:" + (m_blueprint?.Name ?? "");
		public override LocStrFormatted Name => new LocStrFormatted(DisplayName);
		public override LocStrFormatted Description => new LocStrFormatted(m_blueprint?.Desc ?? "");
		public override ColorRgba Color => ColorRgba.Empty; // applied from saved data below
		public override string SearchString => string.Join(" ", DisplayName, m_blueprint?.Desc ?? "");

		public override void Apply(Controller controller)
		{
			if (controller == null || m_blueprint == null || m_blueprint.Items.Length == 0) {
				return;
			}
			EntityConfigData data = m_blueprint.Items[0];

			ImmutableArray<Module>? modules = data.GetArray<Module>("controller_modules", Module.Deserialize);
			if (modules == null) {
				return;
			}

			// Replace existing modules wholesale with the saved set.  Two passes:
			// (1) attach + assign fresh ids from the destination controller's pool
			// (Controller.AllocateModuleId), remembering oldId → newId, then
			// (2) rewrite InputModules so cables that pointed within the template
			// are preserved across the id rewrite.  Connections that pointed
			// outside the template (shouldn't happen — Save scrubs externals — but
			// guard anyway) get dropped because their old id isn't in the map.
			controller.Modules.Clear();
			Mafi.Collections.Dict<long, long> oldToNew = new Mafi.Collections.Dict<long, long>();
			foreach (Module raw in modules.Value.AsEnumerable())
			{
				if (raw == null) {
					continue;
				}
				raw.Context = controller.Context;
				raw.Controller = controller;
				try
				{
					raw.initContexts(-1);
				}
				catch (Exception e)
				{
					Log.Exception(e);
					continue;
				}
				if (raw.Prototype == null || raw.Prototype == ModuleProto.Phantom) {
					continue;
				}
				long oldId = raw.Id;
				long newId = controller.AllocateModuleId();
				raw.Id = newId;
				oldToNew[oldId] = newId;

				foreach (IField field in raw.Prototype.Fields)
				{
					field.Validate(raw);
				}
				controller.Modules.Add(raw);
			}

			// Pass 2 — translate every cable's source-module id from the saved
			// (pre-reset) value to the fresh one we just assigned.  Anything that
			// can't be mapped is dropped to keep the controller in a consistent
			// state.
			foreach (Module m in controller.Modules)
			{
				foreach (var kv in m.InputModules.ToList())
				{
					if (oldToNew.TryGetValue(kv.Value.ModuleId, out long mappedId))
					{
						m.InputModules[kv.Key] = new ModuleConnector(mappedId, kv.Value.OutputId);
					}
					else
					{
						m.InputModules.Remove(kv.Key);
					}
				}
			}

			int? color = data.GetInt("color");
			if (color.HasValue) {
				controller.SetColor(new ColorRgba((uint)color.Value));
			}
			int? speed = data.GetInt("controller_speed");
			if (speed.HasValue) {
				controller.DelayBetweenTicks = speed.Value;
			}

			// Description: prefer the one persisted with the blueprint payload (the
			// last value the original author set in their inspector), and fall back to
			// the blueprint's library Desc.  Either way, the auto-appended module list
			// is produced by GetFullDescription at render time, so we only carry the
			// user-supplied prefix here.
			Option<string> savedDescription = data.GetString("controller_description");
			if (savedDescription.HasValue)
			{
				controller.CustomDescription = savedDescription;
			}
			else if (!string.IsNullOrEmpty(m_blueprint.Desc))
			{
				controller.CustomDescription = m_blueprint.Desc.SomeOption();
			}
		}
	}
}
