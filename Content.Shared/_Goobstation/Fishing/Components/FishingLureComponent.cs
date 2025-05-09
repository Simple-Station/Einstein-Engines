using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Fishing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FishingLureComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid FishingRod;

    [DataField, AutoNetworkedField]
    public EntityUid? AttachedEntity;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan NextUpdate;

    [DataField]
    public float UpdateInterval = 1f;
}
