using System.Collections.Generic;
using ProgramableNetwork.Python;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>
    /// Plain, engine-free description of one module type, recovered by <see cref="ModuleLoader"/> from
    /// the parsed Python class. This is the single metadata object used by BOTH the web preview and the
    /// simulator — it mirrors what <c>ModuleRegistrator.Register</c> reads out of <c>classContext</c>.
    /// </summary>
    public sealed class ModuleDefinition
    {
        public string Id;
        public string Name;
        public string Symbol;
        public string Description;
        public int Width = 1;

        public readonly List<PinDef> Inputs = new List<PinDef>();
        public readonly List<PinDef> Outputs = new List<PinDef>();
        public readonly List<FieldDef> Fields = new List<FieldDef>();
        public readonly List<ProgramableNetwork.ModuleProto.DisplayDef> Displays =
            new List<ProgramableNetwork.ModuleProto.DisplayDef>();
        public readonly List<string> Categories = new List<string>();

        public int InputExtensions;
        public int OutputExtensions;
        public int DisplayExtensions;

        /// <summary>The parsed class — holds the action/Init/Display methods and the class context.</summary>
        public Class PythonClass;

        // Method is an internal interpreter type; consumed only by ModuleLoader/Simulator in this assembly.
        internal Method ActionMethod;
        internal Method InitMethod;
        internal Method DisplayMethod;

        /// <summary>
        /// True when this module carries executable Python behavior (an <c>action</c>) and can be
        /// simulated. C# modules loaded from the generated <c>ids.py</c> descriptor are metadata-only,
        /// so they preview but stay inert in the simulator.
        /// </summary>
        public bool HasBehavior => ActionMethod != null;

        public PinDef FindInput(string id) => Find(Inputs, id);
        public PinDef FindOutput(string id) => Find(Outputs, id);

        private static PinDef Find(List<PinDef> list, string id)
        {
            foreach (PinDef p in list)
            {
                if (p.Id == id)
                {
                    return p;
                }
            }
            return null;
        }
    }

    public sealed class PinDef
    {
        public string Id;
        public string Name;
        public bool Shared;

        public PinDef(string id, string name, bool shared = false)
        {
            Id = id;
            Name = name;
            Shared = shared;
        }
    }

    public enum FieldKind { Int32, Int64, Fix32, Boolean, String, Entity }

    public sealed class FieldDef
    {
        public string Id;
        public string Name;
        public string Description;
        public FieldKind Kind;
        public object Default;
        public bool OverrideInput;
        /// <summary>For <see cref="FieldKind.Entity"/>: the allowed entity type tag (used by the mock picker).</summary>
        public string EntityType;
    }
}
