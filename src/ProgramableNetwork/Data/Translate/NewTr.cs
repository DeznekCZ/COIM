using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public partial class NewTr
    {
        public static readonly LocStr2 InvalidPointerType = Loc.Str2("ProgramableNetwork_InvalidPointerType",
            "Invalid pointer type: expected='{0}' != got:'{1}'", "");

        public static readonly LocStr2 IndexOutOfRange = Loc.Str2("ProgramableNetwork_IndexOutOfRange",
            "Index out of range: 0 <= {0} < {1}", "");

        public static readonly LocStr1 EmptyInput = Loc.Str1("ProgramableNetwork_EmptyInput",
            "Input '{0}' is not set", "");

        public static readonly LocStr2 EmptyInputIndexed = Loc.Str2("ProgramableNetwork_EmptyInputIndexed",
            "Input '{0}' at index '{1}' is not set", "");

        public static readonly LocStr2 TypeOr = Loc.Str2("ProgramableNetwork_TypeOr",
            "{0}' or '{1}", "used in empty example argument");

        public static readonly LocStr CanNotPause = Loc.Str("ProgramableNetwork_CanNotPause",
            "Selected entity can not be paused", "");

        public static readonly LocStr HasNoStorage = Loc.Str("ProgramableNetwork_HasNoStorage",
            "Selected entity has no storage", "");

        public static readonly LocStr WatchDogStop = Loc.Str("ProgramableNetwork_WatchDogStop",
            "Instruction exceeded processable time", "");

        public static readonly LocStr UnknownError = Loc.Str("ProgramableNetwork_UnknownError",
            "Contact captain DeznekCZ, there went something wrong", "");

        public static readonly LocStr1 ModulesCount = Loc.Str1("ProgramableNetwork_ModulesCount",
            "Count of modules: {0}", "");

        public static readonly LocStr InvalidInstruction = Loc.Str("ProgramableNetwork_InvalidInstruction",
            "Invalid instruction", "");

        public static readonly LocStr1 Variable = Loc.Str1("ProgramableNetwork_Variable",
            "Variable picked: {0}", "");

        public static readonly LocStr True = Loc.Str("ProgramableNetwork_True",
            "True", "");

        public static readonly LocStr False = Loc.Str("ProgramableNetwork_False",
            "False", "");

        public static readonly LocStr End = Loc.Str("ProgramableNetwork_End",
            "End", "");

        public static readonly LocStr DivisionByZero = Loc.Str("ProgramableNetwork_DivisionByZero",
            "Division by zero", "This may happen during operation: A / B");

        public static readonly LocStr CableIsNotConnected = Loc.Str("ProgramableNetwork_CableIsNotConnected",
            "Cable is not connected", "");

        //public static readonly Dictionary<InstructionProto.InputType, LocStr> PointerTypes
        //    = new Dictionary<InstructionProto.InputType, LocStr>()
        //    {
        //        { InstructionProto.InputType.None,               Loc.Str("ProgramableNetwork_PointerType_None"         , "None", "") },
        //        { InstructionProto.InputType.Any,                Loc.Str("ProgramableNetwork_PointerType_Any"          , "Any", "") },
        //        { InstructionProto.InputType.Variable,           Loc.Str("ProgramableNetwork_PointerType_Variable"     , "Variable", "") },
        //        { InstructionProto.InputType.Instruction,        Loc.Str("ProgramableNetwork_PointerType_Instruction"  , "Instruction", "") },
        //        { InstructionProto.InputType.Boolean,            Loc.Str("ProgramableNetwork_PointerType_Boolean"      , "Boolean", "") },
        //        { InstructionProto.InputType.Integer,            Loc.Str("ProgramableNetwork_PointerType_Integer"      , "Integer", "") },
        //        { InstructionProto.InputType.Entity,             Loc.Str("ProgramableNetwork_PointerType_Entity"       , "Entity", "") },
        //        { InstructionProto.InputType.StaticEntity,       Loc.Str("ProgramableNetwork_PointerType_StaticEntity" , "Static", "") },
        //        { InstructionProto.InputType.DynamicEntity,      Loc.Str("ProgramableNetwork_PointerType_DynamicEntity", "Dynamic", "") },
        //        { InstructionProto.InputType.Product,            Loc.Str("ProgramableNetwork_PointerType_Product"      , "Product", "") },
        //
        //    };

        public static readonly Dictionary<bool, LocStr> Boolean
            = new Dictionary<bool, LocStr>()
            {
                { true,  True },
                { false, False },
            };

        public partial class Inspector
        {
            public static readonly LocStr ComputingSpeed = Loc.Str("ProgramableNetwork_Inspector_ComputingSpeed",
                "Computing speed", "controller inspector: row label for the speed throttle");
            public static readonly LocStr ComputingSpeedTooltip = Loc.Str("ProgramableNetwork_Inspector_ComputingSpeedTooltip",
                "Ticks per 60 seconds", "controller inspector: tooltip for the speed display");
            public static readonly LocStr Modules = Loc.Str("ProgramableNetwork_Inspector_Modules",
                "Modules", "controller inspector: panel header above the module grid");
            public static readonly LocStr ShowHints = Loc.Str("ProgramableNetwork_Inspector_ShowHints",
                "Hints", "controller inspector: header checkbox label/tooltip — when on, hover hints (slot '+ click to add', per-module LMB/Alt+LMB/Shift+LMB/RMB/Shift+RMB cheatsheet) appear; when off, the inspector stays quiet");
            public static readonly LocStr ExtensionCountInputs = Loc.Str("ProgramableNetwork_Inspector_ExtensionCountInputs",
                "Additional input pins", "module picker settings dialog: label for the input-extension count row (counts EXTRA pins beyond the module's static base — clarified from 'Input pins' to make the additive semantic obvious)");
            public static readonly LocStr ExtensionCountOutputs = Loc.Str("ProgramableNetwork_Inspector_ExtensionCountOutputs",
                "Additional output pins", "module picker settings dialog: label for the output-extension count row (counts EXTRA pins beyond the module's static base)");
            public static readonly LocStr ExtensionCountDisplays = Loc.Str("ProgramableNetwork_Inspector_ExtensionCountDisplays",
                "Additional display cells", "module picker settings dialog: label for the display-extension count row (counts EXTRA cells beyond the display widget's static width)");
            public static readonly LocStr ConfirmRemoveModule = Loc.Str("ProgramableNetwork_Inspector_ConfirmRemoveModule",
                "Remove this module?", "controller inspector: question shown in the right-click confirmation dropdown before deleting a module");
            public static readonly LocStr ConfirmRemoveYes = Loc.Str("ProgramableNetwork_Inspector_ConfirmRemoveYes",
                "Remove", "controller inspector: confirm-remove dialog — destructive action button label");
            public static readonly LocStr ConfirmRemoveCancel = Loc.Str("ProgramableNetwork_Inspector_ConfirmRemoveCancel",
                "Cancel", "controller inspector: confirm-remove dialog — cancel button label");
            public static readonly LocStr All = Loc.Str("ProgramableNetwork_Inspector_All",
                "All", "module picker: button that selects all category filters at once");
            public static readonly LocStr ControllerColor = Loc.Str("ProgramableNetwork_Inspector_ControllerColor",
                "Controller color:", "controller inspector: title of the color picker floater");
            public static readonly LocStr LightColor = Loc.Str("ProgramableNetwork_Inspector_LightColor",
                "Light color:", "display-entity light inspector: title of the color picker floater");
            public static readonly LocStr PickModule = Loc.Str("ProgramableNetwork_Inspector_PickModule",
                "Pick module", "controller inspector: heading for the module picker dialog");
            public static readonly LocStr PickTemplate = Loc.Str("ProgramableNetwork_Inspector_PickTemplate",
                "Pick template", "controller inspector: heading for the template picker dialog");
            public static readonly LocStr Shift = Loc.Str("ProgramableNetwork_Inspector_Shift",
                "Shift", "controller inspector: 'Shift' modifier-key label in add-helper hints");
            public static readonly LocStr AddLastCreated = Loc.Str("ProgramableNetwork_Inspector_AddLastCreated",
                "Add last created / copied", "controller inspector: helper hint for shift+click to repeat the last add");
            public static readonly LocStr AddNewModule = Loc.Str("ProgramableNetwork_Inspector_AddNewModule",
                "Add new module", "controller inspector: helper hint for left-click to add a module");
            public static readonly LocStr AddFromTemplate = Loc.Str("ProgramableNetwork_Inspector_AddFromTemplate",
                "Add from template", "controller inspector: helper hint for right-click to add from a template");
            public static readonly LocStr Alt = Loc.Str("ProgramableNetwork_Inspector_Alt",
                "Alt", "controller inspector: 'Alt' modifier-key label in placed-module hover hints");
            public static readonly LocStr ModuleHintOpen = Loc.Str("ProgramableNetwork_Inspector_ModuleHintOpen",
                "Open settings", "controller inspector: placed-module hover hint for plain LMB");
            public static readonly LocStr ModuleHintMove = Loc.Str("ProgramableNetwork_Inspector_ModuleHintMove",
                "Pick up / drop on free slot", "controller inspector: placed-module hover hint for Alt+LMB move flow");
            public static readonly LocStr ModuleHintCopy = Loc.Str("ProgramableNetwork_Inspector_ModuleHintCopy",
                "Copy as last created", "controller inspector: placed-module hover hint for Shift+LMB copy");
            public static readonly LocStr ModuleHintRemove = Loc.Str("ProgramableNetwork_Inspector_ModuleHintRemove",
                "Remove (with confirm)", "controller inspector: placed-module hover hint for plain RMB");
            public static readonly LocStr ModuleHintRemoveDirect = Loc.Str("ProgramableNetwork_Inspector_ModuleHintRemoveDirect",
                "Remove (no confirm)", "controller inspector: placed-module hover hint for Shift+RMB direct delete");
			public static readonly LocStr NetworkVariablesTitle = Loc.Str(
				"ProgramableNetwork_NetworkVariablesTitle",
				"Network variables", "");

			public static readonly LocStr Connections = Loc.Str("ProgramableNetwork_Inspector_Connections",
                "Connections", "controller inspector: panel header for the per-module entity connections list");
            public static readonly LocStr ConnectionsEmpty = Loc.Str("ProgramableNetwork_Inspector_ConnectionsEmpty",
                "No entity-bound fields", "controller inspector: shown in connections panel when no module has any entity field");
            public static readonly LocStr ConnectionsHintEdit = Loc.Str("ProgramableNetwork_Inspector_ConnectionsHintEdit",
                "Pick / change entity", "connections panel: helper hint for left-click on a slot");
            public static readonly LocStr ConnectionsHintCopyNext = Loc.Str("ProgramableNetwork_Inspector_ConnectionsHintCopyNext",
                "Apply last picked entity (if valid here)", "connections panel: helper hint for shift+click — applies the most recently picked entity to this slot if it passes the slot's filter and distance check");
            public static readonly LocStr ConnectionsHintClear = Loc.Str("ProgramableNetwork_Inspector_ConnectionsHintClear",
                "Clear entity", "connections panel: helper hint for right-click on a slot");
            public static readonly LocStr ConnectionsHintPan = Loc.Str("ProgramableNetwork_Inspector_ConnectionsHintPan",
                "Pan camera to entity", "connections panel: helper hint for middle-click on a slot");

            public static readonly LocStr Active = Loc.Str("ProgramableNetwork_Inspector_Active",
                "Active", "speaker / light inspector: row label for the on/off toggle");
            public static readonly LocStr Color = Loc.Str("ProgramableNetwork_Inspector_Color",
                "Color", "display inspector: row label for the LED color picker");
            public static readonly LocStr Segments = Loc.Str("ProgramableNetwork_Inspector_Segments",
                "Segments", "7/16-segment display inspector: panel header for the segment override toggles");
            public static readonly LocStr Disconnected = Loc.Str("ProgramableNetwork_Inspector_Disconnected",
                "Disconnected", "radio receiver display when no antena is in range");

            public static readonly LocStr Sound_Alarm = Loc.Str("ProgramableNetwork_Speaker_Sound_Alarm",
                "Alarm", "speaker sound option: ship alarm");
            public static readonly LocStr Sound_Cash = Loc.Str("ProgramableNetwork_Speaker_Sound_Cash",
                "Cash", "speaker sound option: money / cash register");
            public static readonly LocStr Sound_Shoot = Loc.Str("ProgramableNetwork_Speaker_Sound_Shoot",
                "Shoot", "speaker sound option: turret shot");
            public static readonly LocStr Sound_Message = Loc.Str("ProgramableNetwork_Speaker_Sound_Message",
                "Message", "speaker sound option: new message chime");
		}

        public partial class FieldStatus
        {
            public static readonly LocStr None = Loc.Str("ProgramableNetwork_FieldStatus_None",
                "NONE", "module field status: nothing connected and the constant override is off");
            public static readonly LocStr On = Loc.Str("ProgramableNetwork_FieldStatus_On",
                "ON", "module field status: constant override is on, value comes from the field");
            public static readonly LocStr Wired = Loc.Str("ProgramableNetwork_FieldStatus_Wired",
                "WIRED", "module field status: an input pin is connected, value comes from the wire");
        }

        public partial class Tools
        {
            public static readonly LocStr Remove = Loc.Str("ProgramableNetwork_Tool_Delete",
                "Remove", "");
            public static readonly LocStr Add = Loc.Str("ProgramableNetwork_Tool_Add",
                "Add", "");
            public static readonly LocStr Copy = Loc.Str("ProgramableNetwork_Tool_Copy",
                "Copy", "");
            public static readonly LocStr Paste = Loc.Str("ProgramableNetwork_Tool_Paste",
                "Paste", "");
            public static readonly LocStr Up = Loc.Str("ProgramableNetwork_Tool_Up",
                "Up", "");
            public static readonly LocStr Down = Loc.Str("ProgramableNetwork_Tool_Down",
                "Down", "");
            public static readonly LocStr Pick = Loc.Str("ProgramableNetwork_Tool_Pick",
                "Pick", "");
            public static readonly LocStr Templates = Loc.Str("ProgramableNetwork_Tool_Templates",
                "Templates", "");
            public static readonly LocStr Apply = Loc.Str("ProgramableNetwork_Tool_Apply",
                "Apply", "");
            public static readonly LocStr Edit = Loc.Str("ProgramableNetwork_Tool_Edit",
                "Edit", "");
        }
    }
}
