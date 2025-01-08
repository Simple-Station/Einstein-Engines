using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Overlays.Switchable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightVisionComponent : SwitchableOverlayComponent
{
    [DataField, AutoNetworkedField]
    public override string? ToggleAction { get; set; } = "ToggleNightVision";

    [DataField, AutoNetworkedField]
    public override Color Color { get; set; } = Color.FromHex("#98FB98");
}

public sealed partial class ToggleNightVisionEvent : InstantActionEvent;
