namespace Content.Server.NanoMessage.Events;

/// <summary>
///     Raised both on the client and the server to determine if they can connect.
/// </summary>
[ByRefEvent]
public sealed class NanoMessageConnectAttemptEvent : CancellableEntityEventArgs
{
    public Entity<NanoMessageServerComponent> Server;
    public Entity<NanoMessageClientComponent> Client;
}
