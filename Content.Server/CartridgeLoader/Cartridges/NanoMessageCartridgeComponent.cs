using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Server.CartridgeLoader.Cartridges;

[RegisterComponent]
public sealed partial class NanoMessageCartridgeComponent : Component
{
    [DataField]
    public List<ulong> KnownRecipients = new();

    [DataField]
    public List<NanoMessageRecipient> KnownRecipientsData = new();

    [DataField]
    public ulong? CurrentRecipient = null;
}
