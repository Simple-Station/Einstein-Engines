using Robust.Shared.Serialization;

namespace Content.Shared.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed partial class NanoMessageUiState : BoundUserInterfaceState
{
    public List<NanoMessageRecipient> KnownRecipients = new();
    public NanoMessageConversation? OpenedConversation = new();
}

[DataDefinition, Serializable]
public partial struct NanoMessageRecipient
{
    [DataField]
    public ulong Id;

    [DataField]
    public string? Name;
}

[DataDefinition, Serializable]
public partial struct NanoMessageConversation
{
    [DataField]
    public ulong User1;

    [DataField]
    public ulong User2;

    [DataField]
    public List<NanoMessageMessage> Messages;
}

[DataDefinition, Serializable]
public partial struct NanoMessageMessage
{
    [DataField]
    public ulong Sender;

    [DataField]
    public string Message;

    [DataField]
    public TimeSpan Timestamp;
}
