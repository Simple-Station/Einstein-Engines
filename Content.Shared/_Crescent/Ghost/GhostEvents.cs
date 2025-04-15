using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Ghost;

/// <summary>
/// A client to server request to get the time we can respawn.
/// Response is sent via <see cref="RespawnTimeResponseEvent"/>
/// </summary>
[Serializable, NetSerializable]
public sealed class RespawnTimeRequestEvent : EntityEventArgs
{
}

/// <summary>
/// A server to client response for a <see cref="RespawnTimeRequestEvent"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class RespawnTimeResponseEvent : EntityEventArgs
{
    public RespawnTimeResponseEvent(TimeSpan respawnTime)
    {
        RespawnTime = respawnTime;
    }

    /// <summary>
    /// Time you may respawn xir.
    /// </summary>
    public readonly TimeSpan RespawnTime;
}

