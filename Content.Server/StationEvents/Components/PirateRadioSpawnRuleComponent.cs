using Content.Server.StationEvents.Events;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(PirateRadioSpawnRule))]
public sealed partial class PirateRadioSpawnRuleComponent : Component
{
    [DataField]
    public List<ResPath> PirateRadioShuttlePath { get; private set; } = new()
    {
        new ResPath("/Maps/Shuttles/pirateradio.yml"),
    };

    [DataField]
    public EntityUid? AdditionalRule;

    [DataField]
    public int DebrisCount { get; set; }

    [DataField]
    public float MinimumDistance { get; set; } = 750f;

    [DataField]
    public float MaximumDistance { get; set; } = 1250f;

    [DataField]
    public float MinimumDebrisDistance { get; set; } = 150f;

    [DataField]
    public float MaximumDebrisDistance { get; set; } = 250f;

    [DataField]
    public float DebrisMinimumOffset { get; set; } = 50f;

    [DataField]
    public float DebrisMaximumOffset { get; set; } = 100f;
}
