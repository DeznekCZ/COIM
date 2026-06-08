using Mafi;
using Mafi.Serialization;
using System;

namespace ProgramableNetwork;

// A single variable bus on a Controller: a fixed strip of 4 bidirectional
// Fix32 pins, each with a player-set name, plus the bus's own name.  Pins are
// addressable from PLC-PY as self.Bus.<bus_name>.<pin_name>.  Values latch
// across ticks (last write wins) — the same semantics the global
// VariableManager already uses, so cross-module/cross-tick reads behave
// consistently.
//
// Serialization follows the Module contract ([ManuallyWrittenSerialization] +
// static delegate thunks + static Serialize/Deserialize + instance
// SerializeData/DeserializeData) so the controller can persist its bus list
// with Lyst<ControllerBus>.Serialize / .Deserialize, exactly like Modules.
//
// Phase scope: data + serialization only.  The left-gutter UI, the bidirectional
// pin wiring (sentinel ModuleConnector with ModuleId == -1), and the per-bus
// inter-controller link are added in later phases.
[ManuallyWrittenSerialization]
public class ControllerBus
{
    // A bus is always 4 pins tall — one strip equals one module-row height
    // (4 × 20px).  This is a hard layout invariant the gutter widget and the
    // pin pitch both rely on, not a tunable.
    public const int PinCount = 4;

    // Which gutter the bus renders in.  The player picks this in the settings
    // tooltip; the left panel renders Left buses, the right panel Right buses.
    public enum BusSide : byte
    {
        Left = 0,
        Right = 1,
    }

    // What a pin does + how it's fed.  Drives connect rules and the per-tick
    // signal plan (stage 2):
    //   Input      — accepts cables from module outputs; readable by PLC-Py and
    //                other controllers; cleared every tick before the module loop.
    //   Controller — reads a pin on another/this controller; latches (cleared only
    //                when the reference is invalid); highlights the target
    //                controller(s) on connection-button hover.
    //   Plc        — written by a PLC-type module.
    //   NetworkRead— reads a network variable into the pin (costs computing;
    //                datacenter-gated).  Network WRITE stays a separate module.
    public enum BusPinType : byte
    {
        Output = 0,
        Controller = 1,
        Plc = 2,
    }

    // ControllerBus's own on-disk format version, independent of the controller's
    // CONTROLLER_VARIABLE_BUS gate.  Written first in SerializeData so per-bus
    // fields can be version-gated without bumping the controller stream.
    //   v1 — Name + 4×(pin name, pin value)
    //   v2 — added stable Id (after version) + a per-pin BusPinSource block
    //   v3 — added Side (after Id)
    //   v4 — added a per-pin BusPinType block (after the sources block)
    //   v5 — ExternalControllerBus sources now carry the remote bus NAME (so a link
    //        survives the remote bus's id changing on copy/paste / reorder)
    private const int SERIALIZATION_VERSION = 5;

    private static readonly Action<object, BlobWriter> s_serializeDataDelayedAction = delegate (object obj, BlobWriter writer)
    {
        ((ControllerBus)obj).SerializeData(writer);
    };
    private static readonly Action<object, BlobReader> s_deserializeDataDelayedAction = delegate (object obj, BlobReader reader)
    {
        ((ControllerBus)obj).DeserializeData(reader);
    };

    public static void Serialize(ControllerBus value, BlobWriter writer)
    {
        if (writer.TryStartClassSerialization(value))
        {
            writer.EnqueueDataSerialization(value, s_serializeDataDelayedAction);
        }
    }

    public static ControllerBus Deserialize(BlobReader reader)
    {
        if (reader.TryStartClassDeserialization(out ControllerBus obj))
        {
            reader.EnqueueDataDeserialization(obj, s_deserializeDataDelayedAction);
        }
        return obj;
    }

    // INLINE config (de)serializers for the clone / blueprint path.  Unlike the
    // Lyst-based save above, EntityConfigData arrays serialize through a standalone
    // BlobWriter/Reader where the deferred class-ref mechanism (TryStartClass* +
    // EnqueueData*) does NOT reliably flush — so a bus written that way came back with
    // its pin names (and other fields) blank after a copy/paste.  These write/read the
    // fields DIRECTLY (the same shape entity-field EntityId arrays and string lists use),
    // so the data round-trips through the config without depending on the deferred queue.
    public static void WriteConfig(ControllerBus bus, BlobWriter writer)
    {
        bus.SerializeData(writer);
    }

    public static ControllerBus ReadConfig(BlobReader reader)
    {
        ControllerBus bus = new ControllerBus();
        bus.DeserializeData(reader);
        return bus;
    }

