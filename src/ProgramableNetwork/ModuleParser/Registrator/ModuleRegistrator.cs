using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using ProgramableNetwork.ModuleParser.Registrator.Definitions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ProgramableNetwork.Python
{
    public class ModuleRegistrator
    {
        public static void Register(ProtoRegistrator registrator, string file, out List<Class> templates, out List<Class> controllers)
        {
            Token[] tokens = Tokenizer.ParseFile(file);
            Block block = Lexer.Parse(tokens);

            Dictionary<string, object> context = new Dictionary<string, object>();
            foreach (IStatement statement in block.statements)
            {
                statement.Execute(context);
            }

            foreach (Class classEntry in context.Values
                .Where(v => v is Class c && c.baseTypes.Contains(typeof(Module))))
            {
                var builder = registrator.ModuleBuilderStart(classEntry.name);
                builder.SetName(classEntry.classContext["name"] as string);
                builder.SetSymbol(classEntry.classContext["symbol"] as string);

                if (classEntry.classContext.TryGetValue("description", out object description)) {
					builder.SetDescription(description as string);
				}
				if (classEntry.classContext.TryGetValue("hint", out object hint)) {
					builder.SetHint(hint as string);
				}
				if (classEntry.classContext.TryGetValue("inputs", out object inputs)) {
					AddIO(inputs as IList, builder.AddInput, builder.AddInput);
				}
				if (classEntry.classContext.TryGetValue("outputs", out object outputs)) {
					AddIO(outputs as IList, builder.AddOutput, builder.AddOutput);
				}
				if (classEntry.classContext.TryGetValue("displays", out object displays)) {
					AddIO(displays as IList, builder.AddDisplayFromPython);
				}
				if (classEntry.classContext.TryGetValue("fields", out object fields)) {
					AddFields(builder, fields as IList);
				}
				if (classEntry.classContext.TryGetValue("Init", out object initAction)) {
					AddInit(builder, classEntry, initAction as Method);
				}
				if (classEntry.classContext.TryGetValue("action", out object action) ||
					classEntry.classContext.TryGetValue("Action", out action)) {
					AddAction(builder, classEntry, action as Method);
				}
				if (classEntry.classContext.TryGetValue("display", out object display) ||
					classEntry.classContext.TryGetValue("Display", out display)) {
					AddDisplay(builder, classEntry, display as Method);
				}
				if (classEntry.classContext.TryGetValue("categories", out object categories)) {
					AddCategories(builder, categories as List<object>);
				}
				if (classEntry.classContext.TryGetValue("width", out object width)) {
					builder.Width(Expressions.__int__(width));
				}
				// Extension declarations — int values on the class control how far the
				// player can grow the module on each side via the inspector edge buttons.
				// 0 (or absent) = not extensible.  Naming defaults to the alphabet
				// generator (single-char ids like "B" → "C", "D" or "1" → "2", "3"); for
				// modules with multi-char pin ids (e.g. "in_1", "out_1") provide an
				// explicit list via `input_extension_names` / `output_extension_names`.
				IList inExtNames = classEntry.classContext.TryGetValue("input_extension_names", out object inNames)
					? inNames as IList : null;
				IList outExtNames = classEntry.classContext.TryGetValue("output_extension_names", out object outNames)
					? outNames as IList : null;
				// Class-level opt-in: route every extension pin's display label through
				// the shared registry instead of minting a per-module key. Requires an
				// explicit *_extension_names list — no shared variant of the alphabet
				// auto-namer (the auto-named ids would themselves be unique per module
				// and there'd be no point sharing them).
				bool inExtShared = classEntry.classContext.TryGetValue("input_extensions_shared", out object inSharedRaw)
								   && inSharedRaw != null && Expressions.__bool__(inSharedRaw);
				bool outExtShared = classEntry.classContext.TryGetValue("output_extensions_shared", out object outSharedRaw)
									&& outSharedRaw != null && Expressions.__bool__(outSharedRaw);

				if (classEntry.classContext.TryGetValue("input_extensions", out object inExt)) {
					int n = Expressions.__int__(inExt);
					if (inExtShared && inExtNames != null && inExtNames.Count >= n) {
						builder.AllowInputExtensionsShared(n, idx => ExtSharedNameAt(inExtNames, idx));
					} else if (inExtNames != null && inExtNames.Count >= n) {
						builder.AllowInputExtensions(n, idx => ExtNameAt(inExtNames, idx));
					} else {
						builder.AllowInputExtensions(n);
					}
				}
				if (classEntry.classContext.TryGetValue("output_extensions", out object outExt)) {
					int n = Expressions.__int__(outExt);
					if (outExtShared && outExtNames != null && outExtNames.Count >= n) {
						builder.AllowOutputExtensionsShared(n, idx => ExtSharedNameAt(outExtNames, idx));
					} else if (outExtNames != null && outExtNames.Count >= n) {
						builder.AllowOutputExtensions(n, idx => ExtNameAt(outExtNames, idx));
					} else {
						builder.AllowOutputExtensions(n);
					}
				}
				if (classEntry.classContext.TryGetValue("display_extensions", out object dispExt)) {
					builder.AllowDisplayExtensions(Expressions.__int__(dispExt));
				}

				// `link_input_output_extensions` — when truthy, growing/shrinking either
				// pin side's extension count is mirrored to the other side too.  Used by
				// paired-channel modules like flip-flop where every in_N must always
				// have its matching out_N.  Caller is expected to declare the same
				// `input_extensions` and `output_extensions` cap so the linked sides
				// can keep up without clamping.
				if (classEntry.classContext.TryGetValue("link_input_output_extensions", out object linkIoRaw)
					&& linkIoRaw != null
					&& Expressions.__bool__(linkIoRaw))
				{
					builder.LinkInputOutputExtensions();
				}

				// Per-extension display widgets — list of Display constructors that
				// materialise alongside their matching pin extension on the linked side.
				// E.g. flip-flop: each new output channel gets a paired LED display.
				// `extension_displays_link` chooses which pin side they follow ("input" or
				// "output", default "output").
				if (classEntry.classContext.TryGetValue("extension_displays", out object extDispRaw))
				{
					IList extDispList = extDispRaw as IList;
					if (extDispList != null && extDispList.Count > 0)
					{
						ExtensionSide linkedSide = ExtensionSide.Output;
						if (classEntry.classContext.TryGetValue("extension_displays_link", out object linkRaw))
						{
							string linkStr = (linkRaw as string)?.ToLowerInvariant() ?? "output";
							if (linkStr == "input") {
								linkedSide = ExtensionSide.Input;
							}
						}
						List<DisplayConstructorAction> actions = new List<DisplayConstructorAction>();
						foreach (object entry in extDispList)
						{
							if (entry is DisplayConstructorAction displayConstructorAction) {
								actions.Add(displayConstructorAction);
							}
						}
						builder.AllowExtensionDisplays(linkedSide, actions);
					}
				}

				// TODO search for variable of device
                builder.AddControllerDevice();

                builder.BuildAndAdd();

                // `deprecates` — list of fixed-arity ids this extensible module
                // supersedes.  Each entry is a tuple/list:
                //   ("OldModuleId", input_ext, output_ext, display_ext)
                // Trailing values may be 0/None when not applicable.  The string id
                // gets the same `.ModuleId()` mangling new modules go through.
                if (classEntry.classContext.TryGetValue("deprecates", out object dep))
                {
                    RegisterDeprecates(classEntry.name, dep as IList);
                }
            }

            templates = context.Values
                .Where(v => v is Class c && c.baseTypes.Contains(typeof(Template)))
                .Cast<Class>()
                .ToList();

            controllers = context.Values
                .Where(v => v is Class c && c.baseTypes.Contains(typeof(ControllerTemplate)))
                .Cast<Class>()
                .ToList();
        }

        private static void AddIO(
            IList modules,
            Func<string, string, ModuleProto.Builder> addPerModule,
            Func<string, SharedLabel, ModuleProto.Builder> addShared)
        {
            foreach (ModuleConnectorProtoDefinition variable in modules ?? new List<ModuleConnectorProtoDefinition>())
            {
                if (variable.shared) {
                    addShared(variable.id, variable.name.Shared());
                } else {
                    addPerModule(variable.id, variable.name);
                }
            }
        }

        private static void AddIO(IList modules, Func<DisplayConstructorAction, ModuleProto.Builder> add)
        {
            foreach (DisplayConstructorAction variable in modules ?? new List<DisplayConstructorAction>())
            {
                add(variable);
            }
        }

        private static void AddFields(ModuleProto.Builder builder, IList list)
        {
            foreach (IModuleFieldProtoDefinition variable in list)
            {
                if (variable is ModuleEntityFieldProtoDefinition entityField)
                {
                    builder.AddEntityField(entityField.type, entityField.id, entityField.name, entityField.desc, showInTooltip: entityField.showInTooltip);
                    continue;
                }

                if (variable is ModuleInt32FieldProtoDefinition int32Field)
                {
                    builder.AddInt32Field(int32Field.id, int32Field.name, int32Field.desc, int32Field.defaultValue, int32Field.overrideInput, int32Field.showInTooltip);
                    continue;
                }

                if (variable is ModuleInt64FieldProtoDefinition int64Field)
                {
                    builder.AddInt64Field(int64Field.id, int64Field.name, int64Field.desc, int64Field.defaultValue, int64Field.overrideInput, int64Field.showInTooltip);
                    continue;
                }

                if (variable is ModuleFix32FieldProtoDefinition fix32Field)
                {
                    builder.AddFix32Field(fix32Field.id, fix32Field.name, fix32Field.desc, fix32Field.defaultValue, fix32Field.overrideInput, fix32Field.showInTooltip);
                    continue;
                }

                if (variable is ModuleStringFieldProtoDefinition stringField)
                {
                    builder.AddStringField(stringField.id, stringField.name, stringField.desc, stringField.defaultValue, stringField.overrideInput, stringField.multilined, stringField.showInTooltip);
                    continue;
                }

                if (variable is ModuleBooleanFieldProtoDefinition booleanField)
                {
                    builder.AddBooleanField(booleanField.id, booleanField.name, booleanField.desc, booleanField.defaultValue, booleanField.overrideInput, booleanField.showInTooltip);
                    continue;
                }
            }
        }

        private static void AddAction(ModuleProto.Builder builder, Class classContext, Method action)
        {
            builder.Action((module) =>
            {
                ModuleWrapper wrapper = new ModuleWrapper(module, classContext);
                action.Self = wrapper;
                object ret = Expressions.__call__(action, new List<(string name, object value)>());
                return ret is ModuleStatus status ? status : ModuleStatus.Running;
            });
        }

        private static void AddDisplay(ModuleProto.Builder builder, Class classContext, Method action)
        {
            builder.Display((module) =>
            {
                ModuleWrapper wrapper = new ModuleWrapper(module, classContext);
                action.Self = wrapper;
                Expressions.__call__(action, new List<(string name, object value)>());
            });
        }

        private static void AddInit(ModuleProto.Builder builder, Class classContext, Method action)
        {
            builder.Init((module) =>
            {
                ModuleWrapper wrapper = new ModuleWrapper(module, classContext);
                action.Self = wrapper;
                object ret = Expressions.__call__(action, new List<(string name, object value)>());
                return ret is ModuleStatus status ? status : ModuleStatus.Running;
            });
        }

        private static void AddCategories(ModuleProto.Builder builder, List<object> list)
        {
            foreach (object item in list)
            {
                builder.AddCategory(item as Category);
            }
        }

        // Reads each entry from the Python `deprecates = [ ("OldId", inExt, outExt, dispExt), ... ]`
        // table and pushes a Deprecation.RegisterDeprecation call for it.  Tuples/lists
        // are accepted; missing trailing items default to null (no extension override).
        private static void RegisterDeprecates(string newClassName, IList list)
        {
            if (list == null) {
                return;
            }
            foreach (object entry in list)
            {
                IList tuple = entry as IList;
                if (tuple == null || tuple.Count == 0) {
                    continue;
                }
                string oldId = tuple[0] as string;
                if (string.IsNullOrEmpty(oldId)) {
                    continue;
                }
                int? inExt = (tuple.Count > 1) ? ToNullableInt(tuple[1]) : null;
                int? outExt = (tuple.Count > 2) ? ToNullableInt(tuple[2]) : null;
                int? dispExt = (tuple.Count > 3) ? ToNullableInt(tuple[3]) : null;
                Deprecation.RegisterDeprecation(
                    new ModuleProto.ID(oldId.ModuleId()),
                    new ModuleProto.ID(newClassName.ModuleId()),
                    inputExt: inExt, outputExt: outExt, displayExt: dispExt);
            }
        }

        private static int? ToNullableInt(object value)
        {
            if (value == null) {
                return null;
            }
            try { return Expressions.__int__(value); }
            catch { return null; }
        }

        // Pulls the extension id+display-name from a Python `input_extension_names` /
        // `output_extension_names` list.  Each entry can be either a plain string
        // (id == display name) or a 2-tuple (id, display).  Out-of-range index falls
        // back to a numeric placeholder so the proto registration doesn't blow up.
        private static (string id, string name) ExtNameAt(IList list, int idx)
        {
            if (idx < 0 || idx >= list.Count) {
                string fallback = "ext_" + idx;
                return (fallback, fallback);
            }
            object entry = list[idx];
            if (entry is IList tuple && tuple.Count >= 2) {
                string id = tuple[0] as string ?? "";
                string display = tuple[1] as string ?? id;
                return (id, display);
            }
            string s = entry as string ?? ("ext_" + idx);
            return (s, s);
        }

        // Same as ExtNameAt but wraps the display label in a SharedLabel so the
        // Builder routes it through SharedFieldLabels.Resolve. Used when the module
        // class declares `input_extensions_shared = True` (or the output flavor).
        private static (string id, SharedLabel shared) ExtSharedNameAt(IList list, int idx)
        {
            var (id, name) = ExtNameAt(list, idx);
            return (id, name.Shared());
        }
    }
}
