using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using ProgramableNetwork.Ui;

namespace ProgramableNetwork
{
	/// <summary>
	/// Single processor that handles every controller/module field-mutation command produced by the
	/// inspector UI. The mutations themselves still go through the existing Module.FieldData API so
	/// the storage layout is identical to the pre-command direct-write code; the only thing that
	/// changes is that each user action is now a serialized command, which keeps multiplayer hosts
	/// and clients in lock-step and lets the same actions appear in replays.
	/// </summary>
	[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
	internal class ControllerCommandExecutor :
		ICommandProcessor<ModuleSetFix32FieldCmd>,
		ICommandProcessor<ModuleSetStringFieldCmd>,
		ICommandProcessor<ModuleSetEntityFieldCmd>,
		ICommandProcessor<ModuleClearFieldCmd>,
		ICommandProcessor<ModuleSetInputConnectionCmd>,
		ICommandProcessor<ControllerSetColorCmd>
	{
		private readonly IEntitiesManager m_entitiesManager;

		public ControllerCommandExecutor(IEntitiesManager entitiesManager)
		{
			m_entitiesManager = entitiesManager;
		}

		public void Invoke(ModuleSetFix32FieldCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			module.Field[cmd.FieldId] = cmd.Value;
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleSetStringFieldCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			module.Field[cmd.FieldId, false] = cmd.Value;
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleSetEntityFieldCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}

			// The dual-write (FieldNumberData + StringData JSON) lives in FieldData.Entity, so route
			// through it. Null target == clear; non-null target must currently exist.
			if (cmd.TargetEntityId.HasValue) {
				if (!m_entitiesManager.TryGetEntity(cmd.TargetEntityId.Value, out IEntity target)) {
					cmd.SetResultError($"Target entity {cmd.TargetEntityId.Value} not found.");
					return;
				}
				module.Field.Entity(cmd.FieldId, target);
			} else {
				module.Field.Entity<IEntity>(cmd.FieldId, null);
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleClearFieldCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			module.FieldNumberData.TryRemove(cmd.FieldId, out _);
			module.StringData.TryRemove("field__" + cmd.FieldId, out _);
			cmd.SetResultSuccess();
		}

		public void Invoke(ControllerSetColorCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			controller.SetColor(cmd.Color);
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleSetInputConnectionCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			if (cmd.IsDisconnect) {
				module.InputModules.TryRemove(cmd.InputId, out _);
			} else {
				module.InputModules[cmd.InputId] = new ModuleConnector(cmd.SourceModuleId, cmd.SourceOutputId);
			}
			cmd.SetResultSuccess();
		}

		private bool tryGetModule(EntityId controllerId, long moduleId,
			out Controller controller, out Module module, out string error)
		{
			module = null;
			if (!m_entitiesManager.TryGetEntity(controllerId, out controller)) {
				error = $"Controller {controllerId} not found.";
				return false;
			}
			module = controller.Modules.AsEnumerable().FirstOrDefault(m => m.Id == moduleId);
			if (module == null) {
				error = $"Module {moduleId} not found on controller {controllerId}.";
				return false;
			}
			error = "";
			return true;
		}
	}
}
