using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using static ProgramableNetwork.ControllerView;

namespace ProgramableNetwork
{
    public class NewModule : AModuleProtoSelector
    {
        private ModuleProto item;

        public NewModule(Controller controller, ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, ModuleProto item)
            : base(controller, controllerView, refresh, onSuccess, tryCreate)
        {
            this.item = item;
        }

        public override string IconPath => item.IconPath;

        public override Proto.Str Strings => item.Strings;

        public override Proto.ID Id => item.Id;

        public override bool IsAvailable => item.IsAvailable;

        public override Button CreateUi()
        {
            var button = new ButtonRow(new ButtonVariant().Gap(5))
            {
                new ModuleView(new Module(item, m_controller.Context, m_controller), m_controllerView, m_controllerView.Inspector.Context, true, () => { }),
                new PanelWithHeader(Strings.Name)
                    .Height(Sizes.BLOCK_SIZE * 4).FlexGrow(1)
                    .BodyAdd(new Label(Strings.DescShort).FlexGrow(1).TextAlign(TextAlignment.LeftTop).AlignSelf(Align.Stretch))
            };
            return button;
        }

        public override void Selected()
        {
            (bool create, Module module) = m_tryCreate(item);
            if (create)
            {
                module.Prototype.ExecuteInit(module);
            }
        }
    }
}
