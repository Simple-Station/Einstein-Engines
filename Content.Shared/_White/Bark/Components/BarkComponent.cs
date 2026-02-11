using Robust.Shared.GameStates;

namespace Content.Shared._White.Bark.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BarkComponent : Component
{
    [DataField, AutoNetworkedField]
    public BarkVoiceData VoiceData { get; set; } = default!;
}
