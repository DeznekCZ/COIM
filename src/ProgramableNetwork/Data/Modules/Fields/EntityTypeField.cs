using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
using Mafi.Unity.UserInterface.Components;
using System;

namespace ProgramableNetwork
{
    internal class EntityTypeField<T> : IField
        where T : EntityProto, IProtoWithIcon
    {
        public EntityTypeField(string id, string name, string description = null, Func<Module, T, bool> func = null)
        {
            Id = id;
            Name = name;
            ShortDesc = description;
            Filter = func ?? ((m, e) => true);
            EntityType = typeof(T);
        }

        public string Id { get; }

        public string Name { get; }
        public string ShortDesc { get; }
        public int Size => 40;

        public Func<Module, T, bool> Filter { get; }
        public Type EntityType { get; }

        public void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, Action updateDialog)
        {
            fieldContainer.SetStackingDirection(StackContainer.Direction.LeftToRight);
            fieldContainer.SetHeight(40);

            var txt = uiBuilder
                .NewBtnGeneral("name")
                .SettingFieldNameStyle(uiBuilder)
                .SetParent(fieldContainer, true)
                .SetWidth(180)
                .SetHeight(40)
                .SetText(Name)
                .ToolTip(inspector, ShortDesc, attached: true)
                .AppendTo(fieldContainer);

            ProtoTab<T> protoTab = new ProtoTab<T>(uiBuilder, module, Id, Filter, updateDialog, parentWindow, inspector.Context);
            protoTab.AppendTo(fieldContainer);
        }

        public void InitData(Module module)
        {
            // nothing to do
        }

        public void Validate(Module module)
        {
            if (module.StringData.TryGetValue("field__" + Id, out var id))
                FixSavedGames.ValidatePrototypeString(id, (value) => module.NumberData["field__" + Id] = value);
        }
    }
}