using Mafi;
using Mafi.Collections;
using Mafi.Localization;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;

namespace ProgramableNetwork.Ui;

public class SideTabContainer : Row {

	private readonly ScrollColumn m_selectionColumnContainer;
	private readonly Lyst<Tab> m_tabs;
	private Button m_selectedButton;

	public SideTabContainer() {
		m_selectionColumnContainer = AddAndReturn(new ScrollColumn())
			.Width(Percent.Twenty)
			.Height(Percent.Hundred);
		m_tabs = [];
	}

	public void AddTab(LocStrFormatted title, UiComponent tabContent) {
		Button button = new ButtonText(title)
			.Width(Percent.Hundred)
			.Toggleable()
			.OnClick((b) => {
				if (m_selectedButton == b) {
					return;
				}
				m_selectedButton?.Selected(false);
				(m_selectedButton = b).Selected();
			});
		Panel panel = AddAndReturn(new Panel())
			.Width(Percent.Eighty)
			.Height(Percent.Hundred)
			.Padding(4.px())
			.ObserveVisible(this, button.IsSelected);
		panel.AddAndReturn(new ScrollColumn())
			.Width(Percent.Hundred)
			.Height(Percent.Hundred)
			.AddAndReturn(tabContent);
		button.Selected(ChildrenCount == 1);
		if (button.IsSelected()) {
			m_selectedButton = button;
		}
		m_selectionColumnContainer.Add(button);
		m_tabs.Add(new Tab(button, title, tabContent, panel));
	}

	private readonly struct Tab(Button button, LocStrFormatted title, UiComponent tabContent, Panel panel) {
		public Button Button { get; } = button;
		public LocStrFormatted Title { get; } = title;
		public UiComponent Content { get; } = tabContent;
		public Panel Panel { get; } = panel;
	}
}