    // Stable per-controller id, allocated when the bus is created (via
    // Controller.AllocateModuleId).  Bus connections reference a bus by Id, not
    // by Name or list position, so renames and reordering don't break wiring.
    public long Id;
    public string Name;
    public BusSide Side;
    public string[] PinNames;
    public Fix32[] PinValues;
    // Per-pin type (drives connect rules + the stage-2 signal plan).
    public BusPinType[] PinTypes;
    // Per-pin input source (the bus reads FROM it); null = no source, pin latches.
    // For Controller/NetworkRead/NetworkWrite pins this holds the external target.
    public BusPinSource[] PinSources;

    public ControllerBus()
    {
        Id = 0;
        Name = "";
        Side = BusSide.Left;
        PinNames = new string[PinCount];
        PinValues = new Fix32[PinCount];
        PinTypes = new BusPinType[PinCount];
        PinSources = new BusPinSource[PinCount];
        for (int i = 0; i < PinCount; i++)
        {
            PinNames[i] = "";
            PinValues[i] = Fix32.Zero;
            PinTypes[i] = BusPinType.Output;
            PinSources[i] = null;
        }
    }

    public ControllerBus(string name)
        : this()
    {
        Name = name ?? "";
    }

    public ControllerBus(long id, string name)
        : this()
    {
        Id = id;
        Name = name ?? "";
    }

    // Returns -1 when no pin carries this name.  Empty / null never matches a
    // real pin, so an unconfigured slot stays invisible to script access.
    public int IndexOfPin(string pinName)
    {
        if (string.IsNullOrEmpty(pinName))
        {
            return -1;
        }
        for (int i = 0; i < PinCount; i++)
        {
            if (PinNames[i] == pinName)
            {
                return i;
            }
        }
        return -1;
    }

    // Reads a pin by name; unknown name reads as Fix32.Zero (matches how an
    // unconnected module input reads).
    public Fix32 Get(string pinName)
    {
        int i = IndexOfPin(pinName);
        return i >= 0 ? PinValues[i] : Fix32.Zero;
    }

    // Writes a pin by name; a write to an unknown name is dropped (pins are
    // defined on the controller via the settings tooltip, not created from a
    // script).
    public void Set(string pinName, Fix32 value)
    {
        int i = IndexOfPin(pinName);
        if (i >= 0)
        {
            PinValues[i] = value;
        }
    }

    protected void SerializeData(BlobWriter writer)
    {
        writer.WriteInt(/*Version*/ SERIALIZATION_VERSION);
        writer.WriteLong(Id); // v2+
        writer.WriteByte((byte)Side); // v3+
        writer.WriteString(Name ?? "");
        for (int i = 0; i < PinCount; i++)
        {
            writer.WriteString(PinNames[i] ?? "");
            Fix32.Serialize(PinValues[i], writer);
        }
        // v2+: per-pin input source, each prefixed by a present flag.
        for (int i = 0; i < PinCount; i++)
        {
            bool hasSource = PinSources[i] != null;
            writer.WriteBool(hasSource);
            if (hasSource)
            {
                PinSources[i].Serialize(writer);
            }
        }
        // v4+: per-pin type.
        for (int i = 0; i < PinCount; i++)
        {
            writer.WriteByte((byte)PinTypes[i]);
        }
    }

    protected void DeserializeData(BlobReader reader)
    {
        // Constructor is bypassed during deserialization (same as Module), so
        // every field is repopulated here — allocate the arrays before filling.
        int version = reader.ReadInt();
        Id = version >= 2 ? reader.ReadLong() : 0;
        Side = version >= 3 ? (BusSide)reader.ReadByte() : BusSide.Left;
        Name = reader.ReadString();
        PinNames = new string[PinCount];
        PinValues = new Fix32[PinCount];
        PinTypes = new BusPinType[PinCount];
        PinSources = new BusPinSource[PinCount];
        for (int i = 0; i < PinCount; i++)
        {
            PinNames[i] = reader.ReadString();
            PinValues[i] = Fix32.Deserialize(reader);
            PinTypes[i] = BusPinType.Output;
            PinSources[i] = null;
        }
        // v2+: per-pin input source block (written after all pin name/value pairs).
        // v5+ external-bus sources carry the remote bus name (see BusPinSource).
        if (version >= 2)
        {
            bool hasBusName = version >= 5;
            for (int i = 0; i < PinCount; i++)
            {
                if (reader.ReadBool())
                {
                    PinSources[i] = BusPinSource.Deserialize(reader, hasBusName);
                }
            }
        }
        // v4+: per-pin type block.
        if (version >= 4)
        {
            for (int i = 0; i < PinCount; i++)
            {
                PinTypes[i] = (BusPinType)reader.ReadByte();
            }
        }
    }
}
