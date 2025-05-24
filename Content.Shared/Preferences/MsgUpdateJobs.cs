using System.IO;
using Lidgren.Network;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Preferences
{
    /// <summary>
    /// The client sends this to update a player's job preferences.
    /// </summary>
    public sealed class MsgUpdateJobs : NetMessage
    {
        public override MsgGroups MsgGroup => MsgGroups.Command;

        public JobPreferences JobPreferences = default!;

        public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
        {
            var length = buffer.ReadVariableInt32();
            using var stream = new MemoryStream(length);
            buffer.ReadAlignedMemory(stream, length);
            JobPreferences = serializer.Deserialize<JobPreferences>(stream);
        }

        public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
        {
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, JobPreferences);
                buffer.WriteVariableInt32((int) stream.Length);
                stream.TryGetBuffer(out var segment);
                buffer.Write(segment);
            }
        }
    }
}
