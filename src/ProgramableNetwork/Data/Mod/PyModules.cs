using Mafi;
using Mafi.Core.Mods;
using ProgramableNetwork.Python;
using System.IO;

namespace ProgramableNetwork.Data.Mod
{
    internal class PyModules : AValidatedData
    {
        protected override void RegisterDataInternal(ProtoRegistrator registrator)
        {
            DirectoryInfo modules = new DirectoryInfo(typeof(PyModules).Assembly.Location + "/../Modules");
            Log.Info("Location of modules: " + modules.FullName);

            foreach (FileInfo file in modules.EnumerateFiles())
            {
                ModuleRegistrator.Register(registrator, file.FullName);
            }
        }
    }
}
