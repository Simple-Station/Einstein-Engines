namespace Content.Server._Crescent.HeatSeeking;


/// <summary>
/// Marker entity for marking objects that are target locks of <see cref="HeatSeekingComponent"/> missiles.
/// </summary>
[RegisterComponent]
public sealed partial class CanBeHeatTrackedComponent : Component
{
    [DataField]
    public float HeatSignature = 1f;
}
