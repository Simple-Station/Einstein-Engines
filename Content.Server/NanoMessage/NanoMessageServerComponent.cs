using Content.Shared.CartridgeLoader.Cartridges;

namespace Content.Server.NanoMessage;

[RegisterComponent]
public sealed partial class NanoMessageServerComponent : Component
{
    [DataField]
    public bool Enabled = true;

    [DataField]
    public int Priority;

    [DataField]
    public List<(EntityUid Ent, ulong Id)> ConnectedClients = new();

    [DataField]
    public Dictionary<ulong, NanoMessageRecipient> ClientData = new();

    [DataField]
    public List<NanoMessageConversation> Conversations = new();

}
