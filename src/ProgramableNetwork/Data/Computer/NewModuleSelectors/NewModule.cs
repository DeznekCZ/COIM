using Mafi.Core.Prototypes;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public class NewModule : AModuleProtoSelector
    {
        private ModuleProto item;

        public NewModule(Controller controller, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, ModuleProto item) : base(controller, refresh, onSuccess, tryCreate)
        {
            this.item = item;
        }

        public override string IconPath => item.IconPath;

        public override Proto.Str Strings => item.Strings;

        public override Proto.ID Id => item.Id;

        public override bool IsAvailable => item.IsAvailable;

        public override Button CreateUi()
        {
            var button = new ButtonRow(new ButtonVariant())
            {
                new Label(new Mafi.Localization.LocStrFormatted("Module:")),
                new Label(Strings.Name)
            };
            return button.Height(40);
        }

        public override void Selected()
        {
            m_tryCreate(item);
        }
    }
}
