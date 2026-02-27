using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Orehum.ME4TA.Vergil;

[RegisterComponent, NetworkedComponent, Access(typeof(VergilAbilitiesSystem))]
public sealed partial class VergilAbilitiesComponent : Component
{
    [DataField]
    public List<EntProtoId> Actions = new()
    {
        "ActionEnergyKatanaDash",
        "ActionVergilJudgementCut"
    };

    [DataField] public EntityUid? DashActionEntity;
    [DataField] public EntityUid? JCEntity;

    [DataField]
    public List<EntityUid> ActionEntities = new();
}

public sealed partial class VergilJudgementCutEvent : WorldTargetActionEvent { }

public sealed partial class VergilDashEvent : WorldTargetActionEvent { }
