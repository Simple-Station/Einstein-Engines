using Robust.Shared.Serialization;

namespace Content.Shared.DeltaV.CartridgeLoader.Cartridges;

[Serializable, NetSerializable]
public sealed class NanoChatUiState : BoundUserInterfaceState
{
    public readonly Dictionary<uint, NanoChatRecipient> Recipients;
    public readonly Dictionary<uint, List<NanoChatMessage>> Messages;
    public readonly uint? CurrentChat;
    public readonly uint OwnNumber;
    public readonly int MaxRecipients;
    public readonly bool NotificationsMuted;

    public NanoChatUiState(
        Dictionary<uint, NanoChatRecipient> recipients,
        Dictionary<uint, List<NanoChatMessage>> messages,
        uint? currentChat,
        uint ownNumber,
        int maxRecipients,
        bool notificationsMuted)
    {
        Recipients = recipients;
        Messages = messages;
        CurrentChat = currentChat;
        OwnNumber = ownNumber;
        MaxRecipients = maxRecipients;
        NotificationsMuted = notificationsMuted;
    }
}
