using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericZapRule))]
public sealed partial class NoosphericZapRuleComponent : Component
{
    /// <summary>
    ///     How long (in seconds) should this event stun its victims.
    /// </summary>
    public float StunDuration = 5f;

    /// <summary>
    ///     How long (in seconds) should this event give its victims the Stuttering condition.
    /// </summary>
    public float StutterDuration = 10f;

    /// <summary>
    ///     When paralyzing a Psion with a reroll still available, how much should this event modify the odds of generating a power.
    /// </summary>
    public float PowerRerollMultiplier = 0.25f;
}
