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

        // Opt-in: when true the module's hover tooltip (only in Edit mode) lists this
        // field's current value. Set via the AddXxxField builder's showInTooltip flag.
        bool ShowInTooltip { get; }

        // When set to a pin id, the row in the settings panel hides when that
        // pin isn't currently active on the module (i.e. when its extension index
        // is past the current InputExtensionCount). Lets threshold/companion
        // fields appear in lock-step with their paired extension pin without
        // leaving orphan controls in the inspector.
        string LinkedInputPinId => null;

        void Validate(Module module);
        void Init(Ui.ControllerInspector inspector, Window parentWindow, UiComponent fieldContainer, UiContext uiContext, Module module, Action updateDialog, bool directEdit = false);
        void InitData(Module module);

        // Renders the field's current value as a string for the hover tooltip. Empty string
        // means "no value to show". Only consulted when ShowInTooltip is true.
        string GetTooltipValue(Module module);
    }
}