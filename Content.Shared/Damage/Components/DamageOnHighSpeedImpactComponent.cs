using Content.Shared.Damage.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Should the entity take damage / be stunned if colliding at a speed above MinimumSpeed?
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(DamageOnHighSpeedImpactSystem))]
public sealed partial class DamageOnHighSpeedImpactComponent : Component
{
    [DataField]
    public float MinimumSpeed = 10f;

    /// <summary>
    ///     Damage dealt per meter/second of velocity.
    /// </summary>
    [DataField]
    public float SpeedDamageFactor = 0.5f;

    [DataField]
    public SoundSpecifier SoundHit = default!;

    [DataField]
    public float StunChance = 0.25f;

    [DataField]
    public int StunMinimumDamage = 5;

    [DataField]
    public float StunSeconds = 1f;

    [DataField]
    public float DamageCooldown = 2f;

    [DataField]
    public TimeSpan? LastHit;

    /// <summary>
    ///     What damage types should be dealt to the entity. DO NOT SET ANY VALUES OUTSIDE OF 0 AND 1.
    /// </summary>
    [DataField]
    public DamageSpecifier Damage = default!;
}
