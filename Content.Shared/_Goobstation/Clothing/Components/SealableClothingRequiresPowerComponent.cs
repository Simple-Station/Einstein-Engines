using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Clothing.Components;

[RegisterComponent]
[NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SealableClothingRequiresPowerComponent : Component
{
    [DataField]
    public LocId NotPoweredPopup = "sealable-clothing-not-powered";

    [DataField]
    public LocId OpenSealedPanelFailPopup = "sealable-clothing-open-sealed-panel-fail";

    [DataField]
    public LocId ClosePanelFirstPopup = "sealable-clothing-close-panel-first";

    /// Movement speed when without power
    [DataField]
    public float MovementSpeedPenalty = 0.3f;

    [DataField, AutoNetworkedField]
    public bool IsPowered = false;

    /// Alert to show for the suit's power
    [DataField]
    public ProtoId<AlertPrototype> SuitPowerAlert = "ModsuitPower";
}
