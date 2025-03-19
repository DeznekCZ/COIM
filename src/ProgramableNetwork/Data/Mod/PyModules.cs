using Mafi;
using Mafi.Core.Mods;
using ProgramableNetwork.Python;
using System.IO;

namespace ProgramableNetwork.Data.Mod
{
    internal class PyModules : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            DirectoryInfo modules = new DirectoryInfo(typeof(PyModules).Assembly.Location + "/../Modules");
            Log.Info("Location of modules: " + modules.FullName);

            int failed = 0;
            foreach (FileInfo file in modules.EnumerateFiles())
            {
                try
                {
                    ModuleRegistrator.Register(registrator, file.FullName);
                }
                catch (System.Exception e)
                {
                    Log.Error("Parsing of python module failed: " + file.Name);
                    Log.Exception(e);
                    failed++;
                }
            }
            if (failed > 0)
            {
                throw new CheckException("Modules was not loaded, see log: " + failed);
            }
        }
    }
}
