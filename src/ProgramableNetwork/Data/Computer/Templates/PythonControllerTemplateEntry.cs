using Mafi;
using Mafi.Localization;
using ProgramableNetwork.Python;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Wraps a Python-defined <see cref="ControllerTemplate"/> (anything from
	/// <see cref="ProgramableNetwork.Data.Mod.ControllerTemplates.GetControllerTemplates"/>
	/// — the built-in FullStorage / VehicleImport plus user-defined Python templates)
	/// into the picker's unified entry model.
	///
	/// Apply runs the same module-creation lambda the existing per-proto controller
	/// templates use, just against an already-placed controller instead of one being
	/// constructed by <see cref="ControllerProto.InitModules"/>.
	/// </summary>
	public class PythonControllerTemplateEntry : AControllerTemplateEntry
	{
		private readonly ControllerTemplate m_template;

		public PythonControllerTemplateEntry(ControllerTemplate template)
		{
			m_template = template;
		}

		public override string Id => "py:" + m_template.id;
		public override LocStrFormatted Name => new LocStrFormatted(m_template.name ?? m_template.id);
		public override LocStrFormatted Description => new LocStrFormatted(m_template.description ?? "");
		public override ColorRgba Color => m_template.color;
		public override string SearchString => string.Join(" ",
			m_template.name ?? "", m_template.description ?? "", m_template.id ?? "");

		// The id used to find this template in <see cref="ProgramableNetwork.Data.Mod.ControllerTemplates.CachedTemplates"/>
		// at command-apply time.  Same value the "py:..." picker id is built from.
		public string TemplateId => m_template.id;

		// In the normal UI flow the picker dispatches <c>ControllerApplyTemplateCmd</c>
		// (see PickControllerTemplate) so the mutation runs on the sim thread and
		// MP/replay stay deterministic.  This direct override is kept for callers
		// that already hold a Controller and want to apply the template synchronously
		// (programmatic uses, tests).  The body delegates to the shared
		// <see cref="Controller.ApplyPythonTemplate"/> helper so there is one
		// authoritative implementation of the apply semantics.
		public override void Apply(Controller controller)
		{
			if (controller == null) {
				return;
			}
			controller.ApplyPythonTemplate(m_template.id);
		}
	}
}
