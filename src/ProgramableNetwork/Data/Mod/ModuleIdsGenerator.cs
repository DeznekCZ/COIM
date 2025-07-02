using Mafi.Core.Mods;
using Mafi.Localization;
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
                sb.AppendLine($"class {id}:");
                sb.AppendLine($"    name = {Multiline(item.Strings.Name)}");
                sb.AppendLine($"    symbol = {Multiline(item.Symbol.AsLoc())}");
                sb.AppendLine($"    width = {item.BaseWidth}");
                sb.AppendLine($"    description = {Multiline(item.Strings.DescShort)}");
                sb.AppendLine($"    fields = [{string.Join(",", item.Fields.Select(f => $"(\"{f.Id}\", \"{f.Name}\", \"{f.GetType().Name}\")"))}]");
                sb.AppendLine($"    inputs = [{string.Join(",", item.Inputs.Select(f => $"(\"{f.Id}\", {Multiline(f.Name.Name)})"))}]");
                sb.AppendLine($"    outputs = [{string.Join(",", item.Outputs.Select(f => $"(\"{f.Id}\", {Multiline(f.Name.Name)})"))}]");
                sb.AppendLine();
            }

            File.WriteAllText( ids, sb.ToString() );
        }

        private string Multiline(LocStrFormatted name)
        {
            string text = name.Value;
            if (text.Contains('\n'))
                return $"\"\"\"{text}\"\"\"";
            else
                return $"\"{text}\"";;
        }
    }
}
