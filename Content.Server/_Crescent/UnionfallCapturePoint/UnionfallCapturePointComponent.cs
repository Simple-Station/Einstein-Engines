namespace Content.Server._Crescent.UnionfallCapturePoint;

[RegisterComponent]
public sealed partial class UnionfallCapturePointComponent : Component
{
    [DataField]
    public string? CapturingFaction = null; // either null, "DSM" or "NCWL"

    /// <summary>
    /// Time needed to end the round once capturing begins. Represents the limit, not the current progress. Measured in seconds.
    /// </summary>
    public float TimeToEnd = 600f;

    /// <summary>
    /// Time it takes to interact with a powered-on capture point to flip it to your side. Measured in seconds.
    /// </summary>
    public float TimeToCapture = 10f;

    /// <summary>
    /// Additional time added whenever the capture point switches sides. Cannot add more time than the TimeToEnd maximum. Measured in seconds.
    /// </summary>
    public float CaptureTimeBonus = 60f;

    /// <summary>
    /// The CURRENT amount of time until the CapturingFaction wins the game. Measured in seconds.
    /// </summary>
    [DataField]
    public float CurrentCaptureProgress = 600f;

    /// <summary>
    /// Time before capturing is allowed. This is from roundstart, so you want to add the ship grace period ON TOP of the value you want.
    /// </summary>
    [DataField]
    public float GracePeriod = 1200f; //20 minutes. 10 ship + 10 point


}
