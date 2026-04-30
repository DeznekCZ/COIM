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

        // Slider display.  Width = number of module cells (1-4).  Defaults for min/max
        // are stored on the proto; the action() can override them at runtime by writing
        // to module.Display[id + "_min"] / module.Display[id + "_max"].  The current
        // value is module.Display[id].
        public static DisplayConstructorAction Slider(string id, string name, Fix32 width, float min = 0f, float max = 1f)
        {
            return moduleProto => moduleProto.AddDisplaySlider(id, name, width, min, max);
        }
    }

    public delegate void DisplayConstructorAction(ModuleProto.Builder moduleProto);
}