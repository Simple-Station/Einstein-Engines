namespace Content.Server._Crescent.Respawn.Components;

[RegisterComponent]
[Access(typeof(RespawnTrackerSystem))]
public sealed partial class RespawnTickerComponent : Component
{
    /// <summary>
    /// Matches username to death time and respawn time.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly)]
    public Dictionary<Guid, TimeSpan> RespawnTrackers = new();
}
