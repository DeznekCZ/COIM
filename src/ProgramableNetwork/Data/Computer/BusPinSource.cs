using Mafi.Core;
using Mafi.Serialization;

namespace ProgramableNetwork;

// The single optional input source feeding one bus pin (the bus reads FROM
// this).  Set directly on the pin via its settings tooltip, or by dragging a
// local module output onto the pin.  A pin with no source latches its last
// value.
//
// Tagged so the on-disk format stays stable as directions are added:
//   LocalModule           — an output pin on a module on the SAME controller.
//   ExternalModule        — an output pin on a module (e.g. a PLC module) on
//                           ANOTHER controller; distance-validated on paste.
//   ExternalControllerBus — a pin on another controller's bus (bus-to-bus
//                           inter-controller link); distance-validated on paste.
//   NetworkVariable       — a global VariableManager variable (bridge; this is
//                           the case that costs computing — see
//                           Controller.GetRequiredComputation).
public class BusPinSource
{
    public enum SourceKind : byte
    {
        LocalModule = 1,
        ExternalModule = 2,
        ExternalControllerBus = 3,
        NetworkVariable = 4,
    }

    public SourceKind Kind { get; }

    // LocalModule / ExternalModule: a module output pin.  ControllerId is unused
    // (default) for LocalModule — the module lives on the bus's own controller.
    public EntityId ControllerId { get; }
    public long ModuleId { get; }
    public string OutputId { get; }

    // ExternalControllerBus: a pin on another controller's bus.  BusName is remembered
    // alongside BusId so the link survives the remote bus's id changing (copy/paste
    // reallocates ids, and bus order can change) — resolution falls back to the name.
    public long BusId { get; }
    public int PinIndex { get; }
    public string BusName { get; }

    // NetworkVariable: a global VariableManager variable name.
    public string VariableName { get; }

    private BusPinSource(SourceKind kind, EntityId controllerId, long moduleId, string outputId,
        long busId, int pinIndex, string variableName, string busName)
    {
        Kind = kind;
        ControllerId = controllerId;
        ModuleId = moduleId;
        OutputId = outputId ?? "";
        BusId = busId;
        PinIndex = pinIndex;
        VariableName = variableName ?? "";
        BusName = busName ?? "";
    }

    public static BusPinSource Local(long moduleId, string outputId)
    {
        return new BusPinSource(SourceKind.LocalModule, default, moduleId, outputId, 0, 0, "", "");
    }

    // A module output on another controller (e.g. a PLC module).  Distance-
    // validated on clone/paste.
    public static BusPinSource ExternalModuleOutput(EntityId controllerId, long moduleId, string outputId)
    {
        return new BusPinSource(SourceKind.ExternalModule, controllerId, moduleId, outputId, 0, 0, "", "");
    }

    public static BusPinSource ExternalBusPin(EntityId controllerId, long busId, int pinIndex, string busName)
    {
        return new BusPinSource(SourceKind.ExternalControllerBus, controllerId, 0, "", busId, pinIndex, "", busName);
    }

    public static BusPinSource Network(string variableName)
    {
        return new BusPinSource(SourceKind.NetworkVariable, default, 0, "", 0, 0, variableName, "");
    }

    // True for sources that reference ANOTHER controller — these get the distance
    // check on clone/paste and may be dropped.
    public bool IsExternalController =>
        Kind == SourceKind.ExternalModule || Kind == SourceKind.ExternalControllerBus;

    public void Serialize(BlobWriter writer)
    {
        writer.WriteByte((byte)Kind);
        switch (Kind)
        {
            case SourceKind.LocalModule:
                writer.WriteLong(ModuleId);
                writer.WriteString(OutputId ?? "");
                break;
            case SourceKind.ExternalModule:
                EntityId.Serialize(ControllerId, writer);
                writer.WriteLong(ModuleId);
                writer.WriteString(OutputId ?? "");
                break;
            case SourceKind.ExternalControllerBus:
                EntityId.Serialize(ControllerId, writer);
                writer.WriteLong(BusId);
                writer.WriteInt(PinIndex);
                writer.WriteString(BusName ?? "");
                break;
            case SourceKind.NetworkVariable:
                writer.WriteString(VariableName ?? "");
                break;
        }
    }

    // hasBusName gates the ExternalControllerBus bus-name string, which only exists in
    // ControllerBus serialization v5+ (the caller passes its loaded version's verdict).
    public static BusPinSource Deserialize(BlobReader reader, bool hasBusName)
    {
        SourceKind kind = (SourceKind)reader.ReadByte();
        switch (kind)
        {
            case SourceKind.LocalModule:
                return Local(reader.ReadLong(), reader.ReadString());
            case SourceKind.ExternalModule:
                return ExternalModuleOutput(EntityId.Deserialize(reader), reader.ReadLong(), reader.ReadString());
            case SourceKind.ExternalControllerBus:
                EntityId controllerId = EntityId.Deserialize(reader);
                long busId = reader.ReadLong();
                int pinIndex = reader.ReadInt();
                string busName = hasBusName ? reader.ReadString() : "";
                return ExternalBusPin(controllerId, busId, pinIndex, busName);
            case SourceKind.NetworkVariable:
                return Network(reader.ReadString());
            default:
                return null;
        }
    }
}
