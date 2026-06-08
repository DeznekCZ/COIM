using System;
using Mafi;
using ProgramableNetwork.Runtime.Sim;

namespace ProgramableNetwork.Python
{
    /// <summary>
    /// Browser/simulator replacement for the mod's <c>ModuleWrapper</c>. It MUST live here (same
    /// name + namespace) because <c>PropertyExpression</c> special-cases <c>value is ModuleWrapper</c>
    /// to resolve Python-declared methods/fields via <see cref="@class"/>'s context (e.g. so
    /// <c>self.getHigh()</c> works). The Python-facing surface mirrors the mod wrapper exactly, but is
    /// backed by <see cref="SimModule"/> dictionaries instead of a COI <c>Module</c> entity.
    /// </summary>
    public class ModuleWrapper
    {
        public readonly SimModule module;
        public readonly Class @class;

        public ModuleWrapper(SimModule module, Class @class = null)
        {
            this.module = module;
            this.@class = @class;
        }

        // ---------------- FieldOrInput ----------------
        public FieldOrInputSetter FieldOrInput => new FieldOrInputSetter(module);

        public class FieldOrInputSetter
        {
            private readonly SimModule m;
            public FieldOrInputSetter(SimModule m) { this.m = m; }

            private bool useField(string name) => !m.FieldUseField.TryGetValue(name, out bool v) || v;

            public MockEntity get_ent(string name) =>
                m.FieldEntities.TryGetValue(name, out MockEntity e) ? e : MockEntity.Null;

            public Fix32 get(string name, Fix32 value) =>
                useField(name) && m.FieldNumbers.ContainsKey(name) ? m.FieldNumbers[name] : m.GetInput(name, value);

            public bool get_bool(string name, bool value) => get(name, value ? Fix32.One : Fix32.Zero).IsNotZero;
            public int get_int(string name, int value) => get(name, Fix32.FromInt(value)).IntegerPart;

            public Fix32 __getattr__(string name) => get(name, Fix32.Zero);
        }

        // ---------------- Field ----------------
        public FieldSetter Field => new FieldSetter(module);

        public class FieldSetter
        {
            private readonly SimModule m;
            public FieldSetter(SimModule m) { this.m = m; }

            public MockEntity get_ent(string name) =>
                m.FieldEntities.TryGetValue(name, out MockEntity e) ? e : MockEntity.Null;

            public bool get_bool(string name, bool value) => m.GetField(name, value ? Fix32.One : Fix32.Zero).IsNotZero;
            public void set_bool(string name, bool value) => m.FieldNumbers[name] = value ? Fix32.One : Fix32.Zero;
            public int get_int(string name, int value) => m.GetField(name, Fix32.FromInt(value)).IntegerPart;
            public void set_int(string name, int value) => m.FieldNumbers[name] = Fix32.FromInt(value);
            public Fix32 get(string name, Fix32 value) => m.GetField(name, value);
            public void set(string name, Fix32 value) => m.FieldNumbers[name] = value;

            public Fix32 __getattr__(string name) => m.GetField(name, Fix32.Zero);

            public void __setattr__(string name, object value)
            {
                m.FieldNumbers[name] = Expressions.__fix__(value);
            }
        }

        // ---------------- Input ----------------
        public InputSetter Input => new InputSetter(module);

        public class InputSetter
        {
            private readonly SimModule m;
            public InputSetter(SimModule m) { this.m = m; }

            public bool get_bool(string name, bool value) => m.GetInput(name, value ? Fix32.One : Fix32.Zero).IsNotZero;
            public void set_bool(string name, bool value) => m.Inputs[name] = value ? Fix32.One : Fix32.Zero;
            public int get_int(string name, int value) => m.GetInput(name, Fix32.FromInt(value)).IntegerPart;
            public void set_int(string name, int value) => m.Inputs[name] = Fix32.FromInt(value);
            public Fix32 get(string name, Fix32 value) => m.GetInput(name, value);
            public void set(string name, Fix32 value) => m.Inputs[name] = value;

            public Fix32 __getattr__(string name) => m.GetInput(name, Fix32.Zero);
            public void __setattr__(string name, object value) => m.Inputs[name] = Expressions.__fix__(value);
        }

