namespace Content.Shared.Crescent.Radar;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class RadarDetectorComponent : Component
{
    /// <summary>
    ///  if the sonar ping system should alert this console of any readings
    /// </summary>
    [DataField]
    public bool alertOnPing = true;

    public TimeSpan lastAlert = TimeSpan.Zero;
}
