using Mafi.Collections;
using Mafi.Core.Mods;
using Mafi.Core.Prototypes;
using Mafi.Localization;
using System;
using System.Collections.Generic;

namespace ProgramableNetwork;

/// <summary>
/// Canonical ids of the built-in swap groups — the single source of truth shared by the C#
/// <c>SwapGroupStart</c> declarations and Python modules.  Python references these via
/// <c>from Core.swap_groups import SwapGroups</c> then <c>swap_groups = [ SwapGroups.Notifications ]</c>
/// (see <see cref="ProgramableNetwork.Python.ImportStatement"/>, which maps the import to this type
/// and reflects the constants).  A Python module may also name an id that isn't here — the
/// registrator creates that group on first reference.
/// </summary>
public static class SwapGroups
{
    public const string Arithmetic = "arithmetic";
    public const string Comparison = "comparison";
    public const string Boolean = "boolean";
    public const string ConvertBridge = "convert_bridge";
    public const string ConstantInt = "constant_int";
    public const string DisplayScalar = "display_scalar";
    public const string SegDisplayConn = "seg_display_conn";
    public const string SegDisplayArith = "seg_display_arith";
    public const string LogisticsModeSet = "logistics_mode_set";
    public const string LogisticsModeGet = "logistics_mode_get";
    public const string Notifications = "notifications";
    public const string StorageLimitGet = "storage_limit_get";
    public const string StorageFlowSet = "storage_flow_set";
    public const string StorageLogisticsSet = "storage_logistics_set";
}

/// <summary>
/// Registry prototype listing a set of mutually swappable module prototypes (e.g. the
/// arithmetic combiners <c>Sum</c>/<c>Sub</c>/<c>Multiply</c>/<c>Divide</c>/<c>Modulo</c>).
/// A placed module carrying a prototype in one of these groups gets a swap button in the
/// inspector that offers the other members; picking one replaces the prototype in place
/// while keeping cables and field values.
///
/// Membership is <b>explicit</b>, not category-derived: modules that merely share a
/// <see cref="Category"/> (like <c>Bits_Encode</c>/<c>Bits_Decode</c> under Boolean) are
/// intentionally NOT swappable. Groups are built via
/// <see cref="ModuleSwapGroupExtensions.SwapGroupStart"/> +
/// <see cref="ModuleSwapGroupExtensions.EnlistSwapable"/> and registered into the
/// <see cref="ProtosDb"/> so the UI can enumerate them via <c>All&lt;ModuleSwapGroup&gt;()</c>.
///
/// The group is a pure registry proto — it is never placed on a controller and never
/// serialized into a save, so it needs no persistence handling.
/// </summary>
public class ModuleSwapGroup : Proto
{
    /// <summary>
    /// Compatibility gate for a single candidate swap. Returns <see cref="LocStrFormatted.Empty"/>
    /// when swapping <paramref name="module"/> to <paramref name="target"/> is allowed, otherwise a
    /// short human-readable reason it is blocked (surfaced directly as the picker tooltip and, via
    /// its <c>.Value</c>, as the command error). Both the inspector (to grey out candidates) and the
    /// command executor (as the authoritative reject) run these.  Checks are attached to the
    /// <b>target</b> member at <see cref="ModuleSwapGroupExtensions.EnlistSwapable"/> time — a member
    /// declares the preconditions for swapping <i>into</i> it (e.g. XOR forbids incoming extension pins).
    /// </summary>
    public delegate LocStrFormatted SwapCheck(Module module, ModuleProto target);

    /// <summary>
    /// Member prototypes.  A member can be a C# module enrolled through the fluent
    /// <see cref="ModuleSwapGroupExtensions.EnlistSwapable"/> chain off <c>BuildAndAdd()</c>, or a
    /// Python-registered module enrolled by <see cref="ProgramableNetwork.Python.ModuleRegistrator"/>
    /// from its <c>swap_groups</c> class property.  This is the SAME list instance the builder
    /// appends to, so members enrolled AFTER <see cref="ModuleSwapGroupBuilder.RegisterSwapable"/>
    /// (e.g. Python modules registered on the later PyModules pass) still show up here.
    /// </summary>
    public Lyst<ModuleProto> Members { get; }

    /// <summary>
    /// Live display name reused from the matching <see cref="Category"/> LocStr rather than a
    /// freshly minted key, so no new translation entries are added for the group.
    /// </summary>
    public LocStr DisplayName { get; }

    private readonly Dictionary<ModuleProto.ID, SwapCheck[]> m_checksByMember;

