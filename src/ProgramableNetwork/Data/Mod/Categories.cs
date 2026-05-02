using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;

namespace ProgramableNetwork
{
    public class Category
    {
        public static ImmutableArray<Category> Categories(ProtosDb protos, Controller controller)
        {
            return protos.All<ModuleProto>()
                .Where(m => m.AllowedDevices.Contains(controller.Prototype.Id))
                .SelectMany(m => m.Categories.AsEnumerable())
                .Distinct()
                .OrderBy(m => m.Name.TranslatedString)
                .ToImmutableArray();
        }

        public Category(string id, LocStr name, params Category[] subcategories)
        {
            Id = id;
            Name = name;
            Subcategories = subcategories.ToImmutableArray();
        }

        public string Id { get; }
        public LocStr Name { get; }
        public ImmutableArray<Category> Subcategories { get; }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is Category cat && cat.Id == Id;
        }

        public override string ToString()
        {
            return $"Category(Id:{Id}, Name:{Name.TranslatedString})";
        }

        public static Dictionary<string, Category> Categories()
        {
            Dictionary<string, Category> categories = new Dictionary<string, Category>();

            System.Reflection.PropertyInfo[] properties = typeof(Category).GetProperties();
            foreach (System.Reflection.PropertyInfo item in properties)
            {
                if (item.PropertyType == typeof(Category))
                {
                    categories.Add(item.Name, (Category)item.GetValue(null));
                }
            }

            return categories;
        }

        public static bool operator ==(Category a, Category b) => a?.Id == b?.Id;
        public static bool operator !=(Category a, Category b) => a?.Id != b?.Id;

        // Translation keys are seeded off the C# property name (not Id) because some categories
        // share the same Id (Connection / ConnectionRead / ConnectionWrite all use "connection")
        // but want different display names.
        private static LocStr name(string propertyName, string enUs)
            => Loc.Str("ProgramableNetwork_Category_" + propertyName + "__name", enUs, "module category label shown in the new-module picker");

        //// known types
        public static Category Display { get; } = new Category(id: "display", name: name(nameof(Display), "Display modules"));
        public static Category ConnectionRead { get; } = new Category(id: "connection", name: name(nameof(ConnectionRead), "Connection modules (read)"));
        public static Category ConnectionWrite { get; } = new Category(id: "connection", name: name(nameof(ConnectionWrite), "Connection modules (write)"));
        public static Category Connection { get; } = new Category(id: "connection", name: name(nameof(Connection), "Connection modules"), ConnectionRead, ConnectionWrite);
        public static Category Arithmetic { get; } = new Category(id: "arithmetic", name: name(nameof(Arithmetic), "Arithmetic modules"));
        public static Category Constants { get; } = new Category(id: "constants", name: name(nameof(Constants), "Constant modules"), Arithmetic);
        public static Category Boolean { get; } = new Category(id: "boolean", name: name(nameof(Boolean), "Boolean modules"));
        public static Category Decision { get; } = new Category(id: "decision", name: name(nameof(Decision), "Decision modules"));
        public static Category Control { get; } = new Category(id: "control", name: name(nameof(Control), "Control modules"));
        public static Category Stats { get; } = new Category(id: "stats", name: name(nameof(Stats), "Stats modules"));
        public static Category AnteneFM { get; } = new Category(id: "antene_fm", name: name(nameof(AnteneFM), "FM modules"));
        public static Category AnteneAM { get; } = new Category(id: "antene_am", name: name(nameof(AnteneAM), "AM modules"));
        public static Category Antene { get; } = new Category(id: "antene", name: name(nameof(Antene), "Antena modules"), AnteneAM, AnteneFM);
        public static Category DevicesDisplay { get; } = new Category(id: "devices_display", name: name(nameof(DevicesDisplay), "Display devices"));
        public static Category DevicesSound { get; } = new Category(id: "devices_sound", name: name(nameof(DevicesSound), "Sound devices"));
        public static Category Devices { get; } = new Category(id: "devices_av", name: name(nameof(Devices), "Audiovisual devices"), DevicesDisplay, DevicesSound);

        // Player-saved blueprints surface in the picker under this category. Populated at
        // runtime by scanning the base-game BlueprintsLibrary for entries with the
        // [PN-Module]- title prefix; never registered on a real ModuleProto.
        public static Category Saved { get; } = new Category(id: "saved", name: name(nameof(Saved), "Saved blueprints"));
    }
}