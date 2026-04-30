using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using ProgramableNetwork.Data.DisplayEntity;

namespace ProgramableNetwork
{
	/// <summary>
	/// Processes the display-entity mutation commands dispatched from
	/// <see cref="ProgramableNetwork.Ui.DisplayEntity.Displays.ColorizedLightInspector"/> and
	/// segment-display inspectors.  Keeps the prior call shapes (<c>SetActive</c>,
	/// <c>SetProperty</c>) so the on-disk format and renderer behaviour are unchanged.
	/// </summary>
	[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
	internal class DisplayEntityCommandExecutor :
		ICommandProcessor<DisplayEntitySetActiveCmd>,
		ICommandProcessor<DisplayEntitySetPropertyCmd>
	{
		private readonly IEntitiesManager m_entitiesManager;

		public DisplayEntityCommandExecutor(IEntitiesManager entitiesManager)
		{
			m_entitiesManager = entitiesManager;
		}

		public void Invoke(DisplayEntitySetActiveCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out DisplayEntity entity)) {
				cmd.SetResultError($"DisplayEntity {cmd.EntityId} not found.");
				return;
			}
			entity.SetActive(cmd.IsActive);
			cmd.SetResultSuccess();
		}

		public void Invoke(DisplayEntitySetPropertyCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out DisplayEntity entity)) {
				cmd.SetResultError($"DisplayEntity {cmd.EntityId} not found.");
				return;
			}
			entity.SetProperty(cmd.Name, cmd.Value);
			cmd.SetResultSuccess();
		}
	}
}
