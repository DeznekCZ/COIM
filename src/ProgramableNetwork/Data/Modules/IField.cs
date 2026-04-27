using Mafi.Localization;
using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public interface IField
    {
        string Id { get; }
        LocStr Name { get; }
        LocStr ShortDesc { get; }
        int Size { get; }

        void Validate(Module module);
        void Init(Ui.ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog);
        void InitData(Module module);
    }
}