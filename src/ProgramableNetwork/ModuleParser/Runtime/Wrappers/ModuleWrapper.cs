using Mafi;
using Mafi.Collections;
using Mafi.Core.Entities;
using System;

namespace ProgramableNetwork.Python
{
    public class ModuleWrapper
    {
        public readonly Module module;
        public readonly Class @class;

        public ModuleWrapper(Module module, Class @class = null)
        {
            this.module = module;
            this.@class = @class;
        }

        public FieldOrInputSetter FieldOrInput => new FieldOrInputSetter(module);

        public class FieldOrInputSetter
        {
            private Module module;

            public FieldOrInputSetter(Module module)
            {
                this.module = module;
            }

            public Entity get_ent(string fieldName)
            {
                return module.FieldOrInput.Entity<Entity>(fieldName);
            }

            public bool get_bool(string name, bool value)
            {
                return module.FieldOrInput.Bool[name, value];
            }

            public int get_int(string name, int value)
            {
                return module.FieldOrInput.Integer[name, value];
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.FieldOrInput[name, value];
            }

            // Dotted accessor — read-only side of the FieldOrInput coalescer.
            // Mirrors the existing `get` shape so `self.FieldOrInput.A` is
            // equivalent to `self.FieldOrInput.get("A", 0)`.
            public Fix32 __getattr__(string name)
            {
                return module.FieldOrInput[name, Fix32.Zero];
            }
        }

        public FieldSetter Field => new FieldSetter(module);

        public class FieldSetter
        {
            private Module module;

            public FieldSetter(Module module)
            {
                this.module = module;
            }

            public Entity get_ent(string fieldName)
            {
                return module.Field.Entity<Entity>(fieldName);
            }

            public bool get_bool(string name, bool value)
            {
                return module.Field.Bool[name, value];
            }

            public void set_bool(string name, bool value)
            {
                module.Field.Bool[name] = value;
            }

            public int get_int(string name, int value)
            {
                return module.Field.Integer[name, value];
            }

            public void set_int(string name, int value)
            {
                module.Field.Integer[name] = value;
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Field[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Field[name] = value;
            }

            // Dotted accessor — see InputSetter.__getattr__/__setattr__.
            public Fix32 __getattr__(string name)
            {
                return module.Field[name, Fix32.Zero];
            }

            public void __setattr__(string name, object value)
            {
                if (value is bool b) { module.Field.Bool[name] = b; return; }
                if (value is int i) { module.Field.Integer[name] = i; return; }
                if (value is Fix32 f) { module.Field[name] = f; return; }
                if (value is float fl) { module.Field[name] = fl.ToFix32(); return; }
                throw new System.InvalidOperationException(
                    "Cannot assign " + (value?.GetType().Name ?? "null") + " to field '" + name + "'");
            }
        }

        public InputSetter Input => new InputSetter(module);

        public class InputSetter
        {
            private Module module;

            public InputSetter(Module module)
            {
                this.module = module;
            }

            public bool get_bool(string name, bool value)
            {
                return module.Input.Bool[name, value];
            }

            public void set_bool(string name, bool value)
            {
                module.Input.Bool[name] = value;
            }

            public int get_int(string name, int value)
            {
                return module.Input.Integer[name, value];
            }

            public void set_int(string name, int value)
            {
                module.Input.Integer[name] = value;
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Input[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Input[name] = value;
            }

            // Python-style fallback hooks invoked by PropertyExpression when
            // normal reflection finds no member with the given name.  These
            // turn `self.Input.A` into `self.Input.get("A", 0)` and
            // `self.Input.A = X` into a typed write.  Default-on-miss is
            // Fix32.Zero so unconnected inputs read as 0.
            public Fix32 __getattr__(string name)
            {
                return module.Input[name, Fix32.Zero];
            }

            public void __setattr__(string name, object value)
            {
                if (value is bool b) { module.Input.Bool[name] = b; return; }
                if (value is int i) { module.Input.Integer[name] = i; return; }
                if (value is Fix32 f) { module.Input[name] = f; return; }
                if (value is float fl) { module.Input[name] = fl.ToFix32(); return; }
                throw new System.InvalidOperationException(
                    "Cannot assign " + (value?.GetType().Name ?? "null") + " to input '" + name + "'");
            }
        }

        public OutputSetter Output => new OutputSetter(module);

        public class OutputSetter
        {
            private Module module;

            public OutputSetter(Module module)
            {
                this.module = module;
            }

            public bool get_bool(string name, bool value)
            {
                return module.Output.Bool[name];
            }

            public void set_bool(string name, bool value)
            {
                module.Output.Bool[name] = value;
            }
            

            public int get_int(string name, int value)
            {
                return module.Output.Integer[name];
            }

            public void set_int(string name, int value)
            {
                module.Output.Integer[name] = value;
            }

            public Fix32 get(string name, Fix32 value)
            {
                return module.Output[name, value];
            }

            public void set(string name, Fix32 value)
            {
                module.Output[name] = value;
            }

            // Dotted accessor — see InputSetter.__getattr__/__setattr__.
            public Fix32 __getattr__(string name)
            {
                return module.Output[name, Fix32.Zero];
            }

