namespace Content.Server.NanoMessage;

[RegisterComponent]
public sealed partial class NanoMessageClientComponent : Component
{
    /// <summary>
    ///     The unique ID of this client. Must be positive. If set to 0, will be overridden on startup.
    /// </summary>
    [DataField]
    public ulong Id;

    [DataField]
    public string? PreferredName;

    [DataField]
    public EntityUid ConnectedServer = EntityUid.Invalid;

    [DataField]
    public TimeSpan ReconnectInterval = TimeSpan.FromSeconds(2);

    [DataField]
    public TimeSpan NextReconnectAttempt = TimeSpan.Zero;
}
