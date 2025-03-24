using Mafi.Unity.UiFramework;
using Mafi.Unity.UiFramework.Components;
using Mafi.Unity.UserInterface;
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
        void Init(ControllerInspector inspector, ItemDetailWindowView parentWindow, StackContainer fieldContainer, UiBuilder uiBuilder, Module module, System.Action updateDialog);
        void InitData(Module module);
    }
}