            public void __setattr__(string name, object value)
            {
                if (value is bool b) { module.Output.Bool[name] = b; return; }
                if (value is int i) { module.Output.Integer[name] = i; return; }
                if (value is Fix32 f) { module.Output[name] = f; return; }
                if (value is float fl) { module.Output[name] = fl.ToFix32(); return; }
                throw new System.InvalidOperationException(
                    "Cannot assign " + (value?.GetType().Name ?? "null") + " to output '" + name + "'");
            }
        }

        public DisplaySetter Display => new DisplaySetter(module);

        public class DisplaySetter
        {
            private Module module;

            public DisplaySetter(Module module)
            {
                this.module = module;
            }

            public string get(string name, string value)
            {
                return module.Display[name, value];
            }

            public void set(string name, string value)
            {
                module.Display[name] = value;
            }

            // Dotted accessor — Display values are strings, so we coerce any
            // assigned value via Expressions.__str__ (matches what the
            // implicit "" + value path would do at parse time).
            public string __getattr__(string name)
            {
                return module.Display[name, ""];
            }

            public void __setattr__(string name, object value)
            {
                module.Display[name] = Expressions.__str__(value);
            }
        }

        // Per-module Fix32 scratch array.  Exposed as `module.Array` in Python.
        public ArraySetter Array => new ArraySetter(module);

        public class ArraySetter
        {
            private readonly Module module;

            public ArraySetter(Module module)
            {
                this.module = module;
            }

            public int length => module.Array.Length;

            public Fix32 get(int idx, Fix32 defaultValue)
            {
                return module.Array[idx, defaultValue];
            }

            public void set(int idx, Fix32 value)
            {
                module.Array[idx] = value;
            }

            public void resize(int size)
            {
                module.Array.Resize(size);
            }

            // Python overload — when growing, seed the new tail slots with the
            // supplied value instead of zero.  Lets Python modules avoid a loop
            // when extending a delay buffer / window with a continuation value.
            public void resize(int size, Fix32 fill_new)
            {
                module.Array.Resize(size, fill_new);
            }

            public void clear()
            {
                module.Array.Clear();
            }

            // Atomic shift-register step: shifts every slot one position left,
            // writes `incoming` to the last slot, and returns the value that
            // was previously in slot[0].  Lets Python modules implement an
            // array-copy delay / FIR / sliding window without a per-tick loop.
            public Fix32 shift_left_with(Fix32 incoming)
            {
                return module.Array.ShiftLeftWith(incoming);
            }
        }

        public ModuleStatus Status
        {
            get => module.Status;
            set => module.SetStatus(value);
        }

        public string Error
        {
            get => module.Error;
            set => module.SetError(value);
        }

        public bool Info
        {
            get => module.Info;
            set => module.Info = value;
        }

        public bool Warning
        {
            get => module.Warning;
            set => module.Warning = value;
        }

        // Per-module persistent state.  Wrapped so Python can use both
        // dict-style (`self.NumberData["foo"]`) and attribute-style
        // (`self.NumberData.foo`) access — both go through the same backing
        // Dict<string,*> on the underlying Module, so they round-trip the
        // same data and are safe to mix in the same script.
        public NumberDataSetter NumberData => new NumberDataSetter(module);
        public StringDataSetter StringData => new StringDataSetter(module);

        public class NumberDataSetter
        {
            private Module module;
            public NumberDataSetter(Module module) { this.module = module; }

            public int __getattr__(string name) => __getitem__(name);
            public void __setattr__(string name, object value) => __setitem__(name, value);

            public int __getitem__(object key)
            {
                string name = key as string;
                if (name == null) {
                    return 0;
                }
                return module.NumberData.TryGetValue(name, out int v) ? v : 0;
            }

            public void __setitem__(object key, object value)
            {
                string name = key as string;
                if (name == null) {
                    return;
                }
                if (value is bool b) { module.NumberData[name] = b ? 1 : 0; return; }
                if (value is int i) { module.NumberData[name] = i; return; }
                if (value is Fix32 f) { module.NumberData[name] = f.IntegerPart; return; }
                if (value is float fl) { module.NumberData[name] = (int)fl; return; }
                throw new System.InvalidOperationException(
                    "Cannot store " + (value?.GetType().Name ?? "null") + " in NumberData['" + name + "']");
            }
        }

        public class StringDataSetter
        {
            private Module module;
            public StringDataSetter(Module module) { this.module = module; }

            public string __getattr__(string name) => __getitem__(name);
            public void __setattr__(string name, object value) => __setitem__(name, value);

            public string __getitem__(object key)
            {
                string name = key as string;
                if (name == null) {
                    return "";
                }
                return module.StringData.TryGetValue(name, out string v) ? v : "";
            }

            public void __setitem__(object key, object value)
            {
                string name = key as string;
                if (name == null) {
                    return;
                }
                module.StringData[name] = Expressions.__str__(value);
            }
        }
        public ModuleProto Prototype => module.Prototype;
        public Controller Controller => module.Controller;

        public void __setitem__(object key, object value)
        {
            if (key is string id && value is ModuleConnector connector)
            {
                module.InputModules[id] = connector;
            }
        }

        public object __getitem__(object key)
        {
            if (key is string id)
            {
                return new ModuleConnector(module.Id, id);
            }
            throw new NotImplementedException("Invalid indexer for ModuleWrapper");
        }
    }
}