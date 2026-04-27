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
			// TODO: copy, paste, template
			RowContainer row = new PanelRow();
			//row.Height(Sizes.BLOCK_SIZE);
			//row.Class(Cls.groupHeader);
			//row.AlignItemsEnd();

			m_module = module;

			var panel = new PanelWithHeader(m_module.Prototype.Strings.Name);
			panel.Height(Px.Auto);
			Add(panel);

			var body = panel.Body;
			body.Add(row);
			body.Gap(5.px());

			// add filler
			row.AddAndReturn(new UiComponent()).Fill();

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

			ButtonIcon paste = new ButtonIcon(Mafi.Unity.Assets.Unity.UserInterface.General.ImportFromString_svg)
				.Size(Sizes.BLOCK_SIZE * 2, Sizes.BLOCK_SIZE)
				.Margin(Px.Zero)
				.IconSize(Sizes.IMAGE_SIZE * 2, Sizes.IMAGE_SIZE)
				.Icon.Padding(Sizes.IMAGE_PADDING, Sizes.IMAGE_PADDING)
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
				m_module.Prototype.ExecuteInit(m_module);
			});
			this.Observe(() => ControllerView.m_lastCreated)
				.Do(m => paste.Enabled(!(m is null) && m.Prototype.Id == m_module.Prototype.Id));
			row.Add(paste);

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