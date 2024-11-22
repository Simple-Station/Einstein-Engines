using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.GameStates;

namespace Content.Shared.Projectiles;

/// <summary>
///   Passively damages the mob this embeddable is attached to.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmbedPassiveDamageComponent : Component
{
    /// <summary>
    ///   The entity this embeddable is attached to.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public EntityUid? Embedded = null;

    /// <summary>
    ///   The damage component to deal damage to.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public DamageableComponent? EmbeddedDamageable = null;

    /// <summary>
    ///   Damage per interval dealt to the entity every interval.
    /// </summary>
    [DataField(required: true), ViewVariables(VVAccess.ReadWrite)]
    public DamageSpecifier Damage;

    /// <summary>
    /// The maximum HP the damage will be given to. If 0, disabled.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 DamageCap = 250;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan NextDamage = TimeSpan.Zero;
}
