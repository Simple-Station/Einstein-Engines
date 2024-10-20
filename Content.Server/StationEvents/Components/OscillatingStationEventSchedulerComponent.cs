namespace Content.Server.StationEvents.Components;

/// <summary>
///     A station event scheduler that emits events at irregular intervals, with occasional chaos and occasional calmness.
/// </summary>
[RegisterComponent]
public sealed partial class OscillatingStationEventSchedulerComponent : Component
{
    // TODO cvars?
    [DataField]
    public float MinChaos = 0.1f, MaxChaos = 15f;

    /// <summary>
    ///     The amount of chaos at the beginning of the round.
    /// </summary>
    [DataField]
    public float StartingChaosRatio = 0f;

    /// <summary>
    ///     The value of the first derivative of the event delay function at the beginning of the shift.
    ///     Must be between 1 and -1.
    /// </summary>
    [DataField]
    public float StartingSlope = 1f;

    /// <summary>
    ///     Biases that determine how likely the event rate is to go up or down, and how fast it's going to happen.
    /// </summary>
    /// <remarks>
    ///     Downwards bias must always be negative, and upwards must be positive. Otherwise, you'll get odd behavior or errors.
    /// </remarks>
    [DataField]
    public float DownwardsBias = -1f, UpwardsBias = 1f;

    /// <summary>
    ///     Limits that define how large the chaos slope can become.
    /// </summary>
    /// <remarks>
    ///     Downwards limit must always be negative, and upwards must be positive. Otherwise, you'll get odd behavior or errors.
    /// </remarks>
    [DataField]
    public float DownwardsLimit = -1f, UpwardsLimit = 1f;

    /// <summary>
    ///     A value between 0 and 1 that determines how slowly the chaos and its first derivative change in time.
    /// </summary>
    /// <remarks>
    ///     Changing these values will have a great impact on how fast the event rate changes.
    /// </remarks>
    [DataField]
    public float ChaosStickiness = 0.93f, SlopeStickiness = 0.96f;

    /// <summary>
    ///     Actual chaos data at the current moment. Those are overridden at runtime.
    /// </summary>
    [DataField]
    public float CurrentChaos, CurrentSlope, LastAcceleration;


    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero, LastEventTime = TimeSpan.Zero;

    /// <summary>
    ///     Update interval, which determines how often current chaos is recalculated.
    ///     Modifying this value does not directly impact the event rate, but changes how stable the slope is.
    /// </summary>
    [DataField]
    public TimeSpan UpdateInterval = TimeSpan.FromSeconds(5f);
}
