using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericZapRule))]
public sealed partial class NoosphericZapRuleComponent : Component
{
    /// <summary>
    ///     How long (in seconds) should this event stun its victims.
    /// </summary>
    [DataField]
    public float StunDuration = 5f;

    /// <summary>
    ///     How long (in seconds) should this event give its victims the Stuttering condition.
    /// </summary>
    [DataField]
    public float StutterDuration = 10f;

    /// <summary>
    ///     When paralyzing a Psion with a reroll still available, how much should this event modify the odds of generating a power.
    /// </summary>
    [DataField]
    public float PowerRerollMultiplier = 0.25f;

    /// <summary>
    ///     The status effect that will be applied to this Psion when mindbroken.
    /// </summary>
    [DataField]
    public string ZapStatusEffect = "Stutter";

    /// <summary>
    ///     The accent that will be applied to this Psion when mindbroken.
    /// </summary>
    [DataField]
    public string ZapStatusAccent = "StutteringAccent";
}
