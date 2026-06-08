using System.Collections.Generic;
using Mafi;

namespace ProgramableNetwork.Runtime.Sim
{
    /// <summary>
    /// Browser stand-in for a COI game object that a module's entity field would normally point at.
    /// The web app has no game world, so entity fields resolve to one of these instead. A module can
    /// read arbitrary named values off it via the interpreter's reflection/<c>__getattr__</c> path;
    /// the user populates the values from the UI. <see cref="Null"/> is a null-object so modules that
    /// touch an unset entity field read defaults instead of throwing.
    /// </summary>
    public sealed class MockEntity
    {
        /// <summary>Shared null-object for unset entity fields.</summary>
        public static readonly MockEntity Null = new MockEntity("", "(none)", "");

        public long Id { get; set; }
        public string Name { get; set; }
        public string EntityType { get; set; }

        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public MockEntity(string entityType = "", string name = "", string idTag = "")
        {
            EntityType = entityType;
            Name = name;
        }

        public Fix32 GetFix(string key) => values.TryGetValue(key, out object v) && v is Fix32 f ? f : Fix32.Zero;
        public int GetInt(string key) => GetFix(key).IntegerPart;
        public bool GetBool(string key) => GetFix(key).IsNotZero;
        public string GetString(string key) => values.TryGetValue(key, out object v) && v is string s ? s : "";

        public void Set(string key, object value) => values[key] = value;

        // Dotted access from Python (`self.Field.get_ent("x").some_value`) routes here.
        public object __getattr__(string name) => values.TryGetValue(name, out object v) ? v : (object)Fix32.Zero;

        public void __setattr__(string name, object value) => values[name] = value;
    }
}
