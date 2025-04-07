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

        public static Dictionary<string, Template> GetTemplates() => templates;

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

                templates[templateName] = new Template(displayName, moduleProto, (module) =>
                {
                    ModuleWrapper wrapper = new ModuleWrapper(module, templateEntry);
                    (action as Method).Self = wrapper;
                    Expressions.__call__(action, new List<(string name, object value)>());
                });
            }
        }

        internal static void ClearTemplates()
        {
            templates = new Dictionary<string, Template>();
        }
    }
}
