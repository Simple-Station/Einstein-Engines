using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cuffs.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedCuffableSystem))]
public sealed partial class HandcuffComponent : Component
{
    /// <summary>
    ///     The time it takes to cuff an entity.
    /// </summary>
    [DataField]
    public float CuffTime = 3.5f;

    /// <summary>
    ///     The time it takes to uncuff an entity.
    /// </summary>
    [DataField]
    public float UncuffTime = 3.5f;

    /// <summary>
    ///     The time it takes for a cuffed entity to uncuff itself.
    /// </summary>
    [DataField]
    public float BreakoutTime = 15f;

    /// <summary>
    ///     If an entity being cuffed is stunned, this amount of time is subtracted from the time it takes to add/remove their cuffs.
    /// </summary>
    [DataField]
    public float StunBonus = 2f;

    /// <summary>
    ///     Will the cuffs break when removed?
    /// </summary>
    [DataField]
    public bool BreakOnRemove;

    /// <summary>
    ///     Will the cuffs break when removed?
    /// </summary>
    [DataField]
    public EntProtoId? BrokenPrototype;

    /// <summary>
    /// Whether or not these cuffs are in the process of being removed.
    /// Used simply to prevent spawning multiple <see cref="BrokenPrototype"/>.
    /// </summary>
    [DataField]
    public bool Removing;

    /// <summary>
    ///     The path of the RSI file used for the player cuffed overlay.
    /// </summary>
    [DataField]
    public string? CuffedRSI = "Objects/Misc/handcuffs.rsi";

    /// <summary>
    ///     The iconstate used with the RSI file for the player cuffed overlay.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? BodyIconState = "body-overlay";

    /// <summary>
    /// An opptional color specification for <see cref="BodyIconState"/>
    /// </summary>
    [DataField]
    public Color Color = Color.White;

    [DataField]
    public SoundSpecifier StartCuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_start.ogg");

    [DataField]
    public SoundSpecifier EndCuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_end.ogg");

    [DataField]
    public SoundSpecifier StartBreakoutSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_breakout_start.ogg");

    [DataField]
    public SoundSpecifier StartUncuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_takeoff_start.ogg");

    [DataField]
    public SoundSpecifier EndUncuffSound = new SoundPathSpecifier("/Audio/Items/Handcuffs/cuff_takeoff_end.ogg");

    /// <summary>
    ///     Both a bool and a multiplier combined. If it is 0, handcuffs are unaffected by mass contests. The absolute value of any nonzero acts as a multiplier on how much mass affects uncuff speed.
    ///     A value of 1 provides the full modifier from MassContest. 0.5 is half the effect of mass contests, and so on.
    /// </summary>
    [DataField]
    public float UncuffMassModifier = 0f;
}

/// <summary>
/// Event fired on the User when the User attempts to cuff the Target.
/// Should generate popups on the User.
/// </summary>
[ByRefEvent]
public record struct UncuffAttemptEvent(EntityUid User, EntityUid Target)
{
    public readonly EntityUid User = User;
    public readonly EntityUid Target = Target;
    public bool Cancelled = false;
}
