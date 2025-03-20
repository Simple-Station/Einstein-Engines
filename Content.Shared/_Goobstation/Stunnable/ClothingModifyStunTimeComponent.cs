using Robust.Shared.GameStates;

namespace Content.Shared.Stunnable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClothingModifyStunTimeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Modifier = 1f;
}
