using Content.Shared.Actions;
using Robust.Shared.GameStates;

namespace Content.Shared.Overlays.Switchable;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ThermalVisionComponent : SwitchableOverlayComponent
{
    [DataField, AutoNetworkedField]
    public override string? ToggleAction { get; set; } = "ToggleThermalVision";

    [DataField, AutoNetworkedField]
    public override Color Color { get; set; } = Color.FromHex("#F84742");
}

public sealed partial class ToggleThermalVisionEvent : InstantActionEvent;
