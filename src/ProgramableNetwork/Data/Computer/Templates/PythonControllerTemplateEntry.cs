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

		public override void Apply(Controller controller)
		{
			if (controller == null || m_template.modules == null) {
				return;
			}
			// Wipe any existing modules first.  ControllerTemplate.modules is the
			// same lambda registered when the original Python templates ran on a
			// freshly-constructed controller, so the controller must look fresh
			// for it to behave the same way.  We also drop any cable connections
			// (the lambda will re-establish whatever wiring it needs).
			controller.Modules.Clear();

			ControllerTemplate.Settings settings = m_template.modules(controller);
			foreach (Module module in controller.Modules)
			{
				module.Prototype.ExecuteInit(module);
			}
			settings?.Invoke();

			controller.SetColor(m_template.color);

			// Auto-populate the description from the template — the player can edit
			// it later in the inspector.  GetFullDescription will append the live
			// module list each time it renders.
			if (!string.IsNullOrEmpty(m_template.description))
			{
				controller.CustomDescription = m_template.description.SomeOption();
			}
		}
	}
}
