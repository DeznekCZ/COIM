using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork.Ui {
	internal class EntityTypeField<T> : IField
		where T : EntityProto, IProtoWithIcon {
		public EntityTypeField(string id, Proto.Str strs, Func<Module, T, bool> func = null, bool showInTooltip = false) {
			Id = id;
			Name = strs.Name;
			ShortDesc = strs.DescShort;
			Filter = func ?? ((m, e) => true);
			EntityType = typeof(T);
			ShowInTooltip = showInTooltip;
		}

		public string Id { get; }

		public LocStr Name { get; }
		public LocStr ShortDesc { get; }
		public bool ShowInTooltip { get; }
		public int Size => 40;

		public Func<Module, T, bool> Filter { get; }
		public Type EntityType { get; }

		public string GetTooltipValue(Module module) {
			if (module.StringData.TryGetValue("field__" + Id, out var protoId)) {
				return protoId ?? "";
			}
			return "";
		}

		public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog, bool directEdit = false) {
			ProtoTab<T> protoTab = new ProtoTab<T>(uiContext, module, Id, Filter, updateDialog, parentWindow, inspector, directEdit);
			fieldContainer.Row(this, module, uiContext, out _, directEdit: directEdit).Add(protoTab);
		}

		public void InitData(Module module) {
			// nothing to do
		}

		public void Validate(Module module) {
			if (module.StringData.TryGetValue("field__" + Id, out var id)) {
				FixSavedGames.ValidatePrototypeString(id, (value) => module.FieldNumberData[Id] = Mafi.Fix32.FromRaw(value));
			}
		}
	}
}