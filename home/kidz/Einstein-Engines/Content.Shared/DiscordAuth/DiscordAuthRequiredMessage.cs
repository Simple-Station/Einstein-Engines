using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.DiscordAuth;

/// <summary>
///     Server sends this event to client on connect if Discord auth is required
/// </summary>
public sealed class DiscordAuthRequiredMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public string AuthUrl = string.Empty;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        AuthUrl = buffer.ReadString();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.Write(AuthUrl);
    }
}
