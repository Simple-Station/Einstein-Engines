using Content.Shared.Antag;
using Content.Shared.FixedPoint;
using Content.Shared.Mind;
using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.WhiteDream.BloodCult.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodCultistComponent : Component, IAntagStatusIconComponent
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float HolyConvertTime = 15f;

    [AutoNetworkedField]
    public List<NetEntity?> SelectedEmpowers = new();

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 RitesBloodAmount = FixedPoint2.Zero;

    public Color OriginalEyeColor = Color.White;

    // TODO: Do we really need all of it hardcoded here?
    public static string SummonCultDaggerAction = "InstantActionSummonCultDagger";

    public static string BloodRitesAction = "InstantActionBloodRites";

    public static string EmpPulseAction = "InstantActionEmpPulse";

    public static string ConcealPresenceAction = "InstantActionConcealPresence";

    public static string CultTwistedConstructionAction = "ActionCultTwistedConstruction";

    public static string CultTeleportAction = "ActionCultTeleport";

    public static string CultSummonCombatEquipmentAction = "ActionCultSummonCombatEquipment";

    public static string CultStunAction = "InstantActionCultStun";

    public static string CultShadowShacklesAction = "ActionCultShadowShackles";

    public static List<string> CultistActions = new()
    {
        SummonCultDaggerAction, BloodRitesAction, CultTwistedConstructionAction, CultTeleportAction,
        CultSummonCombatEquipmentAction, CultStunAction, EmpPulseAction, ConcealPresenceAction, CultShadowShacklesAction
    };

    // [ViewVariables, NonSerialized]
    // public Entity<BloodSpearComponent>? BloodSpear;

    [ViewVariables, NonSerialized]
    public EntityUid? BloodSpearActionEntity;

    [ViewVariables, NonSerialized]
    public Entity<MindComponent>? OriginalMind;

    [DataField]
    public ProtoId<StatusIconPrototype> StatusIcon { get; set; } = "BloodCultMember";

    [DataField]
    public bool IconVisibleToGhost { get; set; } = true;
}
