using Robust.Shared.Network;


namespace Content.Shared.Chat;

public interface ISharedChatManager
{
    void Initialize();
    void SendAdminAlert(string message);
    void SendAdminAlert(EntityUid player, string message);

    void ChatMessageToAll(
        ChatChannel channel,
        string message,
        string wrappedMessage,
        EntityUid source,
        bool hideChat,
        bool recordReplay,
        Color? colorOverride = null,
        string? audioPath = null,
        float audioVolume = 0,
        NetUserId? author = null,
        bool ignoreChatStack = false
    );
}
