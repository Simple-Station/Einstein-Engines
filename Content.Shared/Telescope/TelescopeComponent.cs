using Robust.Shared.GameStates;

namespace Content.Shared.Telescope;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TelescopeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Divisor = 0.1f;

    [DataField, AutoNetworkedField]
    public float LerpAmount = 0.1f;

    [ViewVariables]
    public EntityUid? LastEntity;
}
