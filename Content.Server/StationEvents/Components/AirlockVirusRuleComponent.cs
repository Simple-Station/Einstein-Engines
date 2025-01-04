using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent]
public sealed partial class AirlockVirusRuleComponent : Component
{
    /// <summary>
    ///     The minimum amount of time in seconds before each infected door is self-emagged.
    /// </summary>
    [DataField]
    public int MinimumTimeToEmag = 30;

    /// <summary>
    ///     The maximum amount of time in seconds before each infected door is self-emagged.
    /// </summary>
    [DataField]
    public int MaximumTimeToEmag = 120;
}
