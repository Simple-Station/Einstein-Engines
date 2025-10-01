using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.HullrotFaction;

/// <summary>
/// Stores "rank" of the user for certain contexts.
/// Mostly given by role.
/// </summary>

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class HullrotFactionComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Faction = "";
}
