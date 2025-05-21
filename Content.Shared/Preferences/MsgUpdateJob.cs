using System.IO;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Preferences
{
    /// <summary>
    /// The client sends this to update a player's job preferences.
    /// </summary>
    public sealed class MsgUpdateJob : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public string Job = default!;
        public int CharSlot;
        public JobPriority Priority;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            Job = buffer.ReadString();
            CharSlot = buffer.ReadInt32();
            var length = buffer.ReadVariableInt32();
            using var stream = new MemoryStream(length);
            buffer.ReadAlignedMemory(stream, length);
            Priority = serializer.Deserialize<JobPriority>(stream);
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            buffer.Write(Job);
            buffer.WriteVariableInt32(CharSlot);
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, Priority);
                buffer.WriteVariableInt32((int) stream.Length);
                stream.TryGetBuffer(out var segment);
                buffer.Write(segment);
            }
        }
    }
}
