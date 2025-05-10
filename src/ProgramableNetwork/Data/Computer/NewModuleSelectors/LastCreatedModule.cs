using Mafi.Core.Prototypes;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork
{
    public class LastCreatedModule : AModuleProtoSelector
    {
        private Module lastCreated;

        public LastCreatedModule(Controller controller, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, Module lastCreated)
            : base(controller, refresh, onSuccess, tryCreate)
        {
            this.lastCreated = lastCreated;
        }

        public override string IconPath => lastCreated.GetIcon();

        public override Proto.Str Strings => lastCreated.Prototype.Strings;

        public override Proto.ID Id => lastCreated.Prototype.Id;

        public override bool IsAvailable => lastCreated.Prototype.AllowedDevices.Contains(m_controller.Prototype.Id);

        public override Button CreateUi()
        {
            var button = new ButtonRow(new ButtonVariant())
            {
                new Label(new Mafi.Localization.LocStrFormatted("Copy last created:")),
                new Label(Strings.Name)
            };
            return button.Height(Sizes.BLOCK_SIZE);
        }

        public override void Selected()
        {
            (bool created, Module module) = m_tryCreate(lastCreated.Prototype);
            if (created)
            {
                foreach (KeyValuePair<string, int> item in lastCreated.NumberData)
                    module.NumberData[item.Key] = item.Value;
                foreach (KeyValuePair<string, string> item in lastCreated.StringData)
                    module.StringData[item.Key] = item.Value;
                module.Prototype.Init(module);
            }
        }
    }
}
