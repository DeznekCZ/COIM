using Mafi;
using Mafi.Core.Mods;
using ProgramableNetwork.Python;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProgramableNetwork.Data.Mod
{
    public class PyModules : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            string path = $@"{registrator.ActiveMod.Manifest.RootDirectoryPath}\Modules";
            DirectoryInfo modules = new DirectoryInfo(path);
            //DirectoryInfo modules = new DirectoryInfo(typeof(PyModules).Assembly.Location + "/../Modules");
            Log.Info("Location of modules: " + modules.FullName);

            List<Class> allTemplates = new List<Class>();

            ControllerTemplates.Clear();

            StringBuilder logBuilder = new();

            int failed = 0;
            foreach (FileInfo file in modules.EnumerateFiles())
            {
                try
                {
                    ModuleRegistrator.Register(registrator, file.FullName, out var templates, out var controllers);
                    allTemplates.AddRange(templates);
                    ControllerTemplates.AddControllers(controllers);
                }
                catch (System.Exception e)
                {
                    Log.Error("Parsing of python module failed: " + file.Name);
                    Log.Exception(e);
					logBuilder.AppendLine("Parsing of python module failed: " + file.Name);
                    logBuilder.AppendLine(e.Message);
					logBuilder.AppendLine(e.StackTrace);
                    failed++;
                }
            }

            TemplateRegistrator.ClearTemplates();
            foreach (Class template in allTemplates)
            {
                try
                {
                    TemplateRegistrator.Register(registrator, template);
                }
                catch (System.Exception e)
                {
                    Log.Error("Parsing of python template failed: " + template.name);
                    Log.Exception(e);
					logBuilder.AppendLine("Parsing of python module failed: " + template.name);
					logBuilder.AppendLine(e.Message);
					logBuilder.AppendLine(e.StackTrace);
                    failed++;
                }
            }

            if (failed > 0)
            {
                throw new CheckException("Modules was not loaded, see log: " + failed + '\n' + logBuilder);
            }
        }
    }
}
