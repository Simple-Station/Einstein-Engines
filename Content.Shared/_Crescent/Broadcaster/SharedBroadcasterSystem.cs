using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.Broadcaster;

/// <summary>
/// This handles...
/// </summary>
public class SharedBroadcasterSystem : EntitySystem
{
}

[Serializable, NetSerializable]
public sealed class BroadcasterConsoleState : BoundUserInterfaceState
{
    public Dictionary<int, string> playableBroadcasts;

    public int currentlyPlaying = -1;

    public BroadcasterConsoleState(Dictionary<int, string> broadcasts, int currentlyPlaying)
    {
        playableBroadcasts = broadcasts;
        this.currentlyPlaying = currentlyPlaying;
    }
}

[Serializable, NetSerializable]
public sealed class BroadcasterBroadcastMessage : BoundUserInterfaceMessage
{
    public int indexForBroadcast = -1;

    public BroadcasterBroadcastMessage(int indexForBroadcast)
    {
        this.indexForBroadcast = indexForBroadcast;
    }
}


[NetSerializable, Serializable]
public enum BroadcasterUIKey
{
    Key,
}
