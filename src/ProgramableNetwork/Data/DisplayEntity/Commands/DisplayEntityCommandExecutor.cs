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
		ICommandProcessor<DisplayEntitySetPropertyCmd>,
		ICommandProcessor<DisplayEntitySetLightColorCmd>
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

		public void Invoke(DisplayEntitySetLightColorCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out DisplayEntity entity)) {
				cmd.SetResultError($"DisplayEntity {cmd.EntityId} not found.");
				return;
			}
			// On colour is the user's pick; off colour is each channel clamped at 100,
			// matching the previous six-cmd batch. Channels are bytes; widen to int
			// before .ToFix32() since the extension is only on int/float/double.
			int rOn = cmd.Color.R;
			int gOn = cmd.Color.G;
			int bOn = cmd.Color.B;
			entity.SetProperty("colorOn.R", rOn.ToFix32());
			entity.SetProperty("colorOn.G", gOn.ToFix32());
			entity.SetProperty("colorOn.B", bOn.ToFix32());
			entity.SetProperty("colorOff.R", rOn.Min(100).ToFix32());
			entity.SetProperty("colorOff.G", gOn.Min(100).ToFix32());
			entity.SetProperty("colorOff.B", bOn.Min(100).ToFix32());
			cmd.SetResultSuccess();
		}
	}
}
