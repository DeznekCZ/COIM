using Mafi;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Blueprints;
using Mafi.Localization;
using Mafi.Unity;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Generic "name your blueprint" floater used by both the module-save flow
	/// (<see cref="ModuleEditDialog"/>) and the controller-save flow
	/// (<see cref="ControllerInspector"/>).  Captures a user-given name and forwards
	/// it to a caller-supplied save delegate that knows how to build the actual
	/// EntityConfigData and register it with <see cref="BlueprintsLibrary"/>.
	///
	/// The dialog itself is purely UI — library APIs (BlueprintsLibrary /
	/// ConfigSerializationContext) are resolved by the helper the delegate runs.
	/// </summary>
	public class SaveBlueprintDialog : FloatingColumn
	{
		private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();

		/// <param name="title">Header text shown at the top of the dialog.</param>
		/// <param name="defaultName">Initial value for the name TextField.</param>
		/// <param name="prefixHint">The library-title prefix shown to the user — purely
		/// informational, helps them anticipate how the entry will appear in the base
		/// blueprint browser.</param>
		/// <param name="anchor">Component the floater opens off of.</param>
		/// <param name="uiContext">For the invalid-op audio cue on save failure.</param>
		/// <param name="onSave">Performs the actual save with the user-given name.
		/// Returns the new blueprint or None if the save failed.</param>
		public SaveBlueprintDialog(
			string title,
			string defaultName,
			string prefixHint,
			Button anchor,
			UiContext uiContext,
			Func<string, Option<IBlueprint>> onSave)
			: base(POLICY, false, false, true)
		{
			PanelWithHeader panel = AddAndReturn(new PanelWithHeader(new LocStrFormatted(title)));
			panel.Height(Px.Auto);

			TextField nameField = new TextField()
				.Width(280.px())
				.Height(Sizes.BLOCK_SIZE);
			nameField.Value(new LocStrFormatted(defaultName ?? ""));
			nameField.FocusOnShow();
			panel.Body.Add(nameField);

			LocStrFormatted hintText = new LocStrFormatted(
				$"Title in library will be: {prefixHint}<your name>");
			panel.Body.Add(new Label(hintText).TextOverflow(TextOverflow.Wrap));

			Row buttons = panel.Body.AddAndReturn(new Row()).Gap(5.px());

			ButtonText cancel = new ButtonText("Cancel".AsLoc())
				.Width(80.px())
				.Height(Sizes.BLOCK_SIZE)
				.OnClick(Close);
			buttons.Add(cancel);

			ButtonText save = new ButtonText("Save".AsLoc())
				.Width(120.px())
				.Height(Sizes.BLOCK_SIZE);
			save.OnClick(() =>
			{
				string given = nameField.GetText();
				Option<IBlueprint> created = Option<IBlueprint>.None;
				try
				{
					created = onSave(given);
				}
				catch (Exception ex)
				{
					Log.Exception(ex);
				}
				if (created.HasValue)
				{
					Log.Info($"[Blueprint] Saved '{created.Value.Name}' to library");
				}
				else
				{
					Log.Warning("[Blueprint] Save returned no blueprint");
					uiContext?.AudioDb?.InvalidOp(true).Play();
				}
				Close();
			});
			buttons.Add(save);

			this.Width(320.px());
			this.Height(Px.Auto);

			Open(anchor);
		}

		/// <summary>
		/// Backwards-compatible shorthand for the module-save flow used by
		/// <see cref="ModuleEditDialog"/>.
		/// </summary>
		public static SaveBlueprintDialog ForModule(Module module, Button anchor, UiContext uiContext,
			DependencyResolver resolver)
		{
			BlueprintsLibrary library = resolver.GetResolvedInstance<BlueprintsLibrary>().Value;
			ConfigSerializationContext context = resolver.GetResolvedInstance<ConfigSerializationContext>().Value;
			EntitiesCloneConfigHelper entitiesCloneConfigHelper = resolver.GetResolvedInstance<EntitiesCloneConfigHelper>().Value;
			return new SaveBlueprintDialog(
				title: "Save module as blueprint",
				defaultName: module.Prototype.Strings.Name.TranslatedString,
				prefixHint: ModuleBlueprints.TitlePrefix,
				anchor: anchor,
				uiContext: uiContext,
				onSave: name => ModuleBlueprints.Save(library, context, entitiesCloneConfigHelper, module, name));
		}

		/// <summary>
		/// Companion factory for the controller-save flow used by
		/// <see cref="ControllerInspector"/>.
		/// </summary>
		public static SaveBlueprintDialog ForController(Controller controller, Button anchor, UiContext uiContext, DependencyResolver resolver)
		{
			BlueprintsLibrary library = resolver.GetResolvedInstance<BlueprintsLibrary>().Value;
			ConfigSerializationContext context = resolver.GetResolvedInstance<ConfigSerializationContext>().Value;
			EntitiesCloneConfigHelper entitiesCloneConfigHelper = resolver.GetResolvedInstance<EntitiesCloneConfigHelper>().Value;
			string defaultName = controller.CustomTitle.HasValue
				? controller.CustomTitle.Value
				: controller.Prototype.Strings.Name.TranslatedString;
			return new SaveBlueprintDialog(
				title: "Save controller as blueprint",
				defaultName: defaultName,
				prefixHint: ControllerBlueprints.TitlePrefix,
				anchor: anchor,
				uiContext: uiContext,
				onSave: name => {
					return ControllerBlueprints.Save(library, context, controller, entitiesCloneConfigHelper, name);
				});
		}
	}
}
