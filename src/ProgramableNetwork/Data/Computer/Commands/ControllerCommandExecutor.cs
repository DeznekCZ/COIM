using System.Linq;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using Mafi.Core.Prototypes;
using ProgramableNetwork.Data.Variables;
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
		ICommandProcessor<ModuleSetExtensionCountCmd>,
		ICommandProcessor<ControllerSetColorCmd>,
		ICommandProcessor<ModuleRemoveCmd>,
		ICommandProcessor<ModuleMoveToCmd>,
		ICommandProcessor<ModulePlaceCmd>,
		ICommandProcessor<ModuleShiftAddCmd>,
		ICommandProcessor<ModulePasteCmd>,
		ICommandProcessor<ModulePlaceFromBlueprintCmd>,
		ICommandProcessor<ControllerApplyTemplateCmd>,
		ICommandProcessor<VariableRemoveCmd>
	{
		private readonly IEntitiesManager m_entitiesManager;
		private readonly ProtosDb m_protosDb;
		private readonly VariableManager m_variableManager;

		public ControllerCommandExecutor(IEntitiesManager entitiesManager, ProtosDb protosDb, VariableManager variableManager)
		{
			m_entitiesManager = entitiesManager;
			m_protosDb = protosDb;
			m_variableManager = variableManager;
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

		public void Invoke(VariableRemoveCmd cmd)
		{
			// Stale-variable cleanup — UI only exposes the trash button when the
			// writer entity is gone, but the executor stays permissive and lets
			// any peer issue the removal.  Removing a name that no longer exists
			// is a no-op and still reports success.
			if (string.IsNullOrEmpty(cmd.Name)) {
				cmd.SetResultError("Variable name is empty.");
				return;
			}
			m_variableManager.RemoveVariable(cmd.Name);
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
			module.Controller?.InvalidateTopology();
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleSetExtensionCountCmd cmd)
		{
			if (!tryGetModule(cmd.ControllerId, cmd.ModuleId, out _, out Module module, out string error)) {
				cmd.SetResultError(error);
				return;
			}
			// SetExtensionCountLinked clamps to [0, MaxXxxExtensions], prunes any cables
			// bound to a pin that disappeared, AND mirrors the count to the linked
			// side when the prototype opted into input↔output lock-step (e.g. flip-
			// flop).  No separate InputModules reconciliation needed — the side
			// setters already handle it.
			module.SetExtensionCountLinked(cmd.Side, cmd.NewCount);
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleRemoveCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			if (!controller.TryRemoveModule(cmd.ModuleId)) {
				cmd.SetResultError($"Module {cmd.ModuleId} not found on controller {cmd.ControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleMoveToCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			if (!controller.TryMoveModule(cmd.ModuleId, cmd.TargetRow, cmd.TargetColumn)) {
				// Authoritative re-check on the sim thread refused the move (slot
				// occupied by a different module, or the module itself disappeared
				// between click and apply).  Surface this so the UI's OnApplied
				// callback can react — cmd.HasError will be true, cmd.Result false.
				cmd.SetResultError($"Module {cmd.ModuleId} could not be moved to ({cmd.TargetRow}, {cmd.TargetColumn}) on controller {cmd.ControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModulePlaceCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			Option<ModuleProto> proto = m_protosDb.Get<ModuleProto>(cmd.ProtoId);
			if (!proto.HasValue) {
				cmd.SetResultError($"Module proto '{cmd.ProtoId}' not found.");
				return;
			}
			long newId = controller.TryPlaceModule(proto.Value, cmd.TargetRow, cmd.TargetColumn);
			cmd.CreatedModuleId = newId;
			if (newId == 0) {
				cmd.SetResultError($"Could not place module '{cmd.ProtoId}' at ({cmd.TargetRow}, {cmd.TargetColumn}) on controller {cmd.ControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModuleShiftAddCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.DestControllerId, out Controller dest)) {
				cmd.SetResultError($"Controller {cmd.DestControllerId} not found.");
				return;
			}
			if (!m_entitiesManager.TryGetEntity(cmd.SourceControllerId, out Controller src)) {
				cmd.SetResultError($"Source controller {cmd.SourceControllerId} not found.");
				return;
			}
			Module source = src.Modules.AsEnumerable().FirstOrDefault(m => m.Id == cmd.SourceModuleId);
			if (source == null) {
				cmd.SetResultError($"Source module {cmd.SourceModuleId} not found on controller {cmd.SourceControllerId}.");
				return;
			}
			long newId = dest.TryShiftAddModule(source, cmd.TargetRow, cmd.TargetColumn);
			cmd.CreatedModuleId = newId;
			if (newId == 0) {
				cmd.SetResultError($"Could not stamp module at ({cmd.TargetRow}, {cmd.TargetColumn}) on controller {cmd.DestControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModulePasteCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.DestControllerId, out Controller dest)) {
				cmd.SetResultError($"Controller {cmd.DestControllerId} not found.");
				return;
			}
			if (!m_entitiesManager.TryGetEntity(cmd.SourceControllerId, out Controller src)) {
				cmd.SetResultError($"Source controller {cmd.SourceControllerId} not found.");
				return;
			}
			Module source = src.Modules.AsEnumerable().FirstOrDefault(m => m.Id == cmd.SourceModuleId);
			if (source == null) {
				cmd.SetResultError($"Source module {cmd.SourceModuleId} not found on controller {cmd.SourceControllerId}.");
				return;
			}
			if (!dest.TryPasteModule(cmd.DestModuleId, source)) {
				cmd.SetResultError($"Destination module {cmd.DestModuleId} not found on controller {cmd.DestControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ModulePlaceFromBlueprintCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			Option<ModuleProto> proto = m_protosDb.Get<ModuleProto>(cmd.ProtoId);
			if (!proto.HasValue) {
				cmd.SetResultError($"Module proto '{cmd.ProtoId}' not found.");
				return;
			}
			long newId = controller.TryPlaceModuleFromSnapshot(
				proto.Value, cmd.TargetRow, cmd.TargetColumn, cmd.Snapshot);
			cmd.CreatedModuleId = newId;
			if (newId == 0) {
				cmd.SetResultError($"Could not place blueprint module '{cmd.ProtoId}' at ({cmd.TargetRow}, {cmd.TargetColumn}) on controller {cmd.ControllerId}.");
				return;
			}
			cmd.SetResultSuccess();
		}

		public void Invoke(ControllerApplyTemplateCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.ControllerId, out Controller controller)) {
				cmd.SetResultError($"Controller {cmd.ControllerId} not found.");
				return;
			}
			if (!controller.ApplyPythonTemplate(cmd.TemplateId)) {
				cmd.SetResultError($"Template '{cmd.TemplateId}' not registered.");
				return;
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
