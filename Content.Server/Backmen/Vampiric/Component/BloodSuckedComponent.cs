namespace Content.Server.Backmen.Vampiric;

/// <summary>
/// For entities who have been succed.
/// </summary>
[RegisterComponent]
public sealed partial class BloodSuckedComponent : Component
{
    [ViewVariables]
    public EntityUid? BloodSuckerMindId;
}
