namespace Content.Server.Forensics;

/// <summary>
/// This component is for mobs that have a Scent.
/// </summary>
[RegisterComponent]
public sealed partial class ScentComponent : Component
{
    [DataField]
    public string Scent = String.Empty;
}
