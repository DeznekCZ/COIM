using System.Collections.Generic;
using Mafi;
using Mafi.Core;
using Mafi.Core.Entities;
using Mafi.Core.Entities.Static.Layout;
using Mafi.Core.Factory.ElectricPower;
using Mafi.Core.Mods;
using Mafi.Serialization;

namespace NightMod.LampPosts;

/// <summary>
/// Runtime entity for a placed lamp post. It is a deliberately minimal powered building: while it
/// is night it draws its prototype's fixed electricity, and exposes whether that draw succeeded
/// through <see cref="IsLit"/>. During the day it draws nothing at all. Everything visible - the
/// cast light cone - is a render-only effect that <see cref="NightLampLights"/> drives from
/// <see cref="IsLit"/> and the day-night cycle; the entity itself does no rendering work.
///
/// <para>Serialization lives in the sibling partial file <c>LampPost.Serialization.cs</c>.</para>
/// </summary>
[ManuallyWrittenSerialization]
public sealed partial class LampPost : LayoutEntity, IElectricityConsumingEntity, IEntityWithSimUpdate {

	/// <summary>
	/// Every live lamp post in the current game registers itself here. <see cref="NightLampLights"/>
	/// reads from this set instead of scanning the scene, because purely static layout entities are
	/// rendered via instanced rendering and have no per-entity <c>EntityMb</c> to find. Destroyed
	/// lamps are pruned by the light system on its periodic scan.
	/// </summary>
	internal static readonly HashSet<LampPost> All = new HashSet<LampPost>();

	/// <summary>Strongly-typed prototype; hides <see cref="IEntity.Prototype"/> with the concrete type.</summary>
	public new readonly LampPostProto Prototype;

	private readonly IElectricityConsumer m_electricityConsumer;

	// Resolved fresh, never saved: set in the constructor for a newly built lamp and re-resolved
	// in initAfterLoad for a loaded one, so it always points at the live service.
	private TimeOfDayManager m_timeOfDay;

	/// <summary>Lamp posts can be paused by the player, which switches the light off.</summary>
	public override bool CanBePaused => true;

	/// <summary>Electricity this lamp draws while enabled - read by the power grid.</summary>
	Electricity IElectricityConsumingEntity.PowerRequired => Prototype.ElectricityConsumed;

	/// <summary>The lamp's power-grid consumer, exposed read-only for the grid UI.</summary>
	public Option<IElectricityConsumerReadonly> ElectricityConsumer =>
		((IElectricityConsumerReadonly)m_electricityConsumer).SomeOption();

	/// <summary>
	/// Whether the lamp is currently powered and on - true only when it is enabled, night has
	/// fallen and the grid supplied its electricity on the last tick. <see cref="NightLampLights"/>
	/// only lights lamps for which this is true.
	/// </summary>
	public bool IsLit => m_electricityConsumer != null && m_electricityConsumer.DidConsumeLastTick;

	/// <summary>
	/// Per-lamp light tint, 0 = warm yellow .. 1 = cool blue. Derived deterministically from the
	/// entity id, so it is fixed the moment the lamp is constructed, is identical on every client,
	/// and survives save/load without being stored separately. <see cref="NightLampLights"/> turns
	/// this into the actual light colour.
	/// </summary>
	public float LightHue01 => (float)frac(Id.Value * 0.6180339887498949);

	/// <summary>
	/// Per-lamp turn-on delay, 0..5 seconds, also derived from the entity id. Each lamp flicks on
	/// at its own moment after night falls instead of a whole row lighting in unison.
	/// </summary>
	public float TurnOnDelaySeconds => (float)frac(Id.Value * 0.7548776662466927) * 5f;

	/// <summary>Fractional part of a value, in [0, 1).</summary>
	private static double frac(double v) => v - System.Math.Floor(v);

	public LampPost(EntityId id, LampPostProto proto, TileTransform transform, EntityContext context,
		TimeOfDayManager timeOfDay)
		: base(id, proto, transform, context) {
		Prototype = proto;
		m_timeOfDay = timeOfDay;
		m_electricityConsumer = context.ElectricityConsumerFactory.CreateConsumer(this);
		All.Add(this);
	}

	/// <summary>
	/// Re-resolves the (non-saved) time-of-day service after the entity is loaded from a save -
	/// the constructor handled it for a freshly built lamp - and re-registers the lamp into the
	/// scene-wide set (the set is in-memory only, so loaded lamps have to put themselves back).
	/// </summary>
	[InitAfterLoad]
	private void initAfterLoad(DependencyResolver resolver) {
		m_timeOfDay = resolver.Resolve<TimeOfDayManager>();
		All.Add(this);
	}

	/// <summary>
	/// Draws the lamp's electricity for this tick - but only at night. During the day the lamp is
	/// off and a no-op here means the grid records no demand from it at all.
	/// </summary>
	public void SimUpdate() {
		if (IsEnabled && (m_timeOfDay.IsNight || m_timeOfDay.IsVisual)) {
			m_electricityConsumer.TryConsume();
		}
	}
}
