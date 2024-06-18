using Robust.Shared.GameStates;

namespace Content.Shared.OfferingItem;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedOfferingItemSystem))]
public sealed partial class OfferingItemComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool IsInOfferMode;

    [DataField, AutoNetworkedField]
    public bool IsInReceiveMode;

    [DataField, AutoNetworkedField]
    public string? Hand;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    [DataField]
    public float MaxOfferDistance = 2f;
}
