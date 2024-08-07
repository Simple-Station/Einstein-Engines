using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.NanoMessage.Data;

namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class NanoMessageCartridgeComponent : Component
{
    /// <summary>
    ///     A cartridge does not know anyone by default. This list indicates what clients this cartridge can message.
    /// </summary>
    [DataField]
    public List<ulong> KnownRecipients = new();

    /// <summary>
    ///     Cached user-level data for clients listed in <see cref="KnownRecipients"/>.
    ///     Only clients available on the current server are listed here.
    /// </summary>
    [DataField]
    public List<NanoMessageRecipient> KnownRecipientsData = new();

    [DataField]
    public ulong? CurrentConversationId;
}
