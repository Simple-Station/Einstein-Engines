using Content.Shared.Actions;
using Content.Shared.Antag;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.StatusIcon;
using Content.Shared.Store;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Backmen.Vampiric;

[RegisterComponent]
[NetworkedComponent]
public sealed partial class BkmVampireComponent : Component, IAntagStatusIconComponent
{
    [DataField("currencyPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<CurrencyPrototype>))]
    public string CurrencyPrototype = "BloodEssence";

    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "VampireFaction";
    public bool IconVisibleToGhost { get; set; } = true;


    [ViewVariables(VVAccess.ReadWrite)]
    public int SprintLevel = 0;

    public EntityUid? ActionNewVamp;
    public ProtoId<EntityPrototype> NewVamp = "ActionConvertToVampier";

    public Dictionary<string, FixedPoint2> DNA = new();
}

public sealed partial class InnateNewVampierActionEvent : EntityTargetActionEvent
{
}

[Serializable, NetSerializable]
public sealed partial class InnateNewVampierDoAfterEvent : SimpleDoAfterEvent
{
}
