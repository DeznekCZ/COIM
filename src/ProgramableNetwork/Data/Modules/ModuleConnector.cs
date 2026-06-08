using Mafi.Serialization;

namespace ProgramableNetwork
{
    public class ModuleConnector
    {
        // What the source side of this connection is.  A bus pin reuses ModuleId for
        // the bus's stable Id and OutputId for the pin INDEX (as a string); a module
        // output uses ModuleId for the module id and OutputId for the output pin id.
        public enum ConnectorKind : byte
        {
            Module = 0,
            Bus = 1,
        }

        public long ModuleId { get; }
        public string OutputId { get; }
        public ConnectorKind Kind { get; }

        public ModuleConnector(long moduleId, string name)
            : this(moduleId, name, ConnectorKind.Module)
        {
        }

        public ModuleConnector(long moduleId, string name, ConnectorKind kind)
        {
            ModuleId = moduleId;
            OutputId = name;
            Kind = kind;
        }

        public bool IsBus => Kind == ConnectorKind.Bus;

        // A bus-pin connection: ModuleId = the bus's stable Id, OutputId = the pin
        // INDEX as a string.  Buses draw from the same unique id pool as modules, so
        // the id never collides; Kind makes the bus-ness explicit and persisted.
        public static ModuleConnector ForBusPin(long busId, int pinIndex)
        {
            return new ModuleConnector(busId, pinIndex.ToString(), ConnectorKind.Bus);
        }
		public static void Serialize(ModuleConnector value, BlobWriter writer)
		{
			writer.WriteLong(value.ModuleId);
			writer.WriteString(value.OutputId);
		}

		public static ModuleConnector Deserialize(BlobReader reader)
		{
			return new ModuleConnector(reader.ReadLong(), reader.ReadString());
		}

        // Parses OutputId as a bus pin index.  Only meaningful when IsBus is true.
        public bool TryGetPinIndex(out int pinIndex)
        {
            return int.TryParse(OutputId, out pinIndex);
        }

        public override bool Equals(object obj)
        {
            return obj is ModuleConnector mc
                && this.ModuleId == mc.ModuleId
                && this.OutputId == mc.OutputId
                && this.Kind == mc.Kind;
        }
    }
}