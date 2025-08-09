namespace Content.Server._Crescent.UnionfallCapturePoint;

[RegisterComponent]
public sealed partial class UnionfallAnnouncerComponent : Component
{
    /// <summary>
    /// Time before capturing is allowed. This is from roundstart, so you want to add the ship grace period ON TOP of the value you want.
    /// </summary>
    [DataField]
    public float GracePeriod = 1200f; //20 minutes for unionfall

}
