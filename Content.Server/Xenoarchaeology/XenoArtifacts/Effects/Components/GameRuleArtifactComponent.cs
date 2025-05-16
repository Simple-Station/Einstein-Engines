namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;

/// <summary>
/// Artifact that adds a game rule/
/// does a station event when activated.
/// </summary>
[RegisterComponent]
public sealed partial class GameRuleArtifactComponent : Component
{
    [DataField]
    public List<string> Rules = new();
}

