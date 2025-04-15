using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.Broadcaster;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class BroadcasterComponent : Component
{
    [DataField("range")]
    public float Range = 15f;

    /// <summary>
    /// Can only receive broadcasts from a console with the same outpost set.
    /// </summary>
    [DataField("outpost")]
    public string? Outpost;
}
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]

public sealed partial class BroadcastingConsoleComponent : Component
{
    [DataField("outpost")]
    public string? Outpost;
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public int currentlyPlaying = -1;
    [AutoNetworkedField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<int, string>? AvailableAnnouncements;
}




