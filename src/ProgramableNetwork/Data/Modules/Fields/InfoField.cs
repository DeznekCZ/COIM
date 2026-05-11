using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui
{
	/// <summary>
	/// Non-interactive read-only field that renders a paragraph of explanatory
	/// text in the inspector / picker settings panel.  Useful for modules whose
	/// other fields need a one-time "what these values mean" intro — keeps the
	/// per-field labels short (e.g. just "A".Shared()) without dropping context.
	/// Holds no data; <see cref="InitData"/> / <see cref="Validate"/> are no-ops.
	/// </summary>
	public class InfoField : IField
	{
		public InfoField(string id, Proto.Str strs)
		{
			Id = id;
			Name = strs.Name;
			ShortDesc = strs.DescShort;
		}

		public string Id { get; }
		public LocStr Name { get; }
		public LocStr ShortDesc { get; }
		public bool ShowInTooltip => false;
		public int Size => 40;

		public string GetTooltipValue(Module module) => "";

		public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog, bool directEdit = false)
		{
			Label info = new Label();
			info.Value(Name);
			info.TextOverflow(TextOverflow.Wrap);
			info.Padding(5.px());
			info.Margin(top: 2.px(), right: Px.Zero, bottom: 2.px(), left: Px.Zero);
			fieldContainer.Add(info);
		}

		public void InitData(Module module) { }

		public void Validate(Module module) { }
	}
}
