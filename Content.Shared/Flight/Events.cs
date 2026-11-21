using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared.Flight.Events;

[Serializable, NetSerializable]
public sealed partial class DashDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class FlightDoAfterEvent : SimpleDoAfterEvent { }

// WoF start - local event so other systems (dragon altitude) can react to takeoff and landing
/// <summary>
///     Raised directed at an entity on the server after its flight is turned on or off.
/// </summary>
[ByRefEvent]
public readonly record struct FlightToggledEvent(bool Active);
// WoF end

[Serializable, NetSerializable]
public sealed class FlightEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public bool IsFlying { get; }
    public bool IsAnimated { get; }
    public FlightEvent(NetEntity uid, bool isFlying, bool isAnimated)
    {
        Uid = uid;
        IsFlying = isFlying;
        IsAnimated = isAnimated;
    }
}
