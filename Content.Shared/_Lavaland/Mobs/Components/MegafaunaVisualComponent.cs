using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Mobs.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MegafaunaVisualComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? SpriteState;
}
