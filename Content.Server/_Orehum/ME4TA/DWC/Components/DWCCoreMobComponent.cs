namespace Content.Server._Orehum.ME4TA.DWCCore.Components;

/// <summary>
/// A mob created by a tendril. Upon death, it is removed from its spawn list
/// </summary>
[RegisterComponent]
public sealed partial class DWCCoreMobComponent : Component
{
    public EntityUid? DWCCore;
}
