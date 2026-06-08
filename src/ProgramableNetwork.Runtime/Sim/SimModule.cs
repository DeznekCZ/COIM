using System.Collections.Generic;
using Mafi;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>
    /// Runtime state of one placed module in the browser simulator. Holds the dictionaries the
    /// interpreter reads/writes through <see cref="Python.ModuleWrapper"/> (inputs, outputs, fields,
    /// displays, number/string data, the scratch array) plus placement, status and wiring.
    /// </summary>
    public sealed class SimModule
    {
        public long Id;
        public int Row;
        public int Column;

        public readonly ModuleDefinition Definition;
        public SimController Controller;

        public int InputExtensionCount;
        public int OutputExtensionCount;
        public int DisplayExtensionCount;

        public readonly Dictionary<string, Fix32> Inputs = new Dictionary<string, Fix32>();
        public readonly Dictionary<string, Fix32> Outputs = new Dictionary<string, Fix32>();
        public readonly Dictionary<string, Fix32> FieldNumbers = new Dictionary<string, Fix32>();
        public readonly Dictionary<string, string> FieldStrings = new Dictionary<string, string>();
        public readonly Dictionary<string, MockEntity> FieldEntities = new Dictionary<string, MockEntity>();
        /// <summary>Per-field FieldOrInput toggle: true = read the field, false = read the connected input.</summary>
        public readonly Dictionary<string, bool> FieldUseField = new Dictionary<string, bool>();

        public readonly Dictionary<string, string> Displays = new Dictionary<string, string>();
        public readonly Dictionary<string, int> NumberData = new Dictionary<string, int>();
        public readonly Dictionary<string, string> StringData = new Dictionary<string, string>();
        public readonly List<Fix32> Array = new List<Fix32>();

        /// <summary>input pin id -> source of its value (another module's output, or a constant).</summary>
        public readonly Dictionary<string, Connection> Connections = new Dictionary<string, Connection>();

        public ModuleStatus Status = ModuleStatus.Init;
        public string Error = "";
        public bool Info;
        public bool Warning;
        public bool HasInitialised;

        public SimModule(ModuleDefinition definition)
        {
            Definition = definition;
        }

        // ---- effective pin enumeration (statics + active extensions), mirrors ModuleWrapper ----
        public int EffectiveInputCount =>
            Definition.Inputs.Count + System.Math.Min(InputExtensionCount, Definition.InputExtensions);

        public int EffectiveOutputCount =>
            Definition.Outputs.Count + System.Math.Min(OutputExtensionCount, Definition.OutputExtensions);

        public string EffectiveInputId(int idx)
        {
            if (idx < 0)
            {
                return "";
            }
            if (idx < Definition.Inputs.Count)
            {
                return Definition.Inputs[idx].Id;
            }
            return "";
        }

        public string EffectiveOutputId(int idx)
        {
            if (idx < 0)
            {
                return "";
            }
            if (idx < Definition.Outputs.Count)
            {
                return Definition.Outputs[idx].Id;
            }
            return "";
        }

        public Fix32 GetInput(string name, Fix32 fallback) => Inputs.TryGetValue(name, out Fix32 v) ? v : fallback;
        public Fix32 GetOutput(string name, Fix32 fallback) => Outputs.TryGetValue(name, out Fix32 v) ? v : fallback;
        public Fix32 GetField(string name, Fix32 fallback) => FieldNumbers.TryGetValue(name, out Fix32 v) ? v : fallback;
    }

    public enum ConnectionKind { None, Module, Constant, Bus }

    /// <summary>Where an input pin draws its value from in the simulator.</summary>
    public sealed class Connection
    {
        public ConnectionKind Kind = ConnectionKind.None;
        public long SourceModuleId;
        public string SourceOutputId;
        public Fix32 ConstantValue;
        public long BusId;
        public int BusPinIndex;
    }
}
