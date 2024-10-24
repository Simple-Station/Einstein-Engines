using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.DiscordAuth;

/// <summary>
///     Client sends this event to force server check player Discord verification state
/// </summary>
public sealed class DiscordAuthCheckMessage : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer) { }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer) { }
}
