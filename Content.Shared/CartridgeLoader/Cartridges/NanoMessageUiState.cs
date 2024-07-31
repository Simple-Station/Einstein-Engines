using Content.Shared.NanoMessage.Data;
using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed partial class NanoMessageUiState : BoundUserInterfaceState
{
    public string? ConnectedServerLabel;
    public List<NanoMessageRecipient> KnownRecipients = new();
    public NanoMessageConversation? OpenedConversation = new();
}
