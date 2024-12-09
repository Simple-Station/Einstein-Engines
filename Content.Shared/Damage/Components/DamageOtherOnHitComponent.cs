using Content.Shared.Contests;
using Content.Shared.Damage.Systems;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Damage.Components;

/// <summary>
///   Deals damage when thrown.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DamageOtherOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool IgnoreResistances = false;

    /// <summary>
    ///   The damage that a throw deals.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new();

    /// <summary>
    ///   The stamina cost of throwing this entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float StaminaCost = 3.5f;

    /// <summary>
    ///   The maximum amount of hits per throw before landing on the floor.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxHitQuantity = 1;

    /// <summary>
    ///   The tracked amount of hits in a single throw.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int HitQuantity = 0;

    /// <summary>
    ///   The multiplier to apply to the entity's light attack damage to calculate the throwing damage.
    ///   Only used if this component has a MeleeWeaponComponent and Damage is not set on the prototype.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MeleeDamageMultiplier = 1f;

    /// <summary>
    ///   The sound to play when this entity hits on a throw.
    ///   If null, attempts to retrieve the SoundHit from MeleeWeaponComponent.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundHit;

    /// <summary>
    ///   Arguments for modifying the throwing weapon damage with contests.
    ///   These are the same ContestArgs in MeleeWeaponComponent.
    /// </summary>
    [DataField]
    public ContestArgs ContestArgs = new ContestArgs
    {
        DoStaminaInteraction = true,
        StaminaDisadvantage = true,
        DoHealthInteraction = true,
    };

    /// <summary>
    ///   Plays if no damage is done to the target entity.
    ///   If null, attempts to retrieve the SoundNoDamage from MeleeWeaponComponent.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundNoDamage { get; set; } = new SoundCollectionSpecifier("WeakHit");
}
