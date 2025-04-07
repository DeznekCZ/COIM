using Mafi.Unity.Ui;
using Mafi.Unity.UiToolkit.Component;
using Mafi.Unity.UiToolkit.Library;
using System;

namespace ProgramableNetwork
{
    public interface IField
    {
        string Id { get; }
        [Obsolete("Usable only in tooltip", true)]
        string Name { get; }
        int Size { get; }

        void Validate(Module module);
        void Init(ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, System.Action updateDialog);
        void InitData(Module module);
    }
}