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

    // Everything below is overridden in the RampingStationEventSchedulerSystem based on CVars
    [DataField("endTime"), ViewVariables(VVAccess.ReadWrite)]
    public float EndTime;

    [DataField("maxChaos"), ViewVariables(VVAccess.ReadWrite)]
    public float MaxChaos;

    [DataField("startingChaos"), ViewVariables(VVAccess.ReadWrite)]
    public float StartingChaos;

    [DataField("timeUntilNextEvent"), ViewVariables(VVAccess.ReadWrite)]
    public float TimeUntilNextEvent;
}
