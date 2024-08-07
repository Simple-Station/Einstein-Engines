using Content.Shared.NanoMessage.Data;

namespace Content.Server.NanoMessage.Events;

/// <summary>
///     Raised directly on the server before it dispatches a message.
///     If this event is cancelled, the message will not be dispatched.
/// </summary>
[ByRefEvent]
public sealed class NanoMessageMessageDispatchAttemptEvent(
    NanoMessageConversation context,
    NanoMessageMessage message
) : CancellableEntityEventArgs
{
    public NanoMessageConversation Conversation => context;
    public NanoMessageMessage Message => message;
}
