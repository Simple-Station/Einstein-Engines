namespace Content.Server._Crescent.HeatSeeking;


/// <summary>
/// Marker entity for marking objects that are target locks of <see cref="HeatSeekingComponent"/> missiles.
/// </summary>
[RegisterComponent]
public sealed partial class ExpendableHeatTrackedComponent : Component
{
    //[DataField]
    //public float FadeOutTime = 5f;

    [DataField]
    public float ToggleDelay = 30f;

    //public float StartingHeatSignature;

    //public float FadeOutTicker;
}
