namespace Content.Shared.NanoMessage.Data;

[DataDefinition, Serializable]
public partial struct NanoMessageMessage
{
    [DataField]
    public ulong Sender;

    [DataField]
    public string Content;

    [DataField]
    public TimeSpan Timestamp;
}
