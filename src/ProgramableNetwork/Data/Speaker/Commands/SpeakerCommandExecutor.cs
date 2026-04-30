using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Input;
using ProgramableNetwork.Data.Speaker;

namespace ProgramableNetwork
{
	/// <summary>
	/// Processor for <see cref="ProgramableNetwork.Data.Speaker.Speaker"/> mutation commands
	/// dispatched from <see cref="ProgramableNetwork.Data.Speaker.SpeakerInspector"/>.
	/// Calls the same setters the inspector previously invoked directly, but routes through
	/// the input scheduler.
	/// </summary>
	[GlobalDependency(RegistrationMode.AsAllInterfaces, false, false)]
	internal class SpeakerCommandExecutor :
		ICommandProcessor<SpeakerSetPlayingCmd>,
		ICommandProcessor<SpeakerSetSoundCmd>,
		ICommandProcessor<SpeakerSetVolumeCmd>
	{
		private readonly IEntitiesManager m_entitiesManager;

		public SpeakerCommandExecutor(IEntitiesManager entitiesManager)
		{
			m_entitiesManager = entitiesManager;
		}

		public void Invoke(SpeakerSetPlayingCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out Speaker speaker)) {
				cmd.SetResultError($"Speaker {cmd.EntityId} not found.");
				return;
			}
			speaker.SetPlaying(cmd.IsPlaying);
			cmd.SetResultSuccess();
		}

		public void Invoke(SpeakerSetSoundCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out Speaker speaker)) {
				cmd.SetResultError($"Speaker {cmd.EntityId} not found.");
				return;
			}
			speaker.SetSound(cmd.SoundPrefab);
			cmd.SetResultSuccess();
		}

		public void Invoke(SpeakerSetVolumeCmd cmd)
		{
			if (!m_entitiesManager.TryGetEntity(cmd.EntityId, out Speaker speaker)) {
				cmd.SetResultError($"Speaker {cmd.EntityId} not found.");
				return;
			}
			speaker.SetVolume(cmd.Volume);
			cmd.SetResultSuccess();
		}
	}
}
