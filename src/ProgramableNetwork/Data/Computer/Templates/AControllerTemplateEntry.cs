using Mafi;
using Mafi.Localization;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// One row in the controller-template picker (analogous to a recipe choice on a
	/// machine).  Concrete subclasses wrap the two upstream sources of templates —
	/// Python-defined ones registered via <see cref="ProgramableNetwork.Python.TemplateRegistrator"/>
	/// and player-saved blueprints under the <c>[PN-Controller]-</c> prefix — so the
	/// picker UI can iterate them uniformly.
	///
	/// <see cref="Apply"/> is what fires when the user commits a choice — concrete
	/// subclasses know how to read their backing data (Python lambda or
	/// <see cref="EntityConfigData"/>) and pour the result into the live controller.
	/// </summary>
	public abstract class AControllerTemplateEntry
	{
		// Stable id used by the picker for de-duplication and search.
		public abstract string Id { get; }

		// Human-readable name + short description for the picker row.
		public abstract LocStrFormatted Name { get; }
		public abstract LocStrFormatted Description { get; }

		// Tint applied to the placed controller after the template is committed,
		// mirrors the existing Python-template behaviour.  Use the controller's
		// current color when the entry has nothing meaningful to set (caller can
		// pre-fill with controller.Color).
		public abstract ColorRgba Color { get; }

		// Free-form lowercased tokens for the picker's search filter.
		public abstract string SearchString { get; }

		/// <summary>
		/// Pours the entry's contents into the given controller in place.
		/// Implementations should clear <see cref="Controller.Modules"/> first and
		/// rewire color / speed as appropriate, then add modules with fresh ids and
		/// run their proto's ExecuteInit so the inspector renders correctly.
		///
		/// All cable connections internal to the template ARE preserved (the saved
		/// layout is the whole point); only references that point outside the
		/// template need to be sanitised, which never applies for these entries.
		/// </summary>
		public abstract void Apply(Controller controller);
	}
}
