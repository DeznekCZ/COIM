using Mafi;
using Mafi.Core;
using Mafi.Core.Syncers;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using Mafi.Unity.UiToolkit.Library.FloatingPanel;
using System.Collections.Generic;

namespace ProgramableNetwork.Ui
{
	public class ModuleEditDialog : FloatingColumn
	{
		private static readonly DropdownPositionPolicy POLICY = new DropdownPositionPolicy();
		private readonly Module m_module;

		public ModuleEditDialog(Module module, ControllerView controllerView, UiContext uiContext, Button button, ControllerInspector controllerInspector)
			: base(POLICY, false, false, true)
		{
			RowContainer row = new PanelRow();

			m_module = module;

			var panel = new PanelWithHeader(m_module.Prototype.Strings.Name);
			panel.Height(Px.Auto);
			Add(panel);

			var body = panel.Body;
			body.Add(row);
			body.Gap(5.px());

			// Filler pushes the toolbar buttons to the right edge of the row.
			row.AddAndReturn(new UiComponent()).Fill();

			// Toolbar order: Copy → Paste → Save → Delete.
			// Movement is handled by Move mode on the inspector and pin-extension
			// editing by the inline edge buttons on the module itself, so neither
			// has a slot in this dialog anymore.

			ButtonIcon copy = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ExportToString_svg)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.Margin(Px.Zero)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
				.Icon.Padding(Sizes.IMAGE_PADDING)
					 .Margin(Px.Zero)
				.Parent.As<ButtonIcon>().Value;
			copy.Tooltip("Copy to last created for next addition".ToDoLoc());
			copy.OnClick(() => ControllerView.m_lastCreated = m_module);
			row.Add(copy);

			ButtonIcon paste = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ImportFromString_svg)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.Margin(Px.Zero)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
				.Icon.Padding(Sizes.IMAGE_PADDING)
					 .Margin(Px.Zero)
				.Parent.As<ButtonIcon>().Value;
			paste.Tooltip("Copy data from last created or copied module".ToDoLoc());
			paste.OnClick(() =>
			{
				m_module.SetStatus(ModuleStatus.Init);
				m_module.NumberData.Clear();
				m_module.FieldNumberData.Clear();
				m_module.StringData.Clear();
				foreach (KeyValuePair<string, int> item in ControllerView.m_lastCreated.NumberData) {
					m_module.NumberData[item.Key] = item.Value;
				}
				foreach (KeyValuePair<string, Fix32> item in ControllerView.m_lastCreated.FieldNumberData) {
					m_module.FieldNumberData[item.Key] = item.Value;
				}
				foreach (KeyValuePair<string, string> item in ControllerView.m_lastCreated.StringData) {
					m_module.StringData[item.Key] = item.Value;
				}
				// Carry over pin extension counts so a copy of an extended A+B keeps
				// the same shape; SetXxx prunes any cables already on m_module that
				// would land outside the new extension range.
				m_module.SetInputExtensionCount(ControllerView.m_lastCreated.InputExtensionCount);
				m_module.SetOutputExtensionCount(ControllerView.m_lastCreated.OutputExtensionCount);
				m_module.Prototype.ExecuteInit(m_module);
			});
			this.Observe(() => ControllerView.m_lastCreated)
				.Do(m => paste.Enabled(!(m is null) && m.Prototype.Id == m_module.Prototype.Id));
			row.Add(paste);

			ButtonIcon saveBp = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Save_svg)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.Margin(Px.Zero)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
				.Icon.Padding(Sizes.IMAGE_PADDING)
					 .Margin(Px.Zero)
				.Parent.As<ButtonIcon>().Value;
			saveBp.Tooltip("Save this module as a reusable blueprint".ToDoLoc());
			saveBp.OnClick(() =>
			{
				SaveBlueprintDialog.ForModule(m_module, saveBp, uiContext);
			});
			row.Add(saveBp);

			ButtonIcon remove = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.Trash128_png)
				.Size(Sizes.BLOCK_SIZE * 1.5f, Sizes.BLOCK_SIZE)
				.Margin(Px.Zero)
				.IconSize(Sizes.IMAGE_SIZE, Sizes.IMAGE_SIZE)
				.Icon.Padding(Sizes.IMAGE_PADDING)
					 .Margin(Px.Zero)
				.Parent.As<ButtonIcon>().Value;
			remove.Tooltip("Removes current module".ToDoLoc());
			remove.OnClick(() =>
			{
				controllerView.RemoveModule(m_module);
				Close();
			});
			row.Add(remove);

			foreach (IField item in m_module.Prototype.Fields)
			{
				item.Init(controllerInspector, controllerInspector, body, uiContext, (Module)m_module, () => { });
			}

			body.AddAndReturn(new UiComponent()).FlexGrow(1);

			//this.MaxHeight(450.px());
			this.Height(Px.Auto);
			this.Width(420.px());

			Open(button);
		}
	}
}