using System.Collections.Generic;
using Mafi;

// Lightweight stand-ins for the handful of mod types the source-linked interpreter references by
// name. They live in namespace `ProgramableNetwork` so the parser's `ProgramableNetwork.Python`
// files resolve them by walking up the namespace (exactly as they do against the real mod types).
// Only the surface the interpreter actually touches is reproduced.
namespace ProgramableNetwork
{
    /// <summary>Base-class marker. The interpreter only checks <c>baseTypes.Contains(typeof(Module))</c>.</summary>
    public class Module { }

    /// <summary>Template/controller-template markers (base-type checks only).</summary>
    public class Template { }

    /// <summary>Controller marker — referenced by the linked <c>ControllerTemplate</c> delegate.</summary>
    public class Controller { }

    /// <summary>Module execution status — values copied verbatim from the mod's enum.</summary>
    public enum ModuleStatus
    {
        Init = 0, Running, Iddle, Error, Paused, Skipped
    }

    /// <summary>
    /// Python <c>controllers = [ DefaultControllers.Controller ]</c> resolves
    /// <c>DefaultControllers</c> to <c>typeof(NewIds.Controllers)</c> and reads a static member off it.
    /// </summary>
    public static class NewIds
    {
        public static class Controllers
        {
            public static readonly object Controller = "Controller";
        }
    }

    /// <summary>
    /// Stand-in for the mod's category registry. Python reads <c>DefaultCategories.&lt;Name&gt;</c>
    /// via reflection, so every category the Custom modules can reference is exposed as a static
    /// property. <see cref="Id"/>/<see cref="Name"/> are enough for the web picker to group modules.
    /// </summary>
    public sealed class Category
    {
        public string Id { get; }
        public string Name { get; }
        public Category[] Subcategories { get; }

        public Category(string id, string name, params Category[] subcategories)
        {
            Id = id;
            Name = name;
            Subcategories = subcategories ?? new Category[0];
        }

        public static bool operator ==(Category a, Category b) => a?.Id == b?.Id;
        public static bool operator !=(Category a, Category b) => a?.Id != b?.Id;
        public override bool Equals(object obj) => obj is Category c && c.Id == Id;
        public override int GetHashCode() => Id?.GetHashCode() ?? 0;
        public override string ToString() => $"Category({Id})";

        public static Category Display { get; } = new Category("display", "Display modules");
        public static Category ConnectionRead { get; } = new Category("connection", "Connection modules (read)");
        public static Category ConnectionWrite { get; } = new Category("connection", "Connection modules (write)");
        public static Category Connection { get; } = new Category("connection", "Connection modules", ConnectionRead, ConnectionWrite);
        public static Category Arithmetic { get; } = new Category("arithmetic", "Arithmetic modules");
        public static Category Constants { get; } = new Category("constants", "Constant modules", Arithmetic);
        public static Category Boolean { get; } = new Category("boolean", "Boolean modules");
        public static Category Decision { get; } = new Category("decision", "Decision modules");
        public static Category Control { get; } = new Category("control", "Control modules");
        public static Category Stats { get; } = new Category("stats", "Stats modules");
        public static Category AnteneFM { get; } = new Category("antene_fm", "FM modules");
        public static Category AnteneAM { get; } = new Category("antene_am", "AM modules");
        public static Category Antene { get; } = new Category("antene", "Antena modules", AnteneAM, AnteneFM);
        public static Category DevicesDisplay { get; } = new Category("devices_display", "Display devices");
        public static Category DevicesSound { get; } = new Category("devices_sound", "Sound devices");
        public static Category Devices { get; } = new Category("devices_av", "Audiovisual devices", DevicesDisplay, DevicesSound);
        public static Category Saved { get; } = new Category("saved", "Saved blueprints");
    }

    /// <summary>
    /// Recording stand-in for <c>ModuleProto.Builder</c>. The linked <c>DisplayConstructor</c>
    /// produces lambdas that call these methods; the sim loader runs each lambda against a fresh
    /// builder and reads back <see cref="Displays"/> to recover the display metadata. Reusing the
    /// real DisplayConstructor keeps the LED/Text/Icon/Slider semantics identical.
    /// </summary>
    public sealed class ModuleProto
    {
        public enum DisplayKind { Text, Led, Icon, Slider, Filler }

        public sealed class DisplayDef
        {
            public string Id;
            public string Name;
            public int Width;
            public DisplayKind Kind;
            public string DefaultText;
            public float Min;
            public float Max;
        }

        public sealed class Builder
        {
            public readonly List<DisplayDef> Displays = new List<DisplayDef>();

            public Builder AddDisplay(string id, string name, int width, bool led = false, string defaultText = "")
            {
                Displays.Add(new DisplayDef
                {
                    Id = id,
                    Name = name,
                    Width = width,
                    Kind = led ? DisplayKind.Led
                         : (defaultText != null && defaultText.StartsWith("[image]")) ? DisplayKind.Icon
                         : DisplayKind.Text,
                    DefaultText = defaultText ?? ""
                });
                return this;
            }

            public Builder AddDisplayFiller(Fix32 width)
            {
                Displays.Add(new DisplayDef { Id = "", Name = "", Width = width.IntegerPart, Kind = DisplayKind.Filler });
                return this;
            }

            public Builder AddDisplaySlider(string id, string name, Fix32 width, float min, float max)
            {
                Displays.Add(new DisplayDef
                {
                    Id = id,
                    Name = name,
                    Width = width.IntegerPart,
                    Kind = DisplayKind.Slider,
                    Min = min,
                    Max = max
                });
                return this;
            }
        }
    }
}
