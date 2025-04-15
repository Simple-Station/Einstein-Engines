namespace Content.Shared._Crescent.DynamicCodes;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GridDynamicCodeOnSpawnGiverComponent : Component
{
    [DataField]
    public HashSet<string> DynamicCodesOnWakeUp = new();
}
