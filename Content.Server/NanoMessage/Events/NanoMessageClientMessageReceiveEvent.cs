using Content.Shared.NanoMessage.Data;

namespace Content.Server.NanoMessage.Events;

/// <summary>
///     Raised directly on a client to receive a message belonging to a conversation this client is a part of.
/// </summary>
public sealed class NanoMessageClientMessageReceiveEvent(
    NanoMessageConversation conversation,
    NanoMessageMessage message
) : EntityEventArgs
{
    public NanoMessageConversation Conversation => conversation;
    public NanoMessageMessage Message => message;
}
