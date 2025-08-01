namespace Content.Server._Crescent.HullrotFaction;

/// <summary>
/// Stores "rank" of the user for certain contexts.
/// Mostly given by role.
/// </summary>

[RegisterComponent]
public sealed partial class HullrotFactionComponent : Component
{
    [DataField]
    public string Faction = "";
}
