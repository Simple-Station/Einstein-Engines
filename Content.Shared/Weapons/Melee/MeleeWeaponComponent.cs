using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.UserInterface;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Weapons.Melee;

/// <summary>
/// When given to a mob lets them do unarmed attacks, or when given to an item lets someone wield it to do attacks.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause]
public sealed partial class MeleeWeaponComponent : Component
{
    // TODO: This is becoming bloated as shit.
    // This should just be its own component for alt attacks.
    /// <summary>
    /// Does this entity do a disarm on alt attack.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AltDisarm = true;

    /// <summary>
    /// Should the melee weapon's damage stats be examinable.
    /// </summary>
    [DataField]
    public bool Hidden;

    /// <summary>
    /// Next time this component is allowed to light attack. Heavy attacks are wound up and never have a cooldown.
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan NextAttack;

    /// <summary>
    /// Starts attack cooldown when equipped if true.
    /// </summary>
    [DataField]
    public bool ResetOnHandSelected = true;

    /*
     * Melee combat works based around 2 types of attacks:
     * 1. Click attacks with left-click. This attacks whatever is under your mnouse
     * 2. Wide attacks with right-click + left-click. This attacks whatever is in the direction of your mouse.
     */

    /// <summary>
    /// How many times we can attack per second.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float AttackRate = 1f;

    /// <summary>
    ///     When power attacking, the swing speed (in attacks per second) is multiplied by this amount
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeavyRateModifier = 0.8f;
    /// <summary>
    /// Are we currently holding down the mouse for an attack.
    /// Used so we can't just hold the mouse button and attack constantly.
    /// </summary>
    [AutoNetworkedField]
    public bool Attacking = false;

    /// <summary>
    /// If true, attacks will be repeated automatically without requiring the mouse button to be lifted.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool AutoAttack;

    /// <summary>
    /// Base damage for this weapon. Can be modified via heavy damage or other means.
    /// </summary>
    [DataField(required: true)]
    [AutoNetworkedField]
    public DamageSpecifier Damage = default!;

    [DataField, AutoNetworkedField]
    public FixedPoint2 BluntStaminaDamageFactor = FixedPoint2.New(1f);

    /// <summary>
    /// Multiplies damage by this amount for single-target attacks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ClickDamageModifier = FixedPoint2.New(1);

    // TODO: Temporarily 1.5 until interactionoutline is adjusted to use melee, then probably drop to 1.2
    /// <summary>
    /// Nearest edge range to hit an entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    /// <summary>
    ///     Attack range for heavy swings
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeavyRangeModifier = 1f;

    /// <summary>
    ///     Weapon damage is multiplied by this amount for heavy swings
    /// </summary>
    [DataField, AutoNetworkedField]
    public float HeavyDamageBaseModifier = 1.2f;

    /// <summary>
    /// Total width of the angle for wide attacks.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle Angle = Angle.FromDegrees(45);

    [DataField, AutoNetworkedField]
    public EntProtoId Animation = "WeaponArcPunch";

    [DataField, AutoNetworkedField]
    public EntProtoId WideAnimation = "WeaponArcSlash";

    /// <summary>
    /// Rotation of the animation.
    /// 0 degrees means the top faces the attacker.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Angle WideAnimationRotation = Angle.Zero;

    [DataField]
    public bool SwingLeft;

    [DataField, AutoNetworkedField]
    public float HeavyStaminaCost = 10f;

    [DataField, AutoNetworkedField]
    public int MaxTargets = 1;

    // Sounds

    /// <summary>
    /// This gets played whenever a melee attack is done. This is predicted by the client.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundSwing { get; set; } = new SoundPathSpecifier("/Audio/Weapons/punchmiss.ogg")
    {
        Params = AudioParams.Default.WithVolume(-3f).WithVariation(0.025f),
    };

    // We do not predict the below sounds in case the client thinks but the server disagrees. If this were the case
    // then a player may doubt if the target actually took damage or not.
    // If overwatch and apex do this then we probably should too.

    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundHit;

    /// <summary>
    /// Plays if no damage is done to the target entity.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundNoDamage { get; set; } = new SoundCollectionSpecifier("WeakHit");

    #region Melee Contests Controller

    /// <summary>
    ///     Controls whether this melee weapon allows for mass to factor into damage.
    /// </summary>
    [DataField]
    public bool DoMassInteraction;

    /// <summary>
    ///     When true, mass provides a disadvantage.
    /// </summary>
    [DataField]
    public bool MassDisadvantage;

