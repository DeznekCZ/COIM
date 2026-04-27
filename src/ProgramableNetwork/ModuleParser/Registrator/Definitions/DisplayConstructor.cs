using Mafi;

namespace ProgramableNetwork.ModuleParser.Registrator.Definitions
{
    public class DisplayConstructor
    {
        public static DisplayConstructorAction Filler(Fix32 width)
        {
            return moduleProto => moduleProto.AddDisplayFiller(width);
        }

        public static DisplayConstructorAction LED(string id, string name)
        {
            return moduleProto => moduleProto.AddDisplay(id, name, 1, led: true);
        }

        public static DisplayConstructorAction Text(string id, string name, string defaultText)
        {
            return moduleProto => moduleProto.AddDisplay(id, name, 1);
        }

        public static DisplayConstructorAction Icon(string id, string name, string defaultText = "")
        {
            return moduleProto => moduleProto.AddDisplay(id, name, 1, defaultText: "[image]" + defaultText);
        }
    }

    public delegate void DisplayConstructorAction(ModuleProto.Builder moduleProto);
}