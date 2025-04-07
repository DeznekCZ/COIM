using Mafi.Core.Mods;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgramableNetwork
{
    public class ModuleIdsGenerator : IModData
    {
        public void RegisterData(ProtoRegistrator registrator)
        {
            string dir = Environment.GetEnvironmentVariable("APPDATA") + "/Captain of Industry/Mods/ProgramableNetwork/Modules/Core";
            string ids = dir + "/ids.py";

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# generated automatically from known prototypes");
            sb.AppendLine();
            sb.AppendLine("from Core.template import Template");
            sb.AppendLine();

            foreach (var item in registrator.PrototypesDb.All<ModuleProto>().OrderBy(m => m.Id))
            {
                string id = item.Id.Value.Replace("ProgramableNetwork_Module_", "");
                sb.AppendLine($"class {id}: pass");
                sb.AppendLine();
            }

            File.WriteAllText( ids, sb.ToString() );
        }
    }
}
