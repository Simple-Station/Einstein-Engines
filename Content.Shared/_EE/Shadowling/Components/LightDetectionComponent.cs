namespace Content.Shared._EE.Shadowling.Components;


/// <summary>
/// This is used for detecting if an entity is near a lighted area
/// </summary>
[RegisterComponent]
public sealed partial class LightDetectionComponent : Component
{
    /// <summary>
    ///  Is user standing on a lighted area?
    /// </summary>
    [DataField]
    public bool IsOnLight;
}