    /// <summary>
    ///     When true, mass contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool MassBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float MassRangeModifier = 1;

    /// <summary>
    ///     The output of a mass contest is increased by this amount.
    /// </summary>
    [DataField]
    public float MassOffset;

    /// <summary>
    ///     Controls whether this melee weapon allows for stamina to factor into damage.
    /// </summary>
    [DataField]
    public bool DoStaminaInteraction = true;

    /// <summary>
    ///     When true, stamina provides a disadvantage.
    /// </summary>
    [DataField]
    public bool StaminaDisadvantage = true;

    /// <summary>
    ///     When true, stamina contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool StaminaBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float StaminaRangeModifier = 2;

    /// <summary>
    ///     The output of a stamina contest is increased by this amount.
    /// </summary>
    [DataField]
    public float StaminaOffset = 0.25f;

    /// <summary>
    ///     Controls whether this melee weapon allows for health to factor into damage.
    /// </summary>
    [DataField]
    public bool DoHealthInteraction = true;

    /// <summary>
    ///     When true, health contests provide a disadvantage.
    /// </summary>
    [DataField]
    public bool HealthDisadvantage;

    /// <summary>
    ///     When true, health contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool HealthBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mass contests for melee.
    /// </summary>
    [DataField]
    public float HealthRangeModifier = 2;

    /// <summary>
    ///     The output of health contests is increased by this amount.
    /// </summary>
    [DataField]
    public float HealthOffset;

    /// <summary>
    ///     Controls whether this melee weapon allows for psychic casting stats to factor into damage.
    /// </summary>
    [DataField]
    public bool DoMindInteraction;

    /// <summary>
    ///     When true, high psychic casting stats provide a disadvantage.
    /// </summary>
    [DataField]
    public bool MindDisadvantage;

    /// <summary>
    ///     When true, mind contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool MindBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mind contests for melee.
    /// </summary>
    [DataField]
    public float MindRangeModifier = 1;

    /// <summary>
    ///     The output of a mind contest is increased by this amount.
    /// </summary>
    [DataField]
    public float MindOffset;

    /// <summary>
    ///     Controls whether this melee weapon allows mood to factor into damage.
    /// </summary>
    [DataField]
    public bool DoMoodInteraction;

    /// <summary>
    ///     When true, mood provides a disadvantage.
    /// </summary>
    [DataField]
    public bool MoodDisadvantage;

    /// <summary>
    ///     When true, mood contests ignore clamp limitations for a melee weapon.
    /// </summary>
    [DataField]
    public bool MoodBypassClamp;

    /// <summary>
    ///     Multiplies the acceptable range of outputs provided by mood contests for melee.
    /// </summary>
    [DataField]
    public float MoodRangeModifier = 1;

    /// <summary>
    ///     The output of mood contests is increased by this amount.
    /// </summary>
    [DataField]
    public float MoodOffset;

    /// <summary>
    ///     Enables the EveryContest interaction for a melee weapon.
    ///     IF YOU PUT THIS ON ANY WEAPON OTHER THAN AN ADMEME, I WILL COME TO YOUR HOUSE AND SEND YOU TO MEET YOUR CREATOR WHEN THE PLAYERS COMPLAIN.
    /// </summary>
    [DataField]
    public bool DoEveryInteraction;

    /// <summary>
    ///     When true, EveryContest provides a disadvantage.
    /// </summary>
    [DataField]
    public bool EveryDisadvantage;

    /// <summary>
    ///     How much Mass is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryMassWeight = 1;

    /// <summary>
    ///     How much Stamina is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryStaminaWeight = 1;

    /// <summary>
    ///     How much Health is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryHealthWeight = 1;

    /// <summary>
    ///     How much psychic casting stats are considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryMindWeight = 1;

    /// <summary>
    ///     How much mood is considered for an EveryContest.
    /// </summary>
    [DataField]
    public float EveryMoodWeight = 1;

    /// <summary>
    ///     When true, the EveryContest sums the results of all contests rather than multiplying them,
    ///     probably giving you a very, very, very large multiplier...
    /// </summary>
    [DataField]
    public bool EveryInteractionSumOrMultiply;

    #endregion
}

/// <summary>
/// Event raised on entity in GetWeapon function to allow systems to manually
/// specify what the weapon should be.
/// </summary>
public sealed class GetMeleeWeaponEvent : HandledEntityEventArgs
{
    public EntityUid? Weapon;
}
