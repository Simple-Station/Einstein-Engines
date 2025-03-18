using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingInfectionImplantComponent : Component
{
    [DataField]
    public ProtoId<TagPrototype> ChangelingInfectionImplant = "ChangelingInfectionImplant";

    [DataField]
    public string ImplantFailPopup = "changeling-convert-implant-fail";

    [DataField]
    public PopupType ImplantFailPopupType = PopupType.MediumCaution;

    [DataField]
    public string ImplantPopup = "changeling-convert-implant";

    [DataField]
    public PopupType ImplantPopupType = PopupType.LargeCaution;
}
