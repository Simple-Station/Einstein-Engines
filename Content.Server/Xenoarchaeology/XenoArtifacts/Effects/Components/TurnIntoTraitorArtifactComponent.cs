namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that turns person into traitor.
/// </summary>
[RegisterComponent]
public sealed partial class TurnIntoTraitorArtifactComponent : Component
{
    [DataField]
    public string? Rule;
}
