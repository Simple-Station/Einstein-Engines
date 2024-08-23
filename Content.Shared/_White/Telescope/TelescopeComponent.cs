using Robust.Shared.GameStates;

namespace Content.Shared._White.Telescope;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TelescopeComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float Divisor = 0.1f;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float LerpAmount = 0.1f;

    [ViewVariables]
    public EntityUid? LastEntity;
}
