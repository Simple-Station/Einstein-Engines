using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PlasmaTransferComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public EntProtoId Action = "ActionAlienTransferPlasma";

    [DataField("actionEntity")]
    public EntityUid? ActionEntity;

    [DataField]
    public float Amount = 50f;
}

public sealed partial class TransferPlasmaActionEvent : EntityTargetActionEvent { }