        // ---------------- Output ----------------
        public OutputSetter Output => new OutputSetter(module);

        public class OutputSetter
        {
            private readonly SimModule m;
            public OutputSetter(SimModule m) { this.m = m; }

            public bool get_bool(string name, bool value) => m.GetOutput(name, Fix32.Zero).IsNotZero;
            public void set_bool(string name, bool value) => m.Outputs[name] = value ? Fix32.One : Fix32.Zero;
            public int get_int(string name, int value) => m.GetOutput(name, Fix32.FromInt(value)).IntegerPart;
            public void set_int(string name, int value) => m.Outputs[name] = Fix32.FromInt(value);
            public Fix32 get(string name, Fix32 value) => m.GetOutput(name, value);
            public void set(string name, Fix32 value) => m.Outputs[name] = value;

            public Fix32 __getattr__(string name) => m.GetOutput(name, Fix32.Zero);
            public void __setattr__(string name, object value) => m.Outputs[name] = Expressions.__fix__(value);
        }

        // ---------------- Display ----------------
        public DisplaySetter Display => new DisplaySetter(module);

        public class DisplaySetter
        {
            private readonly SimModule m;
            public DisplaySetter(SimModule m) { this.m = m; }

            public string get(string name, string value) => m.Displays.TryGetValue(name, out string v) ? v : value;
            public void set(string name, string value) => m.Displays[name] = value;

            public string __getattr__(string name) => m.Displays.TryGetValue(name, out string v) ? v : "";
            public void __setattr__(string name, object value) => m.Displays[name] = Expressions.__str__(value);
        }

        // ---------------- Array ----------------
        public ArraySetter Array => new ArraySetter(module);

        public class ArraySetter
        {
            private readonly SimModule m;
            public ArraySetter(SimModule m) { this.m = m; }

            public int length => m.Array.Count;

            public Fix32 get(int idx, Fix32 defaultValue) =>
                idx >= 0 && idx < m.Array.Count ? m.Array[idx] : defaultValue;

            public void set(int idx, Fix32 value)
            {
                if (idx >= 0 && idx < m.Array.Count)
                {
                    m.Array[idx] = value;
                }
            }

            public Fix32 __getitem__(object key)
            {
                int i = Expressions.__int__(key);
                return i >= 0 && i < m.Array.Count ? m.Array[i] : Fix32.Zero;
            }

            public void __setitem__(object key, object value)
            {
                int i = Expressions.__int__(key);
                if (i >= 0 && i < m.Array.Count)
                {
                    m.Array[i] = Expressions.__fix__(value);
                }
            }

            public void resize(int size) => resize(size, Fix32.Zero);

            public void resize(int size, Fix32 fill_new)
            {
                if (size < 0)
                {
                    size = 0;
                }
                while (m.Array.Count > size)
                {
                    m.Array.RemoveAt(m.Array.Count - 1);
                }
                while (m.Array.Count < size)
                {
                    m.Array.Add(fill_new);
                }
            }

            public void clear()
            {
                for (int i = 0; i < m.Array.Count; i++)
                {
                    m.Array[i] = Fix32.Zero;
                }
            }

            public Fix32 shift_left_with(Fix32 incoming)
            {
                if (m.Array.Count == 0)
                {
                    return incoming;
                }
                Fix32 oldest = m.Array[0];
                for (int i = 0; i < m.Array.Count - 1; i++)
                {
                    m.Array[i] = m.Array[i + 1];
                }
                m.Array[m.Array.Count - 1] = incoming;
                return oldest;
            }
        }

        // ---------------- status / flags ----------------
        public ModuleStatus Status
        {
            get => module.Status;
            set => module.Status = value;
        }

        public string Error
        {
            get => module.Error;
            set => module.Error = value;
        }

        public bool Info { get => module.Info; set => module.Info = value; }
        public bool Warning { get => module.Warning; set => module.Warning = value; }

        // ---------------- NumberData / StringData ----------------
        public NumberDataSetter NumberData => new NumberDataSetter(module);
        public StringDataSetter StringData => new StringDataSetter(module);

