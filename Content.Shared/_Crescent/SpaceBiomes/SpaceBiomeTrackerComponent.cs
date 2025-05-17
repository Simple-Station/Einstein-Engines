using Robust.Shared.GameStates;

namespace Content.Shared._Crescent.SpaceBiomes;

//attached to the player
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SpaceBiomeTrackerComponent : Component
{
    //for ambience rule
    [AutoNetworkedField]
    [ViewVariables]
    public string Biome;

    //server only
    [ViewVariables]
    public SpaceBiomeSourceComponent? Source;

    [ViewVariables]
    public List<EntityUid> BoringStations = new List<EntityUid>();
}
