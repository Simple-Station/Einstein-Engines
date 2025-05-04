using Content.Server.StationEvents.Events;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(NoosphericFryRule))]
public sealed partial class NoosphericFryRuleComponent : Component
{
    [DataField]
    public float FryHeadgearMinorThreshold = 750f;

    [DataField]
    public float FryHeadgearMajorThreshold = 900f;
}