        public class NumberDataSetter
        {
            private readonly SimModule m;
            public NumberDataSetter(SimModule m) { this.m = m; }

            public int __getattr__(string name) => __getitem__(name);
            public void __setattr__(string name, object value) => __setitem__(name, value);

            public int __getitem__(object key)
            {
                string name = key as string;
                if (name == null)
                {
                    return 0;
                }
                return m.NumberData.TryGetValue(name, out int v) ? v : 0;
            }

            public void __setitem__(object key, object value)
            {
                string name = key as string;
                if (name == null)
                {
                    return;
                }
                m.NumberData[name] = Expressions.__int__(value);
            }
        }

        public class StringDataSetter
        {
            private readonly SimModule m;
            public StringDataSetter(SimModule m) { this.m = m; }

            public string __getattr__(string name) => __getitem__(name);
            public void __setattr__(string name, object value) => __setitem__(name, value);

            public string __getitem__(object key)
            {
                string name = key as string;
                if (name == null)
                {
                    return "";
                }
                return m.StringData.TryGetValue(name, out string v) ? v : "";
            }

            public void __setitem__(object key, object value)
            {
                string name = key as string;
                if (name == null)
                {
                    return;
                }
                m.StringData[name] = Expressions.__str__(value);
            }
        }

        // ---------------- Bus ----------------
        public BusNamespace Bus => new BusNamespace(module);

        public class BusNamespace
        {
            private readonly SimModule m;
            public BusNamespace(SimModule m) { this.m = m; }

            public BusPinSetter get(string busName) => new BusPinSetter(m, busName);
            public BusPinSetter __getattr__(string busName) => new BusPinSetter(m, busName);
        }

        public class BusPinSetter
        {
            private readonly SimModule m;
            private readonly string busName;

            public BusPinSetter(SimModule m, string busName)
            {
                this.m = m;
                this.busName = busName;
            }

            private SimBus bus => m.Controller?.GetBus(busName);

            public Fix32 get(string pinName, Fix32 value)
            {
                SimBus b = bus;
                if (b == null || b.IndexOfPin(pinName) < 0)
                {
                    return value;
                }
                return b.Get(pinName);
            }

            public void set(string pinName, Fix32 value) => bus?.Set(pinName, value);

            public int get_int(string pinName, int value) => get(pinName, Fix32.FromInt(value)).IntegerPart;
            public void set_int(string pinName, int value) => bus?.Set(pinName, Fix32.FromInt(value));
            public bool get_bool(string pinName, bool value) => get(pinName, value ? Fix32.One : Fix32.Zero).IsNotZero;
            public void set_bool(string pinName, bool value) => bus?.Set(pinName, value ? Fix32.One : Fix32.Zero);

            public Fix32 __getattr__(string pinName)
            {
                SimBus b = bus;
                return b != null ? b.Get(pinName) : Fix32.Zero;
            }

            public void __setattr__(string pinName, object value) => bus?.Set(pinName, Expressions.__fix__(value));

            public Fix32 __getitem__(object key)
            {
                SimBus b = bus;
                if (b == null)
                {
                    return Fix32.Zero;
                }
                int i = Expressions.__int__(key);
                return i >= 0 && i < SimBus.PinCount ? b.PinValues[i] : Fix32.Zero;
            }

            public void __setitem__(object key, object value)
            {
                SimBus b = bus;
                if (b == null)
                {
                    return;
                }
                int i = Expressions.__int__(key);
                if (i >= 0 && i < SimBus.PinCount)
                {
                    b.PinValues[i] = Expressions.__fix__(value);
                }
            }
        }

        // ---------------- extension counts / sized iteration ----------------
        public int input_extension_count => module.InputExtensionCount;
        public int output_extension_count => module.OutputExtensionCount;
        public int display_extension_count => module.DisplayExtensionCount;
        public int effective_input_count => module.EffectiveInputCount;
        public int effective_output_count => module.EffectiveOutputCount;
        public string effective_input_id(int idx) => module.EffectiveInputId(idx);
        public string effective_output_id(int idx) => module.EffectiveOutputId(idx);
    }
}
