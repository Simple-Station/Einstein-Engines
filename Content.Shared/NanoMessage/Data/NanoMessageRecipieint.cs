namespace Content.Shared.NanoMessage.Data;

[DataDefinition, Serializable]
public partial struct NanoMessageRecipient
{
    [DataField]
    public ulong Id;

    [DataField]
    public string? Name;

    /// <summary>
    ///     True if the NM server provides a name override for this recipient.
    ///     If this is true, then the preferred name set by the client itself will be ignored.
    /// </summary>
    [DataField]
    public bool CustomNameOverridden = false;
}
