using Robust.Shared.Serialization;

namespace Content.Shared.NPC.Events;

/// <summary>
/// Raised from client to server to notify a faction was added to an NPC.
/// </summary>
[Serializable, NetSerializable]
public sealed class NpcFactionAddedEvent : EntityEventArgs
{
    public string FactionID;

    public NpcFactionAddedEvent(string factionId) => FactionID = factionId;
}

/// <summary>
/// Raised from client to server to notify a faction was removed from an NPC.
/// </summary>
[Serializable, NetSerializable]
public sealed class NpcFactionRemovedEvent : EntityEventArgs
{
    public string FactionID;

    public NpcFactionRemovedEvent(string factionId) => FactionID = factionId;
}
