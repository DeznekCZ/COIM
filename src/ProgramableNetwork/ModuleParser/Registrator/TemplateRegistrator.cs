using Mafi;
using Mafi.Base;
using Mafi.Core.Mods;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProgramableNetwork;
using Mafi.Core.Prototypes;

namespace ProgramableNetwork.Python
{
    public class TemplateRegistrator
    {
        private static Dictionary<string, Template> templates;

        public static Dictionary<string, Template> GetTemplates() => templates ?? new Dictionary<string, Template>();

        // Module-picker subset — entries flagged with `picker = True` in
        // their Python class definition.  These are mode / preset variants
        // of a single ModuleProto that the picker shows alongside the
        // unconfigured module so the player can pick a configured shape
        // directly.  Other templates stay only in the dedicated template
        // picker.
        public static IEnumerable<KeyValuePair<string, Template>> GetModulePickerTemplates(Option<ModuleProto> proto)
        {
            if (templates == null)
            {
                yield break;
            }
            foreach (KeyValuePair<string, Template> kv in templates)
            {
                if (kv.Value.ShowInModulePicker
					&& (proto.IsNone || kv.Value.ModuleProto.Id.Equals(proto.Value.Id)))
                {
                    yield return kv;
                }
            }
        }

        public static void Register(ProtoRegistrator registrator, Class templateEntry)
        {
            if (templateEntry.name == "Favorites")
            {
                foreach (var item in templateEntry.baseTypes
                    .Where(t => !(t.IsAssignableTo<Template>())))
                {
                    try
                    {
                        string favoriteName = "favorite_" + DateTime.Now.Ticks;
                        Proto.ID moduleId = new Proto.ID(item.Name.ModuleId());
                        ModuleProto moduleProto = registrator.PrototypesDb.Get<ModuleProto>(moduleId).ValueOrThrow("missing module");
                        templates.Add(favoriteName, new Template(null, moduleProto, m => { }));
                    }
                    catch (Exception e)
                    {
                        Log.Error("Falied to load type of module for template: " + item.Name);
                        Log.Exception(e);
                    }
                }
                return;
            }

            var displayName = templateEntry.classContext["name"] as string;

            if (templateEntry.classContext.TryGetValue("settings", out object action) ||
                templateEntry.classContext.TryGetValue("Settings", out action))
            {
                Proto.ID moduleId = new Proto.ID(templateEntry.baseTypes[1].Name.ModuleId());
                ModuleProto moduleProto = registrator.PrototypesDb.Get<ModuleProto>(moduleId).ValueOrThrow("missing module");

                string templateName = templateEntry.name;
                if (templates.ContainsKey(templateName))
                {
                    templateName = templateName + "_" + DateTime.Now.Ticks;
                }

                // `picker = True` at class scope opts the template into the
                // regular module picker (alongside the unconfigured module),
                // not just the dedicated template picker.  Use it for mode
                // variants of one ModuleProto so the player can pick the
                // configured shape directly.  Anything truthy counts; the
                // attribute is optional and defaults to false.
                bool showInModulePicker = false;
                if (templateEntry.classContext.TryGetValue("picker", out object pickerFlag) && pickerFlag != null)
                {
                    showInModulePicker = pickerFlag is bool b ? b
                                       : pickerFlag is int i ? i != 0
                                       : false;
                }

                templates[templateName] = new Template(displayName, moduleProto, (module) =>
                {
                    ModuleWrapper wrapper = new ModuleWrapper(module, templateEntry);
                    (action as Method).Self = wrapper;
                    Expressions.__call__(action, new List<(string name, object value)>());
                }, showInModulePicker);
            }
        }

        internal static void AddControllers(List<Class> allControllers)
        {
            throw new NotImplementedException();
        }

        internal static void ClearTemplates()
        {
            templates = new Dictionary<string, Template>();
        }
    }
}
