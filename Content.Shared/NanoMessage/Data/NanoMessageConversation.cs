namespace Content.Shared.NanoMessage.Data;

[DataDefinition, Serializable]
public partial struct NanoMessageConversation
{
    [DataField]
    public ulong Id;

    [DataField]
    public ulong User1;

    [DataField]
    public ulong User2;

    [DataField]
    public List<NanoMessageMessage> Messages;
}
