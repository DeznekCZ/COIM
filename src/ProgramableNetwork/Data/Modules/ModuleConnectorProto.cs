using Mafi;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;

namespace ProgramableNetwork
{
    public class ModuleConnectorProto
    {
        public readonly string Id;
        public readonly Proto.Str Name;
        public readonly Fix32 Width;
        public readonly string DefaultText;

        public ModuleConnectorProto(string id, Proto.Str str, Fix32 width, string defaultText = "")
        {
            Id = id;
            Name = str;
            Width = width;
            DefaultText = defaultText;
        }
    }
}