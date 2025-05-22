using Mafi.Core.Prototypes;
using Mafi.Localization;
using Mafi.Unity.UiToolkit;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;
using System.Collections.Generic;
using static ProgramableNetwork.ControllerView;

namespace ProgramableNetwork
{
    public class TemplateModule : AModuleProtoSelector
    {
        private KeyValuePair<string, Template> item;

        public TemplateModule(Controller controller, ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, KeyValuePair<string, Template> item)
            : base(controller, controllerView, refresh, onSuccess, tryCreate)
        {
            this.item = item;
        }

        public override string IconPath => item.Value.ModuleProto.IconPath;

        public override Proto.Str Strings => item.Value.ModuleProto.Strings;

        public override Proto.ID Id => new Proto.ID($"Template_{item.Value.ModuleProto.Id.Value}_{item.Key}");

        public override bool IsAvailable => item.Value.ModuleProto.IsAvailable;

        public override Button CreateUi()
        {
            var button = new ButtonRow(new ButtonVariant().Gap(5))
            {
                new ModuleView(new Module(item.Value.ModuleProto, m_controller.Context, m_controller), m_controllerView, m_controllerView.Inspector.Context, true, () => { }),
                new PanelWithHeader($"Template: {Strings.Name.TranslatedString}".AsLoc())
                    .Height(Sizes.BLOCK_SIZE * 4).FlexGrow(1)
                    .BodyAdd(new Label(new LocStrFormatted(item.Value.Name)).FlexGrow(1).TextAlign(TextAlignment.LeftTop).AlignSelf(Align.Stretch))
            };
            return button;
        }

        public override void Selected()
        {
            (bool created, Module module) = m_tryCreate(item.Value.ModuleProto);
            if (created) {
                module.Prototype.ExecuteInit(module);
                item.Value.Setting(module);
            }
        }
    }
}
