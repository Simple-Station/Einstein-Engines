namespace Content.Shared.NanoMessage.Data;

[DataDefinition, Serializable]
public partial struct NanoMessageRecipient
{
    [DataField]
    public ulong Id;

    [DataField]
    public string? Name;
}
