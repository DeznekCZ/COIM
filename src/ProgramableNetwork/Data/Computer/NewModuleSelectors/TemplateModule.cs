using Mafi.Core.Prototypes;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class TemplateModule : AModuleProtoSelector
    {
        private KeyValuePair<string, Template> item;

        public TemplateModule(Controller controller, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, KeyValuePair<string, Template> item)
            : base(controller, refresh, onSuccess, tryCreate)
        {
            this.item = item;
        }

        public override string IconPath => item.Value.ModuleProto.IconPath;

        public override Proto.Str Strings => item.Value.ModuleProto.Strings;

        public override Proto.ID Id => new Proto.ID($"Template_{item.Value.ModuleProto.Id.Value}_{item.Key}");

        public override bool IsAvailable => item.Value.ModuleProto.IsAvailable;

        public override Button CreateUi()
        {
            var button = new ButtonColumn(new ButtonVariant())
            {
                new Row {
                    new Label(new Mafi.Localization.LocStrFormatted("Module type:")),
                    new Label(Strings.Name)
                }.Height(Sizes.BLOCK_SIZE),
                new Row {
                    new Label(new Mafi.Localization.LocStrFormatted("Template alias:")),
                    new Label(new Mafi.Localization.LocStrFormatted(item.Value.Name))
                }.Height(Sizes.BLOCK_SIZE),
            };
            return button.Height(80);
        }

        public override void Selected()
        {
            (bool created, Module module) = m_tryCreate(item.Value.ModuleProto);
            if (created) {
                item.Value.Setting(module);
                module.Prototype.Init(module);
            }
        }
    }
}
