using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Physics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._WoF.Flight;

/// <summary>
///     Adds altitude bands on top of the existing <see cref="Content.Shared.Flight.FlightComponent"/>.
///     Taking off puts a dragon at <see cref="FlightAltitude.Low"/>; it can then climb to
///     <see cref="FlightAltitude.High"/> and descend again. Granted by the dragon wing body part,
///     so losing or disabling the wing removes it (and drops the dragon if it was airborne).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class DragonFlightComponent : Component
{
    [DataField, AutoNetworkedField]
    public FlightAltitude Altitude = FlightAltitude.Ground;

    [DataField]
    public EntProtoId ClimbAction = "ActionDragonClimb";

    [DataField]
    public EntProtoId DescendAction = "ActionDragonDescend";

    [DataField, AutoNetworkedField]
    public EntityUid? ClimbActionEntity;

    [DataField, AutoNetworkedField]
    public EntityUid? DescendActionEntity;

    /// <summary>
    ///     Extra speed multiplier while at high altitude, applied on top of the flight speed modifier.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HighSpeedModifier = 1.35f;

    /// <summary>
    ///     How far the sprite is lifted at each altitude, in tiles. Purely visual.
    /// </summary>
    [DataField]
    public float LowVisualOffset = 0.15f;

    [DataField]
    public float HighVisualOffset = 0.4f;

    /// <summary>
    ///     Collision bits removed from the dragon's fixtures while airborne. The default lets a flying
    ///     dragon pass over tables, crates, railings and half walls, but walls and doors still block it.
    /// </summary>
    [DataField]
    public int AirborneIgnoredMask = (int) (CollisionGroup.MidImpassable | CollisionGroup.LowImpassable);

    /// <summary>
    ///     Damage taken when the dragon drops out of the sky from high altitude without descending first
    ///     (stunned, knocked out, wing lost, flight toggled off).
    /// </summary>
    [DataField]
    public DamageSpecifier? HighFallDamage;

    /// <summary>
    ///     Damage taken when dropping from low altitude. Usually small or none.
    /// </summary>
    [DataField]
    public DamageSpecifier? LowFallDamage;

    /// <summary>
    ///     Bits actually removed when taking off, per fixture, so landing only restores what we took away
    ///     and does not fight other systems (e.g. climbing) that also change collision masks.
    /// </summary>
    [DataField]
    public Dictionary<string, int> RemovedMaskBits = new();
}

[Serializable, NetSerializable]
public enum FlightAltitude : byte
{
    Ground,
    Low,
    High,
}

public sealed partial class DragonClimbActionEvent : InstantActionEvent { }

public sealed partial class DragonDescendActionEvent : InstantActionEvent { }