    public ModuleSwapGroup(ID id, Str strings, LocStr displayName,
        Lyst<ModuleProto> members, Dictionary<ModuleProto.ID, SwapCheck[]> checksByMember)
        : base(id, strings, null)
    {
        DisplayName = displayName;
        Members = members ?? new Lyst<ModuleProto>();
        m_checksByMember = checksByMember ?? new Dictionary<ModuleProto.ID, SwapCheck[]>();
    }

    public bool Contains(ModuleProto.ID protoId)
    {
        foreach (ModuleProto member in Members)
        {
            if (member.Id == protoId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Runs the checks attached to the <paramref name="target"/> member (the module being
    /// swapped into) and returns the first failure reason, or <see cref="LocStrFormatted.Empty"/>
    /// when the swap is allowed. A member with no attached checks is always swappable-into.
    /// </summary>
    public LocStrFormatted Check(Module module, ModuleProto target)
    {
        if (!m_checksByMember.TryGetValue(target.Id, out SwapCheck[] checks) || checks == null)
        {
            return LocStrFormatted.Empty;
        }
        foreach (SwapCheck check in checks)
        {
            LocStrFormatted reason = check(module, target);
            if (!reason.IsEmptyOrNull)
            {
                return reason;
            }
        }
        return LocStrFormatted.Empty;
    }

    // ---- built-in checks -------------------------------------------------------------

    /// <summary>
    /// Blocks the swap when the target prototype is wider than the module's current footprint
    /// and the extra cells to the right are off-grid or occupied by another module. The
    /// module's own current cells count as free. Same slot-occupancy rule as
    /// <see cref="Controller.TryMoveModule"/>. Passes freely in preview (no controller bound).
    /// </summary>
    public static LocStrFormatted FitsSpace(Module module, ModuleProto target)
    {
        Controller controller = module?.Controller;
        if (controller == null)
        {
            return LocStrFormatted.Empty;
        }
        ModuleLayout targetLayout = new ModuleLayout(target);
        int baseW = targetLayout.GetBaseWidth(module);
        // Effective extension cells the target can actually keep (clamped to its own maxes),
        // so we don't over-estimate width when the target holds fewer extension slots.
        int extra = Math.Max(
            Math.Min(module.InputExtensionCount, target.MaxInputExtensions),
            Math.Max(
                Math.Min(module.OutputExtensionCount, target.MaxOutputExtensions),
                Math.Min(module.DisplayExtensionCount, target.MaxDisplayExtensions)));
        int targetWidth = baseW + extra;
        int currentWidth = module.Layout.GetWidth(module);
        if (targetWidth <= currentWidth)
        {
            // Same or shrinking — the module's existing cells already cover the target.
            return LocStrFormatted.Empty;
        }
        int extraCells = targetWidth - currentWidth;
        int rightEdge = module.Column + currentWidth;
        if (!controller.IsRangeFree(module.Row, rightEdge, extraCells, module))
        {
            return new LocStrFormatted("Not enough space to the right for the wider module");
        }
        return LocStrFormatted.Empty;
    }

    /// <summary>
    /// Blocks the swap when the target prototype holds fewer extension pins than the module
    /// currently uses, which would silently clamp the count and drop the cables on the removed
    /// pins. Lets a player keep a many-input Sum's wiring intact when swapping between equally
    /// extensible siblings, and warns before a lossy swap into a narrower one.
    /// </summary>
    public static LocStrFormatted PreservesExtensions(Module module, ModuleProto target)
    {
        if (module.InputExtensionCount > target.MaxInputExtensions)
        {
            return new LocStrFormatted($"Would drop {module.InputExtensionCount - target.MaxInputExtensions} input pin(s)");
        }
        if (module.OutputExtensionCount > target.MaxOutputExtensions)
        {
            return new LocStrFormatted($"Would drop {module.OutputExtensionCount - target.MaxOutputExtensions} output pin(s)");
        }
        if (module.DisplayExtensionCount > target.MaxDisplayExtensions)
        {
            return new LocStrFormatted("Would shrink the display width");
        }
        return LocStrFormatted.Empty;
    }

    /// <summary>
    /// Explicit window on how many input extension pins a swap may carry: the target must be
    /// able to hold at least <paramref name="min"/> and the module must not exceed
    /// <paramref name="max"/>. Use when a group wants a fixed bound rather than the
    /// "preserve current" rule of <see cref="PreservesExtensions"/>.
    /// </summary>
    public static SwapCheck ExtensionRange(int min, int max)
    {
        return (module, target) =>
        {
            if (target.MaxInputExtensions < min)
            {
                return new LocStrFormatted($"Target supports fewer than {min} extension pins");
            }
            if (module.InputExtensionCount > max)
            {
                return new LocStrFormatted($"Cannot swap with more than {max} extension pins");
            }
            return LocStrFormatted.Empty;
        };
    }
}

/// <summary>
/// Accumulates member prototypes for a single <see cref="ModuleSwapGroup"/> as they are
/// registered, then materialises the group proto. Created via
/// <see cref="ModuleSwapGroupExtensions.SwapGroupStart"/>; members enrol themselves through
/// <see cref="ModuleSwapGroupExtensions.EnlistSwapable"/> chained off <c>BuildAndAdd()</c>;
/// <see cref="RegisterSwapable"/> finishes the group. All of this runs during
/// <c>RegisterData</c>, before the game uses any prototype.
/// </summary>
public class ModuleSwapGroupBuilder
{
    // Registry so a later registration pass — chiefly the Python ModuleRegistrator on the
    // PyModules pass — can look a group builder up by its id and enrol a freshly-built
    // prototype into it.  Populated by SwapGroupStart; ids are unique, so re-registration on a
    // fresh mod load simply overwrites.
    private static readonly Dictionary<string, ModuleSwapGroupBuilder> s_byId
        = new Dictionary<string, ModuleSwapGroupBuilder>();

    public static ModuleSwapGroupBuilder Find(string id)
    {
        return s_byId.TryGetValue(id, out ModuleSwapGroupBuilder builder) ? builder : null;
    }

    private readonly ProtoRegistrator m_registrator;
    private readonly string m_id;
    private readonly LocStr m_name;
    private readonly Lyst<ModuleProto> m_members = new Lyst<ModuleProto>();
    private readonly Dictionary<ModuleProto.ID, ModuleSwapGroup.SwapCheck[]> m_checksByMember
        = new Dictionary<ModuleProto.ID, ModuleSwapGroup.SwapCheck[]>();

    public ModuleSwapGroupBuilder(ProtoRegistrator registrator, string id, LocStr name)
    {
        m_registrator = registrator;
        m_id = id;
        m_name = name;
        s_byId[id] = this;
    }

    internal void Add(ModuleProto proto, ModuleSwapGroup.SwapCheck[] checks)
    {
        m_members.Add(proto);
        if (checks != null && checks.Length > 0)
        {
            m_checksByMember[proto.Id] = checks;
        }
    }

    public ModuleSwapGroup RegisterSwapable()
    {
        Proto.ID id = new Proto.ID("ProgramableNetwork_SwapGroup_" + m_id);
        // Share m_members by reference so members enrolled AFTER this call (e.g. Python modules
        // on the later PyModules pass, via ModuleRegistrator) are still seen through the proto.
        ModuleSwapGroup group = new ModuleSwapGroup(
            id, Proto.CreateStr(id, m_id, ""), m_name, m_members, m_checksByMember);
        m_registrator.PrototypesDb.Add(group);
        return group;
    }
}

public static class ModuleSwapGroupExtensions
{
    /// <summary>
    /// Opens a new swap group with the given stable <paramref name="id"/> (used to form the
    /// group proto id, and the key Python modules reference via <c>swap_groups</c>) and a reused
    /// category <paramref name="name"/> LocStr for the picker header. Compatibility checks are
    /// attached per member via <see cref="EnlistSwapable"/>, not on the group.
    /// </summary>
    public static ModuleSwapGroupBuilder SwapGroupStart(this ProtoRegistrator registrator,
        string id, LocStr name)
    {
        return new ModuleSwapGroupBuilder(registrator, id, name);
    }

    /// <summary>
    /// Enrols <paramref name="proto"/> into <paramref name="group"/> with optional
    /// <paramref name="checks"/> that gate swapping <i>into</i> this member (e.g. XOR passes
    /// <see cref="ModuleSwapGroup.PreservesExtensions"/> so a many-input AND/OR can't swap in
    /// and lose its extension pins). Returns the proto so the call chains cleanly off
    /// <c>BuildAndAdd()</c>:
    /// <c>registrator.ModuleBuilderStart(...)....BuildAndAdd().EnlistSwapable(group, checks…)</c>.
    /// The same method is used for Python modules — <see cref="ProgramableNetwork.Python.ModuleRegistrator"/>
    /// resolves the group by id via <see cref="ModuleSwapGroupBuilder.Find"/> and calls this.
    /// </summary>
    public static ModuleProto EnlistSwapable(this ModuleProto proto, ModuleSwapGroupBuilder group,
        params ModuleSwapGroup.SwapCheck[] checks)
    {
        group.Add(proto, checks);
        return proto;
    }
}
