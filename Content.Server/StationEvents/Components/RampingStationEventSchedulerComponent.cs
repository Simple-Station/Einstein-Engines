namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(RampingStationEventSchedulerSystem))]
public sealed partial class RampingStationEventSchedulerComponent : Component
{
    /// <summary>
    ///     Multiplies the End Time of the Ramping Event curve. Lower this number for shorter, hectic shifts, increase this number for longer shifts.
    /// </summary>
    [DataField]
    public float ShiftChaosModifier = 1f;

    /// <summary>
    ///     The number by which all event delays will be multiplied. Unlike chaos, remains constant throughout the shift.
    /// </summary>
    [DataField]
    public float EventDelayModifier = 1f;


    /// <summary>
    ///     Shift Length(in Minutes) is directly reduced by this value.
    /// </summary>
    [DataField]
    public float ShiftLengthOffset = 0f;

    /// <summary>
    ///     Minimum time between events is decreased by this value.
    /// </summary>
    [DataField]
    public float MinimumEventTimeOffset = 0f;

    /// <summary>
    ///     Maximum time between events is decreased by this value.
    /// </summary>

    [DataField]
    public float MaximumEventTimeOffset = 0f;

    [DataField]
    public bool IgnoreMinimumTimes = false;

    // Everything below is overridden in the RampingStationEventSchedulerSystem based on CVars
    [DataField]
    public float EndTime;

    [DataField]
    public float MaxChaos;

    [DataField]
    public float StartingChaos;

    [DataField]
    public float TimeUntilNextEvent;
}
