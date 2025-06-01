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
    public class LastCreatedModule : AModuleProtoSelector
    {
        private Module lastCreated;

        public LastCreatedModule(Controller controller, ControllerView controllerView, Action refresh, Action<Module> onSuccess, Func<ModuleProto, (bool, Module)> tryCreate, Module lastCreated)
            : base(controller, controllerView, refresh, onSuccess, tryCreate)
        {
            this.lastCreated = lastCreated;
        }

        public override string IconPath => lastCreated.GetIcon();

        public override Proto.Str Strings =>
            new Proto.Str(
                LocalizationManager.GetLocalizedString0Arg(
                    Id.Value,
                    string.Join(" ",
                        lastCreated.Prototype.Symbol,
                        lastCreated.Prototype.Strings.Name.TranslatedString,
                        lastCreated.Prototype.Strings.DescShort.TranslatedString
                    ),
                    "No comment"
                ));

        public override Proto.ID Id => lastCreated.Prototype.Id;

        public override bool IsAvailable => lastCreated.Prototype.AllowedDevices.Contains(m_controller.Prototype.Id);

        public override Button CreateUi()
        {
            var button = new ButtonRow(new ButtonVariant().Gap(5))
            {
                new ModuleView(new Module(lastCreated.Prototype, m_controller.Context, m_controller), m_controllerView, m_controllerView.Inspector.Context, true, () => { })
                    .With(mv => {
                        mv.Module.Prototype.ExecuteInit(mv.Module, log: false);

                        foreach (KeyValuePair<string, int> item in lastCreated.NumberData)
                            mv.Module.NumberData[item.Key] = item.Value;
                        foreach (KeyValuePair<string, string> item in lastCreated.StringData)
                            mv.Module.StringData[item.Key] = item.Value;

                        mv.Module.Prototype.DisplayUpdate(mv.Module);
                    }),
                new PanelWithHeader($"Copy last created: {lastCreated.Prototype.Strings.Name.TranslatedString}".AsLoc())
                    .Height(Sizes.BLOCK_SIZE * 4).FlexGrow(1)
                    .BodyAdd(new Label(lastCreated.Prototype.Strings.DescShort).FlexGrow(1).TextAlign(TextAlignment.LeftTop).AlignSelf(Align.Stretch))
            };
            return button;
        }

        public override void Selected()
        {
            (bool created, Module module) = m_tryCreate(lastCreated.Prototype);
            if (created)
            {
                module.Prototype.ExecuteInit(module);

                foreach (KeyValuePair<string, int> item in lastCreated.NumberData)
                    module.NumberData[item.Key] = item.Value;
                foreach (KeyValuePair<string, string> item in lastCreated.StringData)
                    module.StringData[item.Key] = item.Value;

                foreach (IField field in module.Prototype.Fields)
                    field.Validate(module);
            }
        }
    }
}
