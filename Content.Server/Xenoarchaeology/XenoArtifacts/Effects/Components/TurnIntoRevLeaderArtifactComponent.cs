namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that turns person into a head rev.
/// </summary>
[RegisterComponent]
public sealed partial class TurnIntoRevleaderArtifactComponent : Component
{
    [DataField]
    public string? Rule;
}
