using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(RampingStationEventSchedulerSystem))]
public sealed partial class RampingStationEventSchedulerComponent : Component
{
    /// <summary>
    ///     The maximum number by which the event rate will be multiplied when shift time reaches the end time.
    /// </summary>
    [DataField]
    public float ChaosModifier = 3f;

    /// <summary>
    ///     The minimum number by which the event rate will be multiplied when the shift has just begun.
    /// </summary>
    [DataField]
    public float StartingChaosRatio = 0.1f;

    /// <summary>
    ///     The number by which all event delays will be multiplied. Unlike chaos, remains constant throughout the shift.
    /// </summary>
    [DataField]
    public float EventDelayModifier = 1f;

    /// <summary>
    ///     The number by which average expected shift length is multiplied. Higher values lead to slower chaos growth.
    /// </summary>
    [DataField]
    public float ShiftLengthModifier = 1f;

    [DataField]
    public float MinimumTimeUntilNextEvent = 240f;

    [DataField]
    public float MaximumTimeUntilNextEvent = 720f;

    /// <summary>
    ///     Average ending chaos modifier for the ramping event scheduler. Higher means faster.
    ///     Max chaos chosen for a round will deviate from this
    /// </summary>
    [DataField]
    public float AverageChaos = 6f;

    /// <summary>
    ///     Average time (in minutes) for when the ramping event scheduler should stop increasing the chaos modifier.
    ///     Close to how long you expect a round to last, so you'll probably have to tweak this on downstreams.
    /// </summary>
    [DataField]
    public float AverageEndTime = 40f;

    [DataField]
    public float EndTime;

    [DataField]
    public float MaxChaos;

    [DataField]
    public float StartingChaos;

    [DataField]
    public float TimeUntilNextEvent;

    /// <summary>
    /// The gamerules that the scheduler can choose from
    /// </summary>
    /// Reminder that though we could do all selection via the EntityTableSelector, we also need to consider various <see cref="StationEventComponent"/> restrictions.
    /// As such, we want to pass a list of acceptable game rules, which are then parsed for restrictions by the <see cref="EventManagerSystem"/>.
    [DataField(required: true)]
    public EntityTableSelector ScheduledGameRules = default!;
}
