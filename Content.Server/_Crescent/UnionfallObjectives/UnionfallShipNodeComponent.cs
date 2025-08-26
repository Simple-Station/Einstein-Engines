namespace Content.Server._Crescent.UnionfallCapturePoint;

[RegisterComponent]
public sealed partial class UnionfallShipNodeComponent : Component
{
    [DataField]
    public string? OwningFaction = null; // either null, "DSM" or "NCWL"

    public bool IsBeingCaptured = false; //when its true tick down the timer

    /// <summary>
    /// Time needed to explode the round once capturing begins. Represents the limit, not the current progress. Measured in seconds.
    /// </summary>
    [DataField]
    public float TimeToCapture = 60f;

    /// <summary>
    /// Time it takes to interact with a powered-on capture point to flip it to your side. Measured in seconds.
    /// </summary>
    [DataField]
    public float DoAfterDelay = 10f;

    /// <summary>
    /// The CURRENT amount of time until a faction that ISN'T OwningFaction succeeds and the point blows up.
    /// </summary>
    [DataField]
    public float CurrentCaptureProgress = 60f;

    /// <summary>
    /// Time before capturing is allowed. This is from roundstart, so you want to add the ship grace period ON TOP of the value you want.
    /// </summary>
    [DataField]
    public float GracePeriod = 1200f; //20 minutes for unionfall
}
