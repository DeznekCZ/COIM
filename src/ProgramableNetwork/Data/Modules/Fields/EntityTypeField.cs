using Mafi.Core.Entities;
using Mafi.Core.Prototypes;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
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

        public void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog)
        {
            ProtoTab<T> protoTab = new ProtoTab<T>(uiContext, module, Id, Filter, updateDialog, parentWindow, inspector);
            fieldContainer.Row(this, module, out _).Add(protoTab);
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