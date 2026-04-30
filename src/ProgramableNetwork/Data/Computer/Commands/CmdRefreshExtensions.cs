using System;
using Mafi.Core.Input;
using Mafi.Core.Syncers;
using Mafi.Unity.UiToolkit.Component;

namespace ProgramableNetwork
{
	/// <summary>
	/// Schedules an <see cref="InputCommand"/> through the input scheduler, then waits for
	/// it to actually be processed before invoking <paramref name="onApplied"/>.  Mirrors
	/// the <c>cmd.IsProcessedAndSynced</c> pattern used in the base game (see
	/// TrainDepotInspector / TrainDesignerWindow) — without this the UI would redraw on the
	/// dispatch tick using state that hasn't yet absorbed the command.
	/// </summary>
	public static class CmdRefreshExtensions
	{
		/// <summary>
		/// Schedule <paramref name="cmd"/> and invoke <paramref name="onApplied"/> the first
		/// time its IsProcessedAndSynced flag flips to true.  The observer is bound to
		/// <paramref name="lifetime"/>, so it cleans itself up when that component is detached.
		/// </summary>
		public static T ScheduleAndOnApplied<T>(
			this IInputScheduler scheduler,
			T cmd,
			UiComponent lifetime,
			Action onApplied)
			where T : InputCommand
		{
			scheduler.ScheduleInputCmd(cmd);
			if (onApplied != null && lifetime != null)
			{
				// remove observer when done
				lifetime.Observe(() => cmd.IsProcessedAndSynced).Do(done =>
				{
					if (done) {
						onApplied();
					}
				});
			}
			return cmd;
		}
	}
}
